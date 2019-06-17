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
            Crawler crawler = new Crawler(new HttpClient());
            var result =  crawler.ToTheSite(new Uri(@"http://kino.kz/"));
            Console.WriteLine(result);
            Console.ReadKey(true);
        }
    }
}
