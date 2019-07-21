using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vvget.Library
{
    public class CrawlerEventArgs : EventArgs
    {
        public string Message { get; set; }

        public CrawlerEventArgs(string message)
        {
            Message = message;
        }
    }
}
