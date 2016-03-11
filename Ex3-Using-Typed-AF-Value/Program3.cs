using System;
using System.Collections.Generic;
using System.Linq;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace Ex3_Using_Typed_AF_Value
{
    class Program3
    {
        static void Main(string[] args)
        {
            Random rnd = new Random(10);

            // Change the line below to use the AFValue.Create instead of "new AFValue"
            List<AFValue> valuesToSort = Enumerable.Range(0, 10).Select(i => new AFValue(
                attribute: null,
                newValue: rnd.Next(100),
                timestamp: new AFTime(DateTime.Today.AddSeconds(i))))
                .ToList();

            valuesToSort.Sort(new AFValueComparer());

            foreach (AFValue val in valuesToSort)
            {
                if (val.ValueTypeCode == TypeCode.Int32) // should not be false in this case
                {
                    Console.WriteLine(val.ValueAsInt32());
                }

            }

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
