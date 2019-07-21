using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using vvget.Library;

namespace vvget
{
    class Program
    {
        static void Main(string[] args)
        {
            Crawler crawler = new Crawler()
            {
                IsVerbose = true
            };
            crawler.Downloaded += Show;
            crawler.ToTheSite(new Uri(@"http://kino.kz/"), 1);
            Console.ReadKey(true);
        }
        static void Show(object sender, CrawlerEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
