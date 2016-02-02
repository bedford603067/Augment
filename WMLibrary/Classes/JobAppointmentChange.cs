using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;

namespace BusinessObjects.WorkManagement
{
    public class JobAppointmentChange
    {
        #region Public Methods

        public JobAppointmentChange(int jobID, BusinessObjects.WorkManagement.Appointment jobAppointment)
        {
            EnqueueMessage(jobAppointment, jobID.ToString());
        }

        #endregion

        #region Private Methods

        private void EnqueueMessage(BusinessObjects.WorkManagement.Appointment messageBody, string messageLabel)
        {
            FinalBuild.QueueWriter objMessageWriter = null;
            string strQueuePath = @"FormatName:Direct=OS:cs717189\private$\JobAppointmentQueue";

            if (ConfigurationManager.AppSettings["JobAppointmentQueue"] != null)
            {
                strQueuePath = ConfigurationManager.AppSettings["JobAppointmentQueue"];
            }

            objMessageWriter = new FinalBuild.QueueWriter(strQueuePath, true);
            objMessageWriter.MessageWritten += new FinalBuild.QueueWriter.MessageWrittenHandler(mobjMessageWriter_MessageWritten);
            objMessageWriter.WriteToQueue(messageBody, messageBody.GetType(), messageLabel);
        }

        void mobjMessageWriter_MessageWritten(string feedbackMessage)
        {
            Console.WriteLine(feedbackMessage);
        }

        #endregion
    }
}

