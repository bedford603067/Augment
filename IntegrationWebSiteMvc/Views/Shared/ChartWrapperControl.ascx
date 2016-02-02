<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<System.Web.UI.DataVisualization.Charting.Chart>" %>
<%
    Model.Page = this.Page;
    var writer = new HtmlTextWriter(Page.Response.Output);
    Model.RenderControl(writer);
%>
