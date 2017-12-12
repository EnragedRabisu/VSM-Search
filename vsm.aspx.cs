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
        Dictionary<int, double> siteRelevancy = new Dictionary<int, double>();

        string word1 = TextBox1.Text;
        string word2 = TextBox2.Text;
        string word3 = TextBox3.Text;

        double weight1, weight2, weight3; // Weights are assiged in the if statement below
        if (Double.TryParse(TextBox4.Text, out weight1) && Double.TryParse(TextBox5.Text, out weight2) && Double.TryParse(TextBox6.Text, out weight3))
        {
            statusLabel.Text = "Good to go!";
            // Pass the weights and words to where ever they need to go
            siteRelevancy = VectorSpaceModel.DoVSM(word1, weight1, word2, weight2, word3, weight3);
            displayLinks(siteRelevancy);
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
                Label weightValue = new Label();
                weightValue.Text = site.Value.ToString();


                HyperLink hyperlink = new HyperLink();
                string linkText = getSiteName(site.Key, conn);
                linkText = linkText.Remove(0, 1);
                linkText = linkText.Remove(linkText.Length - 1, 1);

                hyperlink.Text = linkText;
                hyperlink.NavigateUrl = linkText;

                hyperLinkLabel.Controls.Add(hyperlink);
                hyperLinkLabel.Controls.Add(weightValue);
                hyperLinkLabel.Controls.Add(new LiteralControl("<br>"));
            }
            conn.Close();
        }

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

}