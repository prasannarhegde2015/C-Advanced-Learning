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
using System.IO;

namespace UpdateRTUEMuXMLFiles
{
    public partial class Form1 : Form
    {
        #region FormLoad
        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            textBox1.Visible = false;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox1.SelectedItem = "Register";
            comboBox2.SelectedItem = "Address";
            comboBox3.SelectedItem = "Value";
            label1.Visible = false;
            label5.Visible = false;
            checkBox2.Visible = false;
            textBox4.Visible = false;
            textBox5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;
        }
        #endregion

        #region PublicProperties
        private string _fname;
        public string fname
        {
            get { return _fname; }
            set { _fname = value; }
        }
        

        private string _randnum;
        public string randnum
    {
        get { return _randnum; }
        set { _randnum = value; }
    }
        #endregion

        #region FileUploadButton
        private void button1_Click(object sender, EventArgs e)
        {
            // Browse button
            openFileDialog1.Title = "Please Select the XML file to update";
            openFileDialog1.Filter = "XML Files (*.xml)|*.xml";
            openFileDialog1.FileName = "";
            openFileDialog1.ShowDialog();
            string flname = openFileDialog1.FileName;
            label5.Font = new Font(label1.Font, FontStyle.Bold);
            label5.Visible = true;
            label5.ForeColor = Color.Blue;
            label5.BackColor = Color.White;
            label5.Text = "File Opened: "+flname;
            this.fname = flname;
        }
        #endregion

        #region UpdateButton
        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label3.Visible = false;
                comboBox3.Visible = false;
                textBox2.Visible = false;
                checkBox2.Visible = true;
                UpdateSingleAttribute();
            }
            else
            {
                label3.Visible = true;
                comboBox3.Visible = true;
                textBox2.Visible = true;
                checkBox2.Visible = false;
                textBox4.Visible = false;
                textBox5.Visible = false;
                UpdateAttributeClause();
            }
             
        }
        #endregion


        #region UpdateusingCluase
        private void UpdateAttributeClause()
        {
            string newValue = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();
            bool updatedone = false;
            if (fname != null && fname.Length > 0)
            {
                if (File.Exists(fname) == false)
                {
                    MessageBox.Show("File Does not exist on this machine", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    resetlable();
                    return;
                }
                xmlDoc.Load(this.fname);
                XmlNodeList xnList = null;
                string attributesvalues = "";
                xnList = xmlDoc.SelectNodes("//" + comboBox1.SelectedItem.ToString());
                //  xnList = xmlDoc.GetElementsByTagName(comboBox1.SelectedItem.ToString());
                if (xnList.Count == 0)
                {
                    MessageBox.Show("No matching tagnames found ", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    resetlable();
                    return;
                }
                string attrname = "";

                #region EachSingleNODE
                foreach (XmlNode xn in xnList)
                {

                    if (comboBox2.SelectedItem.ToString() == "Other(Please Specify)")
                    {
                        textBox1.Visible = true;
                        if (textBox1.Text.Length > 0)
                        {
                            attrname = textBox1.Text;
                        }
                        else
                        {
                            MessageBox.Show("Enter Attribute Name to Look for", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                    }
                    else
                    {
                        attrname = comboBox2.SelectedItem.ToString();
                    }
                    if (isatrributePresent(xn, attrname) == false)
                    {
                        MessageBox.Show("No matching Attributes found for TagName: " + comboBox1.SelectedItem.ToString() + "Attribute: " + attrname, "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetlable();
                        return;
                    }
                    attributesvalues = xn.Attributes[attrname].InnerText;

                    if ((textBox3.Text.Length > 0) == false)
                    {
                        MessageBox.Show("Please Specify Attribute Value " + attrname, "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetlable();
                        return;
                    }
                    if ((textBox2.Text.Length > 0) == false)
                    {
                        MessageBox.Show("Please Specify Value to update " + attrname, "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetlable();
                        return;
                    }

                    if (attributesvalues == textBox3.Text)
                    {
                        updatedone = true;
                        if (isatrributePresent(xn,comboBox3.SelectedItem.ToString()))
                        {
                        xn.Attributes[comboBox3.SelectedItem.ToString()].Value = textBox2.Text;
                        }
                        else
                        {
                            MessageBox.Show("No Matching Attribute  "+comboBox3.SelectedItem.ToString()+ " Found" , "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        break;
                    }
                }
                #endregion

                string msgsuccess = "Updated the Node With Tag Name: " + comboBox1.SelectedItem.ToString() + " whose Attrubute: " + attrname + "== " + attributesvalues + "  with value = " + textBox2.Text;
                xmlDoc.Save(this.fname);

                if (updatedone)
                {
                    label1.Font = new Font(label1.Font, FontStyle.Bold);
                    label1.Text = msgsuccess;
                    label1.Visible = true;
                    label1.ForeColor = Color.Green;
                    label1.BackColor = Color.White;
                }
                else
                {
                    label1.Font = new Font(label1.Font, FontStyle.Bold);
                    label1.Text = "No values were updated as No match was found for Attribute's Value";
                    label1.Visible = true;
                    label1.ForeColor = Color.OrangeRed;
                    label1.BackColor = Color.White;
                }

            }
            else
            {
                MessageBox.Show("Please Enter Valid Xml File Path", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region updatesingleAttibuteValue
        private void UpdateSingleAttribute()
        {
            string newValue = string.Empty;
            Random rand = new Random();
            XmlDocument xmlDoc = new XmlDocument();
            bool updatedone = false;
            int minlimit = -1, maxlimit = -1;
            if (fname != null && fname.Length > 0)
            {
                if (File.Exists(fname) == false)
                {
                    MessageBox.Show("File Does not exist on this machine", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                xmlDoc.Load(this.fname);
                XmlNodeList xnList = null;
                xnList = xmlDoc.SelectNodes("//" + comboBox1.SelectedItem.ToString());
                //  xnList = xmlDoc.GetElementsByTagName(comboBox1.SelectedItem.ToString());
                if (xnList.Count == 0)
                {
                    MessageBox.Show("No matching tagnames found ", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                string attrname = "";
                if (checkBox2.Checked)
                {
                    if (textBox4.Text.Length > 0 && textBox4.Text.Length > 0)
                    {
                         minlimit = Int32.Parse(textBox4.Text);
                         maxlimit = Int32.Parse(textBox5.Text);
                        if (maxlimit > minlimit)
                        {
                           // int randomnumber = rand.Next(minlimit, maxlimit);
                           // textBox3.Text = randomnumber.ToString();
                        }
                        else
                        {
                            MessageBox.Show("Maximum should be Greater than Minimum ", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter Max and min for Random values: ", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                #region eachnode2
                foreach (XmlNode xn in xnList)
                {
                    if (comboBox2.SelectedItem.ToString() == "Other(Please Specify)")
                    {
                        textBox1.Visible = true;
                        if (textBox1.Text.Length > 0)
                        {
                            attrname = textBox1.Text;
                        }
                        else
                        {
                            MessageBox.Show("Enter Attribute Name to Look for", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                    }
                    else
                    {
                        attrname = comboBox2.SelectedItem.ToString();
                    }
                    if (isatrributePresent(xn, attrname) == false)
                    {
                        MessageBox.Show("No matching Attributes found for TagName: " + comboBox1.SelectedItem.ToString() + "Attribute: " + attrname, "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetlable();

                        return;
                    }
                    if (checkBox2.Checked == false)
                    {
                        if ((textBox3.Text.Length > 0) == false)
                        {


                            MessageBox.Show("Please Specify Attribute Value " + attrname, "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            resetlable();
                            return;

                        }
                    }

                    try
                    {
                        if (checkBox2.Checked)
                        {
                            
                        int randomnumber = rand.Next(minlimit, maxlimit);
                        textBox3.Text = randomnumber.ToString();
                                
                        }
                       xn.Attributes[attrname].InnerText = textBox3.Text;
                       updatedone = true;
                    }
                    catch
                    {
                        updatedone = false;
                        break;
                    }
                }
                #endregion

                string msgsuccess = "Updated the Node With Tag Name: " + comboBox1.SelectedItem.ToString() + " whose Attrubute: " + attrname + "  with value = " + textBox3.Text;
                xmlDoc.Save(this.fname);

                if (updatedone)
                {
                    label1.Font = new Font(label1.Font, FontStyle.Bold);
                    label1.Text = msgsuccess;
                    label1.Visible = true;
                    label1.ForeColor = Color.Green;
                    label1.BackColor = Color.White;
                }
                else
                {
                    label1.Font = new Font(label1.Font, FontStyle.Bold);
                    label1.Text = "No values were updated as No match was found for Attributes Value";
                    label1.Visible = true;
                    label1.ForeColor = Color.OrangeRed;
                    label1.BackColor = Color.White;
                }

            }
            else
            {
                MessageBox.Show("Please Enter Valid Xml File Path", "XML updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        #endregion


        private bool isatrributePresent(XmlNode node, string attribute)
        {
            bool isp = false;
            StringBuilder sb = new StringBuilder();
            if (node.Attributes.Count > 0)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    sb.Append(att.Name); ;
                    sb.Append(";");
                }

                if ( sb.ToString().Contains(attribute))
                {
                    isp = true;
                }
                else
                {
                    isp =false;
                }
            }

            return isp;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem.ToString() == "Other(Please Specify)")
            {
                textBox1.Visible = true;
            }
            else
            {
                textBox1.Visible = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                label3.Visible = false;
                comboBox3.Visible = false;
                textBox2.Visible = false;
                checkBox2.Visible = true;
                if (checkBox2.Checked)
                {
                    checkBox2.Checked = false;
                }

            }
            else
            {
                label3.Visible = true;
                comboBox3.Visible = true;
                textBox2.Visible = true;
                checkBox2.Visible = false;
                textBox4.Visible = false;
                textBox5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                textBox3.ReadOnly = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            
            if (checkBox2.Checked)
            {
                textBox4.Visible = true;
                textBox5.Visible = true;
                label6.Visible = true;
                label7.Visible = true;
                textBox3.ReadOnly = true;

              
            }
            else
            {
                textBox4.Visible = false;
                textBox5.Visible = false;
                label6.Visible = false;
                label7.Visible = false;
                textBox3.ReadOnly = false;
            }
        }

        private void resetlable()
        {
            label1.Text = "";
            label1.Visible = false;
        }
    }
}
