using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Configuration;
using ControlType = System.Windows.Automation.ControlType;
using PropertyCondition = System.Windows.Automation.PropertyCondition;
using Microsoft.VisualStudio.TestTools.UITest;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
using Microsoft.VisualStudio.TestTools.UITesting.HtmlControls;
namespace LowisUMAutomation
{
    class Program
    {
        static void Main(string[] args)
        {
            Playback.Initialize();
            LogtoFile("Started logging ");

            try
            {
                //  ClickWindowButton("System Inventory", "Load Update");
                //   ClickToolBar("System Inventory", "View");
                string strstartfrom = ConfigurationManager.AppSettings["startfrom"];
                LogtoFile("got start from  ");
                string spath = ConfigurationManager.AppSettings["hotfixpath"];
                LogtoFile("got hotfixpath ");
                string hfversion = ConfigurationManager.AppSettings["HFVersionString"];
                LogtoFile("got HFVersionString ");
                int intStartHFnum = Int32.Parse(strstartfrom);
                LogtoFile("Parsed sTartfrom");
				LogtoFile("Start from Hotfix"+strstartfrom);
				LogtoFile("HF path i"+spath);
				LogtoFile("HF version  s"+hfversion);
				LogtoFile("int valie is "+intStartHFnum );
                //  string spath = @"E:\Hotfixes 7.0.1";

                DirectoryInfo dinfo = new DirectoryInfo(spath);

                List<string> IndHFs = (from d in dinfo.GetDirectories()
                                       orderby d.FullName
                                       select d.FullName).ToList();

                foreach (string HFF in IndHFs)
                {
                    if (HFF.Contains("HF") )
                    {
                        LogtoFile("Inside If");
                        string HF = HFF.Substring(HFF.LastIndexOf("\\") + 1, HFF.Length - HFF.LastIndexOf("\\") - 1);
                        LogtoFile("HF nu " + HF);
                        if (HF.Contains("HF"))
                        {
                            string hfnum = HF.Replace(hfversion, "").Trim();
                            LogtoFile("Thi is" + HF);
                            string inthfnum = hfnum.Replace("HF", "").Trim();
                            LogtoFile("fullint is" + inthfnum);
                            int fullhfnum = Int32.Parse(inthfnum);
                            if (fullhfnum > intStartHFnum)
                            {
                                LogtoFile("Inside Maniset");
                                DirectoryInfo dr2 = new DirectoryInfo(HFF);
                                var flname = (from f in dr2.GetFiles()
                                              orderby f.Name
                                              where f.Extension == ".manifest"
                                              select f.FullName).First();
                                //   Console.WriteLine("Full manisfext file path is " + flname);
                                LogtoFile("Started execution");
                                ClickWindowButton("Load Update Wizard", "Load Update");
                                EnterDatainTextbox("Open Update Manifest File", "File name:", flname);
                                ClickWindowButton("Open Update Manifest File", "Open");
                                //Conditional OK and Yes
                                OptionalClickWindowButton("Load Update Wizard", "&OK");
                                OptionalClickWindowButton("Load Update Wizard", "Yes");
                                OptionalClickWindowButton("Load Update Wizard", "Yes");
                                ClickWindowButton("Load Update Wizard", "Process Update");
                                OptionalClickWindowButton("Load Update Wizard", "&OK");
                                OptionalClickWindowButton("Load Update Wizard", "Yes");
                                OptionalClickWindowButton("Load Update Wizard", "Yes");
                                //Conditional OK and Yes
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LogtoFile("General Error is: " + ex.Message);
            }
        
            Playback.Cleanup();
        }
        static void UIAmethods()
        {
            //AutomationElement rootelem = AutomationElement.RootElement;
            //AutomationElement LUMWindow = null;
            //AutomationElementCollection LUMWindows = rootelem.FindAll(TreeScope.Descendants,
            //    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

            //foreach (AutomationElement inwindow in LUMWindows)
            //{
            //    if (inwindow.Current.Name.Contains("System Inventory"))
            //    {
            //        LUMWindow = inwindow;
            //        break;
            //    }
            //}
            //LUMWindow.SetFocus();
            //WindowPattern winpatn = (WindowPattern)LUMWindow.GetCurrentPattern(WindowPattern.Pattern);
            //winpatn.SetWindowVisualState(WindowVisualState.Maximized);

            //AutomationElement btnLoadUpdate = LUMWindow.FindFirst(TreeScope.Descendants,
            //    new AndCondition(
            //        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
            //        new PropertyCondition(AutomationElement.NameProperty, "Load Update")
            //        ));

            //InvokePattern invk = (InvokePattern)btnLoadUpdate.GetCurrentPattern(InvokePattern.Pattern);
            //invk.Invoke();

            //if (btnLoadUpdate != null)
            //{
            //    Console.WriteLine("Button was found ");
            //    ClickControl(btnLoadUpdate);
            //}
        }
        static void ClickControl(AutomationElement ae)
        {
            //AutoItX3Lib.AutoItX3 at = new AutoItX3Lib.AutoItX3();
            //System.Windows.Point p = ae.GetClickablePoint();
            //at.MouseClick("LEFT", Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
           
        }
        public static void ClickWindowButton(string windowtitle, string buttontext)
        {
            WinWindow addposn = new WinWindow();
            addposn.SearchProperties.Add(WinWindow.PropertyNames.Name, windowtitle, PropertyExpressionOperator.Contains);
            WinButton addposbtn = new WinButton(addposn);
            addposbtn.SearchProperties.Add(WinButton.PropertyNames.Name, buttontext);
            Mouse.Click(addposbtn);
        }
        public static void OptionalClickWindowButton(string windowtitle, string buttontext)
        {
            WinWindow addposn = new WinWindow();
            addposn.SearchProperties.Add(WinWindow.PropertyNames.Name, windowtitle, PropertyExpressionOperator.Contains);
            LogtoFile("Searcing for Dialog Object ");
            string opwait = ConfigurationManager.AppSettings["optionalwait"];
            int dwait = Int32.Parse(opwait);
            UITestControl windlg = new UITestControl(addposn);
            //for (int i = 0; i < 6; i++)
            //{
                Playback.Wait(dwait);
                LogtoFile("Attempt....... #");
                windlg.TechnologyName = "MSAA";
                windlg.SearchProperties.Add("ControlType", "Dialog");
                windlg.SearchProperties.Add("Name", "Load Update Wizard");
                Playback.Wait(1000);
                //if (windlg.Exists)
                //{
                //    break;
                //}
                
            //}
            
            if (windlg.Exists)
            {

                WinButton addposbtn = new WinButton(windlg);
                addposbtn.SearchProperties.Add(WinButton.PropertyNames.Name, buttontext);
                if (addposbtn.Exists)
                {
                    Mouse.Click(addposbtn);
                }
            }
            else
            {
                LogtoFile("Did not find the Dialog Object dialog  in Max number of Attemps for button Text "+buttontext);
            }
        }
        public static void OptionalClickWindowButton2(string windowtitle, string buttontext)
        {
            WinWindow addposn = new WinWindow();
            addposn.SearchProperties.Add(WinWindow.PropertyNames.Name, windowtitle, PropertyExpressionOperator.Contains);
            LogtoFile("Searcing for Dialog Object ");
            string opwait = ConfigurationManager.AppSettings["optionalwait"];
            int dwait = Int32.Parse(opwait);
            UITestControl windlg = new UITestControl(addposn);
            
            //for (int i = 0; i < 6; i++)
            //{
            Playback.Wait(dwait);
            LogtoFile("Attempt....... #");
            windlg.TechnologyName = "MSAA";
            windlg.SearchProperties.Add("ControlType", "Dialog");
            windlg.SearchProperties.Add("Name", "Load Update Wizard");
            Playback.Wait(1000);
            //if (windlg.Exists)
            //{
            //    break;
            //}

            //}

            if (windlg.Exists)
            {

                WinButton addposbtn = new WinButton(windlg);
                addposbtn.SearchProperties.Add(WinButton.PropertyNames.Name, buttontext);
                if (addposbtn.Exists)
                {
                    Mouse.Click(addposbtn);
                }
            }
            else
            {
                LogtoFile("Did not find the Dialog Object dialog  in Max number of Attemps for button Text " + buttontext);
            }
        }
        public static void ClickToolBar(string windowtitle, string toolbarname)
        {
            WinWindow addposn = new WinWindow();
            addposn.SearchProperties.Add(WinWindow.PropertyNames.Name, windowtitle, PropertyExpressionOperator.Contains);
            WinToolBar addposbtn = new WinToolBar(addposn);
            addposbtn.SearchProperties.Add(WinToolBar.PropertyNames.Name, toolbarname);
            Mouse.Click(addposbtn);
        }


        public static void EnterDatainTextbox(string windowtitle, string buttontext,string val)
        {
            WinWindow addposn = new WinWindow();
            addposn.SearchProperties.Add(WinWindow.PropertyNames.Name, windowtitle, PropertyExpressionOperator.Contains);
            WinEdit addposbtn = new WinEdit(addposn);
            addposbtn.SearchProperties.Add(WinEdit.PropertyNames.Name, buttontext);
            Mouse.Click(addposbtn);
            addposbtn.Text = val;
        }

        public static void LogtoFile(string logtext)
        {

            string fullpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            System.IO.File.AppendAllText(Path.Combine(fullpath, "UIA.txt"), logtext + Environment.NewLine);
        }
    }
}
