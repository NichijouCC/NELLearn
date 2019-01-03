using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    /// <summary>
    /// 三连1/3 getstorage
    /// </summary>
    class sanlian_1
    {
        public static void Test(DataMgr mgr)
        {
            string api = "https://api.nel.group/api/testnet";
            string scriptaddress = "0x2e88caf10afe621e90142357236834e010b16df2";
            string key = "9b87a694f0a282b2b5979e4138944b6805350c6fa3380132b21a2f12f9c2f4b6";

            api = mgr.rpcUrl;
            scriptaddress = pubSmartContract.tokenScript;//tpp
            key = "totalSupply";

            byte[] bytes = Encoding.UTF8.GetBytes(key);
            key = ThinNeo.Helper.Bytes2HexString(bytes);

            var result = mgr.loader.rpc_getstorage_2(api, scriptaddress, key);
            var number= MyJson.Parse(result).AsDict().GetDictItem("result").AsString();

            var bytess = ThinNeo.Helper.HexString2Bytes(number);
            var real = new System.Numerics.BigInteger(bytess);

            //string result = await Helper.HttpGet(url);
            Console.WriteLine("key:" + key + "json:"+result+"   得到的结果是：" + real);
        }
    }
}
