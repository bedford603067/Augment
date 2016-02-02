using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BusinessObjects.WorkManagement
{
    public partial class Job
    {
        #region Derived Properties

        private bool mblnHasAppointmentDateChanged = false;
        private bool mblnHasConfirmationChanged = false;
        private Job mobjAssociatedJob = null;

        public int Duration
        {
            get
            {
                int durationRemaining = 0;
                int tasksCount = 0;
                int tasksCompletedCount = 0;
                int durationUndertaken = 0;

                if (this.Tasks != null)
                {
                    // Calculate the Duration from Task.Duration for completed Tasks
                    // NB - tasksCompleted & tasksCount used if Activity Duration set too
                    tasksCount = this.Tasks.Count;
                    for (int intIndex = 0; intIndex < this.Tasks.Count; intIndex++)
                    {
                        durationRemaining += this.Tasks[intIndex].Duration;
                        if (this.Tasks[intIndex].IsComplete)
                        {
                            durationRemaining -= this.Tasks[intIndex].Duration;
                            tasksCompletedCount += 1;
                        }
                    }
                }
                if (mintActivityDuration > 0)
                {
                    // Calculate the Duration from ActivityDuration 
                    // NB - this overrides the calculation of Duration from Tasks above
                    if (tasksCompletedCount > 0)
                    {
                        decimal percentageCompleted = (decimal)tasksCompletedCount / tasksCount;
                        durationUndertaken = (int)(mintActivityDuration * percentageCompleted);
                    }
                    durationRemaining = mintActivityDuration - durationUndertaken;
                }
                
                return durationRemaining;
            }
        }

        public bool HasAppointmentDateChanged
        {
            get
            {
                return mblnHasAppointmentDateChanged;
            }
        }

        public bool HasConfirmationChanged
        {
            get
            {
                return mblnHasConfirmationChanged;
            }
        }

        public eRAGCoding RAGValue
        {
            get
            {
                //  If amending this - take note of FieldDataClientCoreXP.Classes.GridWorker.SetRowColour
                eRAGCoding ragValue = eRAGCoding.Green; // Not overdue
                
                if (mdteDueDate.Date < DateTime.Now.Date) 
                {
                    ragValue = eRAGCoding.Red;
                }
                else if (mdteDueDate.Day == DateTime.Now.Day &&
                         mdteDueDate.Month == DateTime.Now.Month &&
                         mdteDueDate.Year == DateTime.Now.Year)
                {
                    ragValue = eRAGCoding.Amber;
                }

                return ragValue;
            }
        }

        public string Summary
        {
            get
            {
                string summary = string.Empty;
                string template = string.Empty;
                string activityDescription = (mobjActivity != null ? mobjActivity.Description : "Unknown");

                template += "Job: {0}  Asset Type: {1}" + Environment.NewLine;
                template += "Work Type: {2}  Activity: {3}" + Environment.NewLine;
                template += "Description: {4} Appointment: {5}";
                summary = string.Format(template,
                                        mintID.ToString(),
                                        mobjAsset != null ? mobjAsset.AssetType : "Unknown",
                                        mobjWorkType.ToString(),
                                        activityDescription,
                                        (mColComments != null && mColComments.Count > 0) ? mColComments[0].Text : "None",
                                        mobjAppointment !=null ? "Yes" : "No");

                return summary;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public ResourceProfileCollection ResourcesRequired
        {
            get
            {
                ResourceProfileCollection resourcesRequired = new ResourceProfileCollection();
                int resourceCounter = 0;

                if (mobjSkillsBreakdown == null || mobjSkillsBreakdown.ResourceProfiles == null)
                {
                    return null;
                }

                if (!mobjSkillsBreakdown.AreSkillsMappedAtActivityLevel)
                {
                    // Task-Skills mapping
                    for (int intIndex = 0; intIndex < this.Tasks.Count; intIndex++)
                    {
                        if (this.Tasks[intIndex].SkillsBreakdown != null && this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles != null &&
                            this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles.Count > 0)
                        {
                            resourceCounter = resourcesRequired.Count;
                            while (resourceCounter < this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles.Count)
                            {
                                resourcesRequired.Add(new ResourceProfile());
                                resourcesRequired[resourcesRequired.Count - 1].Skills = new SkillCollection();

                                resourceCounter += 1;
                            }

                            for (int resourceIndex = 0; resourceIndex < this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles.Count; resourceIndex++)
                            {
                                for (int skillIndex = 0; skillIndex < this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles[resourceIndex].Skills.Count; skillIndex++)
                                {
                                    if (resourcesRequired[resourceIndex].Skills.Find("Description", this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles[resourceIndex].Skills[skillIndex].Description) < 0)
                                    {
                                        resourcesRequired[resourceIndex].Skills.Add(this.Tasks[intIndex].SkillsBreakdown.ResourceProfiles[resourceIndex].Skills[skillIndex]);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Activity-Skills mapping
                    resourcesRequired = mobjSkillsBreakdown.ResourceProfiles;
                }

                return resourcesRequired;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public int NoOfWorkersRequired
        {
            get
            {
                if (this.ResourcesRequired != null)
                {
                    return this.ResourcesRequired.Count;
                }
                return 0;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public MaterialCollection MaterialsCollection
        {
            get
            {
                MaterialCollection materialsRequired = new MaterialCollection();

                if (this.Tasks != null)
                {
                    for (int intIndex = 0; intIndex < this.Tasks.Count; intIndex++)
                    {
                        if (this.Tasks[intIndex].MaterialsRequired != null)
                        {
                            for (int materialIndex = 0; materialIndex < this.Tasks[intIndex].MaterialsRequired.Count; materialIndex++)
                            {
                                if (materialsRequired.Find("Description", this.Tasks[intIndex].MaterialsRequired[materialIndex].Description) < 0)
                                {
                                    materialsRequired.Add(this.Tasks[intIndex].MaterialsRequired[materialIndex]);
                                }
                            }
                        }
                    }
                }

                return materialsRequired;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public string CriticalTasksOutstanding
        {
            get
            {
                string criticalTasksOutstanding = string.Empty;

                if (mColTasks != null)
                {
                    foreach (BusinessObjects.WorkManagement.ActivityTask activityTask in mColTasks)
                    {
                        if (activityTask.IsCritical && !activityTask.IsComplete)
                        {
                            criticalTasksOutstanding += activityTask.Description + Environment.NewLine;
                        }
                    }
                }

                return criticalTasksOutstanding;
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public Job AncestorJob
        {
            get
            {
                int ancestorJobID = -1;

                if (mobjAssociatedJob == null && 
                    mobjParentJobReference != null && 
                    mobjParentJobReference.ID > 0)
                {
                    ancestorJobID = FindAncestor();
                    if (ancestorJobID > 0)
                    {
                        mobjAssociatedJob = BusinessObjects.WorkManagement.Job.GetSerializedObject(ancestorJobID, mobjSourceSystem, true);
                    }
                }

                return mobjAssociatedJob;
            }
        }

        #endregion

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

        #region Public Methods

        public abstract void IncludeMostRecentFeedback();
        public abstract void IncludeMostRecentFeedback(string userID, string[] gangMembers);
        public abstract void IncludeMostRecentFeedback(DateTime specificChangeDate);

        public abstract bool HasFeedbackBeenLoaded(out DateTime sessionStartDate);

        #region Instance

        public void DetermineUserChanges(Job currentCopyOfJob)
        {
            if (currentCopyOfJob.Appointment != null && mobjAppointment != null)
            {
                if (currentCopyOfJob.Appointment.Date != mobjAppointment.Date ||
                   currentCopyOfJob.Appointment.TimeFrom != mobjAppointment.TimeFrom ||
                   currentCopyOfJob.Appointment.TimeTo != mobjAppointment.TimeTo)
                {
                    mblnHasAppointmentDateChanged = true;
                }
                mblnHasConfirmationChanged = (currentCopyOfJob.Appointment.IsConfirmed != mobjAppointment.IsConfirmed);
            }
        }

        public override bool Validate(out ValidationExceptionCollection validationExceptions)
        {
            return base.Validate(out validationExceptions);
        }

        public bool StoreOnHoldReasons()
        {
            int intRowsAffected = 0;

            if (mColOnHoldReasons != null && mColOnHoldReasons.Count > 0)
            {
                foreach (OnHoldReason onHoldReason in mColOnHoldReasons)
                {
                    if (onHoldReason.Comments != null)
                    {
                        foreach (CommentAuditRecord onHoldComment in onHoldReason.Comments)
                        {
                            if (onHoldComment.ChangeDate == DateTime.MinValue)
                            {
                                onHoldComment.ChangeDate = DateTime.Now;
                            }
                        }
                    }

                    intRowsAffected += onHoldReason.Save(mintID);
                }
            }

            return (intRowsAffected > 0);
        }

        public void IncludeComments()
        {
            mColComments = CommentAuditRecordCollection.FindByJob(mintID, mobjSourceSystem);
        }

        public  void AddRequiredMaterials()
        {
            bool materialOHRFound = false;
            bool weHaveRequestedMaterials = false;
            BusinessObjects.WorkManagement.MaterialCollection taskAssociatedMaterials = null;

            //Check whether this Job has any Requested Materials allocated to it
            BusinessObjects.WorkManagement.MaterialCollection requestedMaterials = BusinessObjects.WorkManagement.MaterialCollection.FindByJob(this.ID);

            if (requestedMaterials != null && requestedMaterials.Count > 0)
            {
                weHaveRequestedMaterials = true;
                BusinessObjects.WorkManagement.OnHoldReason materialOHR = null;
                if (this.OnHoldReasons != null)
                {
                    int materialsReasonIndex = -1;
                    materialsReasonIndex = this.OnHoldReasons.Find("Code", "MAT");
                    if (materialsReasonIndex > -1)
                    {
                        materialOHR = this.OnHoldReasons[materialsReasonIndex];
                        materialOHRFound = true;
                    }
                }
                else
                {
                    this.OnHoldReasons = new BusinessObjects.WorkManagement.OnHoldReasonCollection();
                }

                if (!materialOHRFound)
                {
                    materialOHR = new BusinessObjects.WorkManagement.OnHoldReason();
                    materialOHR.ID = 1;
                    materialOHR.Description = "Materials Required";
                    materialOHR.Code = "MAT"; // NB: Required by SP to insert new OH Reason
                    materialOHR.DateResolved = DateTime.Now;

                    this.OnHoldReasons.Add(materialOHR);
                }

                //Now add latest MaterialsRequested 
                materialOHR.MaterialsRequired = requestedMaterials;

                //Also check to see if we need to add any materials into Job.Tasks[]
                if (weHaveRequestedMaterials)
                {
                    //Get a list of Task-associated materials
                    if (this.Tasks != null && this.Tasks.Count > 0)
                    {
                        taskAssociatedMaterials = new BusinessObjects.WorkManagement.MaterialCollection();
                        foreach (BusinessObjects.WorkManagement.ActivityTask task in this.Tasks)
                        {
                            if (task.MaterialsRequired != null)
                            {
                                for (int index = 0; index < task.MaterialsRequired.Count; index++)
                                {
                                    if (taskAssociatedMaterials.Find("Description", task.MaterialsRequired[index].Description) < 0)
                                    {
                                        taskAssociatedMaterials.Add(task.MaterialsRequired[index]);
                                    }
                                }
                            }
                        }
                    }

                    //Compare our Task Associated Materials with our database Requested Materails
                    if (taskAssociatedMaterials != null && taskAssociatedMaterials.Count > 0)
                    {
                        foreach (BusinessObjects.WorkManagement.Material material in requestedMaterials)
                        {
                            //Is this Requested Material already associated with a task ?
                            if (taskAssociatedMaterials.Find("Description", material.Description) < 0)
                            {
                                //NO .. so add it to Task[0]
                                this.Tasks[0].MaterialsRequired.Add(material.Clone(true) as BusinessObjects.WorkManagement.Material);
                            }
                        }
                    }
                    else
                    {
                        //No Task associated Jobs ... but we have Requested Materials
                        //So add them all to Job.Task[0]
                        this.Tasks[0].MaterialsRequired = requestedMaterials;
                    }

                }
            }
        }



        #endregion

        #region Static


        public static void AddRequiredMaterialsStatic(BusinessObjects.WorkManagement.Job job)
        {
            bool materialOHRFound = false;
            bool weHaveRequestedMaterials = false;
            BusinessObjects.WorkManagement.MaterialCollection taskAssociatedMaterials = null;

            //Check whether this Job has any Requested Materials allocated to it
            BusinessObjects.WorkManagement.MaterialCollection requestedMaterials = BusinessObjects.WorkManagement.MaterialCollection.FindByJob(job.ID);

            if (requestedMaterials != null && requestedMaterials.Count > 0)
            {
                weHaveRequestedMaterials = true;
                BusinessObjects.WorkManagement.OnHoldReason materialOHR = null;
                if (job.OnHoldReasons != null)
                {
                    int materialsReasonIndex = -1;
                    materialsReasonIndex = job.OnHoldReasons.Find("Code", "MAT");
                    if (materialsReasonIndex > -1)
                    {
                        materialOHR = job.OnHoldReasons[materialsReasonIndex];
                        materialOHRFound = true;
                    }
                }
                else
                {
                    job.OnHoldReasons = new BusinessObjects.WorkManagement.OnHoldReasonCollection();
                }

                if (!materialOHRFound)
                {
                    materialOHR = new BusinessObjects.WorkManagement.OnHoldReason();
                    materialOHR.ID = 1;
                    materialOHR.Description = "Materials Required";
                    materialOHR.Code = "MAT"; // NB: Required by SP to insert new OH Reason
                    materialOHR.DateResolved = DateTime.Now;

                    job.OnHoldReasons.Add(materialOHR);
                }

                //Now add latest MaterialsRequested 
                materialOHR.MaterialsRequired = requestedMaterials;

                //Also check to see if we need to add any materials into Job.Tasks[]
                if (weHaveRequestedMaterials)
                {
                    //Get a list of Task-associated materials
                    if (job.Tasks != null && job.Tasks.Count > 0)
                    {
                        taskAssociatedMaterials = new BusinessObjects.WorkManagement.MaterialCollection();
                        foreach (BusinessObjects.WorkManagement.ActivityTask task in job.Tasks)
                        {
                            if (task.MaterialsRequired != null)
                            {
                                for (int index = 0; index < task.MaterialsRequired.Count; index++)
                                {
                                    if (taskAssociatedMaterials.Find("Description", task.MaterialsRequired[index].Description) < 0)
                                    {
                                        taskAssociatedMaterials.Add(task.MaterialsRequired[index]);
                                    }
                                }
                            }
                        }
                    }

                    //Compare our Task Associated Materials with our database Requested Materails
                    if (taskAssociatedMaterials != null && taskAssociatedMaterials.Count > 0)
                    {
                        foreach (BusinessObjects.WorkManagement.Material material in requestedMaterials)
                        {
                            //Is this Requested Material already associated with a task ?
                            if (taskAssociatedMaterials.Find("Description", material.Description) < 0)
                            {
                                //NO .. so add it to Task[0]
                                job.Tasks[0].MaterialsRequired.Add(material.Clone(true) as BusinessObjects.WorkManagement.Material);
                            }
                        }
                    }
                    else
                    {
                        //No Task associated Jobs ... but we have Requested Materials
                        //So add them all to Job.Task[0]
                        job.Tasks[0].MaterialsRequired = requestedMaterials;

                        //TODO: Do we need to check the other way around?
                        //ie. No Requested Materials but Task Associated ones on the job !!
                    }

                }
            }
        }


        public static Job FindJobByID(int jobID, List<Job> searchList)
        {
            return searchList.Find(new Predicate<Job>
            (
                new SearchByIDClass(jobID).PredicateDelegate)
            );
        }

        public static int FindJobIndexInList(Job targetJob, List<Job> jobList)
        {
            int intJobIndex = -1;

            for (int intIndex = 0; intIndex < jobList.Count; intIndex++)
            {
                if (jobList[intIndex].ID == targetJob.ID && jobList[intIndex].SourceSystem == targetJob.SourceSystem)
                {
                    return intIndex;
                }
            }
            return intJobIndex;
        }

        public static List<Job> FindAssignedJobsByUser(string userID, List<Job> searchList)
        {
            return searchList.FindAll(new Predicate<Job>
            (
                new SearchByAssignedUserClass(userID).PredicateDelegate)
            );
        }

        public static List<Job> RemoveJobsWithGivenStatuses(List<Job> jobs, eJobStatus[] statuses)
        {
            // Remove Jobs which are already Completed or Cancelled
            foreach (eJobStatus status in statuses)
            {
                jobs.RemoveAll(x => x.Status == status);
            }

            return jobs;
        }

        #endregion

        #endregion

        #region Public Classes

        public class SortByDueDate : System.Collections.Generic.IComparer<Job>
        {
            public int Compare(Job x, Job y)
            {
                return x.DueDate.CompareTo(y.DueDate);
            }
        }

        public class SearchByIDClass
        {
            int mintJobID;

            public SearchByIDClass(int jobID)
            {
                this.mintJobID = jobID;
            }

            public bool PredicateDelegate(Job memberJob)
            {
                return memberJob.ID == mintJobID;
            }
        }

        public class SearchByAssignedUserClass
        {
            string mstrUserID;

            public SearchByAssignedUserClass(string userID)
            {
                this.mstrUserID = userID;
            }

            public bool PredicateDelegate(Job memberJob)
            {
                return memberJob.Workers[0].UserID == mstrUserID;
            }
        }


        #endregion

        #region Public Enumerations

        public enum eRAGCoding
        {
            Red,
            Amber,
            Green
        }

        #endregion
	}

    /// <summary>
    /// Supporting Integration Web Site - Job Viewer functionality specifically
    /// Additional properties defined herein do NOT form part of the Data Contract
    /// </summary>
    public partial class Job
    {
        #region Public Properties

        [System.Xml.Serialization.XmlIgnore]
        public string CurrentAssignee
        {
            get
            {
                // TODO: May need to convert Assignee key to Surname-Forename
                // TODO: Mixing local WMLibrary & WCF service is something of a hack

                string assignee = string.Empty;
                string[] clickAssignees = BusinessObjects.WorkManagement.Job.GetAssigneeInformation(this.ID, (BusinessObjects.WorkManagement.eWMSourceSystem)Enum.Parse(typeof(BusinessObjects.WorkManagement.eWMSourceSystem), this.SourceSystem.ToString(), true), 0);

                if (clickAssignees != null)
                {
                    assignee = clickAssignees[0];
                }
                return assignee;
            }
            set
            {
                // Just exposed for auto-databinding purposes
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public string CommentsString
        {
            get
            {
                string commentsString = string.Empty;

                if (this.Comments != null && this.Comments.Count > 0)
                {
                    return this.Comments.ToString();
                }

                return commentsString;
            }
            set
            {
                // Just exposed for auto-databinding purposes
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public string StartedDate
        {
            get
            {
                // TODO: Mixing local WMLibrary & WCF service is something of a hack

                string startedDate = string.Empty;
                List<BusinessObjects.WorkManagement.WorkflowAuditRecord> statusHistory = BusinessObjects.WorkManagement.Job.GetWorkflowAuditHistory(this.ID, (BusinessObjects.WorkManagement.eWMSourceSystem)Enum.Parse(typeof(BusinessObjects.WorkManagement.eWMSourceSystem), this.SourceSystem.ToString()));
                if (statusHistory != null)
                {
                    foreach (BusinessObjects.WorkManagement.WorkflowAuditRecord auditRecord in statusHistory)
                    {
                        if (auditRecord.Status == BusinessObjects.WorkManagement.eJobStatus.Started)
                        {
                            startedDate = string.Format("{0:g}", auditRecord.ChangeDate);
                            break;
                        }
                    }
                }
                return startedDate;
            }
            set
            {
                // Just exposed for auto-databinding purposes
            }
        }

        #endregion

        #region Private Methods

        private System.ComponentModel.BindingList<BusinessObjects.WorkManagement.CommentAuditRecord> GetTestComments()
        {
            System.ComponentModel.BindingList<CommentAuditRecord> testComments = new System.ComponentModel.BindingList<CommentAuditRecord>();
            CommentAuditRecord commentRecord = null;

            commentRecord = new CommentAuditRecord();
            commentRecord.ChangeDate = DateTime.Now;
            commentRecord.ChangeUser = "TESTER";
            commentRecord.Text = "First Test Comment record";
            testComments.Add(commentRecord);


            commentRecord = new CommentAuditRecord();
            commentRecord.ChangeDate = DateTime.Now;
            commentRecord.ChangeUser = "TESTER";
            commentRecord.Text = "Second Test Comment record. Some longer text to show : abcdefghijklmnopqrstuvwxyz";
            testComments.Add(commentRecord);

            commentRecord = new CommentAuditRecord();
            commentRecord.ChangeDate = DateTime.Now;
            commentRecord.ChangeUser = "TESTER";
            commentRecord.Text = "Third Test Comment record. Even longer text to show : abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz abcdefghijklmnopqrstuvwxyz";
            testComments.Add(commentRecord);

            return testComments;
        }

        #endregion
    }
}
