using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;



namespace CreaterResultHTMLfrom_XML
{
    class Program
    {
        static void Main(string[] args)
        {
            //DataSet d1 = new DataSet();
            //d1.ReadXml(ConfigurationManager.AppSettings["inputfile"]);
            //int cnt = 1;
            //string fname = "set";
            //foreach( DataTable dtin  in d1.Tables)
            //{
            //    DataTable dt1 = dtin;
            //    LogtoFileCSV(dt1,fname+cnt+".csv");
            //    cnt++;
            //}

            genfile(ConfigurationManager.AppSettings["inputfile"]);
         //   Console.ReadLine();

        }


        public static void LogtoFileCSV(DataTable dtin, string fname)
        {
            char delm = '\u0022';
            StringBuilder sb = new StringBuilder();

            if (System.IO.File.Exists(Path.Combine(ConfigurationManager.AppSettings["logpath"], fname)) == false)
            {
                //Adding Header Row only once
                for (int kk = 0; kk < dtin.Columns.Count; kk++)
                {
                    sb.Append(delm + dtin.Columns[kk].ColumnName.ToString() + delm + ",");

                }

                sb.Append(Environment.NewLine);
            }

            for (int i = 0; i < dtin.Rows.Count; i++)
            {
                for (int kk = 0; kk < dtin.Columns.Count; kk++)
                {
                    sb.Append(delm + dtin.Rows[i][kk].ToString() + delm + ",");
                }
                sb.Append(Environment.NewLine);
            }



            System.IO.File.AppendAllText(Path.Combine(ConfigurationManager.AppSettings["logpath"], fname), sb.ToString());
        }
        public static void genfile(string xmlfile)
        {

            StringBuilder sb = new StringBuilder();
            string mpfailed = "";
             int  necnt = -1 ;
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append("<b> Overall Summary <b> ");
            sb.Append("<br>");
            sb.Append("<table  border='1' >");
            sb.Append("<tr>");
            sb.Append("<td> <b>Total</b> </td>");
            sb.Append("<td> <b>Executed</b> </td>");
            sb.Append("<td> <font color='green' > <b>Passed</b></font> </td>");
            sb.Append("<td> <font color='red' ><b>Failed</b> </font></td>");
            sb.Append("</tr>");
            DirectoryInfo drinfo = new DirectoryInfo(ConfigurationManager.AppSettings["trxfilelocation"]);
            foreach( var file in drinfo.GetFiles("*.trx") )
            {

            
            XmlDocument xml = new XmlDocument();
            xml.Load(file.FullName.ToString());
            XmlNodeList xnList = xml.GetElementsByTagName("ResultSummary");
            foreach (XmlNode xn in xnList)
            {

                Console.WriteLine("Outcome: {0} ", xn.Attributes["outcome"].InnerText);

            }
             xnList = xml.GetElementsByTagName("Counters");
            foreach (XmlNode xn in xnList)
            {

                Console.WriteLine("Total: {0} ", xn.Attributes["total"].InnerText);
                string ntotal = xn.Attributes["total"].InnerText;
                Console.WriteLine("Executed: {0} ", xn.Attributes["executed"].InnerText);
                string nexecuted = xn.Attributes["executed"].InnerText;
                Console.WriteLine("Passed: {0} ", xn.Attributes["passed"].InnerText);
                string mpassed = xn.Attributes["passed"].InnerText;
                Console.WriteLine("Failed: {0} ", xn.Attributes["failed"].InnerText);
                mpfailed = xn.Attributes["failed"].InnerText;

            sb.Append("<tr>");
            sb.Append("<td> "+ntotal+" </td>");
            sb.Append("<td> "+nexecuted+" </td>");
            sb.Append("<td> <font color='green' >"+mpassed+" <font> </td>");
            sb.Append("<td> <font color='red' >" + mpfailed + "<font> </td>");
            sb.Append("</tr>");
            


            }
            sb.Append("</table>");
            sb.Append("<br>");
            sb.Append("<br>");
            sb.Append("<br>");
            sb.Append("<b> Test Case Detailed Summary:  <b> ");
            sb.Append("<br>");
            sb.Append("<table  border='1' >");
            sb.Append("<tr>");
            sb.Append("<td>  <b>Test Case Name </b></td>");
            sb.Append("<td>  <b>Execution Machine Name:</b> </td>");
            sb.Append("<td>  <b>Test Start Time</b></td>");
            sb.Append("<td>  <b>Test End Time</b></td>");
            sb.Append("<td>  <b>Test Run Duration</b></td>");
            sb.Append("<td>  <b>Status</b> </td>");
            sb.Append("<td>  <b>Error Info ( If any )</b> </td>");
            sb.Append("</tr>");







            string errinfo = "No Errors";
            xnList = xml.GetElementsByTagName("UnitTestResult");
            string duration = "", tstart = "", tend = "" ,outcome ="";

            foreach (XmlNode xn in xnList)
            {
                XmlElement xParentEle = xn as XmlElement;
                Console.WriteLine(" Test Case Name: {0} ", xn.Attributes["testName"].InnerText);
                string tcname = xn.Attributes["testName"].InnerText;
                Console.WriteLine(" Execution Machine Name: {0} ", xn.Attributes["computerName"].InnerText);
                string compname= xn.Attributes["computerName"].InnerText;
                if (xParentEle.HasAttribute("duration"))
                {
                    Console.WriteLine(" Test Run Duration {0} ", xn.Attributes["duration"].Value);
                    duration = xn.Attributes["duration"].Value;
                }
                if (xParentEle.HasAttribute("startTime"))
                {
                    Console.WriteLine(" Test Start Time: {0} ", DateTime.Parse(xn.Attributes["startTime"].InnerText).ToString("dd-MMM-yyyy hh:mm:ss"));
                    tstart = DateTime.Parse(xn.Attributes["startTime"].InnerText).ToString("dd-MMM-yyyy hh:mm:ss");
                }

                if (xParentEle.HasAttribute("endTime"))
                {
                    Console.WriteLine(" Test End Time: {0} ", DateTime.Parse(xn.Attributes["endTime"].InnerText).ToString("dd-MMM-yyyy hh:mm:ss"));
                    tend = DateTime.Parse(xn.Attributes["endTime"].InnerText).ToString("dd-MMM-yyyy hh:mm:ss");
                }

                if (xParentEle.HasAttribute("outcome"))
                {
                     outcome = xn.Attributes["outcome"].InnerText;
                }

                XmlNodeList xml2list = xn.ChildNodes;
                if (xml2list.Count > 0)
                {
                    // output 
                    foreach(XmlNode xn2  in xml2list)
                    {
                        XmlNodeList xml3list = xn2.ChildNodes;
                        // Error Info
                        foreach (XmlNode xn3 in xml3list)
                        {
                            XmlNodeList xml4list = xn3.ChildNodes;
                            foreach (XmlNode xn4 in xml4list)
                            {
                                if (xn4.Name == "Message")
                                {
                                    errinfo = xn4.InnerText;
                                }

                                //if (errinfo.Length > 255)
                                //{
                                //    errinfo = errinfo.Substring(0, 255);
                                //    errinfo = errinfo + " ...and more ";
                                //}

                            }

                        }
                    }
                    
                }
                
                sb.Append("<tr>");
                sb.Append("<td> "+tcname  +"</td>");
                sb.Append("<td> "+compname+"  </td>");
                sb.Append("<td> "+tstart+"</td>");
                sb.Append("<td> "+tend+"</td>");
                sb.Append("<td> "+duration+" </td>");
                
             //   sb.Append("<td> " + returnstatus(@"E:\Project\Lowis7\TestData\Results\LowisReports.csv") + " </td>");
                //if (returnstatus(@"E:\Project\Lowis7\TestData\Results\LowisReports.csv") == "Failed")
                //{
                //    sb.Append("<td>  <font color ='red' > Failed </font> </td>");
                //}
                //else
                //{
                //    sb.Append("<td>  <font color ='green' > Passed </font> </td>");
                //}

                if (errinfo != "No Errors")
                {
                    if (outcome != "Failed")
                    {
                        outcome = "Failed";
                        necnt = Int32.Parse(mpfailed);
                        necnt++;
                        sb.Replace("<td> <font color='red' >" + mpfailed + "<font> </td>", "<td> <font color='red' >" + necnt.ToString() + "<font> </td>");
                    }
                    
                }
                if (outcome == "Failed")
                {
                    sb.Append("<td>  <font color ='red' > Failed </font> </td>");
                }
                else
                {
                    sb.Append("<td>  <font color ='green' > Passed </font> </td>");
                }
                sb.Append("<td> " + errinfo + " </td>");
                sb.Append("</tr>");
                
                
            }
            sb.Append("</table>");




            sb.Append("</body>");
            sb.Append("</html>");
            string ooufile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString(), "TestResults.html");
            File.AppendAllText(ooufile, sb.ToString());
            System.Diagnostics.Process.Start(ooufile);
            }

        }

        public static string returnstatus(string csvfilePath)
        {
            string status = "NA";
            
            DataTable dtResults = GetResultsData(csvfilePath, "");
            DataRow[] success = dtResults.Select("Result= 'Pass'");
            DataRow[] failed = dtResults.Select("Result='Fail'");

            if (failed.Length > 0)
            {
                status = "Failed";
            }
            else
            {
                status = "Pased";
            }


            return status;
        }


        public static DataTable GetResultsData(string testDataFile, string testCase)
        {


            Helper.TestDataManagement testData = new Helper.TestDataManagement();
            Console.WriteLine("Trying to get testat from" + testDataFile);
            testData.GetTestData(testDataFile, "");
            DataTable dt = testData.Data;
            //Console.WriteLine(dt.Rows.Count.ToString());
            return dt;
        }
    }
}
