using System.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            WebClient clinet = new WebClient();
            string connectionString, temp_string,after = string.Empty;
            string[] seps = { "/", "v=" };
            int laps = 0;
            DataTable data = new DataTable();
            data.Columns.Add("title", typeof(string));
            data.Columns.Add("author", typeof(string));
            data.Columns.Add("youtubeID", typeof(string));
            data.Columns.Add("playCount", typeof(int));
            connectionString = "";
            JObject temp = JObject.Parse(clinet.DownloadString("https://www.reddit.com/r/listentothis.json"));
            do {
                after = temp["data"]["after"].ToString();
                foreach (var j in temp["data"]["children"])
                {

                    if (j["data"]["domain"].ToString() == "youtu.be" || j["data"]["domain"].ToString() == "youtube")
                    {
                        DataRow rower = data.NewRow();
                        rower["title"] = j["data"]["media"]["oembed"]["title"].ToString();
                        rower["author"] = j["data"]["media"]["oembed"]["author_name"].ToString();
                        temp_string = j["data"]["url"].ToString();
                        rower["youtubeID"] = temp_string.Split(seps, StringSplitOptions.RemoveEmptyEntries)[temp_string.Split(seps, StringSplitOptions.RemoveEmptyEntries).Length - 1];
                        rower["playCount"] = 0;
                        data.Rows.Add(rower);
                    }
                }

                temp = JObject.Parse(clinet.DownloadString("https://www.reddit.com/r/listentothis.json?count=25&after="+after));
                laps++;

            }
            while (laps < 4);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("[dbo].[MusicUpload]", connection);
                command.CommandType = CommandType.StoredProcedure;
                SqlParameter param = new SqlParameter("@data", SqlDbType.Structured)
                {
                    TypeName = "dbo.MusicUploadTable",
                    Value = data
                };
                command.Parameters.Add(param);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                    }
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }


        }
    }
}
