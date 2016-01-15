using System;
using System.Windows.Automation;

namespace wildert
{
    public class Metronome
    {
        public delegate void popupEventHandler(Metronome m, EventArgs e);
        public event popupEventHandler PopupOccured;
      //  public event EventHandler PopupOccured;
        
      
        public EventArgs e = null;
        public  void OnPopupOccured()
        {
            popupEventHandler handler = PopupOccured;
            
                if (handler != null)
                {
                    
                    handler(this, e);
                    
                }
            
        }
        //public void Start()
        //{
        //    if (PopupOccured != null)
        //    {
        //        PopupOccured(this, e);
        //    }

        //}
       
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

 

    public class Listener
    {
        public void Subscribe(Metronome m)
        {
            
            m.PopupOccured += new Metronome.popupEventHandler(HeardIt);
            
            
        }
        public void HeardIt(Metronome m, EventArgs e)
        {
            Console.WriteLine("Popup Ocured");
            //if (m.IswindowExists() == true)
            //{
            //    m.closepopup();
            //}
        }

    }
    class Test
    {
        static void Main()
        {
            Metronome m = new Metronome();
            Listener l = new Listener();
         //   l.Subscribe(m);
        //    m.OnPopupOccured();
            m.PopupOccured += new Metronome.popupEventHandler( l.HeardIt);
            for (int i = 0; i < 300; i++)
            {
                Console.WriteLine("Executing Line "+i);
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}
