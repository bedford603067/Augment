using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class CustomersController : Controller
    {
        IntegrationWebSiteMvc.Models.CorporateMetadata _modelContext = new IntegrationWebSiteMvc.Models.CorporateMetadata();

        //
        // GET: /Customers/

        public ActionResult Index()
        {
            return View(_modelContext.Customers);
        }

        //
        // GET: /Customers/EditOrCreate/5
        // GET: /Customers/EditOrCreate/0
        public ActionResult EditOrCreate(int id)
        {
            BusinessObjects.WorkManagement.Customer customer = null;

            if (id < 1)
            {
                customer = new BusinessObjects.WorkManagement.Customer();
                customer.BillingAddress = new BusinessObjects.WorkManagement.Location();
            }
            else
            {
                customer = _modelContext.Customers.Find(p => p.ID == id);
            }
            ViewBag.LocationXml = customer.BillingAddress.Serialize();

            return View(customer);
        }

        //
        // POST: /Customers/Edit/5

        [HttpPost]
        public ActionResult EditOrCreate(BusinessObjects.WorkManagement.Customer newInstance)
        {
            string userID = "SYSTEM";

            try
            {
                bool hasLocationContentChanged = false;

                if (newInstance.BillingAddress != null && newInstance.BillingAddress.ID > 0)
                {
                    string serialized = (Server.HtmlDecode(Request["originalModel"]));

                    /*
                    // Test that Request["originalModel"] is actually a serialized Location instance 
                    System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
                    xml.LoadXml(serialized);
                    BusinessObjects.WorkManagement.Location originalContent = null;
                    originalContent = (BusinessObjects.WorkManagement.Location)BusinessObjects.Base.Deserialize(typeof(BusinessObjects.WorkManagement.Location),xml);
                    */

                    hasLocationContentChanged = (serialized != newInstance.BillingAddress.Serialize());
                }

                newInstance.HasLocationChanged = hasLocationContentChanged;
                _modelContext.SaveCustomer(newInstance, userID);

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: /Customers/EditOrCreatePreferences/0
        public ActionResult EditOrCreatePreferences(int id)
        {
            BusinessObjects.WorkManagement.Customer customer = null;

            customer = _modelContext.Customers.Find(p => p.ID == id);
            if (customer.Preferences == null)
            {
                customer.Preferences = new BusinessObjects.WorkManagement.Preferences();
            }
            if (customer.Comments == null)
            {
                customer.Comments = new BusinessObjects.WorkManagement.CommentAuditRecordCollection();
            }
            return View(customer);
        }

        //
        // POST: /Customers/Edit/5

        [HttpPost]
        public ActionResult EditOrCreatePreferences(BusinessObjects.WorkManagement.Customer newInstance)
        {
            try
            {

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
