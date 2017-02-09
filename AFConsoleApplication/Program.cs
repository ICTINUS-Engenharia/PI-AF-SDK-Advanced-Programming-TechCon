using OSIsoft.AF;
using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace AFConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Exercises.Run(interactive: true);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
        }
    }
}
