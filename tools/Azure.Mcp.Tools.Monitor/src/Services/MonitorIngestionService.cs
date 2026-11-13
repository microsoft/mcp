// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Models.Ingestion;
using Azure.Mcp.Tools.Monitor.Validation;
using Azure.Monitor.Ingestion;
using Azure.ResourceManager.Monitor;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Services.Azure.Authentication;

namespace Azure.Mcp.Tools.Monitor.Services;

public sealed class MonitorIngestionService(IAzureService azureService)
    : BaseAzureService(azureService), IMonitorIngestionService
{
    private const string EndpointServiceType = "monitor-ingestion";

    public async Task<IngestionUploadResult> UploadLogsAsync(
        string ingestionEndpoint,
        string dataCollectionRule,
        string stream,
        string logData,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(ingestionEndpoint), ingestionEndpoint),
            (nameof(dataCollectionRule), dataCollectionRule),
            (nameof(stream), stream),
            (nameof(logData), logData));

        var shape = IngestionLogDataValidator.Validate(logData, schema: null);
        if (!shape.IsValid)
        {
            var message = string.Join(" ", shape.Errors.Select(e => e.Message));
            throw new CommandValidationException(message) { TelemetrySafeMessage = "Invalid ingestion log data." };
        }

        var endpoint = CreateAndValidateEndpoint(AzureService, ingestionEndpoint);

        var credential = await GetCredential(tenant, cancellationToken);
        var clientOptions = AddDefaultPolicies(new LogsIngestionClientOptions());
        clientOptions.Audience = GetAudience();
        clientOptions.Transport = new HttpClientTransport(AzureService.GetClient());

        var client = new LogsIngestionClient(endpoint, credential, clientOptions);
        using var content = RequestContent.Create(BinaryData.FromString(logData));
        var response = await client.UploadAsync(dataCollectionRule, stream, content, context: new RequestContext { CancellationToken = cancellationToken });

        // The payload is sent as a single all-or-nothing request, so no records are ingested on error.
        return response.IsError
            ? new("Failed", dataCollectionRule, stream, 0)
            : new("Succeeded", dataCollectionRule, stream, shape.RecordCount);
    }

    public async Task<IngestionValidationResult> ValidateLogDataAsync(
        string subscription,
        string resourceGroup,
        string dataCollectionRule,
        string? stream,
        string logData,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(dataCollectionRule), dataCollectionRule),
            (nameof(logData), logData));

        var resourceGroupResource = await AzureService.GetResourceGroupResource(subscription, resourceGroup, tenant, cancellationToken: cancellationToken)
            ?? throw new KeyNotFoundException($"Resource group '{resourceGroup}' not found in subscription '{subscription}'.");

        var rule = await resourceGroupResource.GetDataCollectionRuleAsync(dataCollectionRule, cancellationToken);
        var declarations = rule.Value.Data.StreamDeclarations;

        string? selectedStream = stream;
        IReadOnlyDictionary<string, string>? schema = null;

        if (string.IsNullOrWhiteSpace(selectedStream))
        {
            if (declarations.Count == 1)
            {
                selectedStream = declarations.Keys.First();
            }
            else
            {
                throw new CommandValidationException(declarations.Count == 0
                    ? $"Data collection rule '{dataCollectionRule}' declares no custom streams. Provide --stream to name the target stream."
                    : $"Data collection rule '{dataCollectionRule}' declares multiple streams ({string.Join(", ", declarations.Keys.Order(StringComparer.Ordinal))}). Provide --stream to select one.")
                {
                    TelemetrySafeMessage = "Ambiguous data collection rule stream."
                };
            }
        }

        if (!declarations.TryGetValue(selectedStream, out var declaration))
        {
            throw new CommandValidationException(
                $"Stream '{selectedStream}' is not declared in data collection rule '{dataCollectionRule}'. Declared streams: {string.Join(", ", declarations.Keys.Order(StringComparer.Ordinal))}.")
            {
                TelemetrySafeMessage = "Stream is not declared in the data collection rule."
            };
        }

        schema = declaration.Columns
            .Where(c => c.Name is not null)
            .ToDictionary(c => c.Name!, c => c.DefinitionType?.ToString() ?? string.Empty, StringComparer.Ordinal);

        var result = IngestionLogDataValidator.Validate(logData, schema);
        return result with
        {
            DataCollectionRule = dataCollectionRule,
            Stream = selectedStream
        };
    }

    internal static Uri CreateAndValidateEndpoint(IAzureService azureService, string ingestionEndpoint)
    {
        if (!Uri.TryCreate(ingestionEndpoint, UriKind.Absolute, out var endpoint)
            || endpoint.Scheme != Uri.UriSchemeHttps
            || !string.IsNullOrEmpty(endpoint.UserInfo)
            || endpoint.AbsolutePath != "/"
            || !string.IsNullOrEmpty(endpoint.Query)
            || !string.IsNullOrEmpty(endpoint.Fragment))
        {
            throw new CommandValidationException("--ingestion-endpoint must be an HTTPS data collection endpoint root URL without user information, path, query, or fragment.");
        }

        azureService.ValidateAzureServiceEndpoint(endpoint.AbsoluteUri, EndpointServiceType);
        return endpoint;
    }

    private LogsIngestionAudience GetAudience() =>
        AzureService.CloudConfiguration.CloudType switch
        {
            AzureCloudConfiguration.AzureCloud.AzurePublicCloud => LogsIngestionAudience.AzurePublicCloud,
            AzureCloudConfiguration.AzureCloud.AzureChinaCloud => LogsIngestionAudience.AzureChina,
            AzureCloudConfiguration.AzureCloud.AzureUSGovernmentCloud => LogsIngestionAudience.AzureGovernment,
            var cloud => throw new NotSupportedException($"Cloud type '{cloud}' is not supported for Azure Monitor log ingestion.")
        };
}
