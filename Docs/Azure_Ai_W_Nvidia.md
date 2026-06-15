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

**What it does:** Deploys GPT-4o on Azure OpenAI Service and integrates it into a .NET console app with a chat loop.

**Key decisions made:**
- Must create resource using **Azure OpenAI** option in the portal — NOT the Foundry (Recommended) option. Foundry creates a project/hub wrapper that does not expose Keys and Endpoint correctly for SDK use.
- Used `Azure.AI.OpenAI` SDK (not plain `OpenAI`) — required for Azure endpoint/key format
- Used `.Value.Content[0].Text` pattern for SDK 2.x (lab sheet PDF used older 1.x pattern)
- Remove `using Azure.AI.OpenAI.Chat` — this namespace no longer exists in SDK 2.x, causes conflicts with `using OpenAI.Chat`
- Used base endpoint URL from Portal → Keys and Endpoint — not the Target URI from Foundry
- Deployment name comes from the name assigned inside Foundry during model deployment — not the resource name or service name
- Credentials managed via `dotnet user-secrets` — never committed to Git
- Installing .NET 9 SDK causes projects to retarget to net9.0 — revert to net8.0 in .csproj if needed

**Services deployed:**
- Resource Group: `rg-openai-lab`
- Region: East US 2 (East US may have quota issues)
- Model: GPT-4o | TPM: 10,000
- Created via: Portal → Create resource → **Azure OpenAI** (not Foundry)

---

### Lab 2: Azure AI Search
**Status:** ✅ Complete

**What it does:** Deploys Azure AI Search, indexes the hotels sample dataset, and implements full-text search from a .NET console app.

**Key decisions made:**
- Use Central US region — East US Basic tier quota fills frequently
- Import data Samples option removed — create index via JSON editor + push docs via PowerShell REST script
- Azure CLI `az search` has no index/document commands — use REST API directly
- Service must be recreated if deleted between sessions — re-run `Push-SampleHotels.ps1` after each recreation
- User secrets must exactly match the new service endpoint after recreation — old endpoint = `No such host` error
- Unicode characters render as `?` in Windows terminals — use ASCII alternatives

**Services deployed:**
- Search service: `aisearch-lab-[name]`
- Index: `hotels-sample-index`
- Region: Central US
- Tier: Basic (~$2.50/day — delete between sessions)

---

### Lab 3: NVIDIA NIM on Azure Container Instances
**Status:** ⏸ Blocked — GPU Unavailable

**What it does:** Deploys a GPU-accelerated Llama3-8B container via NVIDIA NIM on Azure Container Instances. Provides OpenAI-compatible API endpoints for self-hosted inference.

**Blocker:** NVIDIA T4 GPU instances are currently unavailable across all Azure ACI regions including East US, Central US, South Central US, and West US. This is a capacity constraint on Azure's side, not a configuration issue. Lab 3 will be completed when GPU availability returns.

**Important architectural note:** Lab 3 is additive — Lab 4 operates fully without it using GPT-4o as the inference backend. NIM plugs in as an alternative inference layer when GPU capacity is available.

**Cost warning:** ~$1.50–3.00/hour. Must be stopped after every session.

---

### Lab 4: AI-Powered RAG Chat Application
**Status:** ✅ Complete (local) | 🔜 App Service deployment pending

**What it does:** Blazor Web App that combines Labs 1 and 2 into a full RAG pipeline — user asks a question, AI Search retrieves relevant hotel docs, GPT-4o generates a grounded answer.

**Key decisions made:**
- .NET 9 renamed the Blazor Server template — use `dotnet new blazor --interactivity Server` not `dotnet new blazorserver`
- .NET 9 Blazor Web App uses `Components/Pages/Home.razor` not `Pages/Index.razor`
- .NET 9 Program.cs uses `AddRazorComponents().AddInteractiveServerComponents()` not `AddServerSideBlazor()`
- `@rendermode InteractiveServer` directive required on razor pages for interactivity
- No `_Host.cshtml` in .NET 9 template — use `MapRazorComponents<App>().AddInteractiveServerRenderMode()`
- `RagService` registered as `AddSingleton` in DI — injected into razor page
- RAG flow: Search → build context string → pass as system message to GPT-4o → return answer
- App Service deployment uses double-underscore `__` in config key names instead of `:` separator

**Confirmed working:** Hotel queries return AI-generated answers grounded in search results (pool details, food options, ratings).

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
│   └── Push-SampleHotels.ps1
├── Docs/
│   ├── Azure_Ai_W_Nvidia.md     ← this file
│   ├── SETUP.md
│   ├── COST_GUIDE.md
│   ├── TROUBLESHOOTING.md
│   ├── AI_Inference_Decision_Guide.docx
│   └── CREDENTIALS_TRACKER.example.md
└── README.md
```

---

## Security Practices

- All API keys and endpoints stored via `dotnet user-secrets` (outside repo, per-project)
- No `.env` files, no hardcoded credentials, no `appsettings.json` secrets
- `CREDENTIALS_TRACKER.md` (local only) kept out of Git via `.gitignore`
- Exposed keys must be regenerated immediately in the portal — rotate via Keys and Endpoint → Regenerate Key 1
- Production pattern for future: replace API keys with Azure Managed Identity

---

## Technology Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 8 (net9.0 SDK installed but projects target net8.0) |
| Managed LLM | Azure OpenAI (GPT-4o) |
| Search / RAG Memory | Azure AI Search |
| Self-hosted LLM | NVIDIA NIM (Llama3-8B on ACI) — pending GPU availability |
| Web Frontend | ASP.NET Blazor Web App (.NET 9 template, Server interactivity) |
| Secret Management | dotnet user-secrets (dev) |
| Infrastructure | Azure Portal + Azure CLI |

---

## Key Concepts

**RAG (Retrieval-Augmented Generation):** A pattern where the LLM's response is grounded in retrieved documents rather than relying on training data alone. Reduces hallucinations and enables domain-specific answers.

**Managed vs. Self-hosted Inference:** Azure OpenAI is fully managed — Microsoft handles infrastructure, scaling, and safety filtering. NVIDIA NIM is self-hosted — you control the container, GPU, and model version, with better cost efficiency at scale.

**Token vs. Hourly Billing:** OpenAI charges per token consumed (idle = $0). GPU containers charge per hour running (idle = expensive). Understanding this distinction drives the cleanup habits in this project.

**Foundry vs. Azure OpenAI Resource:** Foundry is a project/hub management layer on top of Azure OpenAI. The SDK communicates directly with the underlying Azure OpenAI resource — not Foundry. Always get credentials from Portal → resource → Keys and Endpoint, not from the Foundry UI.

---

## Lessons Learned

| Lab | Lesson |
|---|---|
| Lab 1 | Create resource using Azure OpenAI option — not Foundry (Recommended) |
| Lab 1 | Azure AI Foundry is now the deployment UI — lab sheet portal steps are outdated |
| Lab 1 | SDK 2.x requires `.Value.Content[0].Text` — not `.Content[0].Text` |
| Lab 1 | Remove `using Azure.AI.OpenAI.Chat` — namespace no longer valid in SDK 2.x |
| Lab 1 | Use base endpoint URL from Keys and Endpoint page — not Foundry Target URI |
| Lab 1 | Deployment name = the name given inside Foundry, not the resource or service name |
| Lab 1 | First responses are slow due to cold start — not a code issue |
| Lab 1 | Installing .NET 9 SDK retargets projects to net9.0 — revert .csproj to net8.0 |
| Lab 1 | Exposed API keys must be regenerated immediately via portal |
| Lab 2 | East US Basic tier quota fills up — Central US is a reliable fallback |
| Lab 2 | Import data Samples option removed — create index via JSON editor + REST API |
| Lab 2 | Azure CLI `az search` has no index/document commands — use REST API directly |
| Lab 2 | Unicode characters render as `?` in Windows terminals — use ASCII alternatives |
| Lab 2 | Deleted service = `No such host` error — recreate service and update secrets |
| Lab 2 | Secret typos point to deleted host — always verify with `dotnet user-secrets list` |
| Lab 3 | GPU instances unavailable across all ACI regions — capacity constraint, not config |
| Lab 3 | Lab 4 works without Lab 3 — NIM is additive, not a prerequisite |
| Lab 4 | .NET 9 renamed `blazorserver` template to `blazor --interactivity Server` |
| Lab 4 | .NET 9 Blazor structure differs from .NET 8 — Components/Pages not Pages |
| Lab 4 | `@rendermode InteractiveServer` required on pages for event handling to work |
| Lab 4 | App Service config uses `__` double-underscore instead of `:` for nested keys |

---

*Updated June 15, 2026 — Labs 1, 2, and 4 complete. Lab 3 pending GPU availability.*