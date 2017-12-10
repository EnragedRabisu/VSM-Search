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
        string word1 = TextBox1.Text;
        string word2 = TextBox2.Text;
        string word3 = TextBox3.Text;

        queryDataBase(word1, word2, word3);
    }

    protected void queryDataBase(string word1, string word2, string word3)
    {
        string connectionString = ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            DataTable t1 = new DataTable();
            DataTable t2 = new DataTable();
            DataTable t3 = new DataTable(); 

            string query1 = "SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word = @word1) AS X WHERE X.word_Id = swc.word_id) AS Y WHERE Y.site_id = S.site_id AND Y.count != 0";
            string query2 = "SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word = @word2) AS X WHERE X.word_Id = swc.word_id) AS Y WHERE Y.site_id = S.site_id AND Y.count != 0";
            string query3 = "SELECT site_name, Y.word, Y.count FROM Sites S, (SELECT X.word, site_id, count, swc.word_id FROM SiteWordsCount swc, (SELECT word, word_Id FROM Words WHERE word = @word3) AS X WHERE X.word_Id = swc.word_id) AS Y WHERE Y.site_id = S.site_id AND Y.count != 0";

            SqlCommand cmd1 = new SqlCommand(query1, conn);
            SqlCommand cmd2 = new SqlCommand(query2, conn);
            SqlCommand cmd3 = new SqlCommand(query3, conn);

            cmd1.Parameters.AddWithValue("@word1", TextBox1.Text);
            cmd2.Parameters.AddWithValue("@word2", TextBox2.Text);
            cmd3.Parameters.AddWithValue("@word3", TextBox3.Text);

            getData(t1, cmd1);
            getData(t2, cmd2);
            getData(t3, cmd3);

            t1.Merge(t2);
            t1.Merge(t3);

            GridView1.DataSource = t1;
            GridView1.DataBind();
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

    static int getSiteId(string fieldValue, SqlConnection connection)
    {
        string commandText = "SELECT site_id FROM Sites WHERE site_name LIKE @val";
        using (var command = new SqlCommand(commandText, connection))
        {
            command.Parameters.AddWithValue("@val", fieldValue);
            return (int)command.ExecuteScalar();
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