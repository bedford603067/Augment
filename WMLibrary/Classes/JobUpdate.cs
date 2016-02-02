using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class JobUpdate
    {
        #region Public Properties (outside XSD schema)

        private SerializableHashTable _ExtendedProperties;
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
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

        public abstract bool ApplyToJob(string userID, int jobInstanceNumber);

        public bool DoOthersNeedInformedOfUpdate()
        {
            if (this.Status != eJobStatus.InTransit &&
                this.Status != eJobStatus.Started)
            {
                AssignmentDetailsCollection assigments = AssignmentDetailsCollection.GetAssignments(this.UserID, new int[] { this.ID });
                if (assigments != null && assigments.Count > 0)
                {
                    WorkerCollection assignees = assigments[0].Workers;
                    if(assignees !=null)
                    {
                        for (int index = 0; index < assignees.Count; index++)
                        {
                            if(assignees[index].LoginName.ToUpper()!=this.UserID.ToUpper())
                            {
                                // Another Engineer\Gang is to be informed of the update
                                return true;
                            }
                        }
                    }                }
            }

            return false;
        }

        public bool Resubmit(out string failedMessage)
        {
            failedMessage = string.Empty;

            // Get Job Instance No. (Assignment in Click for this User on this Job)
            int jobInstanceNumber = BusinessObjects.WorkManagement.JobReference.GetJobInstanceNumber(mintID, mobjSourceSystem, mstrUserID);

            // Process JobUpdates at Source System
            try
            {
                // Process and Save update detail
                this.ApplyToJob(mstrUserID, jobInstanceNumber);
                // Delete Serialized Update
                // this.DeleteSerializedObject();
            }
            catch (Exception excE)
            {
                failedMessage = string.Format("Resubmission of JobUpdate for Job {0} failed with Exception message {1}", this.ID.ToString(), excE.Message);
                return false;
            }

            return true;
        }

        public BusinessObjects.WorkManagement.Job UpdateTasks(BusinessObjects.WorkManagement.Job targetJob, BusinessObjects.WorkManagement.EngineerFeedback feedback)
        {
            // Tasks
            ActivityTaskCollection activityTaskCollection = new ActivityTaskCollection();
            ActivityTask activityTask = null;

            /*
            foreach (TaskUpdate taskUpdate in feedback.Tasks)
            {
                if (targetJob.Tasks != null && targetJob.Tasks.Count > 0 && targetJob.Tasks.Find("ID", taskUpdate.ID) >= 0)
                {
                    // Update to existing Task
                    activityTask = targetJob.Tasks.Find(taskUpdate.ID);
                }
                else
                {
                    // Adding a new Task
                    activityTask = new ActivityTask();
                    activityTask.Description = taskUpdate.Comments[0].Text;
                }
                activityTask.IsComplete = taskUpdate.IsComplete;
                activityTask.IsDatabaseComplete = activityTask.IsComplete;
                activityTask.LastUpdatedDate = feedback.EndDateTime != null ? feedback.EndDateTime.Value : this.EndDateTime.Value;
                activityTaskCollection.Add(activityTask);
            }
            foreach (ActivityTask updatedActivityTask in activityTaskCollection)
            {
                if (updatedActivityTask.ID > 0)
                {
                    targetJob.Tasks.Replace(targetJob.Tasks.Find(updatedActivityTask.ID), updatedActivityTask);
                }
                else
                {
                    targetJob.Tasks.Add(updatedActivityTask);
                }
            }
            */

            foreach (TaskUpdate taskUpdate in feedback.Tasks)
            {
                activityTask = new ActivityTask();
                if (taskUpdate.ID <= 0)
                {
                    activityTask.Description = taskUpdate.Comments[0].Text;
                }
                activityTask.IsComplete = taskUpdate.IsComplete;
                activityTask.IsDatabaseComplete = activityTask.IsComplete;
                activityTask.LastUpdatedDate = feedback.EndDateTime != null ? feedback.EndDateTime.Value : this.EndDateTime.Value;

                activityTask.MaterialsRequired = taskUpdate.MaterialsUsed;
                activityTask.MeasurementsRequired = taskUpdate.MeasurementsTaken;
                
                activityTaskCollection.Add(activityTask);
            }
            targetJob.Tasks = activityTaskCollection;

            return targetJob;
        }
    }

    public partial class TaskUpdate
    {
        public bool ContainsCompletedData()
        {
            if (mColMaterialsUsed != null && mColMaterialsUsed.Count > 0)
            {
                return true;
            }

            if (mColMeasurementsTaken != null)
            {
                for (int index = 0; index < mColMeasurementsTaken.Count; index++)
                {
                    if (mColMeasurementsTaken[index].ReadDate != DateTime.MinValue &&
                        mColMeasurementsTaken[index].Value > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
