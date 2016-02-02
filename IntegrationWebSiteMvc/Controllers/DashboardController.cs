using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using IntegrationWebSiteMvc.Models;

namespace IntegrationWebSiteMvc.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            DashboardModel model = new DashboardModel();
            return View(model);
        }
    }
}
