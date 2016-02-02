using System;
using System.Text;
using System.Configuration;

using MultiPurposeService.Public.MultiThreading;
using MultiPurposeService.Internal.Abstract;
using MultiPurposeService.Interfaces;
using MultiPurposeService.Internal.MultiThreading;
using MultiPurposeService.Internal.TcpIP;

namespace MultiPurposeService.Internal
{
	/// <summary>
	/// Summary description for Processor.
	/// </summary>
	[Serializable()] 	
	internal class Processor:SocketClient,IProcessor
	{
		#region Event Handlers

		private void Processor_DataReceived(string responseData)
		{
			/*
			Commidea.MeterReadingResponseCollection colResponses = null;

			colResponses = new Commidea.MeterReadingResponseCollection(responseData);
			mobjLastResponse = colResponses[colResponses.Count - 1];
			mblnTerminateLoop = mobjLastResponse.TerminateLoop;
			*/
			// MeterReadingResponseReceived(mobjLastResponse);

		}

		private void Processor_ExceptionEncountered(Exception childException)
		{
			OnException(this, childException);
		}

		#endregion

		#region Enumerations

		public enum eResponseType
		{
			MeterReadingTransaction = 0,
			MeterReadingCardInfo = 1
		}

		#endregion

		/*
		eResponseType meResponseType = 0;
		bool mblnTerminateLoop = false;
		bool mblnSkipBasketSteps = false;
		*/

		#region Public Methods

		public void Process()
		{
			//--------------------------------------------------------------------------------------------------------
			Logging.WriteToLog(this, "Processor thread started successfully");
			//--------------------------------------------------------------------------------------------------------
		}

		public void Process(object inputData)
		{
			//--------------------------------------------------------------------------------------------------------
			Logging.WriteToLog(this, "Processor received message from TcpClient " + Environment.NewLine + inputData.ToString());
			//--------------------------------------------------------------------------------------------------------

			string strInputData = null;

			if (inputData is WorkerTaskInfo)
			{
				// Submit Task for immediate processing on separate thread(s)
				WorkerProcess objWorkerThread = new WorkerProcess((WorkerTaskInfo)inputData);
				objWorkerThread.Start();
			}
			else
			{
				// Read data as string, either directly or by extraction from byte[]
				if (inputData is Byte[])
				{
					byte[] buffer = (byte[])inputData;
					strInputData = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
				}
				else
				{
					if (inputData is string)
					{
						strInputData = ((string)inputData);

						// Logging (optional)
						if (ConfigurationManager.AppSettings["LoggingMode"] != null)
						{
                            if (ConfigurationManager.AppSettings.Get("LoggingMode") == "verbose")
							{
								Logging.WriteToLog(this,"Request received : " + strInputData);
							}
						}
					}
				}
			}
		}

		public void Shutdown()
		{
			Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Release managed resources.
			}
			// Release unmanaged resources.

			// Call Dispose on Base
			base.Dispose(disposing);
		}

		public Processor(SocketConnection socketConnection)
			: base(socketConnection)
		{
			//this.DataReceived += new ReceiveData(Processor_DataReceived);
			//this.ExceptionEncountered += new RaiseException(Processor_ExceptionEncountered);

			//// Development only - purely test Transaction processing with Commidea
			//if (ConfigurationSettings.AppSettings.Get("SkipBasketSteps") != null)
			//{
			//    mblnSkipBasketSteps = bool.Parse(ConfigurationSettings.AppSettings.Get("SkipBasketSteps"));
			//}
		}

		public Processor()
		{
			//
		}

		#endregion

		#region Inter-Thread Communication

		public delegate object ParentCallback(object sender, object state);
		public delegate void HandleException(object sender, Exception childException);
		public delegate void ParentFeedback(object sender, string Data);

		public event ParentCallback OnCallback;
		public event HandleException OnException;

		void InitialiseDelegates()
		{
			OnCallback += new ParentCallback(InvokeParentCallback);
			OnException += new HandleException(InvokeHandleException);
		}

		public object InvokeParentCallback(object sender, object state)
		{
			// Process incoming request
			if (state.GetType().Name == "WorkerTaskInfo")
			{
				// TODO: Process the input
				Console.WriteLine("Work to process");
			}
			return this;
		}

		public void InvokeHandleException(object sender, Exception childException)
		{
			throw childException;
		}

		#endregion

	}
}


