# Azure AI W Nvidia — Troubleshooting Guide

> Documents real issues encountered during development with verified fixes. Organized by lab.

---

## Lab 1: Azure OpenAI Service

### Issue: `dotnet sln add` — "Could not find solution or directory"

**Symptom:**
```
Could not find solution or directory `Azure-Ai-W-Nvidia.sln`
```

**Cause:** Running the `sln add` command from inside the `Projects/` subfolder. The `.sln` file lives one level up in the root.

**Fix:** Navigate to the solution root first:
```powershell
cd ..   # go up from Projects/ to Azure-Ai-W-Nvidia/
dotnet sln Azure-Ai-W-Nvidia.sln add Projects\Lab1.AzureOpenAI\Lab1.AzureOpenAI.csproj --solution-folder Projects
```

**Verify you're in the right place:**
```powershell
ls *.sln   # should return Azure-Ai-W-Nvidia.sln
```

---

### Issue: `'ClientResult<ChatCompletion>' does not contain a definition for 'Content'`

**Symptom:**
```
'ClientResult<ChatCompletion>' does not contain a definition for 'Content' and 
no accessible extension method 'Content' accepting a first argument of type 
'ClientResult<ChatCompletion>' could be found
```

**Cause:** SDK version mismatch. The lab sheet PDF was written for an older version of `Azure.AI.OpenAI`. In SDK 2.x, `CompleteChatAsync` returns `ClientResult<ChatCompletion>` — a wrapper type. You must unwrap it with `.Value` before accessing `.Content`.

**Fix:** Change:
```csharp
// ❌ Old pattern (SDK 1.x)
var completion = await chatClient.CompleteChatAsync(messages);
Console.WriteLine(completion.Content[0].Text);
```

To:
```csharp
// ✅ New pattern (SDK 2.x)
var completion = await chatClient.CompleteChatAsync(messages);
Console.WriteLine(completion.Value.Content[0].Text);
```

**Also check your usings** — in SDK 2.x, chat types moved to the base `OpenAI` namespace:
```csharp
using OpenAI.Chat;          // ✅ correct for SDK 2.x
// using Azure.AI.OpenAI.Chat;  // ❌ lab sheet PDF — no longer valid
```

---

### Issue: Wrong endpoint used for user secrets

**Symptom:** `ResourceNotFound` or `DeploymentNotFound` errors at runtime.

**Cause:** Azure AI Foundry now shows two endpoint formats. Using the full "Target URI" breaks the SDK because it double-constructs the path.

**Clarification:**

| Endpoint Type | Example | Use? |
|---|---|---|
| Base Endpoint URL | `https://openai-lab-yourname.openai.azure.com/` | ✅ Use this |
| Target URI (full path) | `https://.../openai/deployments/gpt4o-deployment/chat/completions?api-version=...` | ❌ SDK builds this itself |

**Fix:**
```powershell
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://openai-lab-yourname.openai.azure.com/"
```

---

### Issue: Slow first responses (~60 seconds)

**Symptom:** First 1–2 queries take 30–60 seconds even for simple inputs like "what is 2+2".

**Cause:** This is expected behavior, not a code issue. Three factors contribute:
1. **Cold start** — Azure spins up a model instance for your resource on first request
2. **10K TPM rate limit** — minimum allocation gets lower scheduling priority on shared infrastructure
3. **East US regional load** — most popular region, shared capacity varies

**Behavior:** Responses warm up noticeably after the first 2–3 requests in a session.

**To diagnose vs. verify:** Add a timer around the API call:
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
var completion = await chatClient.CompleteChatAsync(messages);
sw.Stop();
Console.WriteLine($"Assistant: {completion.Value.Content[0].Text}\n");
Console.WriteLine($"[{sw.ElapsedMilliseconds}ms]\n");
```

If times are consistently >10 seconds after warmup, check: TPM limit in Foundry, region availability, and whether the deployment status is still `Succeeded`.

---

### Issue: Azure Portal no longer shows Model Deployments inline

**Symptom:** Clicking **Model deployments** in the Azure Portal OpenAI resource redirects to Azure AI Foundry instead of showing a deployment panel.

**Cause:** Azure migrated model deployment management to Azure AI Foundry as of early 2026. This is a portal UI change, not an error.

**Fix:** Complete the deployment through Foundry:
1. Accept the redirect to Foundry
2. Click **+ Deploy model** → **Deploy base model**
3. Select `gpt-4o`, configure as normal
4. Return to the Azure Portal resource for **Keys and Endpoint** — those remain in the original portal view

---

## General

### Checking installed package versions
```powershell
dotnet list package
```

### Verifying user secrets are set
```powershell
dotnet user-secrets list
```

### Regenerating an API key (if 401 Unauthorized)
Azure Portal → Your OpenAI resource → Keys and Endpoint → Regenerate Key 1 → Update user secret:
```powershell
dotnet user-secrets set "AzureOpenAI:ApiKey" "NEW_KEY_HERE"
```

---

*Lab 2 troubleshooting entries will be added upon completion of Lab 2.*
