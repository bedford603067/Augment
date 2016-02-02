using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;
using System.Collections;
using System.Data.SqlClient;
using System.Data;

namespace FinalBuild
{
    public abstract class UserMessageWriter
    {
        public static void WriteToDataStore(UserMessage userMessage)
        {
            UserMessagingSection objSection = (UserMessagingSection)ConfigurationManager.GetSection("UserMessagingSection");
            SQLServerDB databaseSettings = null;
            DataAccess objADO = null;
            ArrayList colParameters = null;
            string strMessage = userMessage.Body;
            int intReturn = 0;
            int intOutstandingMessageCharacters = 0;

            if (objSection != null)
            {
                databaseSettings = objSection.DataStore;
            }

            if (databaseSettings == null)
            {
                throw new Exception("Unable to write User Message to SQL Server as Connection information was not specified");
            }

            try
            {
                // Create Data Access instance for communicating with SQL Server
                objADO = new DataAccess(databaseSettings.ConnectionString);
                // Write to the Log
                intOutstandingMessageCharacters = userMessage.Body.Length;
                while (intOutstandingMessageCharacters > 0)
                {
                    // Send Message in multiple calls to accommodate limit on No. of Characters
                    if (userMessage.Body.Length > 7500)
                    {
                        strMessage = userMessage.Body.Substring(0, 7500);
                        intOutstandingMessageCharacters = (userMessage.Body.Length - 7500);
                    }
                    else
                    {
                        strMessage = userMessage.Body;
                        intOutstandingMessageCharacters = 0;
                    }
                    colParameters = new ArrayList();
                    colParameters.Add(new SqlParameter("@RecipientID", userMessage.Recipient));
                    colParameters.Add(new SqlParameter("@Body", userMessage.Body));
                    if (!string.IsNullOrEmpty(userMessage.Sender))
                    {
                        colParameters.Add(new SqlParameter("@ReplyAddress", userMessage.Sender));
                    }
                    if (!string.IsNullOrEmpty(userMessage.Subject))
                    {
                        colParameters.Add(new SqlParameter("@Subject", userMessage.Subject));
                    }
                    if (!string.IsNullOrEmpty(userMessage.TargetUrl))
                    {
                        colParameters.Add(new SqlParameter("@TargetUrl", userMessage.TargetUrl));
                    }
                    if (userMessage.ExpiryMinutesAfterCreation > -1)
                    {
                        colParameters.Add(new SqlParameter("@ExpiryMinutesAfterCreation", userMessage.ExpiryMinutesAfterCreation));
                    }
                    if (userMessage.ExpiryMinutesAfterSent > -1)
                    {
                        colParameters.Add(new SqlParameter("@ExpiryMinutesAfterCreation", userMessage.ExpiryMinutesAfterSent));
                    }
                    if (userMessage.PriorityLevel > 0)
                    {
                        colParameters.Add(new SqlParameter("@PriorityLevel", userMessage.PriorityLevel));
                    }
                    //workitem #36: Alert Escalation
                    if (userMessage.AlertID > 0)
                    {
                        colParameters.Add(new SqlParameter("@AlertID", userMessage.AlertID));
                    }

                    intReturn = objADO.ExecuteSQL(databaseSettings.StoredProcedure, (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
                    if (intOutstandingMessageCharacters > 0)
                    {
                        string charactersNotWritten = userMessage.Body.Substring(7500, userMessage.Body.Length - 7500);
                    }
                }
            }
            catch (Exception excE)
            {
                throw new Exception("Unable to write User Message to SQL Server as Connection information was not specified", excE);
            }
            finally
            {
                objADO = null;
                colParameters = null;
            }
        }

        public static UserMessageCollection GetMessages(string recipientID)
        {
            UserMessageCollection colMessages = null;
            DataTable dtMessages = new DataTable();
            UserMessagingSection objSection = (UserMessagingSection)ConfigurationManager.GetSection("UserMessagingSection");
            SQLServerDB objDatabaseSettings = null;
            DataAccess objADO = null;
            ArrayList colParameters = new ArrayList();
            SqlParameter[] arrParameters = null;

            try
            {
                if (objSection != null)
                {
                    objDatabaseSettings = objSection.DataStore;
                }

                if (objDatabaseSettings == null)
                {
                    throw new Exception("Unable to retrieve User Messages from SQL Server as Connection information was not specified");
                }

                objADO = new DataAccess(objDatabaseSettings.ConnectionString);
                colParameters.Add(new SqlParameter("@RecipientID", recipientID));
                arrParameters = (SqlParameter[])colParameters.ToArray(typeof(SqlParameter));
                dtMessages = objADO.GetDataTable("selUserMessages", "UserMessages", arrParameters);
                if (dtMessages.Rows.Count > 0)
                {
                    colMessages = UserMessageCollection.Populate(dtMessages);
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

            return colMessages;
        }

        public static bool ConfirmMessageReceipt(int messageID, DateTime dateReceived)
        {
            UserMessagingSection objSection = (UserMessagingSection)ConfigurationManager.GetSection("UserMessagingSection");
            SQLServerDB databaseSettings = null;
            DataAccess objADO = null;
            ArrayList colParameters = null;
            int intRowsAffected = 0;

            if (objSection != null)
            {
                databaseSettings = objSection.DataStore;
            }

            if (databaseSettings == null)
            {
                throw new Exception("Unable to write User Message to SQL Server as Connection information was not specified");
            }

            try
            {
                // Create Data Access instance for communicating with SQL Server
                objADO = new DataAccess(databaseSettings.ConnectionString);
                // Write to the Log
                colParameters = new ArrayList();
                colParameters.Add(new SqlParameter("@UserMessageID", messageID));
                colParameters.Add(new SqlParameter("@DateReceived", dateReceived));
                intRowsAffected = objADO.ExecuteSQL("updUserMessageOnReceipt", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            catch (Exception excE)
            {
                throw new Exception("Unable to write User Message to SQL Server as Connection information was not specified", excE);
            }
            finally
            {
                objADO = null;
                colParameters = null;
            }

            return (intRowsAffected > 0);
        }

        public static bool UpdateMessageExpiryStatus(int messageID, bool isExpired)
        {
            return UpdateMessageExpiryStatus(messageID, isExpired, false);
        }
        public static bool UpdateMessageExpiryStatus(int messageID, bool isExpired, bool requiresClientConfirmation)
        {
            UserMessagingSection objSection = (UserMessagingSection)ConfigurationManager.GetSection("UserMessagingSection");
            SQLServerDB databaseSettings = null;
            DataAccess objADO = null;
            ArrayList colParameters = null;
            int intRowsAffected = 0;

            if (objSection != null)
            {
                databaseSettings = objSection.DataStore;
            }

            if (databaseSettings == null)
            {
                throw new Exception("Unable to write User Message to SQL Server as Connection information was not specified");
            }

            try
            {
                // Create Data Access instance for communicating with SQL Server
                objADO = new DataAccess(databaseSettings.ConnectionString);
                // Write to the Log
                colParameters = new ArrayList();
                colParameters.Add(new SqlParameter("@UserMessageID", messageID));
                colParameters.Add(new SqlParameter("@IsExpired", isExpired));
                colParameters.Add(new SqlParameter("@RequiresClientConfirmation", requiresClientConfirmation));
                intRowsAffected = objADO.ExecuteSQL("updUserMessageExpiryStatus", (SqlParameter[])colParameters.ToArray(typeof(SqlParameter)));
            }
            catch (Exception excE)
            {
                throw new Exception("Unable to write User Message to SQL Server as Connection information was not specified", excE);
            }
            finally
            {
                objADO = null;
                colParameters = null;
            }

            return (intRowsAffected > 0);
        }
    }

    public sealed class UserMessagingSection : ConfigurationSection
    {
        // Example declaration of this Section in a Configuration file
        /**********************************************************************
	    <configSections>
		    <section name="UserMessagingSection"
				     type="FinalBuild.UserMessagingSection,SharedComponents" />
	    </configSections>

        <UserMessagingSection>
		    <DataSource 
			ConnectionString="DataSource=WXSQLDEV001;InitialCatalog=Logging;"
			StoredProcedure="insUserMessage" />
	    </UserMessagingSection> 
        */

        #region Public Properties

        [ConfigurationProperty("DataStore", IsRequired = true, IsKey = false)]
        public SQLServerDB DataStore
        {

            get
            {
                return (SQLServerDB)this["DataStore"];
            }
            set
            {
                this["DataStore"] = value.ToString();
            }

        }

        #endregion

    }

    public sealed class SQLServerDB : ConfigurationElement
    {
        // Example declaration of this Section in a Configuration file
        /**********************************************************************
	    <configSections>
		    <section name="UserMessagingSection"
				     type="FinalBuild.UserMessagingSection,SharedComponents" />
	    </configSections>

        <UserMessagingSection>
		    <DataSource 
			ConnectionString="DataSource=WXSQLDEV001;InitialCatalog=Logging;"
			StoredProcedure="insUserMessage" />
	    </UserMessagingSection> 
        */

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

        #endregion

    }

    public partial class UserMessageCollection
    {
        public static UserMessageCollection Populate(DataTable userMessageData)
        {
            UserMessageCollection colMessages = null;
            UserMessage objMessage = null;

            if (userMessageData.Rows.Count > 0)
            {
                colMessages = new UserMessageCollection();
                foreach (DataRow drMessage in userMessageData.Rows)
                {
                    objMessage = new UserMessage();
                    objMessage.ID = (int)drMessage["ID"];
                    objMessage.Recipient = drMessage["RecipientID"].ToString();
                    objMessage.Body = drMessage["Body"].ToString();
                    objMessage.DateCreated = (DateTime)drMessage["DateCreated"];
                    if (!drMessage["Subject"].Equals(DBNull.Value))
                    {
                        objMessage.Subject = drMessage["Subject"].ToString();
                    }
                    if (!drMessage["DateReceived"].Equals(DBNull.Value))
                    {
                        objMessage.DateReceived = (DateTime)drMessage["DateReceived"];
                    }
                    if (!drMessage["TargetUrl"].Equals(DBNull.Value))
                    {
                        objMessage.TargetUrl = drMessage["TargetUrl"].ToString();
                    }
                    if (!drMessage["ReplyAddress"].Equals(DBNull.Value))
                    {
                        objMessage.Sender = drMessage["ReplyAddress"].ToString();
                    }
                    if (!drMessage["ExpiryMinutesAfterCreation"].Equals(DBNull.Value))
                    {
                        objMessage.ExpiryMinutesAfterCreation = (int)drMessage["ExpiryMinutesAfterCreation"];
                    }
                    if (!drMessage["ExpiryMinutesAfterSent"].Equals(DBNull.Value))
                    {
                        objMessage.ExpiryMinutesAfterSent = (int)drMessage["ExpiryMinutesAfterSent"];
                    }
                    if (!drMessage["PriorityLevel"].Equals(DBNull.Value))
                    {
                        objMessage.PriorityLevel = (int)drMessage["PriorityLevel"];
                    }

                    //Workitem #36: Alert Escalation
                    objMessage.AlertID = (int)drMessage["AlertID"];

                    colMessages.Add(objMessage);
                }
            }
            return colMessages;
        }
    }
}
