<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ScrollingRssFeed.aspx.cs" Inherits="IntegrationWebSiteMvc.Forms.ScrollingRssFeed" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div style="background-color:White; height:20px; font-size:medium; font-weight:bold">
            <marquee scrollamount="5" scrolldelay=100> <!-- Control speed with scrollAmount and scrollDelay [ms] -->
                <asp:Repeater ID="rptRSS" DataSourceID="XmlDataSource1" EnableViewState="False" runat="server"> 
                    <ItemTemplate>
                        <a href="<%# XPath("link")%>"><%# XPath("description")%>...........................</a>
                        </ItemTemplate> 
                    </asp:Repeater>
            </marquee>
                <asp:XmlDataSource ID="XmlDataSource1" runat="server" 
                DataFile="~/App_Data/Feed.xml" XPath="rss/channel/item" ></asp:XmlDataSource> <!--  XPath="rss/channel/item[position()<3]"   -->
        </div>
    </form>
</body>
</html>
