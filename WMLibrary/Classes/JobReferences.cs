using System;
using System.Collections.Generic;
using System.Text;

using System.Data;

namespace BusinessObjects.WorkManagement
{
    public abstract class JobReferencesContainer
    {
        #region Auto Generated Code

        /// <remarks/>
        private JobReferenceCollection mColNew;

        /// <remarks/>
        private JobReferenceCollection mColModified;

        /// <remarks/>
		private JobReferenceCollection mColRemoved;

		/// <remarks/>
		private SupportingDataRequestCollection mColSupportingDataRequests;

        /// <summary>
        /// New
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public JobReferenceCollection New
        {
            get
            {
                return this.mColNew;
            }
            set
            {
                this.mColNew = value;
            }
        }

        /// <summary>
        /// Modified
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public JobReferenceCollection Modified
        {
            get
            {
                return this.mColModified;
            }
            set
            {
                this.mColModified = value;
            }
        }

        /// <summary>
        /// Removed
        /// </summary>
        [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
        public JobReferenceCollection Removed
        {
            get
            {
                return this.mColRemoved;
            }
            set
            {
                this.mColRemoved = value;
            }
		}

		/// <summary>
		/// Removed
		/// </summary>
		[System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)]
		public SupportingDataRequestCollection SupportingData
		{
			get
			{
				return this.mColSupportingDataRequests;
			}
			set
			{
				this.mColSupportingDataRequests = value;
			}
		}
        #endregion

        #region Public Methods

        public void Populate(DataSet jobReferences)
        {
            // Note. Ordering of return sets to match FieldData SP selUserWork

            if (jobReferences != null && jobReferences.Tables.Count > 0)
            {
                // New
                if (jobReferences.Tables[0].Rows.Count > 0)
                {
                    mColNew = new JobReferenceCollection();
                    foreach (DataRow drJob in jobReferences.Tables[0].Rows)
                    {
                        mColNew.Add(JobReference.Create(drJob,true));
                    }
                }
                // Removed
                if (jobReferences.Tables.Count > 1 && jobReferences.Tables[1].Rows.Count > 0)
                {
                    mColRemoved = new JobReferenceCollection();
                    foreach (DataRow drJob in jobReferences.Tables[1].Rows)
                    {
                        mColRemoved.Add(JobReference.Create(drJob, true)); 
                    }
                }
                // Modified
                if (jobReferences.Tables.Count > 2 && jobReferences.Tables[2].Rows.Count > 0)
                {
                    mColModified = new JobReferenceCollection();
                    foreach (DataRow drJob in jobReferences.Tables[2].Rows)
                    {
                        mColModified.Add(JobReference.Create(drJob, true));
                    }
				}
				// Supporing Data
				if (jobReferences.Tables.Count > 3 && jobReferences.Tables[3].Rows.Count > 0)
				{
					mColSupportingDataRequests = SupportingDataRequestCollection.Populate(jobReferences.Tables[3]);
				}
				
            }
        }

        #endregion
    }

    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://FinalBuild.co.uk/WorkManagement/BusinessObjects.xsd")]
    [System.Xml.Serialization.XmlRootAttribute("ActiveReferences", Namespace = "http://FinalBuild.co.uk/WorkManagement/BusinessObjects.xsd", IsNullable = false)]
    public partial class ActiveJobReferences:JobReferencesContainer
    {
    }
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://FinalBuild.co.uk/WorkManagement/BusinessObjects.xsd")]
    [System.Xml.Serialization.XmlRootAttribute("ScheduledReferences", Namespace = "http://FinalBuild.co.uk/WorkManagement/BusinessObjects.xsd", IsNullable = false)]
    public partial class ScheduledJobReferences : JobReferencesContainer
    {
    }

    public partial class JobReferenceCollection
    {
        /// <summary>
        /// Get list of unique SourceSystem values present in the collection
        /// </summary>
        /// <returns></returns>
		public BusinessObjects.WorkManagement.eWMSourceSystem[] GetSourceSystems()
        {
            BusinessObjects.WorkManagement.eWMSourceSystem[] arrPropertyValues = null;
            System.Collections.ArrayList colPropertyValues = new System.Collections.ArrayList();

            foreach (JobReference objReference in this)
            {
                if (colPropertyValues.IndexOf(objReference.SourceSystem) < 0)
                {
                    colPropertyValues.Add(objReference.SourceSystem);
                }
            }

            if (colPropertyValues.Count > 0)
            {
                arrPropertyValues = (BusinessObjects.WorkManagement.eWMSourceSystem[])colPropertyValues.ToArray(typeof(BusinessObjects.WorkManagement.eWMSourceSystem));
            }

            return arrPropertyValues;
        }

        /// <summary>
        /// Get list of SourceID values present in the collection
        /// </summary>
        /// <returns></returns>
		public string[] GetSourceIDs(eWMSourceSystem sourceSystem)
        {
            string[] arrPropertyValues = null;
            System.Collections.ArrayList colPropertyValues = new System.Collections.ArrayList();

            foreach (JobReference objReference in this)
            {
                if (objReference.SourceSystem == sourceSystem)
                {
                    colPropertyValues.Add(objReference.SourceID);
                }
            }

            if (colPropertyValues.Count > 0)
            {
                arrPropertyValues = (string[])colPropertyValues.ToArray(typeof(string));
            }

            return arrPropertyValues;
        }

    //    /// <summary>
    //    /// Get list of unique Region values present in the collection
    //    /// </summary>
    //    /// <returns></returns>
    //    public string[] GetRegions()
    //    {
    //        string[] arrPropertyValues = null;
    //        System.Collections.ArrayList colPropertyValues = new System.Collections.ArrayList();

    //        foreach (JobReference objReference in this)
    //        {
    //            if (colPropertyValues.IndexOf(objReference.Region) < 0)
    //            {
    //                colPropertyValues.Add(objReference.Region);
    //            }
    //        }

    //        if (colPropertyValues.Count > 0)
    //        {
    //            arrPropertyValues = (string[])colPropertyValues.ToArray(typeof(string));
    //        }

    //        return arrPropertyValues;
    //    }
    }
}
