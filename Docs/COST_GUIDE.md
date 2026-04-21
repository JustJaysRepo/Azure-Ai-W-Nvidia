# Azure AI W Nvidia — Cost Guide

> Track and manage spending across all four labs. These are real costs — read before deploying anything.

---

## Cost Summary by Service

| Service | Billing Model | Est. Daily Cost | Risk Level |
|---|---|---|---|
| Azure OpenAI (GPT-4o) | Per token consumed | ~$2–5 active use | 🟡 Low–Medium |
| Azure AI Search (Basic) | Per day provisioned | ~$2.50/day | 🟡 Medium |
| NVIDIA NIM (GPU Container) | Per hour running | ~$36–72/day | 🔴 **Critical** |
| App Service (Free F1) | Free tier | $0 | 🟢 None |

---

## Lab 1: Azure OpenAI

**Billing model:** Token consumption — you pay only when the model processes text.

**GPT-4o pricing (as of Feb 2026):**
- Input tokens: ~$0.03 per 1K tokens
- Output tokens: ~$0.06 per 1K tokens

**What 10K TPM actually means:**
- TPM = Tokens Per Minute — this is your **rate cap**, not a charge
- At 10K TPM you can process up to ~150K tokens per 15 minutes maximum
- Idle deployments with no requests = $0

**Cost controls:**
- During active development: costs are minimal for testing — a few dollars at most
- For multi-week breaks: delete the model deployment (not the resource) from Foundry
  - This stops all token charges while keeping your resource and credentials intact
  - Redeploy the model in ~2 minutes when resuming

```powershell
# Delete just the deployment to pause costs (keeps the resource)
az cognitiveservices account deployment delete \
  --resource-group rg-openai-lab \
  --name openai-lab-yourname \
  --deployment-name gpt4o-deployment
```

---

## Lab 2: Azure AI Search

**Billing model:** Daily rate — you pay whether or not you're actively searching.

- Basic tier: ~$75/month = ~$2.50/day prorated
- Starts billing the moment the service is created

**Cost controls:**
- You cannot "pause" a search service — you pay daily while it exists
- For breaks longer than 3 days: delete the service entirely, recreate when resuming
  - Recreating takes ~5 minutes; re-indexing takes ~2 minutes for the sample data
- Deleting indexes (not the service) saves minor storage costs but not the daily rate

```powershell
# Delete search service entirely to stop billing
az search service delete --resource-group rg-openai-lab --name aisearch-lab-yourname
```

---

## Lab 3: NVIDIA NIM (GPU Container)

> ⚠️ **This is the highest risk service in the project. Read carefully.**

**Billing model:** Hourly while running — GPU containers charge continuously.

- NVIDIA T4 GPU on ACI: ~$1.50–3.00/hour
- If left running overnight: ~$36–72
- If left running over a weekend: ~$250–500+

**Required habits for Lab 3:**

After EVERY session — Stop the container, do not just close your browser:

```powershell
# Stop (not delete) — preserves config, cheaper to restart
az container stop --resource-group rg-openai-lab --name nvidia-nim-lab

# Restart when resuming (model reloads in ~5 mins)
az container start --resource-group rg-openai-lab --name nvidia-nim-lab
```

For breaks longer than a day — Delete entirely:

```powershell
az container delete --resource-group rg-openai-lab --name nvidia-nim-lab --yes
```

**Set a budget alert before starting Lab 3:**
1. Azure Portal → Cost Management → Budgets → + Add
2. Set amount: $20/month for the project
3. Alert at 80% and 100%
4. This gives you an email before costs spiral

---

## Lab 4: App Service

- Free F1 tier: **$0** — no action needed
- The only costs in Lab 4 come from OpenAI token usage (tracked under Lab 1 above)

---

## Complete Cleanup (All Labs)

When the project is finished, one command removes everything:

```powershell
az group delete --name rg-openai-lab --yes
```

This deletes: OpenAI resource, AI Search service, any remaining containers, App Service — everything under that resource group. Verify deletion in the portal afterward.

---

## Actual Cost Tracker

Fill this in as you go:

| Date | Service | Action | Tokens/Hours | Actual Cost |
|---|---|---|---|---|
| | Azure OpenAI | Lab 1 testing | | $ |
| | Azure AI Search | Lab 2 (daily) | | $ |
| | NVIDIA NIM | Lab 3 session | | $ |
| | | | **Total:** | **$** |

---

## Cost Monitoring Links

- Azure Cost Management: [portal.azure.com/#blade/Microsoft_Azure_CostManagement](https://portal.azure.com/#blade/Microsoft_Azure_CostManagement)
- OpenAI token usage: Azure Portal → Your OpenAI resource → Monitoring → Metrics → Generated Tokens

---

*Update this tracker after each lab session.*
