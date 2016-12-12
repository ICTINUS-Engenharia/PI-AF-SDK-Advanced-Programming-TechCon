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
