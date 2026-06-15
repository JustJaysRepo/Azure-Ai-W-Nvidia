using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI.Chat;

namespace Lab4.ChatApp.Services;

public class RagService
{
    private readonly ChatClient _chatClient;
    private readonly SearchClient _searchClient;

    public RagService(IConfiguration config)
    {
        var openAiClient = new AzureOpenAIClient(
            new Uri(config["AzureOpenAI:Endpoint"]!),
            new AzureKeyCredential(config["AzureOpenAI:ApiKey"]!));

        _chatClient = openAiClient.GetChatClient(config["AzureOpenAI:DeploymentName"]!);

        _searchClient = new SearchClient(
            new Uri(config["AzureSearch:Endpoint"]!),
            config["AzureSearch:IndexName"]!,
            new AzureKeyCredential(config["AzureSearch:ApiKey"]!));
    }

    public async Task<string> AskAsync(string question)
    {
        // Step 1 — search for relevant hotels
        var searchOptions = new SearchOptions
        {
            Size = 3,
            Select = { "HotelName", "Description", "Category", "Rating" }
        };

        var searchResults = await _searchClient.SearchAsync<SearchDocument>(question, searchOptions);

        // Step 2 — build context from search results
        var context = new System.Text.StringBuilder();
        await foreach (var result in searchResults.Value.GetResultsAsync())
        {
            context.AppendLine($"Hotel: {result.Document["HotelName"]}");
            context.AppendLine($"Category: {result.Document["Category"]}");
            context.AppendLine($"Rating: {result.Document["Rating"]}");
            context.AppendLine($"Description: {result.Document["Description"]}");
            context.AppendLine();
        }

        // Step 3 — send to GPT-4o with context
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a helpful hotel concierge assistant. " +
                "Answer questions using only the hotel information provided below. " +
                "If the answer is not in the provided information, say so.\n\n" +
                "Hotel Information:\n" + context.ToString()),
            new UserChatMessage(question)
        };

        var completion = await _chatClient.CompleteChatAsync(messages);
        return completion.Value.Content[0].Text;
    }
}