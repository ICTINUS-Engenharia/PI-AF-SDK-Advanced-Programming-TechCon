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
