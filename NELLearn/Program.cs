using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    class Program
    {
        static readonly string rpcurl = "http://localhost:10332/";

        static void Main(string[] args)
        {
            Console.WriteLine("BeginScan!");
            DataMgr mgr = new DataMgr(rpcurl);

            System.Threading.Thread thread = new System.Threading.Thread(async () =>
            {
                await mgr.CheckBlockLoop();
            });
            thread.Start();

            while (true)
            {
                Console.Write("cmd>");
                string cmd = Console.ReadLine();
                cmd = cmd.Replace(" ", "");
                if (cmd == "") continue;
                switch (cmd)
                {
                    case "exit":
                        
                        return;
                    case "help":
                    case "?":
                        break;
                    case "state":
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
