# Azure AI with NVIDIA — Lab Series

A four-part hands-on lab series for building an AI-powered RAG (Retrieval-Augmented Generation) application on Azure, combining managed cloud AI services with GPU-accelerated self-hosted inference via NVIDIA NIM.

> **This repo serves two purposes:**
> - A working reference for anyone wanting to build the same stack
> - The foundation for a future enterprise-grade AI assistant with monitoring, caching, and company-specific knowledge bases

---

## What You'll Build

By the end of all four labs you'll have a complete AI stack:

```
[Browser]
    ↓
[Lab 4: Blazor Chat App — Azure App Service]
    ↓                        ↓
[Lab 1: Azure OpenAI]   [Lab 2: Azure AI Search]
   GPT-4o (managed)        Document index + search
    ↓
[Lab 3: NVIDIA NIM — Azure Container Instance]
   Llama3-8B (self-hosted, GPU-accelerated)
```

Each lab is independently useful — you can stop after Lab 1 and have a working chat app, or complete all four and have a full RAG pipeline with both managed and self-hosted inference options.

---

## Labs Overview

| Lab | Service | What You Build | Est. Cost |
|---|---|---|---|
| Lab 1 | Azure OpenAI | GPT-4o chat console app | ~$2–5 tokens |
| Lab 2 | Azure AI Search | Document search console app | ~$2.50/day |
| Lab 3 | NVIDIA NIM | Self-hosted Llama3 on GPU | ~$1.50–3.00/hr ⚠️ |
| Lab 4 | Azure App Service | Full RAG chat web app | Free tier |

> See [COST_GUIDE.md](Docs/COST_GUIDE.md) before starting Lab 3. GPU containers bill by the hour.

---

## Prerequisites

- Azure subscription (with Azure OpenAI access approved — [request here](https://aka.ms/oai/access))
- .NET 8 SDK
- Azure CLI (`az`) installed
- Visual Studio 2022+ or VS Code with C# extension
- Git

---

## Getting Started

### 1. Clone the repo

```powershell
git clone https://github.com/yourusername/Azure-Ai-W-Nvidia.git
cd Azure-Ai-W-Nvidia
```

### 2. Set up your credentials tracker

```powershell
# Copy the template — never commit the filled version
copy Docs\CREDENTIALS_TRACKER.example.md CREDENTIALS_TRACKER.md
```

Confirm `CREDENTIAL_TRACKER.md` is listed in `.gitignore` before filling it in.

### 3. Follow the lab setup docs in order

Start with [SETUP.md](Docs/SETUP.md) — it documents the actual steps taken including deviations from the original lab sheets caused by Azure portal and SDK updates.

---

## Repository Structure

```
Azure-Ai-W-Nvidia/
├── Azure-Ai-W-Nvidia.sln
├── Projects/
│   ├── Lab1.AzureOpenAI/        # Console app — GPT-4o chat
│   ├── Lab2.AISearch/           # Console app — document search
│   ├── Lab3.NvidiaNim/          # Console app — self-hosted LLM client
│   └── Lab4.ChatApp/            # Blazor Server — full RAG web app
├── Scripts/
├── Docs/
│   ├── Azure_Ai_W_Nvidia.md     # Project overview and architecture
│   ├── SETUP.md                 # Step-by-step setup with real fixes
│   ├── COST_GUIDE.md            # Cost breakdown and cleanup commands
│   ├── TROUBLESHOOTING.md       # Real issues encountered with verified fixes
│   └── CREDENTIALS_TRACKER.example.md  # Template — copy and fill locally
└── README.md
```

---

## Credential Security

All API keys and endpoints are managed via `dotnet user-secrets` — they are stored in your OS user profile outside the project folder and never touch the repository.

```powershell
# Example for Lab 1 — run from inside the project folder
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-key-here"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt4o-deployment"
```

See [CREDENTIAL_TRACKER.example.md](Docs/CREDENTIALS_TRACKER.example.md) for the full list of secrets needed per lab.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 8 |
| Managed LLM | Azure OpenAI (GPT-4o) |
| Document Search | Azure AI Search |
| Self-hosted LLM | NVIDIA NIM (Llama3-8B) |
| Web Frontend | ASP.NET Blazor Server |
| Secret Management | dotnet user-secrets |
| Hosting | Azure App Service (Free F1) |

---

## Known Deviations from Original Lab Sheets

The lab PDF was written against an earlier Azure portal flow and an older SDK version. These are the changes needed to run everything correctly today:

| Issue | Fix |
|---|---|
| Model deployments now redirect to Azure AI Foundry | Follow Foundry UI — steps documented in SETUP.md |
| `Azure.AI.OpenAI` SDK 2.x changed the response type | Use `.Value.Content[0].Text` not `.Content[0].Text` |
| Foundry shows two endpoint formats | Use the base URL only — SDK builds the full path |
| `using Azure.AI.OpenAI.Chat` no longer valid | Use `using OpenAI.Chat` in SDK 2.x |

Full details with code examples in [TROUBLESHOOTING.md](Docs/TROUBLESHOOTING.md).

---

## Roadmap — Enterprise Version (v2)

This repo is the foundation for a more advanced build. Planned additions:

- [ ] Redis caching layer for frequent queries
- [ ] Application Insights end-to-end telemetry
- [ ] Azure Managed Identity replacing API key auth
- [ ] Custom document ingestion pipeline (your own data, not the sample index)
- [ ] Company-specific knowledge base with access controls
- [ ] Azure AD authentication for the chat app
- [ ] Smart routing — GPT-3.5 for simple queries, GPT-4 for complex
- [ ] Streaming responses for better UX
- [ ] Conversation memory across sessions
- [ ] Terraform / Bicep infrastructure as code

---

## Cost Management — Quick Reference

```powershell
# Stop NIM container after Lab 3 sessions (CRITICAL)
az container stop --resource-group rg-openai-lab --name nvidia-nim-lab

# Full cleanup after project completion
az group delete --name rg-openai-lab --yes
```

See [COST_GUIDE.md](Docs/COST_GUIDE.md) for full breakdown, budget alert setup, and per-service cleanup strategies.

---

## Resources

- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
- [Azure AI Search Documentation](https://learn.microsoft.com/en-us/azure/search/)
- [NVIDIA NIM Documentation](https://docs.nvidia.com/nim/)
- [Azure AI Foundry](https://ai.azure.com)
- [dotnet user-secrets docs](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)

---

*Built by JustJay | Lab series based on NVIDIA Hackathon task structure*
