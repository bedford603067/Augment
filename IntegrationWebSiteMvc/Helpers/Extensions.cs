using System;
using System.Web.Mvc;
using System.Web.Routing;
  
namespace System.Web.Mvc.Html 
{
    public static class ActionLinkButtonHelper 
    {
        public static MvcHtmlString ActionLinkButton(this HtmlHelper htmlHelper, string buttonText, string actionName, string controllerName, RouteValueDictionary routeValues) 
        {
            string href = UrlHelper.GenerateUrl("default", actionName, controllerName, routeValues, RouteTable.Routes, htmlHelper.ViewContext.RequestContext, false);
            string buttonHtml = string.Format("<input type=\"button\" title=\"{0}\" value=\"{0}\" onclick=\"location.href='{1}'\" class=\"button\" />",buttonText,href);

            return new MvcHtmlString(buttonHtml);
        }
    }
} 

