using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Web;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;

namespace Exercise7
{
    public class AppCheckSecurity
    {
        private WindowsIdentity _identity;
        private PISystem _piSystem;

        public AppCheckSecurity()
        {
            if (HttpContext.Current != null) //WebApp
                _identity = (WindowsIdentity)HttpContext.Current.User.Identity;
            else //ConsoleApp
                _identity = new WindowsIdentity(@"test01@pischool.int");

            PISystems piSystems = new PISystems();
            _piSystem = piSystems.DefaultPISystem;
        }

        public AFAttributeList GetAttributesForIdentity(string databaseName, string elementTemplateName, string attributeTemplateName)
        {
            return GetAttributesForIdentity(databaseName, elementTemplateName, attributeTemplateName, fullLoad: false);
        }

        public AFAttributeList GetAttributesForIdentity(string databaseName, string elementTemplateName, string attributeTemplateName, bool fullLoad)
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

                //Console.WriteLine(String.Format("   Element count: {0}, found by service/admin account: {1}",
                //    elements.Count, WindowsIdentity.GetCurrent().Name));

                foreach (var element in elements)
                {
                    // create a new chunk if needed
                    if (currentChunk == null)
                        currentChunk = new List<AFElement>(loadChunkSize);

                    // add to current chunk
                    if (CheckReadPermission(element, _identity)) //CheckReadPermission
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

        public IList<AFValue> GetValueForIdentity(AFAttributeList attributeList)
        {
            AFValues values = new AFValues();

            AFAttributeList attributesReadData = new AFAttributeList();
            foreach (AFAttribute attr in attributeList)
            {
                if (CheckReadDataPermission(attr, _identity)) //CheckReadDataPermission
                    attributesReadData.Add(attr);
                else //return bad value
                    values.Add(AFValue.CreateSystemStateValue(AFSystemStateCode.AccessDenied, DateTime.Now));
            }
            //var pointResolution = attributesReadData.GetPIPoint(); //resolve PI points in bulk (Caution: don't do this every time).

            values.AddRange(attributesReadData.GetValue());
            return values;
        }
        

        #region CheckSecurityPermission
        //
        static public AFSecurityRights CheckSecurity(AFObject afObj, System.Security.Principal.WindowsIdentity identity)
        {
            IAFSecurable securableItem = FindSecurableObjectOrParentObject(afObj) as IAFSecurable;
            return securableItem.Security.CheckSecurity(identity);
        }
        static public bool CheckReadPermission(AFObject afObj, System.Security.Principal.WindowsIdentity identity)
        {
            if ((CheckSecurity(afObj, identity) & AFSecurityRights.Read) == 0)
            {
                //throw new System.Security.SecurityException("No Read Permission");
                return false;
            }
            else
                return true;
        }
        static public bool CheckReadDataPermission(AFObject afObj, System.Security.Principal.WindowsIdentity identity)
        {
            if ((CheckSecurity(afObj, identity) & AFSecurityRights.ReadData) == 0)
            {
                //throw new System.Security.SecurityException("No Read Permission");
                return false;
            }
            else
                return true;
        }
        static public bool CheckWriteDataPermission(AFObject afObj, System.Security.Principal.WindowsIdentity identity)
        {
            if ((CheckSecurity(afObj, identity) & AFSecurityRights.WriteData) == 0)
            {
                //throw new System.Security.SecurityException("No Read Permission");
                return false;
            }
            else
                return true;
        }
        static public AFObject FindSecurableObjectOrParentObject(AFObject afObj)
        {
            if (afObj is IAFSecurable)
            {
                return afObj;
            }

            AFObject parent = FindSecurableParentObject(afObj);
            if (parent != null)
            {
                return FindSecurableObjectOrParentObject(parent);
            }
            return null;
        }
        static internal AFObject FindSecurableParentObject(AFObject afObj)
        {
            AFObject returnObj = null;

            switch (afObj.Identity)
            {
                case AFIdentity.Attribute:
                    AFAttribute attribute = afObj as AFAttribute;
                    returnObj = attribute.Element;
                    break;
                case AFIdentity.Element:
                    AFElement element = afObj as AFElement;
                    if ((object)element.Database != null && element.Parent.ID == element.Database.ID)
                    {
                        returnObj = element.Database;
                    }
                    else
                    {
                        returnObj = element.Parent;
                    }
                    break;
            }

            return returnObj;
        }
        //
        #endregion
    }
}
