using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UITest;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using System.Windows.Forms;

namespace ConsoleApplication3
{
    class Program
    {
        static void Main(string[] args)
        {

            Playback.Initialize();
            WinWindow GetCUITWindow = new WinWindow();
             GetCUITWindow.SearchProperties.Add(WinWindow.PropertyNames.ControlName, "BrowserMain");
             //UITestControlCollection allwnds = GetCUITWindow.FindMatchingControls();
             //GetCUITWindow = (WinWindow)allwnds[0];
            #region usingautoid
            //UITestControlCollection wndCollection = GetCUITWindow.FindMatchingControls();
            //foreach (UITestControl inwin in wndCollection)
            //{
            //   if (inwin.TechnologyName == "MSAA" && ((WinWindow)inwin).ControlName == "BrowserMain")
            //    {
            //        GetCUITWindow = (WinWindow)inwin;
            //    }
            //}
            #endregion
         //   GetCUITWindow.SearchProperties.Add(WinWindow.PropertyNames.Name, "LOWIS:", PropertyExpressionOperator.Contains);
            WinButton btn = new WinButton(GetCUITWindow);
            btn.SearchProperties.Add(WinButton.PropertyNames.Name, "Add a New Well");
            Mouse.Click(btn);
            Playback.Cleanup();

        }
    }
}
