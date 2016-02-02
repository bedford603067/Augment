using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class Asset
    {
        #region Public Properties (outside XSD schema)

        private SerializableHashTable _ExtendedProperties;
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        [System.Runtime.Serialization.DataMemberAttribute()]
        public SerializableHashTable ExtendedProperties
        {
            get
            {
                if (_ExtendedProperties == null)
                {
                    _ExtendedProperties = new SerializableHashTable();
                }
                return _ExtendedProperties;
            }
            set
            {
                _ExtendedProperties = value;
            }
        }

        #endregion

        public class SearchByDescriptionClass
        {
            string _description;

            public SearchByDescriptionClass(string description)
            {
                this._description = description;
            }

            public bool PredicateDelegate(Asset memberObject)
            {
                return memberObject.Description.ToUpper() == _description.ToUpper();
            }
        }

    }
}
