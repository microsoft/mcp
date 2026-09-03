// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Xml;
using Azure.Mcp.Tools.Advisor.Models.Chaos;

namespace Azure.Mcp.Tools.Advisor.Services;

public sealed partial class AdvisorChaosReviewService
{
    private static IReadOnlyList<WorkspaceCandidate> ParseWorkspaces(string body)
    {
        using var document = JsonDocument.Parse(body);
        var values = GetRequiredArray(document.RootElement, "value");
        return values.EnumerateArray()
            .Select(ParseWorkspace)
            .ToArray();
    }

    private static WorkspaceCandidate ParseWorkspace(string body)
    {
        using var document = JsonDocument.Parse(body);
        return ParseWorkspace(document.RootElement);
    }

    private static WorkspaceCandidate ParseWorkspace(JsonElement workspace) =>
        new(
            GetRequiredString(workspace, "id"),
            GetRequiredString(workspace, "name"),
            GetString(workspace, "location"),
            GetString(workspace, "properties", "provisioningState"),
            GetString(workspace, "identity", "type"),
            GetString(workspace, "identity", "principalId"),
            GetStringArray(workspace, "properties", "scopes"));

    private static ChaosWorkspaceCandidate ToWorkspaceCandidate(
        WorkspaceCandidate workspace,
        bool selectable,
        string? reasonCode) =>
        new(
            workspace.Id,
            workspace.Name,
            workspace.Location,
            workspace.ProvisioningState,
            workspace.IdentityType,
            workspace.PrincipalId,
            workspace.Scopes,
            selectable,
            reasonCode);

    private static IReadOnlyList<ScenarioCandidate> ParseScenarios(string body)
    {
        using var document = JsonDocument.Parse(body);
        var values = GetRequiredArray(document.RootElement, "value");
        return values.EnumerateArray()
            .Select(scenario => new ScenarioCandidate(
                GetRequiredString(scenario, "id"),
                GetRequiredString(scenario, "name"),
                GetString(scenario, "properties", "recommendation", "recommendationStatus"),
                GetActionIds(scenario)))
            .ToArray();
    }

    private static IReadOnlyList<ConfigurationCandidate> ParseConfigurations(string body)
    {
        using var document = JsonDocument.Parse(body);
        var values = GetRequiredArray(document.RootElement, "value");
        return values.EnumerateArray()
            .Select(configuration => new ConfigurationCandidate(
                GetRequiredString(configuration, "id"),
                GetRequiredString(configuration, "name"),
                GetString(configuration, "properties", "provisioningState"),
                GetString(configuration, "properties", "scenarioId"),
                GetConfigurationTargets(configuration),
                GetStringArray(
                    configuration,
                    "properties",
                    "resourceTargeting",
                    "include",
                    "locations"),
                GetStringArray(
                    configuration,
                    "properties",
                    "resourceTargeting",
                    "include",
                    "zones"),
                GetConfigurationParameter(configuration, "duration"),
                HasExactConfigurationParameterContract(configuration),
                HasExactResourceTargetingContract(configuration),
                ParseDateTime(GetString(
                    configuration,
                    "systemData",
                    "lastModifiedAt"))))
            .ToArray();
    }

    private static IReadOnlyList<RunCandidate> ParseRuns(string body)
    {
        using var document = JsonDocument.Parse(body);
        var values = GetRequiredArray(document.RootElement, "value");
        var runs = new List<RunCandidate>();

        foreach (var value in values.EnumerateArray())
        {
            var status = GetString(value, "properties", "status");
            var id = GetString(value, "id");
            var name = GetString(value, "name");
            var configurationName = GetString(
                value,
                "properties",
                "scenarioConfigurationName");
            var resources = GetRunResources(value);

            if (string.IsNullOrWhiteSpace(status) ||
                string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(configurationName))
            {
                if (string.IsNullOrWhiteSpace(status) || IsActiveRun(status))
                {
                    throw new JsonException(
                        "The run list contains an incomplete active or potentially active run.");
                }

                continue;
            }

            runs.Add(new(
                id,
                name,
                status,
                configurationName,
                resources,
                ParseDateTime(GetString(value, "properties", "startTime")),
                ParseDateTime(GetString(value, "properties", "endTime"))));
        }

        return runs;
    }

    private static ChaosRunSummary ToRunCandidate(
        ChaosWorkspaceCandidate workspace,
        ChaosScenarioCandidate scenario,
        RunCandidate run)
    {
        if (!IsSafeResourceNameSegment(run.ConfigurationName))
        {
            throw new JsonException(
                "The Chaos run configuration name is not a safe ARM resource name.");
        }

        return new(
            run.Id,
            run.Name,
            workspace.Id,
            scenario.Id,
            scenario.Name,
            $"{scenario.Id.TrimEnd('/')}/configurations/{run.ConfigurationName}",
            run.ConfigurationName,
            run.Status,
            run.StartTime,
            run.EndTime,
            run.ResourceIds);
    }

    private static IReadOnlyList<string> GetActionIds(JsonElement scenario)
    {
        if (!TryGetProperty(scenario, out var actions, "properties", "actions") ||
            actions.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return actions.EnumerateArray()
            .Select(action => GetString(action, "actionId"))
            .Where(actionId => !string.IsNullOrWhiteSpace(actionId))
            .Select(actionId => actionId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> GetConfigurationTargets(JsonElement configuration)
    {
        var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddStrings(
            targets,
            configuration,
            "properties",
            "resourceTargeting",
            "include",
            "resources");

        if (TryGetProperty(configuration, out var parameters, "properties", "parameters") &&
            parameters.ValueKind == JsonValueKind.Array)
        {
            foreach (var parameter in parameters.EnumerateArray())
            {
                if (!string.Equals(
                        GetString(parameter, "key"),
                        "targetResourceIds",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var value = GetString(parameter, "value");
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                using var targetDocument = JsonDocument.Parse(value);
                if (targetDocument.RootElement.ValueKind != JsonValueKind.Array)
                {
                    throw new JsonException("targetResourceIds must be a JSON array.");
                }

                foreach (var target in targetDocument.RootElement.EnumerateArray())
                {
                    if (target.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(target.GetString()))
                    {
                        targets.Add(target.GetString()!);
                    }
                }
            }
        }

        return targets.ToArray();
    }

    private static IReadOnlyList<string> GetRunResources(JsonElement run)
    {
        if (!TryGetProperty(run, out var resources, "properties", "resources") ||
            resources.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return resources.EnumerateArray()
            .Select(resource => GetString(resource, "id"))
            .Where(resourceId => !string.IsNullOrWhiteSpace(resourceId))
            .Select(resourceId => resourceId!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? GetConfigurationParameter(
        JsonElement configuration,
        string key)
    {
        if (!TryGetProperty(configuration, out var parameters, "properties", "parameters") ||
            parameters.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        return parameters.EnumerateArray()
            .Where(parameter => string.Equals(
                GetString(parameter, "key"),
                key,
                StringComparison.OrdinalIgnoreCase))
            .Select(parameter => GetString(parameter, "value"))
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static void AddStrings(
        HashSet<string> target,
        JsonElement root,
        params string[] path)
    {
        if (!TryGetProperty(root, out var values, path) ||
            values.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var value in values.EnumerateArray())
        {
            if (value.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(value.GetString()))
            {
                target.Add(value.GetString()!);
            }
        }
    }

    private static bool HasExactConfigurationParameterContract(
        JsonElement configuration)
    {
        if (!TryGetProperty(
                configuration,
                out var parameters,
                "properties",
                "parameters") ||
            parameters.ValueKind != JsonValueKind.Array ||
            parameters.GetArrayLength() != 2)
        {
            return false;
        }

        var keys = parameters.EnumerateArray()
            .Select(parameter => GetString(parameter, "key"))
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .ToArray();
        return keys.Length == 2 &&
            keys.Count(key => string.Equals(
                key,
                "duration",
                StringComparison.OrdinalIgnoreCase)) == 1 &&
            keys.Count(key => string.Equals(
                key,
                "targetResourceIds",
                StringComparison.OrdinalIgnoreCase)) == 1;
    }

    private static bool HasExactResourceTargetingContract(
        JsonElement configuration)
    {
        if (!TryGetProperty(
                configuration,
                out var targeting,
                "properties",
                "resourceTargeting") ||
            targeting.ValueKind != JsonValueKind.Object ||
            !targeting.TryGetProperty("include", out var include) ||
            include.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        foreach (var property in include.EnumerateObject())
        {
            if (property.Name is not ("locations" or "zones") ||
                property.Value.ValueKind != JsonValueKind.Array)
            {
                return false;
            }
        }

        if (!include.TryGetProperty("locations", out _) ||
            !include.TryGetProperty("zones", out _))
        {
            return false;
        }

        if (!targeting.TryGetProperty("exclude", out var exclude))
        {
            return true;
        }

        return exclude.ValueKind == JsonValueKind.Object &&
            exclude.EnumerateObject().All(property =>
                property.Value.ValueKind == JsonValueKind.Array &&
                property.Value.GetArrayLength() == 0);
    }

    private static bool IsSupportedDuration(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
        {
            return false;
        }

        try
        {
            var parsed = XmlConvert.ToTimeSpan(duration);
            return parsed >= TimeSpan.FromMinutes(1) &&
                parsed <= TimeSpan.FromDays(3);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }

    private static (int PermissionErrors, int ResourceErrors)
        GetValidationErrorCounts(JsonElement validation)
    {
        if (!TryGetProperty(
                validation,
                out var errors,
                "properties",
                "validationErrors") ||
            errors.ValueKind != JsonValueKind.Object)
        {
            return (0, 0);
        }

        return (
            GetArrayLength(errors, "permission"),
            GetArrayLength(errors, "resource"));
    }

    private static int GetArrayLength(
        JsonElement root,
        string propertyName) =>
        root.TryGetProperty(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;

    private static bool IsActiveRun(string status) =>
        !string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidationPending(string status) =>
        string.Equals(status, "Resolving", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Generating", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Validating", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Accepted", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "NotStarted", StringComparison.OrdinalIgnoreCase);

    private static DateTimeOffset? ParseDateTime(string? value) =>
        DateTimeOffset.TryParse(value, out var parsed) ? parsed : null;

    private static bool IsExactRunResource(
        string runResourceId,
        string scenarioId,
        string runId) =>
        Guid.TryParse(runId, out var parsedRunId) &&
        parsedRunId != Guid.Empty &&
        string.Equals(
            runResourceId.TrimEnd('/'),
            $"{scenarioId.TrimEnd('/')}/runs/{parsedRunId:D}",
            StringComparison.OrdinalIgnoreCase);

    private static bool DoesWorkspaceScopeCoverTarget(
        string? workspaceScope,
        string targetResourceId,
        Uri managementEndpoint)
    {
        if (string.IsNullOrWhiteSpace(workspaceScope) ||
            !TryGetArmPath(workspaceScope, managementEndpoint, out var scopePath) ||
            !TryGetArmPath(targetResourceId, managementEndpoint, out var targetPath))
        {
            return false;
        }

        var canonicalScope = scopePath.TrimEnd('/');
        var canonicalTarget = targetPath.TrimEnd('/');
        var scopeSegments = canonicalScope.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);
        var targetSegments = canonicalTarget.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);
        if (scopeSegments.Length < 2 ||
            targetSegments.Length < 2 ||
            !string.Equals(
                scopeSegments[0],
                "subscriptions",
                StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(
                targetSegments[0],
                "subscriptions",
                StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParse(scopeSegments[1], out var scopeSubscriptionId) ||
            !Guid.TryParse(targetSegments[1], out var targetSubscriptionId) ||
            scopeSubscriptionId != targetSubscriptionId)
        {
            return false;
        }

        return string.Equals(
                canonicalScope,
                canonicalTarget,
                StringComparison.OrdinalIgnoreCase) ||
            canonicalTarget.StartsWith(
                $"{canonicalScope}/",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExactChaosWorkspaceResourceId(
        string resourceId,
        Guid subscriptionId,
        string workspaceName,
        Uri managementEndpoint)
    {
        if (!TryGetArmPath(resourceId, managementEndpoint, out var path))
        {
            return false;
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 8 &&
            string.Equals(segments[0], "subscriptions", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(segments[1], out var parsedSubscriptionId) &&
            parsedSubscriptionId == subscriptionId &&
            string.Equals(segments[2], "resourceGroups", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(segments[3]) &&
            string.Equals(segments[4], "providers", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(segments[5], "Microsoft.Chaos", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(segments[6], "workspaces", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(segments[7], workspaceName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExactChildResourceId(
        string resourceId,
        string parentResourceId,
        string collectionName,
        string resourceName,
        Uri managementEndpoint)
    {
        if (!IsSafeResourceNameSegment(resourceName) ||
            !IsSafeResourceNameSegment(collectionName) ||
            !TryGetArmPath(resourceId, managementEndpoint, out var path) ||
            !TryGetArmPath(parentResourceId, managementEndpoint, out var parentPath))
        {
            return false;
        }

        return string.Equals(
            path.TrimEnd('/'),
            $"{parentPath.TrimEnd('/')}/{collectionName}/{resourceName}",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool AreRunResourcesWithinTarget(
        RunCandidate run,
        string targetResourceId,
        Uri managementEndpoint) =>
        run.ResourceIds.Count > 0 &&
        run.ResourceIds.All(resourceId =>
            IsResourceAtOrBelowTarget(
                resourceId,
                targetResourceId,
                managementEndpoint));

    private static bool IsResourceAtOrBelowTarget(
        string? resourceId,
        string targetResourceId,
        Uri managementEndpoint)
    {
        if (string.IsNullOrWhiteSpace(resourceId) ||
            !TryGetArmPath(resourceId, managementEndpoint, out var resourcePath) ||
            !TryGetArmPath(targetResourceId, managementEndpoint, out var targetPath))
        {
            return false;
        }

        var canonicalTarget = targetPath.TrimEnd('/');
        var canonicalResource = resourcePath.TrimEnd('/');
        return string.Equals(
                canonicalResource,
                canonicalTarget,
                StringComparison.OrdinalIgnoreCase) ||
            canonicalResource.StartsWith(
                $"{canonicalTarget}/",
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSafeResourceNameSegment(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 260 &&
        value is not "." and not ".." &&
        !value.Any(character =>
            character is '/' or '\\' or '%' or '?' or '#' or '<' or '>' or '&' or ':') &&
        !value.Any(char.IsControl);

    private static T? SelectById<T>(
        IReadOnlyList<T> candidates,
        string? selectedId,
        Func<T, string> getId)
        where T : class
    {
        if (selectedId is null)
        {
            return candidates.Count == 1 ? candidates[0] : null;
        }

        var matches = candidates
            .Where(candidate => string.Equals(
                getId(candidate).TrimEnd('/'),
                selectedId.TrimEnd('/'),
                StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static string WithChaosApiVersion(string resourceId) =>
        $"{resourceId}?api-version={Uri.EscapeDataString(ChaosApiVersion)}";

    private static bool TryValidateArmUri(string? value, Uri managementEndpoint)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 4096 ||
            value.Contains('#', StringComparison.Ordinal) ||
            value.Any(char.IsControl))
        {
            return false;
        }

        if (value.StartsWith("/", StringComparison.Ordinal))
        {
            return !value.StartsWith("//", StringComparison.Ordinal);
        }

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            string.Equals(
                uri.Host,
                managementEndpoint.Host,
                StringComparison.OrdinalIgnoreCase) &&
            uri.Port == managementEndpoint.Port &&
            string.IsNullOrEmpty(uri.UserInfo) &&
            string.IsNullOrEmpty(uri.Fragment);
    }

    private static bool TryGetArmPath(
        string value,
        Uri managementEndpoint,
        out string path)
    {
        path = string.Empty;
        if (!TryValidateArmUri(value, managementEndpoint))
        {
            return false;
        }

        if (value.StartsWith("/", StringComparison.Ordinal))
        {
            path = value.Split('?', 2)[0];
            return true;
        }

        path = new Uri(value, UriKind.Absolute).AbsolutePath;
        return true;
    }

    private static ChaosRemediationStatus Status(
        string status,
        bool ready,
        string? reasonCode,
        string message,
        ChaosTargetReview target,
        ChaosWorkspaceCandidate? workspace = null,
        IReadOnlyList<ChaosWorkspaceCandidate>? workspaceCandidates = null,
        ChaosScenarioCandidate? scenario = null,
        IReadOnlyList<ChaosScenarioCandidate>? scenarioCandidates = null,
        ChaosConfigurationCandidate? configuration = null,
        IReadOnlyList<ChaosConfigurationCandidate>? configurationCandidates = null,
        IReadOnlyList<ChaosRunSummary>? runCandidates = null,
        string? requiredPermission = null,
        ChaosValidationStatus? validation = null) =>
        new()
        {
            Status = status,
            Ready = ready,
            ReasonCode = reasonCode,
            Message = message,
            Target = target,
            WorkspaceCandidates = workspaceCandidates ?? [],
            Workspace = workspace,
            ScenarioCandidates = scenarioCandidates ?? [],
            Scenario = scenario,
            ConfigurationCandidates = configurationCandidates ?? [],
            Configuration = configuration,
            Runs = runCandidates ?? [],
            RequiredPermission = requiredPermission,
            Validation = validation,
        };

    private static ChaosRemediationStatus StatusReadFailure(
        string reasonCode,
        string message,
        string? recommendationTypeId,
        string? resourceId) =>
        Status(
            "Failed",
            false,
            reasonCode,
            message,
            new(
                "Failed",
                false,
                reasonCode,
                message,
                recommendationTypeId,
                resourceId,
                null,
                [],
                null,
                null));

    private static string? GetNextLink(string body)
    {
        using var document = JsonDocument.Parse(body);
        return GetString(document.RootElement, "nextLink");
    }

    private static JsonElement GetRequiredArray(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) ||
            value.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException($"{propertyName} must be an array.");
        }

        return value;
    }

    private static string GetRequiredString(
        JsonElement root,
        string propertyName) =>
        GetString(root, propertyName) is { Length: > 0 } value
            ? value
            : throw new JsonException($"{propertyName} is required.");

    private static bool TryGetProperty(
        JsonElement root,
        out JsonElement value,
        params string[] path)
    {
        value = root;
        foreach (var segment in path)
        {
            if (value.ValueKind != JsonValueKind.Object ||
                !value.TryGetProperty(segment, out value))
            {
                return false;
            }
        }

        return true;
    }

    private static string? GetString(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current.ValueKind == JsonValueKind.String
            ? current.GetString()
            : current.ToString();
    }

    private static long? GetInt64(JsonElement root, params string[] path)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(segment, out current))
            {
                return null;
            }
        }

        return current.ValueKind == JsonValueKind.Number &&
            current.TryGetInt64(out var value)
            ? value
            : null;
    }

    private static IReadOnlyList<string> GetStringArray(
        JsonElement root,
        params string[] path)
    {
        var current = root;
        foreach (var segment in path)
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(segment, out current))
            {
                return [];
            }
        }

        return current.ValueKind == JsonValueKind.Array
            ? current.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [];
    }

    private sealed record WorkspaceCandidate(
        string Id,
        string Name,
        string? Location,
        string? ProvisioningState,
        string? IdentityType,
        string? PrincipalId,
        IReadOnlyList<string> Scopes);

    private sealed record ScenarioCandidate(
        string Id,
        string Name,
        string? RecommendationStatus,
        IReadOnlyList<string> ActionIds);

    private sealed record ConfigurationCandidate(
        string Id,
        string Name,
        string? ProvisioningState,
        string? ScenarioId,
        IReadOnlyList<string> TargetResourceIds,
        IReadOnlyList<string> Locations,
        IReadOnlyList<string> Zones,
        string? Duration,
        bool HasExactParameterContract,
        bool HasExactResourceTargetingContract,
        DateTimeOffset? LastModifiedAt);

    private sealed record RunCandidate(
        string Id,
        string Name,
        string Status,
        string ConfigurationName,
        IReadOnlyList<string> ResourceIds,
        DateTimeOffset? StartTime,
        DateTimeOffset? EndTime);

    private sealed record ScenarioRunHistory(
        IReadOnlyList<RunCandidate> Runs,
        IReadOnlyList<ChaosRunSummary> Candidates,
        ChaosRemediationStatus? Failure)
    {
        public static ScenarioRunHistory Failed(ChaosRemediationStatus failure) =>
            new([], [], failure);
    }
}
