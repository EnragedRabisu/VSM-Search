<%@ Page Title="Search" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="vsm.aspx.cs" Inherits="search_vsm" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Page.Title %></h2>
    <h3>&nbsp;</h3>
    <div class="container">
        <div class="row">
            <div class="col-sm-4">
                <asp:Label ID="Label1" runat="server" Text="Keyword 1"></asp:Label>
            </div>
            <div class="col-sm-4">
                <asp:Label ID="Label2" runat="server" Text="Keyword 2"></asp:Label>
            </div>
            <div class="col-sm-4">
                <asp:Label ID="Label3" runat="server" Text="Keyword 3"></asp:Label>
            </div>
        </div>
        
        <div class="row">
            <div class="col-sm-4">
                <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
            </div>
            <div class="col-sm-4">
                <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
            </div>
            <div class="col-sm-4">
                <asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
            </div>
        </div>

        <div class="row">
            <div class="col-sm-4">
                <asp:Label ID="Label4" runat="server" Text="Weight 1"></asp:Label>
            </div>
            <div class="col-sm-4">
                <asp:Label ID="Label5" runat="server" Text="Weight 2"></asp:Label>
            </div>
            <div class="col-sm-4">
                <asp:Label ID="Label6" runat="server" Text="Weight 3"></asp:Label>
            </div>
        </div>
        
        <div class="row">
            <div class="col-sm-4">
                <asp:TextBox ID="TextBox4" runat="server">10</asp:TextBox>
            </div>
            <div class="col-sm-4">
                <asp:TextBox ID="TextBox5" runat="server">10</asp:TextBox>
            </div>
            <div class="col-sm-4">
                <asp:TextBox ID="TextBox6" runat="server">10</asp:TextBox>
            </div>
        </div>


    </div>

        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Find Websites" />
        &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="statusLabel" runat="server" Text=" "></asp:Label>
        <p>
            <asp:Label ID="hyperLinkLabel" runat="server" Text=" "></asp:Label>
        </p>
</asp:Content>
