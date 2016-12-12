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

namespace Ex2_Measuring_AF_Server_RPCs_Sln
{
    class Program2
    {
        static void Main(string[] args)
        {
            // This factory method is new in 2.8.
            PISystem ps = PISystem.CreatePISystem("PISRV01");

            using (new AFProbe("PrintAttributeCounts", ps))
            {
                AFDatabase db = ps.Databases["Feeder Voltage Monitoring"];

                // Build search object
                AFSearchToken searchToken = new AFSearchToken(
                    filter: AFSearchFilter.Root,
                    searchOperator: AFSearchOperator.Equal,
                    value: db.Elements["Assets"].GetPath());

                AFElementSearch elementSearch = new AFElementSearch(db, "Feeders and Transformers", new[] { searchToken });

                Console.WriteLine("Feeders and Transformers");
                // Use full load: true to fully load the elements
                foreach (AFElement element in elementSearch.FindElements(fullLoad: true))
                {
                    Console.WriteLine("Element: {0}, # Attributes: {1}", element.Name, element.Attributes.Count);
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
