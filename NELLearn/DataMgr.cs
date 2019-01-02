using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public const string rpc_getstorage = "getstorage";
        public const string rpc_invokescript = "invokescript";

        public const string rpc_sendrawtransaction = "sendrawtransaction";
    }

    public class AssetType
    {
        public const string NEO = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        public const string GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
    }

    public class UtxoAsset
    {
        public string address;
        public string txid;
        public string assetType;
        public decimal count;
        public int n;

        public bool besused = false;
        public string usetxId = null;

        public UtxoAsset(string _address, string _txid,string _assetTyp, decimal _count,int _n)
        {
            this.address = _address;
            this.txid = _txid;
            this.assetType = _assetTyp;
            this.count = _count;
            this.n = _n;
        }
    }
    public class DataMgr
    {
        public  readonly DataIO loader;
        public readonly DataParaser paraser;

        public string rpcUrl { get; private set; }

        readonly MyJson.JsonNode_Object dataJson;

        /// <summary>
        /// key1:address      key2:assettype            value:vout[]
        /// </summary>
        readonly Dictionary<string, Dictionary<string, List<UtxoAsset>>> utxoDic = new Dictionary<string, Dictionary<string, List<UtxoAsset>>>();
        /// <summary>
        /// key:texid+ vout index   value:vout
        /// </summary>
        readonly Dictionary<string, UtxoAsset> utxoMap = new Dictionary<string, UtxoAsset>();
        public DataMgr(string rpcurl)
        {
            this.rpcUrl = rpcurl;
            this.dataJson = new MyJson.JsonNode_Object();

            loader = new DataIO(rpcurl);
            paraser = new DataParaser(loader, dataJson);

            paraser.utxoMap = this.utxoMap;
            paraser.utxoDic = this.utxoDic;

            System.Threading.Thread thread = new System.Threading.Thread(async () =>
            {
                await paraser.CheckBlockLoop();
            });
            thread.Start();
        }

        public void showState()
        {
            Console.WriteLine("sync height=" + paraser.processedBlock + "  remote height=" + paraser.remoteBlockHeight);
        }
        /// <summary>
        /// 获得地址的资产
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Dictionary<string, List<UtxoAsset>> getAddressMoney(string address)
        {
            if(this.utxoDic.ContainsKey(address)==false)
            {
                Console.WriteLine("address 无效！！！ addres: "+address);
                return null;
            }
            Dictionary<string, decimal> money = new Dictionary<string, decimal>();
            foreach(var item in this.utxoDic[address])
            {
                decimal allcount = 0;
                foreach(var utx in item.Value)
                {
                    allcount += utx.count;
                }
                money.Add(item.Key,allcount);
            }
            Console.WriteLine("------------------------addres: " + address);
            foreach (var item in money)
            {
                if(item.Key==AssetType.NEO)
                {
                    Console.WriteLine("assetType: NEO"  + "       count: " + item.Value);
                }else if(item.Key==AssetType.GAS)
                {
                    Console.WriteLine("assetType: GAS" + "       count: " + item.Value);
                }else
                {
                    Console.WriteLine("unkown Asset Type. asset:"+item.Key);
                }
            }
            return this.utxoDic[address];
        }

        //public async Task CheckBlockLoop()
        //{
        //    await paraser.CheckBlockLoop();
        //}
    }




}
