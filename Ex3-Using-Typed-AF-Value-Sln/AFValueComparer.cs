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

namespace Ex3_Using_Typed_AF_Value_Sln
{
    public class AFValueComparer : IComparer<AFValue>
    {
        public int Compare(AFValue val1, AFValue val2)
        {
            if (val1.ValueTypeCode != val2.ValueTypeCode)
            {
                throw new InvalidOperationException("Value types do not match");
            }

            if (val1.ValueTypeCode == TypeCode.Double)
            {
                return val1.ValueAsDouble().CompareTo(val2.ValueAsDouble());
            }
            else if (val1.ValueTypeCode == TypeCode.Single)
            {
                return val1.ValueAsSingle().CompareTo(val2.ValueAsSingle());
            }
            else if (val1.ValueTypeCode == TypeCode.Int32)
            {
                return val1.ValueAsInt32().CompareTo(val2.ValueAsInt32());
            }
            else
            {
                throw new InvalidOperationException(string.Format("Cannot compare type {0}", val2.ValueType.Name));
            }
        }
    }
}
