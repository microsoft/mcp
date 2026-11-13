// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.ResourceManager;
using Azure.ResourceManager.CloudHealth;
using Azure.ResourceManager.CloudHealth.Models;
using Azure.ResourceManager.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorHealthModelService(IAzureService azureService, ILogger<MonitorHealthModelService> logger)
    : BaseAzureService(azureService), IMonitorHealthModelService
{
    private readonly ILogger<MonitorHealthModelService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    internal static HealthModelSummary ToSummary(HealthModelData data) =>
        new()
        {
            Id = data.Id?.ToString(),
            Name = data.Name,
            ResourceGroup = data.Id?.ResourceGroupName,
            Location = data.Location.ToString(),
            ProvisioningState = data.HealthModelProvisioningState?.ToString(),
        };

    public async Task<List<HealthModelSummary>> ListHealthModels(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        var results = new List<HealthModelSummary>();

        if (string.IsNullOrEmpty(resourceGroup))
        {
            await foreach (var model in subscriptionResource.GetHealthModelsAsync(cancellationToken))
            {
                results.Add(ToSummary(model.Data));
            }
        }
        else
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            await foreach (var model in resourceGroupResource.Value.GetHealthModels().GetAllAsync(cancellationToken: cancellationToken))
            {
                results.Add(ToSummary(model.Data));
            }
        }

        return results;
    }

    internal static HealthModelDetail ToDetail(HealthModelData data, string? healthState) =>
        new()
        {
            Id = data.Id?.ToString(),
            Name = data.Name,
            ResourceGroup = data.Id?.ResourceGroupName,
            Location = data.Location.ToString(),
            ProvisioningState = data.HealthModelProvisioningState?.ToString(),
            HealthState = healthState,
            Identity = ToIdentity(data.Identity),
            Tags = data.Tags,
        };

    internal static HealthModelIdentity? ToIdentity(ManagedServiceIdentity? identity)
    {
        if (identity is null)
        {
            return null;
        }

        return new HealthModelIdentity
        {
            Type = identity.ManagedServiceIdentityType.ToString(),
            PrincipalId = identity.PrincipalId?.ToString(),
            TenantId = identity.TenantId?.ToString(),
            UserAssignedIdentities = identity.UserAssignedIdentities?
                .Keys.Select(id => id.ToString()).ToList(),
        };
    }

    public async Task<HealthModelDetail> GetHealthModel(
        string subscription,
        string resourceGroup,
        string healthModelName,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(healthModelName), healthModelName));

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var model = await resourceGroupResource.Value.GetHealthModels().GetAsync(healthModelName, cancellationToken);
        var healthState = await TryGetRootHealthStateAsync(model.Value, healthModelName, cancellationToken);
        return ToDetail(model.Value.Data, healthState);
    }

    private async Task<string?> TryGetRootHealthStateAsync(HealthModelResource model, string rootEntityName, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await model.GetHealthModelEntityAsync(rootEntityName, cancellationToken);
            return entity.Value.Data.Properties?.HealthState?.ToString();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex,
                "Could not resolve root-entity health for health model '{HealthModel}'; returning null healthState. Error: {Message}",
                rootEntityName,
                ex.Message);
            return null;
        }
    }

    /// <summary>Upper bounds of one inspection: entities, relationships and signal definitions listed, requests in flight.</summary>
    internal const int MaxListed = 5_000;
    internal const int MaxConcurrentRequests = 8;

    // The fields kept when the caller names none: an entity's state and what makes it, without layout, icon or signals.
    private static readonly string[] DefaultFields =
    [
        "entity.name",
        "entity.properties.displayName",
        "entity.properties.healthState",
        "entity.properties.impact",
        "entity.properties.healthObjective",
        "entity.properties.discoveredBy",
        "entity.properties.provisioningState",
        "entity.properties.tags",
        "entity.properties.signalAggregationGroups",
        "entity.properties.signalGroups.dependencies",
        "history",
        "signalHistory",
        "annotations",
        "signalDefinitions",
    ];

    public async Task<HealthModelInspectResult> InspectHealthModel(
        string subscription,
        string resourceGroup,
        string healthModel,
        HealthModelInspectRequest request,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(healthModel), healthModel));

        var subscriptionId = (await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken)).Id.SubscriptionId!;
        var armClient = await CreateArmClientAsync(tenant, HealthModelApi.ClientOptions(), cancellationToken);
        var model = armClient.GetHealthModelResource(HealthModelResource.CreateResourceIdentifier(subscriptionId, resourceGroup, healthModel));

        // One entity at the current time is a single GET. Anything else lists the entities, at --timestamp when given.
        var (listed, truncated) = request is { Entity: { } single, Walk: null, Timestamp: null }
            ? ([Body((await Entity(armClient, model.Id, single).GetAsync(cancellationToken)).GetRawResponse())], false)
            : await ListAsync(model.GetHealthModelEntities().GetAllAsync(request.Timestamp, cancellationToken), cancellationToken);
        var entities = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var entity in listed)
        {
            entities.TryAdd(HealthModelInspection.Name(entity) ?? string.Empty, entity);
        }

        // A walk's paths are built only for the page it returns.
        List<(HealthModelWalkStep? Step, JsonElement Entity)> selected;
        if (request.Walk is { } walk)
        {
            var start = Find(entities, request.Entity ?? healthModel, healthModel, truncated);
            var (relationships, relationshipsTruncated) = await ListAsync(model.GetHealthModelRelationships().GetAllAsync(request.Timestamp, cancellationToken), cancellationToken);
            truncated |= relationshipsTruncated;
            selected = [.. HealthModelInspection.Walk(entities, relationships, start, walk).Select(step => ((HealthModelWalkStep?)step, step.Entity))];
        }
        else
        {
            selected = request.Entity is { } name
                ? [(null, entities[Find(entities, name, healthModel, truncated)])]
                : [.. listed.Select(entity => ((HealthModelWalkStep?)null, entity))];
        }

        selected = [.. selected.Where(item => HealthModelInspection.Admits(request.WhereHealth, HealthModelInspection.HealthState(item.Entity)))];
        var (page, nextSkip) = HealthModelInspection.Page(selected, request.Skip, request.Top);

        var fields = request.Fields is { Count: > 0 } chosen
            ? chosen
            : [.. DefaultFields, .. request.Include.Contains(HealthModelInclude.Signals) ? ["entity.properties.signalGroups"] : Array.Empty<string>()];

        List<JsonElement>? definitions = null;
        var named = page.SelectMany(item => HealthModelInspection.SignalNames(item.Entity).Definitions).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (named.Count > 0 && Keeps(fields, "signalDefinitions")
            && (request.Include.Contains(HealthModelInclude.Signals) || request.Include.Contains(HealthModelInclude.SignalHistory)))
        {
            var (listedDefinitions, definitionsTruncated) = await ListAsync(model.GetHealthModelSignalDefinitions().GetAllAsync(request.Timestamp, cancellationToken), cancellationToken);
            truncated |= definitionsTruncated;
            definitions = [.. listedDefinitions.Where(definition => named.Contains(HealthModelInspection.Name(definition) ?? string.Empty))];
        }

        // A payload no field keeps is not read at all.
        var reads = request with
        {
            Include = [.. request.Include.Where(include => include switch
            {
                HealthModelInclude.History => Keeps(fields, "history"),
                HealthModelInclude.SignalHistory => Keeps(fields, "signalHistory"),
                HealthModelInclude.Annotations => Keeps(fields, "annotations"),
                _ => true,
            })],
        };
        var items = new HealthModelInspectItem[page.Count];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, page.Count),
            new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentRequests, CancellationToken = cancellationToken },
            async (index, token) => items[index] = await CompleteAsync(armClient, model.Id, page[index].Step?.Path, page[index].Entity, reads, token));

        return new([.. items.Select(item => Project(item, fields))], selected.Count)
        {
            NextMarker = nextSkip?.ToString(CultureInfo.InvariantCulture),
            Truncated = truncated ? true : null,
            SignalDefinitions = definitions is null ? null : [.. definitions.Select(definition => Project(definition, fields, "signalDefinitions")!.Value)],
            Fields = request.Fields is { Count: > 0 } ? [.. items.SelectMany(Paths).Concat(definitions?.SelectMany(definition => HealthModelInspection.Paths(definition, "signalDefinitions")) ?? []).Distinct(StringComparer.Ordinal)] : null,
            Id = model.Id.ToString(),
            ReadAt = DateTimeOffset.UtcNow,
        };
    }

    private static string Find(Dictionary<string, JsonElement> entities, string name, string healthModel, bool truncated) =>
        entities.ContainsKey(name) ? name
        : throw new KeyNotFoundException(truncated
            ? $"Entity '{name}' is not among the first {MaxListed} entities of health model '{healthModel}'; read it without --walk or --timestamp."
            : $"Entity '{name}' was not found in health model '{healthModel}'.");

    /// <summary>Reads what the request includes for one entity, each as the API returns it. A failed read is named in the item's errors.</summary>
    private static async Task<HealthModelInspectItem> CompleteAsync(
        ArmClient armClient, ResourceIdentifier modelId, string? path, JsonElement entity, HealthModelInspectRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var resource = Entity(armClient, modelId, HealthModelInspection.Name(entity) ?? string.Empty);
        var (startAt, endAt) = HealthModelInspection.Window(request.StartAt, request.EndAt, request.Timestamp);
        var item = new HealthModelInspectItem(path, entity);

        if (request.Include.Contains(HealthModelInclude.History))
        {
            item = item with
            {
                History = await ReadAsync(errors, "history", async () =>
                    (await resource.GetHistoryAsync(new EntityHistoryContent { StartsOn = startAt, EndsOn = endAt }, cancellationToken)).GetRawResponse()),
            };
        }
        if (request.Include.Contains(HealthModelInclude.SignalHistory))
        {
            // Signal history is read for one entity only, so its signals share the request bound.
            // Results stay in signal order.
            var signals = HealthModelInspection.SignalNames(entity).Signals;
            var reads = new (JsonElement? Read, List<string> Errors)[signals.Count];
            await Parallel.ForEachAsync(
                Enumerable.Range(0, signals.Count),
                new ParallelOptions { MaxDegreeOfParallelism = MaxConcurrentRequests, CancellationToken = cancellationToken },
                async (index, token) =>
                {
                    var signalErrors = new List<string>();
                    reads[index] = (await ReadAsync(signalErrors, $"signal history {signals[index]}", async () =>
                        (await resource.GetSignalHistoryAsync(new EntitySignalHistoryContent(signals[index]) { StartsOn = startAt, EndsOn = endAt }, token)).GetRawResponse()), signalErrors);
                });
            errors.AddRange(reads.SelectMany(read => read.Errors));
            item = item with { SignalHistory = [.. reads.Where(read => read.Read is not null).Select(read => read.Read!.Value)] };
        }
        if (request.Include.Contains(HealthModelInclude.Annotations))
        {
            item = item with
            {
                Annotations = await ReadAsync(errors, "annotations", async () =>
                    (await resource.GetDataAnnotationsAsync(new EntityGetDataAnnotationsContent { StartsOn = startAt, EndsOn = endAt }, cancellationToken)).GetRawResponse()),
            };
        }
        return errors.Count > 0 ? item with { Errors = errors } : item;
    }

    // Keeps the selected fields of each payload. "*" keeps everything, and a payload that no field names is left out.
    private static HealthModelInspectItem Project(HealthModelInspectItem item, IReadOnlyCollection<string> fields) =>
        fields.Contains("*") ? item : item with
        {
            Entity = Project(item.Entity, fields, "entity") ?? EmptyObject,
            History = item.History is { } history ? Project(history, fields, "history") : null,
            SignalHistory = item.SignalHistory is { } signalHistory ? [.. signalHistory.Select(read => Project(read, fields, "signalHistory")!.Value)] : null,
            Annotations = item.Annotations is { } annotations ? Project(annotations, fields, "annotations") : null,
        };

    private static JsonElement? Project(JsonElement payload, IReadOnlyCollection<string> fields, string key)
    {
        if (fields.Contains("*") || fields.Contains(key))
        {
            return payload;
        }
        var paths = fields.Where(field => Names(field, key)).Select(field => field[(key.Length + 1)..]).ToList();
        return paths.Count > 0 ? HealthModelInspection.Project(payload, paths) : null;
    }

    private static bool Keeps(IReadOnlyCollection<string> fields, string key) => fields.Any(field => field == "*" || Names(field, key));

    private static bool Names(string field, string key) =>
        field == key || (field.Length > key.Length && field.StartsWith(key, StringComparison.Ordinal) && field[key.Length] == '.');

    private static IEnumerable<string> Paths(HealthModelInspectItem item) =>
        HealthModelInspection.Paths(item.Entity, "entity")
            .Concat(item.History is { } history ? HealthModelInspection.Paths(history, "history") : [])
            .Concat(item.SignalHistory?.SelectMany(read => HealthModelInspection.Paths(read, "signalHistory")) ?? [])
            .Concat(item.Annotations is { } annotations ? HealthModelInspection.Paths(annotations, "annotations") : []);

    private static readonly JsonElement EmptyObject = JsonDocument.Parse("{}").RootElement.Clone();

    private static HealthModelEntityResource Entity(ArmClient armClient, ResourceIdentifier modelId, string name) =>
        armClient.GetHealthModelEntityResource(modelId.AppendChildResource("entities", name));

    private static JsonElement Body(Response response)
    {
        using var document = JsonDocument.Parse(response.Content);
        return document.RootElement.Clone();
    }

    // Authentication, authorization and cancellation failures end the command.
    // Any other failure is named in the entity's errors.
    private static async Task<JsonElement?> ReadAsync(List<string> errors, string read, Func<Task<Response>> fetch)
    {
        try
        {
            return Body(await fetch());
        }
        catch (RequestFailedException ex) when (ex.Status is not (401 or 403))
        {
            errors.Add($"{read}: HTTP {ex.Status} {ex.ErrorCode}: {ex.Message.Split('\n')[0].Trim()}");
            return null;
        }
        // Retries that ran out on transport failures or timeouts.
        catch (AggregateException ex)
        {
            errors.Add($"{read}: {ex.Message.Split('\n')[0].Trim()}");
            return null;
        }
    }

    /// <summary>The items of every page of a list, each as the API returns it, up to <see cref="MaxListed"/>.</summary>
    private static async Task<(List<JsonElement> Items, bool Truncated)> ListAsync<T>(AsyncPageable<T> pageable, CancellationToken cancellationToken)
        where T : notnull
    {
        var items = new List<JsonElement>();
        await foreach (var page in pageable.AsPages().WithCancellation(cancellationToken))
        {
            using var document = JsonDocument.Parse(page.GetRawResponse().Content);
            if (!document.RootElement.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                continue;
            }
            foreach (var item in value.EnumerateArray())
            {
                if (items.Count == MaxListed)
                {
                    return (items, true);
                }
                items.Add(item.Clone());
            }
        }
        return (items, false);
    }
}
