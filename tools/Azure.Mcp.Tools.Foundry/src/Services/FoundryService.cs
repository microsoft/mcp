// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.AI.Projects;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Foundry.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Services.Models;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.Foundry.Services;

public class FoundryService(
    IHttpClientService httpClientService,
    ISubscriptionService subscriptionService,
    ITenantService tenantService) : BaseAzureResourceService(subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService)), tenantService ?? throw new ArgumentNullException(nameof(tenantService))), IFoundryService
{
    private readonly IHttpClientService _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
    public async Task<List<ModelInformation>> ListModels(
        bool searchForFreePlayground = false,
        string publisherName = "",
        string licenseName = "",
        string modelName = "",
        int maxPages = 3,
        RetryPolicyOptions? retryPolicy = null)
    {
        string url = "https://api.catalog.azureml.ms/asset-gallery/v1.0/models";
        var request = new ModelCatalogRequest { Filters = [new ModelCatalogFilter("labels", ["latest"], "eq")] };

        if (searchForFreePlayground)
        {
            request.Filters.Add(new ModelCatalogFilter("freePlayground", ["true"], "eq"));
        }

        if (!string.IsNullOrEmpty(publisherName))
        {
            request.Filters.Add(new ModelCatalogFilter("publisher", [publisherName], "contains"));
        }

        if (!string.IsNullOrEmpty(licenseName))
        {
            request.Filters.Add(new ModelCatalogFilter("license", [licenseName], "contains"));
        }

        if (!string.IsNullOrEmpty(modelName))
        {
            request.Filters.Add(new ModelCatalogFilter("name", [modelName], "eq"));
        }

        var modelsList = new List<ModelInformation>();
        int pageCount = 0;

        try
        {
            while (pageCount < maxPages)
            {
                pageCount++;
                try
                {
                    var content = new StringContent(
                        JsonSerializer.Serialize(request, FoundryJsonContext.Default.ModelCatalogRequest),
                        Encoding.UTF8,
                        "application/json");

                    var httpResponse = await _httpClientService.DefaultClient.PostAsync(url, content);
                    httpResponse.EnsureSuccessStatusCode();

                    var responseText = await httpResponse.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize(responseText,
                        FoundryJsonContext.Default.ModelCatalogResponse);
                    if (response == null || response.Summaries.Count == 0)
                    {
                        break;
                    }

                    foreach (var summary in response.Summaries)
                    {
                        try
                        {
                            summary.DeploymentInformation.IsFreePlayground = summary.PlaygroundLimits != null;
                            if (!string.IsNullOrEmpty(summary.Publisher) &&
                                summary.Publisher.Equals("openai", StringComparison.OrdinalIgnoreCase))
                            {
                                summary.DeploymentInformation.IsOpenAI = true;
                            }
                            else
                            {
                                if (summary.AzureOffers != null)
                                {
                                    summary.DeploymentInformation.IsServerlessEndpoint =
                                        summary.AzureOffers.Contains("standard-paygo");

                                    summary.DeploymentInformation.IsManagedCompute =
                                        summary.AzureOffers.Contains("VM") ||
                                        summary.AzureOffers.Contains("VM-withSurcharge");
                                }
                            }

                            modelsList.Add(summary);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if (string.IsNullOrEmpty(response.ContinuationToken))
                    {
                        break;
                    }

                    request.ContinuationToken = response.ContinuationToken;
                }
                catch (HttpRequestException)
                {
                    break;
                }
                catch (JsonException)
                {
                    break;
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving models from model catalog: {e.Message}");
        }

        return modelsList;
    }

    public async Task<List<Deployment>> ListDeployments(string endpoint, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint);

        try
        {
            var credential = await GetCredential(tenantId);
            var deploymentsClient = new AIProjectClient(new Uri(endpoint), credential).GetDeploymentsClient();

            var deployments = new List<Deployment>();
            await foreach (var deployment in deploymentsClient.GetDeploymentsAsync())
            {
                deployments.Add(deployment);
            }

            return deployments;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list deployments: {ex.Message}", ex);
        }
    }

    public async Task<ModelDeploymentResult> DeployModel(string deploymentName, string modelName, string modelFormat,
        string azureAiServicesName, string resourceGroup, string subscriptionId, string? modelVersion = null, string? modelSource = null,
        string? skuName = null, int? skuCapacity = null, string? scaleType = null, int? scaleCapacity = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(deploymentName, modelName, modelFormat, azureAiServicesName, resourceGroup, subscriptionId);

        try
        {
            // Create ArmClient for deployments
            ArmClient armClient = await CreateArmClientWithApiVersionAsync("Microsoft.CognitiveServices/accounts/deployments", "2025-06-01", null, retryPolicy);

            // Retrieve the Cognitive Services account
            var cognitiveServicesAccount = await GetGenericResourceAsync(
                armClient,
                new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.CognitiveServices/accounts/{azureAiServicesName}"));

            // Prepare data for the deployment
            ResourceIdentifier deploymentId = new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.CognitiveServices/accounts/{azureAiServicesName}/deployments/{deploymentName}");
            var deploymentData = new CognitiveServicesAccountDeploymentData
            {
                Properties = new CognitiveServicesAccountDeploymentProperties
                {
                    Model = new CognitiveServicesAccountDeploymentModel
                    {
                        Format = modelFormat,
                        Name = modelName,
                        Version = modelVersion,
                        Source = string.IsNullOrEmpty(modelSource) ? null : modelSource
                    },
                    ScaleSettings = string.IsNullOrEmpty(scaleType) ? null : new CognitiveServicesAccountDeploymentScaleSettings
                    {
                        ScaleType = scaleType,
                        Capacity = scaleCapacity
                    }
                },
                Sku = string.IsNullOrEmpty(skuName) ? null : new CognitiveServicesSku
                {
                    Name = skuName,
                    Capacity = skuCapacity
                }
            };

            var result = await CreateOrUpdateGenericResourceAsync(
                armClient,
                deploymentId,
                cognitiveServicesAccount.Data.Location,
                deploymentData,
                FoundryJsonContext.Default.CognitiveServicesAccountDeploymentData);
            if (!result.HasData)
            {
                return new ModelDeploymentResult
                {
                    HasData = false
                };
            }
            else
            {
                return new ModelDeploymentResult
                {
                    HasData = true,
                    Id = result.Data.Id.ToString(),
                    Name = result.Data.Name,
                    Type = result.Data.ResourceType.ToString(),
                    Sku = result.Data.Sku,
                    Tags = result.Data.Tags,
                    Properties = result.Data.Properties?.ToObjectFromJson(FoundryJsonContext.Default.IDictionaryStringObject)
                };
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to deploy model: {ex.Message}", ex);
        }
    }

    public async Task<List<KnowledgeIndexInformation>> ListKnowledgeIndexes(string endpoint, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint);

        try
        {
            var credential = await GetCredential(tenantId);
            var indexesClient = new AIProjectClient(new Uri(endpoint), credential).GetIndexesClient();

            var indexes = new List<KnowledgeIndexInformation>();
            await foreach (var index in indexesClient.GetIndicesAsync())
            {
                // Determine the type based on the actual type of the index
                string indexType = index switch
                {
                    AzureAISearchIndex => "AzureAISearchIndex",
                    ManagedAzureAISearchIndex => "ManagedAzureAISearchIndex",
                    CosmosDBIndex => "CosmosDBIndex",
                    _ => index.GetType().Name
                };

                var knowledgeIndex = new KnowledgeIndexInformation
                {
                    Type = indexType,
                    Id = index.Id,
                    Name = index.Name,
                    Version = index.Version,
                    Description = index.Description,
                    Tags = index.Tags?.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value) ?? null
                };

                indexes.Add(knowledgeIndex);
            }

            return indexes;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list knowledge indexes: {ex.Message}", ex);
        }
    }

    public async Task<KnowledgeIndexSchema> GetKnowledgeIndexSchema(string endpoint, string indexName, string? tenantId = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(endpoint, indexName);

        try
        {
            var credential = await GetCredential(tenantId);
            var indexesClient = new AIProjectClient(new Uri(endpoint), credential).GetIndexesClient();

            // Find the index by name using async enumerable
            var index = await indexesClient.GetIndicesAsync()
                .Where(i => string.Equals(i.Name, indexName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync();

            if (index == null)
            {
                throw new Exception($"Knowledge index '{indexName}' not found.");
            }

            // Map the SDK index to our AOT-safe schema type
            string indexType = index switch
            {
                AzureAISearchIndex => "AzureAISearchIndex",
                ManagedAzureAISearchIndex => "ManagedAzureAISearchIndex",
                CosmosDBIndex => "CosmosDBIndex",
                _ => index.GetType().Name
            };

            return new KnowledgeIndexSchema
            {
                Type = indexType,
                Id = index.Id,
                Name = index.Name,
                Version = index.Version,
                Description = index.Description,
                Tags = index.Tags?.ToDictionary(kvp => kvp.Key, kvp => (string?)kvp.Value)
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get knowledge index schema: {ex.Message}", ex);
        }
    }
}
