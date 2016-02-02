#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BusinessObjects.WorkManagement
{
	#region CommentAuditRecordCollection class

	public partial class CommentAuditRecordCollection
    {
        #region Public Methods

        public bool Contains(DateTime changeDate, string changeUser, bool isEngineerComment, string text)
		{
			foreach (CommentAuditRecord commentAuditRecord in this)
			{
				if (
					commentAuditRecord.ChangeDate.Equals(changeDate)
					&& commentAuditRecord.ChangeUser == null ? string.Empty.Equals(changeUser == null ? string.Empty : changeUser) : commentAuditRecord.ChangeUser.Equals(changeUser == null ? string.Empty : changeUser)
					&& commentAuditRecord.IsEngineerComment.Equals(isEngineerComment) 
					&& commentAuditRecord.Text.Equals(text==null?string.Empty:text)
					)
				{
					return true;
				}
			}
			return false;
        }

        /// <summary>
        /// Converts all comments to a single string.
        /// </summary>
        /// <returns>Comments ToString()</returns>
        public string ToString(bool includeDateAndUser, bool includeDelimiters)
        {
            string commentsString = string.Empty;

            // Convert to comment Strings for Display purposes
            if (this != null && this.Count > 0)
            {
                CommentAuditRecordCollection telemetryComments = new CommentAuditRecordCollection();
                CommentAuditRecordCollection nonTelemetryComments = new CommentAuditRecordCollection();

                foreach (CommentAuditRecord commentRecord in this)
                {
                    switch (commentRecord.Type)
                    {
                        case eCommentType.Telemetry:
                            {
                                telemetryComments.Add(commentRecord);
                                break;
                            }
                        default:
                            {
                                nonTelemetryComments.Add(commentRecord);
                                break;
                            }
                    }
                }

                if (telemetryComments.Count > 0)
                {
                    // Order Comments by SortExpression (as this = AlarmLevel)
                    telemetryComments.Sort("SortExpression", System.ComponentModel.ListSortDirection.Descending);
                }
                if (nonTelemetryComments.Count > 0)
                {
                    // Order Comments with most recent displayed first
                    nonTelemetryComments.Sort("ChangeDate", System.ComponentModel.ListSortDirection.Ascending);
                }

                // Build formatted CommentsString containing all the Comments in the collection
                for (int intIndex = 0; intIndex < telemetryComments.Count; intIndex++)
                {
                    commentsString += telemetryComments[intIndex].ToString(includeDateAndUser);
                    if (includeDelimiters)
                    {
                        commentsString += Environment.NewLine;
                        commentsString += "--------------------------------------";
                        commentsString += Environment.NewLine;
                    }
                }
                for (int intIndex = 0; intIndex < nonTelemetryComments.Count; intIndex++)
                {
                    commentsString += nonTelemetryComments[intIndex].ToString(includeDateAndUser);
                    if (includeDelimiters)
                    {
                        commentsString += Environment.NewLine;
                        commentsString += "--------------------------------------";
                        commentsString += Environment.NewLine;
                    }
                }
            }

            return commentsString;
        }

        public string ToString()
        {
            return ToString(true, true);
        }

        /// <summary>
        /// Derive overall Alarm Level for the Request by iterating constituent Alarms to find the highest Level
        /// </summary>
        public DateTime GetEarliestChangeDate(bool ignoreItemsWithEmptySortExpression)
        {
            DateTime earliestChangeDate = DateTime.Now;

            foreach (CommentAuditRecord commentRecord in this)
            {
                if (ignoreItemsWithEmptySortExpression &&
                   (commentRecord.SortExpression != null && commentRecord.SortExpression != string.Empty))
                {
                    if (commentRecord.ChangeDate < earliestChangeDate)
                    {
                        earliestChangeDate = commentRecord.ChangeDate;
                    }
                }
                else
                {
                    if (commentRecord.ChangeDate < earliestChangeDate)
                    {
                        earliestChangeDate = commentRecord.ChangeDate;
                    }
                }
            }

            return earliestChangeDate;
        }

        public int Save(int jobID, eWMSourceSystem sourceSystem)
        {
            return Save(jobID, sourceSystem, false);
        }

        public int Save(int jobID, eWMSourceSystem sourceSystem, bool informEngineers)
        {
            foreach (CommentAuditRecord member in this)
            {
                member.Save(jobID, sourceSystem, informEngineers);
            }

            return (this.Count);
        }

		#endregion
	}

	#endregion

    public partial class CommentAuditRecord
    {
        #region Constructors

        public CommentAuditRecord(string text, string changeUser)
        {
            this.ChangeDate = DateTime.Now;
            this.ChangeUser = changeUser;
            this.Text = text;
        }

        public CommentAuditRecord()
        {
            // Parameterless Constructor required for Serialization purposes
        }

        #endregion

        #region Public Methods

        public string ToString(bool includeDateAndUser)
        {
            string commentsString = string.Empty;

            // Build formatted CommentsString
            if (includeDateAndUser)
            {
                commentsString = mdteChangeDate.ToShortDateString();
                commentsString += " " + mdteChangeDate.ToLongTimeString();
                if (mstrChangeUser.IndexOf(@"\") > -1)
                {
                    commentsString += " " + mstrChangeUser.Substring(mstrChangeUser.IndexOf(@"\") + 1);
                }
                else
                {
                    commentsString += " " + mstrChangeUser;
                }
                commentsString += " " + mstrText;
            }
            else
            {
                commentsString = mstrText;
            }


            return commentsString;
        }

        public static string FormatCommentToString(CommentAuditRecord comment)
        {
            string commentsString = string.Empty;

            if (comment.ChangeDate == DateTime.MinValue)
            {
                commentsString = "[Unspec time]";
            }
            else
            {
                commentsString = comment.ChangeDate.ToShortDateString();
                commentsString += " " + comment.ChangeDate.ToLongTimeString();
            }
            if (comment.ChangeUser != null)
            {
                if (comment.ChangeUser.IndexOf(@"\") > -1)
                {
                    commentsString += " " + comment.ChangeUser.Substring(comment.ChangeUser.IndexOf(@"\") + 1);
                }
                else
                {
                    commentsString += " " + comment.ChangeUser;
                }
            }
            commentsString += " " + comment.Text;

            return commentsString;
        }

        public override string ToString()
        {
            return ToString(true);
        }

        #endregion
    }
}
