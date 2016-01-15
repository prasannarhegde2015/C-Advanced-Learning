using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace TestODBC_Connection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label1.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string filepath = null;
            string sheetname = null;
            filepath = textBox2.Text;
            sheetname = textBox1.Text;
           
            bool odbcfound = true ;
            if (sheetname.Length > 0)
            {
                try
                {
                    DataTable dtble = new DataTable();

                    OdbcConnection oconn = new OdbcConnection();
                    oconn.ConnectionString = ConfigurationManager.ConnectionStrings["ReportLinks"].ToString() + filepath;
                    string odbccmdtext = "Select * from [" + sheetname + "$]";
                    OdbcCommand ocmd = new OdbcCommand(odbccmdtext, oconn);
                    oconn.Open();
                    OdbcDataAdapter da = new OdbcDataAdapter(ocmd);
                    da.Fill(dtble);
                    oconn.Close();
                    // odbcfound = true;

                }
                catch
                {
                    odbcfound = false;
                    // throw new Exception();
                }
                finally
                {
                    //
                }

                if (odbcfound)
                {
                    label1.Show();
                    label1.Text = "ODBC Drivers Exist";
                    label1.ForeColor = Color.Green;
                }

                else
                {
                    label1.Show();
                    label1.Text = "ODBC Drivers Do not Exist";
                    label1.ForeColor = Color.Red;

                }
            }
            else
            {
                MessageBox.Show("Need to enter SheetName Manually", "Sheet Name required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            openFileDialog1.Title = "Select Excel File";
            openFileDialog1.Filter = "Excel Files | (*.xls,*.xlsx)";
            textBox2.Text = openFileDialog1.FileName;
        }

        
    }
}
