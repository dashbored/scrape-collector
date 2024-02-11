using scrape_collector.Helpers;

namespace scrape_collector.Tests
{
    [TestClass]
    public class HelperTests
    {
        private Uri _baseUri = new Uri("http://example.com");
        
        [TestMethod]
        public void AnchorParsesCorrectly()
        {
            string html = """
                <a href="index.html">
                    <div class="books">
                        <a href="index.html"></a>
                        <a href="catalogue/bookname/index.html"></a>
                        <a href="/catalogue/bookname/index.html"></a>
                    </div>
                </a>
                """;

            var anchors = HTMLParser.ParseHTMLforUris(html,_baseUri);
            var urls = new List<Uri>();
            foreach (var anchor in anchors)
            {
                var newUri = new Uri(_baseUri, anchor);
                urls.Add(newUri);
            }

            Assert.AreEqual(anchors.Count, 2);
            Assert.IsTrue(anchors.Any(a => Uri.Compare(a, new Uri(_baseUri, "index.html"), UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0));
            Assert.IsTrue(anchors.Any(a => Uri.Compare(a, new Uri(_baseUri, "catalogue/bookname/index.html"), UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0));
        }
    }
}