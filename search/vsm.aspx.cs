using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class search_vsm : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        //queryDataBase();
        Dictionary<int, double> siteRelavancy = new Dictionary<int, double>();
        siteRelavancy.Add(1, 15.6);
        siteRelavancy.Add(2, 13.9);
        siteRelavancy.Add(3, 20.7);
        displayLinks(siteRelavancy);


        double weight1, weight2, weight3;
        if (Double.TryParse(TextBox4.Text, out weight1) && Double.TryParse(TextBox5.Text, out weight2) && Double.TryParse(TextBox6.Text, out weight3))
        {
            statusLabel.Text = "Good to go!";
            // Pass the weights and words to where ever they need to go

        }
        else
        {
            statusLabel.Text = "Please enter only numbers in the weight boxes!";
        }

    }

    protected void displayLinks(Dictionary<int, double> dict)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            var sortedLinks = from site in dict orderby site.Value descending select site;
            foreach (KeyValuePair<int, double> site in sortedLinks)
            {
                HyperLink hyperlink = new HyperLink();
                string linkText = getSiteName(site.Key, conn);
                linkText = linkText.Remove(0, 1);
                linkText = linkText.Remove(linkText.Length - 1, 1);

                hyperlink.Text = linkText;
                hyperlink.NavigateUrl = linkText;

                hyperLinkLabel.Controls.Add(hyperlink);
                hyperLinkLabel.Controls.Add(new LiteralControl("<br>"));
            }
            conn.Close();
        }

    }

    static DataTable getData(DataTable t, SqlCommand cmd)
    {
        using (SqlDataAdapter a = new SqlDataAdapter(cmd))
        {
            a.Fill(t);
        }

        return t;
    }

    static string getSiteName(int siteID, SqlConnection connection)
    {
        string commandText = "SELECT site_name FROM Sites WHERE site_id = @val";
        using (var command = new SqlCommand(commandText, connection))
        {
            command.Parameters.AddWithValue("@val", siteID);
            return (string)command.ExecuteScalar();
        }
    }

    static int getWordId(string fieldValue, SqlConnection connection)
    {
        string commandText = "SELECT word_id FROM Words WHERE word LIKE @val";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@val", fieldValue);
        return (int)cmd.ExecuteScalar();
    }

    static void addToTable(string tableName, string fieldName, string fieldValue, SqlConnection connection)
    {
        string commandText = "INSERT INTO " + tableName + "(" + fieldName + ") VALUES (@val)";
        using (var command = new SqlCommand(commandText, connection))
        {
            command.Parameters.AddWithValue("@val", fieldValue);
            command.ExecuteNonQuery();
        }
    }

    static bool fieldExists(string tableName, string fieldName, string fieldValue, SqlConnection connection)
    {
        string commandText = "SELECT COUNT(*) FROM " + tableName + " WHERE " + fieldName + " = @val";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@val", fieldValue);
        int siteCount = (int)cmd.ExecuteScalar();

        if (siteCount < 1)
            return false;
        return true;
    }

    static bool inSiteWordsCount(int siteId, int wordId, SqlConnection connection)
    {
        string commandText = "SELECT COUNT(*) FROM SiteWordsCount WHERE site_id = @site_id AND word_id = @word_id";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@site_id", siteId);
        cmd.Parameters.AddWithValue("@word_id", wordId);
        int siteCount = (int)cmd.ExecuteScalar();

        if (siteCount < 1)
            return false;
        return true;
    }

    static void updateCount(int siteId, int wordId, int count, SqlConnection connection)
    {
        string commandText = "UPDATE SiteWordsCount SET count = @count WHERE site_id = @site_id AND word_id = @word_id";
        using (var command = new SqlCommand(commandText, connection))
        {
            command.Parameters.AddWithValue("@count", count);
            command.Parameters.AddWithValue("@site_id", siteId);
            command.Parameters.AddWithValue("@word_id", wordId);
            command.ExecuteNonQuery();
        }
    }

    static void addLink(int siteId, int wordId, int count, SqlConnection connection)
    {
        string commandText = "INSERT INTO SiteWordsCount (site_id, word_id, count) VALUES (@site_id, @word_id, @count)";
        using (var command = new SqlCommand(commandText, connection))
        {
            command.Parameters.AddWithValue("@site_id", siteId);
            command.Parameters.AddWithValue("@word_id", wordId);
            command.Parameters.AddWithValue("@count", count);
            command.ExecuteNonQuery();
        }
    }
}