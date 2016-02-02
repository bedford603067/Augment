using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntegrationWebSiteMvc.Forms
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Console.WriteLine(Request.QueryString);

            // ClientScript.RegisterStartupScript(this.GetType(), "RefreshParent", "<script type='text/javascript'>var btn = window.parent.document.getElementById('cmdTest');if (btn) btn.click();</script>");

            // ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "window.parent.document.forms[0].submit();");

            // ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.RaiseAlert('Hello from ASP Net Web Form');");

            // ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.Redirect('http://localhost:59798/Activity/Tasks/2175');");

            // ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.Redirect();");
        }

        protected void cmdTest_Click(object sender, EventArgs e)
        {
            string parentUrl = Request.UrlReferrer.PathAndQuery; // This is so we can get the "true" Url when this page is hosted in an iFrame.

            // string parentUrl = "http://localhost:59798/Activity/Tasks/2175";
            // Response.Redirect(parentUrl);

            ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.Redirect();");
        }

        protected void cmdTest2_Click(object sender, EventArgs e)
        {
            string parentUrl = Request.UrlReferrer.PathAndQuery; // This is so we can get the "true" Url when this page is hosted in an iFrame.
            // Response.Redirect(parentUrl);
        }
    }
}