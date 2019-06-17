using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vvget.Library
{
    public class Crawler
    {
        private HttpClient httpClient;

        public Crawler(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public string ToTheSite(Uri url, int level = 0, string extention = "*", bool shouldTrace = false)
        {
            using (httpClient)
            {
                ReadSite(url);
            }
            return null;
        }

        private void ReadSite(Uri url, int level = 0, string extention = "*", bool shouldTrace = false)
        {
            using (HttpResponseMessage response = httpClient.GetAsync(url).Result)
            {
                using (HttpContent content = response.Content)
                {
                    string subPath = "folder"; // your code goes here

                    bool exists = System.IO.Directory.Exists($""Environment.CurrentDirectory subPath));

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath(subPath));
                    }
                    content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}
