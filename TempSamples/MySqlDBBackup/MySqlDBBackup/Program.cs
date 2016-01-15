using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Data.Odbc;


namespace MySqlDBBackup
{
    class globalvars
    {

         public string CommandDirectory = ConfigurationManager.AppSettings["CommandDirectory"];
         public string baseDirectory = ConfigurationManager.AppSettings["baseDirectory"];
         public string mySQLtableslistfilename = ConfigurationManager.AppSettings["mySQLtableslistfilename"];
         public string BackUpDirectory
        {
            get 
            {
                    return  Path.Combine(baseDirectory,getBackupFolderName());
            }
        }
         public string LogDirectory 
        {
            get 
            {
                return   baseDirectory + "Log" + "\\";
            }
        }
         public string LogFile 
         {
             get 
             {
                 return  LogDirectory+"backuplog_"+getBackupFolderName()+".log";
             }
         }
         public string squashNetworkBackupPath 
         {
             get{
                 return Path.Combine(ConfigurationManager.AppSettings["squashNetworkBackupPath"], getBackupFolderName());
             }
         }
         public string getBackupFolderName()
        {
            string curDate = System.DateTime.Now.ToString("dd_MMM_yyyy");
            return curDate;
        }
         public string emaiList = ConfigurationManager.AppSettings["emaillist"];
         public string bkup = ConfigurationManager.AppSettings["bkup"];
         public string compress = ConfigurationManager.AppSettings["compress"];
         public string robocopy = ConfigurationManager.AppSettings["robocopy"];
         public string email = ConfigurationManager.AppSettings["email"];
    }
    
    class Program
    {
      
        static void Main(string[] args)
        {
            
            globalvars  gvar = new globalvars();
            creatdirifnotexist(gvar.LogDirectory);
            creatdirifnotexist(gvar.BackUpDirectory);
            creatdirifnotexist(gvar.squashNetworkBackupPath);
            Console.WriteLine("Started Backup Process..");
            logMessage("Backup Process Started");
            if (gvar.bkup.ToLower() == "true")
            {
                Console.WriteLine("1. Perform Backup");
                PerformMySqlBackUP(gvar.CommandDirectory, gvar.BackUpDirectory, gvar.mySQLtableslistfilename);
            }
            if (gvar.compress.ToLower() == "true")
            {
                Console.WriteLine("2. Perform Compression");
                PerformCompression(gvar.BackUpDirectory, gvar.getBackupFolderName());
            }
            if (gvar.robocopy.ToLower() == "true")
            {
                Console.WriteLine("3. Perform Coopy");
                PerformRoboCopy(gvar.BackUpDirectory, gvar.squashNetworkBackupPath, "");
            }
            if (gvar.email.ToLower() == "true")
            {
                Console.WriteLine("4. Send Email Notifiction");
                SendEmailNotification(gvar.getBackupFolderName(), gvar.squashNetworkBackupPath, gvar.emaiList);
            }
            Console.WriteLine("Completed Backup Process..");
            logMessage("Backup Process Completed");

        }

        public static void PerformMySqlBackUP(string cmdddir, string bkdir,string tablelookupfile)
        {
            logMessage("PerformBackup started");
            Console.WriteLine("Started Backup .....");
            DataTable mysqlinfo = getDatatablefromExcel(tablelookupfile);
            foreach (DataRow dr in mysqlinfo.Rows)
            {
                string dbname = dr["dbname"].ToString();
                string tablename = dr["tablename"].ToString();
                string outputfilename = dr["outputfilename"].ToString();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.FileName =  "\"" + Path.Combine(cmdddir,"mysqldump.exe") + "\"";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                logMessage("Executing command: " + Path.Combine(cmdddir,"mysqldump.exe") + " -u root -padmin " + dbname + "  " + tablename + " > " + Path.Combine(bkdir,outputfilename));
                startInfo.Arguments = " -u root -padmin" + " " + dbname + " " + tablename + " --result-file=" + Path.Combine(bkdir, outputfilename);
                Console.WriteLine("Performing backup for..... {0}", tablename);
                try
                {
                    using (Process exeProcess = Process.Start(startInfo))
                    {
                        exeProcess.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                    logMessage("Encountered Exception "+ ex.Message);
                }
                
            }
            Console.WriteLine("Completed Backup .....");
            logMessage("PerformBackup Completed");
        }
        public static void PerformCompression(string bkdir,string bkfile)
        {
            logMessage("PerformCompression  started");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "rar.exe";
            string targetrarFielName = Path.Combine(bkdir,bkfile+".rar");
	        string targetDirectory=bkdir;
	        logMessage(" ******** Archving Folders now ");
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            logMessage("Executing command: " + " rar a " + targetrarFielName + " -v256M -m2 " + Path.Combine(targetDirectory, "*.sql"));
            startInfo.Arguments = " a "+targetrarFielName+" -v256M -m2 "+ Path.Combine(targetDirectory,"*.sql");
            Console.WriteLine("Performing Compression .....");
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                logMessage("Encountered Exception " + ex.Message);
            }

          

            logMessage("PerformCompression  Completed");
            Console.WriteLine("Cleaning up  .....");
            foreach (string indfile in Directory.GetFiles(bkdir))
            {
                if (Path.GetFileName(indfile).Contains(".sql"))
                {
                    File.Delete(indfile);
                }
            }
            logMessage("Deleted all backed up .sql files retaining only .rar files");
        }
        public static void PerformRoboCopy(string src,string dst, string fln)
        {

          
            logMessage("PerformRobocopy   Started");
            logMessage("Source: "+src+" Destination:    "+dst+" File name: "+ fln);
            if (src != "" && dst != "")
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = "robocopy.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                if (fln.Length > 0)
                {
                    startInfo.Arguments = " \"" + src + "\"" + " \"" + dst + "\"" + " \"" + fln + "\"" + " /z";
                }
                else
                {
                    startInfo.Arguments = "  \"" + src + "\"" + " \"" + dst + "\"" + " *.* /e /z ";
                }
                Console.WriteLine("Performing Robocopy  .....");
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
            logMessage("PerformRobocopy   Completed");
        }
        public static void SendEmailNotification(string bkfol , string squashNetworkBackupPath, string ListTo)
        {
            logMessage("Send Email Started");
            Console.WriteLine("Sending Email .....");
            try
            {
                logMessage("adding receipeint");
                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                logMessage("Email list obtained from Top "+ListTo);
                string[] recipients = ListTo.Split(';');
                logMessage(recipients[0].ToString());
                foreach (string recipient in recipients)
                {
                    if (recipient.Length > 0)
                    {
                        message.To.Add(recipient);
                    }
                }
                logMessage("added receipient");
                message.Subject = "Squash backup- " + bkfol;
                logMessage("gettinng files count");
                message.From = new System.Net.Mail.MailAddress("noreply@bugnet-vm1.com");
                 int filescount = Directory.GetFiles(squashNetworkBackupPath).Length;
                 logMessage("got file count"+filescount);
                 if (filescount > 0)
                 {
                     message.Body = "Total of " + filescount + " files in Compressed Format chunks of 250 MB: are created. Please check path " + squashNetworkBackupPath;
                 }
                 else
                 {
                     message.Body = "Squash backup on U drive has been failed.";
                 }
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
                logMessage("Error in Sending Mails.." + ex.Message);
              //  throw new Exception("Error in Sending Mails.." + ex.Message);
            }
            logMessage("Send Email Completed");
            Console.WriteLine("Sent Email .....");
        }
        public static void  creatdirifnotexist(string dirname)
        {
            if (Directory.Exists(dirname) == false)
            {
                Directory.CreateDirectory(dirname);
            }
        }
        public static void logMessage(string msgtxt)
        {
            globalvars  gvar = new globalvars();
            File.AppendAllText(gvar.LogFile,DateTime.Now +":"+ msgtxt+ Environment.NewLine);
        }
        public static DataTable getDatatablefromExcel(string excelFilePath)
        {
            DataTable dt = new DataTable();
            string odbcconnection = "Driver={Microsoft Excel Driver (*.xls, *.xlsx, *.xlsm, *.xlsb)};DriverId=790;ReadOnly=0;Dbq="+excelFilePath;
            OdbcConnection conn = new OdbcConnection();
            conn.ConnectionString = odbcconnection;
            string query = "Select * from [Sheet1$]";
            OdbcCommand ocmd = new OdbcCommand(query);
            ocmd.Connection = conn;
            conn.Open();
            OdbcDataAdapter da = new OdbcDataAdapter(ocmd);
            da.Fill(dt);
            return dt;
        }
    }
}
