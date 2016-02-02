using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace IntegrationWebSiteMvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // It's importat that this ignore of any url ending with ChartImg.axd is inserted before any Area route registrations.
            // If you won't use Charts in any Area you can put it in your RegisterRoutes() method. 
            RouteTable.Routes.Ignore("{*pathInfo}", new { pathInfo = @"^.*(ChartImg.axd)$" });

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
        }

        public static string AspNetSiteUrl
        {
            get
            {
                if (System.Configuration.ConfigurationManager.AppSettings["AspNetSiteUrl"] != null)
                {
                    return System.Configuration.ConfigurationManager.AppSettings["AspNetSiteUrl"];
                }
                return "DEVAPPS1";
            }
        }
    }
}