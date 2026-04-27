using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
var searchClient = new SearchClient(
 new Uri(config["AzureSearch:Endpoint"]!),
 config["AzureSearch:IndexName"]!,
 new AzureKeyCredential(config["AzureSearch:ApiKey"]!));
Console.WriteLine("Hotel Search (type 'exit' to quit)\n");
while (true)
{
    Console.Write("Search: ");
    var query = Console.ReadLine();
    if (query?.ToLower() == "exit") break;
    var options = new SearchOptions
    {
        Size = 5,
        Select = { "HotelName", "Description", "Category", "Rating" },
        IncludeTotalCount = true
    };
    SearchResults<SearchDocument> results =
    await searchClient.SearchAsync<SearchDocument>(query, options);
    Console.WriteLine($"\nFound {results.TotalCount} results:\n");

    await foreach (SearchResult<SearchDocument> result in results.GetResultsAsync())
    {
        Console.WriteLine($"✓ {result.Document["HotelName"]} (★{result.Document["Rating"]})");
        Console.WriteLine($" {result.Document["Description"]}\n");
    }
    Console.WriteLine(new string('-', 60));
}