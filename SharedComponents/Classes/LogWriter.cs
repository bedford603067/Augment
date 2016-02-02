using System;
using System.IO;
using System.Configuration;
using System.Diagnostics;

using System.Data.SqlClient;
using System.Collections;
using System.Data;

namespace FinalBuild
{
    /// <summary>
    /// Logging mechanisms that are supported by LogWriter and can be declared in LoggingSection 
    /// </summary>
    public enum eLoggingMechanism
    { EventLogAndSQLServer, EventLog, File, SQLServer, None }

    /// <summary>
    /// Provides static methods to log messages using the mechanisms defined by eLoggingMechanism
    /// </summary>
    public abstract class LogWriter
    {
        #region Private Fields

        private static string DEFAULT_EVENT_LOG = "Application";

        #endregion

        #region Public Methods

        /// <summary>
        /// Write Exception to a log called LogName as directed by LoggingMechanism 
        /// Entry will be of type Error if written to the Event Log 
        /// Create a LoggingSection in the config file to control LogName and LoggingMechanism
        /// </summary>
        /// <param name="currentException"></param>
        public static void WriteToLog(Exception exception)
        {
            WriteToLog(exception, string.Empty);
        }

        /// <summary>
        /// Write Exception to a log called LogName as directed by LoggingMechanism 
        /// Entry will be of type Error if written to the Event Log 
        /// Create a LoggingSection in the config file to control LogName and LoggingMechanism
        /// Specify ServiceName to further identify the source of the message (SQL Server only)
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="serviceName"></param>
        public static void WriteToLog(Exception exception, string serviceName)
        {
            string strErrorMessage = exception.Message;
            string strStackTrace = exception.StackTrace;
            int intExceptionCounter = 0;

            while (intExceptionCounter < 10)
            {
                if (exception.InnerException != null)
                {
                    strErrorMessage += Environment.NewLine + exception.InnerException.Message;
                    strStackTrace += Environment.NewLine + string.Format("StackTrace inner {0}:", intExceptionCounter) + Environment.NewLine + exception.InnerException.StackTrace;

                    // Look for Inner Exception in the current Inner Exception..
                    exception = exception.InnerException;
                }
                else
                {
                    break;
                }
                intExceptionCounter += 1;
            }

            string logMessage = strErrorMessage + Environment.NewLine + "----StackTrace(s)----" + strStackTrace;

            CreateLogEntry(logMessage, EventLogEntryType.Error, null, serviceName);
        }

        /// <summary>
        /// Write Message to a log called LogName as directed by LoggingMechanism 
        /// Entry will be of type Information if written to the Event Log 
        /// Create a LoggingSection in the config file to control LogName and LoggingMechanism
        /// </summary>
        /// <param name="logMessage"></param>
        public static void WriteToLog(string logMessage)
        {
            WriteToLog(logMessage, null);
        }

        /// <summary>
        /// Write Message to a log called LogName as directed by LoggingMechanism 
        /// Entry will be of type supplied if written to the Event Log 
        /// Create a LoggingSection in the config file to control LogName and LoggingMechanism
        /// </summary>
        /// <param name="logMessage"></param>
        public static void WriteToLog(string logMessage, EventLogEntryType eventLogEntryType)
        {
            CreateLogEntry(logMessage, eventLogEntryType);
        }

        /// <summary>
        /// Write Message to a log called LogName as directed by LoggingMechanism 
        /// Entry will be of type Information if written to the Event Log 
        /// Create a LoggingSection in the config file to control LogName and LoggingMechanism
        /// Specify ServiceName to further identify the source of the message (SQL Server only)
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="serviceName"></param>
        public static void WriteToLog(string logMessage, string serviceName)
        {
            CreateLogEntry(logMessage, EventLogEntryType.Information);
        }

        public static void WriteToLog(ClientLogEntryCollection clientLogEntries)
        {
            WriteToLog(clientLogEntries, "MobileClient");
        }

        public static void WriteToLog(ClientLogEntryCollection clientLogEntries, string logName)
        {
            if (clientLogEntries != null)
            {
                foreach (ClientLogEntry objEntry in clientLogEntries)
                {
                    WriteToLog(objEntry, logName);
                }
            }
        }

        public static void WriteToLog(ClientLogEntry clientLogEntry, string logName)
        {
            LoggingSection objSection = (LoggingSection)ConfigurationManager.GetSection("LoggingSection");
            SQLServerLog objDatabaseSettings = null;

            if (objSection != null)
            {
                // By definition : eLoggingMechanism.SQLServer
                objDatabaseSettings = objSection.DatabaseLog;
            }

            CreateSQLServerLogEntry(clientLogEntry, objDatabaseSettings);
        }

        public static void WriteToLog(ClientLogEntry clientLogEntry)
        {
            WriteToLog(clientLogEntry, "MobileClient");
        }

        public static DataTable GetLogEntries(string filterExpression)
        {
            return GetLogEntries(DateTime.MinValue, DateTime.MinValue, EventLogEntryType.Information, null, filterExpression, null);
        }

        public static DataTable GetLogEntries(string filterExpression, EventLogEntryType entryType)
        {
            return GetLogEntries(DateTime.MinValue, DateTime.MinValue, entryType, null, filterExpression, null);
        }

        public static DataTable GetLogEntries(DateTime startDate, DateTime endDate, string filterExpression)
        {
            return GetLogEntries(startDate, endDate, EventLogEntryType.Information, null, filterExpression, null);
        }

        public static DataTable GetLogEntries(DateTime startDate, DateTime endDate, string logName, string serviceName)
        {
            return GetLogEntries(startDate, endDate, EventLogEntryType.Information, logName, null, serviceName);
        }

        public static DataTable GetLogEntries(DateTime startDate, DateTime endDate, EventLogEntryType entryType, string logName, string filterExpression, string serviceName)
        {
            DataTable dtLogEntries = new DataTable();
            DataAccess objADO = null;
            ArrayList colParameters = new ArrayList();
            SqlParameter[] arrParameters = null;

            try
            {
                // Use default Parameter values if not specified
                if(startDate == DateTime.MinValue)
                {
                    startDate = DateTime.Now.AddDays(-1);
                }
                if (endDate == DateTime.MinValue)
                {
                    endDate = DateTime.Now;
                }
                if (logName == null || logName == string.Empty)
                {
                    logName = GetLogName();
                }

                objADO = GetADOInstance();

                colParameters.Add(new SqlParameter("@StartDate", startDate));
                colParameters.Add(new SqlParameter("@EndDate", endDate));
                colParameters.Add(new SqlParameter("@EventLogEntryType", entryType));
                colParameters.Add(new SqlParameter("@Application", logName));
                if (filterExpression != null && filterExpression != string.Empty)
                {
                    colParameters.Add(new SqlParameter("@FilterExpression", filterExpression));
                }
                if (serviceName != null && serviceName != string.Empty)
                {
                    colParameters.Add(new SqlParameter("@ServiceName", serviceName));
                }

                arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));
                dtLogEntries = objADO.GetDataTable("selLogEntries", "LogEntries", arrParameters);
            }
            catch(Exception excE)
            {
                throw excE;
            }
            finally
            {
                objADO = null;
            }

            return dtLogEntries;
        }

        public static DataTable GetLogExceptions(string logName, DateTime startDate, DateTime endDate)
        {
            return GetLogExceptions(logName, startDate, endDate, null);
        }

        public static DataTable GetLogExceptions(string logName, DateTime startDate, DateTime endDate, string serviceName)
        {
            return GetLogEntries(startDate, endDate, EventLogEntryType.Error, logName, null, serviceName);
        }

        public static void RemoveLogEntries(EventLogEntryType entryType, string filterExpression)
        {
            RemoveLogEntries(DateTime.MinValue, DateTime.MinValue, entryType, null, filterExpression);
        }

        public static void RemoveLogEntries(DateTime startDate, DateTime endDate, EventLogEntryType entryType, string logName, string filterExpression)
        {
            DataAccess objADO = null;
            ArrayList colParameters = new ArrayList();
            SqlParameter[] arrParameters = null;
            int intReturn = 0;

            try
            {
                // Use default Parameter values if not specified
                if (startDate == DateTime.MinValue)
                {
                    startDate = DateTime.Now.AddDays(-1);
                }
                if (endDate == DateTime.MinValue)
                {
                    endDate = DateTime.Now;
                } 
                if (logName == null || logName == string.Empty)
                {
                    logName = GetLogName();
                }

                objADO = GetADOInstance();

                colParameters.Add(new SqlParameter("@StartDate", startDate));
                colParameters.Add(new SqlParameter("@EndDate", endDate));
                colParameters.Add(new SqlParameter("@EventLogEntryType", entryType));
                colParameters.Add(new SqlParameter("@Application", logName));
                if (filterExpression != null && filterExpression != string.Empty)
                {
                    colParameters.Add(new SqlParameter("@FilterExpression", filterExpression));
                }

                arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));
                intReturn = objADO.ExecuteSQL("delLogEntries", arrParameters);
            }
            catch (Exception excE)
            {
                throw excE;
            }
            finally
            {
                objADO = null;
            }

            objADO = null;
        }

        public static string[] GetPredefinedLogNames()
        {
            string[] logNames = null;
            DataTable dtLogNames = new DataTable();
            DataAccess objADO = null;

            try
            {
                objADO = GetADOInstance();
                dtLogNames = objADO.GetDataTable("selPredefinedLogNames", "LogNames");
                if (dtLogNames != null && dtLogNames.Rows.Count > 0)
                {
                    logNames = new string[dtLogNames.Rows.Count];
                    for (int intIndex = 0; intIndex < logNames.Length; intIndex++)
                    {
                        logNames[intIndex] = dtLogNames.Rows[intIndex]["LogName"].ToString();
                    }
                }
            }
            catch (Exception excE)
            {
                throw excE;
            }
            finally
            {
                objADO = null;
            }

            return logNames;
        }

        #endregion

        #region Private Methods

        private static void CreateLogEntry(string logMessage, EventLogEntryType entryType)
        {
            CreateLogEntry(logMessage, entryType, null, null);
        }

        private static void CreateLogEntry(string logMessage, EventLogEntryType entryType, string logName, string serviceName)
        {
            LoggingSection objSection = (LoggingSection)ConfigurationManager.GetSection("LoggingSection");
            eLoggingMechanism objMechanism = eLoggingMechanism.EventLog;
            SQLServerLog objDatabaseSettings = null;
            string strLogName = "Application";
            string strServiceName = null;

            try
            {
                if (objSection != null)
                {
                    objMechanism = objSection.LoggingMechanism;
                    strLogName = objSection.LogName;
                    objDatabaseSettings = objSection.DatabaseLog;
                    if (objSection.ServiceName != null && objSection.ServiceName != string.Empty)
                    {
                        strServiceName = objSection.ServiceName;
                    }
                }
                if (logName != null && logName != string.Empty)
                {
                    strLogName = logName;
                }
                if (serviceName != null && serviceName != string.Empty)
                {
                    strServiceName = serviceName;
                }
                switch (objMechanism)
                {
                    case eLoggingMechanism.EventLogAndSQLServer:
                        {
                            try
                            {
                                CreateSQLServerLogEntry(logMessage, entryType, strLogName, strServiceName, objSection.DatabaseLog);
                                CreateEventLogEntry(logMessage, entryType, strLogName);
                            }
                            catch (Exception excE)
                            {
                                // If this fails bubbles to outer handler
                                logMessage += Environment.NewLine + "(Unable to log message to custom Event Log or SQL Server or both : " + Environment.NewLine + excE.Message + ")";
                                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
                            }
                            break;
                        }
                    case eLoggingMechanism.EventLog:
                        {
                            try
                            {
                                CreateEventLogEntry(logMessage, entryType, strLogName);
                            }
                            catch (Exception excE)
                            {
                                // If this fails bubbles to outer handler
                                logMessage += Environment.NewLine + "(Unable to log message to Event Log : " + Environment.NewLine + excE.Message + ")";
                                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
                            }
                            break;
                        }
                    case eLoggingMechanism.File:
                        {
                            try
                            {
                                CreateFileLogEntry(logMessage, entryType, strLogName);
                            }
                            catch (Exception excE)
                            {
                                // If this fails bubbles to outer handler
                                logMessage += Environment.NewLine + "(Unable to log message to File : " + Environment.NewLine + excE.Message + ")";
                                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
                            }
                            break;
                        }
                    case eLoggingMechanism.SQLServer:
                        {
                            try
                            {
                                CreateSQLServerLogEntry(logMessage, entryType, strLogName, strServiceName, objSection.DatabaseLog);
                            }
                            catch (Exception excE)
                            {
                                // If this fails bubbles to outer handler
                                logMessage += Environment.NewLine + "(Unable to log message to SQL Server : " + Environment.NewLine + excE.Message + ")";
                                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
                            }
                            break;
                        }
                    case eLoggingMechanism.None:
                        {
                            break;
                        }
                    default:
                        {
                            // eLoggingMechanism.EventLog
                            try
                            {
                                CreateEventLogEntry(logMessage, entryType, strLogName);
                            }
                            catch (Exception excE)
                            {
                                logMessage += Environment.NewLine + "(Unable to log message to custom Event Log : " + Environment.NewLine + excE.Message + ")";
                            }
                            finally
                            {
                                // If this fails bubbles to outer handler
                                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
                            }
                            break;
                        }
                }
            }
            catch (Exception excE)
            {
                throw new Exception("Unable to write message to Log : " + logMessage, excE);
            }
        }

        private static void CreateEventLogEntry(string logMessage, EventLogEntryType entryType, string logName)
        {
            //////EventLog objLog = null;

            try
            {
                using (EventLog objLog = new EventLog())
                {
                    // Bind to custom Event Log, create EventSource if not already present
                    if (!EventLog.SourceExists(AppDomain.CurrentDomain.FriendlyName))
                    {
                        EventLog.CreateEventSource(AppDomain.CurrentDomain.FriendlyName, logName);
                    }
                    //////objLog = new EventLog();
                    objLog.Source = AppDomain.CurrentDomain.FriendlyName;
                    objLog.MachineName = "."; // Local machine
                    //////objLog.EnableRaisingEvents = true;
                    // Write to the Log
                    objLog.WriteEntry(logMessage, entryType);
                    ////objLog.Close();
                }
            }
            catch (Exception exc)
            {
                logMessage += Environment.NewLine + "Unable to log to custom Event Log " + logName + Environment.NewLine + exc.Message;
                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
            }
            finally
            {
                //////objLog = null;
            }
        }

        private static void CreateFileLogEntry(string logMessage, EventLogEntryType entryType, string logName)
        {
            FileStream objStream = null;
            StreamWriter objWriter = null;
            string strFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\" + logName + ".log";
            string strEventTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();

            try
            {
                // Open or Create the Log File
                if (!File.Exists(strFilePath))
                {
                    objStream = File.Create(strFilePath);
                }
                else
                {
                    objStream = new FileStream(strFilePath, FileMode.Append);
                }
                // Write to the Log
                objWriter = new StreamWriter(objStream);
                objWriter.Write(strEventTime + ":" + " ");
                objWriter.Write(entryType.ToString().ToUpper() + " ");
                objWriter.Write(logMessage + Environment.NewLine);
            }
            catch (Exception exc)
            {
                logMessage += Environment.NewLine + "Unable to log to File " + strFilePath + Environment.NewLine + exc.Message;
                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
            }
            finally
            {
                if (objWriter != null)
                {
                    objWriter.Close();
                    objWriter = null;
                }
            }
        }

        private static void CreateSQLServerLogEntry(string logMessage, EventLogEntryType entryType, string applicationName, string serviceName, SQLServerLog databaseSettings)
        {
            DataAccess objADO = null;
            ArrayList colParameters = null;
            string strMessage = logMessage;
            int intReturn = 0;
            int intOutstandingMessageCharacters = 0;

            if (applicationName == null | applicationName == "")
            {
                applicationName = AppDomain.CurrentDomain.FriendlyName;
            }

            if (databaseSettings == null)
            {
                logMessage += Environment.NewLine + "Unable to log to SQL Server as Connection information was not specified";
                CreateEventLogEntry(logMessage, EventLogEntryType.Warning, applicationName);
                return;
            }

            try
            {
                // Create Data Access instance for communicating with SQL Server
                objADO = new DataAccess(databaseSettings.ConnectionString);
                // Write to the Log
                intOutstandingMessageCharacters = logMessage.Length;
                while (intOutstandingMessageCharacters > 0)
                {
                    // Send Message in multiple calls to accommodate limit on No. of Characters
                    if (logMessage.Length > 7500)
                    {
                        strMessage = logMessage.Substring(0, 7500);
                        intOutstandingMessageCharacters = (logMessage.Length - 7500);
                    }
                    else
                    {
                        strMessage = logMessage;
                        intOutstandingMessageCharacters = 0;
                    }
                    colParameters = new ArrayList();
                    colParameters.Add(new SqlParameter("@Application", applicationName));
                    colParameters.Add(new SqlParameter("@EventLogEntryType", entryType));
                    colParameters.Add(new SqlParameter("@Message", strMessage));
                    if (serviceName != null && serviceName != string.Empty)
                    {
                        colParameters.Add(new SqlParameter("@ServiceName", serviceName));
                    }
                    intReturn = objADO.ExecuteSQL(databaseSettings.StoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                    if (intOutstandingMessageCharacters > 0)
                    {
                        logMessage = logMessage.Substring(7500, logMessage.Length - 7500);
                    }
                }
            }
            catch (Exception exc)
            {
                logMessage += Environment.NewLine + "Unable to log to SQL Server " + databaseSettings.ConnectionString + "..." + databaseSettings.StoredProcedure + Environment.NewLine + exc.Message;
                CreateEventLogEntry(logMessage, entryType, DEFAULT_EVENT_LOG);
            }
            finally
            {
                objADO = null;
                colParameters = null;
            }
        }

        private static DataAccess GetADOInstance()
        {
            DataAccess objADO = null;
            LoggingSection objSection = (LoggingSection)ConfigurationManager.GetSection("LoggingSection");
            SQLServerLog objDatabaseSettings = null;

            if (objSection != null)
            {
                objDatabaseSettings = objSection.DatabaseLog;
            }

            if (objDatabaseSettings == null)
            {
                string strLogMessage = Environment.NewLine + "Unable to communicate with SQL Server as Connection information was not specified";
                CreateEventLogEntry(strLogMessage, EventLogEntryType.Warning, GetLogName());
            }
            else
            {
                // Create Data Access instance for communicating with SQL Server
                objADO = new DataAccess(objDatabaseSettings.ConnectionString);
            }

            return objADO;
        }

        private static string GetLogName()
        {
            LoggingSection objSection = (LoggingSection)ConfigurationManager.GetSection("LoggingSection");
            string strLogName = "Application";

            if (objSection != null)
            {
                return objSection.LogName;
            }

            return strLogName;
        }


        /// <summary>
        /// NB: Different SP to Services, databaseSettings.StoredProcedureForClientLog
        /// </summary>
        /// <param name="clientLogEntry"></param>
        /// <param name="databaseSettings"></param>
        private static void CreateSQLServerLogEntry(ClientLogEntry clientLogEntry, SQLServerLog databaseSettings)
        {
            DataAccess objADO = null;
            ArrayList colParameters = null;
            string logMessage = clientLogEntry.Message;
            string strMessage = logMessage;
            string additionalInfo = null;
            int intReturn = 0;
            int intOutstandingMessageCharacters = 0;

            if (!logMessage.Trim().ToUpper().StartsWith("USER FEEDBACK"))
            {
                if (!string.IsNullOrEmpty(clientLogEntry.InnerExceptionMessage) &&
                    !logMessage.Contains(clientLogEntry.InnerExceptionMessage))
                {
                    logMessage += Environment.NewLine + clientLogEntry.InnerExceptionMessage;
                }
                string strStackTrace = Environment.NewLine + clientLogEntry.Application + " : " + clientLogEntry.ApplicationContext + "(" + clientLogEntry.MethodName + ")";
                logMessage += Environment.NewLine + "----StackTrace(s)----" + strStackTrace;
            }

            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.StoredProcedureForClientLog))
            {
                logMessage += Environment.NewLine + "Unable to log to SQL Server as Connection information was not specified";
                CreateEventLogEntry(logMessage, EventLogEntryType.Warning, clientLogEntry.Application);
                return;
            }

            try
            {
                // Create Data Access instance for communicating with SQL Server
                objADO = new DataAccess(databaseSettings.ConnectionString);
                // Write to the Log
                intOutstandingMessageCharacters = logMessage.Length;
                while (intOutstandingMessageCharacters > 0)
                {
                    // Send Message in multiple calls to accommodate limit on No. of Characters
                    if (logMessage.Length > 7500)
                    {
                        strMessage = logMessage.Substring(0, 7500);
                        intOutstandingMessageCharacters = (logMessage.Length - 7500);
                    }
                    else
                    {
                        strMessage = logMessage;
                        intOutstandingMessageCharacters = 0;
                    }

                    // Assemble Parameters
                    colParameters = new ArrayList();
                    colParameters.Add(new SqlParameter("@DateOccurred", clientLogEntry.DateOccurred));
                    colParameters.Add(new SqlParameter("@Application", clientLogEntry.Application));
                    colParameters.Add(new SqlParameter("@EventLogEntryType", clientLogEntry.EventLogEntryType));
                    colParameters.Add(new SqlParameter("@UserID", clientLogEntry.UserID));
                    colParameters.Add(new SqlParameter("@Message", strMessage));

                    if (!string.IsNullOrEmpty(clientLogEntry.MethodName))
                    {
                        colParameters.Add(new SqlParameter("@MethodName", clientLogEntry.MethodName));
                    }
                    else if (!string.IsNullOrEmpty(clientLogEntry.ApplicationContext))
                    {
                        colParameters.Add(new SqlParameter("@MethodName", clientLogEntry.ApplicationContext));
                    }
                    if (!string.IsNullOrEmpty(clientLogEntry.MachineID))
                    {
                        colParameters.Add(new SqlParameter("@MachineID", clientLogEntry.MachineID));
                    }
                    if (clientLogEntry.AdditionalInfo != null && clientLogEntry.AdditionalInfo.Count > 0)
                    {
                        for (int index = 0; index < clientLogEntry.AdditionalInfo.Count; index++)
                        {
                            additionalInfo += clientLogEntry.AdditionalInfo[index].Key + "=" + clientLogEntry.AdditionalInfo[index].Value.ToString() + ";";
                        }
                        colParameters.Add(new SqlParameter("@AdditionalInfo", additionalInfo));
                    }

                    if (!string.IsNullOrEmpty(clientLogEntry.UserFeedback))
                    {
                        colParameters.Add(new SqlParameter("@UserFeedback", clientLogEntry.UserFeedback));
                    }

                    // Execute SQL
                    intReturn = objADO.ExecuteSQL(databaseSettings.StoredProcedureForClientLog, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));

                    if (intOutstandingMessageCharacters > 0)
                    {
                        logMessage = logMessage.Substring(7500, logMessage.Length - 7500);
                    }
                }
            }
            catch (Exception exc)
            {
                logMessage += Environment.NewLine + "Unable to log to SQL Server " + databaseSettings.ConnectionString + "..." + databaseSettings.StoredProcedure + Environment.NewLine + exc.Message;
                CreateEventLogEntry(logMessage, EventLogEntryType.Error, DEFAULT_EVENT_LOG);
            }
            finally
            {
                objADO = null;
                colParameters = null;
            }
        }


        #endregion
    }

    /// <summary>
    /// Custom Configuration Section for Logging
    /// </summary>
    public sealed class LoggingSection : ConfigurationSection
    {
        // Example declaration of this Section in a Configuration file
        /**********************************************************************
	    <configSections>
		    <section name="LoggingSection"
				     type="FinalBuild.LoggingSection,SharedComponents" />
	    </configSections>

	    <LoggingSection LoggingMechanism="File" 
				        LogName="CustomLog" />
        />          
        ***********************************************************************/
        // Including SQL Server Logging
        /*
        <LoggingSection 
			LoggingMechanism="File" 
			LogName="BuyNet">
		    <DatabaseLog 
			ConnectionString="DataSource=WXSQLDEV001;InitialCatalog=Logging;"
			StoredProcedure="insToLog" />
	    </LoggingSection> 
        */

        #region Public Properties

        [ConfigurationProperty("LoggingMechanism", DefaultValue = "EventLogAndSQLServer", IsRequired = true, IsKey = false)]
        public eLoggingMechanism LoggingMechanism
        {

            get
            {
                return (eLoggingMechanism)this["LoggingMechanism"];
            }
            set
            {
                this["LoggingMechanism"] = value.ToString();
            }

        }

        [ConfigurationProperty("LogName", DefaultValue = "Application", IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string LogName
        {

            get
            {
                return (string)this["LogName"];
            }
            set
            {
                this["LogName"] = value;
            }

        }

        [ConfigurationProperty("DatabaseLog", IsRequired = true, IsKey = false)]
        public SQLServerLog DatabaseLog
        {

            get
            {
                return (SQLServerLog)this["DatabaseLog"];
            }
            set
            {
                this["DatabaseLog"] = value.ToString();
            }

        }

        [ConfigurationProperty("ServiceName", IsRequired = false, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 0, MaxLength = 100)]
        public string ServiceName
        {

            get
            {
                return (string)this["ServiceName"];
            }
            set
            {
                this["ServiceName"] = value;
            }

        }

        #endregion

    }

    /// <summary>
    /// Custom Configuration Element for Logging
    /// </summary>
    public sealed class SQLServerLog : ConfigurationElement
    {
        // Example declaration of this section in a Configuration file
        /**********************************************************************
	    <configSections>
		    <section name="LoggingSection"
				     type="BusinessObjects.LoggingSection,SharedComponents" />
	    </configSections>

	    <LoggingSection LoggingMechanism="File" 
				        LogName="CustomLog" />          
        ***********************************************************************/

        #region Public Properties

        [ConfigurationProperty("ConnectionString",
                                DefaultValue = "Data Source=WXSQLDEV001;Initial Catalog=BTBuyNet;workstation id=CS;packet size=4096;integrated security=SSPI;persist security info=False",
                                IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = "~!@#$%^&*(){}", MinLength = 1, MaxLength = 255)]
        public string ConnectionString
        {

            get
            {
                return (string)this["ConnectionString"];
            }
            set
            {
                this["ConnectionString"] = value;
            }

        }

        [ConfigurationProperty("StoredProcedure", DefaultValue = "insLogEntry", IsRequired = true, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string StoredProcedure
        {

            get
            {
                return (string)this["StoredProcedure"];
            }
            set
            {
                this["StoredProcedure"] = value;
            }

        }

        [ConfigurationProperty("StoredProcedureForClientLog", DefaultValue = "insClientLogEntry", IsRequired = false, IsKey = false)]
        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\", MinLength = 1, MaxLength = 60)]
        public string StoredProcedureForClientLog
        {

            get
            {
                return (string)this["StoredProcedureForClientLog"];
            }
            set
            {
                this["StoredProcedureForClientLog"] = value;
            }

        }

        #endregion

    }

    [Serializable]
    [System.Xml.Serialization.XmlType(Namespace = "http://FinalBuild.co.uk/FinalBuild")]
    public class ClientLogEntry
    {
        public EventLogEntryType EventLogEntryType;
        public string UserID;
        public string MachineID;
        public string Message;
        public string Application;
        public DateTime DateOccurred;
        public string InnerExceptionMessage;
        public string ApplicationContext;
        public string MethodName;

        public BusinessObjects.SerializableHashTable AdditionalInfo;
        public string UserFeedback;
    }

    [Serializable]
    [System.Xml.Serialization.XmlType(Namespace="http://FinalBuild.co.uk/FinalBuild")]
    public class ClientLogEntryCollection : System.Collections.Generic.List<ClientLogEntry> { }

}

