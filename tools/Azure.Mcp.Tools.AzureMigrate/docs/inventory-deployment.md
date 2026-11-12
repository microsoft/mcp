# Deploying the Azure Migrate inventory tools to Azure App Service (Microsoft tenant)

This guide hosts the Azure MCP Server on Azure App Service (Linux) in the Microsoft corporate tenant
(`72f988bf-86f1-41af-91ab-2d7cd011db47`) and exposes only the two Azure Migrate inventory tools:

| Tool | Purpose |
|---|---|
| `azuremigrate_inventory_createorupdate` | Creates (or reuses) the caller's inventory session on the Azure Migrate Discovery Service and returns a write-only upload URL and an `azcopy` command. |
| `azuremigrate_inventory_status` | Returns the status of the inventory operation. |

## How authentication works

```
MCP client ──(user token, aud = MCP app)──▶ Azure MCP Server (App Service) ──(same token, unchanged)──▶ Discovery Service /mcprestapi
```

- The MCP server validates the inbound Entra ID token (`AzureAd__*` settings).
- The inventory tools forward the **same** bearer token to the Discovery Service. No On-Behalf-Of exchange happens,
  so the MCP server's app registration must be the audience the Discovery Service accepts.
- Tokens and SAS query strings are never logged.
- The Discovery Service endpoint is fixed in `DsBackdoorClient.BaseUrl`.

## Prerequisites

- Azure CLI signed in to the Microsoft tenant: `az login --tenant 72f988bf-86f1-41af-91ab-2d7cd011db47`.
- A subscription in the Microsoft tenant where you can create a resource group and an App Service.
- .NET SDK matching `global.json` (to publish the server).
- An Entra ID app registration in the Microsoft tenant (`<MCP_APP_ID>`) that:
  - exposes the delegated scope `Mcp.Tools.ReadWrite` (Application ID URI `api://<MCP_APP_ID>`);
  - is configured on the Discovery Service as the MCP audience (`Aad.Mcp.Audience = <MCP_APP_ID>`,
    `Aad.Mcp.RequiredScope = Mcp.Tools.ReadWrite`);
  - pre-authorizes every client that will call the MCP server for the scope, and those client app IDs are in the
    Discovery Service allow-list (`Aad.Mcp.AllowedCallerAppIds`), for example Visual Studio Code
    `aebc6443-996d-45c2-90f0-388ff96faa56` or Azure CLI `04b07795-8ddb-461a-bbee-02f9e1bf7b46`.

## 1. Set variables

```powershell
$tenant  = "72f988bf-86f1-41af-91ab-2d7cd011db47"
$sub     = "<SUBSCRIPTION_ID>"
$rg      = "<RESOURCE_GROUP>"
$loc     = "eastus2"
$plan    = "<APP_SERVICE_PLAN>"
$app     = "<WEB_APP_NAME>"          # becomes https://<WEB_APP_NAME>.azurewebsites.net
$mcpApp  = "<MCP_APP_ID>"

az account set --subscription $sub
```

## 2. Create the App Service

```powershell
az group create -n $rg -l $loc
az appservice plan create -g $rg -n $plan --is-linux --sku P0v3
az webapp create -g $rg -p $plan -n $app --runtime "DOTNETCORE:9.0"
az webapp config set -g $rg -n $app --always-on true
az webapp update -g $rg -n $app --https-only true
```

The published server is self-contained, so the runtime stack only provides the Linux host.

## 3. Configure app settings

```powershell
az webapp config appsettings set -g $rg -n $app --settings `
  ASPNETCORE_URLS="http://0.0.0.0:8080" `
  WEBSITES_PORT="8080" `
  ASPNETCORE_FORWARDEDHEADERS_ENABLED="true" `
  AZURE_MCP_DANGEROUSLY_ENABLE_FORWARDED_HEADERS="true" `
  AZURE_MCP_COLLECT_TELEMETRY="false" `
  AzureAd__Instance="https://login.microsoftonline.com/" `
  AzureAd__TenantId="$tenant" `
  AzureAd__ClientId="$mcpApp" `
  AzureAd__Audiences__0="$mcpApp" `
  AzureAd__Audiences__1="api://$mcpApp"
```

- The forwarded-headers settings are required because App Service terminates TLS in front of the app; without them
  the server advertises `http://` URLs in its OAuth protected resource metadata.
- Optional: `AZUREMIGRATE_INVENTORY_SKILL_TYPE` overrides the Discovery Service skill type (default `Discovery`).

## 4. Set the startup command

```powershell
az webapp config set -g $rg -n $app --startup-file `
  "chmod +x /home/site/wwwroot/azmcp && /home/site/wwwroot/azmcp server start --transport http --tool azuremigrate_inventory_createorupdate --tool azuremigrate_inventory_status --outgoing-auth-strategy UseHostingEnvironmentIdentity"
```

- `--tool` limits the server to the two inventory tools.
- `--outgoing-auth-strategy UseHostingEnvironmentIdentity` avoids the default On-Behalf-Of setup; the inventory tools
  forward the caller's token themselves and do not use the hosting identity.

## 5. Publish and deploy

From the repository root:

```powershell
$out = "$env:TEMP\azmcp-linux"
dotnet publish servers\Azure.Mcp.Server\src\Azure.Mcp.Server.csproj -c Release -r linux-x64 --self-contained -o $out
Compress-Archive -Path "$out\*" -DestinationPath "$env:TEMP\azmcp-linux.zip" -Force
az webapp deploy -g $rg -n $app --src-path "$env:TEMP\azmcp-linux.zip" --type zip
```

## 6. Verify

```powershell
$url = "https://$app.azurewebsites.net"

# Without a token the server must reject the call.
curl.exe -s -o NUL -w "%{http_code}`n" -X POST "$url/" -H "Content-Type: application/json" -d "{}"   # expect 401

# With a user token for the MCP app, list the tools.
$token = az account get-access-token --scope "api://$mcpApp/Mcp.Tools.ReadWrite" --query accessToken -o tsv
curl.exe -s -X POST "$url/" -H "Authorization: Bearer $token" -H "Content-Type: application/json" `
  -H "Accept: application/json, text/event-stream" `
  -d '{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\"}'
```

The second call requires Azure CLI to be pre-authorized on `<MCP_APP_ID>`. The response should list only
`azuremigrate_inventory_createorupdate` and `azuremigrate_inventory_status`.

## Troubleshooting

| Symptom | Check |
|---|---|
| `401` from the MCP server with a token | Token audience must be `<MCP_APP_ID>` or `api://<MCP_APP_ID>` and the tenant must match `AzureAd__TenantId`. |
| Tool returns `401`/`403` from the Discovery Service | The client app ID is missing from `Aad.Mcp.AllowedCallerAppIds`, or the token lacks `Mcp.Tools.ReadWrite`. |
| Create reports an existing session | The user already has an active session for the skill type; the tool reuses it. |
| App fails to start | Run `az webapp log tail -g $rg -n $app`; confirm the startup command and that `azmcp` is in `/home/site/wwwroot`. |
