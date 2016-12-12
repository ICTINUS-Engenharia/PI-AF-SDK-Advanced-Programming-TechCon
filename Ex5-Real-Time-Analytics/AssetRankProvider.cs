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
using System.Threading.Tasks;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

namespace Ex5_Real_Time_Analytics
{
    public class AssetRankProvider : IObserver<AFDataPipeEvent>, IRankProvider
    {
        public AFAttributeTemplate AttributeTemplate { get; set; }
        public AFDataPipe DataPipe { get; set; }

        // Define other instance members here

        public AssetRankProvider(AFAttributeTemplate attrTemplate)
        {
            AttributeTemplate = attrTemplate;
            DataPipe = new AFDataPipe();

            // Your code here
            // 1. Initialize an internal data structure to store the latest values keyed by element
        }

        public void Start()
        {
            // Gets all attributes from the AttributeTemplate
            AFAttributeList attrList = GetAttributes();

            // Your code here
            // 1. Subscribe this instance to the data pipe.
            // 2. Signup the attributes in attrList above to the data pipe.
            // 3. Start polling for data pipe events.
        }

        public void OnCompleted()
        {
            Console.WriteLine("{0} | AssetRankProvider has completed", DateTime.Now);
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(AFDataPipeEvent dpEvent)
        {
            // Your code here
            // 1. Process the AFDataPipeEvent and add the returned AFValue to the internal data structure defined earlier.
        }

        public IList<AFRankedValue> GetTopNElements(int N)
        {
            IList<AFRankedValue> rankings = null;

            // Your code here
            // 1. Return top N Feeder elements (as AFRankedValue) sorted by their current Reactive Power values. 

            return rankings;
        }

        public void Dispose()
        {
            // Your code here
            // 1. Dispose of the AFDataPipe
        }

        private AFAttributeList GetAttributes()
        {
            int startIndex = 0;
            int totalCount;
            int pageSize = 1000;

            AFAttributeList attrList = new AFAttributeList();
            do
            {
                AFAttributeList attrListTemp = AFAttribute.FindElementAttributes(
                    database: AttributeTemplate.Database,
                    searchRoot: null,
                    nameFilter: "*",
                    elemCategory: null,
                    elemTemplate: AttributeTemplate.ElementTemplate,
                    elemType: AFElementType.Any,
                    attrNameFilter: AttributeTemplate.Name,
                    attrCategory: null,
                    attrType: TypeCode.Empty,
                    searchFullHierarchy: true,
                    sortField: AFSortField.Name,
                    sortOrder: AFSortOrder.Ascending,
                    startIndex: startIndex,
                    maxCount: pageSize,
                    totalCount: out totalCount);

                attrList.AddRange(attrListTemp);

                startIndex += pageSize;
            } while (startIndex < totalCount);

            return attrList;
        }
    }
}
