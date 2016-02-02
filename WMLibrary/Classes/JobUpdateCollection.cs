#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using BusinessObjects;
using FinalBuild;

#endregion

namespace BusinessObjects.WorkManagement
{
	#region JobUpdateCollection class

	public partial class JobUpdateCollection
	{
		#region Internal Methods

		#region Static Internal Methods

		#endregion

		#region Non-Static Internal Methods

		#endregion

		#endregion

		#region Public Methods

		#region Static Public Methods

        /// <summary>
        /// Get HashValue saved for each JobUpdate received against a particular Job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public static string[] GetPreviousJobUpdateHashValues(int jobID)
        {
            string[] hashValues = null;
            DataTable dtJobUpdates = JobUpdateDataTable(jobID, eWMSourceSystem.Unspecified);

            if (dtJobUpdates != null && dtJobUpdates.Rows.Count > 0)
            {
                hashValues = new string[dtJobUpdates.Rows.Count];
                for(int index = 0; index < dtJobUpdates.Rows.Count;index++)
                {
                    if (!dtJobUpdates.Rows[index]["HashValue"].Equals(DBNull.Value))
                    {
                        hashValues[index] = dtJobUpdates.Rows[index]["HashValue"].ToString();
                    }
                    else
                    {
                        hashValues[index] = string.Empty;
                    }
                }
            }

            return hashValues;
        }

		public static DataTable JobUpdateDataTable(int jobID, eWMSourceSystem sourceSystem)
		{
			DataSet results = null;
			DataAccess dataAccess = new DataAccess();
			System.Collections.ArrayList arrayList = new System.Collections.ArrayList();
			System.Data.SqlClient.SqlParameter[] parameters = null;
			string storedProcedure = "selJobUpdatesSerialized";
			dataAccess = Domain.GetADOInstance();

			arrayList.Add(new SqlParameter("@WMSourceSystem", sourceSystem.ToString()));
			arrayList.Add(new SqlParameter("@WMSourceID", jobID));
			parameters = (SqlParameter[])arrayList.ToArray(typeof(SqlParameter));

			results = dataAccess.GetDataSet(storedProcedure, parameters);
			if (results != null && results.Tables.Count > 0 && results.Tables[0] != null && results.Tables[0].Rows.Count > 0)
			{
				dataAccess = null;
				return results.Tables[0];
			}
			dataAccess = null;
			return null;
		}

		#endregion

		#region Non-Static Public Methods

		/// <summary>
		/// Should be overriden in any inheriting class to populate instance members from a serialized Xml Document
		/// </summary>
		/// <param name="xmlDocument"></param>
		public virtual void AddFromSerializedObjectInstance(System.Xml.XmlDocument xmlDocument, int archiveID)
		{
			throw new InvalidOperationException("The AddFromSerializedObjectInstance method should not be called directly from its base JobUpdateCollection class. This method should me overridden or re-implemented using the New keywork in the inheriting class.");
		}

		#endregion

		#endregion
	}

	#endregion
}
