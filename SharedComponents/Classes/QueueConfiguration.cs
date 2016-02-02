using System;
using System.Collections.Generic;

using System.IO;
using System.Configuration;

namespace FinalBuild
{
    ///// <summary>
    ///// Custom Configuration Section for Message Queue configuration
    ///// </summary>
    public sealed class MessageQueueSection : ConfigurationSection
    {
        // MessageQueueSection objSection = (MessageQueueSection)ConfigurationManager.GetSection("MessageQueueSection");

        //    // Example declaration of this Section in a Configuration file
        //    /**********************************************************************
        //    <configSections>
        //        <section name="MessageQueueSection" type="FinalBuild.MessageQueueSection,SharedComponents" />
        //    </configSections>

        //    <MessageQueueSection>
        //        <MessageQueues>
        //          <MessageQueue
        //              Name="WorkOutgoingQueue"
        //              QueuePath="FormatName:Direct=OS:DEVAPPS1\private$\WorkOutgoingQueue" 
        //              NumberOfQueues="10" 
        //              PoisonMessagesQueue="FormatName:Direct=OS:DEVAPPS1\private$\WorkOutgoingPoisonMessages"  
        //              ReceiveMessagesAsync="true"
        //              NoOfRetriesAllowed="0" 
        //              IntervalBetweenRetries="30" />
        //          <MessageQueue
        //              Name="JobStatusQueue"
        //              QueuePath="FormatName:Direct=OS:DEVAPPS1\private$\JobStatusQueue" 
        //              NumberOfQueues="50" 
        //              PoisonMessagesQueue="FormatName:Direct=OS:DEVAPPS1\private$\JobStatusPoisonMessages"  
        //              ReceiveMessagesAsync="false"
        //              NoOfRetriesAllowed="0" 
        //              IntervalBetweenRetries="30" />
        //        </MessageQueues>
        //    </MessageQueueSection>

        #region Public Properties

        [ConfigurationProperty("MessageQueues", IsRequired = true)]
        public MessageQueueElementCollection MessageQueues
        {

            get
            {
                return (MessageQueueElementCollection)this["MessageQueues"];
            }
            set
            {
                this["MessageQueues"] = value;
            }

        }

        #endregion
    }

    public sealed class MessageQueueElement : ConfigurationElement
    {
        // MessageQueueSection objSection = (MessageQueueSection)ConfigurationManager.GetSection("MessageQueueSection");

        //    // Example declaration of this Section in a Configuration file
        //    /**********************************************************************
        //    <configSections>
        //        <section name="MessageQueueSection" type="FinalBuild.MessageQueueSection,SharedComponents" />
        //    </configSections>

        //    <MessageQueueSection>
        //        <MessageQueues>
        //          <MessageQueue
        //              Name="WorkOutgoingQueue"
        //              QueuePath="FormatName:Direct=OS:DEVAPPS1\private$\WorkOutgoingQueue" 
        //              NumberOfQueues="10" 
        //              PoisonMessagesQueue="FormatName:Direct=OS:DEVAPPS1\private$\WorkOutgoingPoisonMessages"  
        //              ReceiveMessagesAsync="true"
        //              NoOfRetriesAllowed="0" 
        //              IntervalBetweenRetries="30" />
        //          <MessageQueue
        //              Name="JobStatusQueue"
        //              QueuePath="FormatName:Direct=OS:DEVAPPS1\private$\JobStatusQueue" 
        //              NumberOfQueues="50" 
        //              PoisonMessagesQueue="FormatName:Direct=OS:DEVAPPS1\private$\JobStatusPoisonMessages"  
        //              ReceiveMessagesAsync="false"
        //              NoOfRetriesAllowed="0" 
        //              IntervalBetweenRetries="30" />
        //        </MessageQueues>
        //    </MessageQueueSection>

        #region Public Properties

        [ConfigurationProperty("Name", DefaultValue = "QueueName", IsRequired = true, IsKey = true)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string Name
        {

            get
            {
                return (string)this["Name"];
            }
            set
            {
                this["Name"] = value;
            }

        }

        [ConfigurationProperty("QueuePath", DefaultValue=@".\private$\QueueName", IsRequired=true)]
        [StringValidator(InvalidCharacters = " ~!@#%^&*()[]{}/;", MinLength = 1, MaxLength = 100)]
        public string QueuePath
        {

            get
            {
                return (string)this["QueuePath"];
            }
            set
            {
                this["QueuePath"] = value;
            }

        }

        [ConfigurationProperty("PoisonMessagesQueue", DefaultValue=@".\private$\PoisonMessagesQueueName", IsRequired = true)]
        [StringValidator(InvalidCharacters = " ~!@#%^&*()[]{}/;", MinLength = 1, MaxLength = 100)]
        public string PoisonMessagesQueue
        {

            get
            {
                return (string)this["PoisonMessagesQueue"];
            }
            set
            {
                this["PoisonMessagesQueue"] = value;
            }

        }

        [ConfigurationProperty("NumberOfQueues", DefaultValue = "10", IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 50)]
        public int NumberOfQueues
        {

            get
            {
                return (int)this["NumberOfQueues"];
            }
            set
            {
                this["NumberOfQueues"] = value;
            }

        }

        [ConfigurationProperty("NoOfRetriesAllowed", DefaultValue = "0", IsRequired = false)]
        [IntegerValidator(MinValue = 0, MaxValue = 5)]
        public int NoOfRetriesAllowed
        {

            get
            {
                return (int)this["NoOfRetriesAllowed"];
            }
            set
            {
                this["NoOfRetriesAllowed"] = value;
            }

        }

        [ConfigurationProperty("ReceiveMessagesAsync", DefaultValue = "true", IsRequired = false)]
        public bool ReceiveMessagesAsync
        {

            get
            {
                return (bool)this["ReceiveMessagesAsync"];
            }
            set
            {
                this["ReceiveMessagesAsync"] = value;
            }

        }

        /// <summary>
        /// The Interval in SECONDS between Retries.
        /// Units used are complimentary to how MPL PollingThread sets PollingInterval i.e in Seconds
        /// </summary>
        [ConfigurationProperty("IntervalBetweenRetries", DefaultValue = "30", IsRequired = false)]
        [IntegerValidator(MinValue = 1)]
        public int IntervalBetweenRetries
        {

            get
            {
                return (int)this["IntervalBetweenRetries"];
            }
            set
            {
                this["IntervalBetweenRetries"] = value;
            }

        }

        #endregion
    }

    [ConfigurationCollection(typeof(MessageQueueElement), 
     AddItemName = "MessageQueue",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class MessageQueueElementCollection : ConfigurationElementCollection
    {
        // MessageQueueSection objSection = (MessageQueueSection)ConfigurationManager.GetSection("MessageQueueSection");

        //    // Example declaration of this Section in a Configuration file
        //    /**********************************************************************
        //    <configSections>
        //        <section name="MessageQueueSection" type="FinalBuild.MessageQueueSection,SharedComponents" />
        //    </configSections>

        //    <MessageQueueSection>
        //        <MessageQueues>
        //          <MessageQueue
        //              QueuePath="FormatName:Direct=OS:DEVAPPS1\private$\WorkOutgoingQueue" 
        //              NumberOfQueues="10" 
        //              PoisonMessagesQueue="FormatName:Direct=OS:DEVAPPS1\private$\WorkOutgoingPoisonMessages"  
        //              ReceiveMessagesAsync="true"
        //              NoOfRetriesAllowed="0" 
        //              IntervalBetweenRetries="30" />
        //        </MessageQueues>
        //    </MessageQueueSection>

        #region Private Fields

        private static ConfigurationPropertyCollection _properties;

        #endregion

        #region Constructors

        static MessageQueueElementCollection()
        {
            _properties = new ConfigurationPropertyCollection();
        }

        public MessageQueueElementCollection()
        {
        }
        #endregion

        #region Properties

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        // Override for ConfigurationElementCollectionType.BasicMap or AddRemoveClearMap
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
            //get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        /// <summary>
        /// Override specific to using ConfigurationElementCollectionType.BasicMap
        /// </summary>
        protected override string ElementName
        {
            get { return "MessageQueue"; }
        }

        #endregion

        #region Indexers

        public MessageQueueElement this[int index]
        {
            get { return (MessageQueueElement)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new MessageQueueElement this[string name]
        {
            get { return (MessageQueueElement)base.BaseGet(name); }
        }

        #endregion

        #region Overrides

        protected override ConfigurationElement CreateNewElement()
        {
            return new MessageQueueElement();
        }

        /// <summary>
        /// Allows string based indexing i.e. MessageQueues["Name"].XXX
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as MessageQueueElement).Name;
        }

        #endregion
    }
}
