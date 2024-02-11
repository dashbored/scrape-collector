using scrape_collector.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace scrape_collector
{
    public class Scraper(string url)
    {
        private static readonly HttpClient _client = new();
        private readonly Uri _baseAddress = new(url);
        private int _linkCount = 0;
        private int _spinIndex = 0;
        private bool _done = false;
        private readonly object _lock = new();
        private readonly char[] _spinner = ['-', '\\', '|', '/'];

        private static readonly HashSet<string> _visitedPaths = [];
        private readonly ConcurrentDictionary<string, string> _links = new();

        public async Task Scrape(DirectoryInfo outputFolder)
        {
            var statusUpdater = Task.Run(async () => await StatusUpdate());
            await ScrapeLinkAsync(_baseAddress, outputFolder);
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
                Console.WriteLine($"Links downloaded[{_spinner[_spinIndex++%4]}]:{_linkCount} ");

                await Task.Delay(500);
            }

            Console.Clear();
            Console.WriteLine(header);
            Console.WriteLine($"Done scraping. Scraped {_linkCount} links.");
            Console.WriteLine($"Total time elapsed: {sw.Elapsed.ToString(@"hh\:mm\:ss\.fff")}");
            sw.Stop();
            await Task.CompletedTask;
        }

        private async Task ScrapeLinkAsync(Uri url,DirectoryInfo outputFolder)
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
            await SaveAsync(html, url, outputFolder);
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
                tasks.Add(Task.Run(() => ScrapeLinkAsync(uri, outputFolder)));
            }

            await Task.WhenAll(tasks);
        }

        public async Task SaveAsync(string response, Uri path, DirectoryInfo outputFolder)
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

            var uri = new Uri(outputFolder.FullName);
            var p = Path.GetFullPath(Path.Combine(uri.LocalPath, name));
            if (!string.IsNullOrWhiteSpace(p))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(p)!);
            
                await File.WriteAllTextAsync(p,response);
            }
        }

       
    }
}
