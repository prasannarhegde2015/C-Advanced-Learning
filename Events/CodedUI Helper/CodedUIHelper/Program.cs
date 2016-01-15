using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.Data.Odbc;
using Microsoft.VisualStudio.TestTools.UITest;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;

namespace CodedUI_Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            Playback.Initialize();
            if (args.Length == 0)
            {

                Console.WriteLine("Usage : exename  arg1  arg2");
                return;
            }

            switch (args[0].ToLower())
            {
                case "tabselect":
                    {
                        SelectWinTab(args[1], args[2]);
                        break;
                    }

                case "winbuttonclick":
                    {
                        ClickWinButton(args[1], args[2]);
                        break;
                    }

                case "inputformdata":
                    {

                       // AddData(args[1], args[2]);
                       


                        break;
                    }
            }

            Playback.Cleanup();
        }


        public static void SelectWinTab(string wintitle ,string tabname)
        {
            WinWindow wnd = new WinWindow();
            wnd.SearchProperties.Add(WinWindow.PropertyNames.Name, wintitle, PropertyExpressionOperator.Contains);
            wnd.SearchProperties.Add(WinWindow.PropertyNames.ClassName, "WindowsForms10.Window", PropertyExpressionOperator.Contains);
            wnd.SetFocus();
            wnd.Maximized = true;
            WinTabPage wtbpg = new WinTabPage(wnd);
            wtbpg.SearchProperties.Add(WinTabPage.PropertyNames.Name, tabname, PropertyExpressionOperator.Contains);
            Mouse.Click(wtbpg);
        }

        public static void ClickWinButton(string wintitle, string tabname)
        {
            WinWindow wnd2 = new WinWindow();
            wnd2.SearchProperties.Add(WinWindow.PropertyNames.Name, wintitle, PropertyExpressionOperator.Contains);
            wnd2.SearchProperties.Add(WinWindow.PropertyNames.ClassName, "WindowsForms10.Window", PropertyExpressionOperator.Contains);
            wnd2.SetFocus();
            wnd2.Maximized = true;
            Console.WriteLine("got focus of window"+ wnd2.Name);
            Console.WriteLine("Button Name " + tabname);
            WinButton wbtn = new WinButton(wnd2);
            UITestControlCollection allbtns = wbtn.FindMatchingControls();
            Console.WriteLine("buttons count insiode widnows was " + allbtns.Count);
            foreach (UITestControl indbutton in allbtns)
            {
                Console.WriteLine("Found button " + indbutton.Name);
            }
            wbtn.SearchProperties.Add(WinButton.PropertyNames.Name, tabname, PropertyExpressionOperator.Contains);
            Mouse.Click(wbtn);
        }

        public static void AddData(string filename, string testcase)
        {

        }

        public static void enterkeyboard(string value)
        {
            System.Windows.Forms.SendKeys.SendWait("{Home}");
            System.Windows.Forms.SendKeys.SendWait("+{End}");
            System.Windows.Forms.SendKeys.SendWait("{Del}");
            System.Windows.Forms.SendKeys.SendWait(value);
        }

        public static void hardcoded()
        {
            WinWindow Lwindow = new WinWindow();
            Lwindow.SearchProperties.Add(WinWindow.PropertyNames.Name, "LOWIS:", PropertyExpressionOperator.Contains);
            Lwindow.SearchProperties.Add(WinWindow.PropertyNames.ClassName, "WindowsForms10.Window", PropertyExpressionOperator.Contains);

            UITestControl eliftclient = new UITestControl(Lwindow);
            eliftclient.TechnologyName = "MSAA";
            eliftclient.SearchProperties.Add("ControlType", "Client");
            eliftclient.SearchProperties.Add("ClassName", "Internet Explorer_Server");

            UITestControl webdocument = new UITestControl(eliftclient);
            webdocument.TechnologyName = "Web";
            webdocument.SearchProperties.Add("ControlType", "Document");
            webdocument.SearchProperties.Add("Id", "params_Downhole");

            UITestControl webedit = new UITestControl(webdocument);
            webedit.TechnologyName = "Web";
            webedit.SearchProperties.Add("ControlType", "Edit");
            webedit.SearchProperties.Add("Id", "txtPerfTop");

            webedit.SetFocus();
            Mouse.Click(webedit);
            enterkeyboard("5000");
        }

        public static DataTable getDataTable(string filename, string sheetname)
        {
            DataTable dt = new DataTable();
            return dt;
        }
    }
}
