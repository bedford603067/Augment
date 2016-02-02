#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#endregion

// Note. ContractNamespace is specified in MobileWebService.cs
//[assembly: System.Runtime.Serialization.ContractNamespaceAttribute("urn:BusinessObjects", ClrNamespace = "BusinessObjects")]

namespace BusinessObjects
{
	#region SaveAction Enum

	public enum SaveAction
	{
		None = 0,
		Insert = 1,
		Update = 2,
		Delete = 4
	}

	#endregion

	#region Base class

	/// <summary>
	/// Summary description for Base.
	/// </summary>
	[Serializable]
    [System.Runtime.Serialization.DataContract()]
	public class Base
	{
		public Base()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		#region Serialization

		public static XmlSerializer[] GetSerializers(System.Reflection.Assembly callingAssembly)
		{
			Type[] arrTypes = null;
			XmlSerializer[] arrSerializers = null;
			ArrayList colTypes = new ArrayList();

			foreach (Type objType in callingAssembly.GetTypes())
			{
				if (objType.IsSerializable == true)
				{
					colTypes.Add(objType);
				}
			}
			arrTypes = (Type[])colTypes.ToArray(typeof(Type));
			arrSerializers = XmlSerializer.FromTypes(arrTypes);

			return arrSerializers;
		}

		public XmlSerializer[] GetSerializers()
		{
			Type[] arrTypes = null;
			XmlSerializer[] arrSerializers = null;
			ArrayList colTypes = new ArrayList();

			foreach (Type objType in this.GetType().Assembly.GetTypes())
			{
				if (objType.IsSerializable == true)
				{
					colTypes.Add(objType);
				}
			}
			arrTypes = (Type[])colTypes.ToArray(typeof(Type));
			arrSerializers = XmlSerializer.FromTypes(arrTypes);

			return arrSerializers;
		}

		/// <summary>
		/// Serialize class instance to XML string
		/// </summary>
		public string Serialize()
		{
			XmlSerializer objSerializer = null;
			MemoryStream objStream = null;
			StreamWriter objWriter = null;
			StreamReader  objReader = null;
			string strXML = "";

			try
			{
				objStream = new MemoryStream();
				objWriter = new StreamWriter(objStream);

				objSerializer=new XmlSerializer(this.GetType());
				objSerializer.Serialize(objWriter,this);
				objWriter.Flush();
				objWriter.BaseStream.Seek(0,SeekOrigin.Begin);
				objReader = new StreamReader(objWriter.BaseStream);
				strXML = objReader.ReadToEnd();
			}
			finally
			{
				if (objReader != null)
				{
					objWriter.Close();
				}
				objReader = null;
				if (objWriter != null)
				{
					objWriter.Close();
				}
				objWriter = null;
			}

			return strXML;
		}

		/// <summary>
		/// Serialize a Class instance of given type to XML 
		/// </summary>
		public static string Serialize(Type classType, object classInstance)
		{
			XmlSerializer objSerializer = null;
			MemoryStream objStream = null;
			StreamWriter objWriter = null;
			StreamReader  objReader = null;
			string strXML = "";

			try
			{
				objStream = new MemoryStream();
				objWriter = new StreamWriter(objStream);

				objSerializer = new XmlSerializer(classType);
				objSerializer.Serialize(objWriter, classInstance);
				objWriter.Flush();
				objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
				objReader = new StreamReader(objWriter.BaseStream);
				strXML = objReader.ReadToEnd();
			}
			catch(Exception exception)
			{
				strXML = "EXCEPTION: " + exception.Message;
                Exception e2 = exception.InnerException;
                while (e2 != null)
                {
                    strXML += Environment.NewLine;
                    strXML += e2.Message;
                    e2 = e2.InnerException;
                }
			}
			finally
			{
				if (objReader != null)
				{
					objWriter.Close();
				}
				objReader = null;
				if (objWriter != null)
				{
					objWriter.Close();
				}
				objWriter = null;
			}

			return strXML;
		}

		/// <summary>
		/// Serialize class instance to XML and save to file
		/// </summary>
		public void Serialize(string outputDirectory)
		{
			XmlDocument objXML = null;
			string strXML = "";
			string strOutputFile = null;

			try
			{
				if (outputDirectory.Substring(outputDirectory.Length-1,1) != @"\")
				{
					outputDirectory+=@"\";
				}
				strOutputFile=outputDirectory + this.GetType().Name + ".xml";
				strXML = Serialize();
				objXML = new XmlDocument();
				objXML.LoadXml(strXML);
				objXML.Save(strOutputFile);
			}
			finally
			{
				objXML = null;
			}
		}

        /// <summary>
        /// Serialize a Class instance of given type to XML and save to file with an additional filename suffix
        /// </summary>
        public static void Serialize(Type classType, object classInstance, string outputDirectory, string fileSuffix)
        {
            XmlDocument objXML = null;
            string strXML = "";
            string strOutputFile = null;

            try
            {
                if (outputDirectory.Substring(outputDirectory.Length - 1, 1) != @"\")
                {
                    outputDirectory += @"\";
                }
                strOutputFile = outputDirectory + classType.Name + fileSuffix + ".xml";
                strXML = Serialize(classType, classInstance);
                objXML = new XmlDocument();
                objXML.LoadXml(strXML);
                objXML.Save(strOutputFile);
            }
            finally
            {
                objXML = null;
            }
        }

        /// <summary>
        /// Serialize a Class instance of given type to XML and save to specific named files
        /// </summary>
        public static void Serialize(Type classType, object classInstance, string outputDirectory, string specificName, int dummy1)
        {
            XmlDocument objXML = null;
            string strXML = "";
            string strOutputFile = null;

            try
            {
                if (outputDirectory.Substring(outputDirectory.Length - 1, 1) != @"\")
                {
                    outputDirectory += @"\";
                }
                strOutputFile = outputDirectory + specificName + ".xml";
                strXML = Serialize(classType, classInstance);
                objXML = new XmlDocument();
                objXML.LoadXml(strXML);
                objXML.Save(strOutputFile);
            }
            finally
            {
                objXML = null;
            }
        }

        /// <summary>
        /// Serialize class instance to XML 
        /// Appropriate XmlSerializer already initialised for the Type(s) to be serialized
        /// </summary>
        /// <param name="objSerializer"></param>
        /// <param name="classInstance"></param>
        /// <returns></returns>
        public static string Serialize(XmlSerializer objSerializer, object classInstance)
        {
            MemoryStream objStream = null;
            StreamWriter objWriter = null;
            StreamReader objReader = null;
            string strXML = "";

            try
            {
                objStream = new MemoryStream();
                objWriter = new StreamWriter(objStream);

                objSerializer.Serialize(objWriter, classInstance);
                objWriter.Flush();
                objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                objReader = new StreamReader(objWriter.BaseStream);
                strXML = objReader.ReadToEnd();
            }
            finally
            {
                if (objReader != null)
                {
                    objWriter.Close();
                }
                objReader = null;
                if (objWriter != null)
                {
                    objWriter.Close();
                }
                objWriter = null;
            }

            return strXML;
        }

		/// <summary>
        /// Populate a Class instance by Deserializing from Xml Document 
		/// Appropriate XmlSerializer already initialised for the Type(s) to be deserialized
		/// </summary>
		public static object Deserialize(XmlSerializer typeSerializer, XmlDocument serializedClass)
		{
			object objClassInstance = null;
			StreamReader objReader = null;
			StreamWriter objWriter = new StreamWriter(new MemoryStream());

			objWriter.Write(serializedClass.OuterXml);
			objWriter.Flush();
			objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
			objReader = new StreamReader(objWriter.BaseStream);
			objClassInstance = typeSerializer.Deserialize(objReader);
			objReader.Close();
			objReader = null;
			objWriter.Close();

			return objClassInstance;
		}

		/// <summary>
		/// Populate a Class instance by Deserializing from Xml Document
		/// </summary>
		public static object Deserialize(Type classType, XmlDocument serializedClass)
		{
			object objClassInstance = null;
			XmlSerializer objSerializer = null;
			StreamReader objReader = null;
			StreamWriter objWriter = new StreamWriter(new MemoryStream());

			objWriter.Write(serializedClass.OuterXml);
			objWriter.Flush();
			objWriter.BaseStream.Seek(0,SeekOrigin.Begin);
			objReader = new StreamReader(objWriter.BaseStream);
			objSerializer = new XmlSerializer(classType);
			objClassInstance = objSerializer.Deserialize(objReader);
			objReader.Close();
			objReader = null;
			objWriter.Close();

			return objClassInstance;
		}

		/// <summary>
		/// Populate a Class instance by Deserializing from a file
		/// </summary>
		public static object Deserialize(Type classType, string instanceFilePath)
		{
			object objClassInstance = null;
			XmlSerializer objSerializer = null;
			StreamReader objReader = null;

			if (File.Exists(instanceFilePath) == true)
			{
				objReader = new StreamReader(instanceFilePath);
				objSerializer = new XmlSerializer(classType);
				objClassInstance = objSerializer.Deserialize(objReader);
				objReader.Close();
				objReader = null;
			}
			return objClassInstance;
		}

		/// <summary>
		/// Populate a Class instance by Deserializing from a file
		/// </summary>
        public static object Deserialize(XmlSerializer typeSerializer, string instanceFilePath)
		{
			object objClassInstance = null;
			StreamReader objReader = null;

			if (File.Exists(instanceFilePath) == true)
			{
				objReader = new StreamReader(instanceFilePath);
				objClassInstance = typeSerializer.Deserialize(objReader);
				objReader.Close();
				objReader = null;
			}
			return objClassInstance;
		}

        public static string RemoveXmlNamespaces(string serialized)
        {
            //  Strip out characters that we cant handle in Deserialize
            string replaceString = string.Empty;

            //replaceString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine;
            //serialized = serialized.Replace(replaceString, string.Empty);
            replaceString = " xmlns=\"http://FinalBuild.co.uk/BusinessObjects.Inspections\"";
            serialized = serialized.Replace(replaceString, string.Empty);
            replaceString = " xmlns=\"http://FinalBuild.co.uk/BusinessObjects.WorkManagement\"";
            serialized = serialized.Replace(replaceString, string.Empty);
            replaceString = " xmlns=\"http://FinalBuild.co.uk/BusinessObjects.AssetMaintenance\"";
            serialized = serialized.Replace(replaceString, string.Empty);
            replaceString = " xmlns=\"http://FinalBuild.co.uk/BusinessObjects.MeterReading\"";
            serialized = serialized.Replace(replaceString, string.Empty);
            //replaceString = " xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"";
            //serialized = serialized.Replace(replaceString, string.Empty);
            //replaceString = "xsi:nil=\"true\"";
            //serialized = serialized.Replace(replaceString, string.Empty);
            //replaceString = " xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"";
            //serialized = serialized.Replace(replaceString, string.Empty);

            return serialized;
        }

		public static byte[] BinarySerialize(object classInstance)
        {
            IFormatter objFormatter;
            MemoryStream objStream = null;
            byte[] objBuffer = null;

            objFormatter = new BinaryFormatter();
            objStream = new MemoryStream();
            objFormatter.Serialize(objStream, classInstance);
            objBuffer = objStream.ToArray();
            objStream.Close();

            return objBuffer;
        }

        public static object BinaryDeserialize(byte[] binaryStream)
        {
            IFormatter objFormatter;
            MemoryStream objStream = new MemoryStream();
            object objClassInstance = null;
            BinaryWriter objWriter = new BinaryWriter(objStream);

            objFormatter = new BinaryFormatter();
            objWriter.Write(binaryStream, 0, binaryStream.Length);
            objWriter.Flush();
            objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            objClassInstance = objFormatter.Deserialize(objWriter.BaseStream);
            objWriter.Close();

            return objClassInstance;
        }

		public XmlSchema ToXmlSchema()
		{
			XmlSchema objSchema =null;
			Type objType = this.GetType();
			XmlReflectionImporter objImporter = new XmlReflectionImporter();
			XmlSchemas colSchemata = new XmlSchemas();
			XmlSchemaExporter objExporter = new XmlSchemaExporter(colSchemata);

			XmlTypeMapping objTypeMapping = objImporter.ImportTypeMapping(objType);
			objExporter.ExportTypeMapping(objTypeMapping);
			objSchema = colSchemata[0];

			return objSchema;
		}

		public static XmlSchema ToXmlSchema(object classInstance,string defaultNamespace,Type[] schemaTypes)
		{
			XmlSchema objSchema =null;
			XmlTypeMapping objTypeMapping = null;
			XmlSchemas colSchemata = new XmlSchemas();

			XmlReflectionImporter objImporter = new XmlReflectionImporter(defaultNamespace);
			XmlSchemaExporter objExporter = new XmlSchemaExporter(colSchemata);

			if (schemaTypes != null)
			{
				// Assume that Type of class instance is included in schemaTypes
				foreach(Type objSchemaType in schemaTypes)
				{
					objTypeMapping = objImporter.ImportTypeMapping(objSchemaType);
					objExporter.ExportTypeMapping(objTypeMapping);
				}
			}
			else
			{
				Type objType = classInstance.GetType();
				objTypeMapping = objImporter.ImportTypeMapping(objType);
				objExporter.ExportTypeMapping(objTypeMapping);
			}

			objSchema = colSchemata[0];

			return objSchema;
		}

		public DataSet ToDataSet()
		{
			DataSet dsType = new DataSet();
			MemoryStream objStream = new MemoryStream();
			StreamWriter objWriter = new StreamWriter(objStream);

			// Read Xml Schema
			this.ToXmlSchema().Write(objWriter);
			objWriter.Flush();
			objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
			dsType.ReadXmlSchema(objStream);
			objWriter.Close();

			// Read Data
			string strXML = this.Serialize();
			objStream = new MemoryStream();
			objWriter = new StreamWriter(objStream);
			objWriter.Write(strXML);
			objWriter.Flush();
			objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
			dsType.ReadXml(objStream);
			objWriter.Close();

			return dsType;
		}

		public static DataSet ToDataSet(object classInstance,string defaultNamespace,Type[] schemaTypes,bool useSchema)
		{
			DataSet dsType = new DataSet();
			MemoryStream objStream = new MemoryStream();
			StreamWriter objWriter = new StreamWriter(objStream);

            // Always InferSchema at present. The ReadSchema mode is not working correctly
            useSchema = false;
            
            // useSchema=true: Generate Xml Schema and apply to new DataSet
            // Alternatively all columns of all tables of the DataSet will be type "String"
            if (useSchema == true)
            {
                ToXmlSchema(classInstance, defaultNamespace, schemaTypes).Write(objWriter);
                objWriter.Flush();
                objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                dsType.ReadXmlSchema(objStream);
                objWriter.Close();
            }

			// Read Data
			string strXML = Serialize(classInstance.GetType(),classInstance);
			objStream = new MemoryStream();
			objWriter = new StreamWriter(objStream);
			objWriter.Write(strXML);
			objWriter.Flush();
			objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
            if (useSchema==false)
            {
                dsType.ReadXml(objStream, XmlReadMode.InferSchema);
            }
            else
            {
                dsType.ReadXml(objStream, XmlReadMode.ReadSchema);
            }
			objWriter.Close();

			return dsType;

			/*
			// Usage
			DataSet dsData = new DataSet();
			Type[] arrTypes = null;
			ArrayList colTypes = new ArrayList();
			string strProxyNamespace = "PROXY_NS";

			Type[] arrExportTypes=SOME_OBJECT.GetType().Assembly.GetExportedTypes();
			foreach(Type objType in arrExportTypes)
			{
				if (objType.Namespace==strProxyNamespace && objType.IsSerializable==true)
				{
					colTypes.Add(objType);
				}
			}
			arrTypes=(Type[])colTypes.ToArray(typeof(Type));
			dsData=BusinessObjects.Base.ToDataSet(SOME_OBJECT,strProxyNamespace,arrTypes);
			*/
		}

		public byte[] ToBuffer()
		{
			IFormatter objFormatter;
			MemoryStream objStream = null;
			byte[] objBuffer = null;

			objFormatter = new BinaryFormatter();
			objStream = new MemoryStream();
			objFormatter.Serialize(objStream, this);
			objBuffer = objStream.ToArray();
			objStream.Close();

			return objBuffer;
		}

		public MemoryStream ToStream()
		{
			IFormatter objFormatter;
			MemoryStream objStream = null;

			objFormatter = new BinaryFormatter();
			objStream = new MemoryStream();
			objFormatter.Serialize(objStream, this);

			return objStream;
		}

        public XmlDocument ToXml()
        {
            XmlDocument serialized = null;

            try
            {
                string xml = this.Serialize();
                serialized = new XmlDocument();
                serialized.LoadXml(xml);
            }
            catch
            {
                // So be it, class is not Xml Serializable
            }

            return serialized;
        }

        public static object GetInstanceFromObjectPath(object parentObject, string objectPath)
        {
            object targetObject = null;
            object objectToSearch = parentObject;
            string objectPathToSearchFor = objectPath;
            string rootObjectName = objectPathToSearchFor.Substring(0, objectPathToSearchFor.IndexOf("."));
            string targetPropertyName = objectPathToSearchFor.Substring(objectPathToSearchFor.LastIndexOf(".") + 1);

            if (parentObject.GetType().Name == rootObjectName ||
               (parentObject.GetType().BaseType != null &&
                parentObject.GetType().BaseType.Name == rootObjectName))
            {
                targetObject = SearchTypePropertiesRecursively(objectToSearch, objectPathToSearchFor.Substring(objectPathToSearchFor.IndexOf(".") + 1), targetPropertyName);
            }

            return targetObject;
        }

        private static object SearchTypePropertiesRecursively(object objectToSearch, string objectPathToSearchFor, string targetPropertyName)
        {
            object targetObject = null;
            object matchedObject = null;
            SortedList<string, object> members = new SortedList<string, object>();
            string nextPropertyName = null;

            if (objectPathToSearchFor.Length == 0)
            {
                return targetObject;
            }

            if (objectPathToSearchFor.IndexOf(".") > 0)
            {
                nextPropertyName = objectPathToSearchFor.Substring(0, objectPathToSearchFor.IndexOf("."));
            }
            else
            {
                nextPropertyName = objectPathToSearchFor;
            }


            if (nextPropertyName.IndexOf("()") > -1)
            {
                foreach (System.Reflection.MethodInfo methodInfo in objectToSearch.GetType().GetMethods())
                {
                    if (methodInfo.Name == nextPropertyName.Replace("()", string.Empty) &&
                        methodInfo.ReturnType != null &&
                        methodInfo.GetParameters().Length == 0)
                    {
                        matchedObject = methodInfo.Invoke(objectToSearch, null);
                        if (matchedObject != null)
                        {
                            members.Add(nextPropertyName, matchedObject);
                        }

                        break;
                    }
                }
            }
            else
            {
                foreach (System.Reflection.PropertyInfo propertyInfo in objectToSearch.GetType().GetProperties())
                {
                    if (propertyInfo.PropertyType.IsPublic &&
                        // Exclude properties of Base class
                        propertyInfo.Name != "IsDirty" &&
                        propertyInfo.Name != "IsNew" &&
                        propertyInfo.Name != "IsMarkedForDeletion")
                    {
                        if (propertyInfo.Name == nextPropertyName || 
                           (nextPropertyName.StartsWith("Item[" + "\"") && propertyInfo.Name == "Item"))
                        {
                            switch (propertyInfo.Name)
                            {
                                case "Item":
                                    {
                                        // Collection with String Indexer
                                        if (nextPropertyName.StartsWith("Item[" + "\""))
                                        {
                                            string indexer = nextPropertyName.Replace("\"", string.Empty);
                                            indexer = indexer.Replace("Item[",string.Empty);
                                            indexer = indexer.Replace("]",string.Empty);

                                            if (propertyInfo.GetGetMethod(true).GetParameters().Length > 0)
                                            {
                                                matchedObject = propertyInfo.GetValue(objectToSearch, new string[] { indexer });
                                            }
                                        }
                                        else
                                        {
                                            // Standard Collection
                                            if (objectToSearch.GetType().BaseType.IsGenericType)
                                            {
                                                Type[] containedTypes = objectToSearch.GetType().BaseType.GetGenericArguments();
                                                Type containedType = containedTypes[0];
                                                System.Collections.IList list = (System.Collections.IList)objectToSearch;
                                                if (list.Count == 0)
                                                {
                                                    list.Add(Activator.CreateInstance(containedTypes[0]));
                                                }
                                                matchedObject = list[0];
                                            }
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        // Singles
                                        matchedObject = propertyInfo.GetValue(objectToSearch, null);
                                        if (matchedObject == null)
                                        {
                                            propertyInfo.SetValue(objectToSearch, Activator.CreateInstance(propertyInfo.PropertyType), null);
                                            matchedObject = propertyInfo.GetValue(objectToSearch, null);
                                        }
                                        break;
                                    }
                            }
                            if (matchedObject == null)
                            {
                                return null;
                            }
                            members.Add(propertyInfo.Name, matchedObject);

                            // Recurse down if not a PropertyInfo is not a System type
                            if (propertyInfo.PropertyType.GetProperties().Length > 0 &&
                                propertyInfo.PropertyType.Name != "RuntimePropertyInfo") //Namespace.IndexOf("System") < 0)
                            {
                                if (objectPathToSearchFor.IndexOf(".") > 0)
                                {
                                    objectPathToSearchFor = objectPathToSearchFor.Substring(objectPathToSearchFor.IndexOf(".") + 1);
                                    targetObject = SearchTypePropertiesRecursively(
                                                    matchedObject, objectPathToSearchFor, targetPropertyName);
                                }
                            }

                            break;
                        }
                    }
                }
            }

            for (int index = 0; index < members.Keys.Count; index++)
            {
                if (members.Keys[index] == targetPropertyName)
                {
                    targetObject = members.Values[index];
                    break;
                }
            }

            return targetObject;
        }

		#endregion

		#region Database Concurrency Check support

		private int _lastUpdateCount;
		private const string _dbConcurrencyParameterName = "@LstUpdCnt";

		protected void SetDbConcurrencyValue(int value)
		{
			this._lastUpdateCount = value;
		}

		protected int GetDbConcurrencyValue()
		{
			return this._lastUpdateCount;
		}

		protected void SetDbConcurrencyValue(SqlParameter[] parameters)
		{
			//search from the back as it is probably the last parameter
			for(int index = parameters.Length - 1; index >= 0; index--)
			{
				SqlParameter paramFound = parameters[index];
				if(paramFound.ParameterName == _dbConcurrencyParameterName)
				{
					this._lastUpdateCount = (int)paramFound.Value;
					return;
				}
			}

			throw new Exception(string.Format("The parameters collection does not contain the {0} parameter.", _dbConcurrencyParameterName));
		}

		internal SqlParameter CreateUserNameParameter()
		{
			return Utilities.CreateUserNameParameter();
		}


		internal SqlParameter CreateDbConcurrencyParameter(SaveAction saveAction)
		{
			System.Diagnostics.Debug.Assert(saveAction == SaveAction.Insert || saveAction == SaveAction.Update || saveAction == SaveAction.Delete, "saveAction is .Insert or .Update or .Delete");

			SqlParameter returnParameter = null;
			if(saveAction == SaveAction.Insert)
			{
				returnParameter = CreateDbConcurrencyInsertParameter();
			}
			else if(saveAction == SaveAction.Update)
			{
				returnParameter = CreateDbConcurrencyUpdateParameter();
			}
			else if(saveAction == SaveAction.Delete)
			{
				returnParameter = CreateDbConcurrencyDeleteParameter();
			}
			return returnParameter;
		}

		protected SqlParameter CreateDbConcurrencyInsertParameter()
		{
			return new SqlParameter(_dbConcurrencyParameterName, System.Data.SqlDbType.Int, 4, System.Data.ParameterDirection.Output, 10, 0, null, System.Data.DataRowVersion.Current, false, null, "", "", "");
		}

		protected SqlParameter CreateDbConcurrencyUpdateParameter()
		{
			return new SqlParameter(_dbConcurrencyParameterName, System.Data.SqlDbType.Int, 4, System.Data.ParameterDirection.InputOutput, 10, 0, null, System.Data.DataRowVersion.Current, false, _lastUpdateCount, "", "", "");
		}

		protected SqlParameter CreateDbConcurrencyDeleteParameter()
		{
			return new SqlParameter(_dbConcurrencyParameterName, System.Data.SqlDbType.Int, 4, System.Data.ParameterDirection.Input, 10, 0, null, System.Data.DataRowVersion.Current, false, _lastUpdateCount, "", "", "");
		}

		#endregion

		#region IsDirty

		private bool _isDirty = false;

        [XmlIgnore]
		public virtual bool IsDirty
		{
			get { return this._isDirty; }
			set { this._isDirty = value; }
		}

		internal void MarkDirty()
		{
			this._isDirty = true;
		}

		internal void MarkClean()
		{
			this._isDirty = false;
		}

		protected bool AssignChange<T>(ref T existingValue, T newValue)
		{
			bool isDifferent;

			if(existingValue == null && newValue != null)
			{
				isDifferent = true;
			}
			else if(existingValue != null && newValue == null)
			{
				isDifferent = true;
			}
			else if(existingValue == null && newValue == null)
			{
				isDifferent = false;
			}
			else if(existingValue.Equals(newValue))
			{
				isDifferent = false;
			}
			else
			{
				isDifferent = true;
			}

			if(isDifferent)
			{
				existingValue = newValue;
				MarkDirty();
				this.OnUpdated(new System.EventArgs());
				isDifferent = true;
			}

			return isDifferent;
		}

		#endregion

		#region IsNew

		private bool _isNew = true;

        [XmlIgnore]
		public bool IsNew
		{
			get { return this._isNew; }
			set { this._isNew = value; }
		}

		#endregion

		#region Deletion

		private bool _isMarkedForDeletion = false;

		public bool IsMarkedForDeletion
		{
			get { return this._isMarkedForDeletion; }
		}

		public virtual void MarkForDeletion()
		{
			this._isMarkedForDeletion = true;
			this._isDirty = true;
		}

		#endregion

		#region SaveAction

		public SaveAction GetSaveAction()
		{
			SaveAction action;
			if(IsNew && IsMarkedForDeletion) { action = SaveAction.None; }
			else if(IsNew) { action = SaveAction.Insert; }
			else if(IsMarkedForDeletion) { action = SaveAction.Delete; }
			else if(IsDirty) { action = SaveAction.Update; }
			else { action = SaveAction.None; }
			return action;
		}
		#endregion

		#region Clone

		public virtual object Clone()
		{
			using(System.IO.MemoryStream buffer = new System.IO.MemoryStream())
			{
				System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
				formatter.Serialize(buffer, this);
				buffer.Position = 0;
				object tempCopy = formatter.Deserialize(buffer);
				return tempCopy;
			}
		}

        public virtual object Clone(bool cloneAsXML)
        {
            if (cloneAsXML)
            {
                //XML Version
                object newInstance = null;
                System.Xml.XmlDocument serializedInstance = new System.Xml.XmlDocument();
                serializedInstance.LoadXml(this.Serialize());
                newInstance = BusinessObjects.Base.Deserialize(this.GetType(), serializedInstance);
                return newInstance;
            }
            else
            {
                using (System.IO.MemoryStream buffer = new System.IO.MemoryStream())
                {
                    System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    formatter.Serialize(buffer, this);
                    buffer.Position = 0;
                    object tempCopy = formatter.Deserialize(buffer);
                    return tempCopy;
                }
            }
        }

		/// <summary>
		/// Manually invoke XmlSerializer object to Serialize
		/// using Werb Services style serialization
		/// </summary>
		/// <returns>An Xml string containg values of object properties</returns>
		public virtual string GetData()
		{
			using(System.IO.MemoryStream buffer = new System.IO.MemoryStream())
			{
				System.Xml.Serialization.XmlSerializer formatter = new System.Xml.Serialization.XmlSerializer(this.GetType());
				formatter.Serialize(buffer, this);
				buffer.Position = 0;
				return buffer.ToString();
			}
		}

		#endregion

		#region Loading

		//can use this to avoid individual validation when
		//setting properties during the database load of an
		//instance with the aim of calling a full validation
		//check once all properties have been set
		private bool _loading;

		protected bool Loading
		{
			get { return this._loading; }
			set { this._loading = value; }
		}

		#endregion

		#region Delegates and Events

		public virtual event EventHandler<EventArgs> Updated;
		protected virtual void OnUpdated(EventArgs e)
		{
			EventHandler<EventArgs> handler = this.Updated;
			if(handler != null)
			{
				handler(this, e);
			}
		}

		#endregion

		#region Test 

		private void Test()
		{
		//    DataAccess objADO = new DataAccess(@"GREATBEAR\SQL2000", "Test");
		//    SqlParameter[] arrParameters = null;
		//    string strStoredProcedure = "Test";

		//    XmlDocument objXML = objADO.GetXmlDocument(strStoredProcedure, arrParameters, "Product");
		//    Product objProduct = new Product();
		//    objProduct = (Product)Product.Deserialize(objProduct, objXML);
		//    string strXML = objProduct.Serialize();
		}

		#endregion

        #region Validation

        public virtual bool Validate(out ValidationExceptionCollection validationExceptions)
        {
            return ValidationClass.Validate(this, out validationExceptions);
        }

        #endregion

        #region Hashing

        public bool EqualToInstance(Base instanceToCompareWith)
        {
            string thisInstanceHash = this.GetHashValueForInstance();
            string comparisonInstanceHash = instanceToCompareWith.GetHashValueForInstance();

            return thisInstanceHash.Equals(comparisonInstanceHash);
        }

        public string GetHashValueForInstance()
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] hashValue = null;
            string serializedXml = this.ToXml() != null ? this.ToXml().OuterXml : null;
            
            if (!string.IsNullOrEmpty(serializedXml))
            {
                // Creating a Hash using all of the instance Serialized content
                byte[] xmlBytes = encoding.GetBytes(this.ToXml().OuterXml);
                {
                    hashValue = md5.ComputeHash(xmlBytes);
                }
            }
            else
            {
                // Fallback to at least creating a Hash by instance Type
                encoding.GetBytes(this.GetType().Name);
            }

            return BitConverter.ToString(hashValue);
        }

        #endregion
    }

	#endregion

	#region BaseBindingList<T> class

	/// <summary>
	/// Summary description for Base.
	/// </summary>
	[Serializable]
    [CollectionDataContract()]
	public class BaseBindingList<T> : BindingList<T>
	{
		#region Replace

		public void Replace(T oldInstance, T newInstance)
		{
			if(oldInstance == null || newInstance == null || oldInstance.Equals(newInstance))
			{
				return;
			}

			int existingPosition = this.IndexOf(oldInstance);

			if(existingPosition >= 0)
			{   //was in the collection so replace it
				this.SetItem(existingPosition, newInstance);
			}
		}

		#endregion

		#region Sorting

		private bool _isSorted;
		private T[] _unsorteditems;
		private bool _isSortable = true;
		[NonSerialized]
		private PropertyDescriptor _sortProperty;
		private ListSortDirection _sortDirection = ListSortDirection.Descending;
		protected override bool SupportsSortingCore
		{
			get { return _isSortable; }
		}

		protected override ListSortDirection SortDirectionCore
		{
			get { return _sortDirection; }
		}

		public ListSortDirection SortDirection
		{
			get { return SortDirectionCore; }
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get { return _sortProperty; }
		}

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			if(!this._isSorted)
			{
				this._unsorteditems = new T[this.Count];
				this.CopyTo(_unsorteditems, 0);
			}
			// Check to see if the property type we are sorting by implements
			// the IComparable interface.
			//////////Type interfaceType = property.PropertyType.GetInterface("IComparable");

			//////////if(interfaceType != null)
			//////////{
			//////////    ArrayList sortedList = new ArrayList(this.Count);

			//////////    // If so, set the SortPropertyValue and SortDirectionValue.
			//////////    _sortProperty = property;
			//////////    _sortDirection = direction;

			//////////    // Loop through each item, adding it the the sortedItems ArrayList.
			//////////    foreach(T item in this)
			//////////    {
			//////////        sortedList.Add(property.GetValue(item));
			//////////    }
			//////////    // Call Sort on the ArrayList.
			//////////    sortedList.Sort();
			//////////    T temp;

			//////////    // Check the sort direction and then copy the sorted items
			//////////    // back into the list.
			//////////    if(direction == ListSortDirection.Descending)
			//////////    {
			//////////        sortedList.Reverse();
			//////////    }

			//////////    for(int i = 0; i < this.Count; i++)
			//////////    {
			//////////        int position = Find(property.Name, sortedList[i]);
			//////////        if(position >= 0 && position != i)
			//////////        {
			//////////            temp = this[i];
			//////////            this[i] = this[position];
			//////////            this[position] = temp;
			//////////        }
			//////////    }

			//////////    _isSorted = true;

			//////////    // Raise the ListChanged event so bound controls refresh their
			//////////    // values.
			//////////    OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			//////////}
			//////////else
			//////////{
			// Get list to sort
			// Note: this.Items is a non-sortable ICollection<T>
			List<T> items = this.Items as List<T>;

			// Apply and set the sort, if items to sort
			if(items != null)
			{
				PropertyComparer<T> pc = new PropertyComparer<T>(property, direction);
				items.Sort(pc);
				_isSorted = true;
			}
			else
			{
				_isSorted = false;
			}

			_sortProperty = property;
			_sortDirection = direction;

			this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			//////////}
		}

		public void Sort(string property)
		{
			// Check the properties for a property with the specified name.
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
			PropertyDescriptor propertyDescriptor = properties.Find(property, true);
			//If  the collection is already sorted on this field
			//reverse the direction
			if(propertyDescriptor != null)
			{
				if(!this._isSorted || this._sortProperty != propertyDescriptor)
				{
					Sort(property, ListSortDirection.Ascending);
				}
				else// if (this._sortDirection == ListSortDirection.Ascending)
				{
					Sort(property, this._sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
				}
				//else
				//{
				//    RemoveSort();
				//}
			}
		}

		public void Sort(string property, ListSortDirection direction)
		{
			// Check the properties for a property with the specified name.
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
			PropertyDescriptor propertyDescriptor = properties.Find(property, true);

			// If there is not a match, return -1 otherwise pass search to
			// FindCore method.
			if(property != null && propertyDescriptor != null)
			{
				this.ApplySortCore(propertyDescriptor, direction);
			}
		}

		protected override bool IsSortedCore
		{
			get { return _isSorted; }
		}

		protected override void RemoveSortCore()
		{
			// Ensure the list has been sorted.
			if(this._unsorteditems != null)
			{
				// Loop through the unsorted items and reorder the
				// list per the unsorted list.
				if(this._unsorteditems.Length != this.Count)
				{
					for(int i = 0; i < this._unsorteditems.Length; i++)
					{
						PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
						PropertyDescriptor propertyDescriptor = properties["ID"];
						if(propertyDescriptor == null)
						{
							propertyDescriptor = properties["Id"];
						}
						if(propertyDescriptor == null)
						{
							propertyDescriptor = properties["Reference"];
						}
						if(propertyDescriptor == null)
						{
							propertyDescriptor = properties["Name"];
						}
						if(propertyDescriptor == null)
						{
							propertyDescriptor = properties[0];
						}
						if(!(this.Find(propertyDescriptor.Name, this._unsorteditems[i].GetType().GetProperty(propertyDescriptor.Name).GetValue(this._unsorteditems[i], null)) > -1))
						{
							this.Remove((T)this._unsorteditems[i]);
						}
					}
				}
				for(int i = 0; i < this.Count; i++)
				{
					this[i] = (T)this._unsorteditems[i];
				}
				this._isSorted = false;
				OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			}
		}

		public void RemoveSort()
		{
			RemoveSortCore();
		}


		#endregion

		#region Searching

		protected override bool SupportsSearchingCore
		{
			get { return true; }
		}

		protected override int FindCore(PropertyDescriptor propertyDescriptor, object key)
		{
			if(propertyDescriptor == null)
			{
				return -1;
			}
			else
			{
				// Get the property info for the specified property
				System.Reflection.PropertyInfo propertyInfo = typeof(T).GetProperty(propertyDescriptor.Name);
				T item;

				if(key != null)
				{
					// Loop through the items to see if the key
					// value matches the property value.
					for(int i = 0; i < Count; ++i)
					{
						item = (T)Items[i];
						if(propertyInfo.GetValue(item, null).Equals(key))
							return i;
					}
				}
			}
			return -1;
		}

		public int Find(string property, object key)
		{
			// Check the properties for a property with the specified name.
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
			PropertyDescriptor prop = properties.Find(property, true);

			// If there is not a match, return -1 otherwise pass search to
			// FindCore method.
			if(prop == null)
			{
				return -1;
			}
			else
			{
				return FindCore(prop, key);
			}
		}

		#endregion
	}

	#endregion

	#region PropertyComparer Class
	// The following code contains code implemented by Rockford Lhotka:
	// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnadvnet/html/vbnet01272004.asp

	[System.Serializable()]
	public class PropertyComparer<T> : System.Collections.Generic.IComparer<T>
	{
		#region Private Member Variables

		private PropertyDescriptor _property;
		private ListSortDirection _direction;

		#endregion

		#region Properties

		public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
		{
			_property = property;
			_direction = direction;
		}

		#endregion

		#region IComparer<T>

		public int Compare(T x, T y)
		{
			// Get property values
			object xValue = GetPropertyValue(x, _property.Name);
			object yValue = GetPropertyValue(y, _property.Name);

			// Determine sort order
			if(_direction == ListSortDirection.Ascending)
			{
				return CompareAscending(xValue, yValue);
			}
			else
			{
				return CompareDescending(xValue, yValue);
			}
		}

		public bool Equals(T x, T y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(T obj)
		{
			return obj.GetHashCode();
		}

		#endregion

		#region Comparisons

		// Compare two property values of any type
		private int CompareAscending(object xValue, object yValue)
		{
			int result;
			try
			{
				// If values implement IComparer
				if(xValue is IComparable)
				{
					result = ((IComparable)xValue).CompareTo(yValue);
				}
				// If values don't implement IComparer but are equivalent
				else if(xValue.Equals(yValue))
				{
					result = 0;
				}
				// Values don't implement IComparer and are not equivalent, so compare as string values
				else
					result = xValue.ToString().CompareTo(yValue.ToString());
			}
			catch(Exception exc)
			{
				if(exc is NullReferenceException)
				{
					if(xValue == null && yValue == null)
					{
						result = 0;
					}
					else if(xValue == null && yValue != null)
					{
						result = -1;
					}
					else
					{
						result = 1;
					}
				}
				else
				{
					throw;
				}
			}

			// Return result
			return result;
		}

		private int CompareDescending(object xValue, object yValue)
		{
			// Return result adjusted for ascending or descending sort order ie
			// multiplied by 1 for ascending or -1 for descending
			return CompareAscending(xValue, yValue) * -1;
		}

		private object GetPropertyValue(T value, string property)
		{
			// Get property
			PropertyInfo propertyInfo = value.GetType().GetProperty(property);

			// Return value
			return propertyInfo.GetValue(value, null);
		}

		#endregion
	}

	#endregion

	#region Utilities Class

	internal class Utilities
	{
		#region Transaction helpers

		/// <summary>
		/// Single place to set default transaction properties
		/// Creates a Required transaction scope.
		/// With an Isolation Level of ReadCommitted
		/// </summary>
		/// <returns></returns>
		public static System.Transactions.TransactionScope CreateTransactionScope()
		{
			return CreateTransactionScope(System.Transactions.TransactionScopeOption.Required);
		}

		/// <summary>
		/// Create a transaction scope of specified TransactionScope
		/// Sets the isolation level to ReadCommitted
		/// </summary>
		/// <returns></returns>
		public static System.Transactions.TransactionScope CreateTransactionScope(System.Transactions.TransactionScopeOption tranScopeOption)
		{

			System.Transactions.TransactionOptions tranOption = new System.Transactions.TransactionOptions();
			tranOption.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;

			System.Transactions.TransactionScope scope = new System.Transactions.TransactionScope(tranScopeOption, tranOption);
			//Console.WriteLine(string.Format("Transaction Id={0}, DistId={1}", System.Transactions.Transaction.Current.TransactionInformation.LocalIdentifier, System.Transactions.Transaction.Current.TransactionInformation.DistributedIdentifier));
			return scope;
		}

		#endregion

		#region Database Helpers

		internal static SqlParameter CreateUserNameParameter()
		{
			//extract just the last part of the account name 
			//eg if "abc\cde" we want just "cde"
			//(I tried using LastIndexOf but it missed the last backslash in the pair) 
			string[] userNameSection = ApplicationContext.User.Identity.Name.Split('\\');
			string userName = userNameSection[userNameSection.Length - 1];

			return new SqlParameter("@LstUpdUsr", userName);
		}

		internal static SqlParameter CreateReturnValueSQLParameter()
		{
			SqlParameter returnParameter = new SqlParameter("@RETURN_VALUE", System.Data.SqlDbType.Int, 4, System.Data.ParameterDirection.ReturnValue, 10, 0, null, System.Data.DataRowVersion.Current, false, null, "", "", "");
			return returnParameter;
		}

		#endregion
	}

	#endregion

	#region ApplicationContext Class

	public static class ApplicationContext
	{
		#region Public Methods

		#region Static Methods

		/// <summary>
		/// Get/Set a property to hold the user's Principal.
		/// If running under IIS we use the HttpContext, otherwise
		/// the Thread
		/// </summary>
		public static IPrincipal User
		{
			get
			{
				if(HttpContext.Current != null)
				{
					return HttpContext.Current.User;
				}
				else
				{
					return Thread.CurrentPrincipal;
				}
			}
			set
			{
				if(HttpContext.Current != null)
				{
					HttpContext.Current.User = value;
				}
				else
				{
					Thread.CurrentPrincipal = value;
				}
			}
		}

		#endregion

		#endregion
	}

	#endregion
}

