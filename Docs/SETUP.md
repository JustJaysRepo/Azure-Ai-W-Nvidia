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

*Lab 2 setup steps will be added upon completion of Lab 2.*
