using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HP.LFT.SDK;
using HP.LFT.SDK.Web;
using HP.LFT.Verifications;
using LeanFtTestProject1.ObjectLibrary;


namespace LeanFtTestProject1
{
    [TestClass]
    public class LeanFtTest : UnitTestClassBase<LeanFtTest>
    {
        public IBrowser browser = null;
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            
            GlobalSetup(context);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            browser = BrowserFactory.Launch(BrowserType.Chrome);
        }

        [TestMethod]
        public void HomePageTest()
        {

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

        [TestCleanup]
        public void TestCleanup()
        {
            browser.Close();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GlobalTearDown();
        }
    }
}
