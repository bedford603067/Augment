<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LookupDataForm.aspx.cs" Inherits="IntegrationWebSiteMvc.Forms.LookupDataForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Task Library</title>
    <link href="~/Content/Site.css" type="text/css" rel="stylesheet" />
</head>
<body>
    <div id="body">
        <section class="content-wrapper main-content clear-fix">
            <form id="form1" runat="server">
                <table>
                    <tr>
                        <td>
                            <asp:GridView ID="grdMain" runat="server" AutoGenerateColumns="False" 
                                CellPadding="4" DataSourceID="odsTasks" ForeColor="#333333" GridLines="Both" 
                                onrowcommand="grdMain_RowCommand" onrowdatabound="grdMain_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="ID" HeaderText="ID" ReadOnly="True" 
                                        SortExpression="ID" />
                                    <asp:BoundField DataField="Description" HeaderText="Description" 
                                        SortExpression="Description" />
                                    <asp:CheckBoxField DataField="IsCritical" HeaderText="IsCritical" 
                                        SortExpression="IsCritical" />
                                    <asp:CommandField ShowEditButton="True" />
                                </Columns>
                                <FooterStyle BackColor="#507CD1" Font-Bold="True" ForeColor="White" />
                                <PagerStyle BackColor="#253980" ForeColor="White" HorizontalAlign="Center" />
                                <HeaderStyle BackColor="#253980" Font-Bold="True" ForeColor="White" />
                                <RowStyle HorizontalAlign="Center" VerticalAlign="Top"/>    
                                <AlternatingRowStyle BackColor="White" />
                                <SelectedRowStyle BackColor="Salmon" Font-Bold="True" ForeColor="#333333" />
                                <EditRowStyle BackColor="Yellow" />
                            </asp:GridView>
                            <asp:ObjectDataSource ID="odsTasks" runat="server" 
                                DataObjectTypeName="BusinessObjects.WorkManagement.ActivityTaskTemplate" 
                                InsertMethod="SaveTask" SelectMethod="GetTasks" 
                                TypeName="IntegrationWebSiteMvc.Models.WMSMetadata" 
                                UpdateMethod="SaveTask">
                            </asp:ObjectDataSource>
                        </td>
                        <td valign="top">
                            <h2>Add New Task</h2>
                            <asp:DetailsView ID="DetailsView1" runat="server" AutoGenerateRows="False" 
                                DataSourceID="odsTasks" Height="50px" Width="125px" 
                                oniteminserted="DetailsView1_ItemInserted" 
                                oniteminserting="DetailsView1_ItemInserting" 
                                onitemcommand="DetailsView1_ItemCommand">
                                <Fields>
                                    <asp:TemplateField HeaderText="ID" SortExpression="ID">
                                        <EditItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("ID") %>'></asp:Label>
                                        </EditItemTemplate>
                                        <InsertItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("ID") %>'></asp:Label>
                                        </InsertItemTemplate>
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Bind("ID") %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Description" HeaderText="Description" 
                                        SortExpression="Description" />
                                    <asp:CheckBoxField DataField="IsCritical" HeaderText="IsCritical" 
                                        SortExpression="IsCritical" />
                                    <asp:CommandField ShowInsertButton="True" />
                                </Fields>
                            </asp:DetailsView>
                        </td>
                    </tr>
                </table>
                </form>
            </section>
    </div>
</body>
</html>
