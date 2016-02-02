using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;

using FinalBuild;

namespace BusinessObjects.WorkManagement
{
    public sealed class Domain
    {
        public enum eConnectionName
        {
			FieldData,
            Metadata,
            Corporate
        }

        public static FinalBuild.DataAccess GetADOInstance()
        {
            FinalBuild.DataAccess objADO = null;
            string strAppSettingKey = "sqlConn";

            if (ConfigurationManager.AppSettings[strAppSettingKey] != null)
            {
                objADO = new FinalBuild.DataAccess(ConfigurationManager.AppSettings[strAppSettingKey]);
            }

            return objADO;
        }

        public static FinalBuild.DataAccess GetADOInstance(eConnectionName connectionName)
        {
            FinalBuild.DataAccess objADO = null;
            string strAppSettingKey = "sql" + connectionName.ToString();

            if (ConfigurationManager.AppSettings[strAppSettingKey] != null)
            {
                objADO = new FinalBuild.DataAccess(ConfigurationManager.AppSettings[strAppSettingKey]);
            }

            return objADO;
        }

        public static string GetEmployeeLogin(string employeeNo)
        {
            DataAccess objADO = new DataAccess();
            SqlParameter[] arrParameters = new SqlParameter[2];
            SqlParameter objOutputParameter = null;
            string strNTLogin = employeeNo;

            arrParameters[0] = new SqlParameter("@EmpNo", employeeNo);
            objOutputParameter = new SqlParameter("@NTLogin", SqlDbType.VarChar, 50);
            objOutputParameter.Direction = ParameterDirection.Output;
            arrParameters[1] = objOutputParameter;

            objADO.ExecuteSQL("selUserLogin", arrParameters);
            if (objOutputParameter.Value != null)
            {
                strNTLogin = objOutputParameter.Value.ToString();
            }
            objADO = null;

            return strNTLogin;
        }

        /// <summary>
        /// This is used in creating the Reader Name for Alerts
        /// </summary>
        /// <returns></returns>
        public static string GetEmployeeName(string employeeNo)
        {
            DataAccess objADO = new DataAccess();
            SqlParameter[] arrParameters = new SqlParameter[2];
            SqlParameter objOutputParameter = null;
            string strEmployeeName = employeeNo;

            arrParameters[0] = new SqlParameter("@EmpNo", employeeNo);
            objOutputParameter = new SqlParameter("@EmployeeName", SqlDbType.VarChar, 50);
            objOutputParameter.Direction = ParameterDirection.Output;
            arrParameters[1] = objOutputParameter;

            objADO.ExecuteSQL("selEmployeeName", arrParameters);
            if (objOutputParameter.Value != null)
            {
                strEmployeeName = objOutputParameter.Value.ToString();
            }
            objADO = null;

            return strEmployeeName;
        }
    }
}
