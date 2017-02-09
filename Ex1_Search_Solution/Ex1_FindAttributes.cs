using OSIsoft.AF;
using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Exercise1
{
    public class FindAttributes
    {
        private PISystem _piSystem;

        public FindAttributes()
        {
            PISystems piSystems = new PISystems();
            _piSystem = piSystems.DefaultPISystem;
        }

        public AFAttributeList GetAttributes(string databaseName, string elementTemplateName, string attributeTemplateName)
        {
            return GetAttributes(databaseName, elementTemplateName, attributeTemplateName, fullLoad: false);
        }

        public AFAttributeList GetAttributes(string databaseName, string elementTemplateName, string attributeTemplateName, bool fullLoad)
        {
            AFDatabase database = _piSystem.Databases[databaseName];
            if (database == null)
                throw new InvalidOperationException(String.Format("Database '{0}' not found", databaseName));

            AFElementTemplate elementTemplate = database.ElementTemplates[elementTemplateName];
            if (elementTemplate == null)
                throw new InvalidOperationException(String.Format("Element template '{0}' not found", elementTemplateName));

            AFAttributeTemplate attributeTemplate = elementTemplate.AttributeTemplates[attributeTemplateName];
            if (attributeTemplate == null)
                throw new InvalidOperationException(String.Format("Attribute template '{0}' not found", attributeTemplateName));


            // find all attributes in the given database derived from the specified attribute template
            //   and return them in an AFAttributeList

            int findPageSize = 100000;
            int loadChunkSize = 1000;
            List<List<AFElement>> chunksToLoad = new List<List<AFElement>>();
            List<AFElement> currentChunk = null;
            int startIndex = 0;
            int totalCount;
            do
            {
                //Get elements Header
                var elements = AFElement.FindElementsByTemplate(attributeTemplate.Database, null, attributeTemplate.ElementTemplate, 
                    includeDerived: true, sortField: AFSortField.ID, sortOrder: AFSortOrder.Ascending, 
                    startIndex: startIndex, maxCount: findPageSize, totalCount: out totalCount);
                if (elements == null)
                    break;

                foreach (var element in elements)
                {
                    // create a new chunk if needed
                    if (currentChunk == null)
                        currentChunk = new List<AFElement>(loadChunkSize);

                    // add to current chunk
                    currentChunk.Add(element);

                    // move to next chunk if this one is filled
                    if (currentChunk.Count == loadChunkSize)
                    {
                        chunksToLoad.Add(currentChunk);
                        currentChunk = null;
                    }
                }

                startIndex += findPageSize; // Advance to next page.
            } while (startIndex < totalCount);

            // make sure the last chunk gets added
            if (currentChunk != null)
            {
                chunksToLoad.Add(currentChunk);
                currentChunk = null;
            }

            // now do the load in parallel
            Parallel.ForEach(chunksToLoad, chunk =>
            {
                if (fullLoad)
                    AFElement.LoadElements(chunk);
                else
                    AFElement.LoadAttributes(chunk, new[] { attributeTemplate });
            });

            AFAttributeList attributeList = new AFAttributeList();
            foreach (var element in chunksToLoad.SelectMany(elements => elements))
            {
                var attribute = element.Attributes[attributeTemplate.Name];
                if (attribute != null)
                    attributeList.Add(attribute);
            }

            return attributeList;
        }
    }
}
