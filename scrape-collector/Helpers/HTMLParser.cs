using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scrape_collector.Helpers
{
    public static class HTMLParser
    {
          public static HashSet<Uri> ParseHTMLforUris(string html, Uri baseUri)
        {
            //var pattern = "<a\\s+(?:[^>]*?\\s+)?href=([\"'])(.*?)\\1";
            var pattern = "(?:href|src)=([\"'])(.*?)\\1";
            var anchors = new HashSet<Uri>();
            foreach (Match match in Regex.Matches(html, pattern, RegexOptions.IgnoreCase))
            {
                if (match.Success)
                {
                    anchors.Add(new Uri(baseUri,match.Groups[2].Value));
                }
            }
            return anchors;
        }
    }
}
