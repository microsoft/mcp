# API permissions

A table of known API permissions of downstream APIs that can be called by Azure MCP.

| Namespace | Name | Scope | API Id | API Permission Id |
|-----------|------|-------|--------|-------------------|
| * | Azure Resource Manager | `https://management.azure.com/user_impersonation` | 797f4846-ba00-4fd7-ba43-dac1f8f63013 | 41094075-9dad-400e-a0bd-54e686782033 |
| kusto | Azure Data Explorer (Kusto) | `https://<cluster>.kusto.windows.net/user_impersonation` | 2746ea77-4702-4b45-80ca-3c97e680e8b7 | 00d678f0-da44-4b12-a6d6-c98bcfd1c5fe |
| storage | Azure Storage | `https://storage.azure.com/user_impersonation` | e406a681-f3d4-42a8-90b6-c2b029497af1 | 03e0da56-190b-40ad-a80c-ea378c433f7f |
| postgres, mysql | Azure OSS RDBMS AAD (PostgreSQL/MySQL) | `https://ossrdbms-aad.database.windows.net/user_impersonation` | 123cd850-d9df-40bd-94d5-c9f07b7fa203 | cef99a3a-4cd3-4408-8143-4375d1e38a17 |
| cli_generate | Azure CLI Extension (1P App) | `a5ede409-60d3-4a6c-93e6-eb2e7271e8e3/Azclis.Intelligent.All` | a5ede409-60d3-4a6c-93e6-eb2e7271e8e3 | 94f3aa7c-b710-40be-83e4-8b5de364a323 |
| appconfig | Azure App Configuration | `https://azconfig.io/Snapshot.Action` | 35ffadb3-7fc1-497e-b61b-381d28e744cc | 28bb462a-d940-4cbe-afeb-281756df9af8 |
| appconfig | Azure App Configuration | `https://azconfig.io/Snapshot.Write` | 35ffadb3-7fc1-497e-b61b-381d28e744cc | ea601552-5fd3-4792-9dfc-e85be5a6827c |
| appconfig | Azure App Configuration | `https://azconfig.io/Snapshot.Read` | 35ffadb3-7fc1-497e-b61b-381d28e744cc | 5970d132-a862-421f-9352-8ed18f833d78 |
| appconfig | Azure App Configuration | `https://azconfig.io/KeyValue.Read` | 35ffadb3-7fc1-497e-b61b-381d28e744cc | 8d17f7f7-030c-4b57-8129-cfb5a16433cd |
| appconfig | Azure App Configuration | `https://azconfig.io/KeyValue.Write` | 35ffadb3-7fc1-497e-b61b-381d28e744cc | 77967a14-4f88-4960-84da-e8f71f761ac2 |
| appconfig | Azure App Configuration | `https://azconfig.io/KeyValue.Delete` | 35ffadb3-7fc1-497e-b61b-381d28e744cc | 08eeff12-9b4a-4273-b3d9-ff8a13c32645 |
| keyvault | Azure Key Vault | `https://vault.azure.net/user_impersonation` | cfa8b339-82a2-471a-a3c9-0fc0be7a4093 | f53da476-18e3-4152-8e01-aec403e6edc0 |
| eventhubs | Azure Event Hubs | `https://eventhubs.azure.net/user_impersonation` | 80369ed6-5f11-4dd9-bef3-692475845e77 | 7d388411-3845-4cfc-aa69-33192f4b9735 |
| servicebus | Azure Service Bus | `https://servicebus.azure.net/user_impersonation` | 80a10ef9-8168-493d-abf9-3297c4ef6e3c | 40e16207-c5fd-4916-8ca4-64565f2367ca |
| search | Azure AI Search | `https://search.azure.com/user_impersonation` | 880da380-985e-4198-81b9-e05b1cc53158 | a4165a31-5d9e-4120-bd1e-9d88c66fd3b8 |
| speech | Azure Cognitive Services | `https://cognitiveservices.azure.com/user_impersonation` | 7d312290-28c8-473c-a0ed-8e53749b6d6d | 5f1e8914-a52b-429f-9324-91b92b81adaf |

> [!NOTE]
> <sup>*</sup> Azure Resource Manager (ARM) is used by most of the tools that need to interact with Azure resources.

You can add the desirable API Permissions to your app registration using Azure CLI. Learn more at [az ad app permission add](https://learn.microsoft.com/cli/azure/ad/app/permission?view=azure-cli-latest#az-ad-app-permission-add)

# APIs without exposed API permissions

These APIs don't expose their API permissions so they won't be callable from the self-hosted Azure MCP server.

| Namespace | Name | Scope | API Id |
|-----------|------|-------|--------|
| applicationinsights | Application Insights Profiler Data Plane | `api://dataplane.diagnosticservices.azure.com/.default` | 3603eff4-9141-41d5-ba8f-02fb3a439cd6 |
| monitor | Azure Health Models Data API | `https://data.healthmodels.azure.com/.default` | f3d5b479-4d7f-4a81-9e00-09e2b452c04e |
| cosmos | Azure Cosmos DB | `https://cosmos.azure.com/.default` | f8202e39-5162-4378-8714-8018bb911e5c |
| acr | Azure Container Registry | `https://containerregistry.azure.net/.default` | 76c92352-c057-4cc2-9b1e-f34c32bc58bd |
| signalr | Azure SignalR Service | `https://signalr.azure.com/.default` | cdad765c-f191-43ba-b9f5-7aef392f811d |
| communication | Azure Communication Services | `https://communication.azure.com/.default` | 632ec9eb-fad7-4cbd-993a-e72973ba2acc |
| confidentialledger | Azure Confidential Ledger | `https://confidential-ledger.azure.com/.default` | c9e0b461-3515-4a03-b576-ede91ed4336d |
