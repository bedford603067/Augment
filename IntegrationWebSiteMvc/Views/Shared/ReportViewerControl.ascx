<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %><%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!- NB: The Page_Init handling code is suggested to handle postback, else limited to a single page report. BUT it does not work as ScriptResource.axd throws an error..- ->
<!--
<script type="text/C#">
private void Page_Init(object sender, System.EventArgs e)
    {
        Context.Handler = this.Page;
    }
</script>
-->

<form id="Form1" runat="server">
<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
<rsweb:ReportViewer ID="ReportViewer1" runat="server" ProcessingMode="Remote" AsyncRendering="false" Width="100%" >
    <ServerReport ReportServerUrl="https://polarbear.finalbuild.co.uk/ReportServer" 
        ReportPath="/All/RecentlyCreatedJobs" />
</rsweb:ReportViewer>
</form>
