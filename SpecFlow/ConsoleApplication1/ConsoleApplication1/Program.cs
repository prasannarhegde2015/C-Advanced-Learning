using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DelegateProj
{
    public delegate double CalcDlg(double num1, double num2);

    class Program
    {
        static void Main(string[] args)
        {
            CalcDlg dlobj1 = new CalcDlg(Add);
            CalcDlg dlobj2 = new CalcDlg(Subtract);
            CalcDlg dlobj3 = new CalcDlg(Product);
            CalcDlg dlobj4 = new CalcDlg(Divide);
            CalcDlg dlobj5 = dlobj1 + dlobj2 + dlobj3 + dlobj4;
            dlobj5(10, 5);
            Console.ReadLine();

        }

        static double Add(double num1, double num2)
        {
            double result = num1 + num2;
            Console.WriteLine("Result of Add : " + result);
            return result;
        }

        static double Subtract(double num1, double num2)
        {
            double result = num1 - num2;
            Console.WriteLine("Result of Subtract : " + result);
            return result;
        }

        static double Product(double num1, double num2)
        {
            double result = num1 * num2;
            Console.WriteLine("Result of Product : " + result);
            return result;
        }

        static double Divide(double num1, double num2)
        {
            double result = num1 / num2;
            Console.WriteLine("Result of Divide: " + result);
            return result;
        }
    }
}

