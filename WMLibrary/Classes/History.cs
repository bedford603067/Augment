using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public class HistoricalTaskUpdate
    {
        private System.Xml.XmlDocument _serialized;
        private string _typeName;

        public System.Xml.XmlDocument Serialized
        {
            get
            {
                return _serialized;
            }
            set
            {
                _serialized = value;
            }
        }
        
        public string TypeName
        {
            get
            {
                return _typeName;
            }
            set
            {
                _typeName = value;
            }
        }
    }
}
