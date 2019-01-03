using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    public class DataParaser
    {
        //readonly string rpcUrl;
        DataIO loader;

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
        //public MyJson.JsonNode_Object dataJson;
        /// <summary>
        /// key1:address      key2:assettype            value:vout[]
        /// </summary>
        public Dictionary<string, Dictionary<string, List<UtxoAsset>>> utxoDic;
        /// <summary>
        /// key:texid+ vout index   value:vout
        /// </summary>
        public Dictionary<string, UtxoAsset> utxoMap;

        public DataParaser(DataIO _loder, MyJson.JsonNode_Object _data)
        {
            //this.rpcUrl = rpcurl;
            //loader = new DataLoder(rpcurl);
            this.loader = _loder;
            this.processedBlock = 240000;
            //this.dataJson = _data;
        }

        bool bExit = false;
        public async Task CheckBlockLoop()
        {
            while (!bExit)
            {
                try
                {
                    int blockcount = await loader.rpc_getBlockCount() - 1;
                    if (blockcount >= 0 && this.processedBlock < blockcount)
                    {
                        //Console.WriteLine("block count: " + blockcount);
                        remoteBlockHeight = blockcount;
                        await SyncBlockToHeight(this.processedBlock + 1, remoteBlockHeight);
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
            return;
        }

        /// <summary>
        /// 对区块数据进行处理
        /// </summary>
        /// <param name="block"></param>
        /// <param name="blockjson"></param>
        void SyncBlockData(int block, MyJson.JsonNode_Object blockjson)
        {
            //交易信息
            var txs = blockjson.AsDict()["tx"].AsList();
            foreach (MyJson.JsonNode_Object tx in txs)
            {
                this.SyncBlockTransAction(block, tx);
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
        }

        /// <summary>
        /// 加载并解析(from to )区间的区块数据
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        public async Task SyncBlockToHeight(int fromHeight, int toHeight)
        {
            for (int i = fromHeight; i <= toHeight; i++)
            {
                this.processedBlock = i;
                var blockjson = await loader.rpc_loadBlockData(i);
                if (blockjson == null)
                {

                }
                else
                {
                    //Console.WriteLine("******************同步区块： " + i);
                    this.SyncBlockData(i, blockjson);
                }

            }
        }

        //--------------记录交易
        void recordeTransation(int block, string txid, string type, MyJson.JsonNode_Array vin, MyJson.JsonNode_Array vout)
        {
            foreach (var v in vin)
            {
                this.useMoney(txid, v as MyJson.JsonNode_Object);
            }

            foreach (var vo in vout)
            {
                this.recevieMoney(txid, vo as MyJson.JsonNode_Object);
            }


        }

        //-----------花费
        void useMoney(string curTxid, MyJson.JsonNode_Object vin)
        {
            string fromTxid = vin.GetDictItem("txid").AsString();
            int n = vin.GetDictItem("vout").AsInt();

            var key = fromTxid + n;
            if (!this.utxoMap.ContainsKey(key))
            {
                Console.WriteLine("utxo 不存在！ UTXO txid:" + fromTxid + "    n: " + n);
                return;
            }
            UtxoAsset utxo = this.utxoMap[key];
            utxo.besused = true;
            utxo.usetxId = curTxid;

            this.utxoDic[utxo.address][utxo.assetType].Remove(utxo);
        }

        void recevieMoney(string txid, MyJson.JsonNode_Object vout)
        {
            var n = vout.GetDictItem("n").AsInt();
            var asset = vout.GetDictItem("asset").AsString();
            var address = vout.GetDictItem("address").AsString();
            var value = vout.GetDictItem("value").AsString();
            var count = decimal.Parse(value);

            UtxoAsset newutxo = new UtxoAsset(address, txid, asset, count, n);

            if (address == "AJi1N4tqLvjoYzQBRy8JAfuctvxxbnxoAP")
            {
                Console.WriteLine("-----------------------------钱包收到钱：txid:" + txid + "       count:" + value);
            }
            this.utxoMap[txid + n] = newutxo;
            if (this.utxoDic.ContainsKey(address) == false)
            {
                this.utxoDic.Add(address, new Dictionary<string, List<UtxoAsset>>());
            }
            if (this.utxoDic[address].ContainsKey(asset) == false)
            {
                this.utxoDic[address][asset] = new List<UtxoAsset>();
            }
            this.utxoDic[address][asset].Add(newutxo);
        }



    }
}
