using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {

            string ip ="All Wells count is        345        in Lowis";
            string prefix = "All Wells count is";
            string sufix = "in Lowis";
            string outsr = ip.Replace(prefix, "");
            outsr = outsr.Replace(sufix, "");
            Console.WriteLine("output:" +  outsr.Trim());
            Console.ReadLine();


        }
    }
}
