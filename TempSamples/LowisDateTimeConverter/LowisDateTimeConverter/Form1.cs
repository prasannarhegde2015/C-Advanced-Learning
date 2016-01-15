using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LowisDateTimeConverter
{
    public partial class Form1 : Form
    {
      //  Form1 frm = new Form1();
        public Form1()
        {
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            InitializeComponent();
            label5.Hide();
            label6.Hide();
            label7.Hide();
            label8.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dtinput = textBox1.Text;
            if (dtinput.Length == 0)
            {
                MessageBox.Show("No Date Time was entered , current Date Time has been input", "Input date", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dtinput = DateTime.Now.ToString();
                textBox1.Text = dtinput;
            }
            try
            {
                DateTime dtinp = DateTime.Parse(dtinput);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Incorrect Date Time Format: Expected Format 'dd-MMM-yyyy hh:mm:ss' "+Environment.NewLine+"for e.g  10-Jan-2015 12:34:34", "Incorrect DateTime Format", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            var epoch = (Convert.ToDateTime(dtinput).ToUniversalTime() - new DateTime(1970, 1, 1,0,0,0)).TotalSeconds;
            textBox2.Text = epoch.ToString();
            label5.ForeColor = Color.Blue;
            label6.ForeColor = Color.Blue;
            label5.Show();
            label6.Show();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var epoch = textBox3.Text;
            //TimeSpan  epc1 = new TimeSpan(1970, 1, 1);
            //double dtnowinsec = epc1.TotalSeconds + Convert.ToDouble(epc1);
            if (epoch.Length > 0)
            {
                textBox4.Text = epoch2string(Convert.ToInt32(epoch));
            }
            else
            {
                MessageBox.Show("Please Enter Date in Epoch Format", "Enter Date", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            label7.ForeColor = Color.Blue;
            label8.ForeColor = Color.Blue;
            label7.Show();
            label8.Show();
        }

        private string epoch2string(int epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch).ToLocalTime().ToString();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
    }
}
