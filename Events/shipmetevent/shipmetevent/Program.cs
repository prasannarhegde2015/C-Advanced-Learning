using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Threading;

namespace shipmetevent
{
    class Class1
    {
        static void Main(string[] args)
        {
            // we will create our instance
            Shipment myItem = new Shipment();
            // we need to add the delegate event to new object
            myItem.OnPopUpWindowOccurred +=
                   new Shipment.PopupHandler(ShowUserMessage);
            // we assumed that the item has been just shipped and 
            // we are assigning a tracking number to it.
          //  myItem.TrackingNumber = myItem.IswindowExists() ? "Yes" : "No";

            // The common procedure to see what is going on the 
            // console screen
            Thread t2 = new Thread(delegate()
            {
                checkasync(myItem);
            }
            );
            t2.Start();
            
            for (int i=0; i< 100 ; i++)
            {
                Console.WriteLine("Exeuctelin line " + i);
                System.Threading.Thread.Sleep(1000);
            }
            
            Console.Read();
        }

        static void ShowUserMessage(object a, ShipArgs e)
        {
            Console.WriteLine(e.Message);
            Shipment myItem2 = new Shipment();
            myItem2.closepopup();
        }

        static void checkasync(Shipment spp)
        {
            bool ck = true;
            while (ck == true)
            {
                spp.TrackingNumber = spp.IswindowExists() ? "Yes" : "No";
            }
           //  spp.TrackingNumber => 
        }
    }


    public class ShipArgs : EventArgs
    {
        private string message;

        public ShipArgs(string message)
        {
            this.message = message;
        }

        // This is a straightforward implementation for 
        // declaring a public field
        public string Message
        {
            get
            {
                return message;
            }
        }
    }


    public class Shipment
    {
        private string PopupExists;

        // The delegate procedure we are assigning to our object
        public delegate void PopupHandler(object myObject,
                                             ShipArgs myArgs);

        public event PopupHandler OnPopUpWindowOccurred;

        public string TrackingNumber
        {
            set
            {
                PopupExists = value;

                // We need to check whether a tracking number 
                // was assigned to the field.
                if (PopupExists.ToLower() ==  "yes" )
                {
                    ShipArgs myArgs = new ShipArgs("Popup has occured");

                    // Tracking number is available, raise the event.
                    OnPopUpWindowOccurred(this, myArgs);
                }

                else
                {

                }
            }
        }

        public Shipment()
        {
        }
        public void closepopup()
        {
            AutomationElement ae = AutomationElement.RootElement;
            Condition cond1 = new AndCondition(
                new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                new PropertyCondition(AutomationElement.NameProperty, "LOWIS: Connect"));
            AutomationElement win = ae.FindFirst(TreeScope.Descendants, cond1);
            if (win != null) // perform the action when window is found 
            {
                Console.WriteLine(" Hearing to Event fired I Got Window ...... ");
                Condition cond2 = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button),
                    new PropertyCondition(AutomationElement.NameProperty, "Close"));

                AutomationElement btn = win.FindFirst(TreeScope.Descendants, cond2);
                InvokePattern ivk = (InvokePattern)btn.GetCurrentPattern(InvokePattern.Pattern);
                ivk.Invoke();
            }
            else
            {
                Console.WriteLine(" Hearing to Event fired I did not get a Window ...... ");
                return;
            }
        }
        public bool IswindowExists()
        {

            
           
                AutomationElement win = null;
                //  while (win == null )
                //  {
                AutomationElement ae = AutomationElement.RootElement;
                Condition cond1 = new AndCondition(
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window),
                    new PropertyCondition(AutomationElement.NameProperty, "LOWIS: Connect"));
                win = ae.FindFirst(TreeScope.Descendants, cond1);

                if (win == null)
                {
                    return false;
                }

                else
                {
                    return true;
                }
           

        }
    }
}
