using OSIsoft.AF;
using OSIsoft.AF.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise1
{
    /// <summary>
    /// Hints:
    /// AF Attribute List Methods
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/T_OSIsoft_AF_Asset_AFAttributeList.htm"/>
    /// 
    /// Find Elements By Template
    /// <see cref="https://techsupport.osisoft.com/Documentation/PI-AF-SDK/html/Overload_OSIsoft_AF_Asset_AFElement_FindElementsByTemplate.htm"/>
    /// </summary>
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
            AFDatabase database = _piSystem.Databases[databaseName];
            if (database == null)
                throw new InvalidOperationException(String.Format("Database '{0}' not found", databaseName));

            AFElementTemplate elementTemplate = database.ElementTemplates[elementTemplateName];
            if (elementTemplate == null)
                throw new InvalidOperationException(String.Format("Element template '{0}' not found", elementTemplateName));

            AFAttributeTemplate attributeTemplate = elementTemplate.AttributeTemplates[attributeTemplateName];
            if (attributeTemplate == null)
                throw new InvalidOperationException(String.Format("Attribute template '{0}' not found", attributeTemplateName));

            // find all attributes in the given database derived from the "power" attribute on the "meter template" element template
            //   and return them in an AFAttributeList

            AFAttributeList attributeList = new AFAttributeList();

            // *** Fix this code to perform the operation in a more efficient way ***
            Stack<AFElement> elementsToCheck = new Stack<AFElement>(attributeTemplate.Database.Elements);
            while(elementsToCheck.Count > 0)
            {
                AFElement element = elementsToCheck.Pop();

                // use Template.IsTypeOf to handle base template case
                if (element.Template != null && element.Template.IsTypeOf(attributeTemplate.ElementTemplate))
                {
                    foreach (AFAttribute attribute in element.Attributes)
                    {
                        if (attribute.Template == attributeTemplate)
                        {
                            attributeList.Add(attribute);
                        }
                    }
                }

                // add children to stack to examine
                foreach (var childElement in element.Elements)
                {
                    elementsToCheck.Push(childElement);
                }
            }

            return attributeList;
        }
    }
}
