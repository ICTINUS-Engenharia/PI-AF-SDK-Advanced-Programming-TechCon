#region Copyright
//  Copyright 2016  OSIsoft, LLC
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
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
