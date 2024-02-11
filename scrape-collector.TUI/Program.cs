using scrape_collector;
var scraper = new Scraper("http://books.toscrape.com");
await scraper.Scrape();
Console.WriteLine();