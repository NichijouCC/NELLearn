using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// 分币 depoly
    /// </summary>
    class Deploy
    {
       public static void test(DataMgr mgr)
        {
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(pubSmartContract.wif1);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            //获取地址的资产列表
            Dictionary<string, List<UtxoAsset>> assets = mgr.getAddressMoney(address);
            using (ThinNeo.ScriptBuilder newsb = new ThinNeo.ScriptBuilder())
            {
                //倒叙插入数据
                ThinNeo.Hash160 shash = new ThinNeo.Hash160(pubSmartContract.tokenScript);
                newsb.EmitParamJson(new MyJson.JsonNode_Array());
                newsb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)deploy"));
                newsb.EmitAppCall(shash);

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(newsb.ToArray());

                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = newsb.ToArray();
                extdata.gas = Math.Ceiling((decimal)0);


                //拼装交易体
                ThinNeo.Transaction tran = Transaction.makeTran(assets, null, new ThinNeo.Hash256(pubSmartContract.id_GAS), extdata.gas);
                tran.version = 0;//给手续费的就是1  不要的就是0
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                var resss = mgr.loader.rpc_Transaction_2(mgr.rpcUrl, rawdata);

                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(resss);
                Console.WriteLine(resJO.ToString());
            }
        }
    }
}
