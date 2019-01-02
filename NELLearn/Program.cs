using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    class Program
    {
        static string rpcurl = "http://localhost:10332/";
        static DataMgr mgr;
        static Transaction trans;
        static void Main(string[] args)
        {
            Console.WriteLine("BeginScan!");

            //rpcurl = "http://111.230.231.208:40332";
            rpcurl = "http://localhost:40332";
            mgr = new DataMgr(rpcurl);

            AsyncLoop();
        }

        async static void AsyncLoop()
        {
            while (true)
            {
                //Console.Clear();
                //Console.WriteLine("******************同步区块： " + mgr.paraser.processedBlock);
                Console.Write("cmd>");
                string cmd = Console.ReadLine();
                cmd = cmd.Replace(" ", "");
                if (cmd == "") continue;
                switch (cmd)
                {
                    case "11":
                        Console.WriteLine("$$$$$$$$$$$$$----------发布合约测试------------------");
                        pubSmartContract.test(mgr);
                        break;
                    case "1":
                        Console.WriteLine("$$$$$$$$$$$$$----------查看资产测试------------------");

                        Console.Write("请输入address：");
                        string address = Console.ReadLine();
                        if (address != "")
                        {
                            mgr.getAddressMoney(address);
                        }
                        else
                        {
                            mgr.getAddressMoney("AJi1N4tqLvjoYzQBRy8JAfuctvxxbnxoAP");
                        }
                        break;
                    case "2":
                    case "state":
                        mgr.showState();
                        break;

                    case "3":
                        Console.WriteLine("$$$$$$$$$$$$$----------转账测试------------------");
                        try
                        {
                            Transaction.test(mgr);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("转账问题：" + e.Message);
                        }
                        break;
                    case "s1":
                        Console.WriteLine("$$$$$$$$$$$$$----------getStorage测试------------------");
                        try
                        {
                            Sanlian_1.Test(mgr);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("getStorage测试问题：" + e.Message);
                        }
                        break;
                    case "s2":
                        Console.WriteLine("$$$$$$$$$$$$$----------invokeScript测试------------------");
                        try
                        {
                            Sanlian_2.Test(mgr);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("invokeScript测试问题：" + e.Message);
                        }
                        break;
                    case "s3":
                        Console.WriteLine("$$$$$$$$$$$$$----------transfer测试------------------");
                        try
                        {
                            sanlian_3.test(mgr);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("transfer测试问题：" + e.Message);
                        }
                        break;
                    case "help":
                    case "?":
                        break;
                    case "save":
                        break;
                    default:
                        Console.WriteLine("unknown cmd,type help to get more.");
                        break;
                }
            }
        }

    }



}
