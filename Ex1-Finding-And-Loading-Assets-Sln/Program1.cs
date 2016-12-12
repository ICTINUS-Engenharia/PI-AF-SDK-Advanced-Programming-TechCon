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
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using External;

namespace Ex1_Finding_And_Loading_Assets_Sln
{
    class Program1
    {
        static void Main(string[] args)
        {
            AFDatabase db = ConnectionHelper.GetDatabase("PISRV01", "Feeder Voltage Monitoring");

            AFElementTemplate elemTemp = db.ElementTemplates["Feeder"];
            IList<string> attributesToLoad = new[] { "Reactive Power", "Total Current" }.ToList();

            GC.Collect(2, GCCollectionMode.Forced, blocking: true);
            var begin = GC.GetTotalMemory(forceFullCollection: true);

            IList<AFElement> elementsLoaded = AFElementLoader.LoadElements(elemTemp, attributesToLoad);

            var end = GC.GetTotalMemory(forceFullCollection: true);

            // Keep below 260 KB
            Console.WriteLine("elementsLoaded Memory: {0:N0} KB", (end - begin));

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
    }
}
