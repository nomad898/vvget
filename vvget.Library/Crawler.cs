using CsQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vvget.Library
{
    public class Crawler
    {
        public int Level { get; set; }
        public bool IsVerbose { get; set; }
        public bool IsPageReaded { get; set; }
        public event EventHandler<CrawlerEventArgs> Downloaded;

        private void OnDownloaded(object sender, CrawlerEventArgs e)
        {
            EventHandler<CrawlerEventArgs> handler = Downloaded;
            if (IsVerbose)
            {
                handler?.Invoke(this, e);
            }
        }

        public void ToTheSite(Uri url, int level = 0, string extention = "*")
        {
            using (HttpClient httpClient = new HttpClient())
            {
                Read(url, httpClient, level, extention);
            }
            Level = level;
        }

        private void Read(Uri url, HttpClient httpClient, int level = 0, string extention = "*")
        {
            WebContent content = new WebContent();
            using (HttpResponseMessage response = httpClient.GetAsync(url).Result)
            {
                using (HttpContent httpContent = response.Content)
                {
                    if (!IsPageReaded && IsReaded(httpContent, url, content))
                    {
                        IsPageReaded = true;
                        StartAnalize(content, httpClient, url.AbsoluteUri);
                    }
                }
            }
        }

        private bool IsReaded(HttpContent httpContent, Uri url, WebContent content)
        {
            content.MediaType = httpContent.Headers.ContentType.MediaType;
            if (content.MediaType == ContentTypes.Html)
            {
                content.Text = httpContent.ReadAsStringAsync().Result;
                OnDownloaded(this, new CrawlerEventArgs(content.Text));
                SaveToFile(url, content);
                return true;
            }
            if (content.MediaType == ContentTypes.Css)
            {
                content.Text = httpContent.ReadAsStringAsync().Result;
                OnDownloaded(this, new CrawlerEventArgs(content.Text));
                SaveCssToFile(url, content);
                return false;
            }
            if (content.MediaType == ContentTypes.JavaScriptHeader
                || content.MediaType == ContentTypes.JavaScriptXHeader)
            {
                content.Text = httpContent.ReadAsStringAsync().Result;
                OnDownloaded(this, new CrawlerEventArgs(content.Text));
                SaveJsToFile(url, content);
                return false;
            }
            return false;
        }

        private void SaveCssToFile(Uri url, WebContent content)
        {
            var splitted = url.AbsolutePath.Replace("//", string.Empty).Split('/');
            StringBuilder stringBuilder = new StringBuilder();
            string fileName = string.Empty;
            foreach (var item in splitted)
            {
                if (Regex.IsMatch(item, @"(\w+)(.css)$"))
                {
                    fileName = item;
                }
                else
                {
                    stringBuilder.Append($"{item}/");
                }
            }
            string path = CreateDirectory($"{url.Host}/{stringBuilder.ToString()}");
            string filePath = CreateFilePath(fileName, path);
            if (!File.Exists(filePath))
            {
                WriteFile(filePath, content.Text);
            }
        }

        private void SaveJsToFile(Uri url, WebContent content)
        {
            var splitted = url.AbsolutePath.Replace("//", string.Empty).Split('/');
            StringBuilder stringBuilder = new StringBuilder();
            string fileName = string.Empty;
            foreach (var item in splitted)
            {
                if (Regex.IsMatch(item, @"(\w+)(.js)$"))
                {
                    fileName = item;
                }
                else
                {
                    stringBuilder.Append($"{item}/");
                }
            }
            string path = CreateDirectory($"{url.Host}/{stringBuilder.ToString()}");
            string filePath = CreateFilePath(fileName, path);
            if (!File.Exists(filePath))
            {
                WriteFile(filePath, content.Text);
            }
        }

        private void SaveToFile(Uri url, WebContent content)
        {
            string path = CreateDirectory(url.Host);
            string filePath = CreateFilePath(url.Host, path, content.MediaType);
            if (!File.Exists(filePath))
            {
                WriteFile(filePath, content.Text);
            }
        }

        private void WriteFile(string filePath, string content)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(content);
                }
            }
        }

        private string CreateDirectory(string directoryPath)
        {
            string path = $"{Environment.CurrentDirectory}/{directoryPath}";
            bool isPathExists = Directory.Exists(path);
            if (!isPathExists)
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string CreateFilePath(string absoluteUri, string path, string mediaType = "")
        {
            string fileName = absoluteUri.Replace("/", string.Empty).Replace(":", string.Empty);
            string extention = ExtentionHelper.Create(mediaType);
            return $"{path}/{fileName}{extention}";
        }

        private void StartAnalize(WebContent content, HttpClient httpClient, string baseUrl)
        {
            CQ html = CQ.Create(content.Text);
            FindCss(html, httpClient, baseUrl);
            FindJavaScript(html, httpClient, baseUrl);
            FindAnchors(html, httpClient, baseUrl);
            IsPageReaded = false;
        }

        private void FindCss(CQ html, HttpClient httpClient, string baseUrl)
        {
            Find(html, httpClient, baseUrl, "link", "href", ContentTypes.Css);
        }

        private void FindJavaScript(CQ html, HttpClient httpClient, string baseUrl)
        {
            Find(html, httpClient, baseUrl, "script", "src", ContentTypes.JavaScript);
        }

        private void Find(CQ html, HttpClient httpClient, string baseUrl, string find, string attribute, string typeToCompare)
        {
            var tags = html.Find(find);
            foreach (var item in tags)
            {
                if (item.GetAttribute("type") == typeToCompare)
                {
                    var href = item.GetAttribute(attribute);
                    Read(new Uri($"{baseUrl}{href}"), httpClient);
                }
            }
        }

        private void FindAnchors(CQ html, HttpClient httpClient, string baseUrl)
        {
            var tags = html.Find("a");
            int level = Level;
            for (int l = 0; l <= level; l++)
            {
                foreach (var item in tags)
                {
                    var href = item.GetAttribute("href");
                    Read(new Uri($"{baseUrl}{href}"), httpClient);
                }
                Level -= 1;
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
                    if (content.Headers.ContentType.MediaType != "text/html")
                    {
                        return null;
                    }
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

