using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for VectorSpaceModel
/// </summary>
public class VectorSpaceModel
{
    private static object[] Documents;
    private static Random rand;

    public static void Main(string[] args)
    {
        string[] q = new string[3];
        q[0] = "virus";
        q[1] = "ebola";

        DoVSM(q);
    }

    public VectorSpaceModel()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static void DoVSM(string[] query)
    {
        Documents = new object[20];
        rand = new Random();
        // TODO: Decide how to reference Documents

        // TODO: loop through query terms, do tf-idf on each.
        // loop through documents for each term, do tf on each.

        double[] docVector = new double[Documents.Length];

        for (int i = 0; i < query.Length; i++)
        {
            if (query[i] == null)
                break;
            for (int j = 0; j < Documents.Length; j++)
            {
                docVector[j] += DoTF_IDF(query[i], Documents[j]);
            }
        }
    }

    public static double DoTF_IDF(string query, object document)
    {
        double idf = DoIDF(query);
        return DoTF(query, document) * idf;
    }

    public static double DoTF(string query, object document)
    {

        // TODO: Search database for occurences of query

        return rand.Next(100);
    }

    public static double DoIDF(string query)
    {

        // TODO: Search database for occurences of query
        // and introduce logic to determine the number of documents containing the query
        double x = rand.Next(20);
        return Math.Log(Documents.Length / (1 + x));
    }
}