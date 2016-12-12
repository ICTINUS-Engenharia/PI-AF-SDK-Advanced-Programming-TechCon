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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using External;

namespace Ex5_Real_Time_Analytics_Sln
{
    class Program5
    {
        static void Main(string[] args)
        {
            AFDatabase db = ConnectionHelper.GetDatabase("PISRV01", "Feeder Voltage Monitoring");

            AFAttributeTemplate attrTemp = db.ElementTemplates["Feeder"].AttributeTemplates["Reactive Power"];

            AssetRankProvider rankProvider = new AssetRankProvider(attrTemp);

            rankProvider.Start();

            // Get top 3 Feeders every 5 seconds. Do this 10 times.
            foreach (int i in Enumerable.Range(0, 10))
            {
                IList<AFRankedValue> rankings = rankProvider.GetTopNElements(3);
                foreach (var r in rankings)
                {
                    Console.WriteLine($"{r.Ranking} {r.Value.Attribute.Element.Name} {r.Value.Timestamp} {r.Value.Value}");
                }
                Console.WriteLine();
                Thread.Sleep(5000);
            }

            // Remove unmanaged resources and server-side signup.
            rankProvider.Dispose();

            Console.WriteLine("Press any key to quit");
            Console.ReadKey();
        }
    }
}
