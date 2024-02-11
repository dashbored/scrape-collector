using scrape_collector.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace scrape_collector
{
    public class Scraper
    {
        private static HttpClient _client = new();
        private Uri _baseAddress;
        private int _maxUrls = 0;
        private int _pageCount = 0;

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

            Task.Run(async () => await StatusUpdate());
            await GetLinkAsync(_baseAddress);
        }


        private async Task StatusUpdate()
        {
            var prevCounter = 0;
            while (true)
            {
                if (_pageCount > prevCounter)
                {
                    Console.Clear();
                    Console.WriteLine($"Pages:{_pageCount} ");
                    prevCounter = _pageCount;
                }

                await Task.Delay(10);
            }
        }

        public async Task GetLinkAsync(Uri url)
        {
            if (_pages.ContainsKey(url.LocalPath))
            {
                return;
            }
            var res = await _client.GetAsync(url);
            var html = await res.Content.ReadAsStringAsync();
            if (!_pages.TryAdd(url.LocalPath, html))
            {
                return;
            }
            Interlocked.Increment(ref _pageCount);
            await SaveAsync(html, url);
            var anchors = HTMLParser.ParseHTMLforAnchors(html);

            var tasks = new List<Task>();
            foreach (var anchor in anchors)
            {
                var uris = new Uri(url, anchor);
                var uri = new Uri(_baseAddress, uris);
                if (_visitedPaths.Contains(uri.LocalPath) || _pages.ContainsKey(uri.LocalPath))
                {
                    continue;
                }

                _visitedPaths.Add(uri.LocalPath);
                tasks.Add(Task.Run(() => GetLinkAsync(uri)));
            }
            await Task.Delay(1);
            await Task.WhenAll(tasks);
        }

        private async Task SaveAsync(string response, Uri path)
        {
            var name = path.Segments.Length > 1 ? path.LocalPath : "index.html";
            if (name[^1] == '\\' || name[^1] == '/')
            {
                name += "index.html";
            }
            if (name[0] == '\\' || name[0] == '/')
            {
                name = name[1..];
            }
            var uri = new Uri(@$"C:\Temp\testfolder\");
            var p = Path.GetFullPath(Path.Combine(uri.LocalPath, name));
            Directory.CreateDirectory(Path.GetDirectoryName(p));
            
            await File.WriteAllTextAsync(p,response);
            await Task.CompletedTask;
        }

       
    }
}
