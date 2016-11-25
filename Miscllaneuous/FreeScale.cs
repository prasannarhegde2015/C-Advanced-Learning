/*
 * Created by SharpDevelop.
 * User: lntinfotech
 * Date: 11/23/2016
 * Time: 12:20 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Automation;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support;
using OpenQA.Selenium.Support.UI;


namespace HTTPFoxCapture
{
	class Program
	{
		public static IWebDriver globaldriver = null;
		public static string httpfoxaddonpath = System.Configuration.ConfigurationManager.AppSettings["httpfoxaddonpath"];
		//public static String httpfoxaddonpath = @"C:\Users\lntinfotech\Downloads\FreeScaleWebAnlytics\httpfox-0.8.14-fx+sm.xpi";
		public static string searchstring = System.Configuration.ConfigurationManager.AppSettings["searchstring"];
		public static AutomationElement FFwindow = null;
		public static AutomationElement foxtable = null;
		public static AutomationElement btnStartFox = null;
		public static AutomationElement btnStopFox = null;
		public static AutomationElement btnClearFox = null;
		public static AutomationElementCollection buttonslist = null;
		public static AutomationElement Foxtolbar = null;
		public static String urlfileslist = "";
		public static Helper hlp = new Helper();
		public static string up = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
		public static WebDriverWait dwait ;
		public static string basewindowhndl = "";
		public static string basetitle = "";
		
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			try {
				// TODO: Implement Functionality Here
				setFireFoxDriver();
				string urfile = System.Configuration.ConfigurationManager.AppSettings["urlfilelist"];
				StreamReader rs = new StreamReader(urfile);
				int linecount = 1;
				DataTable dt1 = hlp.dtFromExcelFile(urfile, "URLlist");
				for (int i = 0; i < dt1.Rows.Count; i++) {
					string key = dt1.Rows[i]["Key"].ToString();
					string inputurl = dt1.Rows[i]["InputURL"].ToString();
					string locator = dt1.Rows[i]["Locator"].ToString();
					pageLoadWait();
					if (linecount == 1) { //open only once
						openhttpfox();
						enterSearchURL(searchstring);
						
					}
					wait(1);
					starthttpfox();
					if  (linecount == 1)
					{
						navigate(inputurl);
						basetitle = globaldriver.Title;
					}
					else
					{
				
					PerformClickLocator(globaldriver, locator);
					}
					pageLoadWait();
					AjaxWaitForReady(globaldriver);
					if (globaldriver.WindowHandles.Count > 1 )
					{
						CloseotherWindow(globaldriver);
					}
					
					getdatafromFF2(key); 
					stophttpfox();
					clearhttpfox();
					string curtitle = globaldriver.Title;
					while (curtitle != basetitle)
					{
						
					    navigateBack(globaldriver);
					    curtitle = globaldriver.Title;
					    System.Console.WriteLine("base window: "+basetitle+ "Current window: "+curtitle);
					}
					pageLoadWait();
					AjaxWaitForReady(globaldriver);
					linecount++;
				}
			} catch (Exception ex) {
				System.Console.WriteLine("Error Encoutred during excution" + ex.ToString());
			}
			Console.Write("Press any key to continue . . . ");
			Console.ReadLine();
		}
		
		public static void setFireFoxDriver()
		{
			FirefoxProfile pf = new FirefoxProfile();
			pf.AddExtension(httpfoxaddonpath);
			globaldriver = new FirefoxDriver(pf);
			globaldriver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(300.00));
			basewindowhndl = globaldriver.CurrentWindowHandle;
		}
		
		public static void pageLoadWait()
		{
			IWait<IWebDriver> wait = new WebDriverWait(globaldriver, TimeSpan.FromSeconds(300.00));
			wait.Until(driver1 => ((IJavaScriptExecutor)globaldriver).ExecuteScript("return document.readyState").Equals("complete"));
		}
		public static void starthttpfox()
		{
			getButtoncolection();
			if (buttonslist != null) {
				foreach (AutomationElement btn in buttonslist) {
					if (btn.Current.Name == "Start") {
						clickButton(btn);
						break;
					}
				}
			}
		}
		public static void stophttpfox()
		{
			if (buttonslist != null) {
				foreach (AutomationElement btn in buttonslist) {
					if (btn.Current.Name == "Stop") {
						clickButton(btn);
						break;
					}
				}
			}
		}
		public static void clearhttpfox()
		{
			if (buttonslist != null) {
				foreach (AutomationElement btn in buttonslist) {
					if (btn.Current.Name == "Clear") {
						clickButton(btn);
						break;
					}
				}
			}
		}
		public static void enterSearchURL(string searchurl)
		{
			getFFWindow();
			findtoolbar();
			if (Foxtolbar != null) {
				AutomationElement txtcust = Foxtolbar.FindFirst(TreeScope.Children,
					                           new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Custom));
			
				AutomationElement srchtxtbox = txtcust.FindFirst(TreeScope.Children,
					                              new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
			
				//srchtxtbox.SetFocus();
				if (srchtxtbox != null) {
					InvokePattern ipt = (InvokePattern)srchtxtbox.GetCurrentPattern(InvokePattern.Pattern);
					ipt.Invoke();
					System.Windows.Forms.SendKeys.SendWait(searchurl);
					System.Windows.Forms.SendKeys.SendWait("{Enter}");
					System.Threading.Thread.Sleep(1000);
				} else {
					System.Console.WriteLine("Unable to find the toolbar .. :) ");
				}
			}
			
		}
		public static void navigate(String URL)
		{
			globaldriver.Navigate().GoToUrl(URL);
		}
		public static void openhttpfox()
		{
			System.Windows.Forms.SendKeys.SendWait("^+{F2}");
			System.Threading.Thread.Sleep(2000);
		}
		public static void getdatafromFF()
		{
			if (FFwindow == null) {
				getFFWindow();
			}
			
			StringBuilder sb = new StringBuilder();
			foxtable = FFwindow.FindFirst(TreeScope.Children,
				new PropertyCondition(AutomationElement.ControlTypeProperty,
					ControlType.Table));
			if (foxtable != null) {
				System.Console.WriteLine("Found Table finding Rows now");
			}
			AutomationElementCollection tablerows = foxtable.FindAll(TreeScope.Children,
				                                        new PropertyCondition(AutomationElement.ControlTypeProperty,
					                                        ControlType.Custom));
			char quote = '\u0022';
			int row = 1;
			foreach (AutomationElement trow  in tablerows) {
				AutomationElementCollection tablecols = trow.FindAll(TreeScope.Children,
					                                        new PropertyCondition(AutomationElement.ControlTypeProperty,
						                                        ControlType.Custom));
				foreach (AutomationElement tcol in tablecols) {
					if (tcol.Current.Name.Contains("http")) {
						sb.Append(quote + row.ToString() + quote + "," + quote + tcol.Current.Name + quote);
						
					}
				}
				sb.Append(System.Environment.NewLine);
				row++;
			}
			WriteCSVFile(sb.ToString());
			System.Console.WriteLine("String URL " + sb.ToString());
			
		}
		
		public static void getFFWindow()
		{
			AutomationElement ae = AutomationElement.RootElement;
			AutomationElementCollection allwindows = ae.FindAll(TreeScope.Children,
			       

				                                         new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
			
			System.Console.WriteLine("Finding Window");
			foreach (AutomationElement indwin in allwindows) {
				if (indwin.Current.ClassName.Contains("MozillaWindowClass")) {
					FFwindow = indwin;
					break;
				}
			}
			if (FFwindow != null) {
				System.Console.WriteLine("Obtained Window with Title" + FFwindow.Current.Name);
			} else {
				System.Console.WriteLine("No Window Was found with " + FFwindow.Current.Name);
			}
			
		}
		public static void WriteCSVFile(String strtext)
		{
			char quoteh = '\u0022';
			string op = Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
			StringBuilder sbh = new StringBuilder();
			string[] colnames = new string[]{ "Sr No", "URL" };
			foreach (string ielem in colnames) {
				sbh.Append(quoteh + ielem + quoteh + ",");
				
			}
			sbh.Append(Environment.NewLine);
			string fname = op + "\\NXP" + System.DateTime.Now.ToString("dd-MM-yyyy_hh_mm_ss") + ".csv";
			if (System.IO.File.Exists(fname)) {
				
				System.IO.File.AppendAllText(fname, strtext);
			} else {
				System.IO.File.AppendAllText(fname, sbh.ToString());
				System.IO.File.AppendAllText(fname, strtext);
			}
		}
		
		public static void findtoolbar()
		{
			AutomationElementCollection alltoolbars = FFwindow.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ToolBar));
			Foxtolbar = alltoolbars[3];
			                                                           
		}
		
		public static void getButtoncolection()
		{
			buttonslist =	Foxtolbar.FindAll(TreeScope.Children,
				new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
		}
		
		public static void clickButton(AutomationElement el)
		{
			try {
				InvokePattern ivk = (InvokePattern)el.GetCurrentPattern(InvokePattern.Pattern);
				ivk.Invoke();
				System.Console.WriteLine("Seemed to click button did ! " + el.Current.Name);
			} catch (Exception e) {
				System.Console.WriteLine("Got some Error" + e);
			}
		}
		
		public static void getdatafromFF2(String sKey)
		{
			DataTable dtn = new DataTable();
			dtn.Columns.Add("Key");
			dtn.Columns.Add("URL");
			dtn.Columns.Add("KeyfoundforC16");
			if (FFwindow == null) {
				getFFWindow();
			}
			
			StringBuilder sb = new StringBuilder();
			foxtable = FFwindow.FindFirst(TreeScope.Children,
				new PropertyCondition(AutomationElement.ControlTypeProperty,
					ControlType.Table));
			if (foxtable != null) {
				System.Console.WriteLine("Found Table finding Rows now");
			}
			AutomationElementCollection tablerows = foxtable.FindAll(TreeScope.Children,
				                                        new PropertyCondition(AutomationElement.ControlTypeProperty,
					                                        ControlType.Custom));
			
			
			int row = 0;
			foreach (AutomationElement trow  in tablerows) {
				AutomationElementCollection tablecols = trow.FindAll(TreeScope.Children,
					                                        new PropertyCondition(AutomationElement.ControlTypeProperty,
						                                        ControlType.Custom));
				foreach (AutomationElement tcol in tablecols) {
					if (tcol.Current.Name.Contains("http")) {
						DataRow dr = dtn.NewRow();
						dr["Key"] = sKey;
						dr["URL"] = tcol.Current.Name;
						if (tcol.Current.Name.Contains("c16="+sKey ))
						{
							dr["KeyfoundforC16"]="Found";
						}
						else
						{
						    dr["KeyfoundforC16"]="No Found";
						}
						dtn.Rows.Add(dr);
					}
				} 
				sb.Append(System.Environment.NewLine);
			
				
				row++;
			}
			System.Console.WriteLine("String URL "+row+ "  " + sb.ToString());
		//	hlp.CreateWorkbook( up+"\\Output.xlsx" ,"Sht_"+sKey,dtn);
			hlp.CreateWorkbook2( up+"\\Output.xlsx" ,"Sht_"+sKey,dtn);
			
			
		}
		public static void wait(int sec)
		{
			System.Threading.Thread.Sleep(sec * 1000);
		}
		
		public static void PerformClickLocator(IWebDriver drv , String locator)
		{ 
			char[] delim = new char[] { ':' };
			string[] arr = locator.Split(delim);
			dwait = new WebDriverWait(drv,TimeSpan.FromSeconds(300.00));
			switch(arr[0].ToLower())
			{
				case  "id" :
				{
						drv.FindElement(By.Id(arr[1])).Click();
					break;
				}
					case  "name" :
				{
						drv.FindElement(By.Name(arr[1])).Click();
					break;
				}
					case  "linktext" :
				{
						drv.FindElement(By.LinkText(arr[1])).Click();
					break;
					}
			
					case  "partiallinktext" :
				{
						drv.FindElement(By.PartialLinkText(arr[1])).Click();
					break;
				}
					case  "xpath" :
				{
						System.Console.WriteLine("Locator value"+arr[1]);
						drv.FindElement(By.XPath(arr[1])).Click();
					//	dwait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath(arr[1])));
						
					break;
				}
					case  "cssselector" :
				{
					drv.FindElement(By.CssSelector(arr[1])).Click();
					
					break;
				}
					case  "classname" :
				{
						drv.FindElement(By.ClassName(arr[1])).Click();
					break;
				}
					default: break;
			}
			
		}
	    
		public static void navigateBack(IWebDriver drv)
		{
			//drv.Navigate().Back();
			((IJavaScriptExecutor)drv).ExecuteScript("setTimeout(\"history.go(-1)\", 2000)");
			
		}
	  
		public static void CloseotherWindow(IWebDriver drv )
		{
			System.Collections.ObjectModel.ReadOnlyCollection<string> winhndls = drv.WindowHandles;
			foreach( string swind in winhndls)
			{
				if (swind != basewindowhndl)
				{
					drv.SwitchTo().Window(swind);
					drv.Close();
				}
				
			}
			drv.SwitchTo().Window(basewindowhndl);
		}
	
		public static void AjaxWaitForReady(IWebDriver drv)
{
			WebDriverWait ajwait = new WebDriverWait(drv, TimeSpan.FromSeconds(300.00));
            ajwait.Until(driver => (bool)((IJavaScriptExecutor)driver).
            ExecuteScript("return jQuery.active == 0"));
}
	}
	
	
	
	
}
