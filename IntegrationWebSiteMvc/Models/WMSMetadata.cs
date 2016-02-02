using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;

namespace IntegrationWebSiteMvc.Models
{
    public class WMSMetadata
    {
        public string ActivityMandatoryTaskDescription
        {
            get
            {
                string activityMandatoryTaskDescription = "Attend job and complete feedback";

                if (ConfigurationManager.AppSettings["ActivityMandatoryTaskDescription"] != null &&
                   !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ActivityMandatoryTaskDescription"]))
                {
                    activityMandatoryTaskDescription = ConfigurationManager.AppSettings["ActivityMandatoryTaskDescription"];
                }

                return activityMandatoryTaskDescription;
            }
        }
        public BusinessObjects.WorkManagement.ActivityTaskTemplate ActivityMandatoryTask
        {
            get
            {
                BusinessObjects.WorkManagement.ActivityTaskTemplate mandatoryTask = null;
                Services.ActivityTaskTemplateCollection tasks = GetTasks();
                if (tasks != null)
                {
                    mandatoryTask = tasks.Find(t => t.Description == ActivityMandatoryTaskDescription);
                }

                return mandatoryTask;
            }
        }
        public int DefaultActivityCategoryID
        {
            get
            {
                int defaultActivityCategoryID = 1057;
                if (ConfigurationManager.AppSettings["DefaultActivityCategoryID"] != null)
                {
                    int.TryParse(ConfigurationManager.AppSettings["DefaultActivityCategoryID"], out defaultActivityCategoryID);
                }
                return defaultActivityCategoryID;
            }
        }
        public string DefaultActivityDistrictGroup
        {
            get
            {
                string defaultActivityDistrictGroup = "OPS";
                if (ConfigurationManager.AppSettings["DefaultActivityDistrictGroup"] != null &&
                    !string.IsNullOrEmpty(ConfigurationManager.AppSettings["DefaultActivityDistrictGroup"]))
                {
                    defaultActivityDistrictGroup = ConfigurationManager.AppSettings["DefaultActivityDistrictGroup"];
                }
                return defaultActivityDistrictGroup;
            }
        }

        public Services.ActivityCollection Activities
        {
            get
            {
                Services.ActivityCollection activities = null;
                
                Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
                activities = serviceProxy.GetActivitiesForMobile();
                serviceProxy.Close();

                return activities;
            }
        }

        public BusinessObjects.WorkManagement.ActivityTaskCollection GetActivityTasks(int activityID)
        {
            Services.ActivityCollection activities = this.Activities;
            BusinessObjects.WorkManagement.Activity activity = null;

            activity = activities.Find(new Predicate<BusinessObjects.WorkManagement.Activity>
            (
                new SearchByIDClass(activityID).PredicateDelegate)
            );

            if(activity != null)
            {
                return activity.Tasks;
            }

            return null;
        }

        public BusinessObjects.WorkManagement.Activity GetActivity(int activityID)
        {
            Services.ActivityCollection activities = this.Activities;
            BusinessObjects.WorkManagement.Activity activity = null;

            activity = activities.Find(new Predicate<BusinessObjects.WorkManagement.Activity>
            (
                new SearchByIDClass(activityID).PredicateDelegate)
            );

            if (activity != null)
            {
                return activity;
            }

            return null;
        }

        public class SearchByIDClass
        {
            int mintID;

            public SearchByIDClass(int id)
            {
                this.mintID = id;
            }

            public bool PredicateDelegate(BusinessObjects.WorkManagement.Activity member)
            {
                return member.ID == mintID;
            }
        }

        public Services.ActivityTaskTemplateCollection GetTasks()
        {
            Services.ActivityTaskTemplateCollection tasks = null;
            
            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            tasks = serviceProxy.GetTaskLibrary();
            serviceProxy.Close();

            return tasks;
        }
        public bool SaveTask(BusinessObjects.WorkManagement.ActivityTaskTemplate task)
        {
            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            int rowsAffected = serviceProxy.SaveTaskTemplate(task,0);
            serviceProxy.Close();

            return (rowsAffected > 0);
        }
        public int SaveActivity(BusinessObjects.WorkManagement.Activity activity)
        {
            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            int activityID = serviceProxy.SaveActivity(activity);
            serviceProxy.Close();

            return activityID;
        }

        public bool RemoveTask(int taskID, int activityID)
        {
            bool result = true;

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            result = serviceProxy.RemoveTasksFromActivity(taskID.ToString(), activityID);
            serviceProxy.Close();

            return result;
        }

        public bool AddTaskToActivity(int taskID, int activityID)
        {
            bool result = true;

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            serviceProxy.AddTasksToActivity(taskID.ToString(), activityID);
            serviceProxy.Close();

            return result;
        }

        public BusinessObjects.WorkManagement.PriorityLookupData GetPriorityLookupData()
        {
            BusinessObjects.WorkManagement.PriorityLookupData lookupData = null;

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            lookupData = serviceProxy.GetPriorityLookupData();
            serviceProxy.Close();

            return lookupData;

            /*
            -- Test Data
            List<BusinessObjects.WorkManagement.SchedulingPriority> categories = new List<BusinessObjects.WorkManagement.SchedulingPriority>();
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 0, Description = "Same Day" });
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 1, Description = "2 Hours" });
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 2, Description = "4 Hours" });
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 3, Description = "6 Hours" });
            return categories;
            */

        }

        public List<BusinessObjects.WorkManagement.SchedulingPriority> GetSchedulingPriorityOptions()
        {
            BusinessObjects.WorkManagement.PriorityLookupData lookupData = null;

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            lookupData = serviceProxy.GetPriorityLookupData();
            serviceProxy.Close();

            return new List<BusinessObjects.WorkManagement.SchedulingPriority>(lookupData.Scheduling);

            /*
            -- Test Data
            List<BusinessObjects.WorkManagement.SchedulingPriority> categories = new List<BusinessObjects.WorkManagement.SchedulingPriority>();
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 0, Description = "Same Day" });
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 1, Description = "2 Hours" });
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 2, Description = "4 Hours" });
            categories.Add(new BusinessObjects.WorkManagement.SchedulingPriority() { ID = 3, Description = "6 Hours" });
            return categories;
            */

        }

        public List<BusinessObjects.WorkManagement.DispatchPriority> GetDispatchPriorityOptions()
        {
            BusinessObjects.WorkManagement.PriorityLookupData lookupData = null;

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            lookupData = serviceProxy.GetPriorityLookupData();
            serviceProxy.Close();

            return new List<BusinessObjects.WorkManagement.DispatchPriority>(lookupData.Dispatch);

            /*
            -- Test Data
            List<BusinessObjects.WorkManagement.DispatchPriority> categories = new List<BusinessObjects.WorkManagement.DispatchPriority>();
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 1, Description = "Level 1 (Highest)" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 2, Description = "Level 2" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 3, Description = "Level 3" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 4, Description = "Level 4" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 5, Description = "Level 5 (Lowest)" });
            return categories;
            */
        }

        public BusinessObjects.WorkManagement.SkillCollection GetSkills()
        {
            BusinessObjects.WorkManagement.TaskLookupData lookupData = null;

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            lookupData = serviceProxy.GetTaskLookupData();
            serviceProxy.Close();

            return lookupData.Skills;

            /*
            -- Test Data
            List<BusinessObjects.WorkManagement.DispatchPriority> categories = new List<BusinessObjects.WorkManagement.DispatchPriority>();
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 1, Description = "Level 1 (Highest)" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 2, Description = "Level 2" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 3, Description = "Level 3" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 4, Description = "Level 4" });
            categories.Add(new BusinessObjects.WorkManagement.DispatchPriority() { ID = 5, Description = "Level 5 (Lowest)" });
            return categories;
            */
        }

        public void SaveSkill(BusinessObjects.WorkManagement.Skill skill)
        {
            BusinessObjects.WorkManagement.TaskLookupData lookupData = new BusinessObjects.WorkManagement.TaskLookupData();
            lookupData.Skills = new BusinessObjects.WorkManagement.SkillCollection();
            lookupData.Skills.Add(skill);

            Services.LookupServiceClient serviceProxy = new Services.LookupServiceClient();
            serviceProxy.SaveLookupData(lookupData);
            serviceProxy.Close();
        }
    }
}