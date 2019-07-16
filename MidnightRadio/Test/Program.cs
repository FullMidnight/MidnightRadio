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
            string connectionString = "Data Source=midnightserver.database.windows.net;Initial Catalog=MidnightPool;User ID=Midnight;Password=ClockStrikesTheWitchingHour!1;Connect Timeout=60;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            DataTable data = new DataTable();
            data.Columns.Add("title", typeof(string));
            data.Columns.Add("author", typeof(string));
            data.Columns.Add("youtubeID", typeof(string));
            data.Columns.Add("playCount", typeof(int));

           data.Merge(PullReddit(connectionString),true);

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

        private static DataTable PullReddit(string connectionString)
        {
            WebClient clinet = new WebClient();
            string temp_string, after = string.Empty;
            string[] seps = { "/", "v=" };
            int laps = 0;
            DataTable data = new DataTable();
            data.Columns.Add("title", typeof(string));
            data.Columns.Add("author", typeof(string));
            data.Columns.Add("youtubeID", typeof(string));
            data.Columns.Add("playCount", typeof(int));
            connectionString = "";
            JObject temp = JObject.Parse(clinet.DownloadString("https://www.reddit.com/r/listentothis.json"));
            /*
               '' \
  --header 'Authorization: Bearer [YOUR_ACCESS_TOKEN]' \
  --header 'Accept: application/json' \
  --compressed

             */
            do
            {
                after = temp["data"]["after"].ToString();
                foreach (var j in temp["data"]["children"])
                {

                    if (j["data"]["domain"].ToString() == "youtu.be" || j["data"]["domain"].ToString() == "youtube")
                    {
                        DataRow rower = data.NewRow();
                        temp_string = j["data"]["url"].ToString();
                        rower["youtubeID"] = temp_string.Split(seps, StringSplitOptions.RemoveEmptyEntries)[temp_string.Split(seps, StringSplitOptions.RemoveEmptyEntries).Length - 1];
                        rower["playCount"] = 0;
                        if (j["data"]["media"].HasValues)
                        {
                            rower["title"] = j["data"]["media"]["oembed"]["title"].ToString();
                            rower["author"] = j["data"]["media"]["oembed"]["author_name"].ToString();
                        }
                        else
                        { 
                            rower["title"] = "";
                            rower["author"] = "";
                        }

                        data.Rows.Add(rower);
                    }
                }

                temp = JObject.Parse(clinet.DownloadString("https://www.reddit.com/r/listentothis.json?count=25&after=" + after));
                laps++;

            }
            while (laps < 4);
            return data;
        }

        private JObject QueryYouTube(string api, string apikey)
        {
            string resultsInJSON = string.Empty;

            WebClient client = new WebClient();
            client.Headers.Add("Authorization", "Bearer " + apikey);
            client.Headers.Add("Accept", "application/json");
            return JObject.Parse(client.DownloadString(api));
        }

        private JObject GetVideoInfo(string videoID, string apiKey)
        {
            string api = string.Format("https://www.googleapis.com/youtube/v3/videos?part=snippet%2CcontentDetails%2Cstatistics&id={0}&key={1}",videoID,apiKey);
            return QueryYouTube(api, apiKey);
        }

    }
}
