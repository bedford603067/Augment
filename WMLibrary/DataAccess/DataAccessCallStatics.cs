#region Using

using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using FinalBuild;

#endregion

namespace BusinessObjects.WorkManagement
{
    #region JobReferenceCollection class

    public partial class JobReferenceCollection
    {
        public static JobReferenceCollection Populate(DataTable collectionMembers)
        {
            JobReferenceCollection colReferences = new JobReferenceCollection();

            foreach (DataRow drMember in collectionMembers.Rows)
            {
                colReferences.Add(JobReference.Create(drMember));
            }
            return colReferences;
        }

        public static JobReferenceCollection GetJobReferences(string sourceSystem, int[] sourceIDs)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selJobReferencesFromWMSourceIDs";

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem));
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters).Tables[0];

            return Populate(dtResults);
        }

        public static JobReferenceCollection GetJobReferences(string sourceSystem, string[] sourceKeys)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selJobReferencesFromWMSourceKeys";

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceKeys", sourceKeys);
            objParameter.MaximumLengthOfArrayMember = 12;
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem));
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters).Tables[0];

            return Populate(dtResults);
        }

        public static JobReferenceCollection GetJobReferencesFromClickKeys(int[] clickKeys)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selJobReferencesFromClickKeys";

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("JobReferences", clickKeys);
            colParameters.Add(objParameter);

            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, null).Tables[0];

            return Populate(dtResults);
        }

        public static JobReferenceCollection GetJobReferences(eJobStatus jobStatus, string sourceSystem, int siteID)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selJobReferencesByJobStatus";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@JobStatus", jobStatus.ToString()));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem));
            if (siteID > 0)
            {
                colParameters.Add(new SqlParameter("@SiteID", siteID));
            }
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dtResults = objADO.GetDataTable(strStoredProcedure, "JobReferences", arrParameters);

            return Populate(dtResults);
        }

        public static JobReferenceCollection GetJobReferences(Customer customer)
        {
            JobReferenceCollection references = new JobReferenceCollection();
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selJobReferencesByCustomer";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@CustomerID", customer.ID));
            // colParameters.Add(new SqlParameter("@ParentCustomerID", customer.ParentCustomerID));
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dtResults = objADO.GetDataTable(strStoredProcedure, "JobReferences", arrParameters);
            for (int index = 0; index < dtResults.Rows.Count; index++)
            {
                references.Add(new JobReference());
                references[index].SourceID = dtResults.Rows[index]["WMSourceID"].ToString();
                references[index].ID = int.Parse(references[index].SourceID);
                references[index].Region = dtResults.Rows[index]["PostCode"].ToString();
            }

            return references;
        }
    }

    #endregion

    #region WorkerCollection

    public partial class WorkerCollection
    {
        public static WorkerCollection Populate(DataTable collectionMembers)
        {
            WorkerCollection colMembers = new WorkerCollection();
            Worker objWorker = null;

            foreach (DataRow drMember in collectionMembers.Rows)
            {
                objWorker = new Worker();
                objWorker.LoginName = drMember["ADLoginID"].ToString();
                objWorker.UserID = objWorker.LoginName;

                objWorker.EmpNo = drMember["EmployeeNo"].ToString();
                if (drMember["Surname"] != System.DBNull.Value)
                {
                    objWorker.Surname = drMember["Surname"].ToString();
                }
                if (drMember["Forenames"] != System.DBNull.Value)
                {
                    objWorker.Forenames = drMember["Forenames"].ToString();
                }
                if (drMember["PreferredName"] != System.DBNull.Value)
                {
                    objWorker.EmpPrefname = drMember["PreferredName"].ToString();
                }
                else
                {
                    objWorker.EmpPrefname = objWorker.Forenames;
                }
                /*
                if (drMember["EmpAddress"] != System.DBNull.Value)
                {
                    objWorker.EmpAddress = drMember["EmpAddress"].ToString();
                }
                else
                {
                */ 
                    objWorker.EmpAddress = string.Empty;
                /* } */
                if (!drMember["EmailAddress"].Equals(DBNull.Value))
                {
                    objWorker.Email = drMember["EmailAddress"].ToString();
                }
                if (!drMember["MobileNumber"].Equals(DBNull.Value))
                {
                    objWorker.MobileNo = drMember["MobileNumber"].ToString();
                }
                
                // Add to Collection
                colMembers.Add(objWorker);
            }

            return colMembers;
        }

        public static WorkerCollection PopulateTyped(DataTable collectionMembers, string sortPropertyName)
        {
            DataSet workerDataset = new DataSet();
            workerDataset.Tables.Add(collectionMembers.Copy());

            return PopulateTyped(workerDataset, sortPropertyName);
        }

        public static WorkerCollection PopulateTyped(DataSet collectionMembers, string sortPropertyName)
        {
            WorkerCollection colMembers = new WorkerCollection();
            Worker objWorker = null;
            DataRow[] gangMembers = null;
            DataRow[] contractorDetails = null;
            DataRow drContractor = null;
            string filterExpression = string.Empty;

            collectionMembers.Tables[0].TableName = "Common";
            if (collectionMembers.Tables.Count > 1)
            {
                collectionMembers.Tables[1].TableName = "GangMembers";
                collectionMembers.Tables[2].TableName = "ContractorDetails";
            }

            foreach (DataRow drMember in collectionMembers.Tables["Common"].Rows)
            {
                if (drMember["MobileIdentityType"].ToString().ToUpper() == "GANG")
                {
                    objWorker = new Gang();
                }
                else if (drMember["MobileIdentityType"].ToString().ToUpper() == "CONTRACTOR")
                {
                    objWorker = new Contractor();
                }
                else
                {
                    objWorker = new Engineer();   //  Assume Engineer
                }
                objWorker.UserID = drMember["UserID"].ToString();
                objWorker.LoginName = drMember["ADLoginID"].ToString();
                objWorker.EmpNo = drMember["EmpNo"].ToString();
                if (drMember["EmpSurname"] != System.DBNull.Value)
                {
                    objWorker.Surname = drMember["EmpSurname"].ToString();
                }
                if (drMember["EmpForenames"] != System.DBNull.Value)
                {
                    objWorker.Forenames = drMember["EmpForenames"].ToString();
                }
                if (drMember["EmpPrefname"] != System.DBNull.Value)
                {
                    objWorker.EmpPrefname = drMember["EmpPrefname"].ToString();
                }
                else
                {
                    objWorker.EmpPrefname = objWorker.Forenames;
                }
                if (drMember["EmpAddress"] != System.DBNull.Value)
                {
                    objWorker.EmpAddress = drMember["EmpAddress"].ToString();
                }
                else
                {
                    objWorker.EmpAddress = string.Empty;
                }
                if (!drMember["EmailAddress"].Equals(DBNull.Value))
                {
                    objWorker.Email = drMember["EmailAddress"].ToString();
                }
                if (!drMember["EmpMobileTelNo"].Equals(DBNull.Value))
                {
                    objWorker.MobileNo = drMember["EmpMobileTelNo"].ToString();
                }
                objWorker.AreaDetails = new AreaDetails();
                objWorker.AreaDetails.PrimaryArea = new Area();
                objWorker.AreaDetails.PrimaryArea.Name = "UNKNOWN";
                objWorker.AreaDetails.SubArea = new Area();
                objWorker.AreaDetails.SubArea.Name = "UNKNOWN";

                if (!drMember["PrimaryArea"].Equals(DBNull.Value))
                {
                    objWorker.AreaDetails.PrimaryArea.Name = drMember["PrimaryArea"].ToString();
                }

                if (!drMember["SubArea"].Equals(DBNull.Value))
                {
                    objWorker.AreaDetails.SubArea.Name = drMember["SubArea"].ToString();
                }

                if (drMember["IsClickEngineer"] != System.DBNull.Value)
                {
                    objWorker.IsClickEngineer = (bool)drMember["IsClickEngineer"];
                }

                if (collectionMembers.Tables.Count > 1)
                {
                    if (objWorker is Gang)
                    {
                        try
                        {
                            int numericUserID = 0;
                            if (int.TryParse(objWorker.UserID, out numericUserID))
                            {
                                throw new Exception("Gang ID must start with a non-numeric character");
                                // NB: .Select(filterExpression) can't handle it otherwise
                            }
                            filterExpression = string.Format("GangID='{0}'", objWorker.UserID);
                            gangMembers = collectionMembers.Tables["GangMembers"].Select(filterExpression);
                            if (gangMembers != null && gangMembers.Length > 0)
                            {
                                string[] gangUserIDs = new string[gangMembers.Length];
                                for (int index = 0; index < gangMembers.Length; index++)
                                {
                                    gangUserIDs[index] = gangMembers[index]["MemberID"].ToString();
                                }
                                ((Gang)objWorker).Members = WorkerCollection.GetTypedWorkers(string.Empty, gangUserIDs);
                            }
                        }
                        catch (Exception excE)
                        {
                            throw new Exception("Error attemping to load GangMembers : " + excE.Message, excE);
                        }
                    }
                    if (objWorker is Contractor)
                    {
                        // 2 types of contractor so only add extra properties to sub contractor
                        if (objWorker.AreaDetails.SubArea.Name.ToUpper() == "CONTRACTOR")
                        {
                            try
                            {
                                filterExpression = string.Format("UserID='{0}'", objWorker.UserID);
                                contractorDetails = collectionMembers.Tables["ContractorDetails"].Select(filterExpression);
                                if (contractorDetails != null && contractorDetails.Length > 0)
                                {
                                    drContractor = contractorDetails[0];
                                    objWorker.Email = drContractor[1].ToString();//Email
                                    ((Contractor)objWorker).Telephone = drContractor[2].ToString(); //Telephone
                                    ((Contractor)objWorker).Mobile = drContractor[3].ToString();    //Mobile
                                    ((Contractor)objWorker).Password = drContractor[4].ToString();  //Password
                                }
                            }
                            catch (Exception excE)
                            {
                                throw new Exception("Error attemping to load Contractors : " + excE.Message, excE);
                            }
                        }
                    }
                }

                colMembers.Add(objWorker);
            }

            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                string actualPropertyName = sortPropertyName;
                if (actualPropertyName.EndsWith(" DESC"))
                {
                    actualPropertyName = actualPropertyName.Replace(" DESC", "");
                    colMembers.Sort(actualPropertyName, System.ComponentModel.ListSortDirection.Descending);
                }
                else
                {
                    colMembers.Sort(actualPropertyName, System.ComponentModel.ListSortDirection.Ascending);
                }
            }
            return colMembers;
        }

        public static WorkerCollection GetTypedWorkers(string sortPropertyName) //  sortPropertyName is actually a GridView sortexpression i.e. can end with " DESC"
        {
            DataSet dsResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selWorkers";

            // Assemble Parameters
            dsResults = objADO.GetDataSet(strStoredProcedure, arrParameters);
            return PopulateTyped(dsResults, sortPropertyName);
        }

        public static WorkerCollection GetTypedWorkers(string sortPropertyName, object[] parameterValueArray) //  sortPropertyName is actually a GridView sortexpression i.e. can end with " DESC"
        {
            DataSet dsResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selWorkers";

            // Assemble Parameters

            dsResults = objADO.GetDataSet(strStoredProcedure, new DataAccess.ArrayListParameter("UserIDs", parameterValueArray), arrParameters);
            return PopulateTyped(dsResults, sortPropertyName);
        }

        public static WorkerCollection GetTypedWorkers(string subArea, eMobileIdentityType identityType)
        {
            AreaDetails areaDetails = new AreaDetails();
            areaDetails.SubArea = new Area();
            areaDetails.SubArea.Name = subArea;

            return GetTypedWorkers(areaDetails, identityType);
        }

        public static WorkerCollection GetTypedWorkers(BusinessObjects.WorkManagement.AreaDetails areaDetails, eMobileIdentityType identityType)
        {
            DataSet dsResults = null;
            FinalBuild.DataAccess objADO = new DataAccess();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selWorkersMatchingCriteria";

            // Assemble Parameters
            if(areaDetails.SubArea != null && !string.IsNullOrEmpty(areaDetails.SubArea.Name))
            {
                colParameters.Add(new SqlParameter("@SubArea", areaDetails.SubArea.Name));
            }
            colParameters.Add(new SqlParameter("@MobileIdentityType", identityType.ToString()));
            if(areaDetails.PrimaryArea != null && !string.IsNullOrEmpty(areaDetails.PrimaryArea.Name))
            {
                colParameters.Add(new SqlParameter("@PrimaryArea", areaDetails.PrimaryArea.Name));
            }

            dsResults = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            return WorkerCollection.PopulateTyped(dsResults, string.Empty);
        }

        public static WorkerCollection GetEngineers(string sortPropertyName)
        {
            WorkerCollection colMembers = new WorkerCollection();
            WorkerCollection allMembers = GetTypedWorkers(sortPropertyName);
            foreach (Worker worker in allMembers)
            {
                if (worker is Engineer)
                {
                    colMembers.Add(worker);
                }
            }
            return colMembers;
        }

        public static WorkerCollection GetContractors(string sortPropertyName)
        {
            WorkerCollection colMembers = new WorkerCollection();
            WorkerCollection allMembers = GetTypedWorkers(sortPropertyName);
            foreach (Worker worker in allMembers)
            {
                if (worker is Contractor)
                {
                    colMembers.Add(worker);
                }
            }
            return colMembers;
        }

        public static WorkerCollection GetGangs(string sortPropertyName)
        {
            WorkerCollection colMembers = new WorkerCollection();
            WorkerCollection allMembers = GetTypedWorkers(sortPropertyName);
            foreach (Worker worker in allMembers)
            {
                if (worker is Gang)
                {
                    colMembers.Add(worker);
                }
            }
            return colMembers;
        }

        public static WorkerCollection GetWorkersByNTLoginArray(object[] parameterValueArray)
        {
            //  Cater for being called with null parameterValueArray - dont want to return null as callers use the returned object so return an empty collection instead
            if (parameterValueArray != null)
            {
                DataTable dtResults = null;
                FinalBuild.DataAccess objADO = Domain.GetADOInstance();
                System.Data.SqlClient.SqlParameter[] arrParameters = null;
                string strStoredProcedure = "selEmployeesByNTLoginIDs";

                // Assemble Parameters

                dtResults = objADO.GetDataSet(strStoredProcedure, new DataAccess.ArrayListParameter("NTLoginIDs", parameterValueArray), arrParameters).Tables[0];
                return Populate(dtResults);
            }
            else
            {
                return new WorkerCollection();
            }
        }

        public static WorkerCollection GetWorkersByEmployeeNoArray(object[] parameterValueArray)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            string strStoredProcedure = "selEmployeesByEmpNos";

            // Assemble Parameters

            dtResults = objADO.GetDataSet(strStoredProcedure, new DataAccess.ArrayListParameter("EmpNos", parameterValueArray), arrParameters).Tables[0];
            return Populate(dtResults);
        }

        public static WorkerCollection GetSiteContacts()
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            string strStoredProcedure = "selHansenSiteContacts";
            dtResults = objADO.GetDataTable(strStoredProcedure, "SiteContacts");
            Worker objWorker;
            WorkerCollection colMembers = new WorkerCollection();

            foreach (DataRow drMember in dtResults.Rows)
            {
                objWorker = new Worker();
                objWorker.LoginName = drMember["NTLoginID"].ToString();
                if (drMember["EmpSurname"] != System.DBNull.Value)
                {
                    objWorker.Surname = drMember["EmpSurname"].ToString();
                }
                if (drMember["EmpForenames"] != System.DBNull.Value)
                {
                    objWorker.Forenames = drMember["EmpForenames"].ToString();
                }
                if (drMember["EmpNo"] != System.DBNull.Value)
                {
                    objWorker.EmpNo = drMember["EmpNo"].ToString();
                }

                colMembers.Add(objWorker);

            }

            return colMembers;
        }
    }

    #endregion

    #region BankHolidayCollection

    public partial class BankHolidayCollection
    {
        public static BankHolidayCollection GetBankHolidays()
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selBankHolidays";

            // Assemble Parameters

            dtResults = objADO.GetDataTable(strStoredProcedure, "BankHolidays", arrParameters);
            return Populate(dtResults);
        }

        public static BankHolidayCollection Populate(DataTable collectionMembers)
        {
            BankHolidayCollection colMembers = new BankHolidayCollection();
            BankHoliday obj = null;

            foreach (DataRow drMember in collectionMembers.Rows)
            {
                obj = new BankHoliday();
                obj.Date = (DateTime)drMember["BankHolidayDate"];
                colMembers.Add(obj);
            }

            return colMembers;
        }
    }

    #endregion

    #region Job class

    public partial class Job
    {
        public static Job GetSerializedObject(int jobID, eWMSourceSystem sourceSystem, Type inheritedType)
        {
            Job deserializedJob = null;

            try
            {
                List<Job> matchingJobs = GetJobsByReference(new string[] { jobID.ToString() }, sourceSystem, inheritedType);
                if (matchingJobs.Count > 0)
                {
                    deserializedJob = (Job)matchingJobs[0];
                }
            }
            catch (Exception excE)
            {
                JobLoadException jobLoadException = new BusinessObjects.WorkManagement.JobLoadException(excE);
                jobLoadException.JobID = jobID;
                jobLoadException.Source = sourceSystem.ToString();

                throw jobLoadException;
            }

            return deserializedJob;
        }

        public static Job GetSerializedObject(int jobID, eWMSourceSystem sourceSystem, bool includeArchived)
        {
            Job deserializedJob = null;

            try
            {
                List<Job> matchingJobs = GetJobsByReference(new string[] { jobID.ToString() }, sourceSystem, includeArchived);
                if (matchingJobs.Count > 0)
                {
                    deserializedJob = (Job)matchingJobs[0];
                }
            }
            catch (Exception excE)
            {
                JobLoadException jobLoadException = new BusinessObjects.WorkManagement.JobLoadException(excE);
                jobLoadException.JobID = jobID;
                jobLoadException.Source = sourceSystem.ToString();

                throw jobLoadException;
            }

            return deserializedJob;
        }

        public static List<Job> GetJobsByReference(string[] sourceIDs, eWMSourceSystem sourceSystem, Type inheritedType)
        {
            return GetJobsByReference(sourceIDs, sourceSystem, inheritedType, false);
        }

        public static List<Job> GetJobsByReference(string[] sourceIDs, eWMSourceSystem sourceSystem, Type inheritedType, bool includeArchived)
        {
            List<Job> colJobs = new List<Job>();
            Job deserializedJob = null;
            DataTable dtResults = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selJobsByReference";

            objADO = Domain.GetADOInstance();

            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);
            //If Source System is UnSpecified then it is not relevant .. so any will be returned 
            if (sourceSystem != eWMSourceSystem.Unspecified)
            {
                colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            }
            if (includeArchived)
            {
                colParameters.Add(new SqlParameter("@IncludeArchived", includeArchived));
            }
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters).Tables[0];
            objADO = null;

            foreach (DataRow drSerializedJob in dtResults.Rows)
            {
                Type inheritedTypeToUse = null;

                if (inheritedType == null)
                {
                    inheritedTypeToUse = Type.GetType(drSerializedJob["TypeName"].ToString());
                }
                else
                {
                    inheritedTypeToUse = inheritedType;
                }

                if (!drSerializedJob["Serialized"].Equals(DBNull.Value))
                {
                    System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                    objDOM.LoadXml(drSerializedJob["Serialized"].ToString());
                    deserializedJob = (Job)BusinessObjects.Base.Deserialize(inheritedTypeToUse, objDOM);
                    if (!drSerializedJob["DateReopened"].Equals(DBNull.Value))
                    {
                        deserializedJob.DateReopened = (DateTime)drSerializedJob["DateReopened"];
                    }

                    // NB: Set properties that are Instance specific to empty\default to avoid confusion
                    deserializedJob.Appointment = null;

                    colJobs.Add(deserializedJob);
                }
            }

            return colJobs;
        }

        public static List<Job> GetJobsByReference(string[] sourceIDs, eWMSourceSystem sourceSystem, bool includeArchived)
        {
            return GetJobsByReference(sourceIDs, sourceSystem, null, includeArchived);
        }

        public static List<WorkflowAuditRecord> GetWorkflowAuditHistory(int jobID, eWMSourceSystem sourceSystem)
        {
            List<WorkflowAuditRecord> auditHistory = new List<WorkflowAuditRecord>();

            DataTable dtData = null;
            DataAccess objADO = Domain.GetADOInstance();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobStatusAudit";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            dtData = objADO.GetDataTable(strStoredProcedure, "WorkflowAuditHistory", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData.Rows.Count > 0)
            {
                WorkflowAuditRecord newAuditRecord = null;
                foreach (DataRow drAudit in dtData.Rows)
                {
                    newAuditRecord = new WorkflowAuditRecord();
                    newAuditRecord.ChangeDate = (DateTime)drAudit["ChangeDate"];
                    newAuditRecord.Status = (eJobStatus)Enum.Parse(typeof(eJobStatus), drAudit["JobStatus"].ToString());
                    if (!drAudit["UserID"].Equals(DBNull.Value))
                    {
                        newAuditRecord.ChangeUser = drAudit["UserID"].ToString();
                    }
                    if (!drAudit["ServiceName"].Equals(DBNull.Value))
                    {
                        newAuditRecord.ServiceName = drAudit["ServiceName"].ToString();
                    }

                    auditHistory.Add(newAuditRecord);
                }
            }

            return auditHistory;
        }

        public static DataTable GetWorkInProgressInfo(int jobID, eWMSourceSystem sourceSystem)
        {
            DataTable dtData = null;
            DataAccess objADO = Domain.GetADOInstance();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selWorkInProgress";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            dtData = objADO.GetDataTable(strStoredProcedure, "WorkInProgress", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return dtData;
        }

        public static string[] GetAssigneeInformation(int jobID, eWMSourceSystem sourceSystem)
        {
            return GetAssigneeInformation(jobID, sourceSystem, -1, false);
        }

        public static string[] GetAssigneeInformation(int jobID, eWMSourceSystem sourceSystem, int instanceNumber)
        {
            return GetAssigneeInformation(jobID, sourceSystem, instanceNumber, false);
        }

        public static string[] GetAssigneeInformation(int jobID, eWMSourceSystem sourceSystem, int instanceNumber, bool includeArchived)
        {
            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobAssignees";

            string[] clickEngineerIDs = null;

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            if (instanceNumber != -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", instanceNumber));
            }
            colParameters.Add(new SqlParameter("@IncludeArchived", includeArchived));
            dtData = objADO.GetDataTable(strStoredProcedure, "AssigneeInformation", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData != null && dtData.Rows.Count > 0)
            {
                clickEngineerIDs = new string[dtData.Rows.Count];
                string assigneeID = string.Empty;
                for (int intIndex = 0; intIndex < dtData.Rows.Count; intIndex++)
                {
                    assigneeID = dtData.Rows[intIndex]["UserID"].ToString();
                    if (!dtData.Rows[intIndex]["EmpNo"].Equals(DBNull.Value) && dtData.Rows[intIndex]["EmpNo"] != string.Empty)
                    {
                        assigneeID += ":" + dtData.Rows[intIndex]["EmpNo"].ToString();
                    }
                    clickEngineerIDs[intIndex] = assigneeID;
                }
            }

            return clickEngineerIDs;
        }

        public static List<Job> GetJobsByParentID(int jobID, eWMSourceSystem sourceSystem)
        {
            List<Job> colJobs = new List<Job>();
            DataAccess objADO = Domain.GetADOInstance();
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobsByParentJobID";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            dtResults = objADO.GetDataTable(strStoredProcedure, "ChildJobs", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtResults.Rows.Count > 0)
            {
                Type inheritedType = null;
                System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();

                foreach (DataRow drSerializedJob in dtResults.Rows)
                {
                    if (!drSerializedJob["Serialized"].Equals(DBNull.Value))
                    {
                        objDOM.LoadXml(drSerializedJob["Serialized"].ToString());
                        inheritedType = Type.GetType(drSerializedJob["TypeName"].ToString());
                        colJobs.Add((Job)BusinessObjects.Base.Deserialize(inheritedType, objDOM));
                    }
                }
            }

            return colJobs;
        }

        public static List<Job> GetJobsByAssetID(int assetID)
        {
            List<Job> colJobs = new List<Job>();
            DataAccess objADO = Domain.GetADOInstance();
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobsByAssetID";

            colParameters.Add(new SqlParameter("@assetID", assetID));
            dtResults = objADO.GetDataTable(strStoredProcedure, "AssociatedJobs", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtResults.Rows.Count > 0)
            {
                Type inheritedType = null;
                System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();

                foreach (DataRow drSerializedJob in dtResults.Rows)
                {
                    if (!drSerializedJob["Serialized"].Equals(DBNull.Value))
                    {
                        objDOM.LoadXml(drSerializedJob["Serialized"].ToString());
                        inheritedType = Type.GetType(drSerializedJob["TypeName"].ToString());
                        colJobs.Add((Job)BusinessObjects.Base.Deserialize(inheritedType, objDOM));
                    }
                }
            }

            return colJobs;
        }
        /// <summary>
        /// Find Jobs currently On Hold matching criteria paassed in
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static System.Data.DataSet GetJobsOnHoldByCriteria(SearchParameterCollection parameters)
        {

            DataSet dsResults = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobsOnHoldBySearchParameters";

            objADO = Domain.GetADOInstance();

            if (parameters != null)
            {
                foreach (SearchParameter parameter in parameters)
                {
                    SqlParameter sqlParameter = new SqlParameter();
                    sqlParameter.ParameterName = "@" + parameter.Name;
                    if (parameter.Value is string[])
                    {
                        string arrayValues = string.Empty;
                        foreach (string item in (parameter.Value as string[]))
                        {
                            if (arrayValues.Equals(string.Empty))
                            {
                                arrayValues = item.Trim();
                            }
                            else
                            {
                                arrayValues += "," + item.Trim();
                            }
                        }
                        sqlParameter.Value = arrayValues;
                    }
                    else
                    {
                        sqlParameter.Value = parameter.Value;
                    }
                    colParameters.Add(sqlParameter);
                }
            }

            dsResults = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)), 600);    //  Slow query so set long commandTimeout
            objADO = null;

            dsResults.Tables[0].TableName = "JobsOnHold";



            return dsResults;
        }

        /// <summary>
        /// Find Jobs curent On Hold for a given Reason where Action is required [e.g Approval Required]
        /// </summary>
        /// <param name="onHoldReasonDesc"></param>
        /// <param name="cutoffDateForAction"></param>
        /// <returns></returns>
        public static List<Job> GetJobsOnHoldAwaitingAction(string onHoldReasonDesc, DateTime cutoffDateForAction)
        {
            List<Job> colJobs = new List<Job>();
            Job deserializedJob = null;
            System.Xml.XmlDocument objDOM = null;
            Type inheritedTypeToUse = null;

            DataTable dtResults = null;
            DataAccess objADO = Domain.GetADOInstance();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobsOnHoldAwaitingAction";

            colParameters.Add(new SqlParameter("@OnHoldReasonDesc", onHoldReasonDesc));
            if (cutoffDateForAction != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@CutoffDate", cutoffDateForAction));
            }

            dtResults = objADO.GetDataTable(strStoredProcedure, "Jobs", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            foreach (DataRow drJob in dtResults.Rows)
            {
                inheritedTypeToUse = Type.GetType(drJob["TypeName"].ToString());
                if (inheritedTypeToUse == null)
                {
                    throw new Exception(string.Format("Type {0} cannot be loaded", drJob["TypeName"].ToString()));
                }

                if (!drJob["Serialized"].Equals(DBNull.Value))
                {
                    objDOM = new System.Xml.XmlDocument();
                    objDOM.LoadXml(drJob["Serialized"].ToString());
                    deserializedJob = (Job)BusinessObjects.Base.Deserialize(inheritedTypeToUse, objDOM);

                    if (!drJob["DateCreated"].Equals(DBNull.Value))
                    {
                        deserializedJob.DateCreated = (DateTime)drJob["DateCreated"];
                    }

                    colJobs.Add(deserializedJob);
                }
            }

            return colJobs;
        }

        /// <summary>
        /// Returns the last start time that was sent from the mobile for a Job.  This value is found in tblJobHistory.
        /// </summary>
        /// <param name="jobID">The ID of the Job in question.</param>
        /// <returns>The latest start date.</returns>
        public static DateTime GetLatestStartTime(int jobID)
        {
            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobHistory";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            dtData = objADO.GetDataTable(strStoredProcedure, "JobHistory", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            DateTime latestStartTime = DateTime.MinValue;
            if (dtData != null && dtData.Rows.Count > 0)
            {
                DateTime latestRecord = DateTime.MinValue;
                foreach (DataRow row in dtData.Rows)
                {
                    if ((DateTime)row["LastUpdatedDate"] > latestRecord)
                    {
                        latestRecord = (DateTime)row["LastUpdatedDate"];
                        latestStartTime = (DateTime)row["StartDateTime"];
                    }
                }
            }

            return latestStartTime;
        }

        /// <summary>
        /// Archives the Job in TaskStore.tblJobArchive
        /// </summary>
        /// <returns></returns>
        public static bool Remove(int jobID, eWMSourceSystem sourceSystem)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insJobArchive";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public static bool UpdateJobAttributes(int jobID, int dispatchPriority)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobSerializedAttributes";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@DispatchPriorityRanking", dispatchPriority));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        /// <summary>
        /// Updates relational as well Xml content on the Job for the selected Attribute 
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="schedulingSystem"></param>
        /// <returns></returns>
        public static bool UpdateJobAttributes(int jobID, eSchedulingSystem schedulingSystem)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobSerializedAttributes";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@SchedulingSystem", schedulingSystem.ToString()));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        /// <summary>
        /// Assign\Reassign Job within the Integration layer (TaskStore database)
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="userID"></param>
        /// <param name="removeCurrentAssignees"></param>
        /// <param name="jobInstanceNumber"></param>
        /// <returns></returns>
        public static bool Assign(int jobID, string userID, bool removeCurrentAssignees, int jobInstanceNumber)
        {
            return Assign(jobID, userID, removeCurrentAssignees, jobInstanceNumber, DateTime.MinValue);
        }

        /// <summary>
        /// Assign\Reassign Job within the Integration layer (TaskStore database)
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="userID"></param>
        /// <param name="removeCurrentAssignees"></param>
        /// <param name="jobInstanceNumber"></param>
        /// <param name="assignmentStartDate"></param>
        /// <returns></returns>
        public static bool Assign(int jobID, string userID, bool removeCurrentAssignees, int jobInstanceNumber, DateTime assignmentStartDate)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobAssignees";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@RemoveCurrentAssignees", removeCurrentAssignees));
            if (jobInstanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            }
            if (assignmentStartDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@StartDate", assignmentStartDate));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        /// <summary>
        /// Reopen Job post Complete, passing userID that the reopened Job is to be Assigned to
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="userID"></param>
        /// <param name="sourceSystem"></param>
        /// <returns></returns>
        public static string Reopen(int jobID, string userID, bool makeAvailableForDownload)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter sourceSystemParam = null;
            string strStoredProcedure = "updJobReopenPostComplete";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));

            sourceSystemParam = new SqlParameter("@WMSourceSystem", SqlDbType.VarChar, 20);
            sourceSystemParam.Direction = ParameterDirection.InputOutput;
            sourceSystemParam.Value = DBNull.Value;
            colParameters.Add(sourceSystemParam);

            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@MakeAvailableForDownload", makeAvailableForDownload));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (intReturn > 0)
            {
                // Return SourceSystem value.
                return ((SqlParameter)colParameters[1]).Value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns information about an archived Job not present in the Serialized content
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="lastAssignee"></param>
        /// <param name="lastUpdated"></param>
        /// <returns></returns>
        public static bool GetArchivedJobInformation(int jobID, out string lastAssignee, out DateTime lastUpdated)
        {
            DataTable dtResults = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selArchivedJobInformation";

            lastAssignee = string.Empty;
            lastUpdated = DateTime.MinValue;

            objADO = Domain.GetADOInstance();
            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            dtResults = objADO.GetDataTable(strStoredProcedure, "ArchivedInfo", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                DataRow drArchivedInfo = dtResults.Rows[0];
                lastAssignee = drArchivedInfo["UserID"].ToString();
                lastUpdated = (DateTime)drArchivedInfo["LastUpdatedDate"];
            }

            // Return value indicates if Job is present in the Archive tables
            return (dtResults != null && dtResults.Rows.Count > 0);
        }

        public static Appointment GetAppointment(int jobID, int jobInstanceNumber)
        {
            Appointment deserializedAppointment = null;
            System.Xml.XmlDocument objDOM = null;

            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selAppointmentsMatchingCriteria";

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", new int[]{jobID});
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));

            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter))).Tables[0];
            if (dtResults.Rows.Count > 0)
            {
                foreach (DataRow drSerializedAppointment in dtResults.Rows)
                {
                    if (!drSerializedAppointment["Appointment"].Equals(DBNull.Value))
                    {
                        objDOM = new System.Xml.XmlDocument();
                        objDOM.LoadXml(drSerializedAppointment["Appointment"].ToString());
                        deserializedAppointment = (Appointment)BusinessObjects.Base.Deserialize(typeof(Appointment), objDOM);
                    }

                    break; // This routine only interested in first (current) Appointment value
                }
            }

            return deserializedAppointment;
        }

        public static Appointment GetAppointmentForArchivedJob(int jobID, int jobInstanceNumber)
        {
            Appointment deserializedAppointment = null;
            System.Xml.XmlDocument objDOM = null;

            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selAppointmentForArchivedJob";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Appointment", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            if (dtResults.Rows.Count > 0)
            {
                if (!dtResults.Rows[0]["Appointment"].Equals(DBNull.Value))
                {
                    objDOM = new System.Xml.XmlDocument();
                    objDOM.LoadXml(dtResults.Rows[0]["Appointment"].ToString());
                    deserializedAppointment = (Appointment)BusinessObjects.Base.Deserialize(typeof(Appointment), objDOM);
                }
            }

            return deserializedAppointment;
        }

        public static bool RemoveAppointment(int jobID, int jobInstanceNumber, string changeUser, bool informEngineer,
                                                BusinessObjects.WorkManagement.eSchedulerAppointmentMissedReason missedReason   )
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobAppointment";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            colParameters.Add(new SqlParameter("@ChangeUser", changeUser));
            if (informEngineer)
            {
                colParameters.Add(new SqlParameter("@DateChangeDetected", DateTime.Now));
            }
            colParameters.Add(new SqlParameter("@IsActive", false));
            colParameters.Add(new SqlParameter("@MissedReason", missedReason.ToString()));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public static bool HasBeenReopened(int jobID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter returnParameter = null;
            SqlParameter outputParameterDateReopened;
            string strStoredProcedure = "selJobHasBeenReopened";
            
            bool hasBeenReopened = false;
            DateTime dateReopened;

            returnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
            returnParameter.Direction = ParameterDirection.ReturnValue;

            outputParameterDateReopened = new SqlParameter("@DateReopened", SqlDbType.DateTime);
            outputParameterDateReopened.Direction = ParameterDirection.Output;

            colParameters.Add(returnParameter);
            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(outputParameterDateReopened);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!returnParameter.Value.Equals(DBNull.Value))
            {
                hasBeenReopened = ((int)returnParameter.Value) > 0;
                if (hasBeenReopened && 
                    !outputParameterDateReopened.Value.Equals(DBNull.Value))
                {
                    dateReopened = (DateTime)outputParameterDateReopened.Value;
                }
            }

            return hasBeenReopened;
        }

        public static eJobStatus GetLatestStatus(int jobID)
        {
            return GetLatestStatus(jobID, -1); // 0 of course is a valid Instance
        }

        public static eJobStatus GetLatestStatus(int jobID, int instanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter outputParameterStatus;
            string strStoredProcedure = "selJobLatestStatus";

            eJobStatus status = eJobStatus.Planned;

            outputParameterStatus = new SqlParameter("@Status", SqlDbType.VarChar, 30);
            outputParameterStatus.Direction = ParameterDirection.Output;

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            if (instanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", instanceNumber));
            }
            colParameters.Add(outputParameterStatus);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!outputParameterStatus.Value.Equals(DBNull.Value))
            {
                status = (eJobStatus)Enum.Parse(typeof(eJobStatus), outputParameterStatus.Value.ToString());
            }

            return status;
        }

        public static Job GetJobFromArchive(int jobID)
        {
            Job deserializedJob = null;
            DataTable dtResults = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selArchivedJob";

            objADO = Domain.GetADOInstance();

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Job", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if(dtResults.Rows.Count > 0)
            {
                DataRow drSerializedJob = dtResults.Rows[0];
                Type inheritedTypeToUse = null;

                inheritedTypeToUse = Type.GetType(drSerializedJob["TypeName"].ToString());
                if (!drSerializedJob["Serialized"].Equals(DBNull.Value))
                {
                    System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                    objDOM.LoadXml(drSerializedJob["Serialized"].ToString());
                    deserializedJob = (Job)BusinessObjects.Base.Deserialize(inheritedTypeToUse, objDOM);
                    if (!drSerializedJob["DateReopened"].Equals(DBNull.Value))
                    {
                        deserializedJob.DateReopened = (DateTime)drSerializedJob["DateReopened"];
                    }

                    // NB: Set properties that are Instance specific to empty\default to avoid confusion
                    deserializedJob.Appointment = null;
                }
            }

            return deserializedJob;
        }

        public static System.Collections.Generic.SortedList<int, eJobStatus> GetStatusesOfJobs(int[] jobIDs)
        {
            System.Collections.Generic.SortedList<int, eJobStatus> jobStatusesList = new System.Collections.Generic.SortedList<int, eJobStatus>();
            DataTable dtResults = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selStatusesForWMSourceIDs";

            objADO = Domain.GetADOInstance();

            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", jobIDs);
            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters).Tables[0];
            objADO = null;

            if (dtResults.Rows.Count > 0)
            {
                foreach (DataRow drJobStatusInfo in dtResults.Rows)
                {
                    jobStatusesList.Add(int.Parse(drJobStatusInfo["WMSourceID"].ToString()), (eJobStatus)Enum.Parse(typeof(eJobStatus),drJobStatusInfo["JobStatus"].ToString()));
                }
            }

            return jobStatusesList;
        }

        public static void LoadAssignmentConstraints(int WMSourceID, int instanceNumber)
        {
            AssignmentConstraints.FindAssignmentConstraints(WMSourceID, instanceNumber);
        }
        

        public static bool SaveRequiredEngineer(int jobID, int instanceNumber, string userID)
        {
            return SaveRequiredEngineer(jobID, instanceNumber, userID, DateTime.MinValue);
        }

        public static bool SaveRequiredEngineer(int jobID, int instanceNumber, string userID, DateTime statusChangeDate)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "insRequiredEngineer";
            int rowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@JobInstanceNumber", instanceNumber));
            colParameters.Add(new SqlParameter("@UserID", userID));
            if (statusChangeDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@StatusChangeDate", statusChangeDate));
            }

            rowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (rowsAffected > 0);
        }

        public static BusinessObjects.WorkManagement.Worker GetRequiredEngineer(int jobID, int instanceNumber)
        {
            return GetRequiredEngineer(jobID, instanceNumber, DateTime.MinValue);
        }

        public static BusinessObjects.WorkManagement.Worker GetRequiredEngineer(int jobID, int instanceNumber, DateTime statusChangeDate)
        {
            BusinessObjects.WorkManagement.Worker requiredEngineer = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter outputParameter;
            string strStoredProcedure = "selRequiredEngineer";

            outputParameter = new SqlParameter("@UserID", SqlDbType.VarChar, 10);
            outputParameter.Direction = ParameterDirection.Output;

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@JobInstanceNumber", instanceNumber));
            if (statusChangeDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@StatusChangeDate", statusChangeDate));
            }
            colParameters.Add(outputParameter);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!outputParameter.Value.Equals(DBNull.Value))
            {
                WorkManagement.WorkerCollection workers = WorkManagement.WorkerCollection.GetWorkersByNTLoginArray(new string[] { outputParameter.Value.ToString() });
                if (workers.Count > 0)
                {
                    requiredEngineer = workers[0];
                }
            }

            return requiredEngineer;
        }

    }

    #endregion

    #region JobReference class

    public partial class JobReference
    {
        #region Public Methods

        public static JobReference Create(DataRow jobRecord)
        {
            return Create(jobRecord, false);
        }

        public static JobReference Create(DataRow jobRecord, bool idAndSourceOnly)
        {
            JobReference objJobReference = new JobReference();

            objJobReference.SourceID = jobRecord["WMSourceID"].ToString();
            objJobReference.SourceSystem = (eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), jobRecord["WMSourceSystem"].ToString());

            if (!idAndSourceOnly)
            {
                objJobReference.ID = (int)jobRecord["JobID"];
                objJobReference.TypeName = jobRecord["TypeName"].ToString();

                if (!jobRecord["ClickKey"].Equals(System.DBNull.Value))
                {
                    objJobReference.ClickKey = (int)jobRecord["ClickKey"];
                }
                if (!jobRecord["WMSourceKey"].Equals(System.DBNull.Value))
                {
                    objJobReference.SourceKey = jobRecord["WMSourceKey"].ToString();
                }
                if (!jobRecord["Region"].Equals(System.DBNull.Value))
                {
                    objJobReference.Region = jobRecord["Region"].ToString();
                }
            }

            return objJobReference;
        }

        /// <summary>
        /// Updates TaskStore that Job has been received by Click or in fact any other SchedulingSystem
        /// </summary>
        /// <param name="wmSourceID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="dateSubmitted"></param>
        /// <param name="clickKey"></param>
        /// <param name="jobInstanceNumber"></param>
        /// <returns></returns>
        public static int UpdatePostSubmission(int wmSourceID, eWMSourceSystem sourceSystem, DateTime dateSubmitted, int clickKey, int jobInstanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updJobPostClickSubmission";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@ClickDateSubmitted", dateSubmitted));
            if (clickKey > 0)
            {
                colParameters.Add(new SqlParameter("@ClickKey", clickKey));
            }
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return intRowsAffected;
        }

        public static int UpdatePostScheduling(int wmSourceID, eWMSourceSystem sourceSystem, DateTime dateScheduled, string[] assignedEngineers, DateTime assignmentStartDate,
                                                        int jobInstanceNumber, bool updateDateChangeDetected)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "updJobPostClickScheduling";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            colParameters.Add(new SqlParameter("@ClickDateScheduled", dateScheduled));
            if (assignedEngineers != null && assignedEngineers.Length > 0)
            {
                objParameter = new DataAccess.ArrayListParameter("JobAssignees", assignedEngineers);
            }
            if (assignmentStartDate != null && assignmentStartDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@TargetStartDate", assignmentStartDate));
            }
            colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            if (updateDateChangeDetected)
            {
                colParameters.Add(new SqlParameter("@UpdateDateChangeDetected", true));
            }

            //Updated : AP :  ExecuteSQL method used below is dependent on having Engineers or not
            if (objParameter != null)
            {
                intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, objParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            else
            {
                intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }

            objADO = null;

            return intRowsAffected;
        }

        public static bool Remove(int wmSourceID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delJobFromClick";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@JobID", wmSourceID));
            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }

        public static DataTable GetCurrentSchedulingInfo(int wmSourceID, eWMSourceSystem sourceSystem, out DateTime currentStartDate)
        {
            DataTable dtInfo = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objOutputParameter = null;
            string strStoredProcedure = "selJobSchedulingInfo";

            currentStartDate = DateTime.MinValue;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            objOutputParameter = new SqlParameter("@TargetStartDate", SqlDbType.DateTime);
            objOutputParameter.Direction = ParameterDirection.Output;
            colParameters.Add(objOutputParameter);

            dtInfo = objADO.GetDataTable(strStoredProcedure, "SchedulingInfo", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            // Set currentStartDate out parameter if available
            if (!objOutputParameter.Value.Equals(DBNull.Value))
            {
                currentStartDate = (DateTime)objOutputParameter.Value;
            }

            return dtInfo;
        }

        public static int GetJobInstanceNumber(int wmSourceID, eWMSourceSystem sourceSystem, string assignedEngineer)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strStoredProcedure = "selJobTaskNumberInClick";
            int jobInstanceNumber = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            if (assignedEngineer == null)
            {
                colParameters.Add(new SqlParameter("@UserID", System.DBNull.Value));
            }
            else
            {
                colParameters.Add(new SqlParameter("@UserID", assignedEngineer));
            }
            objParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
            objParameter.Direction = ParameterDirection.ReturnValue;
            colParameters.Add(objParameter);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;
            if (!objParameter.Value.Equals(DBNull.Value))
            {
                jobInstanceNumber = (int)objParameter.Value;
            }

            return jobInstanceNumber;
        }

        public static int GetNextJobInstanceNumber(int wmSourceID, eWMSourceSystem sourceSystem, out string clickCallID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter outputParameterCallID = null;
            SqlParameter outputParameterInstanceNo = null;
            string strStoredProcedure = "selJobNewInstanceDetails";
            int jobInstanceNumber = 0;

            clickCallID = null;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            outputParameterCallID = new SqlParameter("@ClickCallID", SqlDbType.VarChar, 50);
            outputParameterCallID.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameterCallID);
            outputParameterInstanceNo = new SqlParameter("@JobInstanceNumber", SqlDbType.Int);
            outputParameterInstanceNo.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameterInstanceNo);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!outputParameterCallID.Value.Equals(DBNull.Value))
            {
                clickCallID = outputParameterCallID.Value.ToString();
            }
            if (!outputParameterInstanceNo.Value.Equals(DBNull.Value))
            {
                jobInstanceNumber = (int)outputParameterInstanceNo.Value;
            }

            return jobInstanceNumber;
        }

        public static string GetClickCallIDAndNumber(int wmSourceID, eWMSourceSystem sourceSystem, out int instanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selClickCallIDForJob";
            string clickCallID = null;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            SqlParameter outputParameterCallID = new SqlParameter("@ClickCallID", SqlDbType.VarChar, 50);
            outputParameterCallID.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameterCallID);
            SqlParameter outputParameterInstance = new SqlParameter("@JobInstance", SqlDbType.Int);
            outputParameterInstance.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameterInstance);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!outputParameterCallID.Value.Equals(DBNull.Value))
            {
                clickCallID = outputParameterCallID.Value.ToString();
            }

            instanceNumber = -1;
            if (!outputParameterInstance.Value.Equals(DBNull.Value))
            {
                instanceNumber = Convert.ToInt32(outputParameterInstance.Value);
            }

            return clickCallID;
        }

        public static string GetClickCallIDForJob(int wmSourceID, eWMSourceSystem sourceSystem)
        {
            int instanceNumber = -1;

            return GetClickCallIDAndNumber(wmSourceID, sourceSystem, out instanceNumber);
        }

        public static BusinessObjects.WorkManagement.eWMSourceSystem GetSourceSystemForJob(int wmSourceID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter outputParameter = null;
            string strStoredProcedure = "selSourceSystemForJob";
            BusinessObjects.WorkManagement.eWMSourceSystem outputValue = BusinessObjects.WorkManagement.eWMSourceSystem.Unspecified;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            outputParameter = new SqlParameter("@WMSourceSystem", SqlDbType.VarChar, 20);
            outputParameter.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameter);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!outputParameter.Value.Equals(DBNull.Value))
            {
                outputValue = (BusinessObjects.WorkManagement.eWMSourceSystem)Enum.Parse(typeof(BusinessObjects.WorkManagement.eWMSourceSystem), outputParameter.Value.ToString(), true);
            }

            return outputValue;
        }

        public static bool RemoveAssignees(int wmSourceID, eWMSourceSystem sourceSystem)
        {
            return RemoveAssignees(wmSourceID, sourceSystem, null);
        }

        public static bool RemoveAssignees(int wmSourceID, eWMSourceSystem sourceSystem, string[] userList)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delJobAssignees";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            if (userList != null)
            {
                DataAccess.ArrayListParameter objParameter = new DataAccess.ArrayListParameter("UserList", userList);
                intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, objParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            else
            {
                intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            objADO = null;

            return (intRowsAffected > 0);
        }

        public static bool RemoveAssignees(int wmSourceID, eWMSourceSystem sourceSystem, int jobInstanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delJobAssignees";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            if (jobInstanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@JobInstanceNumber", jobInstanceNumber));
            }
            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }

        public static List<BusinessObjects.WorkManagement.TaskUpdate> GetTaskUpdateHistory(int wmSourceID, eWMSourceSystem sourceSystem)
        {
            return GetTaskUpdateHistory(wmSourceID, sourceSystem, DateTime.MinValue);
        }

        public static List<BusinessObjects.WorkManagement.TaskUpdate> GetTaskUpdateHistory(int wmSourceID, eWMSourceSystem sourceSystem, DateTime specificChangeDate)
        {
            List<BusinessObjects.WorkManagement.TaskUpdate> taskUpdates = null;
            System.Collections.Generic.List<HistoricalTaskUpdate> serializedTaskUpdates = BusinessObjects.WorkManagement.JobReference.GetStoredTaskUpdates(wmSourceID, sourceSystem, specificChangeDate);
            string assemblyQualifiedName = string.Empty;

            if (serializedTaskUpdates != null && serializedTaskUpdates.Count > 0)
            {
                taskUpdates = new List<BusinessObjects.WorkManagement.TaskUpdate>();
                foreach (BusinessObjects.WorkManagement.HistoricalTaskUpdate historicalTaskUpdate in serializedTaskUpdates)
                {
                    assemblyQualifiedName = historicalTaskUpdate.TypeName;
                    if (assemblyQualifiedName.IndexOf(",") > -1)
                    {
                        assemblyQualifiedName = assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(","));
                    }
                    taskUpdates.Add(
                   (BusinessObjects.WorkManagement.TaskUpdate)BusinessObjects.Base.Deserialize(
                   System.Type.GetType(historicalTaskUpdate.TypeName), historicalTaskUpdate.Serialized));
                }
            }

            return taskUpdates;
        }

        public static LostTimeUpdateCollection GetLostTimeUpdateHistory(int wmSourceID, eWMSourceSystem sourceSystem, string userID)
        {
            LostTimeUpdateCollection lostTimeUpdates = null;

            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobLostTimeHistory";

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            if (userID != null && userID != string.Empty)
            {
                colParameters.Add(new SqlParameter("@UserID", userID));
            }

            dtData = objADO.GetDataTable(strStoredProcedure, "LostTimeUpdates", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData != null && dtData.Rows.Count > 0)
            {
                lostTimeUpdates = new LostTimeUpdateCollection();
                foreach (DataRow drMember in dtData.Rows)
                {
                    LostTimeUpdate lostTimeUpdate = new LostTimeUpdate();
                    lostTimeUpdate.Reason = new LostTimeReason();
                    lostTimeUpdate.Reason.ID = (int)drMember["LostTimeID"];
                    lostTimeUpdate.Reason.Description = drMember["Description"].ToString();
                    lostTimeUpdate.Comments = drMember["Comments"].ToString();
                    lostTimeUpdate.Minutes = (int)drMember["Minutes"];
                    lostTimeUpdates.Add(lostTimeUpdate);
                    //////lostTimeUpdates.Add(new LostTimeUpdate());
                    //////lostTimeUpdates[lostTimeUpdates.Count - 1].LostTimeID = (int)drMember["LostTimeID"];
                    //////lostTimeUpdates[lostTimeUpdates.Count - 1].Comments = drMember["Description"].ToString();
                    //////lostTimeUpdates[lostTimeUpdates.Count - 1].Minutes = (int)drMember["Minutes"];
                }
            }

            return lostTimeUpdates;
        }

        /// <summary>
        /// Retrieve all TaskUpdates of a given type for the Job
        /// </summary>
        /// <param name="wmSourceID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static System.Collections.Generic.List<HistoricalTaskUpdate> GetStoredTaskUpdates(int wmSourceID, eWMSourceSystem sourceSystem, string typeName)
        {
            return GetStoredTaskUpdates(wmSourceID, sourceSystem, DateTime.MinValue, null, typeName);
        }

        public static BusinessObjects.WorkManagement.AppointmentTaskUpdate GetAppointmentFeedback(int wmSourceID, DateTime dateReceived)
        {
            System.Collections.Generic.List<HistoricalTaskUpdate> serializedTaskUpdates = BusinessObjects.WorkManagement.JobReference.GetStoredTaskUpdates(wmSourceID, dateReceived, typeof(BusinessObjects.WorkManagement.AppointmentTaskUpdate).Name);
            string assemblyQualifiedName = string.Empty;

            if (serializedTaskUpdates != null && serializedTaskUpdates.Count > 0)
            {
                List<BusinessObjects.WorkManagement.TaskUpdate> taskUpdates = new List<BusinessObjects.WorkManagement.TaskUpdate>();
                foreach (BusinessObjects.WorkManagement.HistoricalTaskUpdate historicalTaskUpdate in serializedTaskUpdates)
                {
                    assemblyQualifiedName = historicalTaskUpdate.TypeName;
                    if (assemblyQualifiedName.IndexOf(",") > -1)
                    {
                        assemblyQualifiedName = assemblyQualifiedName.Substring(0, assemblyQualifiedName.IndexOf(","));
                    }
                    taskUpdates.Add(
                    (BusinessObjects.WorkManagement.TaskUpdate)BusinessObjects.Base.Deserialize(
                    System.Type.GetType(historicalTaskUpdate.TypeName), historicalTaskUpdate.Serialized));

                    return (BusinessObjects.WorkManagement.AppointmentTaskUpdate)taskUpdates[0];
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// <summary>
        /// Retrieve TaskUpdates submitted together for the Job at a specific DateTime
        /// </summary>
        /// <param name="wmSourceID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="specificChangeDate"></param>
        /// <returns></returns>
        private static System.Collections.Generic.List<HistoricalTaskUpdate> GetStoredTaskUpdates(int wmSourceID, eWMSourceSystem sourceSystem, DateTime specificChangeDate)
        {
            return GetStoredTaskUpdates(wmSourceID, sourceSystem, specificChangeDate, null);
        }

        /// <summary>
        /// <summary>
        /// Retrieve TaskUpdates submitted together for the Job at a specific DateTime
        /// </summary>
        /// <param name="wmSourceID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="specificChangeDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private static System.Collections.Generic.List<HistoricalTaskUpdate> GetStoredTaskUpdates(int wmSourceID, eWMSourceSystem sourceSystem, DateTime specificChangeDate, string userID)
        {
            return GetStoredTaskUpdates(wmSourceID, sourceSystem, specificChangeDate, userID, null);
        }

        private static System.Collections.Generic.List<HistoricalTaskUpdate> GetStoredTaskUpdates(int wmSourceID, DateTime specificChangeDate, string typeName)
        {
            return GetStoredTaskUpdates(wmSourceID, eWMSourceSystem.Unspecified, specificChangeDate, null, typeName);
        }

        /// <summary>
        /// Retrieve TaskUpdates submitted together for the Job 
        /// Optionally at a specific DateTime
        /// Optionally for a specific User (and at a specific DateTime if set)
        /// </summary>
        /// <param name="wmSourceID"></param>
        /// <param name="sourceSystem"></param>
        /// <param name="specificChangeDate"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        private static System.Collections.Generic.List<HistoricalTaskUpdate> GetStoredTaskUpdates(int wmSourceID, eWMSourceSystem sourceSystem, DateTime specificChangeDate, string userID, string typeName)
        {
            System.Collections.Generic.List<HistoricalTaskUpdate> serializedTaskUpdates = null;

            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobTaskUpdates";

            colParameters.Add(new SqlParameter("@WMSourceID", wmSourceID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            if (!string.IsNullOrEmpty(userID))
            {
                colParameters.Add(new SqlParameter("@UserID", userID));
            }
            if (specificChangeDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@ChangeDate", specificChangeDate));
            }
            if (!string.IsNullOrEmpty(typeName))
            {
                colParameters.Add(new SqlParameter("@TypeName", typeName));
            }

            dtData = objADO.GetDataTable(strStoredProcedure, "TaskUpdates", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData != null && dtData.Rows.Count > 0)
            {
                serializedTaskUpdates = new System.Collections.Generic.List<HistoricalTaskUpdate>();
                foreach (DataRow drMember in dtData.Rows)
                {
                    serializedTaskUpdates.Add(new HistoricalTaskUpdate());
                    serializedTaskUpdates[serializedTaskUpdates.Count - 1].Serialized = new System.Xml.XmlDocument();
                    serializedTaskUpdates[serializedTaskUpdates.Count - 1].Serialized.LoadXml(drMember["Serialized"].ToString());
                    serializedTaskUpdates[serializedTaskUpdates.Count - 1].TypeName = (drMember["TypeName"].ToString());
                }
            }

            return serializedTaskUpdates;
        }

        #endregion
    }

    #endregion

    #region OnHoldReasonCollection class

    public partial class OnHoldReasonCollection
    {
        #region Public Methods

        public static OnHoldReasonCollection FindAll()
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            DataTable dtResults = null;
            string strStoredProcedure = "selOnHoldReasons";

            dtResults = objADO.GetDataTable(strStoredProcedure, "OnHoldReasons");
            objADO = null;

            return Populate(dtResults);
        }

        public static OnHoldReasonCollection FindByActivity(int activityID)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selActivityOnHoldReasons";

            colParameters.Add(new SqlParameter("@ActivityID", activityID));
            dtResults = objADO.GetDataTable(strStoredProcedure, "OnHoldReasons", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return Populate(dtResults);
        }

        public static OnHoldReasonCollection FindByJob(int jobID)
        {
            OnHoldReasonCollection colMembers = new OnHoldReasonCollection();
            OnHoldReason onHoldReason = null;

            DataAccess objADO = new DataAccess(); // TaskStore database
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobOnHoldDetails";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            dtResults = objADO.GetDataTable(strStoredProcedure, "OnHoldReasons", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                foreach (DataRow drMember in dtResults.Rows)
                {
                    onHoldReason = new OnHoldReason();
                    onHoldReason.ID = (int)drMember["OnHoldReasonID"];
                    onHoldReason.Code = drMember["OnHoldReasonCode"].ToString();
                    onHoldReason.Description = drMember["OnHoldReasonDesc"].ToString();

                    // Note. Additional columns expected in this method to other Find methods
                    if (!drMember["DateResolved"].Equals(DBNull.Value))
                    {
                        onHoldReason.DateResolved = (DateTime)drMember["DateResolved"];
                    }
                    if (!drMember["ResolvingUser"].Equals(DBNull.Value))
                    {
                        onHoldReason.ResolvingUser = drMember["ResolvingUser"].ToString();
                    }
                    if (!drMember["Comments"].Equals(DBNull.Value))
                    {
                        System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                        objDOM.LoadXml(drMember["Comments"].ToString());
                        onHoldReason.Comments = (CommentAuditRecordCollection)BusinessObjects.Base.Deserialize(typeof(CommentAuditRecordCollection), objDOM);
                    }

                    colMembers.Add(onHoldReason);
                }
            }

            return colMembers;
        }

        /// <summary>
        /// Populate OH Reason with core attributes (ID, Code, & Description)
        /// Called by all Find overloads and from LoadJobFromWorkOrder in WIS
        /// </summary>
        /// <param name="dtResults"></param>
        /// <returns></returns>
        public static OnHoldReasonCollection Populate(DataTable dtResults)
        {
            return Populate(dtResults, null);
        }

        /// <summary>
        /// Populate OH Reason with core attributes (ID, Code, & Description)
        /// Called by all Find overloads and from LoadJobFromWorkOrder in WIS
        /// </summary>
        /// <param name="dtResults"></param>
        /// <returns></returns>
        public static OnHoldReasonCollection Populate(DataTable dtResults, DataTable materialsRequested)
        {
            OnHoldReasonCollection colMembers = null;
            OnHoldReason onHoldReason = null;
            string filterExpression = string.Empty;

            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                colMembers = new OnHoldReasonCollection();
                foreach (DataRow drMember in dtResults.Rows)
                {
                    onHoldReason = new OnHoldReason();
                    onHoldReason.ID = (int)drMember["OnHoldReasonID"];
                    onHoldReason.Code = drMember["OnHoldReasonCode"].ToString();
                    onHoldReason.Description = drMember["OnHoldReasonDesc"].ToString();
                    if (dtResults.Columns.Contains("WMSourceSystem") &&
                        !drMember["WMSourceSystem"].Equals(DBNull.Value))
                    {
                        onHoldReason.SourceSystem = (eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), drMember["WMSourceSystem"].ToString());
                    }
                    if (dtResults.Columns.Contains("Comments") &&
                        !drMember["Comments"].Equals(DBNull.Value))
                    {
                        System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                        objDOM.LoadXml(drMember["Comments"].ToString());
                        onHoldReason.Comments = (CommentAuditRecordCollection)BusinessObjects.Base.Deserialize(typeof(CommentAuditRecordCollection), objDOM);
                    }

                    if (materialsRequested != null && onHoldReason.Code.ToUpper() == "MAT")
                    {
                        onHoldReason.MaterialsRequired = MaterialCollection.Populate(materialsRequested);
                    }
               
                    if (!drMember["DateResolved"].Equals(DBNull.Value))
                    {
                        DateTime dateResolved = (DateTime)drMember["DateResolved"];
                        if (dateResolved != DateTime.MinValue)
                        {
                            onHoldReason.DateResolved = dateResolved;
                        }
                    }

                    colMembers.Add(onHoldReason);
                }
            }

            return colMembers;
        }

        #endregion
    }

    #endregion

    #region JobFailureCollection class

    public partial class JobFailureCollection
    {
        public JobFailure Find(int iD, eWMSourceSystem sourceSystem)
        {
            foreach (JobFailure jobFailure in this)
            {
                if (jobFailure.ID == iD && jobFailure.SourceSystem == sourceSystem)
                {
                    return jobFailure;
                }
            }
            return null;
        }

        public static JobFailureCollection Populate(DataTable collectionMembers)
        {
            JobFailureCollection jobFailures = new JobFailureCollection();

            foreach (DataRow drMember in collectionMembers.Rows)
            {
                jobFailures.Add(new JobFailure());
                jobFailures[jobFailures.Count - 1].ID = (int)drMember["WMSourceID"];
                jobFailures[jobFailures.Count - 1].SourceSystem = (eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), drMember["WMSourceSystem"].ToString());
                jobFailures[jobFailures.Count - 1].DateRecorded = (DateTime)drMember["FailureDate"];
                jobFailures[jobFailures.Count - 1].Type = (eJobFailureType)Enum.Parse(typeof(eJobFailureType), drMember["FailureType"].ToString());
                if (!drMember["ExceptionMessage"].Equals(DBNull.Value))
                {
                    jobFailures[jobFailures.Count - 1].ExceptionMessage = drMember["ExceptionMessage"].ToString();
                }
                if (!drMember["RetryCount"].Equals(DBNull.Value))
                {
                    jobFailures[jobFailures.Count - 1].RetryCount = (int)drMember["RetryCount"];
                }
                if (!drMember["ServiceWrapperType"].Equals(DBNull.Value))
                {
                    jobFailures[jobFailures.Count - 1].ServiceWrapperType = drMember["ServiceWrapperType"].ToString();
                }
            }

            return jobFailures;
        }

        public static JobFailureCollection Fetch(string sourceSystem)
        {
            return Fetch(-1, sourceSystem);
        }

        public static JobFailureCollection Fetch(int jobID, string sourceSystem)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selFailedJobs";

            if (jobID > 0 || jobID < -1)    //  Cater for those failure IDs held as negative numbers
            {
                colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            }
            if (!string.IsNullOrEmpty(sourceSystem) && sourceSystem.ToUpper()!="UNSPECIFIED")
            {
                colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem));
            }

            // Assemble Parameters
            SqlParameter[] sqlParameterArray = null;
            if (colParameters != null && colParameters.Count > 0)
            {
                sqlParameterArray = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));
            }

            dtResults = objADO.GetDataTable(strStoredProcedure, "FailedJobs", sqlParameterArray);

            return Populate(dtResults);
        }
    }

    #endregion

    #region JobFailure class

    public partial class JobFailure
    {
        public static bool Remove(int jobID, eJobFailureType failureType)
        {
            return Remove(jobID, failureType, DateTime.MinValue);
        }

        public static bool Remove(int jobID, eJobFailureType failureType, DateTime failureDate)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delFailedJobAudit";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@FailureType", failureType.ToString()));
            if (failureDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@FailureDate", failureDate));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public static bool UpdateSerializedContent(int jobID, eWMSourceSystem sourceSystem, string serializedContent)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter xmlParameter = null;
            string strStoredProcedure = "updFailedJobAuditSerialized";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
            xmlParameter = new SqlParameter("@SerializedObject", SqlDbType.Xml);
            xmlParameter.Value = new System.Data.SqlTypes.SqlXml(new System.Xml.XmlTextReader(serializedContent, System.Xml.XmlNodeType.Document, null));
            colParameters.Add(xmlParameter);

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }

        public static System.Xml.XmlDocument GetSerializedContent(int jobID, eJobFailureType failureType)
        {
            return GetSerializedContent(jobID, failureType, DateTime.MinValue);
        }

        public static System.Xml.XmlDocument GetSerializedContent(int jobID, eJobFailureType failureType, DateTime failureDate)
        {
            System.Xml.XmlDocument serializedContent = null;

            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selFailedJobSerializedContent";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@FailureType", failureType.ToString()));
            if (failureDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@FailureDate", failureDate));
            }
            dtResults = objADO.GetDataTable(strStoredProcedure, "FailedJobs", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            foreach (DataRow drSerializedContent in dtResults.Rows)
            {
                if (!drSerializedContent["Serialized"].Equals(DBNull.Value))
                {
                    serializedContent = new System.Xml.XmlDocument();
                    serializedContent.LoadXml(drSerializedContent["Serialized"].ToString()); //.Replace(" xmlns=\"http://FinalBuild.co.uk/BusinessObjects.WorkManagement\"", string.Empty));

                    return serializedContent;
                }
            }

            return serializedContent;
        }
    }

    #endregion

    #region Worker class

    public partial class Worker
    {
        public static WorkerAddress GetWorkerAddress(string empNo)
        {
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = new SqlParameter[1];
            arrParameters[0] = new SqlParameter("@EmpNo", SqlDbType.VarChar, 5);
            arrParameters[0].Value = empNo;

            // Assemble Parameters
            dtResults = objADO.GetDataTable("selEmployeeAddress", "EmployeeAddress", arrParameters);
            if (dtResults == null || dtResults.Rows.Count == 0)
            {
                return null;
            }

            WorkerAddress address = new WorkerAddress();

            if (dtResults.Rows[0]["Address_Line1"] == DBNull.Value)
            {
                address.AddressLine1 = string.Empty;
            }
            else
            {
                address.AddressLine1 = dtResults.Rows[0]["Address_Line1"].ToString();
            }
            if (dtResults.Rows[0]["Address_Line2"] == DBNull.Value)
            {
                address.AddressLine2 = string.Empty;
            }
            else
            {
                address.AddressLine2 = dtResults.Rows[0]["Address_Line2"].ToString();
            }
            if (dtResults.Rows[0]["Address_Line3"] == DBNull.Value)
            {
                address.AddressLine3 = string.Empty;
            }
            else
            {
                address.AddressLine3 = dtResults.Rows[0]["Address_Line3"].ToString();
            }
            if (dtResults.Rows[0]["Town"] == DBNull.Value)
            {
                address.Town = string.Empty;
            }
            else
            {
                address.Town = dtResults.Rows[0]["Town"].ToString();
            }
            if (dtResults.Rows[0]["Postcode"] == DBNull.Value)
            {
                address.PostCode = string.Empty;
            }
            else
            {
                address.PostCode = dtResults.Rows[0]["Postcode"].ToString();
            }

            return address;
        }

        public static bool SaveUserDetails(string userID, AreaDetails areaDetails, bool isClickEngineer)
        {
            return SaveUserDetails(userID, areaDetails, eMobileIdentityType.WindowsIdentity, null, "1");
        }


        public static bool SaveUserDetails(string userID, AreaDetails areaDetails)
        {
            return SaveUserDetails(userID, areaDetails, eMobileIdentityType.WindowsIdentity, null, null);
        }

        public static bool SaveUserDetails(string userID, AreaDetails areaDetails, eMobileIdentityType identityType, string displayName, string isClickEngineer)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updUserDetails";

            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@PrimaryArea", areaDetails.PrimaryArea.Name));
            if (areaDetails.SubArea != null && areaDetails.SubArea.Name != null && areaDetails.SubArea.Name != string.Empty)
            {
                colParameters.Add(new SqlParameter("@SubArea", areaDetails.SubArea.Name));
            }
            if (displayName != null && displayName != string.Empty)
            {
                colParameters.Add(new SqlParameter("@DisplayName", displayName));
            }
            colParameters.Add(new SqlParameter("@MobileIdentityType", identityType.ToString()));

            if (isClickEngineer != null && isClickEngineer != string.Empty)
            {
                bool isClickEngineerValue = false;
                if (isClickEngineer == "1")
                    isClickEngineerValue = true;
 
                colParameters.Add(new SqlParameter("@IsClickEngineer", isClickEngineerValue));
            }

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);
        }
    }

    #endregion

    #region Contractor

    public partial class Contractor
    {
        /// <summary>
        /// Handles New as well as Existing
        /// </summary>
        /// <param name="gangMember"></param>
        /// <returns></returns>
        public static bool Save(string userID, string EmailAddress, string TelephoneNo, string MobileNo, string Password)
        {
            bool bolReturn = false;
            DataAccess objADO = new DataAccess();

            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updContractorDetails";

            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@EmailAddress", EmailAddress));

            colParameters.Add(new SqlParameter("@TelephoneNo", TelephoneNo));
            colParameters.Add(new SqlParameter("@MobileNo", MobileNo));
            colParameters.Add(new SqlParameter("@Password", Password));

            int intReturn = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intReturn > 0);

        }

        public static bool Login(string userID, string password)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            int intReturn = 0;
            string strStoredProcedure = "selContractorByPassword";


            colParameters.Add(new SqlParameter("@ContractorPassword", password));
            colParameters.Add(new SqlParameter("@ContractorID", userID));
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

        public static string GetEmailAddress(string userID)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter objParameter = null;
            string strReturn = "";
            string strStoredProcedure = "selContractorEmail";

            colParameters.Add(new SqlParameter("@UserID", userID));
            objParameter = new SqlParameter("@Email", SqlDbType.VarChar);
            objParameter.Direction = ParameterDirection.Output;
            objParameter.Value = "";
            objParameter.Size = 255;
            colParameters.Add(objParameter);
            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!objParameter.Value.Equals(DBNull.Value))
            {
                strReturn = (string)objParameter.Value;
            }

            return strReturn;
        }

    }

    #endregion

    #region MobileIdentityCollection

    partial class MobileIdentityCollection
    {
        /// <summary>
        /// DataTable is passed by Mobile call to FieldData db routine selUsers initially
        /// </summary>
        /// <param name="dtUsers"></param>
        /// <returns></returns>
        public static MobileIdentityCollection Populate(DataTable dtUsers)
        {
            MobileIdentityCollection mobileIdentities = null;

            if (dtUsers.Rows.Count > 0)
            {
                mobileIdentities = new MobileIdentityCollection();
                for (int rowIndex = 0; rowIndex < dtUsers.Rows.Count; rowIndex++)
                {
                    mobileIdentities.Add(new MobileIdentity());
                    mobileIdentities[rowIndex].UserID = dtUsers.Rows[rowIndex]["UserID"].ToString();
                    if (!dtUsers.Rows[rowIndex]["MobileIdentityType"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].Type = (eMobileIdentityType)Enum.Parse(typeof(eMobileIdentityType), dtUsers.Rows[rowIndex]["MobileIdentityType"].ToString());
                    }
                    if (!dtUsers.Rows[rowIndex]["EmployeeName"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].DisplayName = dtUsers.Rows[rowIndex]["EmployeeName"].ToString();
                    }
                    else
                    {
                        mobileIdentities[rowIndex].DisplayName = mobileIdentities[rowIndex].UserID;
                    }
                    if (!dtUsers.Rows[rowIndex]["EmpNo"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].EmpNo = dtUsers.Rows[rowIndex]["EmpNo"].ToString();
                    }

                    if (!dtUsers.Rows[rowIndex]["EmpForenames"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].Forenames = dtUsers.Rows[rowIndex]["EmpForenames"].ToString();
                    }

                    if (!dtUsers.Rows[rowIndex]["EmpSurname"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].Surname = dtUsers.Rows[rowIndex]["EmpSurname"].ToString();
                    }

                    mobileIdentities[rowIndex].AreaDetails = new AreaDetails();
                    mobileIdentities[rowIndex].AreaDetails.PrimaryArea = new Area();
                    mobileIdentities[rowIndex].AreaDetails.PrimaryArea.Name = "UNKNOWN";
                    mobileIdentities[rowIndex].AreaDetails.SubArea = new Area();
                    mobileIdentities[rowIndex].AreaDetails.SubArea.Name = "UNKNOWN";

                    if (!dtUsers.Rows[rowIndex]["PrimaryArea"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].AreaDetails.PrimaryArea.Name = dtUsers.Rows[rowIndex]["PrimaryArea"].ToString();
                    }

                    if (!dtUsers.Rows[rowIndex]["SubArea"].Equals(DBNull.Value))
                    {
                        mobileIdentities[rowIndex].AreaDetails.SubArea.Name = dtUsers.Rows[rowIndex]["SubArea"].ToString();
                    }
                }
            }

            return mobileIdentities;
        }
    }

    #endregion

    #region EngineerNonAvailabilityCollection class

    public partial class EngineerNonAvailabilityCollection
    {
        #region Public Methods

        public static EngineerNonAvailabilityCollection FindByUser(string userID, DateTime cutoffDateForInclusion, ref DateTime lastKnownUpdatedDate)
        {
            EngineerNonAvailabilityCollection nonAvailability = null;

            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            SqlParameter outputParameterSerialized;
            SqlParameter outputParameterLastUpdated;
            string strStoredProcedure = "selUserNonAvailability";

            colParameters.Add(new SqlParameter("@UserID", userID));
            if (lastKnownUpdatedDate != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@LastKnownUpdatedDate", lastKnownUpdatedDate));
            }
            outputParameterSerialized = new SqlParameter("@NonAvailabilitySerialized", SqlDbType.Xml, 1);
            outputParameterSerialized.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameterSerialized);
            outputParameterLastUpdated = new SqlParameter("@NonAvailabilityLastUpdatedDate", SqlDbType.DateTime);
            outputParameterLastUpdated.Direction = ParameterDirection.Output;
            colParameters.Add(outputParameterLastUpdated);

            objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (!outputParameterSerialized.Value.Equals(DBNull.Value))
            {
                System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                objDOM.LoadXml(outputParameterSerialized.Value.ToString());
                nonAvailability = (EngineerNonAvailabilityCollection)BusinessObjects.Base.Deserialize(typeof(EngineerNonAvailabilityCollection), objDOM);

                if (cutoffDateForInclusion != DateTime.MinValue)
                {
                    nonAvailability.RemoveRedundantItems(cutoffDateForInclusion);
                }
            }
            if (!outputParameterLastUpdated.Value.Equals(DBNull.Value))
            {
                lastKnownUpdatedDate = (DateTime)outputParameterLastUpdated.Value;
            }

            return nonAvailability;
        }

        public static EngineerNonAvailabilityCollection FindByUser(string userID, ref DateTime lastKnownUpdatedDate)
        {
            return FindByUser(userID, DateTime.MinValue, ref lastKnownUpdatedDate);
        }

        public static void ExpireEngineerNonAvailability()
        {
            DateTime cutoffDateForInclusion = DateTime.Now;
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            string strStoredProcedure = "selUsersWithNonAvailability";

            //  Get a table of results from the SP
            dtResults = objADO.GetDataSet(strStoredProcedure, arrParameters).Tables[0];

            if (dtResults != null)
            {
                foreach (DataRow drMember in dtResults.Rows)
                {
                    string userID = drMember["UserID"].ToString();
                    string serializedNonAvailability = drMember["NonAvailabilitySerialized"].ToString();

                    EngineerNonAvailabilityCollection nonAvailability = null;

                    System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                    objDOM.LoadXml(serializedNonAvailability);
                    nonAvailability = (EngineerNonAvailabilityCollection)BusinessObjects.Base.Deserialize(typeof(EngineerNonAvailabilityCollection), objDOM);

                    nonAvailability.Save(userID, cutoffDateForInclusion);
                }
            }
        }

        public static bool IsAssigneeAvailable(string userID, DateTime targetDateTime, bool isGang)
        {
            bool isUserAvailable = true;
            BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection nonAvailabilities = null;
            DateTime lastKnownUpdatedDate = DateTime.MinValue;

            if (isGang)
            {
                // Gang specific check, look through NA of Gang.Members
                WorkerCollection gangs = WorkerCollection.GetTypedWorkers(string.Empty, new string[] { userID });
                if (gangs != null && gangs.Count > 0 && gangs[0] is Gang)
                {
                    Gang assignedGang = (Gang)gangs[0];
                    if (assignedGang.Members != null)
                    {
                        foreach (Worker gangMember in assignedGang.Members)
                        {
                            nonAvailabilities = BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection.FindByUser(gangMember.UserID, ref lastKnownUpdatedDate);
                            isUserAvailable = IsUnavailableInPeriod(targetDateTime, nonAvailabilities);
                            if (!isUserAvailable)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // Any type of Worker check (including Gangs, NA allowed at Gang level)
            if (isUserAvailable)
            {
                nonAvailabilities = BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection.FindByUser(userID, ref lastKnownUpdatedDate);
                isUserAvailable = IsUnavailableInPeriod(targetDateTime, nonAvailabilities);
            }

            return isUserAvailable;
        }

        private static bool IsUnavailableInPeriod(DateTime periodStartDateTime, BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection nonAvailabilities)
        {
            bool isUserAvailable = true;

            if (nonAvailabilities != null)
            {
                foreach (BusinessObjects.WorkManagement.EngineerNonAvailability slot in nonAvailabilities)
                {
                    if (periodStartDateTime >= slot.StartDate &&
                        periodStartDateTime <= slot.EndDate)
                    {
                        isUserAvailable = false;
                        break;
                    }
                }
            }

            return isUserAvailable;
        }

        #endregion
    }

    #endregion

    #region AssignmentDetails

    public partial class AssignmentDetails
    {

        /// <summary>
        /// Handles both New & Existing Assignments in the Eligible For Dispatch table
        /// </summary>
        /// <param name="assignment"></param>
        /// <returns></returns>
        public static bool SaveAssignmentEligibleForDispatch(AssignmentDetails assignment)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.FieldData);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "updEligibleForDispatch";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", assignment.ID));
            colParameters.Add(new SqlParameter("@UserID", assignment.Workers[0].UserID));
            colParameters.Add(new SqlParameter("@InstanceNumber", assignment.InstanceNumber));
            colParameters.Add(new SqlParameter("@DateChangeDetected", DateTime.Now));
            colParameters.Add(new SqlParameter("@WMSourceSystem", assignment.SourceSystem.ToString()));

            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }

        public static bool RemoveAssignment(AssignmentDetails assignment)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.FieldData);
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delEligibleForDispatch";
            int intRowsAffected = 0;

            colParameters.Add(new SqlParameter("@WMSourceID", assignment.ID));
            if (assignment.InstanceNumber > -1)
            {
                colParameters.Add(new SqlParameter("@InstanceNumber", assignment.InstanceNumber));
            }
            if (assignment.Workers != null && assignment.Workers.Count > 0 && !string.IsNullOrEmpty(assignment.Workers[0].UserID))
            {
                colParameters.Add(new SqlParameter("@UserID", assignment.Workers[0].UserID));
            }

            intRowsAffected = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return (intRowsAffected > 0);
        }

        public static AssignmentDetailsCollection GetAllAssignmentInstances(AssignmentDetails assignment)
        {

            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.FieldData);
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selEligibleForDispatch";

            colParameters.Add(new SqlParameter("@WMSourceID", assignment.ID));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Assignments", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            return Populate(dtResults);

        }

        private static AssignmentDetailsCollection  Populate(DataTable collectionMembers)
        {
            AssignmentDetailsCollection colMembers = new AssignmentDetailsCollection();
            AssignmentDetails obj = null;

            foreach (DataRow drMember in collectionMembers.Rows)
            {
                obj = new AssignmentDetails();
                obj.ID = (int)drMember["WMSourceID"];
                obj.InstanceNumber = (int)drMember["JobInstanceNumber"];

                obj.Workers = new WorkerCollection();
                Worker objWorker = new Worker();
                objWorker.UserID = drMember["UserID"].ToString();
                obj.Workers.Add(objWorker);

                obj.DueDate = (DateTime)drMember["DateChangeDetected"];
                colMembers.Add(obj);
            }

            return colMembers;
        }

    }

    #endregion

    #region AssignmentDetailsCollection

    public partial class AssignmentDetailsCollection
    {

        #region Public Methods
        public static AssignmentDetailsCollection GetAssignments(string userID, int[] sourceIDs)
        {
            AssignmentDetailsCollection assignments = new AssignmentDetailsCollection();
            DataSet dsResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters = null;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            DataRow[] filteredRows = null;
            string strStoredProcedure = "selAssignmentsByJobAndUser";

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);
            if (!string.IsNullOrEmpty(userID))
            {
                colParameters.Add(new SqlParameter("@UserID", userID));
                arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));
            }

            dsResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters);
            dsResults.Tables[0].TableName = "Assignments";
            dsResults.Tables[1].TableName = "Assignees";
            dsResults.Tables[2].TableName = "Appointments";

            foreach (DataRow drAssignment in dsResults.Tables["Assignments"].Rows)
            {
                assignments.Add(new AssignmentDetails());
                assignments[assignments.Count - 1].ID = int.Parse(drAssignment["WMSourceID"].ToString());
                assignments[assignments.Count - 1].SourceSystem = (eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), drAssignment["WMSourceSystem"].ToString());
                if (!drAssignment["DueDate"].Equals(DBNull.Value))
                {
                    assignments[assignments.Count - 1].DueDate = (DateTime)drAssignment["DueDate"];
                }
                assignments[assignments.Count - 1].Status = (eJobStatus)Enum.Parse(typeof(eJobStatus), drAssignment["JobStatus"].ToString());

                filteredRows = dsResults.Tables["Assignees"].Select(string.Format("WMSourceID='{0}'", assignments[assignments.Count - 1].ID));

                if (filteredRows.Length > 0)
                {
                    int userIDJobInstanceNumber = 0;
                    assignments[assignments.Count - 1].Workers = new WorkerCollection();
                    for (int assigneeIndex = 0; assigneeIndex < filteredRows.Length; assigneeIndex++)
                    {
                        assignments[assignments.Count - 1].Workers.Add(new Worker());
                        assignments[assignments.Count - 1].Workers[assigneeIndex].LoginName = filteredRows[assigneeIndex]["UserID"].ToString();
                        if (!filteredRows[assigneeIndex]["EmpSurname"].Equals(DBNull.Value))
                        {
                            assignments[assignments.Count - 1].Workers[assigneeIndex].Surname = filteredRows[assigneeIndex]["EmpSurname"].ToString();
                        }
                        if (!filteredRows[assigneeIndex]["EmpForenames"].Equals(DBNull.Value))
                        {
                            assignments[assignments.Count - 1].Workers[assigneeIndex].Forenames = filteredRows[assigneeIndex]["EmpForenames"].ToString();
                        }
                        if (!filteredRows[assigneeIndex]["EmpNo"].Equals(DBNull.Value))
                        {
                            assignments[assignments.Count - 1].Workers[assigneeIndex].EmpNo = filteredRows[assigneeIndex]["EmpNo"].ToString();
                        }
                        if (!string.IsNullOrEmpty(userID)) // Allow for null UserID when called from GetSpecificWork() method
                        {
                            if (filteredRows[assigneeIndex]["UserID"].ToString().ToUpper() == userID.ToUpper())
                            {
                                userIDJobInstanceNumber = int.Parse(filteredRows[assigneeIndex]["JobInstanceNumber"].ToString());
                            }
                        }
                    }
                    assignments[assignments.Count - 1].InstanceNumber = userIDJobInstanceNumber;
                }

                filteredRows = dsResults.Tables["Appointments"].Select(string.Format("WMSourceID={0}", assignments[assignments.Count - 1].ID));
                if (filteredRows.Length > 0)
                {
                    foreach (DataRow drSerializedInstance in filteredRows)
                    {
                        if (!drSerializedInstance["Appointment"].Equals(DBNull.Value))
                        {
                            System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                            objDOM.LoadXml(drSerializedInstance["Appointment"].ToString()); //.Replace(" xmlns=\"http://FinalBuild.co.uk/BusinessObjects.WorkManagement\"", string.Empty));
                            assignments[assignments.Count - 1].Appointment = (Appointment)BusinessObjects.Base.Deserialize(typeof(Appointment), objDOM);
                        }
                    }
                }
            }

            return assignments;
        }

        public static AssignmentDetailsCollection GetAssignments(int[] sourceIDs)
        {
            AssignmentDetailsCollection assignments = new AssignmentDetailsCollection();
            DataSet dsResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            DataRow[] filteredRows = null;
            string strStoredProcedure = "selAssignmentsByJobAndUser";

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);
            //colParameters.Add(new SqlParameter("@UserID", userID));
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dsResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters);
            dsResults.Tables[0].TableName = "Assignments";
            dsResults.Tables[1].TableName = "Assignees";
            dsResults.Tables[2].TableName = "Appointments";

            foreach (DataRow drAssignment in dsResults.Tables["Assignments"].Rows)
            {
                assignments.Add(new AssignmentDetails());
                assignments[assignments.Count - 1].ID = int.Parse(drAssignment["WMSourceID"].ToString());
                assignments[assignments.Count - 1].SourceSystem = (eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), drAssignment["WMSourceSystem"].ToString());
                if (!drAssignment["DueDate"].Equals(DBNull.Value))
                {
                    assignments[assignments.Count - 1].DueDate = (DateTime)drAssignment["DueDate"];
                }
                assignments[assignments.Count - 1].Status = (eJobStatus)Enum.Parse(typeof(eJobStatus), drAssignment["JobStatus"].ToString());

                filteredRows = dsResults.Tables["Assignees"].Select(string.Format("WMSourceID='{0}'", assignments[assignments.Count - 1].ID));

                if (filteredRows.Length > 0)
                {
                    int userIDJobInstanceNumber = 0;
                    assignments[assignments.Count - 1].Workers = new WorkerCollection();
                    for (int assigneeIndex = 0; assigneeIndex < filteredRows.Length; assigneeIndex++)
                    {
                        assignments[assignments.Count - 1].Workers.Add(new Worker());
                        assignments[assignments.Count - 1].Workers[assigneeIndex].LoginName = filteredRows[assigneeIndex]["UserID"].ToString();
                        if (!filteredRows[assigneeIndex]["EmpSurname"].Equals(DBNull.Value))
                        {
                            assignments[assignments.Count - 1].Workers[assigneeIndex].Surname = filteredRows[assigneeIndex]["EmpSurname"].ToString();
                        }
                        if (!filteredRows[assigneeIndex]["EmpForenames"].Equals(DBNull.Value))
                        {
                            assignments[assignments.Count - 1].Workers[assigneeIndex].Forenames = filteredRows[assigneeIndex]["EmpForenames"].ToString();
                        }
                        if (!filteredRows[assigneeIndex]["EmpNo"].Equals(DBNull.Value))
                        {
                            assignments[assignments.Count - 1].Workers[assigneeIndex].EmpNo = filteredRows[assigneeIndex]["EmpNo"].ToString();
                        }
                        //if (filteredRows[assigneeIndex]["UserID"].ToString().ToUpper() == userID.ToUpper())
                        //{
                            userIDJobInstanceNumber = int.Parse(filteredRows[assigneeIndex]["JobInstanceNumber"].ToString());
                        //}
                    }
                    assignments[assignments.Count - 1].InstanceNumber = userIDJobInstanceNumber;
                }

                filteredRows = dsResults.Tables["Appointments"].Select(string.Format("WMSourceID={0}", assignments[assignments.Count - 1].ID));
                if (filteredRows.Length > 0)
                {
                    foreach (DataRow drSerializedInstance in filteredRows)
                    {
                        if (!drSerializedInstance["Appointment"].Equals(DBNull.Value))
                        {
                            System.Xml.XmlDocument objDOM = new System.Xml.XmlDocument();
                            objDOM.LoadXml(drSerializedInstance["Appointment"].ToString()); //.Replace(" xmlns=\"http://FinalBuild.co.uk/BusinessObjects.WorkManagement\"", string.Empty));
                            assignments[assignments.Count - 1].Appointment = (Appointment)BusinessObjects.Base.Deserialize(typeof(Appointment), objDOM);
                        }
                    }
                }
            }

            return assignments;
        }

        public static List<ExtendedAssignment> GetAssignmentsEligibleForDispatch(DateTime maximumDueDate, eWMSourceSystem sourceSystem)
        {
            List<ExtendedAssignment> assignments = new List<ExtendedAssignment>();
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selAssignmentsEligibleForDispatch";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@MaximumDueDate", maximumDueDate));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Assignments", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            assignments = PopulateAssignmentsForDispatch(dtResults);

            return assignments;
        }

        public static List<ExtendedAssignment> GetAssignmentsChangedPostDispatched(DateTime dispatchProcesorLastRunDate)
        {
            List<ExtendedAssignment> assignments = new List<ExtendedAssignment>();
            DataTable dtResults = null;
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            string strStoredProcedure = "selAssignmentsChangedPostDispatched";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@DispatchProcesorLastRunDate", dispatchProcesorLastRunDate));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Assignments", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            assignments = PopulateAssignmentsForDispatch(dtResults);

            return assignments;
        }

        public static AssignmentDetailsCollection GetAssignmentsSupportingDataChanged(string userID, string sourceSystem, DateTime lastRunDate, out Hashtable SupportingDataChangeDates)
        {
            AssignmentDetailsCollection assignments = new AssignmentDetailsCollection();
            FinalBuild.DataAccess objADO = Domain.GetADOInstance();
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;
            DataSet dsResults = null;
            int[] wmSourceIDs = null;
            int[] candidateWMSourceIDs = null;
            string strStoredProcedure = "selSupportingDataChanged";
            SupportingDataChangeDates = new Hashtable();

            //Get a list of all unacknowledged Assignment IDs from Mobile WIP
            candidateWMSourceIDs = GetUnAcknowledgedSupportingDataAssignmentIDs(userID);

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@LastRunDate", lastRunDate));
            if (sourceSystem != null)
            {
                colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem));
            }
            if (candidateWMSourceIDs != null && candidateWMSourceIDs.Length > 0)
            {
                objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", candidateWMSourceIDs);
                dsResults = objADO.GetDataSet(strStoredProcedure, objParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            }
            else
            {
                dsResults = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }

            if (dsResults != null && dsResults.Tables.Count > 0 && dsResults.Tables[0].Rows.Count > 0)
            {
                wmSourceIDs = PopulateSupportingDataAssignmentIDs(dsResults.Tables[0], out SupportingDataChangeDates);
                assignments = GetAssignments(userID, wmSourceIDs);
            }

            return assignments;
        }



        public static int[] GetUnAcknowledgedSupportingDataAssignmentIDs(string userID)
        {
            int[] unacknowledgedSupportingDataWMSourceIDs = null;
            DataTable dtResults = null;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            FinalBuild.DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.FieldData);
            string strStoredProcedure = "selUserSupportingDataNotAcknowledged";

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@UserID", userID));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Work", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                unacknowledgedSupportingDataWMSourceIDs = PopulateSupportingDataAssignmentIDs(dtResults);
            }

            return unacknowledgedSupportingDataWMSourceIDs;
        }

        #endregion

        #region Private Methods

        private static List<ExtendedAssignment> PopulateAssignmentsForDispatch(DataTable dtResults)
        {
            List<ExtendedAssignment> assignments = new List<ExtendedAssignment>();

            foreach (DataRow drAssignment in dtResults.Rows)
            {
                if (!drAssignment["DueDate"].Equals(DBNull.Value) && !string.IsNullOrEmpty(drAssignment["UserID"].ToString()))
                {
                    assignments.Add(new ExtendedAssignment());
                    assignments[assignments.Count - 1].Assignment = new AssignmentDetails();
                    assignments[assignments.Count - 1].Assignment.Workers = new WorkerCollection();
                    assignments[assignments.Count - 1].Assignment.Workers.Add(new Worker());
                    //---------------------------------------------------------------------------------------
                    assignments[assignments.Count - 1].JobTypeName = drAssignment["TypeName"].ToString();
                    if (!drAssignment["SiteID"].Equals(DBNull.Value))
                    {
                        assignments[assignments.Count - 1].SiteID = (int)drAssignment["SiteID"];
                    }
                    assignments[assignments.Count - 1].WorkType = (eWorkType)Enum.Parse(typeof(eWorkType), drAssignment["WorkType"].ToString()); ;
                    //---------------------------------------------------------------------------------------
                    assignments[assignments.Count - 1].Assignment.Workers[0].UserID = drAssignment["UserID"].ToString();
                    assignments[assignments.Count - 1].Assignment.ID = int.Parse(drAssignment["WMSourceID"].ToString());
                    assignments[assignments.Count - 1].Assignment.SourceSystem = (eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), drAssignment["WMSourceSystem"].ToString());
                    assignments[assignments.Count - 1].Assignment.InstanceNumber = (int)drAssignment["JobInstanceNumber"];
                    assignments[assignments.Count - 1].Assignment.DueDate = (DateTime)drAssignment["DueDate"];
                    assignments[assignments.Count - 1].Assignment.Status = (eJobStatus)Enum.Parse(typeof(eJobStatus), drAssignment["JobStatus"].ToString()); ;
                }
            }

            return assignments;
        }

        private static int[] PopulateSupportingDataAssignmentIDs(DataTable dtResults)
        {
            int[] wmSourceIDs = new int[dtResults.Rows.Count];

            for (int i = 0; i < dtResults.Rows.Count; i++)
            {
                wmSourceIDs[i] = int.Parse(dtResults.Rows[i]["WMSourceID"].ToString());
            }

            return wmSourceIDs;
        }

        private static int[] PopulateSupportingDataAssignmentIDs(DataTable dtResults, out Hashtable SupportDataChangesDates)
        {
            int[] wmSourceIDs = new int[dtResults.Rows.Count];
            SupportDataChangesDates = new Hashtable();

            for (int i = 0; i < dtResults.Rows.Count; i++)
            {
                wmSourceIDs[i] = int.Parse(dtResults.Rows[i]["WMSourceID"].ToString());
                SupportDataChangesDates.Add(int.Parse(dtResults.Rows[i]["WMSourceID"].ToString()), DateTime.Parse(dtResults.Rows[i]["SupportingDataChangeDate"].ToString()));
            }

            return wmSourceIDs;
        }

        #endregion


    }

    #endregion

    #region SupportingAssignmentDetailsCollection

    public partial class SupportingAssignmentDetailsCollection
    {

        #region Public Methods

        public static SupportingAssignmentDetailsCollection GetSupportingAssignmentsChanged(string userID, string sourceSystem, DateTime lastRunDate)
        {
            AssignmentDetailsCollection assignments = new AssignmentDetailsCollection();
            SupportingAssignmentDetailsCollection supportingAssignments = new SupportingAssignmentDetailsCollection();
            Hashtable supportingDataChangesDates = new Hashtable();

            assignments = AssignmentDetailsCollection.GetAssignmentsSupportingDataChanged(userID, sourceSystem, lastRunDate, out supportingDataChangesDates);

            //Build a Support Assignment Details Collection.
            if (assignments.Count > 0 && supportingDataChangesDates.Count > 0)
            {
                supportingAssignments = BuildSupportingAssignmentDetailsCollection(assignments, supportingDataChangesDates);
            }

            return supportingAssignments;
        }

        #endregion

        #region Private Methods

        private static SupportingAssignmentDetailsCollection BuildSupportingAssignmentDetailsCollection(AssignmentDetailsCollection assignments, Hashtable changeDates)
        {
            SupportingAssignmentDetailsCollection supportingAssignments = new SupportingAssignmentDetailsCollection();
            SupportingAssignmentDetails supportAssignment = null;

             foreach (AssignmentDetails objAssignment in assignments)
                {
                    int wmSourceID = objAssignment.ID;
                    supportAssignment = new SupportingAssignmentDetails();
                    supportAssignment.Assignment = objAssignment;
                    //Set the SupportingDataChangeDate ... look up in hashtable using WMSourceID as the key
                    if (changeDates.ContainsKey(wmSourceID))
                    {
                        supportAssignment.SupportingDataChangeDate = DateTime.Parse(changeDates[wmSourceID].ToString());
                    }
                    else
                    {
                        supportAssignment.SupportingDataChangeDate = DateTime.MinValue;
                    }
                    supportingAssignments.Add(supportAssignment);
                }

            return supportingAssignments;
        }

        #endregion
    }

    #endregion

    #region Location

    public partial class Location
    {
        public Location Populate(DataRow locationRow)
        {
            Location objProperty = new Location();

            objProperty.ID = (int)locationRow["LocationID"];
            objProperty.PropertyReference = objProperty.ID;

            objProperty.XCoord = (int)decimal.Parse(locationRow["XCoord"].ToString());
            objProperty.YCoord = (int)decimal.Parse(locationRow["YCoord"].ToString());
            objProperty.PostCode = locationRow["PostCode"].ToString();
            if (!locationRow["HouseName"].Equals(DBNull.Value))
            {
                objProperty.HouseName = locationRow["HouseName"].ToString();
            }
            if (!locationRow["HouseNo"].Equals(DBNull.Value))
            {
                int parseResult = 0;
                if (int.TryParse(locationRow["HouseNo"].ToString(), out parseResult))
                {
                    objProperty.HouseNo = parseResult;
                }
                else
                {
                    objProperty.HouseName += locationRow["HouseNo"].ToString();
                }
            }
            objProperty.Street = locationRow["Street"].ToString();
            objProperty.Town = locationRow["Town"].ToString();
            if (!locationRow["District"].Equals(DBNull.Value))
            {
                objProperty.District = locationRow["District"].ToString();
            }
            if (!locationRow["SubBuilding"].Equals(DBNull.Value))
            {
                objProperty.SubBuilding = locationRow["SubBuilding"].ToString();
            }
            //------------------------------------------------------------------
            if (!locationRow["Directions"].Equals(DBNull.Value))
            {
                objProperty.Directions = locationRow["Directions"].ToString();
            }


            return objProperty;
        }

        public static List<Location> GetGeoDataForJobs(string[] sourceIDs)
        {
            List<Location> colLocations = new List<Location>();
            DataTable dtResults = null;
            DataAccess objADO = new DataAccess();
            DataAccess.ArrayListParameter objParameter = null;
            string strStoredProcedure = "selGeoDataForJobs";

            objADO = Domain.GetADOInstance();

            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);

            dtResults = objADO.GetDataSet(strStoredProcedure, objParameter, null).Tables[0];
            objADO = null;

            Location location = null;
            foreach (DataRow locationRow in dtResults.Rows)
            {
                location = new Location();

                location.ID = int.Parse(locationRow["WMSourceID"].ToString());
                location.PostCode = locationRow["PostCode"].ToString();

                if (!locationRow["Latitude"].Equals(DBNull.Value))
                {
                    location.Latitude = double.Parse(locationRow["Latitude"].ToString());
                }
                if (!locationRow["Longitude"].Equals(DBNull.Value))
                {
                    location.Longitude = double.Parse(locationRow["Longitude"].ToString());
                }

                if (!locationRow["Town"].Equals(DBNull.Value))
                {
                    location.Town = locationRow["Town"].ToString();
                }
                if (!locationRow["Street"].Equals(DBNull.Value))
                {
                    location.Street = locationRow["Street"].ToString();
                }
                if (!locationRow["HouseNo"].Equals(DBNull.Value))
                {
                    int parseResult = 0;
                    if (int.TryParse(locationRow["HouseNo"].ToString(), out parseResult))
                    {
                        location.HouseNo = parseResult;
                    }
                }

                colLocations.Add(location);
            }

            return colLocations;
        }
    }

    #endregion

    #region SchedulingPriorityCollection

    public partial class SchedulingPriorityCollection
    {
        public static BusinessObjects.WorkManagement.SchedulingPriorityCollection Populate(DataRow[] filteredRows)
        {
            BusinessObjects.WorkManagement.SchedulingPriorityCollection schedulingPriorities = null;

            // Scheduling Priority
            if (filteredRows.Length > 0)
            {
                schedulingPriorities = new SchedulingPriorityCollection();
                for (int intIndex = 0; intIndex < filteredRows.Length; intIndex++)
                {
                    schedulingPriorities.Add(new SchedulingPriority());
                    if (filteredRows[intIndex].Table.Columns["PriorityID"] != null)
                    {
                        schedulingPriorities[intIndex].ID = (int)filteredRows[intIndex]["PriorityID"];
                    }
                    else
                    {
                        schedulingPriorities[intIndex].ID = (int)filteredRows[intIndex]["SchedulingPriorityID"];
                    }
                    schedulingPriorities[intIndex].Description = filteredRows[intIndex]["PriorityDesc"].ToString();
                    schedulingPriorities[intIndex].Hours = (int)filteredRows[intIndex]["LeadTimeInHours"];
                    schedulingPriorities[intIndex].IsReactive = (bool)filteredRows[intIndex]["IsReactive"];
                }
            }

            return schedulingPriorities;
        }
    }

    #endregion

    #region SupportingDataRequestCollection class

    public partial class SupportingDataRequestCollection
    {
        public static SupportingDataRequestCollection Populate(DataTable collectionMembers)
        {
            SupportingDataRequestCollection colRequests = new SupportingDataRequestCollection();

            foreach (DataRow drMember in collectionMembers.Rows)
            {
                colRequests.Add((eWMSourceSystem)Enum.Parse(typeof(eWMSourceSystem), drMember["WMSourceSystem"].ToString()));
            }
            return colRequests;
        }
    }

    #endregion

    #region CommentAuditRecord class

    public partial class CommentAuditRecord
    {
        #region Public Methods

        public static CommentAuditRecord Populate(DataRow drMember)
        {
            CommentAuditRecord commentRecord = new CommentAuditRecord();
            commentRecord.ChangeDate = (DateTime)drMember["ChangeDate"];
            commentRecord.ChangeUser = drMember["ChangeUser"].ToString();
            commentRecord.IsEngineerComment = (bool)drMember["IsEngineerComment"];
            //CR : 2396 , IsCommentsCritical is used for filtering whether comments are critical / normal
            // Changed to IsCritical as per review comments
            commentRecord.IsCritical = (bool)drMember["IsCritical"];
            commentRecord.Text = drMember["CommentText"].ToString();
            if (!drMember["CommentType"].Equals(DBNull.Value))
            {
                commentRecord.Type = (eCommentType)Enum.Parse(typeof(eCommentType), drMember["CommentType"].ToString());
            }
            if (!drMember["SortExpression"].Equals(DBNull.Value))
            {
                commentRecord.SortExpression = drMember["SortExpression"].ToString();
            }
            return commentRecord;
        }

        #endregion
    }

    #endregion

    #region JobIncompleteOutcome class

    public partial class JobIncompleteOutcome
    {
        #region Public Methods

        public static JobIncompleteOutcome Populate(DataRow drMember)
        {
            JobIncompleteOutcome incompleteOutcome = new JobIncompleteOutcome();
            incompleteOutcome.IncompleteReason = drMember["IncompleteReason"].ToString();
            incompleteOutcome.StartDateTime = (DateTime)drMember["StartDateTime"];
            incompleteOutcome.EndDateTime = (DateTime)drMember["StopDateTime"];
            incompleteOutcome.UserID = drMember["UserID"].ToString();
            return incompleteOutcome;
        }

        #endregion
    }

    #endregion

    #region CommentAuditRecordCollection class

    public partial class CommentAuditRecordCollection
    {
        #region Public Methods

        public static CommentAuditRecordCollection FindByJob(int jobID, eWMSourceSystem sourceSystem)
        {
            DataAccess objADO = Domain.GetADOInstance();
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selJobComments";

            colParameters.Add(new SqlParameter("@WMSourceID", jobID));
            colParameters.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));

            dtResults = objADO.GetDataTable(strStoredProcedure, "Comments", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return Populate(dtResults);
        }

        /// <summary>
        /// Populate OH Reason with core attributes (ID, Code, & Description)
        /// Called by all Find overloads and from LoadJobFromWorkOrder in WIS
        /// </summary>
        /// <param name="dtResults"></param>
        /// <returns></returns>
        public static CommentAuditRecordCollection Populate(DataTable dtResults)
        {
            CommentAuditRecordCollection colMembers = null;
            CommentAuditRecord commentRecord = null;

            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                colMembers = new CommentAuditRecordCollection();
                foreach (DataRow drMember in dtResults.Rows)
                {
                    commentRecord = new CommentAuditRecord();
                    commentRecord.ChangeDate = (DateTime)drMember["ChangeDate"];
                    commentRecord.ChangeUser = drMember["ChangeUser"].ToString();
                    commentRecord.IsEngineerComment = (bool)drMember["IsEngineerComment"];
                    // -----------------------------------------------------
                    commentRecord.Text = drMember["CommentText"].ToString();
                    if (!drMember["CommentType"].Equals(DBNull.Value))
                    {
                        commentRecord.Type = (eCommentType)Enum.Parse(typeof(eCommentType), drMember["CommentType"].ToString());
                    }
                    if (!drMember["SortExpression"].Equals(DBNull.Value))
                    {
                        commentRecord.SortExpression = drMember["SortExpression"].ToString();
                    }
                    colMembers.Add(commentRecord);
                }
            }

            return colMembers;
        }

        public static CommentAuditRecordCollection Populate(DataRow[] drMembers)
        {
            CommentAuditRecordCollection colMembers = null;
            if (drMembers != null && drMembers.Length > 0)
            {
                colMembers = new CommentAuditRecordCollection();
                foreach (DataRow drMember in drMembers)
                {
                    CommentAuditRecord commentRecord = CommentAuditRecord.Populate(drMember);
                    colMembers.Add(commentRecord);
                }
            }
            return colMembers;
        }

        #endregion
    }

    #endregion

    #region JobIncompleteOutcomeCollection class

    public partial class JobIncompleteOutcomeCollection
    {
        #region Public Methods

        /// <summary>
        /// Populate Incomplete History
        /// </summary>
        /// <param name="dtResults"></param>
        /// <returns></returns>
        public static JobIncompleteOutcomeCollection Populate(DataTable dtResults)
        {
            JobIncompleteOutcomeCollection colMembers = null;
            JobIncompleteOutcome incompleteOutcome = null;

            if (dtResults!=null && dtResults.Rows.Count > 0)
            {
                colMembers = new JobIncompleteOutcomeCollection();
                foreach (DataRow drMember in dtResults.Rows)
                {
                    incompleteOutcome = new JobIncompleteOutcome();
                    incompleteOutcome.IncompleteReason = drMember["IncompleteReason"].ToString();
                    incompleteOutcome.StartDateTime = (DateTime)drMember["StartDateTime"];
                    incompleteOutcome.EndDateTime = (DateTime)drMember["StopDateTime"];
                    incompleteOutcome.UserID = drMember["UserID"].ToString();
                    colMembers.Add(incompleteOutcome);
                }
            }

            return colMembers;
        }


        public static JobIncompleteOutcomeCollection Populate(DataRow[] drMembers)
        {
            JobIncompleteOutcomeCollection colMembers = null;
            if (drMembers != null && drMembers.Length > 0)
            {
                colMembers = new JobIncompleteOutcomeCollection();
                foreach (DataRow drMember in drMembers)
                {
                    JobIncompleteOutcome incompleteOutcome = JobIncompleteOutcome.Populate(drMember);
                    colMembers.Add(incompleteOutcome);
                }
            }
            return colMembers;
        }

        #endregion
    }

    #endregion

    #region FileAssociationCollection class

    public partial class FileAssociationCollection
    {
        public static FileAssociationCollection LoadByUser(string userID)
        {
            FileAssociationCollection collection = null;
            DataTable dtData = null;
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selUserAssociatedFiles";

            colParameters.Add(new SqlParameter("@UserID", userID));
            dtData = objADO.GetDataTable(strStoredProcedure, "FileAssociations", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dtData != null && dtData.Rows.Count > 0)
            {
                collection = new FileAssociationCollection();
                FileAssociation fileMetadata = null;

                foreach (DataRow drMember in dtData.Rows)
                {
                    fileMetadata = new FileAssociation();
                    fileMetadata.Populate(drMember);
                    collection.Add(fileMetadata);
                }
            }
            return collection;
        }
    }

    #endregion

    #region ActivityReference class

    public partial class ActivityReference
    {
        #region Public Methods

        public static List<ActivityReference> FindByPrimaryAssetType(string primaryAssetTypeCode)
        {
            DataAccess objADO = Domain.GetADOInstance(Domain.eConnectionName.Metadata);
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selActivityReferencesByPrimaryAssetType";

            colParameters.Add(new SqlParameter("@PrimaryAssetTypeCode", primaryAssetTypeCode));
            dtResults = objADO.GetDataTable(strStoredProcedure, "References", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return Populate(dtResults);
        }

        private static List<ActivityReference> Populate(DataTable dtResults)
        {
            List<ActivityReference> colMembers = null;
            ActivityReference reference = null;

            if (dtResults != null && dtResults.Rows.Count > 0)
            {
                colMembers = new List<ActivityReference>();
                foreach (DataRow drMember in dtResults.Rows)
                {
                    reference = new ActivityReference();
                    reference.ID = (int)drMember["ActivityID"];
                    reference.Code = drMember["ActivityCode"].ToString();
                    reference.Description = drMember["ActivityDesc"].ToString();
                    reference.Path = drMember["Path"].ToString();
                    if (!drMember["DistrictGroup"].Equals(DBNull.Value))
                    {
                        reference.DistrictGroup = drMember["DistrictGroup"].ToString();
                    }
                    colMembers.Add(reference);
                }
            }

            return colMembers;
        }

        #endregion
    }

    #endregion

    #region MaterialCollection

    public partial class MaterialCollection
    {

        public static MaterialCollection FindByJob(int jobID)
        {
            DataAccess objADO = Domain.GetADOInstance();
            DataTable dtResults = null;
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selMaterialsRequested";

            colParameters.Add(new SqlParameter("@JobID", jobID));

            dtResults = objADO.GetDataTable(strStoredProcedure, "MaterialsRequested", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            return Populate(dtResults);
        }

        public static MaterialCollection Populate(DataTable dtResults)
        {
            MaterialCollection materials = null;
            bool isIDColumnPresent = false;

            if (dtResults.Rows.Count > 0)
            {
                isIDColumnPresent = dtResults.Columns.Contains("MaterialID");
                materials = new MaterialCollection();
                for (int intIndex = 0; intIndex < dtResults.Rows.Count; intIndex++)
                {
                    materials.Add(new Material());
                    materials[intIndex].Description = dtResults.Rows[intIndex]["MaterialDesc"].ToString();
                    materials[intIndex].Quantity = (int)dtResults.Rows[intIndex]["Quantity"];
                    if (isIDColumnPresent && !dtResults.Rows[intIndex]["MaterialID"].Equals(DBNull.Value))
                    {
                        materials[intIndex].ID = (int)dtResults.Rows[intIndex]["MaterialID"];
                    }
                }
            }

            return materials;
        }
    }

    #endregion

    #region Material

    public partial class Material
    {

        public static bool DeleteRequestedMaterial(int jobID, string description)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "delMaterialsRequested";

            colParameters.Add(new SqlParameter("@JobID", jobID));
            if (!string.IsNullOrEmpty(description))
            {
                colParameters.Add(new SqlParameter("@MaterialDesc", description));
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

    }

    #endregion

    #region AssignmentConstraints

    public partial class AssignmentConstraints
    {

        //public static bool SaveAssignmentConstraints(AssignmentConstraints assignmentConstraints)
        //{
        //    DataAccess objADO = new DataAccess();
        //    ArrayList colParameters = new ArrayList();
        //    DataAccess.ArrayListParameter arrayParameter = null;
        //    SqlParameter returnParameter = null;
        //    string strStoredProcedure = "insAssignmentContraints";
        //    int returnID = 0;

        //    //SQL SP Paremeters as per below...
        //    //=======================================
        //    //@AssignmentConstraintID	int = null, 
        //    //@WMSourceID int,
        //    //@InstanceNumber int,
        //    //@TimeWindow varchar(20)= null, 
        //    //@Duration int= null, 
        //    //@StartTime datetime= null, 
        //    //@EndTime datetime= null,
        //    //@SpecificStartTime datetime= null, 
        //    //@MustBeCompletedByTime datetime= null

        //    if (assignmentConstraints == null)
        //    {
        //        return false;
        //    }

        //    if (assignmentConstraints.ID > 0)
        //    {
        //        colParameters.Add(new SqlParameter("@AssignmentConstraintID", assignmentConstraints.ID));
        //    }

        //    colParameters.Add(new SqlParameter("@WMSourceID", assignmentConstraints.WMSourceID));
        //    colParameters.Add(new SqlParameter("@InstanceNumber", assignmentConstraints.Instance));
        //    colParameters.Add(new SqlParameter("@TimeWindow", assignmentConstraints.TimeWindow.ToString()));

        //    colParameters.Add(new SqlParameter("@Duration", assignmentConstraints.Duration));

        //    if (assignmentConstraints.StartTime != DateTime.MinValue)
        //    {
        //        colParameters.Add(new SqlParameter("@StartTime", assignmentConstraints.StartTime));
        //    }

        //    if (assignmentConstraints.EndTime != DateTime.MinValue)
        //    {
        //        colParameters.Add(new SqlParameter("@EndTime", assignmentConstraints.EndTime));
        //    }

        //    if (assignmentConstraints.SpecificStartTime != DateTime.MinValue)
        //    {
        //        colParameters.Add(new SqlParameter("@SpecificStartTime", assignmentConstraints.SpecificStartTime));
        //    }

        //    if (assignmentConstraints.MustBeCompletedByTime != DateTime.MinValue)
        //    {
        //        colParameters.Add(new SqlParameter("@MustBeCompletedByTime", assignmentConstraints.MustBeCompletedByTime));
        //    }

        //    colParameters.Add(new SqlParameter("@StartTime_IsUserSet", assignmentConstraints.IsUserSetStartDate));

        //    //Skills
        //    if (assignmentConstraints.Skills != null && assignmentConstraints.Skills.Count > 0)
        //    {
        //        string[] skillCodes = new string[assignmentConstraints.Skills.Count];
        //        for (int intIndex = 0; intIndex < assignmentConstraints.Skills.Count; intIndex++)
        //        {
        //            skillCodes[intIndex] = assignmentConstraints.Skills[intIndex].Code;
        //        }
        //        arrayParameter = new DataAccess.ArrayListParameter("SkillCodes", skillCodes);
        //    }

        //    returnParameter = new SqlParameter("RETURN_VALUE", SqlDbType.Int);
        //    returnParameter.Direction = ParameterDirection.ReturnValue;
        //    colParameters.Add(returnParameter);

        //    if (arrayParameter != null)
        //    {
        //        returnID = objADO.ExecuteSQL(strStoredProcedure, arrayParameter, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
        //    }
        //    else
        //    {
        //        returnID = objADO.ExecuteSQL(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
        //    }

        //    objADO = null;

        //    return (returnID > 0);
        //}

        public static AssignmentConstraints FindAssignmentConstraints(int WMSourceID, int InstanceNumber)
        {
            DataAccess objADO = new DataAccess();
            ArrayList colParameters = new ArrayList();
            string strStoredProcedure = "selAssignmentContraints";
            DataSet dsResults = null;
            AssignmentConstraints returnAssignmentConstraints = null;

            colParameters.Add(new SqlParameter("@WMSourceID", WMSourceID));
            colParameters.Add(new SqlParameter("@InstanceNumber", InstanceNumber));

            dsResults = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            objADO = null;

            if (dsResults != null)
            {
                returnAssignmentConstraints = PopulateAssignmentConstraints(dsResults);
            }

            return returnAssignmentConstraints;
        }

        public static List<AssignmentConstraints> FindAssignmentConstraints(int[] sourceIDs)
        {
            DataAccess objADO = new DataAccess();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;

            string strStoredProcedure = "selAssignmentContraintsFromSourceIDs";
            DataSet dsResults = null;

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);
            colParameters.Add(new SqlParameter("@InstanceNumber", 0));
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dsResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters);
            objADO = null;

            return PopulateAssignmentConstraintsList(dsResults);
        }

        public static List<AssignmentConstraints> FindAssignmentConstraints(string[] sourceIDs)
        {
            DataAccess objADO = new DataAccess();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;

            string strStoredProcedure = "selAssignmentContraintsFromSourceIDs";
            DataSet dsResults = null;

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", sourceIDs);
            colParameters.Add(new SqlParameter("@InstanceNumber", 0));
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dsResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters);
            objADO = null;

            return PopulateAssignmentConstraintsList(dsResults);
        }

        public static List<AssignmentConstraints> FindAssignmentConstraints(string userID, DateTime date)
        {
            return FindAssignmentConstraints(userID, date, -1);
        }

        public static List<AssignmentConstraints> FindAssignmentConstraints(string userID, DateTime date, int dayRange)
        {
            DataAccess objADO = new DataAccess();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();

            string strStoredProcedure = "selAssignmentContraintsForAssignedJobs";
            DataSet dsResults = null;

            // Assemble Parameters
            colParameters.Add(new SqlParameter("@UserID", userID));
            colParameters.Add(new SqlParameter("@DayRange", dayRange));
            if (date != DateTime.MinValue)
            {
                colParameters.Add(new SqlParameter("@Date", date));
            }
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dsResults = objADO.GetDataSet(strStoredProcedure, arrParameters);
            objADO = null;

            return PopulateAssignmentConstraintsList(dsResults);
        }

        public static AssignmentConstraints PopulateAssignmentConstraints(DataSet assignmentConstraintsData)
        {
            AssignmentConstraints assignmentConstraint = null;
            List<AssignmentConstraints> assignmentConstraints = PopulateAssignmentConstraintsList(assignmentConstraintsData);

            if (assignmentConstraints != null && assignmentConstraints.Count > 0)
            {
                assignmentConstraint = assignmentConstraints[0];
            }
            return assignmentConstraint;
        }

        public static AssignmentConstraints PopulateAssignmentConstraints(DataTable assignmentConstraintsData, DataTable assignmentSkillsData)
        {
            //[WMSourceID],
            //[InstanceNumber],
            //[AssignmentConstraintID],
            //[TimeWindow],
            //[Duration],
            //[StartTime],
            //[EndTime],
            //[SpecificStartTime],
            //[MustBeCompletedByTime]
            //[StartTime_IsUserSet]
            //[StartTime_IsUserSet]
            //[SkillCodes]

            AssignmentConstraints assignmentConstraints = new AssignmentConstraints();

            if (assignmentConstraintsData.Rows != null && assignmentConstraintsData.Rows.Count > 0)
            {
                DataRow drAssignmentConstraint = assignmentConstraintsData.Rows[0];
                if (drAssignmentConstraint != null)
                {
                    assignmentConstraints = new AssignmentConstraints();
                    assignmentConstraints.WMSourceID = int.Parse(drAssignmentConstraint["WMSourceID"].ToString());
                    assignmentConstraints.Instance = int.Parse(drAssignmentConstraint["InstanceNumber"].ToString());
                    assignmentConstraints.StartTime = drAssignmentConstraint["StartTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["StartTime"].ToString()) : DateTime.MinValue;
                    assignmentConstraints.EndTime = drAssignmentConstraint["EndTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["EndTime"].ToString()) : DateTime.MinValue;
                    assignmentConstraints.SpecificStartTime = drAssignmentConstraint["SpecificStartTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["SpecificStartTime"].ToString()) : DateTime.MinValue;
                    assignmentConstraints.MustBeCompletedByTime = drAssignmentConstraint["MustBeCompletedByTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["MustBeCompletedByTime"].ToString()) : DateTime.MinValue;
                    assignmentConstraints.Duration = int.Parse(drAssignmentConstraint["Duration"].ToString());
                    //Set Time Window to "None" if nothing is returned from the DB here
                    assignmentConstraints.TimeWindow = drAssignmentConstraint["TimeWindow"] != DBNull.Value ? (eTimeWindow)Enum.Parse(typeof(eTimeWindow), drAssignmentConstraint["TimeWindow"].ToString()) : eTimeWindow.None;
                    assignmentConstraints.IsUserSetStartDate = bool.Parse(drAssignmentConstraint["StartTime_IsUserSet"].ToString());
                    //Load up Skills data ... if we have any
                    if (assignmentSkillsData != null && assignmentSkillsData.Rows.Count > 0)
                    {
                        assignmentConstraints.Skills = PopulateSkillCollection(assignmentSkillsData);
                    }
                }
            }

            return assignmentConstraints;
        }

        public static List<AssignmentConstraints> PopulateAssignmentConstraintsList(DataSet assignmentConstraintsData)
        {
            List<AssignmentConstraints> lstAssignmentConstraints = new List<AssignmentConstraints>();
            AssignmentConstraints assignmentConstraints = null;
            DataRow[] childRows = null;
            string filterExpression = string.Empty;
            bool areSkillsPresent = false;

            if (assignmentConstraintsData != null)
            {
                if ( assignmentConstraintsData.Tables[0].Rows != null && assignmentConstraintsData.Tables[0].Rows.Count > 0)
                {
                    areSkillsPresent = (assignmentConstraintsData.Tables.Count > 1);

                    for (int i = 0; i < assignmentConstraintsData.Tables[0].Rows.Count; i++)
                    {
                        DataRow drAssignmentConstraint = assignmentConstraintsData.Tables[0].Rows[i];
                        if (drAssignmentConstraint != null)
                        {
                            assignmentConstraints = new AssignmentConstraints();

                            assignmentConstraints.WMSourceID = int.Parse(drAssignmentConstraint["WMSourceID"].ToString());
                            assignmentConstraints.Instance = int.Parse(drAssignmentConstraint["InstanceNumber"].ToString());
                            assignmentConstraints.StartTime = drAssignmentConstraint["StartTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["StartTime"].ToString()) : DateTime.MinValue;
                            assignmentConstraints.EndTime = drAssignmentConstraint["EndTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["EndTime"].ToString()) : DateTime.MinValue;
                            assignmentConstraints.SpecificStartTime = drAssignmentConstraint["SpecificStartTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["SpecificStartTime"].ToString()) : DateTime.MinValue;
                            assignmentConstraints.MustBeCompletedByTime = drAssignmentConstraint["MustBeCompletedByTime"] != DBNull.Value ? DateTime.Parse(drAssignmentConstraint["MustBeCompletedByTime"].ToString()) : DateTime.MinValue;
                            assignmentConstraints.Duration = int.Parse(drAssignmentConstraint["Duration"].ToString());
                            //Set Time Window to "None" if nothing is returned from the DB here
                            assignmentConstraints.TimeWindow = drAssignmentConstraint["TimeWindow"] != DBNull.Value ? (eTimeWindow)Enum.Parse(typeof(eTimeWindow), drAssignmentConstraint["TimeWindow"].ToString()) : eTimeWindow.None;
                            assignmentConstraints.IsUserSetStartDate = bool.Parse(drAssignmentConstraint["StartTime_IsUserSet"].ToString());

                            lstAssignmentConstraints.Add(assignmentConstraints);
                        }
                        if (areSkillsPresent)
                        {
                            filterExpression = string.Format("WMSourceID={0}", assignmentConstraints.WMSourceID);
                            childRows = assignmentConstraintsData.Tables[1].Select(filterExpression);
                            // Skills
                            if (childRows != null && childRows.Length > 0)
                            {
                                assignmentConstraints.Skills = new SkillCollection();
                                for (int index = 0; index < childRows.Length; index++)
                                {
                                    assignmentConstraints.Skills.Add(new Skill());
                                    assignmentConstraints.Skills[index].Code = childRows[index]["SkillCode"].ToString();
                                    assignmentConstraints.Skills[index].Description = childRows[index]["SkillDesc"].ToString();
                                    // assignmentConstraints.Skills[index].SourceSystems = childRows[index]["SourceSystems"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            return lstAssignmentConstraints;
        }

        private static SkillCollection PopulateSkillCollection(DataTable assignmentSkillsData)
        {
            SkillCollection skills = null;
            if (assignmentSkillsData != null && assignmentSkillsData.Rows.Count > 0)
            {
                skills = new SkillCollection();
                for (int index = 0; index < assignmentSkillsData.Rows.Count; index++)
                {
                    skills.Add(new Skill());
                    skills[index].Code = assignmentSkillsData.Rows[index]["SkillCode"].ToString();
                    skills[index].Description = assignmentSkillsData.Rows[index]["SkillDesc"].ToString();
                }
            }

            return skills;
        }
    }

    #endregion

    #region Travel

    public partial class Travel
    {
        public static DataTable GetTravelRelations(int[] jobIDs)
        {
            DataAccess objADO = new DataAccess();
            System.Data.SqlClient.SqlParameter[] arrParameters;
            System.Collections.ArrayList colParameters = new System.Collections.ArrayList();
            DataAccess.ArrayListParameter objParameter = null;

            string strStoredProcedure = "selTravelRelations";
            DataSet dsResults = null;

            // Assemble Parameters
            objParameter = new DataAccess.ArrayListParameter("WMSourceIDs", jobIDs);
            arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));

            dsResults = objADO.GetDataSet(strStoredProcedure, objParameter, arrParameters);
            objADO = null;

            return dsResults.Tables[0];
        }
    }

    #endregion
}
