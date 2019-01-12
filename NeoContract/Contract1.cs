using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace NeoContract
{
    public class TPP: SmartContract
    {
        //public static bool Main()
        //{
        //    Storage.Put("Hellosssss", "Worldxxxx");
        //    return true;
        //}

        public delegate void deleTransfer(byte[] from, byte[] to, BigInteger value);
        [DisplayName("transfer")]
        public static event deleTransfer Transferred;

        public class TransferInfo
        {
            public byte[] from;
            public byte[] to;
            public BigInteger value;
        }

        public static object Main(string method, params object[] args)
        {
            //必须在入口函数取得callscript，调用脚本的函数，也会导致执行栈变化，再取callscript就晚了
            var callscript = ExecutionEngine.CallingScriptHash;

            //this is in nep5
            if (method == "totalSupply") return totalSupply();
            if (method == "name") return name();
            if (method == "symbol") return symbol();
            if (method == "decimals") return decimals();
            if (method == "balanceOf")
            {
                if (args.Length != 1) return 0;
                byte[] who = (byte[])args[0];
                if (who.Length != 20)
                    return false;
                return balanceOf(who);
            }
            if (method == "transfer")
            {
                if (args.Length != 3) return false;
                byte[] from = (byte[])args[0];
                byte[] to = (byte[])args[1];
                if (from == to)
                    return true;
                if (from.Length != 20 || to.Length != 20)
                    return false;
                BigInteger value = (BigInteger)args[2];
                //没有from签名，不让转
                if (!Runtime.CheckWitness(from))
                    return false;
                ////如果有跳板调用，不让转
                if (ExecutionEngine.EntryScriptHash.AsBigInteger() != callscript.AsBigInteger())
                    return false;
                //如果to是不可收钱合约,不让转
                if (!IsPayable(to)) return false;

                return transfer(from, to, value);
            }

            if (method == "deploy")
            {
                //if (args.Length != 1) return false;
                if (!Runtime.CheckWitness(superAdmin)) return false;
                //byte[] total_supply = Storage.Get(Storage.CurrentContext, "totalSupply");
                //if (total_supply.Length != 0) return false;
                //var keySuperAdmin = new byte[] { 0x11 }.Concat(superAdmin);
                Storage.Put(Storage.CurrentContext, superAdmin, totalCoin);
                Storage.Put(Storage.CurrentContext, "totalSupply", totalCoin);
            }

            if (method == "getTxInfo")
            {
                if (args.Length != 1) return 0;
                byte[] txid = (byte[])args[0];
                return getTxInfo(txid);
            }
            return false;
        }
        /// <summary>
        /// 币名
        /// </summary>
        /// <returns></returns>
        public static string name()
        {
            return "Sister_P";
        }

        /// <summary>
        /// 缩写币名
        /// </summary>
        /// <returns></returns>
        public static string symbol()
        {
            return "SSRP";
        }
        /// <summary>
        /// 小数点位数
        /// </summary>
        /// <returns></returns>
        public static byte decimals()
        {
            return 2;
        }
        private const ulong factor = 100;//精度2
        private const ulong totalCoin = 10 * 10000 * factor;//发行量

        static readonly byte[] superAdmin = Neo.SmartContract.Framework.Helper.ToScriptHash("AcjVGYytBysSdQTLZXpLarvVVYYNUiiUgG");//管理员
        //发行总量
        public static BigInteger totalSupply()
        {
            return Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();
        }

        /// <summary>
        /// Returns 账户的token金额
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static BigInteger balanceOf(byte[] account)
        {
            var originatorValue = Storage.Get(Storage.CurrentContext, account).AsBigInteger();
            return originatorValue;
        }
        /// <summary>
        /// 交易
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool transfer(byte[] from, byte[] to, BigInteger amount)
        {
            if (amount <= 0) return false;
            if (from == to) return true;

            var fromvalue = Storage.Get(Storage.CurrentContext, from).AsBigInteger();
            if (fromvalue < amount) return false;
            BigInteger fromNowValue =fromvalue - amount;
            if(fromNowValue==0)
            {
                Storage.Delete(Storage.CurrentContext, from);
            }else
            {
                Storage.Put(Storage.CurrentContext,from,fromNowValue);
            }

            var targetValue = Storage.Get(Storage.CurrentContext, to).AsBigInteger();
            BigInteger toNowValue = targetValue + amount;
            Storage.Put(Storage.CurrentContext,to,toNowValue);

            //记录交易信息
            setTxInfo(from, to, amount);
            //notify
            Transferred(from, to, amount);

            return true;
        }

        private static void setTxInfo(byte[] from, byte[] to, BigInteger value)
        {
            TransferInfo info = new TransferInfo();
            info.from = from;
            info.to = to;
            info.value = value;
            byte[] txinfo = Helper.Serialize(info);
            var txid = (ExecutionEngine.ScriptContainer as Transaction).Hash;
            var keytxid = new byte[] { 0x13 }.Concat(txid);
            Storage.Put(Storage.CurrentContext, keytxid, txinfo);
        }
        public static TransferInfo getTxInfo(byte[] txid)
        {
            //byte[] keytxid = new byte[] { 0x13 }.Concat(txid);
            byte[] v = Storage.Get(Storage.CurrentContext, txid);
            if (v.Length == 0)
                return null;
            return Helper.Deserialize(v) as TransferInfo;
        }
        public static bool IsPayable(byte[] to)
        {
            var c = Blockchain.GetContract(to);
            if (c.Equals(null))
                return true;
            return c.IsPayable;
        }
    }
}
