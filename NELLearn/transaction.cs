using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// 转账
    /// </summary>
    class Transaction
    {
        public static void test(DataMgr mgr)
        {
            string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";
            string wif1 = "KyHE9oSaBoEobQrYKtwEptK2VVVry3qYV27L24qdSawdvHRefxVw";
            string targetAddr = "AH2ADnKSuJrhHefqeJ9j83HcNXPfipwr6V";

            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif1);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            mgr.showState();
            Dictionary<string,List<UtxoAsset>> assets =mgr.getAddressMoney(address);

            if(assets==null)
            {
                Console.WriteLine("钱不够！！！");
                return;
            }
            //拼装交易体
            string[] targetAddrs = new string[1] { targetAddr };
            ThinNeo.Transaction tran = makeTran(assets, targetAddrs, new ThinNeo.Hash256(id_GAS), (decimal)300);
            tran.version = 0;
            tran.type = ThinNeo.TransactionType.ContractTransaction;

            //消息
            byte[] msg = tran.GetMessage();//tran是交易，getmessage是得到未签名交易的二进制数据块
            //string msgstr = ThinNeo.Helper.Bytes2HexString(msg);//??
            
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);//私钥签名消息

            tran.AddWitness(signdata, pubkey, address);//添加普通账户鉴证人，私钥公钥地址，全都是一个人
            //string txid = tran.GetHash().ToString();//??
            byte[] data = tran.GetRawData();//得到签名交易的二进制数据块
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);


            //rawdata = "80000001195876cb34364dc38b730077156c6bc3a7fc570044a66fbfeeea56f71327e8ab0000029b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc500c65eaf440000000f9a23e06f74cf86b8827a9108ec2e0f89ad956c9b7cffdaa674beae0f930ebe6085af9093e5fe56b34a5c220ccdcf6efc336fc50092e14b5e00000030aab52ad93f6ce17ca07fa88fc191828c58cb71014140915467ecd359684b2dc358024ca750609591aa731a0b309c7fb3cab5cd0836ad3992aa0a24da431f43b68883ea5651d548feb6bd3c8e16376e6e426f91f84c58232103322f35c7819267e721335948d385fae5be66e7ba8c748ac15467dcca0693692dac";
            //var result = await mgr.loader.rpc_Transaction(mgr.rpcUrl, rawdata);
            var result = mgr.loader.rpc_Transaction_2(mgr.rpcUrl, rawdata);
            if (result != null)
            {
                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
            }
        }

        //拼交易体
        public static ThinNeo.Transaction makeTran(Dictionary<string, List<UtxoAsset>> assets, string[] targetaddrs, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            if (!assets.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<UtxoAsset> utxos = assets[assetid.ToString()];
            utxos.Sort((a, b) =>
            {
                if (a.count > b.count)
                    return 1;
                else if (a.count < b.count)
                    return -1;
                else
                    return 0;
            });
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            decimal count = decimal.Zero;
            string scraddr = "";
            List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
            for (var i = 0; i < utxos.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = new ThinNeo.Hash256(utxos[i].txid);
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].count;
                scraddr = utxos[i].address;
                if (count >= (sendcount))
                {
                    break;
                }
            }

            tran.inputs = list_inputs.ToArray();

            if (count >= sendcount)//输入大于等于输出
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero &&targetaddrs!=null && targetaddrs.Length > 0)
                {
                    foreach (string targetaddr in targetaddrs)
                    {
                        ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                        output.assetId = assetid;
                        output.value = sendcount;
                        output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetaddr);
                        list_outputs.Add(output);
                    }
                }

                //找零
                var change = count - sendcount;
                if (change > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr);
                    outputchange.value = change;
                    outputchange.assetId = assetid;
                    list_outputs.Add(outputchange);
                }
                tran.outputs = list_outputs.ToArray();
            }
            else
            {
                throw new Exception("no enough money.");
            }
            return tran;
        }


    }
}
