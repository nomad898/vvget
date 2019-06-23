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
            Crawler crawler = new Crawler();
            var result =  crawler.ToTheSite(new Uri(@"http://kino.kz/"), 1);
            foreach (var item in result)
            {
                Console.WriteLine(item.Key);
                Console.WriteLine(item.Value);
            };
            Console.ReadKey(true);
        }
    }
}
