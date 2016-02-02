using System.Web;
using System.Web.Mvc;

namespace IntegrationWebSiteMvc
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // This provides the equivalent of ASP.Net Global.asax OnError, catch any unhandled exceptions, handler.
            filters.Add(new HandleErrorAttribute());
        }
    }
}