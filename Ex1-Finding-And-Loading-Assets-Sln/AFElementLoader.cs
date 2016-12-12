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

namespace Ex1_Finding_And_Loading_Assets_Sln
{
    public static class AFElementLoader
    {
        public static IList<AFElement> LoadElements(AFElementTemplate elementTemplate, IEnumerable<string> attributesToLoad)
        {
            int totalCount;
            int startIndex = 0;
            int pageSize = 1000;

            List<AFElement> results = new List<AFElement>();

            // Paging pattern
            do
            {
                var baseElements = elementTemplate.FindInstantiatedElements(
                                includeDerived: true,
                                sortField: AFSortField.Name,
                                sortOrder: AFSortOrder.Ascending,
                                startIndex: startIndex,
                                maxCount: pageSize,
                                totalCount: out totalCount);

                // If there are no elements, break the process
                if (baseElements.Count == 0)
                    break;

                IEnumerable<AFElement> elements = baseElements.OfType<AFElement>();

                IEnumerable<IGrouping<AFElementTemplate, AFElement>> elementGroupings = elements.GroupBy(elm => elm.Template);
                foreach (var item in elementGroupings)
                {
                    // The passed in attribute template name may belong to a base element template.
                    // GetLastAttributeTemplateOverride searches upwards the template inheritance chain
                    // until it finds the desired attribute template.
                    List<AFAttributeTemplate> attrTemplates = attributesToLoad
                        .Select(atr => GetLastAttributeTemplateOverride(item.Key, atr))
                        .Where(atr => atr != null)
                        .ToList();

                    List<AFElement> elementsToLoad = item.ToList();
                    AFElement.LoadAttributes(elementsToLoad, attrTemplates);
                    results.AddRange(elementsToLoad);
                }     

                startIndex += baseElements.Count;
            } while (startIndex < totalCount);

            return results;
        }

        private static AFAttributeTemplate GetLastAttributeTemplateOverride(AFElementTemplate elmTemplate, string attributeName)
        {
            if (elmTemplate == null)
                throw new ArgumentNullException("elmTemplate");

            AFElementTemplate currElementTemplate = elmTemplate;
            do
            {
                var attrTemplate = currElementTemplate.AttributeTemplates[attributeName];

                if (attrTemplate != null)
                    return attrTemplate;
                else
                    currElementTemplate = currElementTemplate.BaseTemplate;
            } while (currElementTemplate != null);

            return null;
        }
    }
}
