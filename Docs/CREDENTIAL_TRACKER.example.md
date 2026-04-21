# Azure AI Labs — Credentials Tracker (Template)

> 📋 **This is a template file.** It contains no real credentials.
>
> **How to use:**
> 1. Copy this file and rename it `CREDENTIALS_TRACKER.md`
> 2. Fill in your real values as you complete each lab
> 3. Confirm `CREDENTIALS_TRACKER.md` is in your `.gitignore` before filling anything in
> 4. Never commit the filled version — treat it like a password manager entry

---

## Lab 1: Azure OpenAI Service

### Azure Resource Details

| Field | Value |
|---|---|
| Resource Name | `openai-lab-yourname###` |
| Resource Group | `rg-openai-lab` |
| Region | East US |
| Pricing Tier | Standard S0 |
| Base Endpoint URL | `https://openai-lab-yourname###.openai.azure.com/` |
| API Key (KEY 1) | `FILL_IN_AFTER_DEPLOYMENT` |
| Deployment Name | `gpt4o-deployment` |
| Model | `gpt-4o` |
| Tokens Per Minute (TPM) | 10,000 |
| Foundry Deployment Status | `Succeeded / Pending` |

> **Which endpoint to use:** Use the base URL only (e.g. `https://openai-lab-yourname.openai.azure.com/`).
> Do NOT use the full Target URI shown in Foundry — the SDK constructs that path itself.

### User Secrets Commands (Lab 1)

```powershell
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://openai-lab-yourname###.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "FILL_IN_YOUR_KEY_1"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt4o-deployment"
```

---

## Lab 2: Azure AI Search

### Azure Resource Details

| Field | Value |
|---|---|
| Service Name | `aisearch-lab-yourname###` |
| Resource Group | `rg-openai-lab` |
| Region | East US |
| Pricing Tier | Basic |
| Search Endpoint URL | `https://aisearch-lab-yourname###.search.windows.net` |
| Admin API Key (Primary) | `FILL_IN_AFTER_DEPLOYMENT` |
| Query API Key (Optional) | `FILL_IN_IF_USING` |
| Index Name | `hotels-sample-index` |

### User Secrets Commands (Lab 2)

```powershell
dotnet user-secrets set "AzureSearch:Endpoint" "https://aisearch-lab-yourname###.search.windows.net"
dotnet user-secrets set "AzureSearch:ApiKey" "FILL_IN_YOUR_ADMIN_KEY"
dotnet user-secrets set "AzureSearch:IndexName" "hotels-sample-index"
```

---

## Lab 3: NVIDIA NIM on Azure Container Instances

> ⚠️ **COST ALERT:** GPU containers cost ~$1.50–3.00/hour. Stop the container after every session.
> See `COST_GUIDE.md` for stop/start commands and budget alert setup.

### Azure Resource Details

| Field | Value |
|---|---|
| Container Name | `nvidia-nim-lab` |
| Resource Group | `rg-openai-lab` |
| Region | East US |
| DNS Name Label | `nvidia-nim-lab-yourname###` |
| FQDN | `nvidia-nim-lab-yourname###.eastus.azurecontainer.io` |
| API Base URL | `http://FILL_IN_FQDN:8000` |
| Container Image | `nvcr.io/nvidia/nim/meta/llama3-8b-instruct:latest` |
| GPU Type | NVIDIA T4 (1 GPU, 4 CPU, 16 GB RAM) |
| NGC API Key | `FILL_IN_IF_USING_PRIVATE_MODELS` |

### User Secrets Commands (Lab 3)

```powershell
dotnet user-secrets set "NvidiaNim:BaseUrl" "http://FILL_IN_FQDN:8000"
```

### Stop / Start Commands

```powershell
# Stop after each session (REQUIRED)
az container stop --resource-group rg-openai-lab --name nvidia-nim-lab

# Restart when resuming (~5 min model reload)
az container start --resource-group rg-openai-lab --name nvidia-nim-lab
```

---

## Lab 4: AI Chat Application (RAG)

### Azure Resource Details

| Field | Value |
|---|---|
| App Service Name | `chat-app-yourname###` |
| Resource Group | `rg-openai-lab` |
| Region | East US |
| Runtime Stack | .NET 8 (LTS) |
| Pricing Tier | Free F1 |
| App URL | `https://chat-app-yourname###.azurewebsites.net` |

### App Service Configuration Settings

Configure these in Azure Portal → App Service → Configuration → Application settings:

| Setting Key | Value |
|---|---|
| `AzureOpenAI__Endpoint` | Copy from Lab 1 |
| `AzureOpenAI__ApiKey` | Copy from Lab 1 |
| `AzureOpenAI__DeploymentName` | `gpt4o-deployment` |
| `AzureSearch__Endpoint` | Copy from Lab 2 |
| `AzureSearch__ApiKey` | Copy from Lab 2 |
| `AzureSearch__IndexName` | `hotels-sample-index` |

> Note the double underscore `__` in App Service settings. This maps to the colon `:` separator used in local user secrets.

---

## Cost Tracker

Fill this in as you go to track actual vs. estimated spending:

| Date | Service | Action | Duration / Tokens | Actual Cost |
|---|---|---|---|---|
| | Azure OpenAI | Lab 1 testing | | $ |
| | Azure AI Search | Lab 2 (daily rate) | | $ |
| | NVIDIA NIM | Lab 3 session | | $ |
| | App Service | Lab 4 | Free | $0 |
| | | | **Total:** | **$** |

---

## Resource Cleanup Checklist

### After Each Lab Session

- [ ] Lab 3: Stop or delete NVIDIA NIM container **(CRITICAL — hourly billing)**
- [ ] Check Azure Cost Management dashboard
- [ ] Confirm no unexpected charges

### After Project Completion

- [ ] Delete entire resource group: `az group delete --name rg-openai-lab --yes`
- [ ] Verify deletion in Azure Portal
- [ ] Shred or securely delete your filled-in `CREDENTIALS_TRACKER.md`
- [ ] Update your README with final cost summary

---

## Notes

*(Copy this section into your private tracker and use it to log challenges per lab)*

**Lab 1:**

**Lab 2:**

**Lab 3:**

**Lab 4:**

---

*Azure AI Labs — JustJaysAiLabs | Template only — contains no real credentials*
