﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="SearchPage.aspx.cs" Inherits="SearchPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
        <asp:Label ID="Label1" runat="server" Text="Keyword 1"></asp:Label>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label2" runat="server" Text="Keyword 2"></asp:Label>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label3" runat="server" Text="Keyword 3"></asp:Label>
        <br />
        <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
        <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        <asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
        <br />
    
    </div>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
        <p>
            &nbsp;</p>
        <asp:GridView ID="GridView1" runat="server">
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:team_proj_431_db %>" DeleteCommand="DELETE FROM [Words] WHERE [word_Id] = @word_Id" InsertCommand="INSERT INTO [Words] ([word]) VALUES (@word)" SelectCommand="SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word = @word1) AS X 
WHERE X.word_Id = swc.word_id) AS Y
WHERE Y.site_id = S.site_id AND Y.count != 0

SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word = @word2) AS X 
WHERE X.word_Id = swc.word_id) AS Y
WHERE Y.site_id = S.site_id AND Y.count != 0

SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word = @word3) AS X 
WHERE X.word_Id = swc.word_id) AS Y
WHERE Y.site_id = S.site_id AND Y.count != 0" UpdateCommand="UPDATE [Words] SET [word] = @word WHERE [word_Id] = @word_Id">
            <DeleteParameters>
                <asp:Parameter Name="word_Id" Type="Int32" />
            </DeleteParameters>
            <InsertParameters>
                <asp:Parameter Name="word" Type="String" />
            </InsertParameters>
            <SelectParameters>
                <asp:FormParameter FormField="TextBox1" Name="word1" />
                <asp:FormParameter DefaultValue="" FormField="TextBox2" Name="word2" />
                <asp:FormParameter FormField="TextBox3" Name="word3" />
            </SelectParameters>
            <UpdateParameters>
                <asp:Parameter Name="word" Type="String" />
                <asp:Parameter Name="word_Id" Type="Int32" />
            </UpdateParameters>
        </asp:SqlDataSource>
    </form>
</body>
</html>