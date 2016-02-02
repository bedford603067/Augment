using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects
{
    /// <summary>
    /// Basic implementation of a Hash table like structure 
    /// Only native types are expected for Value else won't serialize "as is"
    /// </summary>
    [System.SerializableAttribute()]
    public class SerializableHashTable : List<KeyValuePair>{}

    /// <summary>
    /// Basic implementation of Key-Value - only native types expected for Value
    /// </summary>
    [Serializable]
    public class KeyValuePair
    {
        private string _Key;
        private object _Value;

        public string Key
        {
            get
            {
                return _Key;
            }
            set
            {
                _Key = value;
            }
        }

        /// <summary>
        /// This is only warranted to support native types when Xml serialized
        /// </summary>
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        public KeyValuePair(string key, object value)
        {
            _Key = key;
            _Value = value;
        }

        public KeyValuePair()
        {
            // Default constructor for Xml Serialization purposes
        }
    }
}
