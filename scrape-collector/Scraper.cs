using scrape_collector.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int _linkCount = 0;
        private bool _done = false;
        private readonly object _lock = new object();
        private readonly char[] _spinner = new char[] { '-','\\','|','/'};
        private int _spinnIndex = 0;

        private static HashSet<string> _visitedPaths = new HashSet<string>();
        private ConcurrentDictionary<string, string> _links = new ConcurrentDictionary<string, string>();
        
        public Scraper(string url)
        {
            _baseAddress = new(url);
        }

        public async Task Scrape()
        {

            var statusUpdater = Task.Run(async () => await StatusUpdate());
            await ScrapeLinkAsync(_baseAddress);            
            _done = true;
            await statusUpdater;
        }


        private async Task StatusUpdate()
        {
            var sw = new Stopwatch();
            sw.Start();
            var header = $"Scraping {_baseAddress}...";
            while (!_done)
            {
                Console.Clear();
                Console.WriteLine(header);
                Console.WriteLine($"Links downloaded[{_spinner[_spinnIndex++%4]}]:{_linkCount} ");

                await Task.Delay(500);
            }

            Console.Clear();
            Console.WriteLine(header);
            Console.WriteLine($"Done scraping. Scraped {_linkCount} links.");
            Console.WriteLine($"Total time elapsed: {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");
            sw.Stop();
            await Task.CompletedTask;
        }

        public async Task ScrapeLinkAsync(Uri url)
        {
            if (_links.ContainsKey(url.LocalPath))
            {
                return;
            }

            var res = await _client.GetAsync(url);
            var html = await res.Content.ReadAsStringAsync();
            if (!_links.TryAdd(url.LocalPath, html))
            {
                return;
            }

            Interlocked.Increment(ref _linkCount);
            await SaveAsync(html, url);
            var anchors = HTMLParser.ParseHTMLforUris(html, url);

            var tasks = new List<Task>();
            foreach (var anchor in anchors)
            {
                var uri = new Uri(_baseAddress, anchor);
                lock (_lock) 
                { 
                    if (_visitedPaths.Contains(uri.LocalPath))
                    {
                        continue;
                    }

                    _visitedPaths.Add(uri.LocalPath);
                }
                tasks.Add(Task.Run(() => ScrapeLinkAsync(uri)));
            }

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
            if (!string.IsNullOrWhiteSpace(p))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p)!);
            
                await File.WriteAllTextAsync(p,response);
            }
        }

       
    }
}
