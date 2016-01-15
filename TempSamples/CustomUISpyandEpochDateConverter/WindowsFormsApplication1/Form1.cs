using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation;
using System.Globalization;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public DataTable dt = new DataTable();


        public Form1()
        {
            InitializeComponent();
            panel1.Hide();
            panel2.Hide();
        }

        private void AddControlDetails_Click(object sender, EventArgs e)
        {
            
            dt.Columns.Add("InputData");
            dt.Columns.Add("Section");
            dt.Columns.Add("ParentNumber");
            dt.Columns.Add("LocalisedName");
            dt.Columns.Add("ParentType");
            dt.Columns.Add("ParentSearchBy");
            dt.Columns.Add("ParentSearchValue");
            dt.Columns.Add("ControlType");
            dt.Columns.Add("FieldName");
            dt.Columns.Add("Index");
            dt.Columns.Add("SearchBy");
            dt.Columns.Add("ControlName");
            MessageBox.Show("Press Shift key on control to get details");
          //  dataGridView1.DataSource = dt;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            this.Focus();
            #region keyboardeventfire
            if (keyData == (Keys.ShiftKey | Keys.Shift))
            {
                
                

                System.Windows.Point point = new System.Windows.Point(MousePosition.X, MousePosition.Y);
                AutomationElement element = AutomationElement.FromPoint(point);
                System.Windows.Rect boundingRect1 = (System.Windows.Rect)element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty); ;
                object boundingRectNoDefault = element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty, true);
                if (boundingRectNoDefault == AutomationElement.NotSupported)
                {
                    // TODO Handle the case where you do not wish to proceed using the default value.
                }
                else
                {
                    boundingRect1 = (System.Windows.Rect)boundingRectNoDefault;

                }
                System.Windows.Point points = boundingRect1.TopLeft;
                System.Drawing.Point topleft = new System.Drawing.Point(Convert.ToInt32(points.X), Convert.ToInt32(points.Y));
                System.Windows.Size size = boundingRect1.Size;
                System.Drawing.Size windowSize = new System.Drawing.Size(Convert.ToInt32(size.Width), Convert.ToInt32(size.Height));
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(topleft, windowSize);
                ControlPaint.DrawReversibleFrame(rect, SystemColors.Highlight, FrameStyle.Thick);
                TreeWalker walker = TreeWalker.ControlViewWalker;
                string searchby = "";
                string controlName = "";
                string autoIdString;
                object autoIdNoDefault = element.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty, true);
                object nameDefault = element.GetCurrentPropertyValue(AutomationElement.NameProperty, true);
                object controlTypeDefault = element.GetCurrentPropertyValue(AutomationElement.LocalizedControlTypeProperty, true);
                AutomationElement parent = walker.GetParent(element);
                AutomationElement currentParent = null;

                if (nameDefault.ToString().Length > 0)
                {
                    searchby = "name";
                    controlName = nameDefault.ToString();
                }

                else if (autoIdNoDefault.ToString().Length > 0)
                {
                    searchby = "autoid";
                    controlName = autoIdNoDefault.ToString();
                }
                string parenttype = "";
                string parentsearchby = "";
                string parentsearchvalue = "";
                string parentName = parent.GetCurrentPropertyValue(AutomationElement.NameProperty, true) as string;
                int cntr = 1;
                #region loopforcontrol
                do
                {
                    DataRow dr = dt.NewRow();
                    if (parent == AutomationElement.RootElement)
                    {
                        break;
                    }
                    else
                    {
                        currentParent = parent;
                        object curParentautoIdNoDefault = currentParent.GetCurrentPropertyValue(AutomationElement.AutomationIdProperty, true);
                        object curParentnameDefault = currentParent.GetCurrentPropertyValue(AutomationElement.NameProperty, true);
                        object curParentcontrolTypeDefault = currentParent.GetCurrentPropertyValue(AutomationElement.ControlTypeProperty, true);
                        object localised = currentParent.GetCurrentPropertyValue(AutomationElement.LocalizedControlTypeProperty, true);
                        ControlType ct = (ControlType)curParentcontrolTypeDefault;
                       // ct.ProgrammaticName
                        parenttype = ct.ProgrammaticName;

                        if (curParentnameDefault == AutomationElement.NotSupported)
                        {
                  
                            parentsearchvalue = "";
                        }
                        else
                        {
                            
                            parentsearchvalue = curParentnameDefault as string;
                            parentsearchby = "Name";
                        }

                        if (parentsearchvalue.Length == 0) // go for auto id 
                        {
                            if (curParentautoIdNoDefault == AutomationElement.NotSupported)
                            {
                                // 
                                parentsearchvalue = "";
                            }
                            else
                            {
                                parentsearchby = "AutomationID";
                                parentsearchvalue = curParentautoIdNoDefault as string;
                            }

                        }
                        else
                        {

                        }

                      //  dr["Section"] = "";
                        dr["LocalisedName"] = localised.ToString();
                        dr["ParentNumber"] = cntr;
                        dr["ParentType"] = parenttype;
                        dr["ParentSearchBy"] = parentsearchby;
                        dr["ParentSearchValue"] = parentsearchvalue;
                        dr["ControlType"] = controlTypeDefault as string;
                        dr["FieldName"] = "";
                        dr["Index"] = "";
                        dr["SearchBy"] = searchby;
                        dr["ControlName"] = controlName;
                        
                    }
                    cntr++;
                    parent = walker.GetParent(parent);
                    dt.Rows.Add(dr);
                } while (true);
                #endregion
                //  MessageBox.Show("Object detailscolected", "UI Spy details");
                dataGridView1.DataSource = dt;
                return true;
            }
            #endregion
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dtinput = textBox2.Text;
            if (dtinput.Length == 0)
            {
                dtinput = DateTime.Now.ToString();
                textBox2.Text = dtinput;
            }
            var epoch = ( Convert.ToDateTime(dtinput) - new DateTime(1970, 1, 1)).TotalSeconds;
            textBox1.Text = epoch.ToString();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var epoch = textBox1.Text;
            //TimeSpan  epc1 = new TimeSpan(1970, 1, 1);
            //double dtnowinsec = epc1.TotalSeconds + Convert.ToDouble(epc1);
            if (epoch.Length > 0)
            {
                textBox2.Text = epoch2string(Convert.ToInt32(epoch));
            }
            else
            {
                MessageBox.Show("Please Enter Date in Epoch Format", "Enter Date", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            
        }

        private string epoch2string(int epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local).AddSeconds(epoch).ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            dt.Columns.Remove("InputData");
            dt.Columns.Remove("Section");
            dt.Columns.Remove("ParentNumber");
            dt.Columns.Remove("LocalisedName");
            dt.Columns.Remove("ParentType");
            dt.Columns.Remove("ParentSearchBy");
            dt.Columns.Remove("ParentSearchValue");
            dt.Columns.Remove("ControlType");
            dt.Columns.Remove("FieldName");
            dt.Columns.Remove("Index");
            dt.Columns.Remove("SearchBy");
            dt.Columns.Remove("ControlName");
            dt.Clear();
            dt.Dispose();
            dataGridView1.DataSource = "";

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Show();
            panel2.Hide();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Show();
            panel1.Hide();
        }
    }
}

