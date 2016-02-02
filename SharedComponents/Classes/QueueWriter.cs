using System;
using System.Collections.Generic;
using System.Text;

using System.Messaging;

namespace FinalBuild
{
    public class QueueWriter
    {
        #region Private Fields

        private MessageQueue mobjInboundQueue = null;
        private string mstrQueuePath = @".\private$\Inbound";
        private bool mblnIsRemoteQueue = false;

        #endregion

        #region Public Events

        public delegate void MessageWrittenHandler(string feedbackMessage);
        public event MessageWrittenHandler MessageWritten;

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

        #region Public Methods

        public QueueWriter(string queuePath, bool isRemoteQueue)
        {
            mblnIsRemoteQueue = isRemoteQueue;
            if (isRemoteQueue && queuePath.IndexOf("FormatName") < 0)
            {
                queuePath = "FormatName:Direct=OS:" + queuePath;
            }
            mstrQueuePath = queuePath;
        }

                /// <summary>
        /// Write a (serializable) Class instance to a Message Q
        /// </summary>
        /// <param name="messageBody"></param>
        /// <param name="messageBodyType"></param>
        /// <param name="messageLabel"></param>
        public void WriteToQueue(object messageBody, Type messageBodyType, string messageLabel)
        {
            WriteToQueue(messageBody, messageBodyType, messageLabel, 0);
        }

        /// <summary>
        /// Write a (serializable) Class instance to a Message Q
        /// </summary>
        /// <param name="messageBody"></param>
        /// <param name="messageBodyType"></param>
        /// <param name="messageLabel"></param>
        /// <param name="noOfRetriesSofar"></param>
        public void WriteToQueue(object messageBody, Type messageBodyType, string messageLabel, int noOfRetriesSofar)
        {
            Message objMessage = null;

            if (!mblnIsRemoteQueue)
            {
                // Note. Cannot do with Remote Q as Exists cannot resolve Queue Path
                if (!MessageQueue.Exists(mstrQueuePath))
                {
                    MessageQueue.Create(mstrQueuePath, true);
                }
            }

            if (mobjInboundQueue == null)
            {
                mobjInboundQueue = new MessageQueue(mstrQueuePath, QueueAccessMode.SendAndReceive);
            }

            IMessageFormatter objFormatter = SetQueueFormatterAndProperties(mobjInboundQueue, eMessageFormat.XMLSerialize, messageBodyType);
            objMessage = new Message(messageBody, objFormatter);
            if (noOfRetriesSofar > 0)
            {
                objMessage.AppSpecific = noOfRetriesSofar;
            }
            WriteToQueue(objMessage, messageLabel);
        }

        /// <summary>
        /// Write a Xml Document to a Message Q
        /// </summary>
        /// <param name="messageBody"></param>
        /// <param name="messageLabel"></param>
        public void WriteToQueue(System.Xml.XmlDocument messageBody, string messageLabel)
        {
            Message objMessage = null;

            objMessage = new Message(messageBody);
            WriteToQueue(objMessage, messageLabel);
        }

        /// <summary>
        /// Write a String to a Message Q
        /// </summary>
        /// <param name="messageBody"></param>
        /// <param name="messageLabel"></param>
        public void WriteToQueue(string messageBody, string messageLabel)
        {
            Message objMessage = null;

            objMessage = new Message(messageBody);
            WriteToQueue(objMessage, messageLabel);
        }

        /// <summary>
        /// Write a System.Messaging.Message to a Message Q
        /// Called by all the other overloads
        /// Can also used to put a received Message back in the Q it was taken out of
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageLabel"></param>
        public void WriteToQueue(System.Messaging.Message message, string messageLabel)
        {
            if (mobjInboundQueue == null)
            {
                mobjInboundQueue = new MessageQueue(mstrQueuePath, QueueAccessMode.SendAndReceive);
            }

            using (MessageQueueTransaction objTransaction = new MessageQueueTransaction())
            {
                try
                {
                    objTransaction.Begin();
                    mobjInboundQueue.Send(message, messageLabel, objTransaction);
                    objTransaction.Commit();

                    // Raise event
                    MessageWritten(messageLabel + " " + "written to Queue @ " + DateTime.Now.ToLongTimeString());
                }
                catch (MessageQueueException excE)
                {
                    throw excE;
                }
            }

        }

        #endregion

        #region Private Methods

        private IMessageFormatter SetQueueFormatterAndProperties(MessageQueue activeQueue, eMessageFormat messageFormat, Type customType)
        {
            IMessageFormatter objFormatter = null;
            MessagePropertyFilter objMessagePropertyFilter = new MessagePropertyFilter();

            // Message Formatter
            switch (messageFormat)
            {
                case eMessageFormat.XMLSerialize:
                    if (customType == null)
                    {
                        objFormatter = new XmlMessageFormatter();
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

            return objFormatter;
        }

        #endregion

    }
}