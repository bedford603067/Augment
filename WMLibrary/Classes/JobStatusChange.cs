using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;

namespace BusinessObjects.WorkManagement
{
    public partial class JobStatusChange
    {
        #region Public Properties (outside XSD schema)

        private SerializableHashTable _ExtendedProperties;

        [System.Runtime.Serialization.DataMemberAttribute()]
        public SerializableHashTable ExtendedProperties
        {
            get
            {
                if (_ExtendedProperties == null)
                {
                    _ExtendedProperties = new SerializableHashTable();
                }
                return _ExtendedProperties;
            }
            set
            {
                _ExtendedProperties = value;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Shorthand method for creating and queuing a JobStatusChange in a one line call
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="jobStatus"></param>
        /// <param name="userID"></param>
        /// <param name="lastUpdated"></param>
        /// <param name="jobInstanceNumber"></param>
        /// <param name="service"></param>
        /// <param name="windowsIdentity"></param>
        /// <param name="isOfficeUser"></param>
        /// <param name="incompleteReason"></param>
        /// <param name="extendedProperties"></param>
        public static void Broadcast(int jobID, eWMSourceSystem sourceSystem, eJobStatus jobStatus, string userID, DateTime lastUpdated, int jobInstanceNumber, string service, string windowsIdentity, bool isOfficeUser, IncompleteReason incompleteReason, SerializableHashTable extendedProperties)
        {
            JobStatusChange statusChange = new JobStatusChange();
            
            statusChange.ID = jobID;
            statusChange.SourceSystem = sourceSystem;
            statusChange.Status = jobStatus;
            statusChange.UserID = userID;
			statusChange.LastUpdated = lastUpdated;
            statusChange.Service = service;
            statusChange.InstanceNumber = jobInstanceNumber;
            if (windowsIdentity == null)
            {
                statusChange.WindowsIdentity = Environment.UserName;
            }
            else
            {
                statusChange.WindowsIdentity = windowsIdentity;
            }
            statusChange.IsOfficeUser = isOfficeUser;
            statusChange.IncompleteReason = incompleteReason;
            statusChange.ExtendedProperties = extendedProperties;
            statusChange.EnqueueMessage();
        }

        /// <summary>
        /// Shorthand method for creating and queuing a JobStatusChange in a one line call
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="jobStatus"></param>
        /// <param name="userID"></param>
        /// <param name="lastUpdated"></param>
        /// <param name="jobInstanceNumber"></param>
        /// <param name="service"></param>
        /// <param name="windowsIdentity"></param>
        /// <param name="isOfficeUser"></param>
        /// <param name="incompleteReason"></param>
        public static void Broadcast(int jobID, eWMSourceSystem sourceSystem, eJobStatus jobStatus, string userID, DateTime lastUpdated, int jobInstanceNumber, string service, string windowsIdentity, bool isOfficeUser, IncompleteReason incompleteReason)
        {
            Broadcast(jobID, sourceSystem, jobStatus, userID, lastUpdated, jobInstanceNumber, service, windowsIdentity, isOfficeUser, incompleteReason);
        }

        /// <summary>
        /// Shorthand method for creating and queuing a JobStatusChange in a one line call
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="jobStatus"></param>
        /// <param name="userID"></param>
        /// <param name="jobInstanceNumber"></param>
        /// <param name="service"></param>
        /// <param name="windowsIdentity"></param>
		public static void Broadcast(int jobID, eWMSourceSystem sourceSystem, eJobStatus jobStatus, string userID, int jobInstanceNumber, string service, string windowsIdentity)
		{
			Broadcast(jobID, sourceSystem, jobStatus, userID, DateTime.Now, jobInstanceNumber, service, windowsIdentity, false, null, null);
		}

        #endregion

        #region Instance Methods

        public void EnqueueMessage()
        {
            EnqueueMessage(-1);
        }

        public void EnqueueMessage(int noOfRetriesSofar)
        {
            FinalBuild.QueueWriter objMessageWriter = null;
            string queuePath = DetermineJobStatusQueuePath(this.ID);

            objMessageWriter = new FinalBuild.QueueWriter(queuePath, true);
            objMessageWriter.MessageWritten += new FinalBuild.QueueWriter.MessageWrittenHandler(mobjMessageWriter_MessageWritten);
            objMessageWriter.WriteToQueue(this, this.GetType(), this.SourceSystem.ToString() + ":" + this.ID.ToString(), noOfRetriesSofar);
        }

        void mobjMessageWriter_MessageWritten(string feedbackMessage)
        {
            Console.WriteLine(feedbackMessage);
        }

        private string DetermineJobStatusQueuePath(int jobID)
        {
            string queuePath = @"FormatName:Direct=OS:cs717189\private$\JobStatusQueue";
            int numberOfQueues = 1;

            if (ConfigurationManager.AppSettings["JobStatusQueue"] != null)
            {
                queuePath = ConfigurationManager.AppSettings["JobStatusQueue"];
            }

            // If Q. Path is 1 of n possible Qs then choose the appropriate nth one
            if (ConfigurationManager.AppSettings["NumberOfJobStatusQueues"] != null)
            {
                numberOfQueues = int.Parse(ConfigurationManager.AppSettings["NumberOfJobStatusQueues"]);
            }
            // NB: QueueAssistant is responsible for handling all noOfQueues cases internally
            queuePath += new BusinessObjects.WorkManagement.QueueAssistant(numberOfQueues).DeriveQueueExtensionFromItem(jobID);

            return queuePath;
        }

        #endregion
    }
}
