using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    [HandleError] // All unhandled exceptions in this Controller will bubble up to be caught by the "out of the box" handling mechanism.
    public class UsersController : Controller
    {
        IntegrationWebSiteMvc.Models.PersonnelContext _modelContext = new Models.PersonnelContext();
        
        public ActionResult Index()
        {
            // return View(BusinessObjects.WorkManagement.WorkerCollection.GetTypedWorkers("Surname"));

            System.Data.Entity.DbSet<IntegrationWebSiteMvc.Models.Employee> employees = _modelContext.Employees;
            return View(employees.ToList()); 
            // NB: see http://stackoverflow.com/questions/7927990/entity-framework-there-is-already-an-open-datareader-associated-with-this-comma
            // as to why using ToList() or ToArray() is important according to whether Lazy or eager loading child properties which are Complex Types
        }
        public ActionResult EditOrCreate(string id)
        {
            IntegrationWebSiteMvc.Models.Employee newInstance = null;

            if (!string.IsNullOrEmpty(id))
            {
                newInstance = _modelContext.Employees.Find(id);
            }
            else
            {
                newInstance = new Models.Employee();                
            }

            if (newInstance.AreaDetails == null)
            {
                newInstance.AreaDetails = new Models.AreaDetails();
            }

            return View(newInstance);
        }
        [HttpPost]
        public ActionResult EditOrCreate(IntegrationWebSiteMvc.Models.Employee newInstance)
        {
            try
            {
                /*
                // APPROACH 1 => Allow EF to work out what has changed by comparing to copy in DB 
                // Load current account from DB
                IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == newInstance.ADLoginID);

                // Update the properties
                _modelContext.Entry(storedInstance).CurrentValues.SetValues(newInstance);

                // Save the changes
                _modelContext.SaveChanges();
                */

                /*
                // APPROACH 2 => Assumes changes even thougth may not be any
                _modelContext.Entry(newInstance).State = EntityState.Modified;
                _modelContext.SaveChanges();
                */

                // Update Employee Data in Master DB
                IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Find(newInstance.ADLoginID);
                if (storedInstance != null)
                {
                    // newInstance.AreaDetails = _modelContext.Areas.ToList().Find(p => p.ID == newInstance.AreaDetails.ID);
                    _modelContext.Entry(storedInstance).CurrentValues.SetValues(newInstance);
                }
                else
                {
                    _modelContext.Employees.Add(newInstance);                    
                }
                _modelContext.SaveChanges();

                // Update Employee Data in non-Master DBs
                _modelContext.SynchroniseData(newInstance);

                return RedirectToAction("Index");
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        #region Skills

        public ActionResult Skills(string id)
        {
            IntegrationWebSiteMvc.Models.Employee employee = _modelContext.Employees.Find(id);
            if (employee.Skills == null)
            {
                employee.Skills = new List<BusinessObjects.WorkManagement.Skill>();
            }

            ViewBag.ADLoginID = id;
            return View(employee.Skills);
        }
        public ActionResult EditOrCreateSkill(int id, string adLoginID)
        {
            BusinessObjects.WorkManagement.Skill newInstance = null;

            if (id > 0)
            {
                newInstance = _modelContext.Employees.Find(adLoginID).Skills.ToList().Find(p => p.ID == id);
            }
            else
            {
                newInstance = new BusinessObjects.WorkManagement.Skill();
            }

            ViewBag.ADLoginID = adLoginID;
            ViewBag.Skills = new Models.WMSMetadata().GetSkills();

            return View(newInstance);
        }
        [HttpPost]
        public ActionResult EditOrCreateSkill(string code, string adLoginID)
        {
            try
            {
                /*
                // APPROACH 1 => Allow EF to work out what has changed by comparing to copy in DB 
                // Load current account from DB
                IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == newInstance.ADLoginID);

                // Update the properties
                _modelContext.Entry(storedInstance).CurrentValues.SetValues(newInstance);

                // Save the changes
                _modelContext.SaveChanges();
                */

                /*
                // APPROACH 2 => Assumes changes even though may not be any
                _modelContext.Entry(newInstance).State = EntityState.Modified;
                _modelContext.SaveChanges();
                */

                BusinessObjects.WorkManagement.Skill newInstance = new BusinessObjects.WorkManagement.Skill();
                newInstance.Code = code;

                IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == adLoginID);
                if (storedInstance != null && storedInstance.Skills != null && storedInstance.Skills.ToList().Find(p => p.Code == newInstance.Code) != null)
                {
                    _modelContext.Entry(storedInstance).CurrentValues.SetValues(newInstance);
                }
                else
                {
                    // _modelContext.Employees.Single(p => p.ADLoginID == adLoginID).Skills.Add(newInstance);
                    if (storedInstance.Skills == null)
                    {
                        storedInstance.Skills = new List<BusinessObjects.WorkManagement.Skill>();
                    }
                    newInstance.ID = storedInstance.Skills.Count + 1;
                    storedInstance.Skills.Add(newInstance);
                }
                _modelContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch(Exception excE)
            {
                throw excE;
            }
        }
        public ActionResult DeleteSkill(int id, string adLoginID)
        {
            IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == adLoginID);
            if (storedInstance != null && storedInstance.Skills != null && storedInstance.Skills.ToList().Find(p => p.ID == id) != null)
            {
                BusinessObjects.WorkManagement.Skill skill = storedInstance.Skills.ToList().Find(p => p.ID == id);
                _modelContext.Entry(skill).State = System.Data.Entity.EntityState.Deleted;
                _modelContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region PostCodes

        public ActionResult PostCodes(string id)
        {
            IntegrationWebSiteMvc.Models.Employee employee = _modelContext.Employees.Find(id);
            if (employee.PostCodeResponsibilities == null)
            {
                employee.PostCodeResponsibilities = new List<IntegrationWebSiteMvc.Models.PostCodeResponsibility>();
            }

            ViewBag.ADLoginID = id;
            return View(employee.PostCodeResponsibilities);
        }
        public ActionResult EditOrCreatePostCode(int id, string adLoginID)
        {
            IntegrationWebSiteMvc.Models.PostCodeResponsibility newInstance = null;

            if (id > 0)
            {
                newInstance = _modelContext.Employees.Find(adLoginID).PostCodeResponsibilities.ToList().Find(p => p.ID == id);
            }
            else
            {
                newInstance = new Models.PostCodeResponsibility();
            }

            ViewBag.ADLoginID = adLoginID;
            return View(newInstance);
        }
        [HttpPost]
        public ActionResult EditOrCreatePostCode(IntegrationWebSiteMvc.Models.PostCodeResponsibility newInstance, string adLoginID)
        {
            try
            {
                /*
                // APPROACH 1 => Allow EF to work out what has changed by comparing to copy in DB 
                // Load current account from DB
                IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == newInstance.ADLoginID);

                // Update the properties
                _modelContext.Entry(storedInstance).CurrentValues.SetValues(newInstance);

                // Save the changes
                _modelContext.SaveChanges();
                */

                /*
                // APPROACH 2 => Assumes changes even though may not be any
                _modelContext.Entry(newInstance).State = EntityState.Modified;
                _modelContext.SaveChanges();
                */

                IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == adLoginID);
                if (storedInstance != null && storedInstance.PostCodeResponsibilities != null && storedInstance.PostCodeResponsibilities.ToList().Find(p => p.ID == newInstance.ID) != null)
                {
                    _modelContext.Entry(storedInstance).CurrentValues.SetValues(newInstance);
                }
                else
                {
                    // _modelContext.Employees.Single(p => p.ADLoginID == adLoginID).Skills.Add(newInstance);
                    if (storedInstance.PostCodeResponsibilities == null)
                    {
                        storedInstance.PostCodeResponsibilities = new List<IntegrationWebSiteMvc.Models.PostCodeResponsibility>();
                    }
                    newInstance.ID = storedInstance.PostCodeResponsibilities.Count + 1;
                    storedInstance.PostCodeResponsibilities.Add(newInstance);
                }
                _modelContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }
        public ActionResult DeletePostCode(int id, string adLoginID)
        {
            IntegrationWebSiteMvc.Models.Employee storedInstance = _modelContext.Employees.Single(p => p.ADLoginID == adLoginID);
            if (storedInstance != null && storedInstance.PostCodeResponsibilities != null && storedInstance.PostCodeResponsibilities.ToList().Find(p => p.ID == id) != null)
            {
                IntegrationWebSiteMvc.Models.PostCodeResponsibility postCodeResponsibility = storedInstance.PostCodeResponsibilities.ToList().Find(p => p.ID == id);
                _modelContext.Entry(postCodeResponsibility).State = System.Data.Entity.EntityState.Deleted;
                _modelContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region NonAvailability

        public ActionResult NonAvailability(string id)
        {
            BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection nonAvailability = GetNonAvailability(id);
            if (nonAvailability == null)
            {
                nonAvailability = new BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection();
            }

            ViewBag.UserID = id;
            return View(nonAvailability);
        }

        public ActionResult DeleteNonAvailability(string userID, string nonAvailabilityID ) // DateTime startDate, DateTime endDate)
        {
            try
            {
                BusinessObjects.WorkManagement.EngineerNonAvailability instance = new BusinessObjects.WorkManagement.EngineerNonAvailability();
                //instance.StartDate = startDate;
                //instance.EndDate = endDate;
                instance.ID = nonAvailabilityID;

                UpdateNonAvailability(userID, instance, true);

                return RedirectToAction("NonAvailability", new { id = userID });
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        public ActionResult CreateNonAvailability(string id)
        {
            ViewBag.UserID = id;
            return View("NonAvailabilityEditor", new BusinessObjects.WorkManagement.EngineerNonAvailability());
        }

        [HttpPost]
        public ActionResult CreateNonAvailability(BusinessObjects.WorkManagement.EngineerNonAvailability instance)
        {
            try
            {
                string userID = instance.SystemID;
                instance.SystemID = DateTime.Now.ToString();
                UpdateNonAvailability(userID, instance, false);

                return RedirectToAction("NonAvailability", new { id = userID });
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        private BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection GetNonAvailability(string userID)
        {
            DateTime lastKnownUpdatedDate = DateTime.MinValue;

            return BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection.FindByUser(userID, ref lastKnownUpdatedDate);
        }

        private void UpdateNonAvailability(string userID, BusinessObjects.WorkManagement.EngineerNonAvailability nonAvailability, bool removeNonAvailability)
        {
            BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection nonAvailabilities = null;
            DateTime lastKnownUpdatedDate = DateTime.MinValue;
            int appointmentIndex = -1;
            int nonAvailabilityID = 0;

            nonAvailabilities = BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection.FindByUser(userID, ref lastKnownUpdatedDate);
            if (nonAvailabilities == null)
            {
                nonAvailabilities = new BusinessObjects.WorkManagement.EngineerNonAvailabilityCollection();
            }

            appointmentIndex = nonAvailabilities.Find("ID", nonAvailability.ID);
            if (!removeNonAvailability)
            {
                if (appointmentIndex < 0)
                {
                    // Work out an ID value for new NonAvailability and add it
                    nonAvailabilityID = 1;
                    while (nonAvailabilities.Find("ID", nonAvailability.ID) > -1)
                    {
                        nonAvailabilityID += 1;
                    }
                    nonAvailability.ID = nonAvailabilityID.ToString();
                    nonAvailabilities.Add(nonAvailability);
                    appointmentIndex = (nonAvailabilities.Count - 1);
                }
            }
            else
            {
                nonAvailabilities.RemoveAt(appointmentIndex);
            }

            // Save, removing redundant non-availabilities at the same time
            nonAvailabilities.Save(userID, DateTime.Now.Date.AddDays(-1));
        }

        #endregion

        #region Areas

        public ActionResult Areas(string id)
        {
            IntegrationWebSiteMvc.Models.AreaData model = null;

            model = new Models.AreaData();
            model.ADLoginID = id;
            model.SubAreas = _modelContext.GetEmployeeAreas(model.ADLoginID);
            model.PrimaryAreaName = _modelContext.Employees.Find(id).PrimaryAreaName;
            ViewBag.PrimaryAreas = new Models.AreaContext().PrimaryAreas.ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult Areas(IntegrationWebSiteMvc.Models.AreaData model)
        {
            return RedirectToAction("Index");
        }

        public ActionResult DeleteArea(int id, string adLoginID)
        {
            try
            {
                _modelContext.RemoveEmployeeArea(adLoginID, id);

                return RedirectToAction("Areas", new { id = adLoginID });
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        public ActionResult EditOrCreateArea(int id, string adLoginID)
        {
            IntegrationWebSiteMvc.Models.AreaData model = null;
            IntegrationWebSiteMvc.Models.AreaContext context = new Models.AreaContext();

            model = new Models.AreaData();
            model.PrimaryAreas = context.PrimaryAreas.ToList();
            model.SubAreas = new List<Models.SubArea>();

            ViewBag.PrimaryAreas = model.PrimaryAreas;
            model.ADLoginID = adLoginID;

            return View(model);
        }
        [HttpPost]
        public ActionResult EditOrCreateArea(IntegrationWebSiteMvc.Models.AreaData model)
        {
            try
            {
                _modelContext.SaveEmployeeArea(model.ADLoginID, model.SubAreaID);

                return RedirectToAction("Areas", new { id = model.ADLoginID });
            }
            catch (Exception excE)
            {
                throw excE;
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSubAreas(string primaryAreaID)
        {
            if (string.IsNullOrEmpty(primaryAreaID))
            {
                return Json(HttpNotFound());
            }

            var categoryList = GetSubAreaList(Convert.ToInt32(primaryAreaID));
            var categoryData = categoryList.Select(m => new SelectListItem()
            {
                Text = m.Name,
                Value = m.ID.ToString()
            });
            return Json(categoryData, JsonRequestBehavior.AllowGet);
        }

        private IList<Models.SubArea> GetSubAreaList(int primaryAreaID)
        {
            IntegrationWebSiteMvc.Models.AreaContext context = new Models.AreaContext();
            List<Models.SubArea> subAreas = new List<Models.SubArea>();

            subAreas = context.SubAreas.ToList().FindAll(p => p.PrimaryAreaID == primaryAreaID);

            return subAreas;
        }

        #endregion
    }
}
