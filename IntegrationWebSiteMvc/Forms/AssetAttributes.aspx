<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AssetAttributes.aspx.cs" Inherits="IntegrationWebSiteMvc.Forms.AssetAttributes" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <link href="~/Content/Site.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <header>
        <div class="content-wrapper">
            <div class="float-left">
                <p class="site-title">
                <img src= "../Images/Augment_Blue_170x70.gif" style="vertical-align:top" /> 
                </p>
            </div>
            <div style="float:right; overflow:hidden; font-size:18px">
                <nav>
                    <ul id="nav">
                        <li><a href="../Home/Index">Home</a></li> <!-- NB: These links are right for ASP Net MVC, VS intellisense won't approve of them though -->
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
    
            <table style="min-width:300px;margin:0 auto">
                <tr valign="top">
                    <td>
                        <asp:GridView ID="grdMain" DataKeyNames="Name" runat="server" 
                            AllowPaging="True" AutoGenerateColumns="False"
                            CellPadding="4" DataSourceID="odsAttributes" ForeColor="#333333" 
                            AllowSorting="True" PageSize="5" 
                            onselectedindexchanged="grdMain_SelectedIndexChanged">
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="Name" ReadOnly="true" SortExpression="Name" />
                                <asp:BoundField DataField="DataType" HeaderText="Type" SortExpression="DataType" />
                                <asp:BoundField DataField="Value" HeaderText="Value" />
                            </Columns>
                            <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                            <PagerStyle BackColor="#253980" ForeColor="White" HorizontalAlign="Center" />
                            <HeaderStyle BackColor="#253980" Font-Bold="True" ForeColor="White" />
                            <RowStyle HorizontalAlign="Center" VerticalAlign="Top"/>    
                            <AlternatingRowStyle BackColor="White" />
                            <SelectedRowStyle BackColor="Salmon" Font-Bold="True" ForeColor="#333333" />
                            <EditRowStyle BackColor="Yellow" />
                        </asp:GridView>
                    </td>
                    <td>
                        <div id="attributesCaption" runat="server"><h2>Add New Attribute</h2></div>
                        <asp:FormView ID="FormView1" runat="server" DefaultMode="Insert" DataSourceID="odsAttributes">
                            <InsertItemTemplate>
                                Name:
                                <asp:TextBox ID="NameTextBox" runat="server" Text='<%# Bind("Name") %>' />
                                <br />
                                Value:
                                <asp:TextBox ID="ValueTextBox" runat="server" Text='<%# Bind("Value") %>' />
                                <br />
                                Type:
                                <asp:TextBox ID="TypeTextBox" runat="server" Text='<%# Bind("Type") %>' />
                                <br />
                                UserID:
                                <asp:TextBox ID="UserIDTextBox" runat="server" Text='<%# Bind("UserID") %>' />
                                <br />
                                Val. Express:
                                <asp:TextBox ID="ValidationExpressionTextBox" runat="server" 
                                    Text='<%# Bind("ValidationExpression") %>' />
                                <br />
                                Val. Message:
                                <asp:TextBox ID="ValidationMessageTextBox" runat="server" 
                                    Text='<%# Bind("ValidationMessage") %>' />
                                <br />
                                IsEditable:
                                <asp:CheckBox ID="IsEditableCheckBox" runat="server" 
                                    Checked='<%# Bind("IsEditable") %>' />
                                <br />
                                DataType:
                                <asp:TextBox ID="DataTypeTextBox" runat="server" 
                                    Text='<%# Bind("DataType") %>' />
                                <br />
                                Position:
                                <asp:TextBox ID="PositionTextBox" runat="server" 
                                    Text='<%# Bind("Position") %>' />
                                <br />
                                IsMandatory:
                                <asp:CheckBox ID="IsMandatoryCheckBox" runat="server" 
                                    Checked='<%# Bind("IsMandatory") %>' />
                                <br />
                                IsHidden:
                                <asp:CheckBox ID="IsHiddenCheckBox" runat="server" 
                                    Checked='<%# Bind("IsHidden") %>' />
                                <br />
                                <asp:LinkButton ID="InsertButton" runat="server" CausesValidation="True" 
                                    CommandName="Insert" Text="Insert" />
                                &nbsp;<asp:LinkButton ID="InsertCancelButton" runat="server" 
                                    CausesValidation="False" CommandName="Cancel" Text="Cancel" />
                            </InsertItemTemplate>
                        </asp:FormView>

                        <asp:ObjectDataSource ID="odsAttributes" runat="server" 
                            SelectMethod="GetAssetAttributes"
                            TypeName="IntegrationWebSiteMvc.Models.CorporateMetadata" 
                            StartRowIndexParameterName="startIndex" 
                            MaximumRowsParameterName="pageSize" SortParameterName="sortBy" 
                            EnablePaging="True"
                            SelectCountMethod="GetAssetAttributesCount" 
                            onselected="odsAttributes_Selected"
                            InsertMethod="SaveAssetAttribute" oninserting="odsAttributes_Inserting" >
                            <SelectParameters>
                                <asp:QueryStringParameter DefaultValue="0" Name="assetID" 
                                    QueryStringField="assetID" Type="Int32" />
                            </SelectParameters>
                        </asp:ObjectDataSource>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:HyperLink ID="hypUrlReferrer" runat="server" Text="Back to list" />
                    </td>
                </tr>
            </table>

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

