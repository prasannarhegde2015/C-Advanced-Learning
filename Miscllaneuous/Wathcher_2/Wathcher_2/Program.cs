using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace Wathcher_2
{

    public static class globalvar
    {
        private static string _dpath;
        private static string _logpath, _emaillist, _bkbthfile, _buildname;
        private static bool _cfile;
        public static string Dpath
        {
            get { return _dpath; }
            set { _dpath = value; }
        }

        public static string Logpath
        {
            get { return _logpath; }
            set { _logpath = value; }
        }
        public static string EmailList
        {
            get { return _emaillist; }
            set { _emaillist = value; }
        }
        public static string BuildName
        {
            get { return _buildname; }
            set { _buildname = value; }
        }
        public static string bkBacthFile
        {
            get { return _bkbthfile; }
            set { _bkbthfile = value; }
        }

        public static bool chkfile
        {
            get { return _cfile; }
            set { _cfile = value; }
        }

    }
    class Program
    {
        private static FileSystemWatcher listener;
        static void Main(string[] args)
        {
            string[] argsc = System.Environment.GetCommandLineArgs();
            string sourcepath = ConfigurationManager.AppSettings["SourceDirectory"];
            globalvar.Dpath = ConfigurationManager.AppSettings["DestinationDirectory"];
            string emaillist = ConfigurationManager.AppSettings["EmailList"];
            globalvar.Logpath = ConfigurationManager.AppSettings["LogPath"];
            globalvar.BuildName = ConfigurationManager.AppSettings["BuildName"];
            globalvar.bkBacthFile = ConfigurationManager.AppSettings["BatchFile"];
            globalvar.EmailList = emaillist;
            //FileSystemWatcher listener;
            listener = new FileSystemWatcher();
            LogMessage(globalvar.Logpath, "Starting  listening to folder " + sourcepath);
            try
            {
                listener.Path = sourcepath;
              //  listener = new FileSystemWatcher(sourcepath);
                listener.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                         | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                //  watch all exe files.
                listener.Filter = "*.*";
                listener.Created += new FileSystemEventHandler(listener_Created);
                listener.EnableRaisingEvents = true;
                GC.KeepAlive(listener);
            }
            catch (Exception ex)
            {
                LogMessage(globalvar.Logpath, "Got Exepction while trying to listen as :   " + ex.Message);
                System.Diagnostics.Process.Start(globalvar.bkBacthFile);
                GC.KeepAlive(listener);
            }
            LogMessage(globalvar.Logpath, "Started listening to folder " + sourcepath);
            while (Console.ReadLine() != "exit") ;
            
        }

        public static void listener_Created(object sender, FileSystemEventArgs e)
        {
            string excludefiles = ConfigurationManager.AppSettings["ExcludeString"];
            Console.WriteLine
                    (
                        "File Created:\n"
                       + "ChangeType: " + e.ChangeType
                       + "\nName: " + e.Name
                       + "\nFullPath: " + e.FullPath
                    );
            LogMessage(globalvar.Logpath, "File Created:\n"
                       + "ChangeType: " + e.ChangeType
                       + "\nName: " + e.Name
                       + "\nFullPath: " + e.FullPath);
           
            if (e.FullPath.Contains(".exe") && !e.FullPath.Contains(excludefiles) )
            {

                FileInfo fileinfo = new FileInfo(e.FullPath);
                try
                {

                    bool waitflag = IsFileLocked(fileinfo);
                    bool fileexists = File.Exists(e.FullPath);
                    globalvar.chkfile = fileexists;
                    while (waitflag == true && fileexists == true)
                    {
                        System.Threading.Thread.Sleep(1000);
                        Console.WriteLine("WaitFlag Value:=" + waitflag);
                        LogMessage(globalvar.Logpath, "WaitFlag Value:=" + waitflag);
                        waitflag = IsFileLocked(fileinfo);
                        fileexists = File.Exists(e.FullPath);
                        globalvar.chkfile = fileexists;
                        Console.WriteLine("File Exists" + fileexists);
                        
                    }
                    Console.WriteLine("WaitFlag Vlaue:=" + waitflag);
                    LogMessage(globalvar.Logpath, "WaitFlag Value:=" + waitflag);
                }
                catch (Exception ex)
                {
                    LogMessage(globalvar.Logpath, "Got Error while  Checking for File Lock as  : " + ex.Message);
                    LogMessage(globalvar.Logpath, "Executing backup batchfile : " + globalvar.bkBacthFile);
                   // System.Diagnostics.Process.Start(globalvar.bkBacthFile);
                }
                System.Threading.Thread.Sleep(5000);
                if (globalvar.chkfile  == true)
                {

                    try
                    {
                        if (e.FullPath.Contains(".exe") && !e.FullPath.Contains(excludefiles) )
                        {
                           // File.Copy(e.FullPath, globalvar.Dpath + "\\" + e.Name);
                            dorobocopy(ConfigurationManager.AppSettings["SourceDirectory"], globalvar.Dpath, e.Name);
                            LogMessage(globalvar.Logpath, "File Copy Success for " + globalvar.Dpath + "\\" + e.Name);
                          //  sendemails(globalvar.EmailList, globalvar.Dpath + "\\" + e.Name);
                        }
                        else
                        {
                            LogMessage(globalvar.Logpath, "Folder creation was ignore and not copied  ");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage(globalvar.Logpath, "Got Error while  copying file as : " + ex.Message);
                        LogMessage(globalvar.Logpath, "Executing backup batchfile : " + globalvar.bkBacthFile);
                        System.Diagnostics.Process.Start(globalvar.bkBacthFile);
                        /* Wait For Porcess to complete 
                        System.Diagnostics.Process p = new System.Diagnostics.Process();

                        p.StartInfo.FileName = globalvar.bkBacthFile;
                        p.Start();
                        while (!p.HasExited)
                        {
                            Console.WriteLine("Robocopy Process Running: "  + !p.HasExited);
                            System.Threading.Thread.Sleep(10);
                            p.Refresh();

                        } 
                         * */
                        sendemails(globalvar.EmailList, globalvar.Dpath + "\\" + e.Name);
                    }
                }
                else
                {
                    LogMessage(globalvar.Logpath, "Folder creation was ignore and not acted upon ");
                }
            }
            // Console.Read();
        }


        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (Exception ex)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                if (stream != null)
                {
                    LogMessage(globalvar.Logpath, "Stream was not null hence closing and disposing");
                    stream.Close();
                    stream.Dispose();
                }
                LogMessage(globalvar.Logpath, "Error : " + ex.Message);
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }

            //file is not locked
            return false;
        }

        public static void sendemails(string ListTo, string fileName)
        {
            try
            {

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                string[] recipients = ListTo.Split(';');
                foreach (string recipient in recipients)
                {
                    message.To.Add(recipient);
                }
                message.Subject = globalvar.BuildName + " Automated Copy Process";
                message.From = new System.Net.Mail.MailAddress("noreply@bugnet-vm1.com");
                message.Body = globalvar.BuildName + " downloaded @ location :" + fileName;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("mail2.weatherford.com");
                smtp.Port = 25;

                /*    foreach (string attachmentFilename in attachments)
                    {
                        if (System.IO.File.Exists(attachmentFilename))
                        {
                            var attachment = new System.Net.Mail.Attachment(attachmentFilename);
                            message.Attachments.Add(attachment);
                        }
                    } */

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Sending Mails.." + ex.Message);
            }
        }

        public static void LogMessage(string filePath, string message)
        {
            File.AppendAllText(filePath, "[" + System.DateTime.Now + "] :" + message + System.Environment.NewLine);
        }

        private static void dorobocopy(string src, string dst, string fln)
        {


            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "robocopy.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = " \"" + src + "\"" + " \"" + dst + "\"" + " \"" + fln + "\"" + " /z";
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }
    }
}
