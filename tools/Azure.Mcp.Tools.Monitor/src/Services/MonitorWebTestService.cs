// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Models.WebTests;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.ApplicationInsights.Models;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorWebTestService(ISubscriptionService subscriptionService, ITenantService tenantService, IResourceGroupService resourceGroupService)
    : BaseAzureService(tenantService), IMonitorWebTestService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService ?? throw new ArgumentNullException(nameof(resourceGroupService));

    public async Task<List<WebTestSummaryInfo>> ListWebTests(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

            var webTests = await subscriptionResource
                .GetApplicationInsightsWebTestsAsync()
                .Select(webTest => new WebTestSummaryInfo
                {
                    ResourceName = webTest.Data.Name,
                    Location = webTest.Data.Location,
                    ResourceGroup = webTest.Id.ResourceGroupName,
                    Kind = webTest.Data.WebTestKind?.ToString(),
                    AppInsightsComponentId = GetAppInsightsComponentIdFromWebTestData(webTest.Data)
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return webTests;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error retrieving web tests: {ex.Message}", ex);
        }
    }

    public async Task<List<WebTestSummaryInfo>> ListWebTests(
        string subscription,
        string resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup));

        try
        {
            var resourceGroupResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy) ??
                throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");

            var webTests = await resourceGroupResource
                .GetApplicationInsightsWebTests()
                .GetAllAsync()
                .Select(webTest => new WebTestSummaryInfo
                {
                    ResourceName = webTest.Data.Name,
                    Location = webTest.Data.Location,
                    ResourceGroup = webTest.Id.ResourceGroupName,
                    Kind = webTest.Data.WebTestKind?.ToString(),
                    AppInsightsComponentId = GetAppInsightsComponentIdFromWebTestData(webTest.Data)
                })
                .ToListAsync()
                .ConfigureAwait(false);

            return webTests;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error retrieving web tests: {ex.Message}", ex);
        }
    }
    public async Task<WebTestDetailedInfo> GetWebTest(
      string subscription,
      string resourceGroup,
      string resourceName,
      string? tenant = null,
      RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(resourceName), resourceName));

        try
        {
            var resourceGroupResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy) ??
                throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");

            var webTest = await resourceGroupResource.GetApplicationInsightsWebTestAsync(resourceName).ConfigureAwait(false);
            if (webTest == null || !webTest.HasValue || !webTest.Value!.HasData)
            {
                throw new Exception($"Error retrieving details for web test '{resourceName}'");
            }

            if (webTest.Value.Data.WebTestKind == WebTestKind.Ping || webTest.Value.Data.WebTestKind == WebTestKind.MultiStep)
            {
                throw new NotSupportedException($"Web test '{resourceName}' is of type '{webTest.Value.Data.WebTestKind}', which has been deprecated and is not supported by this command.");
            }

            var webTestData = webTest.Value.Data;
            var webTestId = webTest.Value.Id;

            var parsedHeaders = ParseHeadersFromRawResponse(webTest.GetRawResponse());
            PatchRequestWithCorrectedHeaders(webTestData.Request, parsedHeaders);

            return new WebTestDetailedInfo
            {
                ResourceName = webTestData.Name,
                Location = webTestData.Location.ToString(),
                Locations = webTestData.Locations
                    .Where(x => x.Location != null)
                    .Select(x => x.Location.ToString())
                    .Cast<string>()
                    .ToList(),
                ResourceGroup = webTestId.ResourceGroupName,
                Id = webTestId.ToString(),
                Kind = webTestData.WebTestKind?.ToString(),
                WebTestName = webTestData.WebTestName,
                IsEnabled = webTestData.IsEnabled,
                SyntheticMonitorId = webTestData.SyntheticMonitorId,
                FrequencyInSeconds = webTestData.FrequencyInSeconds,
                TimeoutInSeconds = webTestData.TimeoutInSeconds,
                IsRetryEnabled = webTestData.IsRetryEnabled,
                Request = webTestData.Request,
                ValidationRules = webTestData.ValidationRules,
                AppInsightsComponentId = GetAppInsightsComponentIdFromWebTestData(webTestData)
            };
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error retrieving web test {resourceName}: {ex.Message}", ex);
        }
    }

    public async Task<WebTestDetailedInfo> CreateWebTest(
        string subscription,
        string resourceGroup,
        string resourceName,
        string appInsightsComponentId,
        string location,
        string[] locations,
        string requestUrl,
        string? webTestName = null,
        string? description = null,
        bool? enabled = true,
        int? expectedStatusCode = null,
        bool? followRedirects = false,
        int? frequencyInSeconds = null,
        IReadOnlyDictionary<string, string>? headers = null,
        string? httpVerb = "GET",
        bool? ignoreStatusCode = false,
        bool? parseRequests = false,
        string? requestBody = null,
        bool? retryEnabled = false,
        bool? sslCheck = false,
        int? sslLifetimeCheckInDays = null,
        int? timeoutInSeconds = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(resourceName), resourceName),
            (nameof(appInsightsComponentId), appInsightsComponentId),
            (nameof(location), location),
            (nameof(requestUrl), requestUrl));

        try
        {
            var resourceGroupResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy) ??
                throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");

            // Check if web test already exists
            var existingWebTestResponse = await resourceGroupResource.GetApplicationInsightsWebTests()
                .ExistsAsync(resourceName)
                .ConfigureAwait(false);

            if (existingWebTestResponse.HasValue && existingWebTestResponse.Value)
            {
                throw new InvalidOperationException($"Web test '{resourceName}' already exists in resource group '{resourceGroup}'. Use the update command to modify an existing web test.");
            }

            var webTestData = new ApplicationInsightsWebTestData(new(location))
            {
                WebTestName = webTestName ?? resourceName,
                SyntheticMonitorId = resourceName,
                WebTestKind = WebTestKind.Standard,
                IsEnabled = enabled,
                FrequencyInSeconds = frequencyInSeconds,
                TimeoutInSeconds = timeoutInSeconds,
                IsRetryEnabled = retryEnabled,
                Description = description,
                Request = new WebTestRequest
                {
                    RequestUri = new Uri(requestUrl),
                    HttpVerb = (httpVerb ?? HttpMethod.Get.ToString()).ToUpperInvariant(),
                    RequestBody = requestBody,
                    FollowRedirects = followRedirects,
                    ParseDependentRequests = parseRequests
                },
                ValidationRules = new WebTestValidationRules
                {
                    ExpectedHttpStatusCode = expectedStatusCode,
                    IgnoreHttpStatusCode = ignoreStatusCode,
                    CheckSsl = sslCheck,
                    SslCertRemainingLifetimeCheck = sslCheck.HasValue && sslCheck.Value ? sslLifetimeCheckInDays : null
                },
            };

            webTestData.Tags.Add($"hidden-link:{appInsightsComponentId}", "Resource");

            foreach (var webTestLocation in locations)
            {
                webTestData.Locations.Add(new WebTestGeolocation
                {
                    Location = new AzureLocation(webTestLocation)
                });
            }

            if (headers != null)
            {
                foreach (var headerKey in headers.Keys)
                {
                    webTestData.Request.Headers.Add(new WebTestRequestHeaderField()
                    {
                        HeaderFieldName = headerKey,
                        HeaderFieldValue = headers[headerKey]
                    });
                }
            }

            var webTestArmResource = await resourceGroupResource.GetApplicationInsightsWebTests()
                .CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    resourceName,
                    webTestData
                ).ConfigureAwait(false);

            if (webTestArmResource == null || !webTestArmResource.HasCompleted || !webTestArmResource.HasValue)
            {
                throw new Exception($"Error creating web test resource with name '{resourceName}' in resource group '{resourceGroup}'");
            }

            var createdWebTest = webTestArmResource.Value.Data;

            var parsedHeaders = ParseHeadersFromRawResponse(webTestArmResource.GetRawResponse());
            PatchRequestWithCorrectedHeaders(createdWebTest.Request, parsedHeaders);

            return new WebTestDetailedInfo
            {
                ResourceName = createdWebTest.Name,
                Location = createdWebTest.Location.ToString(),
                Locations = createdWebTest.Locations
                    .Where(x => x.Location != null)
                    .Select(x => x.Location.ToString())
                    .Cast<string>()
                    .ToList(),
                ResourceGroup = webTestArmResource.Value.Id.ResourceGroupName,
                Id = webTestArmResource.Value.Id.ToString(),
                Kind = createdWebTest.WebTestKind?.ToString(),
                WebTestName = createdWebTest.WebTestName,
                IsEnabled = createdWebTest.IsEnabled,
                SyntheticMonitorId = createdWebTest.SyntheticMonitorId,
                FrequencyInSeconds = createdWebTest.FrequencyInSeconds,
                TimeoutInSeconds = createdWebTest.TimeoutInSeconds,
                IsRetryEnabled = createdWebTest.IsRetryEnabled,
                Request = createdWebTest.Request,
                ValidationRules = createdWebTest.ValidationRules,
                AppInsightsComponentId = GetAppInsightsComponentIdFromWebTestData(createdWebTest)
            };
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error creating web test {resourceName}: {ex.Message}", ex);
        }
    }

    public async Task<WebTestDetailedInfo> UpdateWebTest(
        string subscription,
        string resourceGroup,
        string resourceName,
        string? appInsightsComponentId = null,
        string? location = null,
        string[]? locations = null,
        string? requestUrl = null,
        string? webTestName = null,
        string? description = null,
        bool? enabled = null,
        int? expectedStatusCode = null,
        bool? followRedirects = null,
        int? frequencyInSeconds = null,
        IReadOnlyDictionary<string, string>? headers = null,
        string? httpVerb = null,
        bool? ignoreStatusCode = null,
        bool? parseRequests = null,
        string? requestBody = null,
        bool? retryEnabled = null,
        bool? sslCheck = null,
        int? sslLifetimeCheckInDays = null,
        int? timeoutInSeconds = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(resourceName), resourceName));

        try
        {
            var resourceGroupResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy) ??
                throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");

            // Get existing web test
            var existingWebTest = await resourceGroupResource.GetApplicationInsightsWebTestAsync(resourceName).ConfigureAwait(false);
            if (existingWebTest == null || !existingWebTest.HasValue || !existingWebTest.Value!.HasData)
            {
                throw new Exception($"Web test '{resourceName}' not found in resource group '{resourceGroup}'. Use the create command to create a new web test.");
            }

            var currentData = existingWebTest.Value.Data;
            if (currentData.WebTestKind == WebTestKind.Ping || currentData.WebTestKind == WebTestKind.MultiStep)
            {
                throw new NotSupportedException($"Web test '{resourceName}' is of type '{currentData.WebTestKind}', which has been deprecated and is not supported by this command.");
            }

            // Create updated web test data using existing values as defaults
            var webTestData = new ApplicationInsightsWebTestData(new(location ?? currentData.Location))
            {
                WebTestName = webTestName ?? currentData.WebTestName,
                SyntheticMonitorId = currentData.SyntheticMonitorId,
                WebTestKind = WebTestKind.Standard,
                IsEnabled = enabled ?? currentData.IsEnabled,
                FrequencyInSeconds = frequencyInSeconds ?? currentData.FrequencyInSeconds,
                TimeoutInSeconds = timeoutInSeconds ?? currentData.TimeoutInSeconds,
                IsRetryEnabled = retryEnabled ?? currentData.IsRetryEnabled,
                Description = description ?? currentData.Description,
                Request = new WebTestRequest
                {
                    RequestUri = requestUrl != null ? new Uri(requestUrl) : currentData.Request?.RequestUri,
                    HttpVerb = httpVerb?.ToUpperInvariant() ?? currentData.Request?.HttpVerb,
                    RequestBody = requestBody ?? currentData.Request?.RequestBody,
                    FollowRedirects = followRedirects ?? currentData.Request?.FollowRedirects,
                    ParseDependentRequests = parseRequests ?? currentData.Request?.ParseDependentRequests
                },
                ValidationRules = new WebTestValidationRules
                {
                    ExpectedHttpStatusCode = expectedStatusCode ?? currentData.ValidationRules?.ExpectedHttpStatusCode,
                    IgnoreHttpStatusCode = ignoreStatusCode ?? currentData.ValidationRules?.IgnoreHttpStatusCode,
                    CheckSsl = sslCheck ?? currentData.ValidationRules?.CheckSsl,
                    SslCertRemainingLifetimeCheck = (sslCheck ?? currentData.ValidationRules?.CheckSsl) == true
                                                   ? (sslLifetimeCheckInDays ?? currentData.ValidationRules?.SslCertRemainingLifetimeCheck)
                                                   : null
                },
            };

            // Preserve existing tags
            foreach (var tag in currentData.Tags)
            {
                webTestData.Tags.Add(tag.Key, tag.Value);
            }

            // Update locations
            if (locations != null)
            {
                foreach (var webTestLocation in locations)
                {
                    webTestData.Locations.Add(new WebTestGeolocation
                    {
                        Location = new AzureLocation(webTestLocation)
                    });
                }
            }
            else
            {
                // Preserve existing locations
                foreach (var existingLocation in currentData.Locations)
                {
                    webTestData.Locations.Add(existingLocation);
                }
            }

            // Update headers
            if (headers != null)
            {
                foreach (var headerKey in headers.Keys)
                {
                    webTestData.Request.Headers.Add(new WebTestRequestHeaderField()
                    {
                        HeaderFieldName = headerKey,
                        HeaderFieldValue = headers[headerKey]
                    });
                }
            }
            else if (currentData.Request?.Headers != null)
            {
                // Preserve existing headers
                foreach (var existingHeader in currentData.Request.Headers)
                {
                    webTestData.Request.Headers.Add(existingHeader);
                }
            }

            var webTestArmResource = await resourceGroupResource.GetApplicationInsightsWebTests()
                .CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    resourceName,
                    webTestData
                ).ConfigureAwait(false);

            if (webTestArmResource == null || !webTestArmResource.HasCompleted || !webTestArmResource.HasValue)
            {
                throw new Exception($"Error updating web test resource with name '{resourceName}' in resource group '{resourceGroup}'");
            }

            var updatedWebTest = webTestArmResource.Value.Data;

            var parsedHeaders = ParseHeadersFromRawResponse(webTestArmResource.GetRawResponse());
            PatchRequestWithCorrectedHeaders(updatedWebTest.Request, parsedHeaders);

            return new WebTestDetailedInfo
            {
                ResourceName = updatedWebTest.Name,
                Location = updatedWebTest.Location.ToString(),
                Locations = updatedWebTest.Locations
                    .Where(x => x.Location != null)
                    .Select(x => x.Location.ToString())
                    .Cast<string>()
                    .ToList(),
                ResourceGroup = webTestArmResource.Value.Id.ResourceGroupName,
                Id = webTestArmResource.Value.Id.ToString(),
                Kind = updatedWebTest.WebTestKind?.ToString(),
                WebTestName = updatedWebTest.WebTestName,
                IsEnabled = updatedWebTest.IsEnabled,
                SyntheticMonitorId = updatedWebTest.SyntheticMonitorId,
                FrequencyInSeconds = updatedWebTest.FrequencyInSeconds,
                TimeoutInSeconds = updatedWebTest.TimeoutInSeconds,
                IsRetryEnabled = updatedWebTest.IsRetryEnabled,
                Request = updatedWebTest.Request,
                ValidationRules = updatedWebTest.ValidationRules,
                AppInsightsComponentId = GetAppInsightsComponentIdFromWebTestData(updatedWebTest)
            };
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error updating web test {resourceName}: {ex.Message}", ex);
        }
    }

    private List<WebTestRequestHeaderField> ParseHeadersFromRawResponse(Response response)
    {
        using (var document = JsonDocument.Parse(response.Content))
        {
            var root = document.RootElement;
            var headers = new List<WebTestRequestHeaderField>();

            if (root.TryGetProperty("properties", out JsonElement properties) &&
                properties.TryGetProperty("Request", out JsonElement request) &&
                request.TryGetProperty("Headers", out JsonElement headersArray) &&
                headersArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var header in headersArray.EnumerateArray())
                {
                    if (header.TryGetProperty("Key", out JsonElement keyElement) &&
                        header.TryGetProperty("Value", out JsonElement valueElement))
                    {
                        string key = keyElement.GetString()!;
                        string value = valueElement.GetString()!;
                        headers.Add(new WebTestRequestHeaderField
                        {
                            HeaderFieldName = key,
                            HeaderFieldValue = value
                        });
                    }
                }
            }

            return headers;
        }
    }

    private void PatchRequestWithCorrectedHeaders(WebTestRequest request, List<WebTestRequestHeaderField> headers)
    {
        request.Headers.Clear();
        foreach (var header in headers)
        {
            request.Headers.Add(header);
        }
    }

    private string? GetAppInsightsComponentIdFromWebTestData(ApplicationInsightsWebTestData webTest)
    {
        if (webTest.Tags == null || webTest.Tags.Count == 0)
        {
            return null;
        }

        var hiddenLinkMatch = webTest.Tags.Keys.Select(x => AppInsightsComponentHiddenLinkTagRegex.Match(x)).FirstOrDefault(match => match.Success);
        return hiddenLinkMatch?.Groups[1].Value;
    }

    private readonly Regex AppInsightsComponentHiddenLinkTagRegex = new Regex("^hidden-link:(\\/subscriptions\\/[^\\/]+\\/resourceGroups\\/[^\\/]+\\/providers\\/microsoft\\.insights\\/components\\/[^\\/]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
