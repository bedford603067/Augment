using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class AssetsController : Controller
    {
        IntegrationWebSiteMvc.Models.CorporateMetadata _modelContext = new IntegrationWebSiteMvc.Models.CorporateMetadata();
        //
        // GET: /Assets/

        public ActionResult Index()
        {
            return View(_modelContext.Assets);
        }

        //
        // GET: /Assets/Children/5

        public ActionResult Children(int id)
        {
            ViewBag.ParentAssetID = id;

            return View(_modelContext.SearchForAsset(id, false));
        }

        public ActionResult Attributes(int id)
        {
            /*
            List<BusinessObjects.WorkManagement.Asset> assets = _modelContext.SearchForAsset(id, false);
            if(assets != null && assets.Count > 0)
            {
                if (assets[0].Attributes == null)
                {
                    assets[0].Attributes = new BusinessObjects.WorkManagement.AssetAttributeCollection();
                }
                return View(assets[0].Attributes);
            }
            return null;
            */

            // Console.WriteLine(this.HttpContext.Request.Url.Host);
            string url = "~/Forms/AssetAttributes.aspx?assetid=" + id;

            Response.Redirect(url,true);

            return null;
        }

        public ActionResult Contacts(int id)
        {
            List<BusinessObjects.WorkManagement.Asset> assets = _modelContext.SearchForAsset(id, false);
            if (assets != null && assets.Count > 0)
            {
                if (assets[0].Contact == null)
                {
                    assets[0].Contact = new BusinessObjects.WorkManagement.Contact();
                }
                return View(assets[0].Contact);
            }

            return null;
        }

        public ActionResult Measurements(int id)
        {
            // ViewBag.AssetID = id;

            List<BusinessObjects.WorkManagement.Asset> assets = _modelContext.SearchForAsset(id, true);
            if (assets != null && assets.Count > 0)
            {
                if (assets[0].MeasurementHistory == null)
                {
                    assets[0].MeasurementHistory = new BusinessObjects.WorkManagement.PerformanceMeasurementCollection();
                }
                return View(assets[0].MeasurementHistory);
            }

            return null;
        }

        //
        // GET: /Assets/Create

        public ActionResult Create(bool isPrimaryAsset, int parentAssetID)
        {
            IntegrationWebSiteMvc.CorporateData.AssetUpdate newAsset = new IntegrationWebSiteMvc.CorporateData.AssetUpdate();

            switch (isPrimaryAsset)
            {
                case true:
                    {
                        newAsset.AssetType = "SITE";
                        ViewBag.Customers = _modelContext.Customers;
                        break;
                    }
                case false:
                    {
                        newAsset.ParentAsset = new BusinessObjects.WorkManagement.ReferenceType();
                        newAsset.ParentAsset.ID = parentAssetID;
                        break;
                    }
            }

            return View(newAsset);
        }

        //
        // POST: /Assets/Create

        [HttpPost]
        public ActionResult Create(CorporateData.AssetUpdate newInstance)
        {
             if (ModelState.IsValid)
             {
                 Console.WriteLine("Done");
             }

            try
            {
                // Save logic HERE
                _modelContext.SaveAsset(newInstance);
                
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Assets/Edit/5
        public ActionResult Edit(int id, int parentAssetID)
        {
            IntegrationWebSiteMvc.CorporateData.AssetUpdate newAsset = new IntegrationWebSiteMvc.CorporateData.AssetUpdate();
            bool isPrimaryAsset = false;

            newAsset = _modelContext.GenerateAssetUpdate(id);
            isPrimaryAsset = !string.IsNullOrEmpty(newAsset.AssetType) && newAsset.AssetType.ToUpper() == "SITE";

            switch (isPrimaryAsset)
            {
                case true:
                    {
                        ViewBag.Customers = _modelContext.Customers;
                        ViewBag.LocationXml = newAsset.Location.Serialize();
                        break;
                    }
                case false:
                    {
                        if (parentAssetID > 0)
                        {
                            newAsset.ParentAsset = new BusinessObjects.WorkManagement.ReferenceType();
                            newAsset.ParentAsset.ID = parentAssetID;
                        }
                        break;
                    }
            }

            return View("Create", newAsset);
        }

        //
        // POST: /Assets/Edit/5

        [HttpPost]
        public ActionResult Edit(CorporateData.AssetUpdate newInstance)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("Done");
            }

            try
            {
                bool hasLocationContentChanged = false;

                if (newInstance.Location != null && newInstance.Location.ID > 0)
                {
                    string serialized = (Server.HtmlDecode(Request["originalModel"]));

                    /*
                    // Test that Request["originalModel"] is actually a serialized Location instance 
                    System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                    xml.LoadXml(serialized);
                    BusinessObjects.WorkManagement.Location originalContent = null;
                    originalContent = (BusinessObjects.WorkManagement.Location)BusinessObjects.Base.Deserialize(typeof(BusinessObjects.WorkManagement.Location),xml);
                    */

                    hasLocationContentChanged = (serialized != newInstance.Location.Serialize());
                }

                newInstance.HasLocationChanged = hasLocationContentChanged;
                _modelContext.SaveAsset(newInstance);

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Edit", new {id=newInstance.AssetID});
            }
        }

        //
        // GET: /Assets/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Assets/Delete/5

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
