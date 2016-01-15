using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace copyonlylatest
{
    class Program
    {
        static void Main(string[] args)
        {

            string src= System.Configuration.ConfigurationManager.AppSettings["src"];
            string dst= System.Configuration.ConfigurationManager.AppSettings["dst"];
            string indipath = System.Configuration.ConfigurationManager.AppSettings["ipath"];
            string attempts  = System.Configuration.ConfigurationManager.AppSettings["attempts"];
            var directory = new DirectoryInfo(src);
            var myDir = (from sbdir in directory.GetDirectories()
                         orderby sbdir.LastWriteTime descending
                         select sbdir).First();

            var myFile = (from f in myDir.GetFiles()
                          orderby f.LastWriteTime descending
                          select f).First();
            string outname = myDir.FullName+ "\\" + myFile;
            File.AppendAllText(indipath, outname.ToString());
        }
    }
}
