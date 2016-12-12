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
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

namespace Ex5_Real_Time_Analytics
{
    interface IRankProvider : IDisposable
    {
        // AFAttributeTemplate specifying which attributes to rank
        AFAttributeTemplate AttributeTemplate { get; set; }

        // The data pipe for getting values
        AFDataPipe DataPipe { get; set; }

        // Initializes sign-ups
        void Start();

        // Returns the rankings
        IList<AFRankedValue> GetTopNElements(int N);
    }
}
