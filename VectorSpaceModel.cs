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
    private static SqlConnection conn;
    Label l;

    //public static void Main(string[] args)
    //{
    //    using (conn = new SqlConnection(ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString))
    //    {
    //        conn.Open();
    //        string q = "virus ebola";
    //
    //        //DoVSM(q, conn);
    //        conn.Close();
    //    }
    //}
    //
    //public VectorSpaceModel()
    //{
    //    //
    //    // TODO: Add constructor logic here
    //    //
    //}

    public static Dictionary<int, double> DoVSM(string q1, double w1, string q2, double w2, string q3, double w3)
    {
        // Open SQL Connection with connection string from config.
        using (conn = new SqlConnection(ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString))
        {
            conn.Open();

            // Debug assignments
            //Label l = label;
            string l2Out = "";
            string labelOutput = "";

            // TODO: loop through query terms, do tf-idf on each.
            // loop through documents for each term, do tf on each.

            // Number of sites and number of queries.
            int sites = GetTotalSites(conn);
            string[] query = new string[3];
            double[] weights = new double[3];

            query[0] = q1;
            query[1] = q2;
            query[2] = q3;

            weights[0] = w1;
            weights[1] = w2;
            weights[2] = w3;
            int queries = query.Length;

            // Vectors for Sites, Query, and Dot Product
            double[,] siteVector = new double[sites, queries];
            double[] queryVector = new double[queries];
            double[] dotProduct = new double[sites];

            // Norm of query vector
            double queryNorm = 0;

            // Output Dictionary
            Dictionary<int, double> outDict = new Dictionary<int, double>();

            for (int i = 0; i < queries; i++)
            {
                int wordID = 0;
                if (query[i] == null)
                {
                    // If word is null, set it to 0
                    queryVector[i] = 0;
                }
                else
                {
                    // Get Word ID for query
                    wordID = GetWordId(query[i], conn);
                    // Add word to query vector
                    queryVector[i] = weights[i] * GetIDF(wordID, conn);
                    // Add word to query norm
                    queryNorm += Math.Pow(queryVector[i], 2);

                    // Debug
                    l2Out += ("Query: " + query[i] + " = " + queryVector[i] + " </br>");
                }

                for (int j = 0; j < siteVector.GetLength(0); j++)
                {
                    // If Query is null, we can skip this. The product will be 0 anyways.
                    if (query[i] == null)
                    {
                        siteVector[j, i] = 0;
                        continue;
                    }
                    // Else, get the tf*idf.
                    double temp = DoTF_IDF(wordID, conn, (j + 1));
                    // Put tf*idf into site vector
                    siteVector[j, i] = temp;
                    // Calculate dot product for this site.
                    dotProduct[j] += temp * queryVector[i];

                    // Debug shenanigans
                    labelOutput += ("Site: " + (j + 1) + " Query : " + query[i] + " tf-idf: " + temp + " </br>");
                    labelOutput += ("Site: " + (j + 1) + " Query : " + query[i] + " queryVector: " + queryVector[i] + " </br>");
                    labelOutput += ("Site: " + (j + 1) + " Query : " + query[i] + " dotProduct: " + dotProduct[j] + " </br></br>");
                }
            }

            // Debug
            //l2Out += ("Query Norm: " + queryNorm + "</br>");

            for (int j = 0; j < siteVector.GetLength(0); j++)
            {
                // declare variable for vsm Result
                double vsmResult = 0;
                // get the site Normal from the database
                double siteNorm = GetNormal(j + 1, conn);

                // Calculate vsmResult, AKA cosine similarity
                // dot product / (square root of query norm * square root of site norm)
                vsmResult = dotProduct[j] / (Math.Sqrt(queryNorm) * Math.Sqrt(siteNorm));

                // Add entry to dictionary
                outDict.Add((j + 1), vsmResult);

                // Debug Statements
                l2Out += ("DotProduct for " + (j + 1) + ": " + dotProduct[j] + "</br>");
                l2Out += ("QueryNorm: " + Math.Sqrt(queryNorm) + "</br>");
                l2Out += ("SiteNorm for " + (j + 1) + ": " + Math.Sqrt(siteNorm) + "</br>");
                l2Out += ("Product for " + (j + 1) + ": " + (Math.Sqrt(queryNorm) * Math.Sqrt(siteNorm)) + "</br></br>");
                labelOutput += ("Site: " + (j + 1) + " Query : " + query.ToString() + " = " + vsmResult + " </br>");
            }

            //l.Text = labelOutput;
            //l.Text += l2Out;

            conn.Close();
            return outDict;
        }
    }

    public static double DoTF_IDF(int wordID, SqlConnection conn, int siteID)
    {
        // Retrieve IDF
        double idf = GetIDF(wordID, conn);
        // Calculate TF
        double tf = DoTF(wordID, conn, siteID);
        // Multiply TF * IDF
        return tf * idf;
    }

    public static double DoTF(int wordID, SqlConnection conn, int siteID)
    {
        // TODO: Search database for occurences of query
        int wordFreq = GetQueryCountInSite(siteID, wordID, conn);
        int siteTotalFreq = GetSiteTotalWordCount(siteID, conn);
        double adjustedFreq = 0;

        // Adjust frequency by dividing by total words in Site
        // Currently unused
        if (siteTotalFreq != 0)
        {
            adjustedFreq = (double)wordFreq / siteTotalFreq;
        }

        return wordFreq;
    }

    static double GetIDF(int wordID, SqlConnection connection)
    {
        string commandText = "SELECT IDF FROM Words WHERE word_Id = @val";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@val", wordID);
        double result = 404;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToDouble(cmd.ExecuteScalar());
        }

        return result;
    }

    static double GetNormal(int siteId, SqlConnection connection)
    {
        string commandText = "SELECT Normal FROM Sites WHERE site_id = @site_id";
        SqlCommand cmd = new SqlCommand(commandText, connection);
        cmd.Parameters.AddWithValue("@site_id", siteId);
        double result = 0;
        if (!Convert.IsDBNull(cmd.ExecuteScalar()))
        {
            result = Convert.ToDouble(cmd.ExecuteScalar());
        }

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
}