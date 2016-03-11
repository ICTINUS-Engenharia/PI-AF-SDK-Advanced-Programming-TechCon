using System;
using System.Collections.Generic;
using System.Linq;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Time;

namespace Ex3_Using_Typed_AF_Value_Sln
{
    class Program3
    {
        static void Main(string[] args)
        {
            Random rnd = new Random(10);

            // Generate 10 random double values between 0 and 100 with timestamps every 1 second from midnight today
            List<AFValue> valuesToSort = Enumerable.Range(0, 10).Select(i => AFValue.Create(
                attribute: null, 
                value: rnd.Next(100), 
                timestamp: new AFTime(DateTime.Today.AddSeconds(i))))
                .ToList();

            valuesToSort.Sort(new AFValueComparer());

            foreach (AFValue val in valuesToSort)
            {
                if (val.ValueTypeCode == TypeCode.Int32) // should not be false in this case
                {
                    Console.WriteLine("Timestamp: {0}, Value: {1}", val.Timestamp, val.ValueAsInt32());
                }
                
            }

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
