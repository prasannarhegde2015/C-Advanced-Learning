using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace compretext
{
    class Program
    {
        static void Main(string[] args)
        {

            string ip = "<--Time--> <Daily Rate>                             __SepId  State      Well         SoFar Left  Oil  H2O  Gas  Status Message";
            Console.WriteLine("modified string ="+cleanIntermediateWhiteSpaces(ip));
            DateTime dtnow = DateTime.Now;
            DateTime dtnow4dback = dtnow.Subtract(TimeSpan.FromDays(4));
            Console.WriteLine("dt 4 days back " + dtnow4dback.ToString());


            Console.ReadLine();
        }

        private static string cleanIntermediateWhiteSpaces(string strinput)
        {

            string pattn = "\\s+";
            Regex re = new Regex(pattn);
            string retstring = re.Replace(strinput, " ");

            string op = "";
            char[] chartotrim = { ' ', '\n', '\t' };
            op = retstring.Trim(chartotrim);
            string fop = op.Replace('\n', '_');
            fop = fop.Replace('\r', '_');
            return retstring;

        }
    }
}
