using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class ContactsController : Controller
    {
        IntegrationWebSiteMvc.Models.CorporateMetadata _modelContext = new IntegrationWebSiteMvc.Models.CorporateMetadata();

        //
        // GET: /Contacts/

        public ActionResult Index(int parentEntityID, string parentEntityType)
        {
            List<BusinessObjects.WorkManagement.Contact> contacts = null;

            ViewBag.ParentEntityID = parentEntityID;
            ViewBag.ParentEntityType = parentEntityType;

            switch (parentEntityType.ToUpper())
            {
                case "ASSET":
                    {
                        BusinessObjects.WorkManagement.Asset asset = null;
                        asset = _modelContext.Assets.Find(p => p.ID == parentEntityID);
                        if (asset.Contact != null)
                        {
                            contacts = new List<BusinessObjects.WorkManagement.Contact>{asset.Contact};
                        }
                        break;
                    }
                case "CUSTOMER":
                    {
                        BusinessObjects.WorkManagement.Customer customer = null;
                        customer = _modelContext.Customers.Find(p => p.ID == parentEntityID);
                        if (customer.Contacts != null)
                        {
                            contacts = new List<BusinessObjects.WorkManagement.Contact>(customer.Contacts);
                        }
                        break;
                    }
                case "SUPPLIER":
                    {
                        break;
                    }
            }

            if (contacts == null || contacts.Count < 1)
            {
                contacts = new List<BusinessObjects.WorkManagement.Contact>();
            }

            return View(contacts);
        }

        //
        // GET: /Contacts/Edit/5

        public ActionResult EditOrCreate(int parentEntityID, string parentEntityType, string contactSurname)
        {
            BusinessObjects.WorkManagement.Customer customer = null;
            BusinessObjects.WorkManagement.Contact contact = null;

            ViewBag.ParentEntityID = parentEntityID;
            ViewBag.ParentEntityType = parentEntityType;

            if (!string.IsNullOrEmpty(contactSurname))
            {
                switch (parentEntityType.ToUpper())
                {
                    case "ASSET":
                        {
                            BusinessObjects.WorkManagement.Asset asset = null;
                            asset = _modelContext.Assets.Find(p => p.ID == parentEntityID);
                            if (asset.Contact != null)
                            {
                                contact = asset.Contact;
                            }
                            break;
                        }
                    case "CUSTOMER":
                        {
                            customer = _modelContext.Customers.Find(p => p.ID == parentEntityID);
                            if (customer.Contacts != null)
                            {
                                contact = new List<BusinessObjects.WorkManagement.Contact>(customer.Contacts).Find(p => p.Surname == contactSurname);
                            }
                            break;
                        }
                    case "SUPPLIER":
                        {
                            break;
                        }
                }
            }

            if(contact == null)
            {
                contact = new BusinessObjects.WorkManagement.Contact();
            }

            return View(contact);
        }

        //
        // POST: /Contacts/Edit/5

        [HttpPost]
        public ActionResult EditOrCreate(int parentEntityID, string parentEntityType, BusinessObjects.WorkManagement.Contact contact)
        {
            try
            {
                _modelContext.SaveContact(parentEntityID, parentEntityType, contact);

                return RedirectToAction("Index", new { parentEntityID, parentEntityType });
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Contacts/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Contacts/Delete/5

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
