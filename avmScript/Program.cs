using System;
using System.Text;

namespace avmScript
{
    class Program
    {
        static void Main(string[] args)
        {
            var noparamAVM = System.IO.File.ReadAllBytes("D:\\Git\\NELLearn\\NeoContract\\bin\\Debug\\NeoContract.avm");

            var str = ThinNeo.Helper.Bytes2HexString(noparamAVM);
            //var str = ThinNeo.Helper.ToHexString(noparamAVM);
            Console.WriteLine("AVM=" + str);
            Console.ReadLine();
        }
    }
}
