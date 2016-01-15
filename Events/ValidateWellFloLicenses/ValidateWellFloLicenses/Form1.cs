using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ValidateWellFloLicenses
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dataGridView1.Visible = false;
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
        }
        private bool isFormatted = false;
        private void button1_Click(object sender, EventArgs e)
        {
            string[] WellFloLicenseList = new string[] { "WELLFLO", "WELLFLO_ESP", "WELLFLO_GLV", "WELLFLO_FLOWASSURANCE", "WELLFLO_COMP_FLUID_MODELLING", "WELLFLO_JETPUMP", "WELLFLO_PCP", "WELLFLO_ICD", "WELLFLO_PLUNGERLIFT", "WELLFLO_RRL", "WELLFLOCOM", "TECH_LIB" ,"PVTFLEX_COMP" };
            if (File.Exists(@"C:\Flexlm\License.Dat") == false)
            {
                MessageBox.Show("The System is not having License File <C:\\Flexlm\\License.Dat> ", "License File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string line = "";
            DataTable dt = new DataTable();
            dt.Columns.Add("Serial Number");
            dt.Columns.Add("WellFlo License Name");
            dt.Columns.Add("WellFlo License Present (Y/N)");
            dt.Columns.Add("License Expiry Date");

            bool isdate = false;
            DateTime dt1 = DateTime.Now;
            int count = 1;
            bool featurefound = false;
            foreach (string indlic in WellFloLicenseList)
            {
                DataRow dr = dt.NewRow();
                dr["Serial Number"] = count;
                dr["WellFlo License Name"] = indlic;
                StreamReader textfilereader = new StreamReader(@"C:\Flexlm\License.Dat");
                #region ReadEachLine
                while ((line = textfilereader.ReadLine()) != null)
                {
                    if (line.Contains(" " + indlic + " ") && line.Contains("FEATURE"))
                    {
                        featurefound = true;
                        string[] licdetails = line.Split(' ');
                        foreach (string indlicdetail in licdetails)
                        {
                            try
                            {
                                if (indlicdetail.Length > 4)
                                {
                                    dt1 = System.DateTime.Parse(indlicdetail);
                                    isdate = true;
                                    break;
                                }
                                
                            }
                            catch (Exception ex)
                            {
                                isdate = false;
                                System.Console.WriteLine("Error" + ex.Message);
                            }
                        }
                        if (isdate)
                        {
                            dr["License Expiry Date"] = dt1.ToString("dd-MMM-yyyy");
                        }
                        else
                        {
                            dr["License Expiry Date"] = "NA";
                        }
                        break;
                    }
                }
                textfilereader.Close();

                #endregion
                if (featurefound)
                {
                    dr["WellFlo License Present (Y/N)"] = "Y";
                }
                else
                {
                    dr["WellFlo License Present (Y/N)"] = "N";
                    dr["License Expiry Date"] = "NA";
                }
                isdate = false;
                dt.Rows.Add(dr);
                featurefound = false;
                count++;
               
            }
            
            dataGridView1.Visible = true;
            dataGridView1.DataSource = dt;
            formatDataGridView();

        }


        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            //If this code is commented out the program will work just fine 
            //by just clicking the button

            //This was added to prevent formatDataGridView from executing more
            //than once.  Even though I unreg and rereg the event handler, the 
            //method was still being called 3 - 4 times. This successfully
            //prevented that but only the *'s were removed and no red back color
            //added to the cells.
            if (!isFormatted)
            {
                formatDataGridView();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.Visible = false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void callformat(DataGridViewCellFormattingEventArgs formatting)
        {
            try
            {
                formatting.FormattingApplied = true;
            }
            catch (FormatException)
            {
                formatting.FormattingApplied = false;
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            foreach (DataGridViewRow Myrow in dataGridView1.Rows)
            {            //Here 2 cell is target value and 1 cell is Volume
                if (Myrow.Cells[2].Value.ToString() == "Y")// Or your condition 
                {
                    Myrow.DefaultCellStyle.BackColor = Color.Green;
                }
                else
                {
                    Myrow.DefaultCellStyle.BackColor = Color.Red;
                }
            }
        }

        private void formatDataGridView()
        {
            dataGridView1.DataBindingComplete -= dataGridView1_DataBindingComplete;
            foreach (DataGridViewRow Myrow in dataGridView1.Rows)
            {
                if (Myrow.Cells[2].Value != null)
                {
                    if (Myrow.Cells[2].Value.ToString() == "Y")// Or your condition 
                    {
                        Myrow.DefaultCellStyle.BackColor = Color.Green;
                    }
                    else
                    {
                        Myrow.DefaultCellStyle.BackColor = Color.Red;
                    }
                }
            }
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
            isFormatted = true;
        }
    }
}
