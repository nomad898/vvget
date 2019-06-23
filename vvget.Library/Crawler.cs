using CsQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vvget.Library
{
    public class Crawler
    {
        public Dictionary<string, string> ToTheSite(Uri url, int level = 0, string extention = "*")
        {
            var result = new Dictionary<string, string>();
            using (HttpClient httpClient = new HttpClient())
            {
                return ReadSite(url, httpClient, result, ref level, extention);
            }
        }

        private Dictionary<string, string> ReadSite(Uri url, HttpClient httpClient, Dictionary<string, string> dictOfContent, ref int level, string extention)
        {
            var hrefTags = new SortedSet<string>();
            string subPath = url.Host;
            string contentString;
            CQ html;
            string path = $"{Environment.CurrentDirectory}/{subPath}";
            string fileName = url.AbsoluteUri.Replace("/", string.Empty).Replace(".", string.Empty).Replace(":", string.Empty);
            bool exists = Directory.Exists(path);
            using (HttpResponseMessage response = httpClient.GetAsync(url).Result)
            {
                using (HttpContent content = response.Content)
                {
                    contentString = content.ReadAsStringAsync().Result;
                }
            }
            html = CQ.Create(contentString);            
            if (!exists)
            {
                Directory.CreateDirectory(path);
            }
            string ext = html.Document.DocType.ToString().Remove(4, 1);
            string filePath = $"{Environment.CurrentDirectory}/{subPath}/{fileName}.{ext}";
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(contentString);
                }
            }

            foreach (IDomObject obj in html.Find("a"))
            {
                var href = obj.GetAttribute("href");
                if (href.Contains("&") || href.Contains("javascript:void") || href.Contains("#") || href.StartsWith("http") || href == "/" || string.IsNullOrWhiteSpace(href))
                {
                    continue;
                }
                hrefTags.Add(obj.GetAttribute("href"));
            }

            dictOfContent.Add(url.AbsoluteUri, contentString);
            if (hrefTags.Count > 0 && level > 0)
            {
                level -= 1;
                foreach (var item in hrefTags)
                {
                    if (item.StartsWith("http"))
                    {
                        ReadSite(new Uri($"{item}"), httpClient, dictOfContent, ref level, extention);                       
                    }
                    else
                    {
                        var absUri = url.AbsoluteUri.Remove(url.AbsoluteUri.Length - 1, 1);
                        ReadSite(new Uri($"{absUri}{item}"), httpClient, dictOfContent, ref level, extention);
                    }
                }
            }
            return dictOfContent;
        }
    }
}

