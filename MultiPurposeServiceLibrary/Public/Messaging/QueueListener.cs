using System;
using System.Messaging;
using System.Collections;

using MultiPurposeService.Internal;

namespace MultiPurposeService.Public.Messaging
{
    /// <summary>
    /// Summary description for QueueListener.
    /// </summary>
    public class QueueListener
    {
        #region Inter-Thread Communication

        public delegate object ParentCallback(object sender, object state);
        public delegate void HandleException(object sender, Exception childException);

        public event ParentCallback OnCallback;
        public event HandleException OnException;

        void InitialiseDelegates()
        {
            OnCallback += new ParentCallback(InvokeParentCallback);
            OnException += new HandleException(InvokeHandleException);
        }

        public object InvokeParentCallback(object sender, object Data)
        {
            //SendToWorkManager(sender, Data);
            return (object)true;
        }

        public void InvokeHandleException(object sender, Exception childException)
        {
            // Event Log
            Logging.WriteToLog(this, childException);

            // Expose error to any callers who may be wired in to this event as well
            OnException(sender, childException);
        }

        #endregion

        #region Private Fields

        private MessageQueue mobjInboundQueue;
        private eMessageFetch meFetchMethod = eMessageFetch.RemovePermanently;
        private eMessageFormat meMessageFormat = eMessageFormat.XMLSerialize;
        private Type mobjCustomType = null;

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

        /// <summary>
        /// True = 100% asynch, receives Next without waiting for Previous message to raise MessageReceived
        /// False = Wait until MessageReceived on current message 100% done before receive Next
        /// </summary>
        public bool BeginReceivePreRaisingLastMessageReceivedEvent = false;

        #endregion

        #region Enumerations

        public enum eMessageFetch
        {
            RetrieveCopy = 0,
            RemovePermanently = 1
        }

        public enum eMessageFormat
        {
            XMLSerialize = 0,
            ActiveXSerialize = 1,
            BinarySerialize = 2
        }

        #endregion

        #region Construct\Finalise

        public QueueListener(string inboundQueue, eMessageFetch fetchMethod, eMessageFormat messageFormat, Type customType)
        {
            if (inboundQueue.IndexOf("private$") < 0)
            {
                mobjInboundQueue = new MessageQueue(QUEUE_PREFIX + inboundQueue,QueueAccessMode.Receive);
            }
            else
            {
                mobjInboundQueue = new MessageQueue(inboundQueue, QueueAccessMode.Receive);
            }
            meFetchMethod = fetchMethod;
            meMessageFormat = messageFormat;
            mobjCustomType = customType;
        }

        public QueueListener(string inboundQueue, eMessageFetch fetchMethod, eMessageFormat messageFormat)
        {
            if (inboundQueue.IndexOf("private$") < 0)
            {
                mobjInboundQueue = new MessageQueue(QUEUE_PREFIX + inboundQueue, QueueAccessMode.Receive);
            }
            else
            {
                mobjInboundQueue = new MessageQueue(inboundQueue, QueueAccessMode.Receive);
            } 
            meFetchMethod = fetchMethod;
            meMessageFormat = messageFormat;
        }

        public QueueListener(string inboundQueue, eMessageFetch fetchMethod)
        {
            if (inboundQueue.IndexOf("private$") < 0)
            {
                mobjInboundQueue = new MessageQueue(QUEUE_PREFIX + inboundQueue, QueueAccessMode.Receive);
            }
            else
            {
                mobjInboundQueue = new MessageQueue(inboundQueue, QueueAccessMode.Receive);
            }
            meFetchMethod = fetchMethod;
        }

        ~QueueListener()
        {
            if (mobjInboundQueue != null)
            {
                Stop();
            }
        }

        #endregion

        #region Event Handlers

        public delegate void MessageReceivedHandler(Message receivedMessage, bool removalFromQueueRequired, string receivingQueuePath);
        public event MessageReceivedHandler MessageReceived;

        void mobjInboundQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            Message objMessage = null;

            try
            {
                // Get Message from Queue
                objMessage = mobjInboundQueue.EndReceive(e.AsyncResult);

                if (BeginReceivePreRaisingLastMessageReceivedEvent)
                {
                    // Listen for further messages
                    mobjInboundQueue.BeginReceive();
                    // Expose Message to hosting instance
                    MessageReceived(objMessage, false, mobjInboundQueue.Path);
                }
                else
                {
                    // Expose Message to hosting instance
                    MessageReceived(objMessage, false, mobjInboundQueue.Path);
                    // Listen for further messages
                    mobjInboundQueue.BeginReceive();
                }
            }
            catch (NullReferenceException)
            {
                if (mobjInboundQueue != null)
                {
                    // Ignore
                }
            }
            catch (MessageQueueException excE)
            {
                throw excE;
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        void mobjInboundQueue_PeekCompleted(object sender, PeekCompletedEventArgs e)
        {
            Message objMessage = null;

            try
            {
                // Get Message from Queue
                objMessage = mobjInboundQueue.EndPeek(e.AsyncResult);

                if (BeginReceivePreRaisingLastMessageReceivedEvent)
                {
                    // Listen for further messages
                    mobjInboundQueue.BeginPeek();
                    // Expose Message to hosting instance
                    MessageReceived(objMessage, true, mobjInboundQueue.Path);
                }
                else
                {
                    // Expose Message to hosting instance
                    MessageReceived(objMessage, true, mobjInboundQueue.Path);
                    // Listen for further messages
                    mobjInboundQueue.BeginPeek();
                }
            }
            catch (NullReferenceException)
            {
                if (mobjInboundQueue != null)
                {
                    // Ignore
                }
            }
            catch (MessageQueueException excE)
            {
                throw excE;
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            SetQueueFormatterAndProperties(mobjInboundQueue, meMessageFormat, mobjCustomType);
            switch (meFetchMethod)
            {
                case eMessageFetch.RemovePermanently:
                    {
                        mobjInboundQueue.ReceiveCompleted += new ReceiveCompletedEventHandler(mobjInboundQueue_ReceiveCompleted);
                        mobjInboundQueue.BeginReceive();
                        break;
                    }
                case eMessageFetch.RetrieveCopy:
                    {
                        mobjInboundQueue.PeekCompleted += new PeekCompletedEventHandler(mobjInboundQueue_PeekCompleted);
                        mobjInboundQueue.BeginPeek();
                        break;
                    }
            }
        }

        public void Stop()
        {
            mobjInboundQueue.Dispose();
            mobjInboundQueue = null;
        }

        public void RemoveMessageFromQueue(Message targetMessage)
        {
            mobjInboundQueue.ReceiveById(targetMessage.Id);
        }

        public Message GetMessageByLabel(string messageLabel)
        {
            Message[] allMessagesInQueue = null;
            Message matchingMessage = null;

            allMessagesInQueue = PeekMessages(mobjInboundQueue, false, meMessageFormat, mobjCustomType);
            if (allMessagesInQueue != null)
            {
                for (int index = 0; index < allMessagesInQueue.Length; index++)
                {
                    if (allMessagesInQueue[index].Label == messageLabel)
                    {
                        matchingMessage =  allMessagesInQueue[index];
                        mobjInboundQueue.ReceiveById(matchingMessage.Id); /// Remove Message from Q
                        break;
                    }
                }
            }

            return matchingMessage;
        }

        public Message[] PeekMessages(MessageQueue activeQueue, bool blnDynamicConnection, eMessageFormat messageFormat, System.Type customType)
        {
            Message objMessage;
            Message[] arrCurrentMessages = new Message[0];
            Message[] arrCopyOfMessages = null;
            MessagePropertyFilter objMessagePropertyFilter = new MessagePropertyFilter();
            int intArrayIndex;

            // Message Formatter
            SetQueueFormatterAndProperties(activeQueue, messageFormat, customType);

            // Dynamic Connection whilst gathering messages
            if (blnDynamicConnection == true)
            {
                IEnumerator objMessageEnumerator = activeQueue.GetMessageEnumerator2();
                intArrayIndex = 0;
                while (objMessageEnumerator.MoveNext())
                {
                    objMessage = (Message)objMessageEnumerator.Current;
                    if (intArrayIndex > 0)
                    {
                        arrCopyOfMessages = new Message[intArrayIndex];
                        arrCurrentMessages.CopyTo(arrCopyOfMessages, 0);
                        arrCurrentMessages = arrCopyOfMessages;
                    }
                    arrCurrentMessages[intArrayIndex] = objMessage;
                    intArrayIndex += 1;
                }
            }
            else // Snapshot of messages currently in Queue
            {
                arrCurrentMessages = null;
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

        #endregion

        #region Private Methods

        private void SetQueueFormatterAndProperties(MessageQueue activeQueue, eMessageFormat messageFormat, Type customType)
        {
            IMessageFormatter objFormatter = null;
            MessagePropertyFilter objMessagePropertyFilter = new MessagePropertyFilter();

            // Message Formatter
            switch (messageFormat)
            {
                case eMessageFormat.XMLSerialize:
                    if (customType == null)
                    {
                        objFormatter = new XmlMessageFormatter(); //new Type[] {typeof(System.Xml.XmlDocument) });
                    }
                    else
                    {
                        objFormatter = new XmlMessageFormatter(new Type[] { customType });
                    }

                    break;
                case eMessageFormat.ActiveXSerialize:
                    objFormatter = new ActiveXMessageFormatter();
                    break;
                case eMessageFormat.BinarySerialize:
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
        }

        #endregion
    }
}
