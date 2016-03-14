using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using ConsoleControl;


namespace RunRTUEmulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            btnStop.Enabled = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
        }
        ConsoleControl.ConsoleControl cntrl = new ConsoleControl.ConsoleControl();
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (txtControllerType.Text.Length > 0)
            {
                string ControllerType = txtControllerType.Text;
                RunRTUEmulator.Helper hlr = new RunRTUEmulator.Helper();
                DataTable dtemulist = hlr.dtFromExcelFile(@"C:\RTUEmu\RTEMUData.xls", "Template");
                DataRow[] dr = dtemulist.Select("ControllerType='" + ControllerType + "'");
                if (dr.Length == 0)
                {
                    MessageBox.Show("Controller Type Entered Does not Exist for your entered letters ... Please Select from Auto P0pulated list by typing 3 letters");
                    return;
                }
                string conffilepath = dr[0]["FileFullpath"].ToString();
                updateConfigFile(conffilepath);
                RunRTUEMu();
            }
            else
            {
                MessageBox.Show(" Please Select Controller Type from Auto Populated list by typing 3 letters");
                return;
            }
        }


        private void updateConfigFile(string configval)
        {
            XmlDocument xldoc = new XmlDocument();
            xldoc.Load(@"C:\RTUEmu\RtuEmu.exe.config");
            XmlNodeList xlist = xldoc.SelectNodes("//setting");
            bool found = false;
            foreach (XmlNode xn in xlist)
            {
                XmlAttributeCollection allatrributes = xn.Attributes;
                if (allatrributes.Count > 0)
                {
                    foreach (XmlAttribute attr in allatrributes)
                    {
                        if (attr.InnerText == "configFilePath")
                        {
                            XmlNode valnode = xn.SelectSingleNode("//value");
                            valnode.InnerText = configval;
                            found = true;
                            break;
                        }

                    }
                }
                if (found)
                {
                    break;
                }
            }
            xldoc.Save(@"C:\RTUEmu\RtuEmu.exe.config");
        }

        private void RunRTUEMu()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = @"C:\RTUEMU\RTUEMU.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //startInfo.Arguments = " \"" + src + "\"" + " \"" + dst + "\"" + " \"" + fln + "\"" + " /z";
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
        private void RunRTUAsync()
        {
            
            cntrl.StartProcess(@"C:\RTUEMU\RTUEMU.exe","");
            cntrl.Update();
            richTextBox1.Text = " RTU Emulator Has been started";
            richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Bold);
            richTextBox1.BackColor = Color.Green;
            richTextBox1.ForeColor = Color.White;
            btnasync.Enabled = false;
            btnStop.Enabled = true;
        }


       
        private void txtControllerType_TextChanged(object sender, EventArgs e)
        {
            AutoCompleteStringCollection asc = new AutoCompleteStringCollection();
            RunRTUEmulator.Helper hlr = new RunRTUEmulator.Helper();
            DataTable dtemulist = hlr.dtFromExcelFile(@"C:\RTUEmu\RTEMUData.xls", "Template");
            string[] ctrlarr = new string[dtemulist.Rows.Count];
            int i = 0;
            foreach (DataRow dr in dtemulist.Rows)
            {
                ctrlarr[i] = dr["ControllerType"].ToString();
                i++;
            }
            asc.AddRange(ctrlarr);
            if (txtControllerType.Text.Length > 2)
            {
                txtControllerType.AutoCompleteCustomSource = asc;
                txtControllerType.AutoCompleteMode = AutoCompleteMode.Suggest;
                txtControllerType.AutoCompleteSource = AutoCompleteSource.CustomSource;

            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            cntrl.StopProcess();
            richTextBox1.Text = " RTU Emulator Has been Stopped";
            richTextBox1.Font = new Font(richTextBox1.Font, FontStyle.Bold);
            richTextBox1.BackColor = Color.Red;
            richTextBox1.ForeColor = Color.White;
            btnasync.Enabled = true;
            btnStop.Enabled = false;
        }

        private void btnasync_Click(object sender, EventArgs e)
        {
            if (txtControllerType.Text.Length > 0)
            {
                string ControllerType = txtControllerType.Text;
                RunRTUEmulator.Helper hlr = new RunRTUEmulator.Helper();
                DataTable dtemulist = hlr.dtFromExcelFile(@"C:\RTUEmu\RTEMUData.xls", "Template");
                DataRow[] dr = dtemulist.Select("ControllerType='" + ControllerType + "'");
                if (dr.Length == 0)
                {
                    MessageBox.Show("Controller Type Entered Does not Exist for your entered letters ... Please Select from Auto Populated list by typing 3 letters","Controller Selection",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    return;
                }
                string conffilepath = dr[0]["FileFullpath"].ToString();
                updateConfigFile(conffilepath);
                RunRTUAsync();
            }
            else
            {
                MessageBox.Show(" Please Select Controller Type from Auto Populated list by typing 3 letters", "Controller Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
    }


    public class PlinkWrapper
    {
        private string host { get; set; }
        /// <summary>
        /// Initializes the <see cref="PlinkWrapper"/>
        /// Assumes the key for the user is already loaded in PageAnt.
        /// </summary>
        /// <param name="host">The host, on format user@host</param>
        public PlinkWrapper(string host)
        {
            this.host = host;
        }

        /// <summary>
        /// Runs a command and returns the output lines in a List&lt;string&gt;.
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns></returns>
        public List<string> RunCommand()
        {
            List<string> result = new List<string>();

            ProcessStartInfo startInfo = new ProcessStartInfo(@"C:\RTUEMU\RTUEMU.exe");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
         //   startInfo.Arguments =  command;
            using (Process p = new Process())
            {
                p.StartInfo = startInfo;
                p.Start();
                while (p.StandardOutput.Peek() >= 0)
                {
                    result.Add(p.StandardOutput.ReadLine());
                }
                p.WaitForExit();
            }

            return result;
        }
    }

}
