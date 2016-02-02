using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class LookupContainer
    {
        public static string RemoveAllNamespaces(string xmlDocument)
        {
            //  Firstly remove all our known namespace values using simple string.Replace
            xmlDocument = Base.RemoveXmlNamespaces(xmlDocument);
            //  To remove Namespaces we have to recreate the Element from the incoming structure
            //  Load up an XElement with the XML document contents
            System.Xml.Linq.XElement rootElement = System.Xml.Linq.XElement.Parse(xmlDocument);
            System.Xml.Linq.XElement rootElementWithNoNamespace = new System.Xml.Linq.XElement(rootElement.Name.LocalName);
            //  We want to retain Namespace attributes on the root (for LookupData anyway)
            //  Take any existing Namespace (and other attributes) on the top level Element
            foreach (System.Xml.Linq.XAttribute attribute in rootElement.Attributes())
            {
                rootElementWithNoNamespace.Add(attribute);
            }
            if (rootElement.HasElements)
            {
                foreach (System.Xml.Linq.XElement childElement in rootElement.Elements())
                {
                    rootElementWithNoNamespace.Add(RemoveAllNamespaces(childElement));
                }
            }
            return rootElementWithNoNamespace.ToString();
        }

        public static System.Xml.Linq.XElement RemoveAllNamespaces(System.Xml.Linq.XElement rootElement)
        {
            System.Xml.Linq.XElement xElement = new System.Xml.Linq.XElement(rootElement.Name.LocalName);
            if (rootElement.HasAttributes)
            {
                foreach (System.Xml.Linq.XAttribute attribute in rootElement.Attributes())
                {
                    if (!attribute.IsNamespaceDeclaration)
                    {
                        if (rootElement.Name.LocalName == "Category")
                        {
                            xElement.Add(attribute);
                        }
                        else
                        {
                            if (attribute.Name.ToString() == "{http://www.w3.org/2001/XMLSchema-instance}type"
                                || attribute.Name.ToString() == "{http://www.w3.org/2001/XMLSchema-instance}nil")
                            {
                                //  We dont want xsi: attribute
                                //  except for certain types where we will inject our own value
                                if (rootElement.Name.LocalName == "Activity"
                                    && attribute.Name.ToString() == "{http://www.w3.org/2001/XMLSchema-instance}type"
                                    && attribute.Value.EndsWith("RepairAndMaintenanceActivity"))
                                {
                                    attribute.Value = "RepairAndMaintenanceActivity";
                                    xElement.Add(attribute);
                                }
                                if (rootElement.Name.LocalName == "Activity"
                                    && attribute.Name.ToString() == "{http://www.w3.org/2001/XMLSchema-instance}type"
                                    && attribute.Value.EndsWith("CustomerActivity"))
                                {
                                    attribute.Value = "CustomerActivity";
                                    xElement.Add(attribute);
                                }
                            }
                            else
                            {
                                xElement.Add(attribute);
                            }
                        }
                    }
                }
            }
            if (rootElement.HasElements)
            {
                //  Recurse the structure
                foreach (System.Xml.Linq.XElement childElement in rootElement.Elements())
                {
                    xElement.Add(RemoveAllNamespaces(childElement));
                }
            }
            else
            {
                //  Take the current value across
                xElement.Value = rootElement.Value;
            }
            return xElement;
        }
    }
}
