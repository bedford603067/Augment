using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Xml;

namespace FinalBuild
{
    /// <summary>
    /// Summary description for DataAccess.
    /// </summary>
    public class DataAccess
    {
        #region Private Fields

        /// <summary>
        /// Connection object for SQL-Server database
        /// </summary>
        private SqlConnection mobjSQLConnection = new SqlConnection();
        /// <summary>
        /// Connection object for Schema queries
        /// </summary>
        private OleDbConnection mobjOLEConnection = null;

        /// <summary>
        /// Connection string for OleDB calls
        /// </summary>
        private string mstrADOConnectionString = "";

        /// <summary>
        /// SQL Transaction object
        /// </summary>
        private SqlTransaction mobjSQLTransaction = null;

        /// <summary>
        /// Fields that support creation of OLE Connection String
        /// </summary>
        private string mstrServer = "";
        private string mstrDatabase = "";

        /// <summary>
        /// Name of Key in App Settings of configuration file that contains
        /// the database connection string.
        /// </summary>
        /// <remarks>
        /// Change this value if this class is used in other projects/solutions
        /// </remarks>
        private string mstrConnectionConfigKey = "sqlConn";

        /// <summary>
        /// To support transaction across calls leave the connection string open.
        /// Currently only affects ExecuteSQL.
        /// Default is false;        
        /// </summary>
        private bool _leaveConnectionOpen = false;

        private bool _autoRetry = false;

        private int _autoRetryMaximumTimes = 5;

        private int _autoRetrySleep = 10000;    //  Milliseconds

        private bool _logSQLStatement = false;

        private bool _useTransaction = false;

        private IsolationLevel _transactionIsolationLevel =  IsolationLevel.ReadCommitted;

        #endregion

        #region Constructors

        /// <summary>
        /// Sets ConnectionString for SQLClient and OleDb calls during construction
        /// </summary>
        public DataAccess(string dataSource, string initialCatalog)
        {
            mstrServer = dataSource;
            mstrDatabase = initialCatalog;
            // SQLClient Connection
            mobjSQLConnection.ConnectionString = GenerateConnectionString(dataSource, initialCatalog, false);
            // OleDB Connection String
            mstrADOConnectionString = GenerateConnectionString(dataSource, initialCatalog, true);
        }

        /// <summary>
        /// Sets ConnectionString for SQLClient calls during construction
        /// </summary>
        public DataAccess(string connectionString)
        {
            try
            {
                mobjSQLConnection.ConnectionString = connectionString;
                mstrADOConnectionString = mobjSQLConnection.ConnectionString;
            }
            catch (Exception ex)
            {
                // Unable to read connection string
                throw ex;
            }
        }

        /// <summary>
        /// Sets ConnectionString for SQLClient calls during construction
        /// </summary>
        public DataAccess(string connectionString, bool isConnectionKey)
        {
            try
            {
                if (isConnectionKey)
                {
                    mstrConnectionConfigKey = connectionString;
                    mobjSQLConnection.ConnectionString = ConnectionString;
                }
                else
                {
                    mobjSQLConnection.ConnectionString = connectionString;
                    mstrADOConnectionString = mobjSQLConnection.ConnectionString;
                }
            }
            catch (Exception ex)
            {
                // Unable to read connection string
                throw ex;
            }
        }

        /// <summary>
        /// Default constructor for the class
        /// Sets ConnectionString for SQLClient calls during construction
        /// </summary>
        public DataAccess()
        {
            try
            {
                mobjSQLConnection.ConnectionString = ConnectionString;
            }
            catch (Exception ex)
            {
                // Unable to read connection string
                throw ex;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// To support transaction across calls leave the connection string open.
        /// Currently only affects ExecuteSQL.
        /// Default is false;        
        /// </summary>
        public bool LeaveConnectionOpen
        {
            get { return _leaveConnectionOpen; }
            set { _leaveConnectionOpen = value; }
        }

        public string ConnectionConfigKey
        {
            get { return this.mstrConnectionConfigKey; }
            set { this.mstrConnectionConfigKey = value; }
        }


        /// <summary>
        /// Gets the database connection string from the web configuration file
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings[mstrConnectionConfigKey];
            }
        }

        public string DataSource
        {
            get
            {
                if (mstrServer != null && mstrServer != "")
                {
                    return mstrServer;
                }
                else
                {
                    // Need to decode the ConnectionString
                    return "";
                }
            }
        }

        public string InitialCatalog
        {
            get
            {
                if (mstrDatabase != null && mstrDatabase != "")
                {
                    return mstrDatabase;
                }
                else
                {
                    // Need to decode the ConnectionString
                    return "";
                }
            }
        }

        public string UserID
        {
            get
            {
                return "";
                // Need to decode the ConnectionString
            }
        }

        public string Password
        {
            get
            {
                return "";
                // Need to decode the ConnectionString
            }
        }

        public bool AutoRetry
        {
            get { return _autoRetry; }
            set { _autoRetry = value; }
        }

        public int AutoRetryMaximumTimes
        {
            get { return _autoRetryMaximumTimes; }
            set { _autoRetryMaximumTimes = value; }
        }

        public int AutoRetrySleep
        {
            get { return _autoRetrySleep; }
            set { _autoRetrySleep = value; }
        }

        public bool LogSQLStatement
        {
            get { return _logSQLStatement; }
            set { _logSQLStatement = value; }
        }

        public bool UseTransaction
        {
            get { return _useTransaction; }
            set { _useTransaction = value; }
        }

        public IsolationLevel TransactionisolationLevel
        {
            get { return _transactionIsolationLevel; }
            set { _transactionIsolationLevel = value; }
        }

        #endregion

        #region Enumerations

        public enum eDBObjectType
        { Table = 0, View = 1, Procedure = 2 }

        #endregion

        #region Public Events

        public delegate void ADOCallRetriedHandler(string storedProcedure, int retryAttempt, Exception causeOfRetry);
        public event ADOCallRetriedHandler ADOCallRetried;

        #endregion

        #region Public Methods

        #region Data Retrieval

        /// <summary>
        /// Fill a dataset from the database using the specified Stored Procedure
        /// statement and return the results
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns></returns>
        public DataSet GetDataSet(string storedProcedure, SqlParameter[] paramList, int commandTimeout)
        {
            return GetDataSet(storedProcedure, paramList, false, commandTimeout);
        }

        /// <summary>
        /// Fill a dataset from the database using the specified Stored Procedure
        /// statement and return the results
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns></returns>
        public DataSet GetDataSet(string storedProcedure, SqlParameter[] paramList, bool includeSchemaInfo)
        {
            return GetDataSet(storedProcedure, paramList, includeSchemaInfo, -1);   //  Leave as default commandTimeout
        }

        /// <summary>
        /// Fill a dataset from the database using the specified Stored Procedure
        /// statement and return the results
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns></returns>
        public DataSet GetDataSet(string storedProcedure, SqlParameter[] paramList)
        {
            return GetDataSet(storedProcedure, paramList, false, -1);   //  Leave as default commandTimeout
        }

        /// <summary>
        /// Fill a dataset from the database using the specified Stored Procedure
        /// statement and return the results
        /// Takes single ArrayListParameter alongside standard SqlParameters
        /// Converts ArrayListParameter to SqlParameter, runs multiple times if required
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="arrayParameter">Instance of the ArrayParameter class</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns>DataSet of results after executing SQL statement</returns>
        public DataSet GetDataSet(string storedProcedure, ArrayListParameter arrayParameter, SqlParameter[] paramList)
        {
            DataSet dsData = new DataSet();
            ArrayList colParameters = null;
            DataSet dsIntermediaryResult = null;

            try
            {
                // Process ArrayListParameter
                SqlParameter[] arrArrayParameterInstances = arrayParameter.ConvertToParameterInstances();
                if (arrArrayParameterInstances.Length == 1)
                {
                    // Simple case, send converted Array as parameter to standard method
                    colParameters = new ArrayList();
                    colParameters.Add(arrArrayParameterInstances[0]);
                    if (paramList != null)
                    {
                        foreach (SqlParameter objParameter in paramList)
                        {
                            colParameters.Add(objParameter);
                        }
                    }
                    return GetDataSet(storedProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                }
                else
                {
                    // Complex case, submit the query multiple times and merge results
                    colParameters = new ArrayList();
                    foreach (SqlParameter objParameterInstance in arrArrayParameterInstances)
                    {
                        colParameters.Clear();
                        colParameters.Add(objParameterInstance);
                        if (paramList != null)
                        {
                            foreach (SqlParameter objParameter in paramList)
                            {
                                colParameters.Add(objParameter);
                            }
                        }
                        dsIntermediaryResult = GetDataSet(storedProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                        dsData.Merge(dsIntermediaryResult);
                    }
                }
            }
            catch (SqlException se)
            {
                dsData = null;
                throw se;
            }
            catch (Exception ex)
            {
                dsData = null;
                throw ex;
            }

            if (dsData == null || dsData.Tables.Count == 0)
            {
                return null;
            }
            else
            {
                return dsData;
            }
        }

        /// <summary>
        /// Fill a dataset from the database using the specified Stored Procedure
        /// statement and return the results
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <param name="includeSchemaInfo">Include Schema info e.g Column MaxLength</param>
        /// <returns>DataSet of results after executing SQL statement</returns>
        public DataSet GetDataSet(string storedProcedure, SqlParameter[] paramList, bool includeSchemaInfo, int commandTimeout)
        {
            DataSet dsData = new DataSet();
            SqlDataAdapter daDataAdapter = new SqlDataAdapter();
            SqlCommand dcSQLCommand = new SqlCommand();

            try
            {
                dcSQLCommand.CommandText = storedProcedure;
                dcSQLCommand.CommandType = CommandType.StoredProcedure;

                if (commandTimeout > -1)
                {
                    dcSQLCommand.CommandTimeout = commandTimeout;
                }

                // Add stored procedure parameters to command
                if (paramList != null)
                {
                    foreach (SqlParameter param in paramList)
                    {
                        if (param != null)
                        {
                            dcSQLCommand.Parameters.Add(param);
                        }
                    }
                }

                daDataAdapter.SelectCommand = dcSQLCommand;
                dcSQLCommand.Connection = mobjSQLConnection;
                dcSQLCommand.Connection.Open();

                if (includeSchemaInfo)
                {
                    daDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                }
                
                daDataAdapter.Fill(dsData);
            }
            catch (XmlException excX)
            {
                dsData = null;
                int retryCounter = 0;

                if (excX.Message.Contains("Unexpected end of file") ||
                    excX.Message.Contains("Unexpected BinaryXml token"))
                {
                    while (retryCounter < 4)
                    {
                        try
                        {
                            retryCounter += 1;
                            if (ADOCallRetried != null)
                            {
                                ADOCallRetried(storedProcedure, retryCounter, excX);
                            }

                            if (dcSQLCommand.Connection.State != ConnectionState.Open)
                            {
                                dcSQLCommand.Connection.Open();
                            }
                            dsData = new DataSet();
                            daDataAdapter.Fill(dsData);

                            break; // Retry was successful if code reaches here
                        }
                        catch (Exception excRetry)
                        {
                            if (retryCounter >= 3)
                            {
                                throw new Exception(string.Format("{0} ({1}) . Method call was retried {2} times without success. SP {3}", "Xml Exception, unexpected BinaryXml or unexpected end of file", excRetry.Message, retryCounter, storedProcedure));
                            }
                        }
                    }
                }
                else
                {
                    throw excX;
                }
            }
            catch (SqlException excS)
            {
                dsData = null;
                throw excS;
            }
            catch (Exception excE)
            {
                dsData = null;
                throw excE;
            }
            finally
            {
                dcSQLCommand.Connection.Close();
                daDataAdapter.Dispose();
                dcSQLCommand.Dispose();
            }

            if (dsData == null || dsData.Tables.Count == 0)
            {
                return null;
            }
            else
            {
                return dsData;
            }
        }

        /// <summary>
        /// Fill a datatable from the database using the specified Stored Procedure
        /// statement and return the results
        /// </summary>
        /// <param name="SQLStatement">Name of stored procedure to execute</param>
        /// <param name="tableName">Name of dataset table</param>
        /// <param name="command">SQLCommand object </param>
        /// <returns>DataTable of results after executing SQL statement</returns>
        public DataTable GetDataTable(string storedProcedure, string tableName)
        {
            DataTable dtTable = new DataTable(tableName);
            SqlDataAdapter daDataAdapter = new SqlDataAdapter();
            SqlCommand dcSQLCommand = new SqlCommand();

            try
            {
                dcSQLCommand.CommandText = storedProcedure;
                dcSQLCommand.CommandType = CommandType.StoredProcedure;
                daDataAdapter.SelectCommand = dcSQLCommand;

                dcSQLCommand.Connection = mobjSQLConnection;
                dcSQLCommand.Connection.Open();
                daDataAdapter.Fill(dtTable);
            }
            catch (SqlException se)
            {
                dtTable = null;
                throw se;
            }
            finally
            {
                dcSQLCommand.Connection.Close();
                daDataAdapter.Dispose();
                dcSQLCommand.Dispose();
            }

            return dtTable;
        }

        /// <summary>
        /// Fill a datatable from the database using the specified SELECT SQL
        /// statement and its set of parameters. Return the results from the SQL.
        /// </summary>
        /// <remarks>
        /// This method assumes that a SQL statement is a Stored Procedure
        /// </remarks>
        /// <param name="SQLStatement">Name of stored procedure or SQL statement to execute</param>
        /// <param name="tableName">Name of dataset table</param>
        /// <param name="paramList">Array of parameters to apply to stored procedure</param>
        /// <returns>DataTable of results after executing SQL statement</returns>
        public DataTable GetDataTable(string storedProcedure, string tableName, SqlParameter[] paramList)
        {
            DataTable dtTable = new DataTable(tableName);
            SqlDataAdapter daDataAdapter = new SqlDataAdapter();
            SqlCommand dcSQLCommand = new SqlCommand();

            try
            {
                dcSQLCommand.CommandText = storedProcedure;
                dcSQLCommand.CommandType = CommandType.StoredProcedure;

                // Add stored procedure parameters to command
                if (paramList != null)
                {
                    foreach (SqlParameter param in paramList)
                    {
                        if (param != null)
                        {
                            dcSQLCommand.Parameters.Add(param);
                        }
                    }
                }
                daDataAdapter.SelectCommand = dcSQLCommand;
                dcSQLCommand.Connection = mobjSQLConnection;
                dcSQLCommand.Connection.Open();
                daDataAdapter.Fill(dtTable);
            }
            catch (SqlException se)
            {
                dtTable = null;
                throw se;
            }
            catch (Exception ex)
            {
                dtTable = null;
                throw ex;
            }
            finally
            {
                dcSQLCommand.Connection.Close();
                daDataAdapter.Dispose();
                dcSQLCommand.Dispose();
            }

            return dtTable;
        }

        /// <summary>
        /// Fill a datatable from the database using the specified passed SqlCommand.
        /// Return the results from the SQL stored proc.
        /// </summary>
        /// <param name="SQLCommand">Name of stored command object to use</param>
        /// <param name="tableName">Name of dataset table</param>
        /// <returns>DataTable of results after executing SQL statement</returns>
        public DataTable GetDataTable(SqlCommand comm, string tblName)
        {
            DataTable dtTable = new DataTable(tblName);
            SqlDataAdapter daDataAdapter = new SqlDataAdapter();
            SqlCommand localComm = new SqlCommand();

            localComm = comm;

            try
            {
                daDataAdapter.SelectCommand = localComm;
                localComm.Connection = mobjSQLConnection;
                localComm.Connection.Open();
                daDataAdapter.Fill(dtTable);
            }
            catch (SqlException se)
            {
                dtTable = null;
                throw se;
            }
            finally
            {
                localComm.Connection.Close();
                localComm.Dispose();
                daDataAdapter.Dispose();

            }

            return dtTable;
        }

        /// <summary>
        /// Fill a DataReader from the database using the specified Stored Procedure
        /// and return to Caller
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns>DataReader containing the results of executing SQL statement</returns>
        public SqlDataReader GetDataReader(string storedProcedure, SqlParameter[] paramList)
        {
            SqlDataReader objReader;
            SqlCommand dcSQLCommand = new SqlCommand();

            try
            {
                dcSQLCommand.CommandText = storedProcedure;
                dcSQLCommand.CommandType = CommandType.StoredProcedure;

                // Add stored procedure parameters to command
                if (paramList != null)
                {
                    foreach (SqlParameter param in paramList)
                    {
                        if (param != null)
                        {
                            dcSQLCommand.Parameters.Add(param);
                        }
                    }
                }
                dcSQLCommand.Connection = mobjSQLConnection;
                dcSQLCommand.Connection.Open();
                objReader = dcSQLCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                if (dcSQLCommand.Connection.State == ConnectionState.Closed)
                {
                    dcSQLCommand.Connection.Close();
                }
                dcSQLCommand.Dispose();
                objReader = null;
                throw ex;
            }

            return objReader;
        }

        /// <summary>
        /// Fill a DataReader from the database using the specified SQL command
        /// and return to Caller
        /// </summary>
        /// <param name="commandText">SQL query to execute</param>
        /// <returns>DataReader containing the results of executing SQL statement</returns>
        public SqlDataReader GetDataReader(string commandText)
        {
            SqlDataReader objReader;
            SqlCommand dcSQLCommand = new SqlCommand();

            try
            {
                dcSQLCommand.CommandText = commandText;

                dcSQLCommand.Connection = mobjSQLConnection;
                dcSQLCommand.Connection.Open();
                objReader = dcSQLCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                if (dcSQLCommand.Connection.State == ConnectionState.Closed)
                {
                    dcSQLCommand.Connection.Close();
                }
                dcSQLCommand.Dispose();
                objReader = null;
                throw ex;
            }

            return objReader;
        }

        /// <summary>
        /// Fill a DataReader from the database using the specified Stored Procedure
        /// Assemble XmlDocument using the reader and return to Caller
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns>XmlDocument containing the results of executing SQL statement</returns>
        public XmlDocument GetXmlDocument(string storedProcedure, SqlParameter[] paramList, string businessClass)
        {
            SqlDataReader objReader = null;
            DataTable dtSchema = null;
            XmlDocument objXML = null;
            XmlElement objElement = null;

            try
            {
                objReader = GetDataReader(storedProcedure, paramList);
                if (objReader.HasRows == true)
                {
                    objXML = new XmlDocument();
                    objElement = objXML.CreateElement(businessClass);
                    objXML.AppendChild(objElement);
                    dtSchema = objReader.GetSchemaTable();
                    while (objReader.Read())
                    {
                        foreach (DataRow drColumn in dtSchema.Rows)
                        {
                            objElement = objXML.CreateElement(drColumn["ColumnName"].ToString());
                            objElement.InnerXml = objReader[objElement.Name].ToString();
                            objXML.DocumentElement.AppendChild(objElement);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objReader != null && objReader.IsClosed == false)
                {
                    objReader.Close();
                    objReader = null;
                }
                dtSchema = null;
            }

            return objXML;
        }

        /// <summary>
        /// Execute the specified Stored Procedure and return ADO ExecuteScalar output
        /// </summary>
        /// <param name="SQLStatement">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of parameters to apply to stored procedure</param>
        /// <returns>The single value returned by ADO ExecuteScalar method</returns>
        public object ExecuteScalar(string storedProcedure, SqlParameter[] paramList)
        {
            SqlCommand dcSQLCommand = new SqlCommand();
            object objScalarValue = null;

            try
            {

                dcSQLCommand.CommandText = storedProcedure;
                dcSQLCommand.CommandType = CommandType.StoredProcedure;

                // Add stored procedure parameters to command
                if (paramList != null)
                {
                    foreach (SqlParameter param in paramList)
                    {
                        if (param != null)
                        {
                            dcSQLCommand.Parameters.Add(param);
                        }
                    }
                }

                dcSQLCommand.Connection = mobjSQLConnection;
                if (dcSQLCommand.Connection.State != ConnectionState.Open)
                {
                    dcSQLCommand.Connection.Open();
                }

                if (this.UseTransaction)
                {
                    if (mobjSQLTransaction == null)
                    {
                        mobjSQLTransaction = dcSQLCommand.Connection.BeginTransaction(this.TransactionisolationLevel);
                    }
                    dcSQLCommand.Transaction = mobjSQLTransaction;
                }

                objScalarValue = dcSQLCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                dcSQLCommand = null;
                throw ex;
            }
            finally
            {
                if (mobjSQLConnection.State == ConnectionState.Open && !_leaveConnectionOpen)
                {
                    mobjSQLConnection.Close();
                }
            }

            return objScalarValue;
        }

        #endregion

        #region Data Modification

        /// <summary>
        /// Execute the specified Stored Procedure and return a count
        /// of the number of rows affected
        /// </summary>
        /// <param name="SQLStatement">Name of stored procedure to execute</param>
        /// <param name="paramList">Array of parameters to apply to stored procedure</param>
        /// <returns>The number of rows affected</returns>
        public int ExecuteSQL(string storedProcedure, SqlParameter[] paramList)
        {
            int intRowsAffected = 0;
            bool retrying = this.AutoRetry;
            bool finishedRetrying = false;
            int currentRetryCount = 0;
            int maxiumumRetries = this.AutoRetryMaximumTimes;
            while (!finishedRetrying)
            {
                finishedRetrying = true;    //  Assume all will work well and we will want to get out of the loop
                SqlCommand dcSQLCommand = new SqlCommand();
                string sqlStatement = string.Empty;
                try
                {

                    dcSQLCommand.CommandText = storedProcedure;
                    dcSQLCommand.CommandType = CommandType.StoredProcedure;
                    sqlStatement += dcSQLCommand.CommandText;

                    // Add stored procedure parameters to command
                    if (paramList != null)
                    {
                        foreach (SqlParameter param in paramList)
                        {
                            if (param != null)
                            {
                                dcSQLCommand.Parameters.Add(param);
                                if (param.Direction == ParameterDirection.Input)
                                {
                                    if (!string.IsNullOrEmpty(param.ParameterName))
                                    {
                                        sqlStatement += " " + param.ParameterName;
                                    }
                                    if (param.Value != null)
                                    {
                                        sqlStatement += "=" + param.Value.ToString();
                                    }
                                }
                            }
                        }
                    }

                    dcSQLCommand.Connection = mobjSQLConnection;
                    if (dcSQLCommand.Connection.State != ConnectionState.Open)
                    {
                        dcSQLCommand.Connection.Open();
                    }
                    if (this.LogSQLStatement)
                    {
                        string message = "ExecuteSQL starting: " + sqlStatement;
                        FinalBuild.LogWriter.WriteToLog(message, System.Diagnostics.EventLogEntryType.Information);
                    }
                    if (this.UseTransaction)
                    {
                        if (mobjSQLTransaction == null)
                        {
                            mobjSQLTransaction = dcSQLCommand.Connection.BeginTransaction(this.TransactionisolationLevel);
                        }
                        dcSQLCommand.Transaction = mobjSQLTransaction;
                    }

                    intRowsAffected = dcSQLCommand.ExecuteNonQuery();
                    dcSQLCommand.Parameters.Clear();
                    if (this.LogSQLStatement)
                    {
                        string message = "ExecuteSQL completed: " + sqlStatement;
                        FinalBuild.LogWriter.WriteToLog(message, System.Diagnostics.EventLogEntryType.Information);
                    }
                }
                catch (Exception ex)
                {
                    dcSQLCommand.Parameters.Clear();
                    dcSQLCommand.Dispose();
                    dcSQLCommand = null;
                    if (retrying
                        && ex is System.Data.SqlClient.SqlException &&
                        (((System.Data.SqlClient.SqlException)ex).Number == 1205
                        || ((System.Data.SqlClient.SqlException)ex).Number == -2))
                    {
                        currentRetryCount++;
                        if (currentRetryCount > maxiumumRetries)
                        {
                            string message = string.Format("ExecuteSQL retry limit exceeded {0}: ", currentRetryCount.ToString()) + sqlStatement;
                            FinalBuild.LogWriter.WriteToLog(message, System.Diagnostics.EventLogEntryType.Information);
                            throw ex;
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(this.AutoRetrySleep); //  Sleep before retrying
                            string message = string.Format("ExecuteSQL retry {0}: ", currentRetryCount.ToString()) + sqlStatement;
                            FinalBuild.LogWriter.WriteToLog(message, System.Diagnostics.EventLogEntryType.Information);
                            finishedRetrying = false;    //  Continue the loop
                        }
                    }
                    else
                    {
                        throw ex;
                    }
                }
                finally
                {
                    if (mobjSQLConnection.State == ConnectionState.Open && !_leaveConnectionOpen)
                    {
                        mobjSQLConnection.Close();
                    }
                }
            }
            return intRowsAffected;
        }

        /// <summary>
        /// Method to allow closing of the connection string if it had been 
        /// forced open to allow transactions.
        /// </summary>
        public void CloseConnection()
        {
            if (mobjSQLConnection != null && mobjSQLConnection.State == ConnectionState.Open)
            {
                mobjSQLConnection.Close();
            }
        }

        /// <summary>
        /// Execute the specified Stored Procedure and return a count
        /// of the number of rows affected
        /// Takes single ArrayListParameter alongside standard SqlParameters
        /// Converts ArrayListParameter to SqlParameter, runs multiple times if required
        /// </summary>
        /// <param name="storedProcedure">Name of stored procedure to execute</param>
        /// <param name="arrayParameter">Instance of the ArrayParameter class</param>
        /// <param name="paramList">Array of Sql Parameters, may be null</param>
        /// <returns>DataSet of results after executing SQL statement</returns>
        public int ExecuteSQL(string storedProcedure, ArrayListParameter arrayParameter, SqlParameter[] paramList)
        {
            ArrayList parameters = null;
            int result = 0;
            int intermediaryResult = 0;

            try
            {

                // Process ArrayListParameter
                SqlParameter[] arrArrayParameterInstances = arrayParameter.ConvertToParameterInstances();
                if (arrArrayParameterInstances.Length == 1)
                {
                    // Simple case, send converted Array as parameter to standard method
                    parameters = new ArrayList();
                    parameters.Add(arrArrayParameterInstances[0]);
                    if (paramList != null)
                    {
                        foreach (SqlParameter parameter in paramList)
                        {
                            parameters.Add(parameter);
                        }
                    }
                    return ExecuteSQL(storedProcedure, (SqlParameter[])parameters.ToArray(typeof(SqlParameter)));
                }
                else
                {
                    // Complex case, submit the query multiple times and merge results
                    parameters = new ArrayList();
                    foreach (SqlParameter objParameterInstance in arrArrayParameterInstances)
                    {
                        parameters.Clear();
                        parameters.Add(objParameterInstance);
                        if (paramList != null)
                        {
                            foreach (SqlParameter objParameter in paramList)
                            {
                                parameters.Add(objParameter);
                            }
                        }
                        intermediaryResult = ExecuteSQL(storedProcedure, (SqlParameter[])parameters.ToArray(typeof(SqlParameter)));
                        result += intermediaryResult;
                    }
                }
            }
            catch (SqlException se)
            {
                throw se;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }


        #endregion

        #region Transaction

        public void Commit()
        {
            try
            {
                if (mobjSQLTransaction != null)
                {
                    mobjSQLTransaction.Commit();
                    mobjSQLTransaction = null;
                }

                if (mobjSQLConnection != null)
                {
                    mobjSQLConnection.Close();
                    mobjSQLConnection = null;
                }
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        public void Rollback()
        {
            try
            {
                if (mobjSQLTransaction != null)
                {
                    mobjSQLTransaction.Rollback();
                }

                if (mobjSQLConnection != null)
                {
                    mobjSQLConnection.Close();
                }
            }
            catch (Exception excE)
            {
                throw excE;
            }
            finally
            {
                mobjSQLTransaction = null;
                if (mobjSQLConnection != null)
                {
                    CloseConnection();
                    mobjSQLConnection = null;
                } 
            }
        }


        #endregion

        #region Schema

        public OleDbConnection GetOLEConnection(string strServer, string
strDatabase)
        {
            OleDbConnection objSQLConnection;
            string strProvider = "SQLOLEDB";
            string strUser = "";
            string strPassword = "";

            //Creates an OLEDB Connection object based on the parameter values passed in.

            if (strUser == "")
            {
                //Windows Authentication.
                mstrADOConnectionString = "Provider=" + strProvider +
                    ";Data Source=" + strServer +
                    ";Integrated Security=SSPI" +
                    ";Initial Catalog=" + strDatabase;
            }
            else
            {
                //Database Authentication.
                mstrADOConnectionString = "Provider=" + strProvider +
                    ";Data Source=" + strServer +
                    ";Initial Catalog=" + strDatabase +
                    ";User ID=" + strUser +
                    ";Password=" + strPassword;
            }
            //Create Connection
            objSQLConnection = new OleDbConnection(mstrADOConnectionString);

            return objSQLConnection;

        }

        public OleDbConnection GetOLEConnection(string strServer, string
strDatabase, string userID, string password)
        {
            OleDbConnection objSQLConnection;
            string strProvider = "SQLOLEDB";

            //Creates an OLEDB Connection object based on the parameter values passed in.

            if (userID == "")
            {
                //Windows Authentication.
                mstrADOConnectionString = "Provider=" + strProvider +
                    ";Data Source=" + strServer +
                    ";Integrated Security=SSPI" +
                    ";Initial Catalog=" + strDatabase;
            }
            else
            {
                //Database Authentication.
                mstrADOConnectionString = "Provider=" + strProvider +
                    ";Data Source=" + strServer +
                    ";Initial Catalog=" + strDatabase +
                    ";User ID=" + userID +
                    ";Password=" + password;
            }
            //Create Connection
            objSQLConnection = new OleDbConnection(mstrADOConnectionString);

            return objSQLConnection;

        }

        public OleDbConnection GetOLEConnection(string connectionString)
        {
            OleDbConnection objSQLConnection;

            //Creates an OLEDB Connection object based on the parameter values passed in.

            objSQLConnection = new OleDbConnection(connectionString);

            return objSQLConnection;

        }


        public DataTable GetConnectionObjects(string businessClass, eDBObjectType
objectType, string[] includedSPs)
        {
            DataTable objSchemaTable = null;
            string strobjectType = "";

            switch (objectType)
            {
                case eDBObjectType.Table:
                    {
                        strobjectType = "TABLE";
                        break;
                    }
                case eDBObjectType.View:
                    {
                        strobjectType = "VIEW";
                        break;
                    }
            }

            try
            {
                if (mobjOLEConnection == null)
                {
                    mobjOLEConnection = GetOLEConnection(mstrADOConnectionString);
                }
                mobjOLEConnection.Open();
                switch (objectType)
                {
                    case eDBObjectType.Procedure:
                        objSchemaTable =
mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Procedures, new
object[] { null, null, null, null });
                        break;
                    default:
                        objSchemaTable =
mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { 
null, null, null, strobjectType });
                        break;
                }
                mobjOLEConnection.Close();
            }
            catch (Exception ex)
            {
                // Don//t throw, a test of objSchemaTable = null is made by caller
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (mobjOLEConnection.State != ConnectionState.Closed)
                {
                    mobjOLEConnection.Close();
                }
            }

            /*
            if (businessClass != "")
            {
                objSchemaTable = FilterObjectsByClass(objSchemaTable, businessClass);
                if (includedSPs != null)
                {
                    objSchemaTable = FilterObjectsBySelection(objSchemaTable, includedSPs);
                }
            }
            */

            return objSchemaTable;
        }

        public DataTable GetConnectionObjects(eDBObjectType objectType)
        {
            DataTable objSchemaTable = null;
            string strobjectType = "";

            switch (objectType)
            {
                case eDBObjectType.Table:
                    {
                        strobjectType = "TABLE";
                        break;
                    }
                case eDBObjectType.View:
                    {
                        strobjectType = "VIEW";
                        break;
                    }
            }

            try
            {
                if (mobjOLEConnection == null)
                {
                    mobjOLEConnection = GetOLEConnection(mstrADOConnectionString);
                }
                mobjOLEConnection.Open();
                switch (objectType)
                {
                    case eDBObjectType.Procedure:
                        {
                            objSchemaTable =
    mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Procedures, new
    object[] { null, null, null, null });
                            break;
                        }
                    default:
                        {
                            objSchemaTable =
    mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, strobjectType });
                            break;
                        }
                }
                mobjOLEConnection.Close();
            }
            catch (Exception ex)
            {
                // Don//t throw, a test of objSchemaTable = null is made by caller
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (mobjOLEConnection.State != ConnectionState.Closed)
                {
                    mobjOLEConnection.Close();
                }
            }

            return objSchemaTable;

        }

        public DataTable GetTableColumns(string tableName)
        {
            DataTable objSchemaTable = null;

            /*
            objSchemaTable.Columns[3].ColumnName
            "COLUMN_NAME"
            objSchemaTable.Columns[6].ColumnName
            "ORDINAL_POSITION"
            objSchemaTable..Columns[7].ColumnName
            "COLUMN_HASDEFAULT"
            objSchemaTable..Columns[9].ColumnName
            "COLUMN_FLAGS"
            objSchemaTable..Columns[11].ColumnName
            "DATA_TYPE"
            objSchemaTable.Columns[15].ColumnName
            "NUMERIC_PRECISION"

            -- Other fields of interest
            "IS_NULLABLE"
            "CHARACTER_MAXIMUM_LENGTH"
            "CHARACTER_OCTET_LENGTH"

            */
            try
            {
                if (mobjOLEConnection == null)
                {
                    mobjOLEConnection = GetOLEConnection(mstrServer, mstrDatabase);
                }
                mobjOLEConnection.Open();
                objSchemaTable =
mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, tableName, null });
                mobjOLEConnection.Close();
            }
            catch (Exception ex)
            {
                // Don//t throw, a test of objSchemaTable = null is made by caller
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (mobjOLEConnection.State != ConnectionState.Closed)
                {
                    mobjOLEConnection.Close();
                }
            }

            return objSchemaTable;

        }

        public DataTable GetProcedureParameters(string storedProcedure)
        {
            DataTable objSchemaTable = null;

            try
            {
                if (mobjOLEConnection == null)
                {
                    mobjOLEConnection = GetOLEConnection(mstrServer, mstrDatabase);
                }
                mobjOLEConnection.Open();
                objSchemaTable =
mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Procedure_Parameters,
new object[] { null, null, storedProcedure, null });
                mobjOLEConnection.Close();
            }
            catch (Exception ex)
            {
                // Don//t throw, a test of objSchemaTable = null is made by caller
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (mobjOLEConnection.State != ConnectionState.Closed)
                {
                    mobjOLEConnection.Close();
                }
            }

            return objSchemaTable;

            //"PROCEDURE_CATALOG"
            //?dtparameters.Columns(1).ColumnName
            //"PROCEDURE_SCHEMA"
            //?dtparameters.Columns(2).ColumnName
            //"PROCEDURE_NAME"
            //?dtparameters.Columns(3).ColumnName
            //"PARAMETER_NAME"
            //?dtparameters.Columns(4).ColumnName
            //"ORDINAL_POSITION"
            //?dtparameters.Columns(5).ColumnName
            //"PARAMETER_TYPE"
            //?dtparameters.Columns(6).ColumnName
            //"PARAMETER_HASDEFAULT"
            //?dtparameters.Columns(7).ColumnName
            //"PARAMETER_DEFAULT"
            //?dtparameters.Columns(8).ColumnName
            //"IS_NULLABLE"
            //?dtparameters.Columns(9).ColumnName
            //"DATA_TYPE"
            //?dtparameters.Columns(10).ColumnName
            //"CHARACTER_MAXIMUM_LENGTH"
            //?dtparameters.Columns(1).ColumnName
            //"PROCEDURE_SCHEMA"
            //?dtparameters.Columns(12).ColumnName
            //"NUMERIC_PRECISION"
            //?dtparameters.Columns(13).ColumnName
            //"NUMERIC_SCALE"
            //?dtparameters.Columns(14).ColumnName
            //"DESCRIPTION"
            //?dtparameters.Columns(15).ColumnName
            //"TYPE_NAME"
            //?dtparameters.Columns(16).ColumnName
            //"LOCAL_TYPE_NAME"

        }

        public DataTable GetProcedureColumns(string storedProcedure)
        {
            DataTable objSchemaTable = null;

            // Note. OleDbSchemaGuid.Procedure_Columns not supported by SQLOLEDB

            try
            {
                if (mobjOLEConnection == null)
                {
                    mobjOLEConnection = GetOLEConnection(mstrServer, mstrDatabase);
                }
                mobjOLEConnection.Open();
                objSchemaTable = mobjOLEConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Procedure_Columns, new object[] { null, null, storedProcedure, null });
                mobjOLEConnection.Close();
            }
            catch (Exception ex)
            {
                // Don//t throw, a test of objSchemaTable = null is made by caller
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (mobjOLEConnection.State != ConnectionState.Closed)
                {
                    mobjOLEConnection.Close();
                }
            }

            return objSchemaTable;

        }

        public DataSet GetProcedureOutputSchema(string storedProcedure)
        {
            DataSet dsOutput = null;
            SqlParameter[] arrParameters = null;

            arrParameters = GetSqlParametersFromOleSchema(storedProcedure);
            dsOutput = GetDataSet(storedProcedure, arrParameters);

            return dsOutput;
        }

        public SqlParameter[] GetSqlParametersFromOleSchema(string storedProcedure)
        {
            SqlParameter[] arrParameters = null;
            SqlParameter objParameter = null;
            DataTable dtParameters = null;
            ArrayList colParameters = new ArrayList();

            try
            {
                dtParameters = GetProcedureParameters(storedProcedure);
                if (dtParameters != null && dtParameters.Rows.Count > 0)
                {
                    foreach (DataRow drParameter in dtParameters.Rows)
                    {
                        if ((int)drParameter["PARAMETER_TYPE"] != 4) // Return PARAMETER:4
                        {
                            objParameter = new SqlParameter(drParameter["PARAMETER_NAME"].ToString(), null);
                            if ((int)drParameter["PARAMETER_TYPE"] == 2) // Output Parameter
                            {
                                objParameter.Direction = ParameterDirection.Output;
                            }
                            switch (drParameter["TYPE_NAME"].ToString().ToUpper())
                            {
                                case "BIT":
                                case "INT":
                                case "SMALLINT":
                                case "BIGINT":
                                case "DECIMAL":
                                case "FLOAT":
                                case "REAL":
                                    {
                                        objParameter.SqlDbType = SqlDbType.Int;
                                        objParameter.Value = 0;
                                        break;
                                    }
                                case "DATETIME":
                                case "SMALLDATETIME":
                                    {
                                        objParameter.SqlDbType = SqlDbType.DateTime;
                                        objParameter.Value = DateTime.Now;
                                        break;
                                    }
                                default: // String assumed (VARCHAR, CHAR, NVARCHAR, NCHAR)
                                    {
                                        objParameter.DbType = DbType.String;
                                        objParameter.Value = "X";
                                        if (drParameter["CHARACTER_MAXIMUM_LENGTH"].ToString() != string.Empty)
                                        {
                                            objParameter.Size = int.Parse(drParameter["CHARACTER_MAXIMUM_LENGTH"].ToString());
                                        }
                                        if (objParameter.Size > 1)
                                        {
                                            int intCharactersSoFar = 1;
                                            while (intCharactersSoFar < objParameter.Size)
                                            {
                                                objParameter.Value += "X";
                                                intCharactersSoFar += 1;
                                            }
                                        }
                                        break;
                                    }
                            }
                            colParameters.Add(objParameter);
                        }
                    }
                }
            }
            catch (SqlException se)
            {
                dtParameters = null;
                throw se;
            }
            finally
            {
            }

            if (colParameters.Count > 0)
            {
                arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));
            }

            return arrParameters;
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Convert a DataSet to a Flat Table format containing all the Rows across
        /// the Tables of the DataSet
        /// </summary>
        /// <param name="inputData">The DataSet to be flattened</param>
        /// <param name="relatedFields">Array of columns to use to link the Tables</param>
        /// <returns>Flat Table</returns>
        public static DataTable FlattenDataSet(DataSet inputData, string[] relatedFields, string parentClass)
        {
            DataTable dtFlatTable = new DataTable();
            DataTable dtActiveTable;
            DataTable dtExisting;
            DataRow drNewRow;
            DataColumn dcNewColumn;
            int intNextTableIndex = 1;

            // Generate Flat Table structure (All columns of dataset member Tables)
            for (int intTableIndex = 0; intTableIndex < inputData.Tables.Count; intTableIndex++)
            {
                dtExisting = inputData.Tables[intTableIndex];
                foreach (DataColumn dcExisting in dtExisting.Columns)
                {
                    // Add Column to Flat Table
                    if (intTableIndex == 0)
                    {
                        dcNewColumn = new DataColumn(dcExisting.ColumnName, dcExisting.DataType);
                    }
                    else
                    {
                        // Name column so as to ensure uniqueness across dataset member Tables
                        dcNewColumn = new DataColumn(dtExisting.TableName + "_" + dcExisting.ColumnName, dcExisting.DataType);
                    }
                    dtFlatTable.Columns.Add(dcNewColumn);
                }
            }

            // Populate the Flat Table structure 
            dtActiveTable = inputData.Tables[parentClass];
            foreach (DataRow drExisting in dtActiveTable.Rows)
            {
                drNewRow = dtFlatTable.NewRow();
                // Populate columns of Flat Table row from values in current row of first table
                foreach (DataColumn dcExisting in dtActiveTable.Columns)
                {
                    drNewRow[dcExisting.ColumnName] = drExisting[dcExisting.ColumnName];
                }
                // Populate columns of Flat Table row from values in related rows of other tables
                // This loop is to allow for fact first Table may not be parentClass
                for (int intIndex = 0; intIndex < inputData.Tables.Count; intIndex++)
                {
                    if (inputData.Tables[intIndex].TableName == parentClass)
                    {
                        intNextTableIndex = intIndex + 1;
                        break;
                    }
                }
                try
                {
                    drNewRow = RecursivelyMergeChildRowData(inputData, drExisting, ref intNextTableIndex, relatedFields, drNewRow);
                }
                catch (Exception excE)
                {
                    // Ignore for the moment - not all columns will be populated
                    string adummy = excE.Message;
                }
                finally
                {
                    // Add new row to Flat Table
                    dtFlatTable.Rows.Add(drNewRow);
                }
            }

            return dtFlatTable;
        }

        private static DataRow RecursivelyMergeChildRowData(DataSet inputData, DataRow parentRow, ref int nextTableIndex, string[] relatedFields, DataRow outputRow)
        {
            DataRow[] arrMatchingRows = null;
            string strFilterExpression;

            // Populate columns of Flat Table row from values in related row(s) of next table
            if (nextTableIndex < inputData.Tables.Count && nextTableIndex <= (relatedFields.Length))
            {
                strFilterExpression = relatedFields[nextTableIndex - 1] + "=";
                strFilterExpression += parentRow[relatedFields[nextTableIndex - 1]].ToString();
                arrMatchingRows = inputData.Tables[nextTableIndex].Select(strFilterExpression);
                if (arrMatchingRows.Length == 1)
                {
                    DataRow drMatching = arrMatchingRows[0];
                    foreach (DataColumn dcMatching in drMatching.Table.Columns)
                    {
                        outputRow[drMatching.Table.TableName + "_" + dcMatching.ColumnName] = drMatching[dcMatching.ColumnName];
                    }
                    nextTableIndex += 1;
                    outputRow = RecursivelyMergeChildRowData(inputData, drMatching, ref nextTableIndex, relatedFields, outputRow);
                }
            }
            else
            {
                // Provides exit condition to recursive call via byref param
                nextTableIndex = inputData.Tables.Count;
            }

            return outputRow;
        }

        #endregion


        #endregion

        #region Private Methods

        private string GenerateConnectionString(string dataSource, string initialCatalog, bool includeProvider)
        {
            string strProvider = "SQLOLEDB";
            string strUser = "";
            string strPassword = "";
            string strConnectionString = null;

            //Creates an OLEDB Connection object based on the parameter values passed in.

            if (strUser == "")
            {
                //Windows Authentication.
                if (includeProvider == true)
                {
                    strConnectionString = "Provider=" + strProvider + ";";
                }
                strConnectionString += "Data Source=" + dataSource +
                                        ";Integrated Security=SSPI" +
                                        ";Initial Catalog=" + initialCatalog;
            }
            else
            {
                //Database Authentication.
                if (includeProvider == true)
                {
                    strConnectionString = "Provider=" + strProvider + ";";
                }
                strConnectionString += "Data Source=" + dataSource +
                                        ";Initial Catalog=" + initialCatalog +
                                        ";User ID=" + strUser +
                                        ";Password=" + strPassword;
            }

            return strConnectionString;

        }

        private string GenerateDetailedError(string storedProcedure, SqlParameter[] paramList)
        {
            string strErrorMessage = "";

            // TODO: Iterate Parameters to form message inc SP * Param values
            strErrorMessage = "Error executing stored procedure " + storedProcedure;
            if (paramList != null)
            {
                strErrorMessage += Environment.NewLine;
                foreach (SqlParameter objParameter in paramList)
                {
                    strErrorMessage += objParameter.ParameterName + "=" + objParameter.Value.ToString();
                    strErrorMessage += Environment.NewLine;
                }
            }


            return strErrorMessage;
        }

        #endregion

        #region Public Classes

        public class ArrayListParameter
        {
            #region Public Properties and Fields

            public object[] ArrayValues;
            public string ParameterName;
            public string Delimiter = ",";
            public int MaximumLength = 8000;
            public int MaximumLengthOfArrayMember = 8;

            #endregion

            #region Public Methods

            public ArrayListParameter(string parameterName, object[] arrayValues)
            {
                ParameterName = parameterName;
                ArrayValues = arrayValues;
            }

            public ArrayListParameter(string parameterName, int[] arrayValues)
            {
                ParameterName = parameterName;
                ArrayValues = new object[arrayValues.Length];
                for (int intIndex = 0; intIndex < arrayValues.Length; intIndex++)
                {
                    ArrayValues[intIndex] = (object)arrayValues[intIndex];
                }
            }

            public ArrayListParameter(string parameterName, object singleValue)
            {
                ParameterName = parameterName;
                ArrayValues = new object[1];
                ArrayValues[0] = singleValue;
            }

            public ArrayListParameter()
            {
            }

            public SqlParameter[] ConvertToParameterInstances()
            {
                ArrayList colParametersInstances = new ArrayList();
                int intNoOfInstancesRequired = 0;
                int intStartingIndex = 0;
                int intCharactersUsed = 0;
                decimal decResultOfDivision = 0;

                // Calculate how many parameter instances required to fit with MaximumLength
                // Each member of array to string: MaximumLengthOfArrayMember + 1 for delimiter

                decResultOfDivision = ((decimal)(ArrayValues.Length * (MaximumLengthOfArrayMember + 1)) / MaximumLength);
                intNoOfInstancesRequired = (int)Math.Ceiling(decResultOfDivision);

                // Simple case - only one parameter instance required
                if (intNoOfInstancesRequired == 1)
                {
                    colParametersInstances.Add(ConvertArrayToSqlParameter(0, 0));
                    return (SqlParameter[])colParametersInstances.ToArray(typeof(SqlParameter));
                }

                // Multiple Instances required
                for (int intIndex = 0; intIndex < ArrayValues.Length; intIndex++)
                {
                    intCharactersUsed += (MaximumLengthOfArrayMember + 1);
                    if (intCharactersUsed >= MaximumLength)// || intIndex == ArrayValues.GetUpperBound(0))
                    {
                        colParametersInstances.Add(ConvertArrayToSqlParameter(intStartingIndex, intIndex));
                        intStartingIndex = intIndex;
                        intCharactersUsed = 0; // Reset ready for next set of values
                    }
                    // Amended 10 Jul 2007
                    // Rhys Gravell
                    // The ConvertArrayToSqlParameter method endOfArrayIndex variable is an 
                    // exclusive Index. When multiple instances were required the ConvertArrayToSqlParameter
                    // excluded the value at the index == endOfArrayIndex. 
                    // The logic for processing the final parameter instance has been amended to pass 
                    // its final index as the final index value +1 due to this parameters exclusivity.
                    else if (intIndex == ArrayValues.GetUpperBound(0))
                    {
                        colParametersInstances.Add(ConvertArrayToSqlParameter(intStartingIndex, intIndex + 1));
                    }
                }

                return (SqlParameter[])colParametersInstances.ToArray(typeof(SqlParameter));
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Convert Array values into a string, using Delimiter, string <= MaximumLength
            /// </summary>
            /// <param name="endOfArrayIndex"></param>
            /// <returns></returns>
            private SqlParameter ConvertArrayToSqlParameter(int startOfArrayIndex, int endOfArrayIndex)
            {
                SqlParameter objParameter = null;
                string strArrayList = null;

                // Array only has one member
                if (ArrayValues.Length == 1)
                {
                    strArrayList = ArrayValues[0].ToString();
                    objParameter = new SqlParameter("@" + ParameterName, SqlDbType.VarChar, MaximumLength);
                    objParameter.Value = strArrayList;
                    return objParameter;
                }

                // Array has multiple members
                if (endOfArrayIndex == 0)
                {
                    endOfArrayIndex = ArrayValues.Length;
                }
                if (endOfArrayIndex == ArrayValues.GetUpperBound(0) - 1)
                {
                    endOfArrayIndex = ArrayValues.GetUpperBound(0);
                }
                // Add Array Members to delimited string until endOfArrayIndex is reached
                for (int intIndex = startOfArrayIndex; intIndex < endOfArrayIndex; intIndex++)
                {
                    strArrayList += ArrayValues[intIndex].ToString() + Delimiter;
                }
                // Trim trailing Delimiter
                strArrayList = strArrayList.Substring(0, strArrayList.Length - 1);

                // Return Sql Parameter
                objParameter = new SqlParameter("@" + ParameterName, SqlDbType.VarChar, MaximumLength);
                objParameter.Value = strArrayList;

                return objParameter;
            }

            #endregion
        }


        #endregion

    }

    public class DataTypeConverter
    {
        public static Type ConvertSQLType(string sqlDbType)
        {
            Type dataType = typeof(String);

            switch (sqlDbType.ToUpper())
            {
                case "INT":
                case "INTEGER":
                    {
                        dataType = typeof(Int32);
                        break;
                    }
                case "SMALLINT":
                case "TINYINT":
                    {
                        dataType = typeof(Int16);
                        break;
                    }
                case "DATETIME":
                case "SMALLDATETIME":
                    {
                        dataType = typeof(DateTime);
                        break;
                    }
                case "DECIMAL":
                case "FLOAT": // Precision not specified but typically used as Decimal
                case "MONEY":
                case "NUMERIC":
                case "REAL": // Precision not specified but typically used as Decimal
                    {
                        dataType = typeof(Decimal);
                        break;
                    }
                case "BIT":
                    {
                        dataType = typeof(Boolean);
                        break;
                    }
                default:
                    {
                        // Including varchar, nvarchar, char, ntext
                        dataType = typeof(String);
                        break;
                    }
            }

            return dataType;
        }
    }
}

