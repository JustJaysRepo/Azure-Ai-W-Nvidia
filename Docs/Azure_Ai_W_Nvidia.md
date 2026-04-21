# Azure AI with NVIDIA — Project Overview

> A four-part AI lab series building toward an enterprise-ready, GPU-accelerated RAG application on Azure. Based on an NVIDIA Hackathon task structure, extended with real-world documentation and operational patterns.

---

## Project Goals

- Build and connect four interoperable AI services on Azure
- Understand the difference between managed inference (Azure OpenAI) and self-hosted GPU inference (NVIDIA NIM)
- Implement a production-pattern RAG (Retrieval-Augmented Generation) system
- Document every deviation, fix, and lesson learned for use as a personal reference and portfolio

---

## Architecture Overview

```
[Browser / Console Client]
        ↓
[Lab 4: Blazor Chat App — App Service]
        ↓                    ↓
[Lab 1: Azure OpenAI]   [Lab 2: Azure AI Search]
   GPT-4o (managed)       hotels-sample-index
        ↓
[Lab 3: NVIDIA NIM — ACI]
   Llama3-8B (self-hosted, GPU)
```

Labs 1 and 2 are the foundation. Lab 3 introduces self-hosted inference as an alternative brain. Lab 4 wires everything together into a user-facing application.

---

## Labs at a Glance

### Lab 1: Azure OpenAI Service + LLM
**Status:** ✅ Complete

**What it does:** Deploys GPT-4o on Azure OpenAI Service and integrates it into a .NET 8 console app with a streaming chat loop.

**Key decisions made:**
- Used `Azure.AI.OpenAI` SDK (not plain `OpenAI`) — required for Azure endpoint/key format
- Used `.Value.Content[0].Text` pattern for SDK 2.x (lab sheet PDF used older 1.x pattern)
- Used base endpoint URL, not the full Target URI from Foundry
- Credentials managed via `dotnet user-secrets` — never committed to Git

**Services deployed:**
- Resource: `openai-lab-JustJay0421`
- Resource Group: `rg-openai-lab`
- Region: East US
- Model: GPT-4o | Deployment: `gpt4o-deployment` | TPM: 10,000

---

### Lab 2: Azure AI Search
**Status:** 🔜 Upcoming

**What it does:** Deploys Azure AI Search, indexes the hotels sample dataset, and implements full-text search from a .NET 8 console app.

**Planned services:**
- Search service: `aisearch-lab-[name]`
- Index: `hotels-sample-index`
- Tier: Basic (~$2.50/day — delete between sessions)

---

### Lab 3: NVIDIA NIM on Azure Container Instances
**Status:** 🔜 Upcoming

**What it does:** Deploys a GPU-accelerated Llama3-8B container via NVIDIA NIM on Azure Container Instances. Provides OpenAI-compatible API endpoints for self-hosted inference.

**Cost warning:** ~$1.50–3.00/hour. Must be stopped after every session.

---

### Lab 4: AI-Powered RAG Chat Application
**Status:** 🔜 Upcoming

**What it does:** Blazor Server web app that combines Labs 1 and 2 into a full RAG pipeline — user asks a question, AI Search retrieves relevant hotel docs, GPT-4o generates a grounded answer with source citations.

---

## Repository Structure

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
│   ├── Azure_Ai_W_Nvidia.md     ← this file
│   ├── SETUP.md
│   ├── COST_GUIDE.md
│   └── TROUBLESHOOTING.md
└── README.md
```

---

## Security Practices

- All API keys and endpoints stored via `dotnet user-secrets` (outside repo, per-project)
- No `.env` files, no hardcoded credentials, no `appsettings.json` secrets
- `CREDENTIALS_TRACKER.md` (local only) kept out of Git via `.gitignore`
- Production pattern for future: replace API keys with Azure Managed Identity

---

## Technology Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 8 |
| Managed LLM | Azure OpenAI (GPT-4o) |
| Search / RAG Memory | Azure AI Search |
| Self-hosted LLM | NVIDIA NIM (Llama3-8B on ACI) |
| Web Frontend | ASP.NET Blazor Server |
| Secret Management | dotnet user-secrets (dev) |
| Infrastructure | Azure Portal + Azure CLI |

---

## Key Concepts

**RAG (Retrieval-Augmented Generation):** A pattern where the LLM's response is grounded in retrieved documents rather than relying on training data alone. Reduces hallucinations and enables domain-specific answers.

**Managed vs. Self-hosted Inference:** Azure OpenAI is fully managed — Microsoft handles infrastructure, scaling, and safety filtering. NVIDIA NIM is self-hosted — you control the container, GPU, and model version, with better cost efficiency at scale.

**Token vs. Hourly Billing:** OpenAI charges per token consumed (idle = $0). GPU containers charge per hour running (idle = expensive). Understanding this distinction drives the cleanup habits in this project.

---

## Lessons Learned

| Lab | Lesson |
|---|---|
| Lab 1 | Azure AI Foundry is now the deployment UI — lab sheet portal steps are outdated |
| Lab 1 | SDK 2.x requires `.Value.Content[0].Text` — not `.Content[0].Text` |
| Lab 1 | Use base endpoint URL, not the full Target URI |
| Lab 1 | First responses are slow due to cold start — not a code issue |

---

*This document is updated progressively as each lab is completed.*
