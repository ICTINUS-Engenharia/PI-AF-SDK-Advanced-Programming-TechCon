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

namespace Ex3_Using_Typed_AF_Value
{
    public class AFValueComparer : IComparer<AFValue>
    {
        int IComparer<AFValue>.Compare(AFValue val1, AFValue val2)
        {
            // Your code here

            // Change the return code
            return 0;
        }
    }
}
