using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ParseATSOutFiles
{
    class Program
    {
        static void Main(string[] args)
        {

            parse2019format(@"C:\temp\UQAScriptsARST");
            parse2017format(@"C:\temp\UQAScriptsABFT");

        }

        static void parse2019format(string pathARST)
        {
            System.Console.WriteLine("Parsing ARST");
           // string path = @"C:\temp\UQAScripts";
            string path = pathARST;
            List<int> totaltest = new List<int>();
            List<int> totalpass = new List<int>();
            List<int> totalfail = new List<int>();
            List<int> totalskip = new List<int>();

            DirectoryInfo dirinf = new DirectoryInfo(path);
            DirectoryInfo[] allsubdirs = dirinf.GetDirectories();
            StringBuilder rept = new StringBuilder();
            string totalcount = String.Empty;
            string passcount = String.Empty;
            string failcount = String.Empty;
            string skipcount = String.Empty;

            foreach (DirectoryInfo inddir in allsubdirs)
            {
                FileInfo[] outfiles = inddir.GetFiles("*.out");

                foreach (FileInfo outfile in outfiles)
                {
                    // Parse File
                    IEnumerable<string> lines = File.ReadLines(outfile.FullName);

                    foreach (string sline in lines)
                    {

                        if (sline.Contains("Failed   ") || sline.Contains("X "))
                        {
                            rept.AppendLine(string.Format("{0},{1}",sline,outfile.Name.ToString().Replace(".arx","")));
                        }
                        if (sline.Contains("Passed   ") || sline.Contains("û "))
                        {
                            rept.AppendLine(string.Format("{0},{1}", sline, outfile.Name.ToString().Replace(".arx", "")));
                        }

                            //Sanple summary Total tests: 8. Passed: 6. Failed: 2. Skipped: 0.
                            if (sline.Contains("Total tests"))
                        {
                            totalcount = sline.Substring(sline.IndexOf("Total tests:")).Replace("Total tests:", "");
                            totaltest.Add(Convert.ToInt32(totalcount));
                        }
                        if (sline.Contains("Passed: "))
                        {
                            passcount = sline.Substring(sline.IndexOf("Passed:")).Replace("Passed: ", "");
                            totalpass.Add(Convert.ToInt32(passcount));
                        }
                        if (sline.Contains("Failed:"))
                        {
                            failcount = sline.Substring(sline.IndexOf("Failed:")).Replace("Failed:", "");
                            totalfail.Add(Convert.ToInt32(failcount));
                        }
                        if (sline.Contains("Skipped:"))
                        {
                            skipcount = sline.Substring(sline.IndexOf("Skipped:")).Replace("Skipped:", "");
                            totalskip.Add(Convert.ToInt32(skipcount));
                        }


                    }

                }
            }
            rept.AppendLine(string.Format("Total Test Count {0}: ", totaltest.Sum()));
            rept.AppendLine(string.Format("Total Pass Count {0}: ", totalpass.Sum()));
            rept.AppendLine(string.Format("Total Fail Count {0}: ", totalfail.Sum()));
            rept.AppendLine(string.Format("Total Skip Count {0}: ", totalskip.Sum()));
            System.Console.WriteLine(rept);
            File.AppendAllText(@"c:\temp\outARST.csv", rept.ToString());
        }

        static void parse2017format(string pathABFT)
        {
            System.Console.WriteLine("Parsing ABFT");
          //  string path = @"C:\temp\UQAScripts";
            string path = pathABFT;
            List<int> totaltest = new List<int>();
            List<int> totalpass = new List<int>();
            List<int> totalfail = new List<int>();
            List<int> totalskip = new List<int>();

            DirectoryInfo dirinf = new DirectoryInfo(path);
            DirectoryInfo[] allsubdirs = dirinf.GetDirectories();
            StringBuilder rept = new StringBuilder();
            string totalcount = String.Empty;
            string passcount = String.Empty;
            string failcount = String.Empty;
            string skipcount = String.Empty;

            foreach (DirectoryInfo inddir in allsubdirs)
            {
                FileInfo[] outfiles = inddir.GetFiles("*.out");

                foreach (FileInfo outfile in outfiles)
                {
                    // Parse File
                    IEnumerable<string> lines = File.ReadLines(outfile.FullName);

                    foreach (string sline in lines)
                    {

                        if (sline.Contains("Failed   ") || sline.Contains("X "))
                        {
                            rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("Failed   ", ""),"Falied", outfile.Name.ToString().Replace(".arx", "")));
                        }
                        if (sline.Contains("Passed   ") || sline.Contains("û "))
                        {
                            rept.AppendLine(string.Format("{0},{1},{2}", sline.Replace("Passed   ", ""), "Passed", outfile.Name.ToString().Replace(".arx", "")));
                        }
                        //Sanple summary Total tests: 8. Passed: 6. Failed: 2. Skipped: 0.
                        if (sline.Contains("Total tests:"))
                        {

                            totalcount = GetStringBetween(sline, "Total tests:", '.');
                            passcount = GetStringBetween(sline, "Passed:", '.');
                            failcount = GetStringBetween(sline, "Failed:", '.');
                            skipcount = GetStringBetween(sline, "Skipped:", '.');
                            
                            totaltest.Add(Convert.ToInt32(totalcount));
                            totalpass.Add(Convert.ToInt32(passcount));
                            totalfail.Add(Convert.ToInt32(failcount));
                            totalskip.Add(Convert.ToInt32(skipcount));

                        }
       
                    }

                }
            }
            rept.AppendLine(string.Format("Total Test Count {0}: ", totaltest.Sum()));
            rept.AppendLine(string.Format("Total Pass Count {0}: ", totalpass.Sum()));
            rept.AppendLine(string.Format("Total Fail Count {0}: ", totalfail.Sum()));
            rept.AppendLine(string.Format("Total Skip Count {0}: ", totalskip.Sum()));
            System.Console.WriteLine(rept);
            File.AppendAllText(@"c:\temp\outABFT.csv", rept.ToString());
        }
        static  string GetStringBetween(string mainstring,string StartString ,char endstring)
        {
           string  res = mainstring.Substring(mainstring.IndexOf(StartString));
            res = res.Substring(0,res.IndexOf(endstring));
            res = res.Replace(StartString, "");
            return res;
        }
    }
}
