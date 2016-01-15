using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace GetMySQLResusltsSquash
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label2.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlConnection conn = new MySqlConnection();
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["mysqlconn"].ToString();
                string strmycomd = "Select TCLN_ID from squash.test_case " +
                                    "Where squash.test_case.TCLN_ID   IN (" +
                                    "SELECT squash.test_case_library_node.TCLN_ID FROM squash.test_case_library_node " +
                                    "Where PROJECT_ID " +
                                    " In (Select squash.project.Project_ID from squash.project where squash.project.NAME='" + textBox1.Text + "')" +
                                    " AND TCLN_ID not in (Select TCLN_ID from squash.test_case_folder))" +
                                    " AND squash.test_case.tc_status = 'approved';";
                MySqlCommand mycmd = new MySqlCommand(strmycomd, conn);
                try
                {
                    conn.Open();
                    label2.Visible = true;
                    label2.Text = "Connection Success";
                }
                catch (MySqlException ex)
                {
                    label2.Visible = true;
                    label2.Text = "Connection Failed: " + ex.Message;
                }
                MySqlDataAdapter da = new MySqlDataAdapter(mycmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                textBox2.Text = dt.Rows.Count.ToString();
            }
            catch (MySqlException e2)
            {
                label2.Visible = true;
                label2.Text = "Erorr: " + e2.Message;
            }
            
        }
    }
}
