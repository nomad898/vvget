using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vvget.Library
{
    public static class ExtentionHelper
    {
        public static string Create(string contentType)
        {
            switch (contentType)
            {
                case ContentTypes.Html:
                    return ContentTypes.HtmlExtention;
                case ContentTypes.JavaScriptHeader:
                case ContentTypes.JavaScript:
                    return ContentTypes.JavaScriptExtention;
                case ContentTypes.Css:
                    return ContentTypes.CssExtention;
                default:
                    return string.Empty;
            }
        }
    }
}
