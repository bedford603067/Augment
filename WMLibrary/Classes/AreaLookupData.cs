using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;

namespace BusinessObjects.WorkManagement
{
    public class AreaLookupData
    {
        private XmlDocument _xml;

        public XmlDocument Xml
        {
            get
            {
                return _xml;
            }
        }

        public AreaLookupData(string xmlDocumentFilePath) 
        {
            _xml = new System.Xml.XmlDocument();
            _xml.Load(xmlDocumentFilePath);
        }

        public string[] GetRegions()
        {
            string[] regions = null;
            string xPath = @"//Region";

            XmlNodeList nodes = _xml.SelectNodes(xPath);
            if (nodes.Count > 0)
            {
                regions = new string[nodes.Count];
                for(int index =0; index < nodes.Count; index++)
                {
                    regions[index] = nodes[index].Attributes["Name"].Value;
                }
            }

            return regions;
        }

        public string[] GetDistricts(string regionName)
        {
            string[] districts = null;
            string xPath = string.Format(@"//Region[@Name='{0}']/District", regionName);

            XmlNodeList nodes = _xml.SelectNodes(xPath);
            if (nodes.Count > 0)
            {
                districts = new string[nodes.Count];
                for (int index = 0; index < nodes.Count; index++)
                {
                    districts[index] = nodes[index].Attributes["Name"].Value;
                }
            }

            return districts;
        }
    }
}
