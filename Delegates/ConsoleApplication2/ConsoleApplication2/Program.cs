using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        delegate string UppercaseDelegate(string input);

        static string UppercaseFirst(string input)
        {
            char[] buffer = input.ToCharArray();
            buffer[0] = char.ToUpper(buffer[0]);
            return new string(buffer);
        }

        static string UppercaseLast(string input)
        {
            char[] buffer = input.ToCharArray();
            buffer[buffer.Length - 1] = char.ToUpper(buffer[buffer.Length - 1]);
            return new string(buffer);
        }

        static string UppercaseAll(string input)
        {
            return input.ToUpper();
        }

        static void WriteOutput(string input, UppercaseDelegate del)
        {
            Console.WriteLine("Your string before: {0}", input);
            Console.WriteLine("Your string after: {0}", del(input));
        }

        static void Main()
        {
            // Wrap the methods inside delegate instances and pass to the method.
            //WriteOutput("perls", new UppercaseDelegate(UppercaseFirst));
            //WriteOutput("perls", new UppercaseDelegate(UppercaseLast));
            //WriteOutput("perls", new UppercaseDelegate(UppercaseAll));
            //Console.ReadLine();
            //Console.WriteLine("perls {0} ", UppercaseFirst("perls"));
            //Console.WriteLine("perls {0} ", UppercaseLast("perls"));
            //Console.WriteLine("perls {0} ", UppercaseAll("perls"));
            //Console.ReadLine();


            List<int> intlist = new List<int>() { 1, 2, 5, 9 };
             int result = intlist.FindIndex( X =>( X==5 ));
             Console.WriteLine(result);
             Console.ReadLine();
        }
        
    }
}
