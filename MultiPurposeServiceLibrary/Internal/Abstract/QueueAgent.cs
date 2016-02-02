using System;
using System.Messaging;
using System.Collections;

using MultiPurposeService.Public.Abstract;

namespace MultiPurposeService.Internal.Abstract
{
	/// <summary>
	/// Summary description for QueueAgent.
	/// </summary>
	internal abstract class QueueAgent : Base
	{

		#region Private Fields

		private System.Timers.Timer mobjTimer = null;
		private MessageQueue mobjInboundQueue;
		private MessageQueue mobjOutboundQueue;
		private bool mblnCancelProcessing = false;

		const string QUEUE_PREFIX = @".\PRIVATE$\";

		#endregion

		#region Public Properties and Fields

		public MessageQueue InboundQueue
		{
			get
			{
				return mobjInboundQueue;
			}
		}

		public System.TimeSpan PollingInterval = new System.TimeSpan(0,0,1,0,0); // Default 1 minutes

		#endregion

		#region Enumerations

		public enum MessageFetch
		{
			RetrieveCopy = 0,
			RemovePermanently = 1
		}
		
		public enum MessageFormat
		{
			XMLSerialize = 0,
			ActiveXSerialize = 1,
			BinarySerialize = 2
		}

		#endregion

		#region Construct\Finalise

		public QueueAgent(string inboundQueue, string outboundQueue)
		{

		mobjInboundQueue = new MessageQueue(QUEUE_PREFIX + inboundQueue);
		mobjOutboundQueue = new MessageQueue(QUEUE_PREFIX + outboundQueue);
		
		}

		public QueueAgent(string inboundQueue)
		{

		mobjInboundQueue = new MessageQueue(QUEUE_PREFIX + inboundQueue);

		}

		public QueueAgent()
		{
		}

		protected override void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				// Release managed resources.
				mobjInboundQueue=null;
				mobjOutboundQueue=null;
			}
			// Release unmanaged resources.

			// Call Dispose on Base
			base.Dispose(disposing);
		}

		#endregion

		#region Public Methods

		//protected abstract BusinessObjects.Base ExecuteTask(BusinessObjects.Base businessClass);

		public void Poll()
		{
			mobjTimer = new System.Timers.Timer(PollingInterval.TotalMilliseconds);
			mobjTimer.Elapsed +=new System.Timers.ElapsedEventHandler(mobjTimer_Elapsed); 
			mobjTimer.Enabled=true;
		}

		private void mobjTimer_Elapsed(object sender,System.Timers.ElapsedEventArgs e)
		{
			mobjTimer.Enabled=false;
			Process();
			mobjTimer.Enabled=true;
		}

		public virtual void Process()
		{
			//Message[] arrCurrentMessages = null;
			//Base objBusinessClass;
			//Base objReturn;
			//Import objImport = new Import();
			//Export objExport = new Export();

			//// Read Business Objects from Inbound queue
			//try
			//{
			//    arrCurrentMessages = PeekMessages(mobjInboundQueue, false, MessageFormat.BinarySerialize,null);
			//    if (arrCurrentMessages.Length > 0)
			//    {
			//        foreach (Message objMessage in arrCurrentMessages)
			//        {
			//            if (mblnCancelProcessing == false)
			//            {
			//                objBusinessClass = objImport.Load(objMessage);
			//                Console.Write(objBusinessClass.Serialize());
			//                objReturn = ExecuteTask(objBusinessClass);
			//                if (objReturn != null)
			//                {
			//                    if (mobjOutboundQueue != null)
			//                    {	
			//                        // Send each Returned Business Object to Outbound Q
			//                        objExport.Send(objReturn.ToMessage, mobjOutboundQueue.Path);
			//                    }
			//                    // Remove source message from Inbound Q
			//                    mobjInboundQueue.ReceiveById(objMessage.Id);
			//                }
			//            }
			//            else
			//            {
			//                // Reset flag for future calls and then exit
			//                mblnCancelProcessing = false;
			//                break;
			//            }
			//        }
			//    }
			//}
			//catch (Exception excE)
			//{
			//    throw excE;
			//}

			//// Housekeep
			//arrCurrentMessages = null ;
			//objImport = null ;
			//objExport = null ;
			//objBusinessClass = null ;
			//objReturn = null ;
			//GC.Collect();

			// System.Threading.Thread.Sleep(1000)

		}

		public void Stop()
		{
			// Process method will complete processing of current Message and then halt 
			mblnCancelProcessing = true;
		}

		#endregion

		#region Private Methods

		private Message[] PeekMessages(MessageQueue activeQueue, bool blnDynamicConnection, MessageFormat eMessageFormat, System.Type CustomType)
		{
			Message objMessage;
			Message[] arrCurrentMessages = new Message[0];
			Message[] arrCopyOfMessages = null;
			IMessageFormatter objFormatter = null ;
			MessagePropertyFilter objMessagePropertyFilter = new MessagePropertyFilter();
			int intArrayIndex;

			// Message Formatter
			switch (eMessageFormat)
			{
				case MessageFormat.XMLSerialize:
					if (CustomType == null)
					{
						objFormatter = new XmlMessageFormatter();
					}
					else
					{
					// objFormatter = new XmlMessageFormatter(new Type() [CustomType]);
					}

					break;
				case MessageFormat.ActiveXSerialize:
					objFormatter = new ActiveXMessageFormatter();
					break;
				case MessageFormat.BinarySerialize:
					objFormatter = new BinaryMessageFormatter();
					break;
			}

			// Messages in Private Queue
			// Ensure these properties are received (CorrelationID defaults to False)
			objMessagePropertyFilter.SetDefaults();
			objMessagePropertyFilter.CorrelationId = true;
			objMessagePropertyFilter.AppSpecific = true;
			objMessagePropertyFilter.ArrivedTime = true;
			activeQueue.MessageReadPropertyFilter = objMessagePropertyFilter;

			// Message Formatter
			activeQueue.Formatter = objFormatter;

			// Dynamic Connection whilst gathering messages
			if (blnDynamicConnection == true)
			{
				IEnumerator objMessageEnumerator = activeQueue.GetEnumerator();
				intArrayIndex = 0;
				while (objMessageEnumerator.MoveNext())
				{
					objMessage = (Message) objMessageEnumerator.Current;
					if (intArrayIndex > 0)
					{
						arrCopyOfMessages = new Message[intArrayIndex];
						arrCurrentMessages.CopyTo(arrCopyOfMessages,0);
						arrCurrentMessages=arrCopyOfMessages;
					}
					arrCurrentMessages[intArrayIndex] = objMessage;
					intArrayIndex += 1;
				}
			}
			else // Snapshot of messages currently in Queue
			{
				arrCurrentMessages = null ;
				try
				{
					arrCurrentMessages = activeQueue.GetAllMessages();
				}
				catch (System.Messaging.MessageQueueException excM)
				{
					throw excM;
				}
			}

			return arrCurrentMessages;

		}


		#endregion

	}
}
