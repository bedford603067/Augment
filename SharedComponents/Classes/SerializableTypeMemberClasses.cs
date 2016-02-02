using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects
{
    public class SerializableAssembly
    {
        public string Name;
        public string FullName;
        public string Location;

        public List<SerializableNamespace> Namespaces;
    }

    public class SerializableNamespace
    {
        public string Name;
        public string QualifiedName;

        public List<SerializableType> Types;

        public class SearchByNameClass
        {
            string mstrName;

            public SearchByNameClass(string name)
            {
                this.mstrName = name;
            }

            public bool PredicateDelegate(SerializableNamespace memberInstance)
            {
                return memberInstance.Name == mstrName;
            }
        }
    }

    public class SerializableType
    {
        public string Name;
        public string QualifiedName;

        public List<SerializableTypeProperty> Properties;
        public List<SerializableTypeMethod> Methods;

        public string Summary;

        public SerializableType() { }

        public SerializableType(string name, string qualifiedName) 
        {
            this.Name = name;
            this.QualifiedName = qualifiedName;
            this.Properties = new List<SerializableTypeProperty>();
            this.Methods = new List<SerializableTypeMethod>();
        }

        public class SearchByNameClass
        {
            string mstrName;

            public SearchByNameClass(string name)
            {
                this.mstrName = name;
            }

            public bool PredicateDelegate(SerializableType memberInstance)
            {
                return memberInstance.Name == mstrName;
            }
        }
    }

    public class SerializableTypeProperty
    {
        public string Name;
        public string TypeName;
        public string ObjectPath;
        public bool IsSystemType = true;
        public bool IsEnum = false;
        public SerializableType Type;
        public SerializableType CollectionType;

        public string Summary;
    }

    public class SerializableTypeMethod
    {
        public string Name;
        public string ReturnTypeName;
        public string ObjectPath;
        public string[] Args;

        public string Summary;
    }
}
