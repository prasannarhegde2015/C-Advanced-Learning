using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Automation;

namespace GetTextfrmoEventdir_cards
{
    class Program
    {
        static void Main(string[] args)
        {
            DataTable dt = new DataTable();
            
            string windowname = "LOWIS:";
            string gridname = "EventDirectoryCards";
            AutomationElement root = AutomationElement.RootElement;
            Condition condWindow =
               new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
            AutomationElement uiWidnow = null; 
            AutomationElementCollection allwindows  = root.FindAll(TreeScope.Descendants, condWindow);
            for (int i = 0; i < allwindows.Count; i++)
            {
                if (allwindows[i].Current.Name.Contains(windowname))
                {
                    uiWidnow = allwindows[i];
                    break;
                }
            }

            AutomationElement gridControl = uiWidnow.FindFirst(TreeScope.Descendants,
                new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty,ControlType.DataGrid),
                    new PropertyCondition(AutomationElement.NameProperty,gridname)));


            if (gridControl == null)
            {
                Console.WriteLine("Grid was not found");

            }
            else
            {
                Console.WriteLine("Grid was found");
                
            }
         //   Console.ReadLine();

            AutomationElementCollection headers = gridControl.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.HeaderItem));
            int colcount = headers.Count;

            for (int k=0; k< colcount; k++)
            {
                dt.Columns.Add("Column" + k);
            }
            int cnt =0;
            DataRow dr = dt.NewRow();
            foreach (AutomationElement header in headers)
            {

                dr["Column" + cnt] = header.Current.Name;
                cnt++;
            }
            dt.Rows.Add(dr);
            AutomationElementCollection datarows = gridControl.FindAll(TreeScope.Descendants,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.DataItem));

            int drcnt = 1;

            foreach (AutomationElement datarow in datarows)
            {
                dr = dt.NewRow();
                AutomationElementCollection cells = datarow.FindAll(TreeScope.Children,
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Text));
               int cntcl = 0;
                foreach (AutomationElement cell in cells)
                {
                    dr["Column" + cntcl] = cell.Current.Name;
                    cntcl++;
                }
                drcnt++;
                dt.Rows.Add(dr);
            }

            LogtoFileCSV(dt);
        }

        static void LogtoFileCSV(DataTable dtin)
        {
            char  delm = '\u0022';
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dtin.Rows.Count; i++ )
            {
                for (int kk = 0; kk < dtin.Columns.Count; kk++)
                {
                    sb.Append(delm + dtin.Rows[i][kk].ToString() + delm + ",");
                }
                sb.Append(Environment.NewLine);
            }

            System.IO.File.AppendAllText(@"c:\op.csv",sb.ToString());
        }
    }
}
