using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// get storage
    /// </summary>
    class Sanlian_1
    {
        public static void Test(DataMgr mgr)
        {
            string api = "https://api.nel.group/api/testnet";

            string scriptaddress = "0x2e88caf10afe621e90142357236834e010b16df2";
            string key = "9b87a694f0a282b2b5979e4138944b6805350c6fa3380132b21a2f12f9c2f4b6";
            var rev = ThinNeo.Helper.HexString2Bytes(key).Reverse().ToArray();
            var revkey = ThinNeo.Helper.Bytes2HexString(rev);

            //var url = Helper.MakeRpcUrl(api, "getstorage", new MyJson.JsonNode_ValueString(scriptaddress), new MyJson.JsonNode_ValueString(key));

            var result= mgr.loader.rpc_getstorage(api,scriptaddress,key);

            //string result = await Helper.HttpGet(url);
            Console.WriteLine("得到的结果是：" + result);
        }
    }
}
