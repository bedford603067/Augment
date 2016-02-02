using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Collections.Generic;
using System.Collections;
using System.Data.SqlClient;

using FinalBuild;

namespace BusinessObjects
{
	public class ReportCollection : System.Collections.Generic.List<Report>
	{
	}

	public class Report
	{
		#region Private Fields

		private QueryCollection mColQueries = null;

		#endregion

		#region Public Properties And Fields

		public string Name;
        public string Title;
        public int ID;

		public QueryCollection Queries
		{
			get
			{
				if (mColQueries == null)
				{
					mColQueries = new QueryCollection();
				}
				return mColQueries;
			}
		}

		#endregion

		#region Public Methods

		public Report()
		{
		}

		#endregion
	}

	/// <summary>
	/// Summary description for Query
	/// </summary>
	public class Query
	{
		#region Private Fields

		private QueryParameterCollection mColParameters = null;

		#endregion

		#region Public Properties And Fields

		public string Name;

		public QueryParameterCollection Parameters
		{
			get
			{
				if (mColParameters == null)
				{
					mColParameters = new QueryParameterCollection();
				}
				return mColParameters;
			}
		}

        public string RdlcFile;
        public string SchemaFile;

		#endregion

		#region Public Methods

		public Query()
		{
		}

        public DataSet Execute()
        {
            return Execute(null); // Will use default (sqlConn appSetting)
        }

		public DataSet Execute(string connectionString)
		{
			DataSet dsResults = null;
            DataAccess objADO = null; new DataAccess();
			ArrayList colParameters = new ArrayList();
			string strStoredProcedure = Name.ToString();

            if (!string.IsNullOrEmpty(connectionString))
            {
                objADO = new DataAccess(connectionString);
            }
            else
            {
                objADO = null; new DataAccess();
            }

			foreach (QueryParameter objParameter in this.Parameters)
			{
                if (objParameter.Value != null)
                {
                    colParameters.Add(new SqlParameter("@" + objParameter.Name, objParameter.Value));
                }
			}
			dsResults = objADO.GetDataSet(strStoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

			return dsResults;
		}

        public void CopyParameterValuesFromQuery(Query parentQuery)
        {
            for (int intParentIndex = 0; intParentIndex < parentQuery.Parameters.Count; intParentIndex++)
            {
                for (int intIndex = 0; intIndex < this.Parameters.Count; intIndex++)
                {
                    if (parentQuery.Parameters[intParentIndex].Name.ToUpper() == this.Parameters[intIndex].Name.ToUpper())
                    {
                        this.Parameters[intIndex].Value = parentQuery.Parameters[intParentIndex].Value;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Public Classes

        public class QueryParameter
		{
			#region Public Properties And Fields

			public string Name;
			public SqlDbType Type = SqlDbType.Int;
            public int Length = 50;

            private object mobjValue;

            public object Value
            {
                get
                {
                    return mobjValue;
                }
                set
                {
                    if (value!=null && value.GetType() == typeof(System.Xml.XmlNode[]))
                    {
                        // Deserializer does not know the Type so assumes XmlNode[]
                        mobjValue = (object)((System.Xml.XmlNode[])value)[0].Value;
                    }
                    else
                    {
                        // Standard case
                        mobjValue = value;
                    }
                }
            }

			#endregion
		}

		public class QueryParameterCollection : System.Collections.Generic.List<QueryParameter>
		{
		}

		#endregion
	}

	public class QueryCollection : System.Collections.Generic.List<Query>
	{
	}
}




