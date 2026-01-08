# API permissions

A table of known API permissions of downstream APIs that can be called by Azure MCP.

| Namespace | Name | Scope | API Id | API Permission Id |
|-----------|------|-------|--------|-------------------|
| all | Azure Resource Manager | `https://management.azure.com/user_impersonation` | 797f4846-ba00-4fd7-ba43-dac1f8f63013 | 41094075-9dad-400e-a0bd-54e686782033 |
| storage | Azure Storage | `https://storage.azure.com/user_impersonation` | e406a681-f3d4-42a8-90b6-c2b029497af1 | 03e0da56-190b-40ad-a80c-ea378c433f7f |
| speech | Azure Cognitive Services | `https://cognitiveservices.azure.com/user_impersonation` | 7d312290-28c8-473c-a0ed-8e53749b6d6d | 5f1e8914-a52b-429f-9324-91b92b81adaf |
| postgres, mysql | Azure OSS RDBMS AAD (PostgreSQL/MySQL) | `https://ossrdbms-aad.database.windows.net/.default` | 123cd850-d9df-40bd-94d5-c9f07b7fa203 | cef99a3a-4cd3-4408-8143-4375d1e38a17 |
| cli_generate | Azure CLI Extension (1P App) | `a5ede409-60d3-4a6c-93e6-eb2e7271e8e3/Azclis.Intelligent.All` | a5ede409-60d3-4a6c-93e6-eb2e7271e8e3 | 94f3aa7c-b710-40be-83e4-8b5de364a323 |

# APIs that don't set API permissions in their manifest

These API permissions are used but aren't set in the manifest of their applications. This prevents 3rd party app registrations from adding these API permissions.

| Namespace | Name | Scope | API Id | API Permission Id |
|-----------|------|-------|--------|-------------------|
| applicationinsights | Application Insights Profiler Data Plane | `api://dataplane.diagnosticservices.azure.com/.default` | 3603eff4-9141-41d5-ba8f-02fb3a439cd6 | This app doesn't declare this API in its manifest |
| monitor | Azure Health Models Data API | `https://data.healthmodels.azure.com/.default` | f3d5b479-4d7f-4a81-9e00-09e2b452c04e | This app doesn't declare this API in its manifest |
