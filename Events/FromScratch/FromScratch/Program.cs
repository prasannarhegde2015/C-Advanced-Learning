using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FromScratch
{
    class Program
    {
        static void Main(string[] args)
        {

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Path = @"E:\Test";
            watcher.Created += new FileSystemEventHandler(watcher_created);
            watcher.EnableRaisingEvents = true;
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(200);
                Console.WriteLine("executing Line numner " + i);
            }

            while (Console.ReadLine() != "exit") ;
        }

        static void watcher_created(object sender, FileSystemEventArgs e)
        {

            Console.WriteLine("Change Detected is :" + e.ChangeType);
            Console.WriteLine("File anme is " + e.FullPath);
        }
    }
}
