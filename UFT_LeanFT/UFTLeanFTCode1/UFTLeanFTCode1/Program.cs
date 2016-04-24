using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HP.LFT.Common;
using HP.LFT.SDK;
using HP.LFT.SDK.Configuration;
using HP.LFT.SDK.Web;
using HP.LFT.Common.Utils;
using UFTLeanFTCode1.ObjectLibrary;

namespace UFTLeanFTCode1
{
    class Program
    {
        static void Main(string[] args)

        {
           

            SdkConfiguration config = new SdkConfiguration();
            config.ServerAddress = new Uri("ws://localhost:5095");
            SDK.Init(config);
            IBrowser browser = BrowserFactory.Launch(BrowserType.Chrome);
            browser.Navigate("http://localhost");
            // *********  Home Page Object Model ************************ ;
            HomePage home = new HomePage();
            home.brhomepage = browser;
            home.lnkphpsamples.Click();
            // ******* PHP Samples Object Model ************************
            PHPSamples phpsmpleslist = new PHPSamples();
            phpsmpleslist.brphpsampleslistpage = browser;
            phpsmpleslist.lnkcreatelist.Click();
            // *******   Create List Page Object List  ************************
            CreateListPage cretepage = new CreateListPage();
            cretepage.brcreatelistpage = browser;
            cretepage.tbfistname.SetValue("Prasanna");
            cretepage.tblastname.SetValue("Hegde");
            cretepage.tbemail.SetValue("prasannarhegde@gmail.com");
            cretepage.tbphone.SetValue("976812570");
            cretepage.tbaddress.SetValue("Airoli NaviMumbai");
            cretepage.btnsubmit.Click();


        }

        public static ILink GetLink( IBrowser br,  string linktext)
        
        {
            return br.Describe<ILink>(new LinkDescription { InnerText = linktext });
        }

        public static IEditField GetTextBox(IBrowser br, string controlname)
        {
            return br.Describe<IEditField>(new EditFieldDescription { Name = controlname });
        }

        public static IButton GetButton(IBrowser br, string controlname)
        {
            return br.Describe<IButton>(new ButtonDescription { Name = controlname });
        }
    }
}
