using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RepeatRS
{

  
    class person
    {
        private int age;
        private string name;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }

        }
        public int Age
        {
           get
           {
                return age;
            }

            set
            {
                age = value;
            }
        }

    }
    

    class pg2 
    {
        public delegate bool FilterDelegate(person p);

        public static void Main(string[]args)
        {
            person p1 = new person { Name="Andy",Age=23};
            person p2 = new person { Name="Grant",Age=45};
            person p3 = new person { Name="Mark",Age=55};
            person p4 = new person { Name="Ricky",Age=13};
            person p5 = new person { Name = "Jonty", Age = 76 };
            List<person> people = new List<person>{p1,p2,p3,p4,p5};

            Displaymessge("Childern", people, IsChild);
            Displaymessge("Adult", people, IsAdult);
            Displaymessge("Senior", people, IsSenior);
            Console.ReadLine();

        }

        public static void Displaymessge(string titltmessage ,List<person> ppl, FilterDelegate f)
        {
            Console.WriteLine("*******************************************");
            Console.WriteLine(titltmessage);
            Console.WriteLine("*******************************************");
            foreach (person p in ppl)
            {
                if (f(p))
                {
                    Console.WriteLine("Name:{0}  ,   Age:  {1}", p.Name, p.Age);
                }
            }
            Console.WriteLine("*******************************************");
        }
        public static bool IsChild(person p)
        {
            return p.Age <18 ;
        }

        public static bool IsAdult(person p)
        {
            return p.Age >=  18 && p.Age <= 65 ;
        }


        public static bool IsSenior(person p)
        {
            return p.Age > 65;
        }
    }



}
