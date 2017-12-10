﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;

namespace team_project
{
    class Program
    {
        static void Main(string[] args)
        {
            /* connect to database */
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["team_proj_431"].ConnectionString))
            {
                connection.Open();
                //int fileCount = 0;
                //string fileName = fileCount.ToString();

                // for each file in directory
                foreach (string file in Directory.EnumerateFiles("sites", "*.txt"))
                 {

                    /* check to see if an entry with the site name equal to the site name exists in the table */
                    // if it doesn't, add an entry to Sites
                    if (!fieldExists("Sites", "site_name", file, connection))
                    {
                        addToTable("Sites", "site_name", file, connection);
                    }

                    // read file
                    using (var reader = new StreamReader(file))
                    {
                        
                        // create a dictionary to store the words/occurrences in the file
                        Dictionary<string, int> dictionary = new Dictionary<string, int>();

                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {

                            string[] words = line.Split();

                            foreach (string word in words)
                            {
                                /* stop words */
                                // read stop words file
                                using (var freader = new StreamReader("stop_words.txt"))
                                {
                                    
                                    string fline;
                                    bool isStopWord = false;

                                    while ((fline = freader.ReadLine()) != null )
                                    {
                                        if (word.Equals(fline))
                                        {
                                            isStopWord = true;
                                            break;
                                        }
                                        else if (word.Equals(""))
                                        {
                                            isStopWord = true;
                                            break;
                                        }

                                    }
                                    
                                    // if the word isn't a stop word and doesn't exist in the dictionary, add it with a val of one
                                    if (!isStopWord)
                                    {
                                        if (!dictionary.ContainsKey(word))
                                        {
                                            dictionary.Add(word, 1);
                                        }
                                        else
                                        {
                                            int dCount = dictionary[word];
                                            dCount++;
                                            dictionary[word] = dCount;  // if the word is in the dictionary, add one to it's occurrences
                                        }
                                    }
                                }

                                /**********************/
                                Console.WriteLine(word); /* this is a line for testing */
                                /**********************/
                            }
                        }

                        /* update tables from dictionary */
                        // for each word in the dictionary
                        foreach (KeyValuePair<string, int> entry in dictionary)
                        {
                            // word id var = initVal   
                            string word = entry.Key;
                            int count = entry.Value;

                            Regex r = new Regex("^[a-zA-Z]*$");

                            /* update the words table */
                            // if word not in Words
                            if (word.Length < 26 && r.IsMatch(word))
                            {
                                if (!fieldExists("Words", "word", word, connection))
                                {
                                    addToTable("Words", "word", word, connection);     // add word to Words
                                }

                                /* update the linking table */
                                // get site_id from Sites
                                int siteId = getSiteId(file, connection);

                                // get word_id from Words
                                int wordId = getWordId(word, connection);

                                // if SiteWordsCount has an entry where site_id word_id 
                                if (inSiteWordsCount(siteId, wordId, connection))
                                {
                                    // update that entry's count
                                    updateCount(siteId, wordId, count, connection);
                                }
                                else
                                {   // else if it doesn't have that entry yet
                                    // add a new entry with site_id, dictionary current key, dictionary current val   
                                    addLink(siteId, wordId, count, connection);
                                }
                            }

                            /**********************/
                            Console.WriteLine(word); /* this is a line for testing */
                            /**********************/
                        }
                    }
                }

                /* fill data tables for sites and words */
                DataTable sitesTable = new DataTable();
                DataTable wordsTable = new DataTable();

                string q1 = "SELECT site_name FROM Sites";
                string q2 = "SELECT word FROM Words";

                SqlCommand sitesCmd = new SqlCommand(q1, connection);
                SqlCommand wordsCmd = new SqlCommand(q2, connection);

                SqlDataAdapter da1 = new SqlDataAdapter(sitesCmd);
                SqlDataAdapter da2 = new SqlDataAdapter(wordsCmd);

                da1.Fill(sitesTable);
                da2.Fill(wordsTable);

                /* final update in linking table */
                // for each site in the sites table
                foreach (DataRow site in sitesTable.Rows)
                {
                    string s = site["site_name"].ToString();
                    int siteId = getSiteId(s, connection);

                    // for each word in the words table
                    foreach (DataRow word in wordsTable.Rows)
                    {
                        string d = word["word"].ToString();
                        int wordId = getWordId(d, connection);
                        // if SiteWordsCount doesn't have an entry for site,word
                        if (!inSiteWordsCount(siteId, wordId, connection))
                        {
                            // add the entry with a count of 0
                            addLink(siteId, wordId, 0, connection);
                        }
                        /**********************/
                        Console.WriteLine(siteId + ", " + wordId); /* this is a line for testing */
                                                                   /**********************/
                    }
                }

                da1.Dispose();
                da2.Dispose();

                connection.Close();
            }
        }
        static int getSiteId (string fieldValue, SqlConnection connection) {
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
}