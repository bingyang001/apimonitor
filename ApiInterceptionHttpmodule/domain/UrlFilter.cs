using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiInterceptionHttpmodule.domain
{
    public class UrlFilter
    {
        private static readonly String filter = ConfigurationManager.AppSettings["urlExclude"];
        private static readonly String[] declareFilter = new String[] {"/", "/favicon.ico", "/monitor" };

        public static bool isExclude(String url)
        {
            if (declareFilter.Any(l => url.EndsWith(l, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            if (String.IsNullOrEmpty(filter))
            {
                return false;
            }
            return filter
                .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Any(l => url.EndsWith(l, StringComparison.OrdinalIgnoreCase));
        }
    }
}
