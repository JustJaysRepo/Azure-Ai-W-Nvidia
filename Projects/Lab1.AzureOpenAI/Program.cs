using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var client = new AzureOpenAIClient(
 new Uri(config["AzureOpenAI:Endpoint"]!),
 new AzureKeyCredential(config["AzureOpenAI:ApiKey"]!));

var chatClient = client.GetChatClient(config["AzureOpenAI:DeploymentName"]!);

Console.WriteLine("Chat Assistant (type 'exit' to quit)\n");
while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();
    if (input?.ToLower() == "exit") break;
    var messages = new List<ChatMessage>
    {
        new SystemChatMessage("You are a helpful assistant."),
        new UserChatMessage(input!)
    };
    var completion = await chatClient.CompleteChatAsync(messages);
    Console.WriteLine($"Assistant: {completion.Value.Content[0].Text}\n");
}