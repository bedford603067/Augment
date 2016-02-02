using System;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Messaging;

namespace MultiPurposeService.Public.Abstract
{
	/// <summary>
	/// Summary description for Base.
	/// </summary>
	public abstract class Base:IDisposable
	{
		#region Construct\Finalize

		public Base()
		{
			// InitialiseDelegates();
		}

		~Base()
		{
			Dispose(false);
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}

		protected virtual void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Free other state (managed objects).
			}
			// Free your own state (unmanaged objects).
			// Set large fields to null.
		}

		#endregion

		#endregion 

		#region Parent Class communcation

		/*
			public delegate object ParentCallback(object sender,object Data);
			public delegate void HandleException(object sender,Exception childException);

			public event ParentCallback OnCallback;
			public event HandleException OnException;

			void InitialiseDelegates()
			{
				// OnCallback += new ParentCallback(InvokeParentCallback);
				// OnException += new HandleException(InvokeHandleException); 
			}

			public virtual object InvokeParentCallback(object sender,object Data)
			{
				return this;
			}

			public virtual void InvokeHandleException(object sender,Exception childException)
			{
				throw childException;
			}

		*/

		#endregion
		
		#region Transformations

		public XmlDocument ToXML
		{
			get
			{
				XmlDocument objXML = null;
				XmlSerializer objSerializer;
				MemoryStream objStream = new MemoryStream();
				StreamWriter objWriter = new StreamWriter(objStream);
				StreamReader objReader = null;

				try
				{
					objSerializer=new XmlSerializer(this.GetType());
					objSerializer.Serialize(objWriter,this);
					objReader=new StreamReader(objWriter.BaseStream);
					objXML = new XmlDocument();
					objXML.LoadXml(objReader.ReadToEnd());
				}
				catch (Exception e) 
				{
					throw e;
				}
				finally
				{
					objWriter.Close();
					objWriter=null;
					objReader.Close();
					objReader=null;
					objStream=null;
				}

				return objXML;
			}
		}

		public byte[] ToBuffer
		{
			get
			{
				MemoryStream objStream = new MemoryStream();
				BinaryFormatter objFormatter;
				byte[] objBuffer = null;

				objFormatter = new BinaryFormatter();
				objFormatter.Serialize(objStream,this);
				objBuffer=objStream.ToArray();
				objStream.Close();

				return objBuffer;
			}
		}

		public Message ToMessage
		{
			get
			{
				Message objMessage = new Message();
				BinaryMessageFormatter objFormatter;

				objMessage.Label=this.GetType().Name;
				objMessage.Body=this;
				objFormatter=new BinaryMessageFormatter();
				objMessage.Formatter=objFormatter;

				return objMessage;
			}
		}

		#endregion

        #region Serialization

        ///// <summary>
        ///// Populate a Class instance by Deserializing from Xml Document
        ///// Appropriate XmlSerializer already initialised for the Type to be deserialized
        ///// </summary>
        //public static object Deserialize(XmlSerializer typeSerializer, XmlDocument serializedClass)
        //{
        //    object objClassInstance = null;
        //    StreamReader objReader = null;
        //    StreamWriter objWriter = new StreamWriter(new MemoryStream());

        //    objWriter.Write(serializedClass.OuterXml);
        //    objWriter.Flush();
        //    objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
        //    objReader = new StreamReader(objWriter.BaseStream);
        //    objClassInstance = typeSerializer.Deserialize(objReader);
        //    objReader.Close();
        //    objReader = null;
        //    objWriter.Close();

        //    return objClassInstance;
        //}

        ///// <summary>
        ///// Populate a Class instance by Deserializing from Xml Document
        ///// </summary>
        //public static object Deserialize(Type classType, XmlDocument serializedClass)
        //{
        //    object objClassInstance = null;
        //    XmlSerializer objSerializer = null;
        //    StreamReader objReader = null;
        //    StreamWriter objWriter = new StreamWriter(new MemoryStream());

        //    objWriter.Write(serializedClass.OuterXml);
        //    objWriter.Flush();
        //    objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
        //    objReader = new StreamReader(objWriter.BaseStream);
        //    objSerializer = new XmlSerializer(classType);
        //    objClassInstance = objSerializer.Deserialize(objReader);
        //    objReader.Close();
        //    objReader = null;
        //    objWriter.Close();

        //    return objClassInstance;
        //}

        ///// <summary>
        ///// Populate a Class instance by Deserializing from a file
        ///// </summary>
        //public static object Deserialize(Type classType, string instanceFilePath)
        //{
        //    object objClassInstance = null;
        //    XmlSerializer objSerializer = null;
        //    StreamReader objReader = null;

        //    if (File.Exists(instanceFilePath) == true)
        //    {
        //        objReader = new StreamReader(instanceFilePath);
        //        objSerializer = new XmlSerializer(classType);
        //        objClassInstance = objSerializer.Deserialize(objReader);
        //        objReader.Close();
        //        objReader = null;
        //    }
        //    return objClassInstance;
        //}


        //public static byte[] BinarySerialize(object classInstance)
        //{
        //    IFormatter objFormatter;
        //    MemoryStream objStream = null;
        //    byte[] objBuffer = null;

        //    objFormatter = new BinaryFormatter();
        //    objStream = new MemoryStream();
        //    objFormatter.Serialize(objStream, classInstance);
        //    objBuffer = objStream.ToArray();
        //    objStream.Close();

        //    return objBuffer;
        //}

        //public static object BinaryDeserialize(byte[] binaryStream)
        //{
        //    IFormatter objFormatter;
        //    MemoryStream objStream = new MemoryStream();
        //    object objClassInstance = null;
        //    BinaryWriter objWriter = new BinaryWriter(objStream);

        //    objFormatter = new BinaryFormatter();
        //    objWriter.Write(binaryStream, 0, binaryStream.Length);
        //    objWriter.Flush();
        //    objWriter.BaseStream.Seek(0, SeekOrigin.Begin);
        //    objClassInstance = objFormatter.Deserialize(objWriter.BaseStream);
        //    objWriter.Close();

        //    return objClassInstance;
        //}

        #endregion
    }
}
