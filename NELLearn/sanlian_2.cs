using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// invokescript
    /// </summary>
    public class Sanlian_2
    {
        public static void Test(DataMgr mgr)
        {

            string api = "https://api.nel.group/api/testnet";
            string scripthash = "0x3fccdb91c9bb66ef2446010796feb6ca4ed96b05";//nnc

            api = mgr.rpcUrl;
            scripthash= "0xa02339bbb7905bea2503717a526608fb042c9f85";//tpp

            string nnc = scripthash.Replace("0x", "");
            string script = null;
            using (var sb = new ThinNeo.ScriptBuilder())
            {

                sb.EmitParamJson(new MyJson.JsonNode_Array());//参数倒序入，没参数所以直接塞array
                sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)name"));//调用的方法
                ThinNeo.Hash160 shash = new ThinNeo.Hash160(nnc);
                sb.EmitAppCall(shash);//nep5脚本

                sb.EmitParamJson(new MyJson.JsonNode_Array());
                sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)symbol"));
                sb.EmitAppCall(shash);

                sb.EmitParamJson(new MyJson.JsonNode_Array());
                sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)decimals"));
                sb.EmitAppCall(shash);

                sb.EmitParamJson(new MyJson.JsonNode_Array());
                sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)totalSupply"));
                sb.EmitAppCall(shash);


                var data = sb.ToArray();
                script = ThinNeo.Helper.Bytes2HexString(data);

            }

            ////script = "51c56b610a48656c6c6f737373737309576f726c6478787878617c68048418d60d680452a141f561516a00527ac46203006a00c3616c7566";
            //var url = Helper.MakeRpcUrl(api, "invokescript", new MyJson.JsonNode_ValueString(script));
            //string result = await Helper.HttpGet(url);

            //byte[] postdata;
            //var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));


            //var result = await Helper.HttpPost(url, postdata);


            var result =mgr.loader.rpc_invokescript(api,script);
            Console.WriteLine("得到的结果是：" + result);
        }
    }
}
