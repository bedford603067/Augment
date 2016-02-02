using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class FailureManagementController : Controller
    {
        IntegrationWebSiteMvc.Models.JobFailureSearchResults _modelContext = new Models.JobFailureSearchResults();

        //
        // GET: /FailureManagement/

        public ActionResult Index()
        {
            // IntegrationWebSiteMvc.Models.JobFailureSearchResults.Test();

            var query = from instance in _modelContext.Failures

                        orderby instance.ID

                        select instance;

            return View(query.ToList());
        }

        //
        // GET: /FailureManagement/Details/5

        public ActionResult Details(int id)
        {
            var query = from instance in _modelContext.Failures

                        where instance.ID == id

                        select instance;

            return View(query.ToList()[0]);

            // return View();
        }

        //
        // GET: /FailureManagement/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /FailureManagement/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /FailureManagement/Edit/5

        public ActionResult Edit(int id)
        {
            var query = from instance in _modelContext.Failures

                        where instance.ID == id

                        select instance;

            return View(query.ToList()[0]);

            //return View();
        }

        //
        // POST: /FailureManagement/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /FailureManagement/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /FailureManagement/Delete/5

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
    }
}
