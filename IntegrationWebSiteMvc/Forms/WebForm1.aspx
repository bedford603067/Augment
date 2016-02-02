<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="IntegrationWebSiteMvc.Forms.WebForm1" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/Content/Site.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <header>
        <div class="content-wrapper">
            <div class="float-left">
                <p class="site-title">
                <image src= "/Images/Augment_Blue_170x70.gif" style="vertical-align:top" /> 
                </p>
            </div>
            <div class="float-right">
                <nav>
                    <ul id="menu">
                        <li><a href="../Home/Index">Home</a></li>
                        <li><a href="../Activity/Index">Activities</a></li>
                        <li><a href="../Assets/Index">Assets</a></li>
                        <li><a href="../Users/Index">Users</a></li>
                    </ul>
                </nav>
            </div>
        </div>
    </header>
    <div id="body">
        <section class="content-wrapper main-content clear-fix">
            <form id="form1" runat="server">
            <div>
            Hello ASP Net Page
                <asp:Button ID="cmdTest" runat="server" Text="TEST 1" onclick="cmdTest_Click" />
                <asp:Button ID="cmdTest2" runat="server" Text="TEST 2" onclick="cmdTest2_Click" />
            </div>
            </form>
        </section>
    </div>
    <footer>
        <div class="content-wrapper">
            <div class="float-left">
                <p>&copy;2013 Augment Software Ltd</p>
            </div>
        </div>
    </footer>
</body>
</html>
