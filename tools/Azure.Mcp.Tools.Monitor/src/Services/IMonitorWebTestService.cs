// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Models.WebTests;

namespace Azure.Mcp.Tools.Monitor.Services;

public interface IMonitorWebTestService
{
    Task<List<WebTestSummaryInfo>> ListWebTests(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<WebTestSummaryInfo>> ListWebTests(
        string subscription,
        string resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<WebTestDetailedInfo> GetWebTest(
        string subscription,
        string resourceGroup,
        string name,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<WebTestDetailedInfo> CreateWebTest(
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
        RetryPolicyOptions? retryPolicy = null);

    Task<WebTestDetailedInfo> UpdateWebTest(
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
        RetryPolicyOptions? retryPolicy = null);
}
