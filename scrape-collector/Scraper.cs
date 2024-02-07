using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace scrape_collector
{
    internal class Scraper
    {
        private static HttpClient _client = new();
        private Uri _baseAddress;
        private int _maxUrls = 0; 

        private static HashSet<string> _visitedPaths = new HashSet<string>();
        private ConcurrentQueue<Uri> _urls = new ConcurrentQueue<Uri>();
        private ConcurrentDictionary<string, string> _pages = new ConcurrentDictionary<string, string>();
        public Scraper(string url)
        {
            _baseAddress = new(url);
            _urls.Enqueue(_baseAddress);
        }

        public async Task Scrape()
        {
            while (_urls.Count > 0)
            {
                _maxUrls = Math.Max(_maxUrls, _urls.Count);
                Console.Clear();
                Console.WriteLine($"Urls:{_urls.Count} Pages:{_pages.Count} Max Urls: {_maxUrls}");
                if (_urls.TryDequeue(out Uri url))
                {
                    if (_pages.ContainsKey(url.LocalPath))
                    {
                        continue;
                    }
                    var res = await _client.GetAsync(url);
                    var html = await res.Content.ReadAsStringAsync();
                    if(!_pages.TryAdd(url.LocalPath, html))
                    {
                        continue;
                    }

                    await Save(html);
                    var anchors = ParseHTMLforAnchors(html);

                    foreach (var anchor in anchors)
                    {
                        var uri = new Uri(_baseAddress, anchor);
                        if(_visitedPaths.Contains(uri.LocalPath))
                        {
                            continue;
                        }
                        _visitedPaths.Add(uri.LocalPath);

                        if (_pages.ContainsKey(uri.LocalPath))
                        {
                            continue;
                        }
                         _urls.Enqueue(uri);
                        
                    }
                }
            }
        }

        private async Task Save(string response)
        {
            await Task.CompletedTask;
        }

        public HashSet<string> ParseHTMLforAnchors(string html)
        {
            var pattern = "<a\\s+(?:[^>]*?\\s+)?href=([\"'])(.*?)\\1";
            var anchors = new HashSet<string>();
            foreach (Match match in Regex.Matches(html, pattern, RegexOptions.IgnoreCase))
            {
                if (match.Success)
                {
                    anchors.Add(match.Groups[2].Value);
                }
            }
            return anchors;
        }
    }
}
