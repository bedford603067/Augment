using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://FinalBuild.co.uk/BusinessObjects.WorkManagement")]
    [System.Runtime.Serialization.DataContractAttribute()]
    public class WorkerAddress
    {
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddressLine1;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddressLine2;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string AddressLine3;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Town;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string PostCode;
    }
}
