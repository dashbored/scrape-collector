using scrape_collector;

if (args.Count() < 4)
{
    Help();
    return;
}

var (baseDomain, root) = ParseArguments(args);
if (baseDomain is null || root is null)
{
    Help();
    return;
}

ClearPath(root);
var scraper = new Scraper(baseDomain);
await scraper.Scrape(root);
Console.ReadLine();


static void Help()
{
    Console.WriteLine("");
    Console.WriteLine("scrape-collector");
    Console.WriteLine("The simple web scraper");
    Console.WriteLine("Usage: scrape [--url] [--root]");
    Console.WriteLine("");
    Console.WriteLine("-u | --url: Path to webpage to scrape");
    Console.WriteLine("-r | --root: Path to save the scraped links");
    Console.WriteLine("");
    Console.WriteLine("Example: scrape -u https://example.com -r C:\\scrape-collector\\");
}

static (string?, DirectoryInfo?) ParseArguments(string[] args)
{
    string urlString = "";
    string root = "";
    for (int i = 0; i < args.Length; i += 2)
    {
        if (args[i] == "-u" || args[i] == "--url")
        {
            urlString = args[i + 1];
        }
        else if (args[i] == "-r" || args[i] == "--root")
        {
            root = args[i + 1];
        }
        else
        {
            return (null, null);
        }
    }
    try
    {
        return (urlString, new DirectoryInfo(root));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unable to parse arguments, aborting");
        Console.WriteLine(ex.Message);
        return (null, null);
    }
}

static void ClearPath(DirectoryInfo root)
{

    if (root.GetFiles().Length > 0)
    {
        while (true)
        {
            Console.WriteLine($"Root: {root.FullName}");
            Console.WriteLine($"The root folder needs to be empty before scraping.");
            Console.WriteLine("Empty now? [Y/N]");
            var response = Console.ReadLine();

            if (response!.Equals("yes", StringComparison.InvariantCultureIgnoreCase) || response.Equals("y", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var file in root.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (var subDir in root.EnumerateDirectories())
                {
                    subDir.Delete(true);
                }
                break;
            }
            else if (response.Equals("no", StringComparison.InvariantCultureIgnoreCase) || response.Equals("n", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Aborting...");
                Environment.Exit(0);
            }
            Console.WriteLine($"Unable to process '{response}'");
        }
    }
}