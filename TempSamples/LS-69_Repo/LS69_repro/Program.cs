using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UITest;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WinControls;
using Microsoft.VisualStudio.TestTools.UITest.Extension;
using System.Configuration;

namespace ConsoleApplication5
{
    class Program
    {
        static void Main(string[] args)
        {
            Playback.Initialize();
          //  ClickWindowButton("LOWIS:", "Add a new Position");
            string  trycount = ConfigurationManager.AppSettings["trycount"];
            int cnt = Convert.ToInt32(trycount);
            for (int i = 0; i < cnt; i++)
            {
             //  clickaddcancel();
                clickdeletepositions();
                Playback.Wait(3000);
            }
            Playback.Cleanup();
        }


        public static void clickaddcancel()
        {
           // ClickWindowButton("LOWIS:","Add a new Position");
            ClickWindowButton("Add Position","Add Position");
            ClickWindowButton("VBScript", "Yes");
            ClickWindowButton("Message", "OK");
           
        }
        public static void clickdeletepositions()
        {
            ClickWindowButton("LOWIS:", "Delete a Position");
            ClickWindowButton("Delete Position", "Delete Position");
            ClickWindowButton("Position Message", "OK");
        }

        public static void ClickWindowButton(string windowtitle, string buttontext)
        {
            WinWindow addposn = new WinWindow();
            addposn.SearchProperties.Add(WinWindow.PropertyNames.Name, windowtitle, PropertyExpressionOperator.Contains);
            WinButton addposbtn = new WinButton(addposn);
            addposbtn.SearchProperties.Add(WinButton.PropertyNames.Name, buttontext);
            Mouse.Click(addposbtn);
        }
    }
}
