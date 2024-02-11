using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scrape_collector.Tests
{
    [TestClass]
    public class ScrapeTests
    {
        [TestMethod]
        public async Task ScraperSavesCorrectly()
        {
            var scraper = new Scraper( @"http:\\localhost");
            var html = """
                <html>
                    <body>
                        <div>
                        </div>
                    </body>
                </html>
                """;
            var tempDir = Directory.CreateTempSubdirectory();
            var path = Path.Combine(tempDir.FullName, "bookshelf/test.html");
            await scraper.SaveAsync<string>(html, new Uri(path), tempDir);

            Assert.IsTrue(File.Exists(tempDir.FullName + "\\bookshelf\\test.html"));
        }
    }
}
