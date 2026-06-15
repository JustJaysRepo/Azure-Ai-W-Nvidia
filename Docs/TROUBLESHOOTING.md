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
using OpenAI.Chat;             // ✅ correct for SDK 2.x
// using Azure.AI.OpenAI.Chat; // ❌ no longer valid — remove this line
```

---

### Issue: `HTTP 400 — API version not supported`

**Symptom:**
```
System.ClientModel.ClientResultException: HTTP 400 (BadRequest)
API version not supported
```

**Cause:** The `using Azure.AI.OpenAI.Chat` namespace conflicts with `using OpenAI.Chat` in SDK 2.x. The stale namespace reference causes the client to construct requests with an unsupported API version.

**Fix:** Remove `using Azure.AI.OpenAI.Chat` entirely. Your usings should be:
```csharp
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
```

No other code changes required. The `Azure.AI.OpenAI.Chat` namespace does not exist in SDK 2.x.

---

### Issue: `Access denied due to invalid subscription key or wrong API endpoint`

**Symptom:**
```
Access denied due to invalid subscription key or wrong API endpoint.
Make sure to provide a valid key for an active subscription and use a correct
regional API endpoint for your resource.
```

**Cause:** One of three things — wrong endpoint, wrong key, or the resource was deleted and recreated with new credentials that haven't been updated in user secrets.

**Diagnosis:** Run `dotnet user-secrets list` and compare each value against Portal → your OpenAI resource → Keys and Endpoint.

**Fix:**
```powershell
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://exact-name-from-portal.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "fresh-key-from-portal"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "exact-deployment-name-from-foundry"
```

**Common mistake:** Using the Foundry project name or hub name as the deployment name. The deployment name is the value entered in the "Deployment name" field inside Foundry when the model was deployed — not the resource name, not the project name.

---

### Issue: Wrong endpoint used for user secrets

**Symptom:** `ResourceNotFound` or `DeploymentNotFound` errors at runtime.

**Cause:** Azure AI Foundry shows multiple endpoint formats and resource names. Using the wrong one breaks the SDK.

**Clarification — three things that look like endpoints but aren't what you need:**

| What you see | Where | Use it? |
|---|---|---|
| Base Endpoint URL | Portal → resource → Keys and Endpoint | ✅ This is what you need |
| Target URI (full path) | Foundry deployment page | ❌ SDK builds this itself |
| Foundry project endpoint | Foundry → project settings | ❌ Not for direct SDK use |

**Fix:**
```powershell
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://openai-lab-yourname.openai.azure.com/"
```

---

### Issue: Created resource via Foundry instead of Azure OpenAI

**Symptom:** No Keys and Endpoint page visible. Resource shows as a Project or Hub type in the portal. SDK cannot connect.

**Cause:** The Azure portal now shows **Foundry (Recommended)** as the first option when creating AI resources. Foundry creates a project/hub wrapper — it is a management layer, not a raw Azure OpenAI resource. The SDK needs the underlying Azure OpenAI resource, which has its own Keys and Endpoint page.

**Fix:** Create a new resource using **Azure OpenAI** (not Foundry):
- Portal → Create a resource → search `Azure OpenAI` → select **Azure OpenAI** (the non-Foundry option)
- This creates a standalone resource with its own Keys and Endpoint page
- You can still use Foundry to deploy models into this resource — just get credentials from the resource itself, not from Foundry

---

### Issue: .NET 9 SDK installed — projects retarget to net9.0

**Symptom:** After installing .NET 9 SDK, `dotnet list package` shows `[net9.0]` and packages resolve to versions incompatible with the project's intended .NET 8 target.

**Cause:** Installing a newer .NET SDK can cause new projects scaffolded afterward to target the latest SDK version. Existing projects may also be affected depending on global.json settings.

**Fix:** Open the `.csproj` file and change the target framework back:
```xml
<!-- Change this -->
<TargetFramework>net9.0</TargetFramework>

<!-- To this -->
<TargetFramework>net8.0</TargetFramework>
```

Then restore:
```powershell
dotnet restore
```

---

### Issue: Slow first responses (~60 seconds)

**Symptom:** First 1–2 queries take 30–60 seconds even for simple inputs like "what is 2+2".

**Cause:** Expected behavior. Three factors contribute:
1. **Cold start** — Azure spins up a model instance for your resource on first request
2. **10K TPM rate limit** — minimum allocation gets lower scheduling priority on shared infrastructure
3. **East US regional load** — most popular region, shared capacity varies

**Behavior:** Responses warm up noticeably after the first 2–3 requests in a session.

---

### Issue: Azure Portal no longer shows Model Deployments inline

**Symptom:** Clicking **Model deployments** in the Azure Portal OpenAI resource redirects to Azure AI Foundry.

**Cause:** Azure migrated model deployment management to Azure AI Foundry as of early 2026. This is a portal UI change, not an error.

**Fix:** Complete the deployment through Foundry, then return to the Azure Portal resource for Keys and Endpoint credentials.

---

### Issue: API key exposed in chat or terminal output

**Symptom:** API key visible in pasted terminal output or shared conversation.

**Fix:** Regenerate immediately:
- Portal → Your OpenAI resource → Keys and Endpoint → Regenerate Key 1
- Update user secret with new key:
```powershell
dotnet user-secrets set "AzureOpenAI:ApiKey" "NEW_KEY_HERE"
```

The old key is invalidated the moment you regenerate. Any running instances using the old key will start failing with 401 errors.

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

## Lab 2: Azure AI Search

### Issue: East US region unavailable for Basic tier

**Symptom:** Creating a Basic tier search service in East US fails with a quota or availability error.

**Cause:** East US is the most popular Azure region and Basic tier search quota fills up frequently on shared infrastructure.

**Fix:** Select **Central US** during resource creation. All lab functionality works identically regardless of region.

---

### Issue: Samples option missing from Import data wizard

**Symptom:** The Import data wizard only shows storage and database source options — no Samples entry.

**Cause:** Microsoft removed the built-in Samples option from the Import data wizard.

**Fix:** Create the index manually using the **Add index** JSON editor in the portal, then push documents via PowerShell REST call. Full steps in `SETUP.md` Lab 2 section.

---

### Issue: `az search index create` not recognized

**Symptom:**
```
'index' is misspelled or not recognized by the system.
```

**Cause:** The Azure CLI `az search` module only manages service-level infrastructure. It has no commands for index or document management.

**Fix:** Use PowerShell `Invoke-RestMethod` to call the search REST API directly. Script saved as `Scripts/Push-SampleHotels.ps1`.

---

### Issue: Special characters render as `?` in terminal output

**Symptom:** Characters like `★`, `✓`, or emoji in `Console.WriteLine` output appear as `?`.

**Cause:** Windows terminals default to the Windows-1252 codepage which cannot render Unicode characters.

**Fix:** Use plain ASCII alternatives:
```csharp
Console.WriteLine($">> {hotelName} | Rating: {rating}");
```

---

### Issue: `No such host is known` after service recreation

**Symptom:**
```
System.AggregateException: Retry failed after 4 tries.
No such host is known. (aisearch-lab-yourname.search.windows.net:443)
```

**Cause:** The AI Search service was deleted (to stop billing) and the user secrets still point to the old deleted service endpoint.

**Fix:**
1. Recreate the search service in the portal
2. Recreate the index via JSON editor
3. Re-run `Push-SampleHotels.ps1` with the new endpoint and key
4. Update user secrets:
```powershell
dotnet user-secrets set "AzureSearch:Endpoint" "https://your-new-service.search.windows.net"
dotnet user-secrets set "AzureSearch:ApiKey" "YOUR_NEW_ADMIN_KEY"
```

**Verify secrets are updated:**
```powershell
dotnet user-secrets list
```

If the old hostname still appears, the update didn't take — run the set command again from inside the correct project folder.

---

## Lab 3: NVIDIA NIM on Azure Container Instances

### Issue: No GPUs available in any ACI region

**Symptom:** Portal shows "No GPUs are available for the [region] region" on the container size configuration page. Confirmed across East US, Central US, South Central US, West US, East US 2.

**Cause:** GPU-enabled container instances on Azure ACI have limited capacity that fluctuates. This is a platform availability constraint, not a configuration error.

**Workaround options:**
1. Wait and retry — GPU availability restores periodically, South Central US and West US tend to recover first
2. Skip to Lab 4 — NIM is additive, Lab 4 works fully with GPT-4o alone
3. Use NVIDIA's hosted NIM endpoint at `https://integrate.api.nvidia.com` — same OpenAI-compatible API, no GPU required on your end, requires free NVIDIA developer account

**Status:** Lab 3 skipped for now. Lab 4 completed without NIM. Will return when capacity is available.

---

## Lab 4: Blazor Chat App

### Issue: `blazorserver` template not found in .NET 9

**Symptom:**
```
No templates or subcommands found matching: 'blazorserver'
```

**Cause:** .NET 9 renamed and restructured the Blazor templates.

**Fix:**
```powershell
dotnet new blazor -n Lab4.ChatApp -o Projects\Lab4.ChatApp --interactivity Server
```

---

### Issue: .NET 9 Blazor project structure differs from .NET 8

**Symptom:** Code examples referencing `Pages/Index.razor`, `_Host.cshtml`, or `AddServerSideBlazor()` don't match the scaffolded project structure.

**Cause:** .NET 9 Blazor Web App template uses a unified component model with a different folder structure and different Program.cs wiring.

**Key differences:**

| .NET 8 Blazor Server | .NET 9 Blazor Web App |
|---|---|
| `Pages/Index.razor` | `Components/Pages/Home.razor` |
| `Pages/_Host.cshtml` | Not present |
| `AddServerSideBlazor()` | `AddRazorComponents().AddInteractiveServerComponents()` |
| `MapBlazorHub()` | `MapRazorComponents<App>().AddInteractiveServerRenderMode()` |
| No render mode needed | `@rendermode InteractiveServer` required |

**Fix:** Use the correct Program.cs wiring:
```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

app.MapRazorComponents<Lab4.ChatApp.Components.App>()
    .AddInteractiveServerRenderMode();
```

And add render mode directive to interactive pages:
```razor
@rendermode InteractiveServer
```

---

### Issue: Blazor page loads but buttons and inputs do not respond

**Symptom:** Page renders but clicking Send or pressing Enter does nothing. No errors in console.

**Cause:** Missing `@rendermode InteractiveServer` directive on the razor page. Without it the page renders as static HTML with no SignalR connection for interactivity.

**Fix:** Add to the top of the razor page, below `@page`:
```razor
@page "/"
@rendermode InteractiveServer
```

---

*Updated June 15, 2026 — Labs 1, 2, and 4 complete. Lab 3 pending GPU availability.*