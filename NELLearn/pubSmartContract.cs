using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// 发布合约
    /// </summary>
    class pubSmartContract
    {
        public static string tokenScript = "0x7be3ec92a023a531295b54112d859e5304d7cf8b";
        public static string wif1 = "KwUhZzS6wrdsF4DjVKt2XQd3QJoidDhckzHfZJdQ3gzUUJSr8MDd";//地址 AcjVGYytBysSdQTLZXpLarvVVYYNUiiUgG
        public static string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public static void test(DataMgr mgr)
        {
            string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

            string wif = pubSmartContract.wif1;
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            Dictionary<string, List<UtxoAsset>> assets = mgr.getAddressMoney(address);

            //从文件中读取合约脚本
            byte[] script = System.IO.File.ReadAllBytes("..\\..\\..\\NeoContract\\bin\\Debug\\NeoContract.avm"); //这里填你的合约所在地址
            //Console.WriteLine("合约脚本:"+ThinNeo.Helper.Bytes2HexString(script));
            //Console.WriteLine("合约脚本hash："+ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.GetScriptHashFromScript(script).data.ToArray().Reverse().ToArray()));
            byte[] parameter__list = ThinNeo.Helper.HexString2Bytes("0710");  //这里填合约入参  例：0610代表（string，[]）
            byte[] return_type = ThinNeo.Helper.HexString2Bytes("05");  //这里填合约的出参
            int need_storage = 1;
            int need_nep4 = 0;
            int need_canCharge = 4;
            string name = "test";
            string version = "1.0";
            string auther = "tpo";
            string email = "0";
            string description = "0";
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                //倒叙插入数据
                sb.EmitPushString(description);
                sb.EmitPushString(email);
                sb.EmitPushString(auther);
                sb.EmitPushString(version);
                sb.EmitPushString(name);
                sb.EmitPushNumber(need_storage | need_nep4 | need_canCharge);
                sb.EmitPushBytes(return_type);
                sb.EmitPushBytes(parameter__list);
                sb.EmitPushBytes(script);
                sb.EmitSysCall("Neo.Contract.Create");

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(sb.ToArray());
                //用ivokescript试运行并得到消耗

                //byte[] postdata;

                var result = mgr.loader.rpc_invokescript(mgr.rpcUrl, scriptPublish);

                //*******************************这里返回说   state:fault             ********************************************************************

                MyJson.JsonNode_Object res= MyJson.Parse(result) as MyJson.JsonNode_Object;
                Console.WriteLine(res.ToString());
                //var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(scriptPublish));
                //var result = await Helper.HttpPost(url, postdata);

                var consume = ((MyJson.Parse(result) as MyJson.JsonNode_Object)["result"] as MyJson.JsonNode_Object)["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);
                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = sb.ToArray();

                //Console.WriteLine(ThinNeo.Helper.Bytes2HexString(extdata.script));
                extdata.gas = Math.Ceiling(gas_consumed - 10);

                //拼装交易体
                ThinNeo.Transaction tran = Transaction.makeTran(assets, null, new ThinNeo.Hash256(id_GAS), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                //Console.WriteLine("scripthash:"+scripthash);

                //url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                //result = await Helper.HttpPost(url, postdata);
               var resss=mgr.loader.rpc_Transaction_2(mgr.rpcUrl,rawdata);

                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(resss);
                Console.WriteLine(resJO.ToString());
            }
        }
    }
}
