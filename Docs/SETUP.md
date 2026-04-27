# Azure AI W Nvidia — Setup Guide

> This guide documents the actual setup steps as executed during development, including deviations from the original lab sheets due to Azure portal/Foundry updates.

---

## Prerequisites

- Visual Studio Code or Visual Studio 2022+
- .NET 8 SDK
- Azure Subscription (with OpenAI service access approved)
- Git + GitHub account
- PowerShell or Windows Terminal

---

## Solution Structure

```
Azure-Ai-W-Nvidia/
├── Azure-Ai-W-Nvidia.sln
├── Projects/
│   ├── Lab1.AzureOpenAI/
│   ├── Lab2.AISearch/
│   ├── Lab3.NvidiaNim/
│   └── Lab4.ChatApp/
├── Scripts/
├── Docs/
│   ├── SETUP.md              ← this file
│   ├── COST_GUIDE.md
│   ├── TROUBLESHOOTING.md
│   └── Azure_Ai_W_Nvidia.md
└── README.md
```

### Scaffolding the Solution

Run all commands from the `Azure-Ai-W-Nvidia/` root:

```powershell
# Create solution
dotnet new sln -n Azure-Ai-W-Nvidia

# Create Lab 1 project
dotnet new console -n Lab1.AzureOpenAI -o Projects/Lab1.AzureOpenAI

# Add to solution with virtual solution folder
dotnet sln Azure-Ai-W-Nvidia.sln add Projects\Lab1.AzureOpenAI\Lab1.AzureOpenAI.csproj --solution-folder Projects
```

> Repeat the `new` and `sln add` steps for each lab as you reach it. No need to scaffold all four upfront.

---

## Lab 1: Azure OpenAI Service + LLM

### Azure Portal Steps

#### Part 1: Create the OpenAI Resource

1. Navigate to [portal.azure.com](https://portal.azure.com)
2. Click **+ Create a resource** → Search `Azure OpenAI` → Click **Create**
3. Fill in Basics:
   - **Resource Group:** Create new → `rg-openai-lab`
   - **Region:** East US
   - **Name:** `openai-lab-[yourname][digits]` (globally unique)
   - **Pricing Tier:** Standard S0
4. Network → All networks
5. Skip Tags → **Review + create** → **Create**
6. Wait 2–3 minutes → **Go to resource**

#### Part 2: Deploy a Model (Azure AI Foundry — Updated Flow)

> ⚠️ **Portal Change:** As of early 2026, clicking **Model deployments** redirects you to **Azure AI Foundry** instead of handling deployment inline. The steps below reflect the current Foundry flow.

1. From your OpenAI resource, click **Model deployments** → You will be redirected to Azure AI Foundry
2. In Foundry, click **+ Deploy model** → **Deploy base model**
3. Select `gpt-4o` → Click **Confirm**
4. Configure deployment:
   - **Deployment name:** `gpt4o-deployment`
   - **Model version:** Auto-update to default
   - **Deployment type:** Standard
   - **Tokens Per Minute:** 10,000
5. Click **Deploy** → Wait for `Succeeded` status

#### Part 3: Get Credentials

From the Foundry deployment page, or back in the Azure Portal resource under **Keys and Endpoint**:

| Field | Value |
|---|---|
| Endpoint URL | `https://openai-lab-[yourname].openai.azure.com/` |
| API Key | KEY 1 value |
| Deployment Name | `gpt4o-deployment` |

> **Which endpoint to use:** Use the **base endpoint URL** (e.g. `https://openai-lab-yourname.openai.azure.com/`). Do NOT use the full Target URI with `/openai/deployments/...` — the SDK constructs that path itself from the base URL + deployment name.

---

### Project Setup

#### Install Packages

```powershell
cd Projects\Lab1.AzureOpenAI
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Extensions.Configuration.UserSecrets
```

> **SDK Note:** Use `Azure.AI.OpenAI` — not the plain `OpenAI` package. Your Azure endpoint format and API key format only work correctly with the Azure SDK.

#### Configure User Secrets

```powershell
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://openai-lab-yourname.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "YOUR_KEY_1_HERE"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt4o-deployment"
```

> Secrets are stored in `%APPDATA%\Microsoft\UserSecrets\` — completely outside your project folder and never committed to Git.

#### Program.cs

```csharp
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

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
```

> **SDK Version Note:** In `Azure.AI.OpenAI` 2.x, `CompleteChatAsync` returns `ClientResult<ChatCompletion>`. You must access `.Value.Content[0].Text` — not `.Content[0].Text` directly. The lab sheet PDF uses the older pattern; the code above is corrected for current SDK versions.

#### Run

```powershell
dotnet run
```

**Expected output:**
```
Chat Assistant (type 'exit' to quit)
You: Explain Azure in 20 words
Assistant: Azure is Microsoft's cloud platform providing services like computing, storage, AI, and networking for scalable solutions.
```

---

## .gitignore Minimum Requirements

Ensure the following are excluded before pushing:

```
bin/
obj/
*.user
.env
secrets.json
```

User secrets are stored outside the repo by design and do not need to be in `.gitignore`, but the above covers all other common leak points.

---

## Lab 2: Azure AI Search

### Project Scaffolding

Run from the solution root:

```powershell
dotnet new console -n Lab2.AISearch -o Projects\Lab2.AISearch
dotnet sln Azure-Ai-W-Nvidia.sln add Projects\Lab2.AISearch\Lab2.AISearch.csproj --solution-folder Projects
```

### Azure Portal Steps

#### Part 1: Create the Search Service

1. Azure Portal → **+ Create a resource** → Search `Azure AI Search` → **Create**
2. Fill in Basics:
   - **Resource Group:** Use existing → `rg-openai-lab`
   - **Service name:** `aisearch-lab-[yourname][digits]` (globally unique)
   - **Location:** East US (if Basic tier quota is full, use Central US — see Troubleshooting)
   - **Pricing tier:** Basic
3. Scale → Accept defaults (1 replica, 1 partition)
4. Networking → Public endpoint
5. **Review + create** → **Create**
6. Wait 3–5 minutes → **Go to resource**

#### Part 2: Get Credentials

1. Left menu → **Keys**
2. Copy and save:
   - Primary admin key
   - Service URL (`https://aisearch-lab-[name].search.windows.net`)

#### Part 3: Create the Index

> ⚠️ **Portal Change:** The **Import data** wizard no longer includes a Samples option. The **Import and vectorize data** option may also be absent depending on your region. Use the manual JSON approach below.

1. From your search service, click **Add index** (top menu)
2. Switch to JSON view and paste the following schema:

```json
{
  "name": "hotels-sample-index",
  "fields": [
    { "name": "HotelId", "type": "Edm.String", "key": true, "searchable": false },
    { "name": "HotelName", "type": "Edm.String", "searchable": true, "filterable": true },
    { "name": "Description", "type": "Edm.String", "searchable": true },
    { "name": "Category", "type": "Edm.String", "searchable": true, "filterable": true },
    { "name": "Rating", "type": "Edm.Double", "filterable": true, "sortable": true }
  ]
}
```

3. Click **Save**

#### Part 4: Push Sample Documents via PowerShell

Search Explorer is query-only and the Azure CLI `az search` module has no index or document commands. Use this PowerShell REST call to push sample hotel documents directly:

```powershell
$endpoint = "https://aisearch-lab-yourname###.search.windows.net"
$apiKey = "YOUR_ADMIN_KEY_HERE"
$index = "hotels-sample-index"

$body = @{
    value = @(
        @{ "@search.action" = "upload"; HotelId = "1"; HotelName = "Fancy Stay Hotel"; Description = "Luxury hotel with rooftop pool and spa facilities"; Category = "Luxury"; Rating = 5.0 },
        @{ "@search.action" = "upload"; HotelId = "2"; HotelName = "Secret Point Motel"; Description = "Affordable motel with outdoor pool and free breakfast"; Category = "Budget"; Rating = 4.6 },
        @{ "@search.action" = "upload"; HotelId = "3"; HotelName = "Wellness Retreat"; Description = "Boutique hotel specializing in spa packages and massage therapy"; Category = "Boutique"; Rating = 4.8 }
    )
} | ConvertTo-Json -Depth 3

Invoke-RestMethod `
    -Uri "$endpoint/indexes/$index/docs/index?api-version=2024-07-01" `
    -Method POST `
    -Headers @{ "api-key" = $apiKey; "Content-Type" = "application/json" } `
    -Body $body
```

A response containing `@odata.context` confirms documents were indexed successfully.

> This PowerShell script is also saved in `Scripts/Push-SampleHotels.ps1` for reuse when recreating the index between sessions.

#### Part 5: Verify in Search Explorer

1. Left menu → **Search Explorer**
2. Select `hotels-sample-index` from the dropdown
3. Click **Search** with empty query — all 3 hotels should return as JSON

---

### Project Setup

#### Install Packages

```powershell
cd Projects\Lab2.AISearch
dotnet add package Azure.Search.Documents
dotnet add package Microsoft.Extensions.Configuration.UserSecrets
```

#### Configure User Secrets

```powershell
dotnet user-secrets init
dotnet user-secrets set "AzureSearch:Endpoint" "https://aisearch-lab-yourname###.search.windows.net"
dotnet user-secrets set "AzureSearch:ApiKey" "YOUR_ADMIN_KEY"
dotnet user-secrets set "AzureSearch:IndexName" "hotels-sample-index"
```

#### Program.cs

```csharp
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
        Console.WriteLine($">> {result.Document["HotelName"]} | Rating: {result.Document["Rating"]}");
        Console.WriteLine($"   {result.Document["Description"]}\n");
    }

    Console.WriteLine(new string('-', 60));
}
```

> **Terminal Note:** Using `★` or emoji characters in Console.WriteLine will render as `?` in Windows terminals running on the Windows-1252 codepage. Use plain ASCII alternatives like `>>` to avoid this.

#### Run

```powershell
dotnet run
```

**Expected output:**
```
Hotel Search (type 'exit' to quit)
Search: luxury pool
Found 2 results:

>> Fancy Stay Hotel | Rating: 5
   Luxury hotel with rooftop pool and spa facilities

>> Secret Point Motel | Rating: 4.6
   Affordable motel with outdoor pool and free breakfast
```

---

### Cost Management — Lab 2

The Basic tier search service bills ~$2.50/day from the moment it is created regardless of usage. **Delete the service between sessions** if you are pausing more than one day between labs:

```powershell
# Delete search service to stop billing
az search service delete --name aisearch-lab-yourname### --resource-group rg-openai-lab

# Recreate when resuming (then re-run Push-SampleHotels.ps1)
az search service create --name aisearch-lab-yourname### --resource-group rg-openai-lab --sku Basic --location centralus
```

---

## .gitignore Minimum Requirements

Ensure the following are excluded before pushing:

```
bin/
obj/
*.user
.env
secrets.json
CREDENTIALS_TRACKER.md
```

User secrets are stored outside the repo by design and do not need to be in `.gitignore`, but the above covers all other common leak points.

---

*Lab 3 setup steps will be added upon completion of Lab 3.*

