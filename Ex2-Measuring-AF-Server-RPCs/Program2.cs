using System;
using OSIsoft.AF;
using OSIsoft.AF.Search;
using OSIsoft.AF.Asset;
using External;

namespace Ex2_Measuring_AF_Server_RPCs
{
    class Program2
    {
        static void Main(string[] args)
        {
            // This factory method is new in 2.8.
            PISystem ps = PISystem.CreatePISystem("PISRV01");

            //This is an example
            //using (new AFProbe("FindElementsByAttribute", ps))
            //{
            //    AFDatabase db = ps.Databases["Feeder Voltage Monitoring"];

            //    AFElementTemplate elemTemplate = db.ElementTemplates["Substation Transformer"];

            //    var avq = new[]
            //    {
            //        new AFAttributeValueQuery(elemTemplate.AttributeTemplates["Model"], AFSearchOperator.Equal, "506A"),
            //    };

            //    var elements = AFElement.FindElementsByAttribute(null, "*", avq, true, AFSortField.Name, AFSortOrder.Ascending, 100);
            //}

            using (new AFProbe("PrintAttributeCounts", ps))
            {
                // Your code here
            }

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
