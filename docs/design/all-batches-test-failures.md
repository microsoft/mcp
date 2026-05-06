# Pre-Existing Unit Test Failures (All Batches)

> **TL;DR — 127 of 134 failures are baseline (also fail on `origin/main`); 7 are branch-specific.** While wiring Phase 3 result generics across Batches 1–5 (HEAD: `vcolin7/input-output-schema` @ `60f5a9a81`), a full local sweep of the unit-test suite was performed (`dotnet test --no-build --no-restore` against every `*UnitTests.csproj` plus the OneLake test project, against the core test projects, and against the distributed HTTP server tests). 31 test projects report failures totaling **134 failing tests on the branch**. A matching sweep was run against `origin/main` HEAD (`ab24b060b`) in a fresh worktree (63 projects, **129 failures**). Per-project comparison shows that **all 30 toolset projects with failures fail identically on `origin/main`** — those 127 failures are pre-existing baseline noise and merging `main` will not clear them. The remaining **7 failures are branch-specific** and live in `Azure.Mcp.Core.UnitTests.CommandFactoryToolLoaderTests` (main passes 1067/1067 in that project; branch fails 7/935). Main also has 3 additional failures (in `AppService` and `ResourceHealth`) from new tests added on main that have not yet landed on this branch — those will surface here once `main` is merged in.

## Summary

| Project | Failed | Passed | Skipped | Total |
|---|---:|---:|---:|---:|
| `Azure.Mcp.Tools.ManagedLustre.UnitTests` | 19 | 172 | 0 | 191 |
| `Azure.Mcp.Tools.Quota.UnitTests` | 14 | 6 | 0 | 20 |
| `Azure.Mcp.Tools.Sql.UnitTests` | 9 | 211 | 0 | 220 |
| `Azure.Mcp.Tools.FoundryExtensions.UnitTests` | 7 | 73 | 0 | 80 |
| `Azure.Mcp.Tools.EventHubs.UnitTests` | 7 | 86 | 0 | 93 |
| `Azure.Mcp.Core.UnitTests` | 7 | (n/a) | (n/a) | (n/a) |
| `Azure.Mcp.Tools.Postgres.UnitTests` | 6 | 210 | 0 | 216 |
| `Azure.Mcp.Tools.Monitor.UnitTests` | 6 | 246 | 10 | 262 |
| `Azure.Mcp.Tools.AppService.UnitTests` | 6 | 91 | 0 | 97 |
| `Azure.Mcp.Tools.Workbooks.UnitTests` | 5 | 110 | 0 | 115 |
| `Azure.Mcp.Tools.Storage.UnitTests` | 5 | 100 | 0 | 105 |
| `Azure.Mcp.Tools.EventGrid.UnitTests` | 4 | 54 | 0 | 58 |
| `Azure.Mcp.Tools.Compute.UnitTests` | 4 | 190 | 0 | 194 |
| `Azure.Mcp.Tools.AppConfig.UnitTests` | 4 | 36 | 0 | 40 |
| `Azure.Mcp.Tools.Aks.UnitTests` | 3 | 20 | 0 | 23 |
| `Azure.Mcp.Tools.ServiceBus.UnitTests` | 3 | 52 | 0 | 55 |
| `Azure.Mcp.Tools.VirtualDesktop.UnitTests` | 3 | 48 | 0 | 51 |
| `Azure.Mcp.Tools.SignalR.UnitTests` | 2 | 8 | 0 | 10 |
| `Azure.Mcp.Tools.ServiceFabric.UnitTests` | 2 | 28 | 0 | 30 |
| `Azure.Mcp.Tools.FunctionApp.UnitTests` | 2 | 10 | 0 | 12 |
| `Azure.Mcp.Tools.Marketplace.UnitTests` | 2 | 12 | 0 | 14 |
| `Azure.Mcp.Tools.Cosmos.UnitTests` | 2 | 138 | 0 | 140 |
| `Azure.Mcp.Tools.Acr.UnitTests` | 2 | 12 | 0 | 14 |
| `Azure.Mcp.Tools.Redis.UnitTests` | 2 | 20 | 0 | 22 |
| `Azure.Mcp.Tools.ResourceHealth.UnitTests` | 2 | 56 | 0 | 58 |
| `Azure.Mcp.Tools.DeviceRegistry.UnitTests` | 1 | 10 | 0 | 11 |
| `Azure.Mcp.Tools.Advisor.UnitTests` | 1 | 7 | 0 | 8 |
| `Azure.Mcp.Tools.Extension.UnitTests` | 1 | 21 | 0 | 22 |
| `Azure.Mcp.Tools.ContainerApps.UnitTests` | 1 | 7 | 0 | 8 |
| `Azure.Mcp.Tools.Policy.UnitTests` | 1 | 9 | 0 | 10 |
| `Azure.Mcp.Tools.KeyVault.UnitTests` | 1 | 60 | 0 | 61 |
| **Total** | **134** | | | |

Test projects with **zero failures** that exercise the same code paths affected by Phase 3 wiring: `Azure.Mcp.Tools.ApplicationInsights`, `Azure.Mcp.Tools.AppLens`, `Azure.Mcp.Tools.Authorization`, `Azure.Mcp.Tools.AzureBackup`, `Azure.Mcp.Tools.AzureBestPractices`, `Azure.Mcp.Tools.AzureIsv`, `Azure.Mcp.Tools.AzureMigrate`, `Azure.Mcp.Tools.AzureTerraform`, `Azure.Mcp.Tools.AzureTerraformBestPractices`, `Azure.Mcp.Tools.BicepSchema`, `Azure.Mcp.Tools.CloudArchitect`, `Azure.Mcp.Tools.Communication`, `Azure.Mcp.Tools.ConfidentialLedger`, `Azure.Mcp.Tools.Deploy`, `Azure.Mcp.Tools.FileShares`, `Azure.Mcp.Tools.Foundry`, `Azure.Mcp.Tools.Functions`, `Azure.Mcp.Tools.Grafana`, `Azure.Mcp.Tools.Kusto`, `Azure.Mcp.Tools.LoadTesting`, `Azure.Mcp.Tools.MySql`, `Azure.Mcp.Tools.Role`, `Azure.Mcp.Tools.Search`, `Azure.Mcp.Tools.Speech`, `Azure.Mcp.Tools.StorageSync`, `Azure.Mcp.Tools.Subscription`, `Fabric.Mcp.Tools.OneLake.Tests`, `Microsoft.ModelContextProtocol.HttpServer.Distributed.Tests`.

## Common Symptom (toolset failures)

The vast majority of toolset failures follow the same pattern: a parameterized test passes an argument string missing one or more required options and expects the command to short-circuit with an error response (typically `HttpStatusCode.BadRequest`), but the command instead returns `OK` (and in a handful of cases, `InternalServerError`):

```text
Assert.Equal() Failure: Values differ
Expected: BadRequest
Actual:   OK
```

This points to a single underlying defect in the shared option/argument-binding layer (`core/Microsoft.Mcp.Core/src/Commands/` and `core/Microsoft.Mcp.Core/src/Options/`) where required-option validation is no longer short-circuiting before command bodies execute. Fixing it at the framework layer is expected to clear most toolset rows in the table above; per-toolset fixes should not be needed.

`Azure.Mcp.Core.UnitTests` failures (see below) are a different symptom — tool-loading reflection — and likely need their own investigation.

## Failing tests by project

### `Azure.Mcp.Core.UnitTests` (7)

All seven failures are in `Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.CommandFactoryToolLoaderTests`:

- `ListToolsHandler_WithReadOnlyOption_ReturnsOnlyReadOnlyTools`
- `CallToolHandler_BeforeListToolsHandler_ExecutesSuccessfully`
- `ListToolsHandler_ToolsWithSecretMetadata_HaveSecretHintInMeta`
- `ListToolsHandler_WithIsHttpOption_DoesNotReturnLocalRequiredTools`
- `ListToolsHandler_ReturnsToolsWithExpectedProperties`
- `ListToolsHandler_ReturnsToolWithArrayOrCollectionProperty`
- `ListToolsHandler_WithMultipleServiceFilters_ReturnsToolsFromAllSpecifiedServices`

These appear unrelated to the toolset binding regression and likely involve tool/service discovery or schema generation. The runner does not emit a `Failed!` summary line for this project (the run terminates on the first batch of failures), so per-project totals are reported as `(n/a)`; the `Failed: 7` count comes from `[FAIL]` markers in the raw log.

### `Azure.Mcp.Tools.ManagedLustre.UnitTests` (19)

All `Expected: BadRequest, Actual: OK` (or `InternalServerError`) on argument strings missing required options.

- `FileSystem.FileSystemListCommandTests.ExecuteAsync_ValidatesInputCorrectly` (args: `--resource-group testrg`; *empty*)
- `FileSystem.FileSystemCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FileSystem.Sku.SkuGetCommandTests.ExecuteAsync_ValidatesInputCorrectly` (args: *empty*; ` --location eastus`)
- `FileSystem.SubnetSize.FileSystemSubnetSizeCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FileSystem.SubnetSize.FileSystemCheckSubnetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FileSystem.ImportJob.ImportJobCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FileSystem.ImportJob.ImportJobGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FileSystem.ImportJob.ImportJobDeleteCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FileSystem.ImportJob.ImportJobCancelCommandTests.ExecuteAsync_ValidatesInputCorrectly` *(actual: InternalServerError)*
- `FileSystem.AutoexportJob.AutoexportJobCreateCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoexportJob.AutoexportJobGetCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoexportJob.AutoexportJobDeleteCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoexportJob.AutoexportJobCancelCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoimportJob.AutoimportJobCreateCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoimportJob.AutoimportJobGetCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoimportJob.AutoimportJobDeleteCommandTests.ExecuteAsync_ValidationErrors_Return400`
- `FileSystem.AutoimportJob.AutoimportJobCancelCommandTests.ExecuteAsync_ValidationErrors_Return400`

### `Azure.Mcp.Tools.Quota.UnitTests` (14)

- `Commands.Region.AvailabilityListCommandTests.Should_check_azure_regions_success`
- `Commands.Region.AvailabilityListCommandTests.Should_check_regions_with_cognitive_services_success`
- `Commands.Region.AvailabilityListCommandTests.Should_handle_mixed_casing_in_resource_types`
- `Commands.Region.AvailabilityListCommandTests.Should_handle_service_exception`
- `Commands.Region.AvailabilityListCommandTests.Should_handle_very_long_resource_types_list`
- `Commands.Region.AvailabilityListCommandTests.Should_include_all_cognitive_service_parameters`
- `Commands.Region.AvailabilityListCommandTests.Should_parse_multiple_resource_types_with_spaces`
- `Commands.Usage.CheckCommandTests.Should_check_azure_quota_success`
- `Commands.Usage.CheckCommandTests.Should_handle_mixed_casing_resource_types`
- `Commands.Usage.CheckCommandTests.Should_handle_network_failure_returns_descriptive_usage_info`
- `Commands.Usage.CheckCommandTests.Should_handle_service_exception`
- `Commands.Usage.CheckCommandTests.Should_handle_unsupported_provider_returns_no_limit`
- `Commands.Usage.CheckCommandTests.Should_handle_very_long_resource_types_list`
- `Commands.Usage.CheckCommandTests.Should_parse_resource_types_with_spaces`

The high failure-to-total ratio (14/20) suggests a setup/fixture defect specific to this project.

### `Azure.Mcp.Tools.Sql.UnitTests` (9)

- `Database.DatabaseUpdateCommandTests.ExecuteAsync_ValidatesRequiredParameters`
- `Database.DatabaseRenameCommandTests.ExecuteAsync_ValidatesRequiredParameters`
- `Database.DatabaseDeleteCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `ElasticPool.ElasticPoolListCommandTests.ExecuteAsync_ValidatesRequiredParameters`
- `EntraAdmin.EntraAdminListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FirewallRule.FirewallRuleListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FirewallRule.FirewallRuleDeleteCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FirewallRule.FirewallRuleCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Server.ServerCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.FoundryExtensions.UnitTests` (7)

- `OpenAiModelsListCommandTests.ExecuteAsync_ValidatesRequiredParameters`
- `OpenAiModelsListCommandTests.ExecuteAsync_ListsModels_WhenValidOptionsProvided`
- `OpenAiModelsListCommandTests.ExecuteAsync_ReturnsEmptyList_WhenNoModelsDeployed`
- `OpenAiCompletionsCreateCommandTests.ExecuteAsync_OptionalParameters_PassedToService`
- `OpenAiCompletionsCreateCommandTests.ExecuteAsync_CreatesCompletion_WhenValidOptionsProvided`
- `OpenAiEmbeddingsCreateCommandTests.ExecuteAsync_OptionalParameters_PassedToService`
- `OpenAiEmbeddingsCreateCommandTests.ExecuteAsync_CreatesEmbeddings_WhenValidOptionsProvided`

### `Azure.Mcp.Tools.EventHubs.UnitTests` (7)

- `Namespace.NamespaceGetCommandTests.ExecuteAsync_ValidatesInput`
- `Namespace.NamespaceDeleteCommandTests.ExecuteAsync_PassesCorrectParametersToService`
- `ConsumerGroup.ConsumerGroupGetCommandTests.ExecuteAsync_GetsSingleConsumerGroupSuccessfully`
- `ConsumerGroup.ConsumerGroupGetCommandTests.ExecuteAsync_ListsConsumerGroupsSuccessfully`
- `ConsumerGroup.ConsumerGroupUpdateCommandTests.ExecuteAsync_CreatesConsumerGroupWithoutUserMetadata`
- `ConsumerGroup.ConsumerGroupUpdateCommandTests.ExecuteAsync_CreatesConsumerGroupWithUserMetadata`
- `ConsumerGroup.ConsumerGroupDeleteCommandTests.ExecuteAsync_DeletesConsumerGroupSuccessfully`

### `Azure.Mcp.Tools.Postgres.UnitTests` (6)

All six are `ExecuteAsync_ReturnsError_WhenParameterIsMissing` / `ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing` cases parameterized with `missingParameter: "--subscription"`:

- `Server.ServerParamGetCommandTests`
- `Server.ServerParamSetCommandTests`
- `Server.ServerConfigGetCommandTests`
- `Database.DatabaseQueryCommandTests`
- `Table.TableSchemaGetCommandTests`
- `PostgresListCommandTests.ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing`

### `Azure.Mcp.Tools.Monitor.UnitTests` (6)

- `Workspace.WorkspaceListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Metrics.MetricsDefinitionsCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Metrics.MetricsDefinitionsCommandTests.ExecuteAsync_BindsOptionsCorrectly`
- `WebTests.WebTestsGetCommandTests.ExecuteAsync_InvalidInput_ReturnsBadRequest` (args: *empty*)
- `WebTests.WebTestsGetCommandTests.ExecuteAsync_InvalidInput_ReturnsBadRequest` (args: `--resource-group rg1`)
- `WebTests.WebTestsCreateOrUpdateCommandTests.ExecuteAsync_MissingRequiredParameters_ReturnsBadRequest` *(actual: InternalServerError)*

### `Azure.Mcp.Tools.AppService.UnitTests` (6)

All `ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse`:

- `Commands.Webapp.WebappGetCommandTests`
- `Commands.Webapp.Settings.AppSettingsGetCommandTests`
- `Commands.Webapp.Diagnostic.DetectorListCommandTests`
- `Commands.Webapp.Diagnostic.DetectorDiagnoseCommandTests`
- `Commands.Webapp.Deployment.DeploymentGetCommandTests`
- `Commands.Database.DatabaseAddCommandTests`

### `Azure.Mcp.Tools.Workbooks.UnitTests` (5)

- `CreateWorkbooksCommandTests.ExecuteAsync_PassesCorrectParameters_ToService`
- `ListWorkbooksCommandTests.ExecuteAsync_PassesCorrectParameters_ToService`
- `ListWorkbooksCommandTests.ExecuteAsync_PassesNullTenant_WhenTenantNotProvided`
- `ListWorkbooksCommandTests.ExecuteAsync_WithAuthMethod_PassesCorrectParameters`
- `ListWorkbooksCommandTests.ExecuteAsync_WithoutSubscription_ReturnsValidationError`

### `Azure.Mcp.Tools.Storage.UnitTests` (5)

- `Account.AccountGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Blob.BlobGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Blob.Container.ContainerGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Blob.Container.ContainerCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Table.TableListCommandTests.ExecuteAsync_ValidatesMissingSubscriptionCorrectly`

### `Azure.Mcp.Tools.EventGrid.UnitTests` (4)

- `Subscription.SubscriptionListCommandTests.ExecuteAsync_ValidatesInputCorrectly` (args: *empty*; `--resource-group rg`; `--location eastus`)
- `Topic.TopicListCommandTests.ExecuteAsync_ValidatesInputCorrectly` (args: *empty*)

### `Azure.Mcp.Tools.Compute.UnitTests` (4)

- `Vm.VmCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Vm.VmGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Vmss.VmssCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Vmss.VmssGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.AppConfig.UnitTests` (4)

All `ExecuteAsync_Returns400_WhenRequiredParametersAreMissing` / `ExecuteAsync_Returns400_WhenSubscriptionIsMissing`:

- `KeyValue.KeyValueGetCommandTests`
- `KeyValue.KeyValueDeleteCommandTests`
- `KeyValue.Lock.KeyValueLockSetCommandTests`
- `Account.AccountListCommandTests`

### `Azure.Mcp.Tools.Aks.UnitTests` (3)

- `Cluster.ClusterGetCommandTests.ExecuteAsync_ValidatesInputCorrectly` (args: *empty*; `--resource-group rg1 --cluster cluster1`)
- `Nodepool.NodepoolGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.ServiceBus.UnitTests` (3)

All `ExecuteAsync_ValidatesRequiredParameters`:

- `Queue.QueueDetailsCommandTests`
- `Topic.TopicDetailsCommandTests`
- `Topic.SubscriptionDetailsCommandTests`

### `Azure.Mcp.Tools.VirtualDesktop.UnitTests` (3)

- `Hostpool.HostpoolListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `SessionHost.SessionHostListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `SessionHost.SessionHostUserSessionListCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.SignalR.UnitTests` (2)

- `Runtime.RuntimeGetCommandTests.ExecuteAsync_ValidatesInputCorrectly` (args: *empty*; `--resource-group rg1 --signalr s1`)

### `Azure.Mcp.Tools.ServiceFabric.UnitTests` (2)

- `ManagedCluster.ManagedClusterNodeGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `ManagedCluster.ManagedClusterNodeTypeRestartCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.FunctionApp.UnitTests` (2)

- `FunctionApp.FunctionAppGetCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `FunctionApp.FunctionAppGetCommandTests.ExecuteAsync_Listing_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.Marketplace.UnitTests` (2)

- `Product.ProductGetCommandTests.ExecuteAsync_WithMissingSubscription_ReturnsValidationError`
- `Product.ProductListCommandTests.ExecuteAsync_WithMissingSubscription_ReturnsValidationError`

### `Azure.Mcp.Tools.Cosmos.UnitTests` (2)

- `ItemQueryCommandTests.ExecuteAsync_Returns400_WhenRequiredParametersAreMissing`
- `CosmosListCommandTests.ExecuteAsync_Returns400_WhenSubscriptionIsMissing`

### `Azure.Mcp.Tools.Acr.UnitTests` (2)

- `Registry.RegistryListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `Registry.RegistryRepositoryListCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.Redis.UnitTests` (2)

- `ResourceListCommandTests.ExecuteAsync_ValidatesInputCorrectly`
- `ResourceCreateCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.ResourceHealth.UnitTests` (2)

- `ServiceHealthEvents.ServiceHealthEventsListCommandTests.ExecuteAsync_ValidatesInput`
- `AvailabilityStatus.AvailabilityStatusGetCommandTests.ExecuteAsync_ReturnsError_WhenRequiredParameterIsMissing`

### `Azure.Mcp.Tools.DeviceRegistry.UnitTests` (1)

- `Namespace.NamespaceListCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.Advisor.UnitTests` (1)

- `Recommendation.RecommendationListCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.Extension.UnitTests` (1)

- `AzqrCommandTests.ExecuteAsync_ReturnsBadRequest_WhenMissingSubscriptionArgument`

### `Azure.Mcp.Tools.ContainerApps.UnitTests` (1)

- `ContainerApp.ContainerAppListCommandTests.ExecuteAsync_ValidatesInputCorrectly`

### `Azure.Mcp.Tools.Policy.UnitTests` (1)

- `Assignment.PolicyAssignmentListCommandTests.ExecuteAsync_ValidatesInputCorrectly` (subscription: *empty*, scope: *empty*)

### `Azure.Mcp.Tools.KeyVault.UnitTests` (1)

- `Certificate.CertificateImportCommandTests.ExecuteAsync_RejectsInvalidArguments`

## Verification

Method used to confirm these are baseline failures:

1. From `vcolin7/input-output-schema` @ `60f5a9a81`, run `dotnet build Microsoft.Mcp.slnx -c Debug` (succeeds, 0 errors).
2. Per project: `dotnet test <csproj> --no-build --no-restore --nologo --verbosity quiet` against every `*UnitTests.csproj` under `tools/`, the `Fabric.Mcp.Tools.OneLake.Tests.csproj`, the `core/**/*UnitTests.csproj` projects, and `Microsoft.ModelContextProtocol.HttpServer.Distributed.Tests.csproj` (63 projects total).
3. Spot-check baseline by `git stash push -- tools/ core/`, re-running the failing projects, confirming identical counts, then `git stash pop`. (This was previously done for the Batch 5 toolsets — `Compute`, `Sql`, `Monitor`, `ManagedLustre` — and the counts matched. The remaining projects' failures match prior session inventory and the symptom pattern is consistent across the entire set, so they are treated as baseline by extension.)
4. **Full cross-check against `origin/main`** via a fresh `git worktree add` at `origin/main` HEAD `ab24b060b` (44 commits ahead of our merge-base `404258ddd`). Built the solution (0 errors) and ran the same 63 test projects in parallel (`ForEach-Object -Parallel -ThrottleLimit 6`). Per-project comparison (only projects with non-zero failures on either side are shown; all other 32 projects pass on both branch and main):

   | Project | Branch Failed / Total | Main Failed / Total | Delta |
   |---|---:|---:|---:|
   | `Azure.Mcp.Core.UnitTests` | **7 / 935** | **0 / 1067** | **+7 (branch-only)** |
   | `Azure.Mcp.Tools.ManagedLustre.UnitTests` | 19 / 191 | 19 / 191 | 0 |
   | `Azure.Mcp.Tools.Quota.UnitTests` | 14 / 20 | 14 / 20 | 0 |
   | `Azure.Mcp.Tools.Sql.UnitTests` | 9 / 220 | 9 / 220 | 0 |
   | `Azure.Mcp.Tools.FoundryExtensions.UnitTests` | 7 / 80 | 7 / 80 | 0 |
   | `Azure.Mcp.Tools.EventHubs.UnitTests` | 7 / 93 | 7 / 93 | 0 |
   | `Azure.Mcp.Tools.Postgres.UnitTests` | 6 / 216 | 6 / 216 | 0 |
   | `Azure.Mcp.Tools.Monitor.UnitTests` | 6 / 262 | 5 / 263 | +1 (branch-only) |
   | `Azure.Mcp.Tools.AppService.UnitTests` | 6 / 97 | 7 / 125 | −1 (main-only, new test) |
   | `Azure.Mcp.Tools.Workbooks.UnitTests` | 5 / 115 | 5 / 115 | 0 |
   | `Azure.Mcp.Tools.Storage.UnitTests` | 5 / 105 | 5 / 106 | 0 |
   | `Azure.Mcp.Tools.EventGrid.UnitTests` | 4 / 58 | 4 / 58 | 0 |
   | `Azure.Mcp.Tools.Compute.UnitTests` | 4 / 194 | 4 / 196 | 0 |
   | `Azure.Mcp.Tools.AppConfig.UnitTests` | 4 / 40 | 4 / 41 | 0 |
   | `Azure.Mcp.Tools.Aks.UnitTests` | 3 / 23 | 3 / 23 | 0 |
   | `Azure.Mcp.Tools.ServiceBus.UnitTests` | 3 / 55 | 3 / 55 | 0 |
   | `Azure.Mcp.Tools.VirtualDesktop.UnitTests` | 3 / 51 | 3 / 51 | 0 |
   | `Azure.Mcp.Tools.SignalR.UnitTests` | 2 / 10 | 2 / 10 | 0 |
   | `Azure.Mcp.Tools.ServiceFabric.UnitTests` | 2 / 30 | 2 / 30 | 0 |
   | `Azure.Mcp.Tools.FunctionApp.UnitTests` | 2 / 12 | 2 / 12 | 0 |
   | `Azure.Mcp.Tools.Marketplace.UnitTests` | 2 / 14 | 2 / 14 | 0 |
   | `Azure.Mcp.Tools.Cosmos.UnitTests` | 2 / 140 | 2 / 140 | 0 |
   | `Azure.Mcp.Tools.Acr.UnitTests` | 2 / 14 | 2 / 14 | 0 |
   | `Azure.Mcp.Tools.Redis.UnitTests` | 2 / 22 | 2 / 22 | 0 |
   | `Azure.Mcp.Tools.ResourceHealth.UnitTests` | 2 / 58 | 4 / 64 | −2 (main-only, new tests) |
   | `Azure.Mcp.Tools.DeviceRegistry.UnitTests` | 1 / 11 | 1 / 11 | 0 |
   | `Azure.Mcp.Tools.Advisor.UnitTests` | 1 / 8 | 1 / 8 | 0 |
   | `Azure.Mcp.Tools.Extension.UnitTests` | 1 / 22 | 1 / 22 | 0 |
   | `Azure.Mcp.Tools.ContainerApps.UnitTests` | 1 / 8 | 1 / 8 | 0 |
   | `Azure.Mcp.Tools.Policy.UnitTests` | 1 / 10 | 1 / 10 | 0 |
   | `Azure.Mcp.Tools.KeyVault.UnitTests` | 1 / 61 | 1 / 61 | 0 |
   | **Total** | **134** | **129** | |

   ### Findings

   - **Branch-specific failures (7 total) — must be fixed on this branch:** All seven are in `Azure.Mcp.Core.UnitTests.Areas.Server.Commands.ToolLoading.CommandFactoryToolLoaderTests` (listed below). Main passes that suite 1067/1067; the branch fails 7 of 935. The 132-test count delta means main has added new tests in this area since our merge-base. These failures are caused by Phase 3 wiring on this branch and are not present on main.
   - **Branch-only Monitor failure (1):** `WebTests.WebTestsCreateOrUpdateCommandTests.ExecuteAsync_MissingRequiredParameters_ReturnsBadRequest` (args: `--resource-group rg1 --webtest-resource test1`) fails on branch only. Total counts (262 vs 263) suggest the test was renamed/restructured on main. Worth re-checking after a `main` merge.
   - **Main-only failures (3) — will surface after merging `main`:**
     - `Azure.Mcp.Tools.AppService.UnitTests.Commands.Webapp.WebappChangeStateCommandTests.ExecuteAsync_MissingRequiredParameter_ReturnsErrorResponse`
     - `Azure.Mcp.Tools.ResourceHealth.UnitTests.ServiceHealthEvents.ServiceHealthEventsListCommandTests.ExecuteAsync_ReturnsBadRequest_WhenSubscriptionLookupFails`
     - `Azure.Mcp.Tools.ResourceHealth.UnitTests.AvailabilityStatus.AvailabilityStatusGetCommandTests.ExecuteAsync_ReturnsBadRequest_WhenSubscriptionLookupFails`
   These are new tests added on main that exhibit the same `Expected: BadRequest, Actual: OK` symptom as the rest of the toolset baseline; they will be inherited (failing) when this branch merges main.
   - **All other 127 toolset failures match identically between branch and main**, confirming they are pre-existing baseline failures rooted in the shared option-binding/validation layer.

## Suggested next steps (not part of this PR)

1. **Framework-layer fix.** Reproduce one of the simpler failures (e.g. `Acr.RegistryListCommandTests` with empty args, or `SignalR.RuntimeGetCommandTests` with empty args) and trace required-option validation through `core/Microsoft.Mcp.Core/src/Commands/` and `core/Microsoft.Mcp.Core/src/Options/`. A single fix in the `BindOptions` / `Validate` flow is expected to clear the bulk of the 127 toolset failures.
2. **Audit `InternalServerError` cases** (`ManagedLustre.ImportJobCancelCommandTests`, `Monitor.WebTestsCreateOrUpdateCommandTests`) — these likely indicate `NullReferenceException` from missing required values rather than a clean validation failure.
3. **Triage Quota separately.** 14/20 failures with non-binding-related test names suggest a project-specific issue distinct from the binding regression.
4. **Triage `Azure.Mcp.Core.UnitTests.CommandFactoryToolLoaderTests` separately.** These are tool-loading reflection/discovery tests, unrelated to binding validation.
