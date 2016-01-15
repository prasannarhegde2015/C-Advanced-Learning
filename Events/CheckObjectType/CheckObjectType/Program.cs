using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WELLPERFORMANCESERVERLib;
namespace CheckObjectType
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Uage :  CheckObjectType.exe <full file path > <val1> <val2> ");
                    return;
                }
                WFloInterface WfInt = new WFloInterface();
                WfInt.OpenFile(args[0]);
                var OpPtCalc = WfInt.GetOpPtCalculator();
                OpPtCalc.UsePCPPumpPerformanceRange = true;
                OpPtCalc.AutoSearchOpPtforPCP = false;
                var res = OpPtCalc.CalculateOperatingSpeedforPCP(Convert.ToDouble(args[1]), Convert.ToDouble(args[2]));
                Console.WriteLine(" Min Value " + OpPtCalc.MinimumRangeValue);
                Console.WriteLine(" Min Value " + OpPtCalc.MaximumRangeValue);
           
            }
            catch (Exception ex)
            {
                Console.WriteLine("Got Exception : "+ex.Message);
            }

        }
    }

}
    