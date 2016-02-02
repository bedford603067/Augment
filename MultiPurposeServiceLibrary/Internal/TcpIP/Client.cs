using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Configuration;

using MultiPurposeService.Public.Abstract;

namespace MultiPurposeService.Internal.TcpIP
{
	/// <summary>
	/// Summary description for Client.
	/// </summary>
	internal class Client : Base
	{
		#region Private Fields

		private TcpClient mobjClient;
		private byte[] mobjBuffer;
		private Processor mobjProcessor;

		private eClientStatus meStatus = eClientStatus.Connected;
		private DateTime mdteStartTime = DateTime.Now;
		private DateTime mdteLastActivity;

		private const int MAX_TIME_DORMANT = 60; // Seconds

		#endregion
		
		#region Inter-Thread Communication

		public delegate object ParentCallback(object sender,object Data);
		public delegate void HandleException(object sender,Exception childException);

		public event HandleException OnException;
		public event ParentCallback OnCallback;

		void InitialiseDelegates()
		{
			 OnCallback += new ParentCallback(InvokeParentCallback);
			 OnException += new HandleException(InvokeHandleException); 
		}

		public object InvokeParentCallback(object sender,object Data)
		{
			return Data;
		}

		public void InvokeHandleException(object sender,Exception childException)
		{
			if (OnException != null)
			{
				OnException(sender,childException);
			}
			else
			{
				throw childException;
			}
		}

		#endregion

		#region Construct\Finalise

		public Client(TcpClient clientConnection,Processor processorInstance)
		{
			// Create TcpClient and Processor if null reference passed
			mobjClient=clientConnection;
			if (processorInstance ==null)
			{
				mobjProcessor = new Processor();
			}
			else
			{
				mobjProcessor=processorInstance;
			}

			// Wire child Processor class to Pass Exceptions up to Client copy 
			mobjProcessor.OnException -= new Processor.HandleException(mobjProcessor.InvokeHandleException);
			mobjProcessor.OnException += new Processor.HandleException(this.InvokeHandleException);
            
		}

		protected override void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Release managed resources.
			}
			// Release unmanaged resources.
			// Close WinTi connection held by child Processor instance
			if (mobjProcessor != null)
			{
				if (mobjProcessor.IsConnected == true)
				{
					mobjProcessor.Shutdown();
				}
				mobjProcessor = null;
			}

			// Close Client connection held by this instance
			if (mobjClient != null)
			{
				mobjClient.Close();
				mobjClient=null;
			}

			// Call Dispose on Base
			base.Dispose(disposing);
		}

		public void Shutdown()
		{
			Dispose();
		}

		#endregion

		#region Enumerations

		public enum eClientStatus
		{
			Connected = 1,
			Disconnected = 2,
			Dormant = 3
		}

		#endregion

		#region Public Properties

		public eClientStatus Status
		{
			get
			{
				UpdateStatus();
				return meStatus;
			}
		}

		#endregion

		#region Public Methods

		public void Start()
		{
			ReadStream();
		}
		
		public void Send(object Data)
		{
			Send(Data, false);
		}

		public void Send(object Data, bool asynchronous)
		{
			if (asynchronous==true)
			{
				ThreadPool.QueueUserWorkItem(new WaitCallback(SendData), Data);
			}
			else
			{
				SendData(Data);
			}
		}

		#endregion

		#region Private Methods

		private void ReadStream()
		{
			// Note. Prepare to Receive Data from Client
			mobjBuffer=new byte[mobjClient.ReceiveBufferSize];
			mobjClient.GetStream().BeginRead(mobjBuffer, 0, mobjClient.ReceiveBufferSize, new AsyncCallback(OnReceive), null);
		}

		private void OnReceive(IAsyncResult asynchResult)
		{
			int intByteCount = 0;

			try
			{
				intByteCount = mobjClient.GetStream().EndRead(asynchResult);
			}
			catch
			{
				intByteCount = 0;
			}

			if (intByteCount > 0)
			{
				try
				{
					ReceiveData(intByteCount);
				}
				finally
				{
					// Prepare to receive further Data
					ReadStream();
				}
			}
			else
			{
				/*
				RaiseEvent ConnectionClosed();
				RaiseLocalEvent(Me, "Connection Closed");
				*/
				Console.Write("Raise Events");
			}
		}

		private void ReceiveData(int dataLength)
		{
			object objResponse=null;
			try
			{
				if (mobjClient.GetStream().CanRead)
				{
					if (mobjClient.GetStream().DataAvailable)
					{
						// Incoming message cannot be larger than buffer size assigned
						throw new Exception("Buffer insufficient to hold Network Data");
					}
					// Process request from Client
					objResponse = ProcessData(mobjBuffer, dataLength,false);
					if (objResponse !=null)
					{
						// Send response to Client
						Send(objResponse);
					}
				}
			}
			catch (Exception excE)
			{
				objResponse = CreateExceptionMessage(excE);
				if (objResponse !=null)
				{
					Send(objResponse);
				}
				OnException(this,excE);
			}
		}

		private object ProcessData(byte[] receivedData, int dataLength, bool asynchronous)
		{
			object objState;
			string strInput;
			bool blnForwardClientRequests = false;
			bool blnTestMessage = false;

			try
			{
				strInput = Encoding.ASCII.GetString(receivedData, 0, dataLength);
				if (strInput == "Hello World|" || strInput=="Test|")
				{
					Logging.WriteToLog(this, "Test Message received");
					blnForwardClientRequests = true;
					objState = (object)strInput;
				}
				else
				{
                    //if (ConfigurationManager.AppSettings["DefaultExternalAssemblyPath"] != null)
                    //{
                    //    System.Reflection.Assembly.LoadFile(ConfigurationManager.AppSettings["DefaultExternalAssemblyPath"]);
                    //}
					if (dataLength < receivedData.Length)
					{
						byte[] bufferContent = new byte[dataLength];
						for (int intIndex = 0; intIndex < bufferContent.Length; intIndex++)
						{
							bufferContent[intIndex] = receivedData[intIndex];
						}
						objState = BusinessObjects.Base.BinaryDeserialize(bufferContent);
					}
					else
					{
                        objState = BusinessObjects.Base.BinaryDeserialize(receivedData);
					}
				}

				if (ConfigurationManager.AppSettings["ForwardClientRequests"] != null)
				{
					blnForwardClientRequests = bool.Parse(ConfigurationManager.AppSettings["ForwardClientRequests"]);
				}
				if (blnForwardClientRequests == true && blnTestMessage==false)
				{
					// Pass up to caller instance for Processing
					objState = this.OnCallback(this, objState);
				}
				else
				{
					// Process immediately
					if (asynchronous == false)
					{
						mobjProcessor.Process(receivedData);
					}
					else
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(mobjProcessor.Process), receivedData);
					}
					objState = (object)true;
				}
			}
			catch (Exception excE)
			{
				objState = CreateExceptionMessage(excE);
			}
			
			return objState;

		}

		private void SendData(object state)
		{
			byte[] objBuffer = null;

			try
			{
				// Copy Data to Byte Array
				/*
				if (state is BusinessObjects.Base)
				{
					objBuffer = ((BusinessObjects.Base)state).ToBuffer();
				}
				else */
				if (state is byte[])
				{
					objBuffer = (byte[]) state;
				}
				else
				{
					string strResponse;
					if (state is string)
					{
						strResponse = (string)state;
					}
					else
					{
						strResponse = state.ToString();
					}
					objBuffer = Encoding.ASCII.GetBytes(strResponse);
				}

				// Send Data to Network Stream
				lock( mobjClient.GetStream())
				{
					mobjClient.GetStream().Write(objBuffer, 0, objBuffer.Length);
				}
			}
			catch (Exception excE)
			{
				OnException(this,excE);
			}
		}

		private object CreateExceptionMessage(Exception excE)
		{
			object objReturn = null;

			/*
			switch(mobjProcessor.ResponseType)
			{
				case Processor.eResponseType.MeterReadingTransaction:
					Commidea.MeterReadingTransactionResult objResult=new Commidea.MeterReadingTransactionResult();
					objResult.Exceptions.Add(excE);
					objResult.Outcome=Commidea.MeterReadingTransactionResult.eTransactionOutcome.Exception;
					objReturn = (object) objResult.ToBuffer;
					break;
				case Processor.eResponseType.MeterReadingCardInfo:
					Commidea.MeterReadingCardInfo objCardResult=new Commidea.MeterReadingCardInfo();
					objCardResult.Exceptions.Add(excE);
					objCardResult.Result=Commidea.MeterReadingCardInfo.eLookupResult.SchemeNotFound;
					objReturn = (object) objCardResult.ToBuffer;
					break;
			}
			*/

			objReturn = (object)excE;

			Logging.WriteToLog(this, excE);

			return objReturn;

		}

		private void UpdateStatus()
		{
			DateTime dteCurrentTime = DateTime.Now;

			// Note. Work out what Status this Tcp Client is in

			if (meStatus == eClientStatus.Connected)
			{
				if ((dteCurrentTime.Ticks - mdteLastActivity.Ticks) > MAX_TIME_DORMANT)
				{
					meStatus = eClientStatus.Dormant;
				}
			}

		}

		#endregion

	}
}
