using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class AreasController : Controller
    {
        IntegrationWebSiteMvc.Models.AreaContext _modelContext = new Models.AreaContext();

        //
        // GET: /Area/

        public ActionResult Index()
        {
            return View(_modelContext.PrimaryAreas);
        }

        
        public ActionResult SubAreas(int id)
        {
            return View(_modelContext.SubAreas.ToList().FindAll(p => p.PrimaryAreaID == id));
        }

        //
        // GET: /Area/Edit/5

        public ActionResult EditOrCreatePrimaryArea(int id)
        {
            Models.PrimaryArea model = new Models.PrimaryArea();

            if(id > 0)
            {
                model = _modelContext.PrimaryAreas.ToList().FindAll(p => p.ID == id)[0];
            }
            return View(model);
        }

        //
        // POST: /Area/Edit/5

        [HttpPost]
        public ActionResult EditOrCreatePrimaryArea(Models.PrimaryArea model)
        {
            try
            {
                if (model.ID == 0)
                {
                    Models.PrimaryArea newInstance = new Models.PrimaryArea { Name = model.Name };
                    _modelContext.Entry(newInstance).State = System.Data.Entity.EntityState.Added;
                    _modelContext.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Area/EditSubArea/5
        public ActionResult EditSubArea(int id)
        {
            ViewBag.PrimaryAreas = _modelContext.PrimaryAreas.ToList();
            return View(_modelContext.SubAreas.ToList().FindAll(p => p.ID == id)[0] );
        }

        //
        // POST: /Area/EditSubArea/5
        [HttpPost]
        public ActionResult EditSubArea(int id, int primaryAreaID)
        {
            try
            {
                Models.SubArea instance = _modelContext.SubAreas.ToList().FindAll(p => p.ID == id)[0];
                instance.PrimaryAreaID = primaryAreaID;
                _modelContext.Entry(instance).State = System.Data.Entity.EntityState.Modified;
                _modelContext.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
