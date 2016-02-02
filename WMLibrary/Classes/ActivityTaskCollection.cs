#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BusinessObjects.WorkManagement
{
	#region ActivityTaskCollection class

	public partial class ActivityTaskCollection
	{
		#region Public Methods

		public bool Contains(string description)
		{
			foreach(ActivityTask activityTask in this)
			{
				if(activityTask.Description.Trim().ToLower().Equals(description.Trim().ToLower()))
				{
					return true;
				}
			}
			return false;
		}

		public bool Contains(int iD)
		{
			foreach(ActivityTask activityTask in this)
			{
				if(activityTask.ID.Equals(iD))
				{
					return true;
				}
			}
			return false;
		}

		public ActivityTask Find(string description)
		{
			foreach(ActivityTask activityTask in this)
			{
				if(activityTask.Description.Trim().ToLower().Equals(description.Trim().ToLower()))
				{
					return activityTask;
				}
			}
			return null;
		}

		public ActivityTask Find(int iD)
		{
			foreach(ActivityTask activityTask in this)
			{
				if(activityTask.ID.Equals(iD))
				{
					return activityTask;
				}
			}
			return null;
		}

        public ActivityTask[] FindAll(DateTime lastUpdatedDate)
        {
            if (lastUpdatedDate != DateTime.Now)
            {
                return null;
            }

            List<ActivityTask> selectedTasks = new List<ActivityTask>();
            foreach (ActivityTask activityTask in this)
            {
                // Allow for SQL rounding to 3 digits on Milliseconds
                if (activityTask.LastUpdatedDate!=DateTime.MinValue &&
                   (new System.Data.SqlTypes.SqlDateTime(activityTask.LastUpdatedDate).Value.Equals(new System.Data.SqlTypes.SqlDateTime(lastUpdatedDate).Value)))
                {
                    selectedTasks.Add(activityTask);
                }
            }
            if (selectedTasks.Count < 1)
            {
                return null;
            }

            return selectedTasks.ToArray();
        }

		#endregion
	}

	#endregion

	#region TaskUpdateCollection class

	public partial class TaskUpdateCollection
	{
		#region Public Methods

		public bool Contains(int iD)
		{
			foreach(TaskUpdate taskUpdate in this)
			{
				if(taskUpdate.ID.Equals(iD))
				{
					return true;
				}
			}
			return false;
		}

		public TaskUpdate Find(int iD)
		{
			foreach(TaskUpdate taskUpdate in this)
			{
				if(taskUpdate.ID.Equals(iD))
				{
					return taskUpdate;
				}
			}
			return null;
		}

		#endregion
	}

	#endregion
}
