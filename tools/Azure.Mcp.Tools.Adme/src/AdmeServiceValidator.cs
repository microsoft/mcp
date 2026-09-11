// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.Adme;

internal static class AdmeServiceValidator
{
    public static void ValidateTarget(
        string endpoint,
        string dataPartition,
        ValidationResult validationResult)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            validationResult.Errors.Add(
                "--endpoint must be an absolute HTTPS Azure Data Manager for Energy endpoint.");
        }
        else
        {
            try
            {
                ValidateEndpoint(endpointUri);
            }
            catch (Exception)
            {
                validationResult.Errors.Add(
                    "--endpoint must be an HTTPS Azure Data Manager for Energy endpoint hosted on an allowed domain.");
            }
        }

        if (string.IsNullOrWhiteSpace(dataPartition))
        {
            validationResult.Errors.Add("--data-partition must not be empty.");
        }
    }

    public static void ValidateKind(
        string kind,
        ValidationResult validationResult,
        bool allowWildcards = false)
    {
        var components = kind.Split(':');
        var hasValidComponents = components.Length == 4
            && components.All(component => !string.IsNullOrWhiteSpace(component))
            && components.All(component => !component.Any(char.IsWhiteSpace));
        var versionComponents = components.Length == 4
            ? components[^1].Split('.')
            : [];

        bool hasValidVersion;
        if (allowWildcards)
        {
            hasValidComponents = hasValidComponents
                && components[..^1].All(
                    component => component == "*" || !component.Contains('*', StringComparison.Ordinal));
            hasValidVersion = components.Length == 4 && components[^1] == "*"
                || versionComponents.Length == 3
                    && versionComponents.All(component => component == "*" || IsNumericVersionComponent(component));
        }
        else
        {
            hasValidComponents = hasValidComponents
                && components.All(component => !component.Contains('*', StringComparison.Ordinal));
            hasValidVersion = versionComponents.Length == 3
                && versionComponents.All(IsNumericVersionComponent);
        }

        if (!hasValidComponents || !hasValidVersion)
        {
            validationResult.Errors.Add(
                allowWildcards
                    ? "--kind must contain fully-qualified kind selectors in the format "
                        + "'authority:source:type:major.minor.patch', with optional '*' wildcard segments."
                    : "--kind must be a fully-qualified kind in the format "
                        + "'authority:source:type:major.minor.patch'.");
        }
    }

    private static bool IsNumericVersionComponent(string component)
        => component.Length > 0 && component.All(character => character is >= '0' and <= '9');

    public static void ValidateSearch(
        IReadOnlyList<string>? kinds,
        int limit,
        string? sort,
        string? spatialFilter,
        ValidationResult validationResult,
        int? offset = null)
    {
        if (kinds is not { Count: > 0 })
        {
            validationResult.Errors.Add("--kind must contain at least one non-empty OSDU kind selector.");
        }
        else
        {
            foreach (var kind in kinds)
            {
                ValidateKind(kind, validationResult, allowWildcards: true);
            }
        }

        if (limit is < 1 or > 1000)
        {
            validationResult.Errors.Add("--limit must be between 1 and 1000.");
        }

        if (offset is < 0)
        {
            validationResult.Errors.Add("--offset must not be negative.");
        }
        else if (offset is not null && (long)offset.Value + limit > 10000)
        {
            validationResult.Errors.Add("--offset plus --limit must not exceed 10000.");
        }

        if (!string.IsNullOrWhiteSpace(sort))
        {
            try
            {
                var criteria = AdmeServiceHelper.ParseSort(sort);
                if (criteria is null
                    || criteria.Field is not { Count: > 0 }
                    || criteria.Order is not { Count: > 0 }
                    || criteria.Field.Any(string.IsNullOrWhiteSpace)
                    || criteria.Order.Any(order =>
                        !string.Equals(order, "ASC", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(order, "DESC", StringComparison.OrdinalIgnoreCase)))
                {
                    validationResult.Errors.Add(
                        "--sort must contain non-empty field and order arrays using ASC or DESC order values.");
                }
                else if (criteria.Field.Count != criteria.Order.Count)
                {
                    validationResult.Errors.Add("--sort field and order arrays must have the same length.");
                }
            }
            catch (JsonException)
            {
                validationResult.Errors.Add("--sort must be a JSON object containing field and order string arrays.");
            }
        }

        if (!string.IsNullOrWhiteSpace(spatialFilter))
        {
            try
            {
                using var document = JsonDocument.Parse(spatialFilter);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    validationResult.Errors.Add("--spatial-filter must be a JSON object.");
                }
            }
            catch (JsonException)
            {
                validationResult.Errors.Add("--spatial-filter must be a JSON object.");
            }
        }
    }

    public static void ValidateRecordId(
        string? id,
        string optionName,
        ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            validationResult.Errors.Add($"{optionName} must not be empty.");
            return;
        }

        var partitionSeparator = id.IndexOf(':');
        var entitySeparator = partitionSeparator < 0
            ? -1
            : id.IndexOf(':', partitionSeparator + 1);
        var entityComponent = entitySeparator > partitionSeparator
            ? id.AsSpan(partitionSeparator + 1, entitySeparator - partitionSeparator - 1)
            : [];
        var typeSeparator = entityComponent.IndexOf("--", StringComparison.Ordinal);
        var hasValidFormat = partitionSeparator > 0
            && entitySeparator > partitionSeparator + 1
            && typeSeparator > 0
            && typeSeparator < entityComponent.Length - 2
            && !id.Any(char.IsWhiteSpace);

        if (!hasValidFormat)
        {
            validationResult.Errors.Add(
                $"{optionName} must contain fully-qualified record ids in the format "
                + "'{partition}:{group-type}--{EntityType}:{unique-id}'. ");
        }
    }

    public static Uri ValidateEndpoint(Uri endpoint)
    {
        EndpointValidator.ValidateAzureServiceEndpoint(endpoint.AbsoluteUri, "adme", ArmEnvironment.AzurePublicCloud);
        return endpoint;
    }
}
