﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// nnc 转账
    /// </summary>
   public class sanlian_3
    {
        public static void test(DataMgr mgr)
        {
            string nnc = "0x3fccdb91c9bb66ef2446010796feb6ca4ed96b05";
            string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";


            string wif1 = "KyHE9oSaBoEobQrYKtwEptK2VVVry3qYV27L24qdSawdvHRefxVw";
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif1);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            string toaddr = "APwCdakS1NpJsiq6j9SfvkQFS9ubt347a2";
            string targeraddr = address;  //Transfer it to yourself. 

            //获取地址的资产列表
            Dictionary<string, List<UtxoAsset>> assets = mgr.getAddressMoney(address);


            ThinNeo.Transaction tran = Transaction.makeTran(assets, new string[1] { targeraddr }, new ThinNeo.Hash256(id_GAS), decimal.Zero);
            tran.type = ThinNeo.TransactionType.InvocationTransaction;

            ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder();

            var scriptaddress = new ThinNeo.Hash160(nnc);
            //Parameter inversion 
            MyJson.JsonNode_Array JAParams = new MyJson.JsonNode_Array();
            JAParams.Add(new MyJson.JsonNode_ValueString("(address)" + address));
            JAParams.Add(new MyJson.JsonNode_ValueString("(address)" + toaddr));
            JAParams.Add(new MyJson.JsonNode_ValueString("(integer)" + 1));
            sb.EmitParamJson(JAParams);//Parameter list 
            sb.EmitPushString("transfer");//Method
            sb.EmitAppCall(scriptaddress);  //Asset contract 

            ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
            extdata.script = sb.ToArray();
            extdata.gas = 1;
            tran.extdata = extdata;

            byte[] msg = tran.GetMessage();
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string scripthash = ThinNeo.Helper.Bytes2HexString(data);

            //string response = await Helper.HttpGet(api + "?method=sendrawtransaction&id=1&params=[\"" + scripthash + "\"]");
            string response = mgr.loader.rpc_InvocationTransaction(mgr.rpcUrl,scripthash);

            
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(response);
            Console.WriteLine(resJO["result"].ToString());
        }
    }
}