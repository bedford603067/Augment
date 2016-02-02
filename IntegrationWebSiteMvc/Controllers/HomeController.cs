using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            /*
            ViewBag.Message = "Administer WMS data";
            return View();
            */

            // NB: Using a Redirect here because ChartControlWrapper relies on a non-null Page.Response.Output
            // If Dashboard is simply made the Home page - via RouteConfig - Page.Response.Output is null so blows up
            // return RedirectToAction("Index","Dashboard");

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "WMS Administration.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Administrator";

            return View();
        }
    }
}
