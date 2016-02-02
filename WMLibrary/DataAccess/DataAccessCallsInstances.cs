#region Using

using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Text;

using FinalBuild;

#endregion

namespace BusinessObjects.WorkManagement
{
    #region ActiveJobReferences class

    public partial class ActiveJobReferences
    {
        public ActiveJobReferences(eWMSourceSystem wmSourceSystem)
        {
            Load(wmSourceSystem);
        }

        public ActiveJobReferences()
        {
            // Default constructor for Serialization purposes
        }

        private void Load(eWMSourceSystem wmSourceSystem)
        {
            DataSet dsData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobsForClick";

            colParameters.Add(new SqlParameter("@WMSourceSystem", wmSourceSystem.ToString()));
            dsData = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dsData != null && dsData.Tables.Count > 0)
            {
                Populate(dsData);
            }
        }
    }

    #endregion

    #region ScheduledJobReferences class

    public partial class ScheduledJobReferences
    {
        public ScheduledJobReferences(eWMSourceSystem wmSourceSystem)
        {
            Load(wmSourceSystem);
        }

        public ScheduledJobReferences()
        {
            // Default constructor for Serialization purposes
        }

        private void Load(eWMSourceSystem wmSourceSystem)
        {
            DataSet dsData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobsForMobile";

            colParameters.Add(new SqlParameter("@WMSourceSystem", wmSourceSystem.ToString()));
            dsData = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dsData != null && dsData.Tables.Count > 0)
            {
                Populate(dsData);
            }
        }
    }

    #endregion

    #region Job class

    public partial class Job
    {
        #region Public Methods

        public bool SaveSerializedObject(bool informEngineer)
        {
            return SaveSerializedObject(false, informEngineer, null);
        }

        public bool SaveSerializedObject(bool isNew, bool informEngineer, string clickCallID)
        {
            return SaveSerializedObject(isNew, informEngineer, clickCallID, 0);
        }

        public bool SaveSerializedObject(bool isNew, bool informEngineer, string clickCallID, int jobInstanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            SqlParameter returnValueParameter = null;
            string strStoredProcedure = "updJobSerialized";

            string strXML = BusinessObjects.Base.Serialize(this.GetType(), this);
            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
            objParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
            objParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(strXML, System.Xml.XmlNodeType.Document, null));
            colParameters.Add(objParameter);
            if (isNew)
            {
                strStoredProcedure = "insJobSerialized";
                colParameters.Add(new SqlParameter("@DueDate", mdteDueDate));
                colParameters.Add(new SqlParameter("@TypeName", this.GetType().AssemblyQualifiedName));
                if (clickCallID != null && clickCallID != string.Empty)
                {
                    colParameters.Add(new SqlParameter("@ClickCallID", clickCallID));
                }
                returnValueParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
                returnValueParameter.Direction = ParameterDirection.ReturnValue;
                colParameters.Add(returnValueParameter);
            }
            else
            {
                if (this.IsComplete)
                {
                    colParameters.Add(new SqlParameter("@IsComplete", true));
                }
                if (informEngineer)
                {
                    colParameters.Add(new SqlParameter("@DateChangeDetected", DateTime.Now));
                }
            }
            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (returnValueParameter != null && returnValueParameter.Value != DBNull.Value)
            {
                // Set ID if Job is newly created & ID is required
                if (mintID < 1)
                {
                    mintID = (int)returnValueParameter.Value;
                }
            }

            return (intReturn > 0);
        }

        public void LoadFileAssociations()
        {
            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobAssociatedFiles";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
            dtData = objADO.GetDataTable(strStoredProcedure, "FileAssociations", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData != null && dtData.Rows.Count > 0)
            {
                mColFileAssociations = new FileAssociationCollection();
                FileAssociation fileMetadata = null;

                foreach (DataRow drMember in dtData.Rows)
                {
                    fileMetadata = new FileAssociation();
                    fileMetadata.Populate(drMember);
                    mColFileAssociations.Add(fileMetadata);
                }
            }
        }

        public bool ExistsInSchedulingSystem()
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strStoredProcedure = "selJobExistsInClick";
            int intReturn = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
            objParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
            objParameter.Direction = ParameterDirection.ReturnValue;
            colParameters.Add(objParameter);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;
            if (!objParameter.Value.Equals(DBNull.Value))
            {
                intReturn = (int)objParameter.Value;
            }

            return (intReturn > 0);
        }

        /// <summary>
        /// WorkItem No : 2404 , R3_AGA_1.5.5.1
        /// Returns the User whether assigned Jobs IsFirst or IsLastInstance
        /// </summary>
        /// <param name="UserId"></param>
        public void SetJobInstance(string UserId,out bool IsJobFirstInstance,out bool IsJobLastInstance)
        {
            IsJobFirstInstance = IsJobLastInstance = false;
            SqlDataReader drData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobFirstandLastInstance";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID.ToString()));
            colParameters.Add(new SqlParameter("@UserID", UserId));
            drData = objADO.GetDataReader(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (drData != null)
            {
                if (drData.Read())
                {
                    IsJobFirstInstance = Convert.ToBoolean(drData.GetValue(0));
                    IsJobLastInstance = Convert.ToBoolean(drData.GetValue(1));
                }
            }
        }


        public void IncludeRelationallyStoredData()
        {
            IncludeRelationallyStoredData(false);
        }

        public void IncludeRelationallyStoredData(bool IncludeAssignmentConstraints)
        {
            DataSet dsData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobRelationalContent";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            dsData = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dsData != null && dsData.Tables.Count > 2)
            {
                dsData.Tables[0].TableName = "Comments";
                dsData.Tables[1].TableName = "IncompleteHistory";
                dsData.Tables[2].TableName = "OnHoldReasons";
                if (dsData.Tables.Count > 3)
                {
                    dsData.Tables[3].TableName = "MaterialsRequested";
                }
                if (dsData.Tables.Count > 4)
                {
                    dsData.Tables[4].TableName = "AssignmentConstraints";
                }
                if (dsData.Tables.Count > 5)
                {
                    dsData.Tables[5].TableName = "AssignmentSkills";
                }

                mColComments = CommentAuditRecordCollection.Populate(dsData.Tables["Comments"]);
                mColIncompleteOutcomeHistory = JobIncompleteOutcomeCollection.Populate(dsData.Tables["IncompleteHistory"]);
                if (dsData.Tables.Count > 3)
                {
                    mColOnHoldReasons = OnHoldReasonCollection.Populate(dsData.Tables["OnHoldReasons"], dsData.Tables["MaterialsRequested"]);

                    if (dsData.Tables.Count > 4 && IncludeAssignmentConstraints)
                    {
                        //Load Assignment Constraints only ... ie. NO SKILLS
                        if (dsData.Tables.Count == 5)
                        {
                            mobjAssignmentConstraints = AssignmentConstraints.PopulateAssignmentConstraints(dsData.Tables["AssignmentConstraints"], null);
                        }
                        else if(dsData.Tables.Count > 5)
                        {
                            //Load Assignment Constraints WITH SKILLS
                            mobjAssignmentConstraints = AssignmentConstraints.PopulateAssignmentConstraints(dsData.Tables["AssignmentConstraints"], dsData.Tables["AssignmentSkills"]);
                        }
                    }

                }
                else
                {
                    mColOnHoldReasons = OnHoldReasonCollection.Populate(dsData.Tables["OnHoldReasons"]);
                }
            }
        }

        public TimeSpan GetLatestSessionDuration(DateTime sessionFinishTime) // This is the same as JobStatusChange.LastUpdated.
        {
            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobHistory";
            TimeSpan sessionDuration = TimeSpan.MinValue;

            colParameters.Add(new SqlParameter("@WMSourceID", this.ID));
            colParameters.Add(new SqlParameter("@StopDateTime", sessionFinishTime));
            dtData = objADO.GetDataTable(strStoredProcedure, "JobHistory", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData != null && dtData.Rows.Count > 0)
            {
                DataRow drMember = dtData.Rows[0];
                sessionDuration = ((DateTime)drMember["StopDateTime"]).Subtract((DateTime)drMember["StartDateTime"]);
            }

            return sessionDuration;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Find ID of the "First Job in the Chain"
        /// </summary>
        /// <returns></returns>
        public int FindAncestor()
        {
            int ancestorJobID = -1;

            DataAccess objADO = Domain.GetADOInstance();
            ArrayList colParameters = new ArrayList();
            SqlParameter ancestorJobIDOutput = null;
            string strStoredProcedure = "selJobAncestor";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            ancestorJobIDOutput = new SqlParameter("@AncestorJobID", SqlDbType.Int);
            ancestorJobIDOutput.Direction = ParameterDirection.Output;
            colParameters.Add(ancestorJobIDOutput);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!ancestorJobIDOutput.Value.Equals(DBNull.Value))
            {
                ancestorJobID = (int)ancestorJobIDOutput.Value;
            }

            return ancestorJobID;
        }

        #endregion
    }

    #endregion

    #region FileAssociationCollection class

    public partial class FileAssociationCollection
    {
        public bool Save(string wmSourceID, eWMSourceSystem sourceSystem)
        {
            return Save(wmSourceID, sourceSystem, eFileAssociationType.Unspecified);
        }

        public bool Save(string wmSourceID, eWMSourceSystem sourceSystem, eFileAssociationType fileAssociationType)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobFileAssociation";
            if (fileAssociationType == eFileAssociationType.UserRelated)
            {
                strStoredProcedure = "updUserFileAssociation";
            }
            int intReturn = 0;

            foreach (FileAssociation fileMetadata in this)
            {
                if (fileAssociationType != eFileAssociationType.UserRelated)
                {
                    colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
                    colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
                }
                else
                {
                    colParameters.Add(new SqlParameter("@UserID", wmSourceID));
                }
                colParameters.Add(new SqlParameter("@RawFileName", fileMetadata.Path.Substring(fileMetadata.Path.LastIndexOf(@"\") + 1)));
                colParameters.Add(new SqlParameter("@FilePath", fileMetadata.Path));
                colParameters.Add(new SqlParameter("@FileDesc", fileMetadata.Description));
                colParameters.Add(new SqlParameter("@AssociationType", fileMetadata.AssociationType.ToString()));
                colParameters.Add(new SqlParameter("@SizeInBytes", fileMetadata.SizeInBytes));
                colParameters.Add(new SqlParameter("@FileHash", fileMetadata.Hash));
                colParameters.Add(new SqlParameter("@IsEncrypted", fileMetadata.IsEncrypted));
                intReturn += objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                colParameters.Clear();
            }
            objADO = null;

            return (intReturn > 0);
        }

        public bool Save(int activityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updActivityFileAssociation";
            int intReturn = 0;

            foreach (FileAssociation fileMetadata in this)
            {
                colParameters.Add(new SqlParameter("@ActivityID", activityID));
                colParameters.Add(new SqlParameter("@RawFileName", fileMetadata.Path.Substring(fileMetadata.Path.LastIndexOf(@"\") + 1)));
                colParameters.Add(new SqlParameter("@FilePath", fileMetadata.Path));
                colParameters.Add(new SqlParameter("@FileDesc", fileMetadata.Description));
                colParameters.Add(new SqlParameter("@AssociationType", fileMetadata.AssociationType.ToString()));
                colParameters.Add(new SqlParameter("@SizeInBytes", fileMetadata.SizeInBytes));
                colParameters.Add(new SqlParameter("@FileHash", fileMetadata.Hash));
                colParameters.Add(new SqlParameter("@IsEncrypted", fileMetadata.IsEncrypted));
                intReturn += objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                colParameters.Clear();
            }
            objADO = null;

            return (intReturn > 0);
        }

        public bool Remove(FileAssociation fileAssocation, int activityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delActivityFileAssociation";
            int intRowsAffected = 0;

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@ActivityID", activityID));
            colParameters.Add(new SqlParameter("@FilePath", fileAssocation.Path));
            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }
    }

    #endregion

    #region JobUpdate class

    public partial class JobUpdate
    {
        #region Private Member Variables

        private int _archiveID = -1;

        #endregion

        #region Properties

        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ArchiveID
        {
            get { return this._archiveID; }
            set { this._archiveID = value; }
        }

        #endregion

        #region Public Methods

        public bool SaveSerializedObject()
        {
            return SaveSerializedObject(ArchiveID < 1);
        }

        public bool SaveSerializedObject(bool isNew)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter xmlParameter = null;
            SqlParameter returnValue = null;
            string serializedInstance = BusinessObjects.Base.Serialize(this.GetType(), this);
            string strStoredProcedure = "insJobUpdateSerialized";

            switch (isNew)
            {
                case false:
                    strStoredProcedure = "updJobUpdateSerialized";
                    colParameters.Add(new SqlParameter("@ID", this._archiveID));
                    xmlParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
                    xmlParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(serializedInstance, System.Xml.XmlNodeType.Document, null));
                    colParameters.Add(xmlParameter);
                    break;
                default:
                    returnValue = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
                    returnValue.Direction = ParameterDirection.ReturnValue;
                    colParameters.Add(returnValue);

                    colParameters.Add(new SqlParameter("@WMSourceID", mintID));
                    colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
                    xmlParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
                    xmlParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(serializedInstance, System.Xml.XmlNodeType.Document, null));
                    colParameters.Add(xmlParameter);
                    colParameters.Add(new SqlParameter("@HashValue", this.GetHashValueForInstance()));
                    break;
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            if (returnValue != null && returnValue.Value != null)
            {
                this._archiveID = (int)returnValue.Value;
            }
            objADO = null;

            return (intReturn > 0);
        }

        public bool DeleteSerializedObject()
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delJobUpdateSerialized";

            string strXML = BusinessObjects.Base.Serialize(this.GetType(), this);
            colParameters.Add(new SqlParameter("@ID", ArchiveID));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public int UpdateJobHistoryAndLostTime(string userID, int jobInstanceNumber, EngineerFeedback jobFeedback)
        {
            int historyID = SaveHistory(userID, jobInstanceNumber, jobFeedback != null && jobFeedback.Travel != null ? jobFeedback.Travel.MileageAtEndOfDay : 0);
            SaveHistoryLostTimes(historyID, jobInstanceNumber, jobFeedback);

            return historyID;
        }

        public bool UpdateDatabaseIsCompleteFlag()
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobIsCompleteFlag";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@IsComplete", mblnIsComplete));
            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Save Job History from Mobile
        /// </summary>
        /// <param name="userID">NT Login ID</param>
        /// <returns>New Job History ID</returns>
        private int SaveHistory(string userID, int jobInstanceNumber, int mileage)
        {
            DataAccess dataAccess = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter returnParameter = null;
            int historyID = -1;

            try
            {
                // Return Value Parameter...
                returnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                colParameters.Add(returnParameter);
                // Job ID
                colParameters.Add(new SqlParameter("@WMSourceID", this.ID));
                // Source System
                colParameters.Add(new SqlParameter("@WMSourceSystem", this.SourceSystem.ToString()));
                // Start Date Time
                colParameters.Add(new SqlParameter("@StartDateTime", this.StartDateTime));
                // Stop Date Time
                colParameters.Add(new SqlParameter("@StopDateTime", this.EndDateTime.Value));
                // Miles
                colParameters.Add(new SqlParameter("@Miles", mileage));
                // Job Instance Number
                colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
                // Last Updating User
                colParameters.Add(new SqlParameter("@LstUpdUsr", userID));
                // Update
                dataAccess.ExecuteSQL("insJobHistory", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

                // Return Value
                historyID = (int)returnParameter.Value;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            finally
            {
                dataAccess = null;
                colParameters = null;
            }

            // Return
            return historyID;
        }

        /// <summary>
        /// Save Lost Times from Mobile
        /// </summary>
        /// <param name="historyID">Jop History ID</param>
        private void SaveHistoryLostTimes(int historyID, int jobInstanceNumber, EngineerFeedback jobFeedback)
        {
            if (jobFeedback != null && jobFeedback.LostTime != null)
            {
                FinalBuild.DataAccess dataAccess = new DataAccess();
                ArrayList colParameters = new ArrayList();

                try
                {
                    foreach (LostTimeUpdate lostTimeUpdate in jobFeedback.LostTime)
                    {
                        if (lostTimeUpdate.Reason != null)  //  If it is null then it is bad data
                        {
                            // Job History ID
                            colParameters.Add(new SqlParameter("@JobHistoryID", historyID));
                            // Lost Time ID
                            colParameters.Add(new SqlParameter("@LostTimeID", lostTimeUpdate.Reason.ID));
                            // Lost Time Description ... check for null value as not mandatory
                            string description = lostTimeUpdate.Comments != null ? lostTimeUpdate.Comments : "";
                            colParameters.Add(new SqlParameter("@Description", description));
                            // Lost Time Minutes
                            colParameters.Add(new SqlParameter("@Minutes", lostTimeUpdate.Minutes));
                            // Job Instance Number
                            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
                            // Update
                            dataAccess.ExecuteSQL("insJobLostTime", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                            // Clear Parameters ready for next Lost Time
                            colParameters.Clear();
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw exception;
                }
                finally
                {
                    dataAccess = null;
                    colParameters = null;
                }
            }
        }

        #endregion
    }

    #endregion

    #region JobStatusChange class

    public partial class JobStatusChange
    {
        public bool Audit()
        {
            return Audit(-1);
        }

        public bool Audit(int jobInstanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insJobStatusAudit";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@Status", mobjStatus.ToString()));
            if (mdteLastUpdated == DateTime.MinValue)
            {
                mdteLastUpdated = DateTime.Now;
            }
            colParameters.Add(new SqlParameter("@ChangeDate", mdteLastUpdated));
            if (mstrUserID != null && mstrUserID != string.Empty)
            {
                colParameters.Add(new SqlParameter("@UserID", mstrUserID));
            }
            if (mstrService != null && mstrService != string.Empty)
            {
                colParameters.Add(new SqlParameter("@ServiceName", mstrService));
            }
            if (jobInstanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            }
            if (mstrWindowsIdentity == null)
            {
                mstrWindowsIdentity = Environment.UserName;
            }
            colParameters.Add(new SqlParameter("@WindowsIdentity", mstrWindowsIdentity));
            if (mobjIncompleteReason != null && !string.IsNullOrEmpty(mobjIncompleteReason.Description))
            {
                colParameters.Add(new SqlParameter("@IncompleteReason", mobjIncompleteReason.Description));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region OnHoldReason class

    public partial class OnHoldReason
    {
        public string FormatCommentsToString()
        {
            string formattedComments = string.Empty;
            if (this.Comments != null)
            {
                foreach (CommentAuditRecord comment in this.Comments)
                {
                    formattedComments += CommentAuditRecord.FormatCommentToString(comment) + Environment.NewLine;
                }
            }
            return formattedComments;
        }

        public bool Resolve(int jobID)
        {
            DataAccess objADO = new DataAccess(); // TaskStore
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobOnHoldReason";
            int intReturn = 0;

            if (mdteDateResolved.Equals(DateTime.MinValue))
            {
                mdteDateResolved = DateTime.Now;
            }
            if (mstrResolvingUser == null || mstrResolvingUser == string.Empty)
            {
                mstrResolvingUser = "SYSTEM";
            }

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@OnHoldReasonID", mintID));
            colParameters.Add(new SqlParameter("@IsResolved", true));
            colParameters.Add(new SqlParameter("@DateResolved", mdteDateResolved));
            colParameters.Add(new SqlParameter("@ResolvingUser", mstrResolvingUser));
            if (mColComments != null && mColComments.Count > 0)
            {
                SqlParameter xmlParameter = new SqlParameter("@Comments", SqlDbType.Xml);
                xmlParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(BusinessObjects.Base.Serialize(mColComments.GetType(), mColComments), System.Xml.XmlNodeType.Document, null));
                colParameters.Add(xmlParameter);
            }

            intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public int Save(int jobID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insJobOnHoldReason";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@OnHoldReasonID", mintID));
            colParameters.Add(new SqlParameter("@OnHoldReasonCode", mstrCode));
            colParameters.Add(new SqlParameter("@OnHoldReasonDesc", mstrDescription));
            if (mColComments != null && mColComments.Count > 0)
            {
                SqlParameter xmlParameter = new SqlParameter("@Comments", SqlDbType.Xml);
                xmlParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(BusinessObjects.Base.Serialize(mColComments.GetType(), mColComments), System.Xml.XmlNodeType.Document, null));
                colParameters.Add(xmlParameter);
            }

            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return intRowsAffected;
        }
    }

    #endregion

    #region Activity class

    public partial class Activity
    {
        #region Public Methods

        public virtual bool Save()
        {
            return Save(null);
        }

        public bool Save(System.Xml.XmlDocument streamTemplate)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            DataAccess.ArrayListParameter arrayParameter = null;
            SqlParameter returnParameter = null;
            SqlParameter xmlParameter = null;
            string strStoredProcedure = "updActivity";
            int intReturn = 0;

            if (this.ID > 0)
            {
                colParameters.Add(new SqlParameter("@ActivityID", this.ID));
            }
            else
            {
                strStoredProcedure = "insActivity";
                returnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                colParameters.Add(returnParameter);
            }

            colParameters.Add(new SqlParameter("@ActivityCode", this.Code));
            colParameters.Add(new SqlParameter("@CategoryID", mintParentCategoryID));
            colParameters.Add(new SqlParameter("@ActivityDesc", this.Description));
            colParameters.Add(new SqlParameter("@IsReactive", mblnIsReactive));
            colParameters.Add(new SqlParameter("@IsVisibleInClick", mblnIsVisibleInClick));
            colParameters.Add(new SqlParameter("@IsVisibleInField", mblnIsVisibleInField));
            colParameters.Add(new SqlParameter("@AreUtilityPlansRequired", mblnAreUtilityPlansRequired));
            colParameters.Add(new SqlParameter("@IsProject", mblnIsProject));
            colParameters.Add(new SqlParameter("@Duration", mintDuration));
            colParameters.Add(new SqlParameter("@Comments", mstrComments));
            colParameters.Add(new SqlParameter("@MethodOfDetection", mstrMethodOfDetection));
            colParameters.Add(new SqlParameter("@AreSkillsMappedAtActivityLevel", mblnAreSkillsMappedAtActivityLevel)); // AT release 3
            colParameters.Add(new SqlParameter("@IsFailure", mblnIsFailure));

            if (!string.IsNullOrEmpty(this.DistrictGroup))
            {
                colParameters.Add(new SqlParameter("@DistrictGroup", this.DistrictGroup));
            }
            if (!mblnIsProject)
            {
                colParameters.Add(new SqlParameter("@SchedulingPriorityID", mobjSchedulingPriority.ID));
                colParameters.Add(new SqlParameter("@DispatchPriorityID", mobjDispatchPriority.ID));
                if (mstrEarlyStartTime != null && mstrEarlyStartTime != string.Empty)
                {
                    colParameters.Add(new SqlParameter("@EarlyStartTime", mstrEarlyStartTime));
                }
                colParameters.Add(new SqlParameter("@IsDueDateFixed", mblnIsDueDateFixed));
                colParameters.Add(new SqlParameter("@EarlyStartDaysBeforeDue", mintEarlyStartDaysBeforeDue));

                if (mColOnHoldReasons != null && mColOnHoldReasons.Count > 0)
                {
                    int[] onHoldReasonIDs = new int[mColOnHoldReasons.Count];
                    for (int intIndex = 0; intIndex < mColOnHoldReasons.Count; intIndex++)
                    {
                        onHoldReasonIDs[intIndex] = mColOnHoldReasons[intIndex].ID;
                    }
                    arrayParameter = new DataAccess.ArrayListParameter("OnHoldReasons", onHoldReasonIDs);
                }
            }
            if (streamTemplate != null)
            {
                xmlParameter = new SqlParameter("@StreamTemplate", SqlDbType.Xml);
                xmlParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(streamTemplate.OuterXml, System.Xml.XmlNodeType.Document, null));
                colParameters.Add(xmlParameter);
            }

            if (arrayParameter != null)
            {
                intReturn = objADO.ExecuteSQL(strStoredProcedure, arrayParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            else
            {
                intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            objADO = null;

            if (returnParameter != null && !returnParameter.Value.Equals(DBNull.Value))
            {
                this.ID = (int)returnParameter.Value;
            }

            if (mblnIsProject)
            {
                SaveActivityAssociations();
            }

            FileAssociationCollection currentFileAssociations = GetFileAssociations();
            if (mColFileAssociations == null || mColFileAssociations.Count < 1)
            {
                if (currentFileAssociations != null && currentFileAssociations.Count > 0)
                {
                    // Remove all existing File Associations from database
                    foreach (FileAssociation fileToRemove in currentFileAssociations)
                    {
                        currentFileAssociations.Remove(fileToRemove, this.ID);
                    }
                }
            }
            else
            {
                mColFileAssociations.Save(this.ID);
                if (currentFileAssociations != null && currentFileAssociations.Count > 0)
                {
                    // Remove any File Associations not in ATM updated set from database
                    foreach (FileAssociation fileToRemove in currentFileAssociations)
                    {
                        bool fileFound = false;
                        foreach (FileAssociation fileToKeep in mColFileAssociations)
                        {
                            if (fileToRemove.Path == fileToKeep.Path)
                            {
                                fileFound = true;
                                break;
                            }
                        }
                        if (!fileFound)
                        {
                            currentFileAssociations.Remove(fileToRemove, this.ID);
                        }
                    }
                }
            }
            // AT 160911 need to save skills matrix here
            // Child collection special case - Skills
            if (this.AreSkillsMappedAtActivityLevel)
            {
                SaveSkillsMatrix();
            }


            return (intReturn > 0);
        }
        //AT 270911
        public bool RemoveTasksFromActivity(string strTasks, int intActivityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delActivityTasks";
            int intReturn = 0;

            if (intActivityID > 0)
            {
                colParameters.Add(new SqlParameter("@TaskIDs", strTasks));
                colParameters.Add(new SqlParameter("@ActivityID", intActivityID));
                intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            objADO = null;

            return intReturn > 0;


        }
        public bool AddTasksToActivity(string strTasks, int intActivityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insActivityTasks";
            int intReturn = 0;

            if (intActivityID > 0)
            {
                colParameters.Add(new SqlParameter("@TaskIDs", strTasks));
                colParameters.Add(new SqlParameter("@ActivityID", intActivityID));
                intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            objADO = null;

            return intReturn > 0;


        }
        #endregion

        #region Private Methods

        private bool SaveActivityAssociations()
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            DataAccess.ArrayListParameter arrayParameter = null;
            string strStoredProcedure = "updProjectActivities";
            int intReturn = 0;

            // 1. Update Child Activities (remove current db records not in list passed in)
            colParameters.Add(new SqlParameter("@ActivityID", this.ID));
            if (mColAssociatedActivities != null && mColAssociatedActivities.Count > 0)
            {
                if (mColAssociatedActivities != null && mColAssociatedActivities.Count > 0)
                {
                    int[] childActivities = new int[mColAssociatedActivities.Count];
                    for (int intIndex = 0; intIndex < mColAssociatedActivities.Count; intIndex++)
                    {
                        childActivities[intIndex] = mColAssociatedActivities[intIndex].ID;
                    }
                    arrayParameter = new DataAccess.ArrayListParameter("ChildActivities", childActivities);
                }
            }
            if (arrayParameter != null)
            {
                intReturn = objADO.ExecuteSQL(strStoredProcedure, arrayParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            else
            {
                intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }

            // 2. Update LeadTime values for any AssociatedActivities present
            if (mColAssociatedActivities != null && mColAssociatedActivities.Count > 0)
            {
                strStoredProcedure = "updProjectActivityAttributes";
                colParameters.Clear();
                foreach (AssociatedActivityReference activityReference in mColAssociatedActivities)
                {
                    colParameters.Add(new SqlParameter("@ActivityID", this.ID));
                    colParameters.Add(new SqlParameter("@ChildActivityID", activityReference.ID));
                    colParameters.Add(new SqlParameter("@LeadTime", activityReference.LeadTime));
                    intReturn += objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                    colParameters.Clear();
                }
            }

            objADO = null;

            return (intReturn > 0);
        }
        private bool SaveSkillsMatrix()
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updActivitySkillsPerResource";
            int intReturn = 0;

            int noOfResourcesRequired = 0;
            if (this.SkillsBreakdown != null && this.SkillsBreakdown.ResourceProfiles != null)
            {
                noOfResourcesRequired = this.SkillsBreakdown.ResourceProfiles.Count;
                for (int resourceIndex = 0; resourceIndex < this.SkillsBreakdown.ResourceProfiles.Count; resourceIndex++)
                {
                    colParameters.Clear();
                    if (this.SkillsBreakdown.ResourceProfiles[resourceIndex].Skills != null)
                    {
                        string commaSeparatedList = string.Empty;
                        for (int intIndex = 0; intIndex < this.SkillsBreakdown.ResourceProfiles[resourceIndex].Skills.Count; intIndex++)
                        {
                            commaSeparatedList += this.SkillsBreakdown.ResourceProfiles[resourceIndex].Skills[intIndex].Code + ",";
                        }
                        commaSeparatedList = commaSeparatedList.Substring(0, commaSeparatedList.Length - 1);

                        // Add @Skills parameter (else @Skills is null and so will rightly remove any Skills present)
                        colParameters.Add(new SqlParameter("@Skills", commaSeparatedList));
                    }

                    // Update Skills for this Resource Index - NB: setting\not setting @Skills value in the loop above
                    colParameters.Add(new SqlParameter("@ActivityID", this.ID));
                    colParameters.Add(new SqlParameter("@ResourceIndex", resourceIndex));
                    intReturn += objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                }
            }

            // Remove any Resource-Skill records no longer required
            strStoredProcedure = "delActivitySkills ";
            colParameters.Clear();
            colParameters.Add(new SqlParameter("@ActivityID", this.ID));
            colParameters.Add(new SqlParameter("@NoOfResourcesRequired", noOfResourcesRequired));
            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            objADO = null;

            return (intReturn > 0);
        }

        #endregion
    }

    #endregion

    #region ActivityTaskTemplate class

    public partial class ActivityTaskTemplate
    {
        int _intSaveCounter = 1; // BUG 2275
        public bool SaveActivitySequence(int activityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updTaskLibraryActivitySequence";
            int intReturn = 0;

            if (this.ID > 0)
            {
                colParameters.Add(new SqlParameter("@TaskID", this.ID));
                colParameters.Add(new SqlParameter("@ActivityID", activityID));
                colParameters.Add(new SqlParameter("@SequenceNo", this.Sequence));
                intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            objADO = null;

            return intReturn > 0;
        }

        /// <summary>
        /// Release 3 : work item 2422: Added ActivityId parament to save method
        /// </summary>
        /// <param name="ActivityID"></param>
        /// <returns></returns>
        public bool Save(int ActivityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            DataAccess.ArrayListParameter arrayParameter = null;
            SqlParameter returnParameter = null;
            string strStoredProcedure = "updTaskLibrary";
            int intReturn = 0;

            if (this.ID > 0)
            {
                colParameters.Add(new SqlParameter("@TaskID", this.ID));
            }
            else
            {
                strStoredProcedure = "insTaskLibrary";
                returnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                colParameters.Add(returnParameter);
            }
            colParameters.Add(new SqlParameter("@Duration", this.Duration));
            colParameters.Add(new SqlParameter("@IsCritical", this.IsCritical));
            colParameters.Add(new SqlParameter("@TaskDesc", this.Description));

            if (ActivityID != -1)
            {
                colParameters.Add(new SqlParameter("@ActivityID", ActivityID));
            }

            if (this.TaskUpdateType != null)
            {
                colParameters.Add(new SqlParameter("@TypeName", this.TaskUpdateType.Name));
            }

            // Child collections
            if (this.MaterialsRequired != null && this.MaterialsRequired.Count > 0)
            {
                string commaSeparatedList = string.Empty;
                for (int intIndex = 0; intIndex < this.MaterialsRequired.Count; intIndex++)
                {
                    commaSeparatedList += this.MaterialsRequired[intIndex].ID.ToString() + ",";
                }
                commaSeparatedList = commaSeparatedList.Substring(0, commaSeparatedList.Length - 1);
                colParameters.Add(new SqlParameter("@Materials", commaSeparatedList));
            }

            if (mColAssociatedActivities != null && mColAssociatedActivities.Count > 0)
            {
                int[] activities = new int[mColAssociatedActivities.Count];
                for (int intIndex = 0; intIndex < activities.Length; intIndex++)
                {
                    activities[intIndex] = mColAssociatedActivities[intIndex].ID;
                }
                arrayParameter = new DataAccess.ArrayListParameter("Activities", activities);
            }
            // Execute SQL
            if (arrayParameter != null)
            {
                intReturn = objADO.ExecuteSQL(strStoredProcedure, arrayParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            else
            {
                intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }

            if (returnParameter != null && !returnParameter.Value.Equals(DBNull.Value))
            {
                this.ID = (int)returnParameter.Value;
            }
            objADO = null;

            // Child collection special case - Skills
            SaveSkillsMatrix();


            objADO = null;

            return (intReturn > 0);
        }

        private bool SaveSkillsMatrix()
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updTaskLibrarySkillsPerResource";
            int intReturn = 0;

            int noOfResourcesRequired = 0;
            if (this.SkillsBreakdown != null && this.SkillsBreakdown.ResourceProfiles != null)
            {
                noOfResourcesRequired = this.SkillsBreakdown.ResourceProfiles.Count;
                for (int resourceIndex = 0; resourceIndex < this.SkillsBreakdown.ResourceProfiles.Count; resourceIndex++)
                {
                    colParameters.Clear();
                    if (this.SkillsBreakdown.ResourceProfiles[resourceIndex].Skills != null)
                    {
                        string commaSeparatedList = string.Empty;
                        for (int intIndex = 0; intIndex < this.SkillsBreakdown.ResourceProfiles[resourceIndex].Skills.Count; intIndex++)
                        {
                            commaSeparatedList += this.SkillsBreakdown.ResourceProfiles[resourceIndex].Skills[intIndex].Code + ",";
                        }
                        commaSeparatedList = commaSeparatedList.Substring(0, commaSeparatedList.Length - 1);

                        // Add @Skills parameter (else @Skills is null and so will rightly remove any Skills present)
                        colParameters.Add(new SqlParameter("@Skills", commaSeparatedList));
                    }

                    // Update Skills for this Resource Index - NB: setting\not setting @Skills value in the loop above
                    colParameters.Add(new SqlParameter("@TaskID", this.ID));
                    colParameters.Add(new SqlParameter("@ResourceIndex", resourceIndex));
                    intReturn += objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                }
            }

            // Remove any Resource-Skill records no longer required
            strStoredProcedure = "delTaskLibrarySkills ";
            colParameters.Clear();
            colParameters.Add(new SqlParameter("@TaskID", this.ID));
            colParameters.Add(new SqlParameter("@NoOfResourcesRequired", noOfResourcesRequired));
            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region ActivityReference class

    public partial class ActivityReference
    {
        public string GetEarlyStartTime(out bool isDueDateFixed, out int earlyStartDaysBeforeDue)
        {
            string earlyStartTime = string.Empty;
            isDueDateFixed = false;
            earlyStartDaysBeforeDue = 0;

            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            SqlParameter earlyStartTimeOutput = null;
            SqlParameter isDueDateFixedOutput = null;
            SqlParameter earlyStartDaysBeforeDueOutput = null;
            string strStoredProcedure = "selActivityEarlyStartTime";

            colParameters.Add(new SqlParameter("@ActivityID", mintID));
            earlyStartTimeOutput = new SqlParameter("@EarlyStartTime", SqlDbType.Char, 5);
            earlyStartTimeOutput.Direction = ParameterDirection.Output;
            colParameters.Add(earlyStartTimeOutput);
            isDueDateFixedOutput = new SqlParameter("@IsDueDateFixed", SqlDbType.Bit);
            isDueDateFixedOutput.Direction = ParameterDirection.Output;
            colParameters.Add(isDueDateFixedOutput);
            earlyStartDaysBeforeDueOutput = new SqlParameter("@EarlyStartDaysBeforeDue", SqlDbType.Int);
            earlyStartDaysBeforeDueOutput.Direction = ParameterDirection.Output;
            colParameters.Add(earlyStartDaysBeforeDueOutput);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!earlyStartTimeOutput.Value.Equals(DBNull.Value))
            {
                earlyStartTime = earlyStartTimeOutput.Value.ToString().Trim();
            }
            if (!isDueDateFixedOutput.Value.Equals(DBNull.Value))
            {
                isDueDateFixed = (bool)isDueDateFixedOutput.Value;
            }
            if (isDueDateFixed)
            {
                if (!earlyStartDaysBeforeDueOutput.Value.Equals(DBNull.Value))
                {
                    earlyStartDaysBeforeDue = (int)earlyStartDaysBeforeDueOutput.Value;
                }
            }

            return earlyStartTime;
        }

        public PriorityLookupData GetPriorityLookupData()
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            DataSet dsResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selActivityPriorityData";

            colParameters.Add(new SqlParameter("@ActivityID", mintID));

            dsResults = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return PriorityLookupData.Populate(dsResults);
        }

        /// <summary>
        /// Sets value of Activity.Path which contains all Categories above Activity
        /// </summary>
        /// <returns></returns>
        public string SetPath()
        {
            string path = string.Empty;

            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            ArrayList colParameters = new ArrayList();
            SqlParameter pathOutParameter = null;
            string strStoredProcedure = "selActivityPath";

            colParameters.Add(new SqlParameter("@ActivityID", mintID));
            pathOutParameter = new SqlParameter("@Path", SqlDbType.VarChar, 1000);
            pathOutParameter.Direction = ParameterDirection.Output;
            colParameters.Add(pathOutParameter);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!pathOutParameter.Value.Equals(DBNull.Value))
            {
                path = pathOutParameter.Value.ToString();
            }

            // Set private field
            mstrPath = path;

            return path;
        }

        /// <summary>
        /// 1. Used to work out what DB File records need to be removed post ATM changes
        /// 2. Used to get list of Files to associate with Job by WIS (JobRequestProcessor)
        /// </summary>
        /// <returns></returns>
        public FileAssociationCollection GetFileAssociations()
        {
            FileAssociationCollection fileAssociations = null;
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selActivityFileAssociations";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@ActivityID", mintID));
            dtResults = objADO.GetDataTable(strStoredProcedure, "FileAssociations", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                FileAssociation fileMetadata = null;
                fileAssociations = new FileAssociationCollection();
                foreach (DataRow drFile in dtResults.Rows)
                {
                    fileMetadata = new FileAssociation();
                    fileMetadata.Populate(drFile);
                    fileAssociations.Add(fileMetadata);
                }
            }

            return fileAssociations;
        }
    }

    #endregion

    #region JobFailure class

    public partial class JobFailure
    {
        public bool Save()
        {
            return Save(null);
        }

        public bool Save(string xmlSerializedContent)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insFailedJobAudit";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@FailureType", mobjType.ToString()));
            if (mdteDateRecorded == DateTime.MinValue)
            {
                mdteDateRecorded = DateTime.Now;
            }
            colParameters.Add(new SqlParameter("@FailureDate", mdteDateRecorded));
            if (mstrExceptionMessage != null && mstrExceptionMessage != string.Empty)
            {
                colParameters.Add(new SqlParameter("@ExceptionMessage", mstrExceptionMessage));
            }
            colParameters.Add(new SqlParameter("@RetryCount", mintRetryCount));

            if (!string.IsNullOrEmpty(xmlSerializedContent))
            {
                SqlParameter objParameter = new SqlParameter("@Serialized", SqlDbType.Xml);
                objParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(xmlSerializedContent, System.Xml.XmlNodeType.Document, null));
                colParameters.Add(objParameter);
            }
            if (!string.IsNullOrEmpty(mstrServiceWrapperType))
            {
                colParameters.Add(new SqlParameter("@ServiceWrapperType", mstrServiceWrapperType));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region TaskUpdate

    public partial class TaskUpdate
    {
        public bool Save(int jobID, eWMSourceSystem sourceSystem, DateTime dateRecorded)
        {
            return Save(jobID, sourceSystem, dateRecorded, -1);
        }

        public bool Save(int jobID, eWMSourceSystem sourceSystem, DateTime dateRecorded, int jobInstanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strStoredProcedure = "insJobTaskUpdate";

            if (mdteDateUpdated == DateTime.MinValue)
            {
                mdteDateUpdated = dateRecorded;
            }

            string strXML = BusinessObjects.Base.Serialize(this.GetType(), this);
            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@TypeName", this.GetType().AssemblyQualifiedName));
            objParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
            objParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(strXML, System.Xml.XmlNodeType.Document, null));
            colParameters.Add(objParameter);
            colParameters.Add(new SqlParameter("@IsComplete", true));
            colParameters.Add(new SqlParameter("@ChangeDate", dateRecorded));  // NB: mdteDateUpdated not used as may have been set on the client
            colParameters.Add(new SqlParameter("@UserID", mstrUserID));
            if (mintID > 0)
            {
                colParameters.Add(new SqlParameter("@TaskID", mintID));
            }
            if (jobInstanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            }
            if (mintAssetID > 0)
            {
                colParameters.Add(new SqlParameter("@AssetID", mintAssetID));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region Engineer NA

    public partial class EngineerNonAvailabilityCollection
    {
        /// <summary>
        /// Save, removing redundant items by cutoffDateForInclusion beforehand
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="cutoffDateForInclusion"></param>
        /// <returns></returns>
        public bool Save(string userID, DateTime cutoffDateForInclusion)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strStoredProcedure = "updUserNonAvailability";

            if (cutoffDateForInclusion != DateTime.MinValue)
            {
                RemoveRedundantItems(cutoffDateForInclusion);
            }

            string strXML = BusinessObjects.Base.Serialize(this.GetType(), this);
            colParameters.Add(new SqlParameter("@UserID", userID));
            objParameter = new SqlParameter("@NonAvailabilitySerialized", SqlDbType.Xml);
            objParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(strXML, System.Xml.XmlNodeType.Document, null));

            if (this.Count == 0)
            {
                objParameter.Value = System.DBNull.Value;
            }

            colParameters.Add(objParameter);

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public bool Save(string userID)
        {
            return Save(userID, DateTime.MinValue);
        }

        /// <summary>
        /// Exposed as public method so Mobile can filter Redundant without calling Save
        /// </summary>
        /// <param name="cutoffDateForInclusion"></param>
        public void RemoveRedundantItems(DateTime cutoffDateForInclusion)
        {
            if (this.Count > 0)
            {
                for (int appointmentIndex = (this.Count - 1); appointmentIndex > -1; appointmentIndex--)
                {
                    if (this[appointmentIndex].EndDate.Date < cutoffDateForInclusion.Date)
                    {
                        this.RemoveAt(appointmentIndex);
                    }
                }
            }
        }
    }

    #endregion

    #region AssociatedJobReference

    public partial class AssociatedJobReference
    {
        /// <summary>
        /// Return the instance that is effectively the latest, i.e. the latest instance that is not Cancelled or Incomplete (as this latter will have generated a new instance to
        /// replace it).
        /// </summary>
        /// <returns>The latest instance (or -1 if unsuccessful or null).</returns>
        public int GetEffectiveLatestInstance()
        {
            DataAccess objADO = new DataAccess();
            SqlParameter[] colParameters = new SqlParameter[2];
            string strStoredProcedure = "selLatestInstanceForJob";

            colParameters[0] = new SqlParameter("@WMSourceID", mintID);
            colParameters[1] = new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString());

            DataSet result = objADO.GetDataSet(strStoredProcedure, colParameters);
            objADO = null;

            if (result != null && result.Tables != null && result.Tables.Count > 0 && result.Tables[0].Rows.Count > 0)
            {
                if (result.Tables[0].Rows[0][0] == DBNull.Value)
                {
                    return -1;
                }
                else
                {
                    return (int)result.Tables[0].Rows[0][0];
                }
            }
            else
            {
                return -1;
            }
        }
    }

    #endregion

    #region Appointment

    public partial class Appointment
    {
        public bool SaveNew(int jobID, int jobInstanceNumber, bool informEngineer)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strStoredProcedure = "insJobAppointment";

            if (this.ID < 1)
            {
                this.Responsibility = this.LastUser;
            }

            string strXML = BusinessObjects.Base.Serialize(this.GetType(), this);
            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            colParameters.Add(new SqlParameter("@ChangeUser", this.LastUser));
            objParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
            objParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(strXML, System.Xml.XmlNodeType.Document, null));
            colParameters.Add(objParameter);
            if (informEngineer)
            {
                colParameters.Add(new SqlParameter("@DateChangeDetected", DateTime.Now));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public bool SaveSerializedObject(int jobID, int jobInstanceNumber, string changeUser, bool informEngineer)
        {
            return SaveSerializedObject(jobID, jobInstanceNumber, informEngineer, true, false);
        }

        public bool SaveSerializedObject(int jobID, int jobInstanceNumber, bool informEngineer, bool simpleUpdate,
                                            bool deactivatingWithoutReplacing)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strStoredProcedure = "updJobAppointment";

            if (this.ID < 1)
            {
                this.Responsibility = this.LastUser;
            }

            string strXML = BusinessObjects.Base.Serialize(this.GetType(), this);
            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            colParameters.Add(new SqlParameter("@ChangeUser", this.LastUser));
            objParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
            objParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(strXML, System.Xml.XmlNodeType.Document, null));
            colParameters.Add(objParameter);
            if (informEngineer)
            {
                colParameters.Add(new SqlParameter("@DateChangeDetected", DateTime.Now));
            }
            colParameters.Add(new SqlParameter("@DeactivatingWithoutReplacing", deactivatingWithoutReplacing));
            colParameters.Add(new SqlParameter("@IsInternal", this.IsInternal));
            colParameters.Add(new SqlParameter("@SimpleUpdate", simpleUpdate));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region StatusConflict class

    public partial class StatusConflict
    {
        public bool Save()
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insStatusConflict";

            colParameters.Add(new SqlParameter("@WMSourceID", mintID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", mobjSourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@ProposedStatus", mobjProposedStatus.ToString()));
            colParameters.Add(new SqlParameter("@CurrentStatus", mobjCurrentStatus.ToString()));
            if (mdteLastUpdated == DateTime.MinValue)
            {
                mdteLastUpdated = DateTime.Now;
            }
            colParameters.Add(new SqlParameter("@ChangeDate", mdteLastUpdated));
            if (mstrUserID != null && mstrUserID != string.Empty)
            {
                colParameters.Add(new SqlParameter("@UserID", mstrUserID));
            }
            if (mstrService != null && mstrService != string.Empty)
            {
                colParameters.Add(new SqlParameter("@ServiceName", mstrService));
            }
            if (mintInstanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", mintInstanceNumber));
            }
            if (mstrWindowsIdentity == null)
            {
                mstrWindowsIdentity = Environment.UserName;
            }
            colParameters.Add(new SqlParameter("@WindowsIdentity", mstrWindowsIdentity));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region CommentAuditRecord class

    public partial class CommentAuditRecord
    {
        public int Save(int jobID, eWMSourceSystem sourceSystem, bool informEngineers)
        {
            if (string.IsNullOrEmpty(mstrText))
            {
                return 0;
            }

            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insJobComment";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@ChangeDate", mdteChangeDate));
            colParameters.Add(new SqlParameter("@ChangeUser", mstrChangeUser));
            colParameters.Add(new SqlParameter("@IsEngineerComment", mblnIsEngineerComment));
            //CR : 2396 , IsCommentsCritical is used for filtering whether comments are critical / normal
            //Changed as per RAB's review comments  to IsCritical
            colParameters.Add(new SqlParameter("@IsCritical", mblnIsCritical));
            colParameters.Add(new SqlParameter("@CommentType", mobjType.ToString()));
            colParameters.Add(new SqlParameter("@CommentText", mstrText));
            if (informEngineers)
            {
                colParameters.Add(new SqlParameter("@InformEngineers", true));
            }
            if (!string.IsNullOrEmpty(mstrSortExpression))
            {
                colParameters.Add(new SqlParameter("@SortExpression", mstrSortExpression));
            }

            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return intRowsAffected;
        }
    }

    #endregion

    #region Gang

    public partial class Gang
    {
        /// <summary>
        /// Handles New as well as Existing
        /// </summary>
        /// <param name="gangMember"></param>
        /// <returns></returns>
        public bool SaveMember(Engineer gangMember)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updGangMember";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@GangID", this.UserID));
            colParameters.Add(new SqlParameter("@MemberID", gangMember.UserID));
            colParameters.Add(new SqlParameter("@MemberSequence", gangMember.SequenceNo));

            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }

        public bool RemoveMember(Engineer gangMember)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delGangMember";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@GangID", this.UserID));
            colParameters.Add(new SqlParameter("@MemberID", gangMember.UserID));

            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }
    }

    #endregion

    #region Material

    public partial class Material
    {
        public bool Request(int jobID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updMaterialsRequested";

            colParameters.Add(new SqlParameter("@JobID", jobID));
            colParameters.Add(new SqlParameter("@MaterialDesc", mstrDescription));
            colParameters.Add(new SqlParameter("@Quantity", mdecQuantity));
            // colParameters.Add(new SqlParameter("@DateReceived", dateReceived));
            // colParameters.Add(new SqlParameter("@UserID", resolvingUserID));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public bool RecordUse(int jobID, string userID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insMaterialsUsed";

            colParameters.Add(new SqlParameter("@JobID", jobID));
            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@Material", mstrDescription));
            colParameters.Add(new SqlParameter("@Quantity", mdecQuantity));
      
            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public bool RecordPickup(string userID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insMaterialsCollected";

            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@Material", mstrCode));
            colParameters.Add(new SqlParameter("@Quantity", mdecQuantity));
            colParameters.Add(new SqlParameter("@DateAcquired", DateTime.Now));
            // colParameters.Add(new SqlParameter("@Depot", depot));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion


    #region StockItemCollection

    public partial class StockItemCollection
    {

        public bool Update(int jobID)
        {
            return UpdateStockItems(jobID);
        }

        public bool Update(bool updateByMaterial)
        {
            if (updateByMaterial)
            {
                foreach (StockItem item in this)
                {
                    //If this is an UnAllocate then we might not have a location .. so run a check
                    string location = item.Location != null ? item.Location.Code : string.Empty;
                    
                    if (!UpdateStockItems(item.JobID, item.Material, location, item.Quantity, false))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return UpdateStockItems(-1);
            }
        }

        public bool Update(bool updateByMaterial, bool unallocate)
        {
            if (updateByMaterial)
            {
                foreach (StockItem item in this)
                {
                    //If this is an UnAllocate then we might not have a location .. so run a check
                    string location = item.Location != null ? item.Location.Code : string.Empty;
                    
                    if (!UpdateStockItems(item.JobID, item.Material, location, item.Quantity, unallocate))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return UpdateStockItems(-1);
            }
        }

        public bool Update()
        {
            return UpdateStockItems(-1);
        }

        private bool UpdateStockItems(int jobID)
        {
            DataAccess objADO;
            StringBuilder sb = new StringBuilder();
            string stockIDs = string.Empty;
            DateTime? dataRetrievedDate = (this[0] as StockItem).DataRetrievedDate;
            foreach (StockItem item in this)
            {
                sb.Append(item.ID.ToString() + ",");
            }

            stockIDs = sb.ToString();
            if(stockIDs.EndsWith(","))
            {
                stockIDs = stockIDs.Remove(stockIDs.Length - 1);
            }

            if (this.UpdateInsideTransaction && this.StockDataAccess != null)
            {
                objADO = this.StockDataAccess;
                
            }
            else
            {
                objADO = new DataAccess();
            }

            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updStock";

            colParameters.Add(new SqlParameter("@StockIDs", stockIDs));
            if (jobID > 0)
            {
                colParameters.Add(new SqlParameter("@JobID", jobID));
            }

            if (dataRetrievedDate != null && dataRetrievedDate!= DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@DataRetrieved", dataRetrievedDate));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            if (!this.UpdateInsideTransaction)
            {
                objADO = null;
            }

            return (intReturn > 0);
        }


        private bool UpdateStockItems(int jobID, Material material, string locationCode, int quantity, bool unallocateJob)
        {
            DataAccess objADO;
            StringBuilder sb = new StringBuilder();
            string stockIDs = string.Empty;
            StockItem objStockItem = this[0] as StockItem;
            DateTime? dataRetrievedDate = objStockItem.DataRetrievedDate;

            if (this.UpdateInsideTransaction && this.StockDataAccess != null)
            {
                objADO = this.StockDataAccess;
            }
            else
            {
                objADO = new DataAccess();
            }

            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updStockByMaterialAndJob";

            colParameters.Add(new SqlParameter("@JobID", jobID));
            colParameters.Add(new SqlParameter("@MaterialID", material.ID));
            //Now pass in the Material's description .. in case there is no valid Material ID
            colParameters.Add(new SqlParameter("@MaterialDesc", material.Description));
            colParameters.Add(new SqlParameter("@Quantity", quantity));

            if (!string.IsNullOrEmpty(locationCode))
            {
                colParameters.Add(new SqlParameter("@StockLocationCode", locationCode));
            }

            if (unallocateJob)
            {
                colParameters.Add(new SqlParameter("@ClearJobID", 1));
            }

            if (dataRetrievedDate != null && dataRetrievedDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@DataRetrieved", dataRetrievedDate));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            if (!this.UpdateInsideTransaction)
            {
                objADO = null;
            }

            return (intReturn > 0);
        }


        public bool Delete()
        {
            return DeleteStockItems(-1);
        }

        public bool Delete(int quantity)
        {
            return DeleteStockItems(quantity);
        }

        private bool DeleteStockItems(int quantity)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delStockByMaterialAndJob";

            colParameters.Add(new SqlParameter("@MaterialID", this[0].Material.ID));
            //Now pass in the Material's description .. in case there is no valid Material ID
            colParameters.Add(new SqlParameter("@MaterialDesc", this[0].Material.Description));
            colParameters.Add(new SqlParameter("@JobID", this[0].JobID));

            //If no quantity given then ALL matching records will be deleted
            if (quantity > 0)
            {
                colParameters.Add(new SqlParameter("@Quantity", quantity));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);


        }



    }

    #endregion

    #region StockItem

    public partial class StockItem
    {
        public bool Update()
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updStock";
            DateTime? dataRetrievedDate = this.DataRetrievedDate;

            colParameters.Add(new SqlParameter("@StockID", this.ID));
            if (this.JobID > 0)
            {
                colParameters.Add(new SqlParameter("@JobID", this.JobID));
            }
            if (dataRetrievedDate != null && dataRetrievedDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@DataRetrieved", dataRetrievedDate));
            }
            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public bool Delete()
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delStock";

            colParameters.Add(new SqlParameter("@StockID", this.ID));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

    }

    #endregion

    #region AssignmentConstraints

    public partial class AssignmentConstraints
    {

        public bool Save()
        {
            DataAccess objADO = null;
            ArrayList colParameters = new ArrayList();
            DataAccess.ArrayListParameter arrayParameter = null;
            SqlParameter returnParameter = null;
            string strStoredProcedure = "insAssignmentContraints";
            int returnID = 0;

            //SQL SP Paremeters as per below...
            //=======================================
            //@AssignmentConstraintID	int = null, 
            //@WMSourceID int,
            //@InstanceNumber int,
            //@TimeWindow varchar(20)= null, 
            //@Duration int= null, 
            //@StartTime datetime= null, 
            //@EndTime datetime= null,
            //@SpecificStartTime datetime= null, 
            //@MustBeCompletedByTime datetime= null
            //@StartTime_IsUserSet bit = null
            //@SkillCodes varchar(8000) = null

            if (this.UpdateInsideTransaction && this.AssignmentConstraintsDataAccess != null)
            {
                objADO = this.AssignmentConstraintsDataAccess;
            }
            else
            {
                objADO = new DataAccess();
            }


            if (this == null)
            {
                return false;
            }

            if (this.ID > 0)
            {
                colParameters.Add(new SqlParameter("@AssignmentConstraintID", this.ID));
            }

            colParameters.Add(new SqlParameter("@WMSourceID", this.WMSourceID));
            colParameters.Add(new SqlParameter("@InstanceNumber", this.Instance));
            colParameters.Add(new SqlParameter("@TimeWindow", this.TimeWindow.ToString()));

            colParameters.Add(new SqlParameter("@Duration", this.Duration));

            if (this.StartTime != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@StartTime", this.StartTime));
            }

            if (this.EndTime != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@EndTime", this.EndTime));
            }

            if (this.SpecificStartTime != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@SpecificStartTime", this.SpecificStartTime));
            }

            if (this.MustBeCompletedByTime != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@MustBeCompletedByTime", this.MustBeCompletedByTime));
            }

            colParameters.Add(new SqlParameter("@StartTime_IsUserSet", this.IsUserSetStartDate));

            //Skills
            if (this.Skills != null && this.Skills.Count > 0)
            {
                string[] skillCodes = new string[this.Skills.Count];
                for (int intIndex = 0; intIndex < this.Skills.Count; intIndex++)
                {
                    skillCodes[intIndex] = this.Skills[intIndex].Code;
                }
                arrayParameter = new DataAccess.ArrayListParameter("SkillCodes", skillCodes);
            }

            returnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;
            colParameters.Add(returnParameter);

            if (arrayParameter != null)
            {
                returnID = objADO.ExecuteSQL(strStoredProcedure, arrayParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            else
            {
                returnID = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }

            if (!this.UpdateInsideTransaction)
            {
                objADO = null;
            }

            return (returnID > 0);
        }

    }

    #endregion

}
