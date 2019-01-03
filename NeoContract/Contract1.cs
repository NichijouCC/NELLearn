﻿using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace NeoContract
{
    public class TPP: SmartContract
    {
        //public static bool Main()
        //{
        //    Storage.Put("Hellosssss", "Worldxxxx");
        //    return true;
        //}

        public static object Main(string method, params object[] args)
        {
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
                //if (ExecutionEngine.EntryScriptHash.AsBigInteger() != callscript.AsBigInteger())
                //    return false;
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
            return false;
        }
        /// <summary>
        /// 币名
        /// </summary>
        /// <returns></returns>
        public static string name()
        {
            return "SISTER";
        }

        /// <summary>
        /// 缩写币名
        /// </summary>
        /// <returns></returns>
        public static string symbol()
        {
            return "SSR";
        }
        /// <summary>
        /// 小数点位数
        /// </summary>
        /// <returns></returns>
        public static byte decimals()
        {
            return 1;
        }
        private const ulong factor = 10;//精度2
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

            return true;
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
