using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;

namespace BusinessObjects.WorkManagement
{
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://FinalBuild.co.uk/BusinessObjects.WorkManagement")]
    [System.Runtime.Serialization.DataContractAttribute()]
    public class ActivityTree : BusinessObjects.Base
    {
        private XmlElement mobjCategoryHierarchy;
        private ActivityCollection mColActivities;

        /// <summary>
        /// Note. XmlElement can be exposed over WCF, XmlDocument cannot
        /// </summary>
        [System.Runtime.Serialization.DataMemberAttribute()]
        public XmlElement CategoryHierarchy
        {
            get
            {
                return mobjCategoryHierarchy;
            }
            set
            {
                mobjCategoryHierarchy = value;
            }
        }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public ActivityCollection Activities
        {
            get
            {
                return mColActivities;
            }
            set
            {
                mColActivities = value;
            }
        }
    }
}
