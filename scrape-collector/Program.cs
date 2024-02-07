// See https://aka.ms/new-console-template for more information
using scrape_collector;
using System.Runtime.CompilerServices;

var scraper = new Scraper("http://books.toscrape.com");
await scraper.Scrape();
Console.ReadLine();


