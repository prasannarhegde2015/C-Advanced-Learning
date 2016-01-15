using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helper;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Automation;
using System.IO;
namespace getscreenshot
{
    class Program
    {
        static void Main(string[] args)

            
        {
            getScreenshotForWindowwithTitle("Microsoft Lync", @"C:\test");
            

        }

        public static void getScreenshotForWindowwithTitle(string partorfulltext, string screenshotlocation)
        {
            AutomationElement root = AutomationElement.RootElement;
            Condition cndwindows = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);
            AutomationElementCollection ActiveWindows = root.FindAll(TreeScope.Descendants, cndwindows);

            AutomationElement ActiveWindow = null;
            foreach (AutomationElement windw in ActiveWindows)
            {
                if (windw.Current.Name.Contains(partorfulltext))
                {
                    ActiveWindow = windw;
                    break;
                }
            }
            ActiveWindow.SetFocus();
            WindowPattern wndptn = (WindowPattern)ActiveWindow.GetCurrentPattern(WindowPattern.Pattern);
            wndptn.SetWindowVisualState(WindowVisualState.Maximized);
            System.Threading.Thread.Sleep(2000);
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                    Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(bitmap as Image);
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            bitmap.Save(  Path.Combine(screenshotlocation, Guid.NewGuid() + ".jpg"), ImageFormat.Jpeg);
        }
    }
}
