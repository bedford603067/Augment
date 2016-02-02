using System;
using System.Messaging;
using System.Configuration;

using MultiPurposeService.Internal.Abstract;

namespace MultiPurposeService.Internal.Service
{
	/// <summary>
	/// Summary description for BatchAgent.
	/// </summary>
	internal class BatchAgent:QueueAgent
	{
		#region Private Fields

        private eCommideaStatus meStatus = eCommideaStatus.Online;
		private Processor mobjProcessor = null;
	
		#endregion

		#region Public Properties and Fields
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
			object objStatus;

			try
			{
				/*
				if (Data is Commidea.MeterReadingTransaction)
				{
					// If MeterReadingTransaction or meStatus indicates Offline then Queue transaction
					if (((Commidea.MeterReadingTransaction)Data).IsCommideaOnline == false)
					{
						meStatus = eCommideaStatus.Offline;
					}

					// Option to have Agent always running as if Commidea is Offline
					if (int.Parse(ConfigurationSettings.AppSettings.Get("RunInBatchMode")) == 1)
					{
						meStatus = eCommideaStatus.Offline;
					}
					if (meStatus == eCommideaStatus.Offline)
					{
						//QueueTransaction((Commidea.MeterReadingTransaction)Data);
					}
				}
				*/
			}
			catch (Exception e)
			{
				OnException(this,e);
			}

			objStatus = (eCommideaStatus) meStatus;

			return objStatus;
		}

		public void InvokeHandleException(object sender,Exception childException)
		{
			throw childException;
		}

		#endregion

		#region Enumerations

		public enum eCommideaStatus
		{
			Online = 0,
			Offline = 1
		}


		#endregion

		#region Construct\Finalise

		public BatchAgent(string inboundQueue, string outboundQueue,Processor processorInstance): base(inboundQueue,outboundQueue)
		{
			mobjProcessor = processorInstance;

			// Wire Processor instance to call BatchAgent copy of method for OnCallback
			mobjProcessor.OnCallback-= new Processor.ParentCallback(mobjProcessor.InvokeParentCallback);
			mobjProcessor.OnCallback+= new Processor.ParentCallback(this.InvokeParentCallback);
		
			// Configure Processor to keep Socket connection open after each use
			mobjProcessor.StayAlive=true;

		}

		protected override void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Release managed resources.
			}
			// Release local resources.
			if (mobjProcessor != null )
			{
				mobjProcessor.Shutdown();
				mobjProcessor=null;
			}

			// Call Dispose on Base
			base.Dispose(disposing);
		}

		public void Shutdown()
		{
			Dispose();
		}

		#endregion

		#region Public Methods

		/*
		protected Commidea.MeterReadingTransactionResult ExecuteTask(Commidea.MeterReadingTransaction businessClass)
		{
			Commidea.MeterReadingTransactionResult objReturn=null;

			try
			{
				if (mobjProcessor != null)
				{
					objReturn = mobjProcessor.AuthoriseTransaction(businessClass);
					// Stop Processing if Transactionresult indicates still in Batch mode
					if (((Commidea.MeterReadingTransactionResult)objReturn).Outcome == Commidea.MeterReadingTransactionResult.eTransactionOutcome.InBatchMode)
					{
						this.Stop();
					}
				}
			}
			catch (Exception e)
			{
				OnException(this,e);
			}
			
			return objReturn;

		}
		*/

		#endregion

		#region Private Methods

		/*
		private bool QueueTransaction(Commidea.MeterReadingTransaction transactionInstance)
		{
			//Message objMessage = null;
			//BusinessObjects.Export objExport = new BusinessObjects.Export();
			//bool blnSuccess = true;

			//objMessage=transactionInstance.ToMessage;
			//objExport.Send(objMessage,this.InboundQueue.Path);

			//objExport = null;

			//return blnSuccess;

			return true;
		}

		*/

		#endregion

	}
}
