using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IntegrationWebSiteMvc.Forms
{
    public partial class AssetAttributes : System.Web.UI.Page
    {
        private string _urlReferrer;

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
                hypUrlReferrer.NavigateUrl = _urlReferrer;
            }
            if (!IsPostBack)
            {
                bool allowNew = Request.Params["edit"] != null && (Request.Params["edit"] == "1" || Request.Params["edit"] == "true");
                FormView1.Visible = allowNew;
                attributesCaption.Visible = allowNew;
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

        protected object LoadPageStateFromSession()
        {
            string key = Request.RawUrl + "VIEWSTATE";
            object state = Session[key];

            return (state == null) ? base.LoadPageStateFromPersistenceMedium() : state;

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

        protected void grdMain_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void odsAttributes_Selected(object sender, ObjectDataSourceStatusEventArgs e)
        {
        }

        protected void odsAttributes_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
        {
            int paramCount = e.InputParameters.Count;

            int assetID = Request.Params["assetID"] != null ? int.Parse(Request.Params["assetID"]) : 0;
            BusinessObjects.WorkManagement.AssetAttribute assetAttribute = new BusinessObjects.WorkManagement.AssetAttribute();
            assetAttribute.Name = e.InputParameters["Name"].ToString();

            e.InputParameters.Clear();
            e.InputParameters.Add("assetID", assetID);
            e.InputParameters.Add("instance", assetAttribute);
        }
    }
}