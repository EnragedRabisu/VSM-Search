using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class SearchPage : System.Web.UI.Page
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
                hyperlink.Text = getSiteName(site.Key, conn);
                hyperlink.NavigateUrl = hyperlink.Text;
                Panel1.Controls.Add(hyperlink);
                Panel1.Controls.Add(new LiteralControl("<br>"));
            }
            conn.Close();
        }

    }

    // This function will query the database for information on the keywords entered.
    // This function probably shouldn't be used.
    protected void queryDataBase()
    {
        string connectionString = ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            TextBox[] text = new TextBox[3];
            text[0] = TextBox1;
            text[1] = TextBox2;
            text[2] = TextBox3;

            DataTable[] tables = new DataTable[3];
            tables[0] = new DataTable();
            tables[1] = new DataTable();
            tables[2] = new DataTable();

            DataTable main = new DataTable();
            int k = 0;

            conn.Open();

            foreach (TextBox i in text)
            {
                string query = "SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word LIKE @word) AS X WHERE X.word_Id = swc.word_id) AS Y WHERE Y.site_id = S.site_id AND Y.count != 0";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@word", i.Text);

                getData(tables[k], cmd);

                main.Merge(tables[k]);
                k++;
            }

            //displayDataTable(main);
            conn.Close();
        }
    }

    // Displays data in Gridview1
    // This function works with queryDataBase.
    void displayDataTable(DataTable main)
    {
        GridView1.DataSource = main;
        GridView1.DataBind();
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