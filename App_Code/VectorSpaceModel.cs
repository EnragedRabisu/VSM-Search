using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DBProjectWebsite;

/// <summary>
/// Summary description for VectorSpaceModel
/// </summary>
public class VectorSpaceModel
{
    private static object[] Documents;
    private static Random rand;
    private static SqlConnection conn;
    private static Label l;

    public static void Main(string[] args)
    {
        using (conn = new SqlConnection(ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString))
        {
            conn.Open();
            string q = "virus ebola";

            //DoVSM(q, conn);
            conn.Close();
        }
    }

    public VectorSpaceModel()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static void DoVSM(string queryText, Label label, Label l2)
    {
        string[] query = queryText.Split(' ');
        l = label;
        using (conn = new SqlConnection(ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString))
        {
            conn.Open();
            //AddNormals(conn);
            //rand = new Random();
            // TODO: Decide how to reference Documents

            // TODO: loop through query terms, do tf-idf on each.
            // loop through documents for each term, do tf on each.

            int sites = GetTotalSites(conn);
            int queries = query.Length;

            double[,] siteVector = new double[sites, queries];
            double[] queryVector = new double[queries];
            double[] dotProduct = new double[sites];
            //double[] siteNorm = new double[sites];
            double queryNorm = 0;
            string labelOutput = "";
            string l2Out = "";

            for (int i = 0; i < query.Length; i++)
            {
                int wordID = 0;
                if (query[i] == null)
                {
                    queryVector[i] = 0;
                }
                else
                {
                    wordID = GetWordId(query[i], conn);
                    queryVector[i] = DoIDF(wordID, conn);
                    queryNorm += queryVector[i] * queryVector[i];
                    l2Out += ("Query: " + query[i] + " = " + queryVector[i] + " </br>");
                }

                for (int j = 0; j < siteVector.GetLength(0); j++)
                {
                    // If Query is null, we don't do anything with it.
                    if (query[i] == null)
                    {
                        siteVector[j, i] = 0;
                        break;
                    }
                    // Else, get the tf*idf.
                    double temp = DoTF_IDF(wordID, conn, (j + 1));
                    siteVector[j, i] = temp;
                    //siteNorm[j] += temp * temp;
                    dotProduct[j] += temp * queryVector[i];

                    // Debug shenanigans
                    labelOutput += ("Site: " + (j + 1) + " Query : " + query[i] + " tf-idf: " + temp + " </br>");
                    labelOutput += ("Site: " + (j + 1) + " Query : " + query[i] + " queryVector: " + queryVector[i] + " </br>");
                    labelOutput += ("Site: " + (j + 1) + " Query : " + query[i] + " dotProduct: " + dotProduct[j] + " </br></br>");
                }
            }

            l2Out += ("Query Norm: " + queryNorm + "</br>");

            for (int j = 0; j < siteVector.GetLength(0); j++)
            {
                double vsmResult = 0;
                double siteNorm = GetNormal(j + 1, conn);
                l2Out += ("DotProduct for " + (j + 1) + ": " + dotProduct[j] + "</br>");
                l2Out += ("QueryNorm: " + Math.Sqrt(queryNorm) + "</br>");
                l2Out += ("SiteNorm for " + (j + 1) + ": " + Math.Sqrt(siteNorm) + "</br>");
                l2Out += ("Product for " + (j + 1) + ": " + (Math.Sqrt(queryNorm) * Math.Sqrt(siteNorm)) + "</br></br>");
                vsmResult = dotProduct[j] / (Math.Sqrt(queryNorm) * Math.Sqrt(siteNorm));
                labelOutput += ("Site: " + (j + 1) + " Query : " + queryText + " = " + vsmResult + " </br>");
            }

            l2.Text = labelOutput;
            l2.Text += l2Out;
            conn.Close();
        }
    }

    public static double DoTF_IDF(int wordID, SqlConnection conn, int siteID)
    {
        double idf = DoIDF(wordID, conn);
        double tf = DoTF(wordID, conn, siteID);
        return tf * idf;
    }

    public static double DoTF(int wordID, SqlConnection conn, int siteID)
    {
        // TODO: Search database for occurences of query
        int wordFreq = GetQueryCountInSite(siteID, wordID, conn);
        int siteTotalFreq = GetSiteTotalWordCount(siteID, conn);
        double adjustedFreq = 0;
        if (siteTotalFreq != 0)
        {
            adjustedFreq = (double)wordFreq / siteTotalFreq;
        }

        return wordFreq;
    }

    public static double DoIDF(int wordID, SqlConnection conn)
    {

        // TODO: Search database for occurences of query
        // and introduce logic to determine the number of documents containing the query
        int x = 0;
        int sites = GetTotalSites(conn);

        for (int i = 0; i < sites; i++)
        {
            if (DoesWordExist(i, wordID, conn))
            {
                x++;
            }
        }

        //l.Text += wordID + " : " + x + "</br>";

        double result = Math.Log(sites / (1.0 + x));

        //l.Text += wordID + " : " + result + "</br>";

        return result;
    }

    static int GetWordId(string fieldValue, SqlConnection connection)
    {
        string commandText = "SELECT word_id FROM Words WHERE word LIKE @val";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@val", fieldValue);
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return result;
    }

    static int GetTotalSites(SqlConnection connection)
    {
        string commandText = "SELECT COUNT(*) FROM Sites";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return result;
    }

    static int GetTotalWords(SqlConnection connection)
    {
        string commandText = "SELECT COUNT(*) FROM Words";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return result;
    }

    static int GetSiteTotalWordCount(int siteId, SqlConnection connection)
    {
        string commandText = "SELECT SUM(count) FROM SiteWordsCount WHERE site_id = @site_id";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@site_id", siteId.ToString());
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return result;
    }

    static int GetQueryCountInSite(int siteId, int wordId, SqlConnection connection)
    {
        string commandText = "SELECT count FROM SiteWordsCount WHERE site_id = @site_id AND word_id = @word_id";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@site_id", siteId.ToString());
        cmd.Parameters.AddWithValue("@word_id", wordId.ToString());
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return result;
    }

    static int GetNormal(int siteId, SqlConnection connection)
    {
        string commandText = "SELECT site_normal FROM SiteNormals WHERE site_id = @site_id";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@site_id", siteId);
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        return result;
    }

    static bool DoesWordExist(int siteID, int wordID, SqlConnection connection)
    {
        string commandText = "SELECT COUNT(*) FROM SiteWordsCount WHERE site_id = @site AND word_id = @word AND count > 0";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@site", siteID.ToString());
        cmd.Parameters.AddWithValue("@word", wordID.ToString());
        int result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToInt32(cmd.ExecuteScalar());
        }

        if (result < 1)
            return false;
        return true;
    }

    static void AddNormals(SqlConnection conn)
    {
        int words = GetTotalWords(conn);
        int sites = GetTotalSites(conn);
        double normal = 0;
        for (int j = 1; j < sites + 1; j++)
        {
            normal = 0;

            for (int i = 1; i < words + 1; i++)
            {
                normal += Math.Pow(DoTF_IDF(i, conn, j), 2);
            }

            l.Text += ("Site: " + j + "Normal: " + normal + "</br>");
            /*
            string commandText = "INSERT INTO SiteNormals (site_id, site_normal) VALUES (@site_id, @normal)";
            using (var command = new SqlCommand(commandText, conn))
            {
                command.Parameters.AddWithValue("@site_id", j);
                command.Parameters.AddWithValue("@normal", normal);
                command.ExecuteNonQuery();
            }
            */
        }
    }
}