using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{


    public class NeoMethod
    {
        public const string rpc_getblockcount = "getblockcount";
        public const string rpc_getblock = "getblock";
        public const string rpc_getnotifyinfo = "getnotifyinfo";
        public const string rpc_getfullloginfo = "getfullloginfo";
    }


    public class DataMgr
    {
        readonly string rpcUrl;
        readonly Server server;

        public int processedBlock
        {
            get;
            private set;
        }
        public int remoteBlockHeight
        {
            get;
            private set;
        }


        public DataMgr(string rpcurl)
        {
            this.rpcUrl = rpcurl;
            server = new Server();
        }

        public string getRpcUrl(string method, params string[] param)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.rpcUrl + "?jsonrpc=2.0&id=1&method=" + method + "&params=[");
            for (int i = 0; i < param.Length; i++)
            {
                if(i!=param.Length-1)
                {
                    sb.Append(param[i] + ",");
                }else
                {
                    sb.Append(param[i]);
                }
            }
            sb.Append("]");
            return sb.ToString();
        }

        bool bExit = false;
        public async Task CheckBlockLoop()
        {
            while (!bExit)
            {
                try
                {
                    int blockcount = await this.getBlockCount();
                    if (blockcount >= 0)
                    {
                        Console.WriteLine("block count: " + blockcount);
                        remoteBlockHeight = blockcount - 1;
                        await SyncBlockToHeight(this.processedBlock, remoteBlockHeight);
                    }
                    else
                    {
                        await Task.Delay(5000);
                        continue;
                    }
                }
                catch
                {
                    await Task.Delay(5000);
                }

            }
            //SaveState();
            //FullExit = true;
            return;
        }
        
        /// <summary>
        /// 取得当前区块高度
        /// </summary>
        /// <returns></returns>
        async Task<int> getBlockCount()
        {
            var gstr = getRpcUrl(NeoMethod.rpc_getblockcount);
            try
            {
                var json = await server.downLoadString(gstr);
                if (json["result"] != null)
                {
                    int height = json["result"].AsInt();
                    return height;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return -1;
            }
        }


        /// <summary>
        /// 加载指定区块的数据
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        async Task<MyJson.JsonNode_Object> loadBlockData(int blockIndex)
        {
            var gstr = getRpcUrl(NeoMethod.rpc_getblock, blockIndex.ToString(),"1");
            try
            {
                var json=await server.downLoadString(gstr);
                var result = json["result"] as MyJson.JsonNode_Object;
                //var txs = json["result"].AsDict()["tx"].AsList();
                return result;
            }
            catch (Exception err)
            {
                Console.WriteLine("failed to load block data.info:"+err.ToString());
                return null;
            }
        }



        /// <summary>
        /// 对区块数据进行处理
        /// </summary>
        /// <param name="block"></param>
        /// <param name="blockjson"></param>
        void SyncBlockData(int block, MyJson.JsonNode_Object blockjson)
        {
            //交易信息
            var txs=blockjson.AsDict()["tx"].AsList();
            foreach(MyJson.JsonNode_Object tx in txs)
            {
                this.SyncBlockTransAction(block,tx);
            }
        }

        /// <summary>
        /// 处理区块 某一次交易的数据
        /// </summary>
        /// <param name="block"></param>
        /// <param name="txjson"></param>
        void SyncBlockTransAction(int block, MyJson.JsonNode_Object txjson)
        {
            var type = txjson["type"].AsString();
            var txid = txjson["txid"].AsString();
            var vin = txjson["vin"] as MyJson.JsonNode_Array;
            var vout = txjson["vout"] as MyJson.JsonNode_Array;

            if (vout.Count > 0 || vin.Count > 0)
            {
                //记录一笔交易
                recordeTransation(block, txid, type, vin, vout);
            }
            ////销毁utxo
            //foreach (var v in vin)
            //{
            //    var usetxid = v.GetDictItem("txid").AsString();
            //    var usev = v.GetDictItem("vout").AsInt();
            //    DestoryUTXO(txid, usetxid, usev);
            //}
            ////制造utxo
            //foreach (var v in vout)
            //{
            //    var n = v.GetDictItem("n").AsInt();
            //    var asset = v.GetDictItem("asset").AsString();
            //    var value = v.GetDictItem("value").AsString();
            //    var address = v.GetDictItem("address").AsString();
            //    MakeUTXO(address, txid, n, asset, value);
            //}
        }

        

        /// <summary>
        /// 加载并解析(from to )区间的区块数据
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        async Task SyncBlockToHeight(int fromHeight,int toHeight)
        {
            for(int i= fromHeight; i< toHeight; i++)
            {
                var blockjson =await this.loadBlockData(i);
                if(blockjson ==null)
                {

                }else
                {
                    this.SyncBlockData(i,blockjson);
                }
                this.processedBlock = i;
            }
        }




        MyJson.JsonNode_Object dataJson = new MyJson.JsonNode_Object();
        //--------------记录交易
        void recordeTransation(int block,string txid,string type, MyJson.JsonNode_Array vin, MyJson.JsonNode_Array vout)
        {
            if(dataJson.ContainsKey("txs")==false)
            {
                dataJson["txs"] = new MyJson.JsonNode_Object();
            }
            var txInfo = dataJson["txs"] as MyJson.JsonNode_Object;
            MyJson.JsonNode_Object trans = new MyJson.JsonNode_Object();
            txInfo[txid] = trans;

            trans.SetDictValue("block",block);
            trans.SetDictValue("type",type);
            trans.SetDictValue("in",vin);
            trans.SetDictValue("vout",vout);
        }

        //销毁UTXO
        void DestoryUTXO(string txid, string usetxid, int n)
        {
            //取得uxto中的地址
            var txmap = dataJson["txs"] as MyJson.JsonNode_Object; //txmap 相当于一个mongodb 仓库
            var vout = txmap[usetxid].AsDict()["vout"].AsList()[n].AsDict();
            var address = vout["address"].AsString();//这个钱给了谁

            var utxomap = dataJson["utxo"] as MyJson.JsonNode_Object;
            var utxoaddr = utxomap[address].AsDict(); //utxomap 相当于一个mongodb 仓库

            var key = usetxid + n.ToString("x04");
            //utxoaddr.Remove(key);//直接删除一笔花费或者将他标记为已经花费
            var money = utxoaddr[key].AsDict();
            money.SetDictValue("use", txid);//标记为已花费
        }

    }


    class Server
    {
        WebClient wc = new WebClient();

        public async Task<MyJson.JsonNode_Object> downLoadString(string url)
        {
            var str=await wc.DownloadStringTaskAsync(url);
            var json=MyJson.Parse(str).AsDict();
            bool beError = json.ContainsKey("error");
            if(beError)
            {
                return null;
            }else
            {
                return json;
            }
        }
    }
}
