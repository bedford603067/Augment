using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class StockController : Controller
    {
        IntegrationWebSiteMvc.Models.CorporateMetadata _modelContext = new IntegrationWebSiteMvc.Models.CorporateMetadata();

        //
        // GET: /Stock/

        public ActionResult Index()
        {
            List<BusinessObjects.WorkManagement.Material> materials = _modelContext.Materials;
            List<IntegrationWebSiteMvc.Models.StockSummary> stockList = _modelContext.Stock;
            IntegrationWebSiteMvc.Models.StockSummary materialStockRecord = null;

            for (int index = 0; index < materials.Count; index++)
            {
                materialStockRecord = stockList.Find(p => p.Material.ID == materials[index].ID);
                if (materialStockRecord != null)
                {
                    materials[index].Quantity = materialStockRecord.ItemCount;
                }
            }

            return View(materials);
        }

        //
        // GET: /Stock/Details/5

        public ActionResult Details(int id)
        {
            BusinessObjects.WorkManagement.StockItemCollection items = _modelContext.QueryStock(id);

            return View(items);
        }

        //
        // GET: /Stock/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Stock/Create

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
        // GET: /Stock/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Stock/Edit/5

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
        // GET: /Stock/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Stock/Delete/5

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
