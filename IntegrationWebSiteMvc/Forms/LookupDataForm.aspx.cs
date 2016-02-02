using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;

namespace IntegrationWebSiteMvc.Forms
{
    public partial class LookupDataForm : System.Web.UI.Page
    {
        private string _urlReferrer;

        private string _activityMandatoryTaskDescription
        {
            get
            {
                string activityMandatoryTaskDescription = "Attend job and complete feedback";

                if (ConfigurationManager.AppSettings["ActivityMandatoryTaskDescription"] != null &&
                   !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ActivityMandatoryTaskDescription"]))
                {
                    activityMandatoryTaskDescription = ConfigurationManager.AppSettings["ActivityMandatoryTaskDescription"];
                }

                return activityMandatoryTaskDescription;
            }
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            if (ViewState["urlReferrer"] != null)
            {
                _urlReferrer = ViewState["urlReferrer"].ToString();
            }
        }

        protected override object SaveViewState()
        {
            ViewState["urlReferrer"] = _urlReferrer;
            return base.SaveViewState();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.UrlReferrer != null && !IsPostBack)
            {
                _urlReferrer = Request.UrlReferrer.PathAndQuery; // This is so we can get the "true" Url when this page is hosted in an iFrame.
            }

            // if (!string.IsNullOrEmpty(_urlReferrer))
            // {
                //  ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.Redirect();");
            // }
        }

        protected void DetailsView1_ItemInserting(object sender, DetailsViewInsertEventArgs e)
        {

        }

        protected void DetailsView1_ItemInserted(object sender, DetailsViewInsertedEventArgs e)
        {
            
        }

        protected void grdMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.IsNullOrEmpty(_urlReferrer))
            {
                switch (e.CommandName.ToUpper())
                {
                    case "EDIT":
                        {
                            // Tee up postback to hosting form (if this one is in iframe) on the next Post event (Update or Cancel in this case)
                            ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.Redirect();");
                            break;
                        }
                }
            }
        }

        protected void DetailsView1_ItemCommand(object sender, DetailsViewCommandEventArgs e)
        {
            if (!string.IsNullOrEmpty(_urlReferrer))
            {
                switch (e.CommandName.ToUpper())
                {
                    case "NEW":
                        {
                            // Tee up postback to hosting form (if this one is in iframe) on the next Post event (Insert or Cancel in this case)
                            ClientScript.RegisterOnSubmitStatement(this.GetType(), "Refresh", "Javascript:window.parent.Redirect();");
                            break;
                        }
                }
            }
        }

        #region State Management

        protected override object LoadPageStateFromPersistenceMedium()
        {
            return LoadPageStateFromSession();
            //return LoadPageStateFromCompressedViewState();
            //return base.LoadPageStateFromPersistenceMedium();
        }

        protected override void SavePageStateToPersistenceMedium(object viewState)
        {
            SavePageStateToSession(viewState);
            //SavePageStateToCompressedViewState(viewState);
            //base.SavePageStateToPersistenceMedium(viewState);
        }

        protected object LoadPageStateFromCompressedViewState()
        {
            string viewState = Request.Form["__VSTATE"];
            byte[] bytes = Convert.FromBase64String(viewState);
            bytes = IntegrationWebSiteMvc.Classes.Compressor.Decompress(bytes);
            LosFormatter formatter = new LosFormatter();
            return formatter.Deserialize(Convert.ToBase64String(bytes));
        }

        protected void SavePageStateToCompressedViewState(object viewState)
        {
            LosFormatter formatter = new LosFormatter();
            System.IO.StringWriter writer = new System.IO.StringWriter();
            formatter.Serialize(writer, viewState);
            string viewStateString = writer.ToString();
            byte[] bytes = Convert.FromBase64String(viewStateString);
            bytes = IntegrationWebSiteMvc.Classes.Compressor.Compress(bytes);
            ClientScript.RegisterHiddenField("__VSTATE", Convert.ToBase64String(bytes));
        }

        protected object LoadPageStateFromSession ()
        {
            string key = Request.RawUrl + "VIEWSTATE";
            object state = Session[key];               
            
            return (state == null) ? base.LoadPageStateFromPersistenceMedium () : state;

            /*
            // Applicatiuon wide, i.e all Pages, use of Session as the persistence medium, set in .browser file
            <browsers>
              <browser refID="Default">
                <controlAdapters>
                  <adapter controlType="System.Web.UI.Page" adapterType="SessionPageStateAdapter" />
                </controlAdapters>
              </browser>
            </browsers>
            */
        }

        protected void SavePageStateToSession(object viewState)
        {
            string key = Request.RawUrl + "VIEWSTATE";
            Session[key] = viewState;
        }

        #endregion

        protected void grdMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.Header &&
                e.Row.Controls != null &&
                e.Row.Controls.Count > 3 &&
                e.Row.DataItem != null &&
                e.Row.Controls[3] is System.Web.UI.WebControls.DataControlFieldCell)
            {
                ((System.Web.UI.WebControls.DataControlFieldCell)e.Row.Controls[3]).Visible = 
                    (((BusinessObjects.WorkManagement.ActivityTaskTemplate)e.Row.DataItem).Description != _activityMandatoryTaskDescription);
            }
        }
    }
}