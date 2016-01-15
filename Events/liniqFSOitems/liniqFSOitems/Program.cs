using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace liniqFSOitems
{
    class Program
    {
        static void Main(string[] args)
        {
            string srcdir = ConfigurationManager.AppSettings["srcdir"];
            DirectoryInfo dir = new DirectoryInfo(srcdir);
            List<String> arrfiles = (from f in dir.GetFiles() 
                                     orderby f.Name 
                                     select f.Name).ToList();
        

            foreach(string innm in arrfiles)
            {
                File.AppendAllText(Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "list.txt"), innm+Environment.NewLine);
            }
        }
    }
}
