using scrape_collector.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;


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
            object html;
            if (res.Content.Headers.ContentType is null || res.Content.Headers.ContentType.ToString().StartsWith("text"))
            {
                html = await res.Content.ReadAsStringAsync();
            }
            else
            {
                html = await res.Content.ReadAsByteArrayAsync();
            }
            if (!_links.TryAdd(url.LocalPath, html.ToString()))
            {
                return;
            }

            Interlocked.Increment(ref _linkCount);
            await SaveAsync(html, url, outputFolder);
            HashSet<Uri> anchors = new HashSet<Uri>();
            if (html is string)
            {
                anchors = HTMLParser.ParseHTMLforUris(html as string, url);
            }

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

        public async Task SaveAsync<T>(T response, Uri path, DirectoryInfo outputFolder)
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
                if (response is string)
                {
                    await File.WriteAllTextAsync(p,response as string);
                }
                else
                {
                    await File.WriteAllBytesAsync(p,response as byte[]);
                }
            }
        }

       
    }
}
