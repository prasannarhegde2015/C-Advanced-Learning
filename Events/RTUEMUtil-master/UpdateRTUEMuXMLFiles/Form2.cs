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

namespace UpdateRTUEMuXMLFiles
{
    public partial class frmaddreg : Form
    {
        private string _holder;
        public string holder
        {
            set { _holder = value; }
            get { return _holder; }
        }

        public frmaddreg(string fname)
        {
            InitializeComponent();
            cmbrandomize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbrandomize.DropDownStyle = ComboBoxStyle.DropDownList;
            holder = fname;

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            addregsiternode(holder, txtdesc.Text, txtadd.Text, cmbType.SelectedItem.ToString(), txtval.Text, cmbrandomize.SelectedItem.ToString(), txtmin.Text, txtmax.Text);
            this.Close();
        }

        #region AddRegisterNode
        private void addregsiternode(string fname,string regdescription, string regaddress,string rtype , string val,string ranval,string min,string max)
        {
            string newValue = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();
            
            xmlDoc.Load(fname);
          //  XmlNodeList rootNodes = xmlDoc.SelectNodes("RegisterData");
          //  XmlNode rootNode = rootNodes[0];
            XmlNode root = xmlDoc.DocumentElement;
            XmlNode registerNode = xmlDoc.CreateElement("Register");
            XmlAttribute descattribute = xmlDoc.CreateAttribute("Description");
            descattribute.Value = regdescription;
            registerNode.Attributes.Append(descattribute);
            XmlAttribute addrattribute = xmlDoc.CreateAttribute("Address");
            addrattribute.Value = regaddress;
            registerNode.Attributes.Append(addrattribute);
            XmlAttribute dtypeattribute = xmlDoc.CreateAttribute("Type");
            dtypeattribute.Value = rtype;
            registerNode.Attributes.Append(dtypeattribute);
            XmlAttribute valattibute = xmlDoc.CreateAttribute("Value");
            valattibute.Value = val;
            registerNode.Attributes.Append(valattibute);
            if (ranval.ToLower() == "yes")
            {
                XmlAttribute mincattribute = xmlDoc.CreateAttribute("Min");
                mincattribute.Value = min;
                registerNode.Attributes.Append(mincattribute);
                XmlAttribute maxcattribute = xmlDoc.CreateAttribute("Max");
                maxcattribute.Value = max;
                registerNode.Attributes.Append(maxcattribute);
                XmlAttribute randzcattribute = xmlDoc.CreateAttribute("Randomize");
                randzcattribute.Value = ranval;
                registerNode.Attributes.Append(randzcattribute);
            }
            root.AppendChild(registerNode);
            xmlDoc.Save(fname);



            
        }
        #endregion
    }
}
