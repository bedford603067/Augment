using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class ActivityController : Controller
    {
        IntegrationWebSiteMvc.Models.WMSMetadata _modelContext = new Models.WMSMetadata();
        //
        // GET: /Activity/

        public ActionResult Index()
        {
            return View(_modelContext.Activities);
        }

        //
        // GET: /Activity/Tasks/5

        public ActionResult Tasks(int id)
        {
            BusinessObjects.WorkManagement.ActivityTaskCollection activityTasks = null;

            ViewBag.ActivityID = id;
            ViewBag.Tasks = _modelContext.GetTasks(); // NB: .Tasks being passed through for the _TaskCreate PartialView to use.
            ViewBag.ActivityMandatoryTaskDescription = _modelContext.ActivityMandatoryTaskDescription;

            activityTasks = _modelContext.GetActivityTasks(id);
            if (activityTasks == null || activityTasks.Count == 0)
            {
                // Include the mandantory Task
                BusinessObjects.WorkManagement.ActivityTaskTemplate mandatoryTask = _modelContext.ActivityMandatoryTask;
                if (mandatoryTask != null)
                {
                    _modelContext.AddTaskToActivity(mandatoryTask.ID, id);
                    activityTasks = _modelContext.GetActivityTasks(id);
                }
                else
                {
                    throw new Exception("Unable to add mandatory Task to the Activity.Tasks collection");
                }
            }

            return View(activityTasks);
        }

        //
        // GET: /Activity/Edit/5
        public ActionResult EditOrCreate(int id)
        {
            BusinessObjects.WorkManagement.Activity activity = null;
            BusinessObjects.WorkManagement.PriorityLookupData lookupData = _modelContext.GetPriorityLookupData();

            ViewBag.ActivityID = id;
            ViewBag.SchedulingPriorities = new List<BusinessObjects.WorkManagement.SchedulingPriority>(lookupData.Scheduling);
            ViewBag.DispatchPriorities = new List<BusinessObjects.WorkManagement.DispatchPriority>(lookupData.Dispatch);

            if (id > 0)
            {
                activity = _modelContext.GetActivity(id);
            }
            else
            {
                activity = new BusinessObjects.WorkManagement.Activity();
                activity.SchedulingPriority = new BusinessObjects.WorkManagement.SchedulingPriority();
                activity.DispatchPriority = new BusinessObjects.WorkManagement.DispatchPriority();
                activity.ParentCategoryID = _modelContext.DefaultActivityCategoryID;
                activity.DistrictGroup = _modelContext.DefaultActivityDistrictGroup;
            }

            return View(activity);
        }

        //
        // POST: /Activity/Edit/5

        [HttpPost]
        public ActionResult EditOrCreate(BusinessObjects.WorkManagement.Activity activity)
        {
            try
            {
                // These flags controlling whether Activity is shown to all Users or not.
                activity.IsReactive = true;
                activity.IsVisibleInClick = true;
                activity.IsVisibleInField = true;

                // Save 
                _modelContext.SaveActivity(activity);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Activity/Delete/5

        public ActionResult Delete(int id)
        {
            return RedirectToAction("Index");
        }

        //
        // POST: /Activity/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult RemoveTask(int id, int activityID)
        {
            _modelContext.RemoveTask(id, activityID);

            return RedirectToAction("Tasks", new {id=activityID});
        }

        [HttpPost]
        public ActionResult _TaskCreate(BusinessObjects.WorkManagement.ActivityTask model)
        {
            if (ModelState.IsValid)
            {
                // Saving data .....
                _modelContext.AddTaskToActivity(model.ID, model.TemplateID); // NB: ActivityID is being stored in TemplateID for convenience when creating new
            }
            else
            {
                // Show Server Validation Errors
                return PartialView();
            }

            return Redirect(Request.UrlReferrer.PathAndQuery);
        }

        public ActionResult LookupData(int id)
        {
            ViewBag.ActivityID = id;

            return View();
        }

        public ActionResult Skills()
        {
            BusinessObjects.WorkManagement.SkillCollection skills = _modelContext.GetSkills();

            return View(skills);
        }

        public ActionResult EditOrCreateSkill(string code)
        {
            BusinessObjects.WorkManagement.Skill skill = null;

            if (!string.IsNullOrEmpty(code))
            {
                BusinessObjects.WorkManagement.SkillCollection skills = _modelContext.GetSkills();
                if (skills != null)
                {
                    int index = skills.Find("Code", code);
                    if (index < 0)
                    {
                        throw new Exception(string.Format("Unable to find Skill with Code {0}", code));
                    }
                    skill = skills[index];
                }
            }
            else
            {
                skill = new BusinessObjects.WorkManagement.Skill();
            }

            return View(skill);
        }

        [HttpPost]
        public ActionResult EditOrCreateSkill(BusinessObjects.WorkManagement.Skill model)
        {
            try
            {
                _modelContext.SaveSkill(model);

                return RedirectToAction("Skills");
            }
            catch
            {
                return View();
            }
        }
    }
}
