// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.ResiliencyAgent.Models;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Converts structured A2A data artifacts into portable Markdown without changing the source data.
/// The layouts mirror the Azure Portal artifact experience while using absolute public-cloud links.
/// </summary>
internal static class StructuredArtifactMarkdownRenderer
{
    private static readonly IReadOnlyDictionary<string, string> s_resourceTypeDisplayNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Microsoft.Compute/virtualMachines"] = "Virtual Machines",
            ["Microsoft.DBforPostgreSQL/flexibleServers"] = "PostgreSQL Flexible Servers",
            ["Microsoft.Network/loadBalancers"] = "Load Balancers",
            ["Microsoft.Sql/servers/databases"] = "SQL Databases",
            ["Microsoft.Storage/storageAccounts"] = "Storage Accounts",
        };

    internal static AgentArtifact? Render(
        string? artifactName,
        string? description,
        string? format,
        JsonElement data)
    {
        if (string.Equals(format, "datagrid", StringComparison.OrdinalIgnoreCase)
            && TryRenderDataGrid(data, out RenderedMarkdown grid))
        {
            return new AgentArtifact(
                ResolveMarkdownFileName(artifactName, grid.FileName),
                description,
                "text/markdown",
                grid.Content,
                format,
                grid.Family);
        }

        if (string.Equals(format, "resiliencyagent", StringComparison.OrdinalIgnoreCase)
            && TryRenderResiliencyAgent(data, out RenderedMarkdown resiliency))
        {
            return new AgentArtifact(
                ResolveMarkdownFileName(artifactName, resiliency.FileName),
                description,
                "text/markdown",
                resiliency.Content,
                format,
                resiliency.Family);
        }

        return null;
    }

    private static bool TryRenderDataGrid(JsonElement data, out RenderedMarkdown rendered)
    {
        rendered = default;
        if (data.ValueKind != JsonValueKind.Object
            || !data.TryGetProperty("columns", out JsonElement columnsElement)
            || columnsElement.ValueKind != JsonValueKind.Array
            || !data.TryGetProperty("data", out JsonElement rowsElement)
            || rowsElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        List<GridColumn> columns = [];
        foreach (JsonElement column in columnsElement.EnumerateArray())
        {
            string? id = ReadString(column, "id");
            string? header = ReadString(column, "header");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(header))
            {
                return false;
            }

            columns.Add(new GridColumn(id, header, ReadString(column, "type") ?? "string"));
        }

        if (columns.Count == 0)
        {
            return false;
        }

        HashSet<string> columnIds = columns.Select(column => column.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        string title;
        string fileName;
        string countLabel;
        if (columnIds.Contains("goalAssignedStatus"))
        {
            title = "Service Group goal status";
            fileName = "ServiceGroupGoalStatus.md";
            countLabel = "Service groups";
        }
        else if (columnIds.Contains("exemptionStatus"))
        {
            title = "Goal resource participation";
            fileName = "GoalResourceExemptionStatus.md";
            countLabel = "Resources";
        }
        else
        {
            title = "Azure resiliency data";
            fileName = "AzureResiliencyDataGrid.md";
            countLabel = "Rows";
        }

        List<JsonElement> rows = [.. rowsElement.EnumerateArray()];
        var markdown = new StringBuilder()
            .Append("# ")
            .AppendLine(title)
            .AppendLine()
            .Append("**")
            .Append(countLabel)
            .Append(":** ")
            .AppendLine(rows.Count.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .AppendLine()
            .Append("| ")
            .Append(string.Join(" | ", columns.Select(column => MarkdownRenderingHelpers.EscapeTableCell(column.Header))))
            .AppendLine(" |")
            .Append("|")
            .Append(string.Concat(columns.Select(_ => "---|")))
            .AppendLine();

        foreach (JsonElement row in rows)
        {
            IEnumerable<string> cells = columns.Select(column =>
                row.ValueKind == JsonValueKind.Object && row.TryGetProperty(column.Id, out JsonElement cell)
                    ? RenderCell(cell, column.Type)
                    : string.Empty);
            markdown.Append("| ").Append(string.Join(" | ", cells)).AppendLine(" |");
        }

        rendered = new RenderedMarkdown(fileName, "goals", markdown.ToString().TrimEnd());
        return true;
    }

    private static bool TryRenderResiliencyAgent(JsonElement content, out RenderedMarkdown rendered)
    {
        rendered = default;
        if (content.ValueKind != JsonValueKind.Object
            || ReadString(content, "resiliencyAgentArtifactType") is not string artifactType)
        {
            return false;
        }

        if (string.Equals(artifactType, "GetZonalPosture", StringComparison.OrdinalIgnoreCase)
            && TryRenderZonalPosture(content, out string zonalPosture))
        {
            rendered = new RenderedMarkdown(
                "Zone-Resilient Posture Report.md",
                "zonal-resiliency",
                zonalPosture);
            return true;
        }

        if (string.Equals(artifactType, "CheckPrerequisites", StringComparison.OrdinalIgnoreCase)
            && TryRenderZonalPrerequisites(content, out string prerequisites))
        {
            rendered = new RenderedMarkdown(
                "Zone-Resilient Enablement Assessment.md",
                "zonal-resiliency",
                prerequisites);
            return true;
        }

        if (string.Equals(artifactType, "SetZonalPosture", StringComparison.OrdinalIgnoreCase)
            && TryRenderZonalEnablement(content, out string enablement))
        {
            rendered = new RenderedMarkdown(
                "Zone-Resilient Enablement Results.md",
                "zonal-resiliency",
                enablement);
            return true;
        }

        if (!content.TryGetProperty("data", out JsonElement data)
            || data.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (string.Equals(artifactType, "ResourceCriteriaResolution", StringComparison.OrdinalIgnoreCase))
        {
            rendered = new RenderedMarkdown(
                "ResourceCriteriaResolution.md",
                "resources",
                RenderResourceCriteriaResolution(data));
            return true;
        }

        if (string.Equals(artifactType, "ServiceGroupDetails", StringComparison.OrdinalIgnoreCase))
        {
            rendered = new RenderedMarkdown(
                "ServiceGroupDetails.md",
                "service-groups",
                RenderServiceGroupDetails(data));
            return true;
        }

        return false;
    }

    private static bool TryRenderZonalPosture(JsonElement data, out string markdown)
    {
        markdown = string.Empty;

        if (!data.TryGetProperty("summary", out JsonElement summary)
            || summary.ValueKind != JsonValueKind.Object
            || !TryReadNumber(summary, "totalResources", out string totalResources)
            || !TryReadNumber(summary, "resilientResources", out string resilientResources)
            || !TryReadNumber(summary, "nonResilientResources", out string nonResilientResources)
            || !data.TryGetProperty("resources", out JsonElement resources)
            || resources.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        List<ZonalPostureResource> rows = [];
        foreach (JsonProperty resourceType in resources.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(resourceType.Name)
                || resourceType.Value.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement resource in resourceType.Value.EnumerateArray())
            {
                if (resource.ValueKind != JsonValueKind.Object
                    || ReadString(resource, "resourceId") is not string resourceId
                    || ReadString(resource, "status") is not string status
                    || string.IsNullOrWhiteSpace(status)
                    || !TryParseResource(resourceId, out ParsedResource parsed))
                {
                    return false;
                }

                rows.Add(new ZonalPostureResource(
                    parsed.Name,
                    resourceType.Name,
                    status,
                    parsed.ResourceId));
            }
        }

        var builder = new StringBuilder()
            .AppendLine("# Zonal Posture Report")
            .AppendLine()
            .AppendLine("## Summary")
            .AppendLine()
            .AppendLine("| Total Resources | Resilient Resources | Non-Resilient Resources |")
            .AppendLine("|---|---|---|")
            .AppendLine($"| {totalResources} | {resilientResources} | {nonResilientResources} |")
            .AppendLine()
            .AppendLine("## Resources")
            .AppendLine()
            .AppendLine("| Resource | Resource Type | Status |")
            .AppendLine("|---|---|---|");

        foreach (ZonalPostureResource row in rows)
        {
            builder.Append("| [")
                .Append(MarkdownRenderingHelpers.EscapeLinkLabel(row.Label))
                .Append("](")
                .Append(MarkdownRenderingHelpers.BuildPortalResourceUrl(row.ResourceId))
                .Append(") | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.ResourceType))
                .Append(" | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.Status))
                .AppendLine(" |");
        }

        markdown = builder.ToString().TrimEnd();
        return true;
    }

    private static bool TryRenderZonalPrerequisites(JsonElement data, out string markdown)
    {
        markdown = string.Empty;

        if (!data.TryGetProperty("summary", out JsonElement summary)
            || summary.ValueKind != JsonValueKind.Object
            || !TryReadNumber(summary, "totalResources", out string totalResources)
            || !TryReadNumber(summary, "alreadyEnabled", out string alreadyEnabled)
            || !TryReadNumber(summary, "readyToEnableByAgentOrGetScripts", out string readyToEnable)
            || !TryReadNumber(summary, "requiresManualAction", out string requiresManualAction)
            || !data.TryGetProperty("resources", out JsonElement resources)
            || resources.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        List<ZonalAssessmentResource> rows = [];
        foreach (JsonProperty resourceType in resources.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(resourceType.Name)
                || resourceType.Value.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement resource in resourceType.Value.EnumerateArray())
            {
                if (resource.ValueKind != JsonValueKind.Object
                    || ReadString(resource, "resourceId") is not string resourceId
                    || ReadString(resource, "assessmentStatus") is not string status
                    || string.IsNullOrWhiteSpace(status)
                    || !TryParseResource(resourceId, out ParsedResource parsed))
                {
                    return false;
                }

                List<string> blockingReasons = [];
                if (resource.TryGetProperty("blockingReasons", out JsonElement reasons))
                {
                    if (reasons.ValueKind != JsonValueKind.Array)
                    {
                        return false;
                    }

                    foreach (JsonElement reason in reasons.EnumerateArray())
                    {
                        if (reason.ValueKind != JsonValueKind.String)
                        {
                            return false;
                        }

                        blockingReasons.Add(reason.GetString() ?? string.Empty);
                    }
                }

                rows.Add(new ZonalAssessmentResource(
                    parsed.Name,
                    resourceType.Name,
                    status,
                    blockingReasons,
                    parsed.ResourceId));
            }
        }

        var builder = new StringBuilder()
            .AppendLine("# Zone-Resilient Enablement Assessment")
            .AppendLine()
            .AppendLine("## Summary")
            .AppendLine()
            .AppendLine("| Total Resources | Already Enabled | Ready to Enable | Requires Manual Action |")
            .AppendLine("|---|---|---|---|")
            .AppendLine($"| {totalResources} | {alreadyEnabled} | {readyToEnable} | {requiresManualAction} |")
            .AppendLine()
            .AppendLine("## Resources")
            .AppendLine()
            .AppendLine("| Resource | Resource Type | Assessment | Blocking Reasons |")
            .AppendLine("|---|---|---|---|");

        foreach (ZonalAssessmentResource row in rows)
        {
            string blockingReasons = row.BlockingReasons.Count > 0
                ? string.Join("<br>", row.BlockingReasons.Select(MarkdownRenderingHelpers.EscapeTableCell))
                : "None";
            builder.Append("| [")
                .Append(MarkdownRenderingHelpers.EscapeLinkLabel(row.Label))
                .Append("](")
                .Append(MarkdownRenderingHelpers.BuildPortalResourceUrl(row.ResourceId))
                .Append(") | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.ResourceType))
                .Append(" | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.Status))
                .Append(" | ")
                .Append(blockingReasons)
                .AppendLine(" |");
        }

        markdown = builder.ToString().TrimEnd();
        return true;
    }

    private static bool TryRenderZonalEnablement(JsonElement data, out string markdown)
    {
        markdown = string.Empty;

        if (!data.TryGetProperty("summary", out JsonElement summary)
            || summary.ValueKind != JsonValueKind.Object
            || !TryReadNumber(summary, "totalResources", out string totalResources)
            || !TryReadNumber(summary, "successfullyEnabled", out string successfullyEnabled)
            || !TryReadNumber(summary, "failedToEnable", out string failedToEnable)
            || !data.TryGetProperty("resources", out JsonElement resources)
            || resources.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        List<ZonalEnablementResource> rows = [];
        foreach (JsonProperty resourceType in resources.EnumerateObject())
        {
            if (string.IsNullOrWhiteSpace(resourceType.Name)
                || resourceType.Value.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement resource in resourceType.Value.EnumerateArray())
            {
                if (resource.ValueKind != JsonValueKind.Object
                    || ReadString(resource, "resourceId") is not string resourceId
                    || ReadString(resource, "status") is not string status
                    || string.IsNullOrWhiteSpace(status)
                    || !TryParseResource(resourceId, out ParsedResource parsed))
                {
                    return false;
                }

                rows.Add(new ZonalEnablementResource(
                    parsed.Name,
                    resourceType.Name,
                    status,
                    ReadString(resource, "remarks") ?? string.Empty,
                    parsed.ResourceId));
            }
        }

        var builder = new StringBuilder()
            .AppendLine("# Zone-Resilient Enablement Results")
            .AppendLine()
            .AppendLine("## Summary")
            .AppendLine()
            .AppendLine("| Total Resources | Successfully Enabled | Failed to Enable |")
            .AppendLine("|---|---|---|")
            .AppendLine($"| {totalResources} | {successfullyEnabled} | {failedToEnable} |")
            .AppendLine()
            .AppendLine("## Resources")
            .AppendLine()
            .AppendLine("| Resource | Resource Type | Status | Remarks |")
            .AppendLine("|---|---|---|---|");

        foreach (ZonalEnablementResource row in rows)
        {
            builder.Append("| [")
                .Append(MarkdownRenderingHelpers.EscapeLinkLabel(row.Label))
                .Append("](")
                .Append(MarkdownRenderingHelpers.BuildPortalResourceUrl(row.ResourceId))
                .Append(") | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.ResourceType))
                .Append(" | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.Status))
                .Append(" | ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(row.Remarks))
                .AppendLine(" |");
        }

        markdown = builder.ToString().TrimEnd();
        return true;
    }

    private static bool TryReadNumber(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (!element.TryGetProperty(propertyName, out JsonElement property)
            || property.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        value = property.GetRawText();
        return true;
    }

    private static string RenderResourceCriteriaResolution(JsonElement data)
    {
        var markdown = new StringBuilder("# Resource criteria resolution\n\n");
        AppendSummary(markdown, data, "Total resources");
        AppendResources(markdown, data);
        return markdown.ToString().TrimEnd();
    }

    private static string RenderServiceGroupDetails(JsonElement data)
    {
        var markdown = new StringBuilder("# Service Group details\n\n");
        if (data.TryGetProperty("serviceGroup", out JsonElement serviceGroup)
            && serviceGroup.ValueKind == JsonValueKind.Object)
        {
            string name = ReadString(serviceGroup, "name") ?? "Unknown";
            string resourceId = ReadString(serviceGroup, "resourceId") ?? string.Empty;
            string? parent = ReadString(serviceGroup, "parent");
            string? parentResourceId = ReadString(serviceGroup, "parentResourceId");

            markdown.AppendLine("## Service Group")
                .AppendLine()
                .AppendLine("| Property | Value |")
                .AppendLine("|---|---|")
                .Append("| Name | ")
                .Append(RenderResourceLink(name, resourceId))
                .AppendLine(" |");

            if (!string.IsNullOrWhiteSpace(parent))
            {
                markdown.Append("| Parent | ")
                    .Append(RenderResourceLink(parent, parentResourceId ?? string.Empty))
                    .AppendLine(" |");
            }

            if (data.TryGetProperty("goalsStatus", out JsonElement goalsStatus)
                && goalsStatus.ValueKind == JsonValueKind.Object
                && goalsStatus.TryGetProperty("assigned", out JsonElement assigned)
                && assigned.ValueKind is JsonValueKind.True or JsonValueKind.False)
            {
                markdown.Append("| Resiliency goal | ")
                    .Append(assigned.GetBoolean() ? "Assigned" : "Not assigned")
                    .AppendLine(" |");
            }

            markdown.AppendLine();
        }

        AppendSummary(markdown, data, "Total members");
        AppendResources(markdown, data);
        return markdown.ToString().TrimEnd();
    }

    private static void AppendSummary(StringBuilder markdown, JsonElement data, string totalLabel)
    {
        if (!data.TryGetProperty("summary", out JsonElement summary)
            && !data.TryGetProperty("memberSummary", out summary))
        {
            return;
        }

        if (summary.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        List<(string Type, int Count)> entries = [];
        foreach (JsonProperty property in summary.EnumerateObject())
        {
            int count = property.Value.ValueKind switch
            {
                JsonValueKind.Number when property.Value.TryGetInt32(out int number) => number,
                JsonValueKind.String when int.TryParse(
                    property.Value.GetString(),
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out int textNumber) => textNumber,
                _ => 0,
            };
            entries.Add((property.Name, count));
        }

        int total = entries.Sum(entry => entry.Count);
        markdown.Append("**").Append(totalLabel).Append(":** ")
            .AppendLine(total.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .AppendLine()
            .AppendLine("| Resource type | Count |")
            .AppendLine("|---|---:|");

        foreach ((string type, int count) in entries.OrderByDescending(entry => entry.Count))
        {
            markdown.Append("| ")
                .Append(MarkdownRenderingHelpers.EscapeTableCell(ResourceTypeDisplayName(type)))
                .Append(" | ")
                .Append(count.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .AppendLine(" |");
        }

        markdown.AppendLine();
    }

    private static void AppendResources(StringBuilder markdown, JsonElement data)
    {
        if (!data.TryGetProperty("resources", out JsonElement resources)
            || resources.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        List<ParsedResource> parsed = [];
        foreach (JsonElement resource in resources.EnumerateArray())
        {
            if (resource.ValueKind == JsonValueKind.String
                && resource.GetString() is string resourceId
                && TryParseResource(resourceId, out ParsedResource item))
            {
                parsed.Add(item);
            }
        }

        foreach (IGrouping<string, ParsedResource> group in parsed
                     .GroupBy(resource => resource.ResourceType, StringComparer.OrdinalIgnoreCase)
                     .OrderByDescending(group => group.Count()))
        {
            markdown.Append("## ")
                .AppendLine(ResourceTypeDisplayName(group.Key))
                .AppendLine()
                .AppendLine("| Resource | Subscription | Resource group |")
                .AppendLine("|---|---|---|");

            foreach (ParsedResource resource in group)
            {
                markdown.Append("| ")
                    .Append(RenderResourceLink(resource.Name, resource.ResourceId))
                    .Append(" | ")
                    .Append(MarkdownRenderingHelpers.EscapeTableCell(resource.SubscriptionId))
                    .Append(" | ")
                    .Append(MarkdownRenderingHelpers.EscapeTableCell(resource.ResourceGroup))
                    .AppendLine(" |");
            }

            markdown.AppendLine();
        }
    }

    private static string RenderCell(JsonElement cell, string type)
    {
        if (cell.ValueKind == JsonValueKind.Object)
        {
            string label = ReadString(cell, "label") ?? cell.GetRawText();
            if (string.Equals(type, "deepLink", StringComparison.OrdinalIgnoreCase))
            {
                string? deepLink = ReadString(cell, "deepLink");
                return string.IsNullOrWhiteSpace(deepLink)
                    ? MarkdownRenderingHelpers.EscapeTableCell(label)
                    : $"[{MarkdownRenderingHelpers.EscapeLinkLabel(label)}]({MarkdownRenderingHelpers.AbsolutePortalLink(deepLink)})";
            }

            if (string.Equals(type, "iconLabel", StringComparison.OrdinalIgnoreCase))
            {
                string? icon = ReadString(cell, "icon");
                return string.IsNullOrWhiteSpace(icon)
                    ? MarkdownRenderingHelpers.EscapeTableCell(label)
                    : $"{MarkdownRenderingHelpers.EscapeTableCell(IconText(icon))} {MarkdownRenderingHelpers.EscapeTableCell(label)}";
            }
        }

        return MarkdownRenderingHelpers.EscapeTableCell(Scalar(cell));
    }

    private static string RenderResourceLink(string label, string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return MarkdownRenderingHelpers.EscapeTableCell(label);
        }

        return $"[{MarkdownRenderingHelpers.EscapeLinkLabel(label)}]({MarkdownRenderingHelpers.BuildPortalResourceUrl(resourceId)})";
    }

    private static bool TryParseResource(string resourceId, out ParsedResource resource)
    {
        resource = default;
        string[] segments = resourceId
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return false;
        }

        string subscriptionId = SegmentAfter(segments, "subscriptions") ?? "Not provided";
        string resourceGroup = SegmentAfter(segments, "resourceGroups") ?? "Not provided";
        int providerIndex = FindFinalProviderIndex(segments);
        string resourceType = "Other";
        if (providerIndex >= 0)
        {
            List<string> typeSegments = [segments[providerIndex + 1]];
            for (int index = providerIndex + 2; index < segments.Length; index += 2)
            {
                typeSegments.Add(segments[index]);
            }

            resourceType = string.Join("/", typeSegments);
        }

        resource = new ParsedResource(
            "/" + string.Join("/", segments),
            segments[^1],
            subscriptionId,
            resourceGroup,
            resourceType);
        return true;
    }

    private static int FindFinalProviderIndex(string[] segments)
    {
        for (int index = segments.Length - 1; index >= 0; index--)
        {
            int remainingSegments = segments.Length - index - 1;
            if (string.Equals(segments[index], "providers", StringComparison.OrdinalIgnoreCase)
                && remainingSegments >= 3
                && remainingSegments % 2 == 1)
            {
                return index;
            }
        }

        return -1;
    }

    private static string? SegmentAfter(string[] segments, string name)
    {
        int index = Array.FindIndex(
            segments,
            segment => string.Equals(segment, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < segments.Length ? segments[index + 1] : null;
    }

    private static string ResourceTypeDisplayName(string resourceType)
    {
        if (s_resourceTypeDisplayNames.TryGetValue(resourceType, out string? displayName))
        {
            return displayName;
        }

        string leaf = resourceType.Split('/').LastOrDefault() ?? resourceType;
        var words = new StringBuilder();
        for (int index = 0; index < leaf.Length; index++)
        {
            char current = leaf[index];
            if (index > 0 && char.IsUpper(current) && char.IsLower(leaf[index - 1]))
            {
                words.Append(' ');
            }

            words.Append(current);
        }

        string value = words.ToString();
        return value.Length == 0
            ? resourceType
            : char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string IconText(string icon) =>
        icon.Contains("warning", StringComparison.OrdinalIgnoreCase) ? "Warning:" : icon;

    private static string Scalar(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Null or JsonValueKind.Undefined => string.Empty,
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => value.ToString(),
    };

    private static string ResolveMarkdownFileName(string? artifactName, string fallbackName)
    {
        string candidate = string.IsNullOrWhiteSpace(artifactName) ? fallbackName : artifactName.Trim();
        return string.Equals(Path.GetExtension(candidate), ".md", StringComparison.OrdinalIgnoreCase)
            ? candidate
            : candidate + ".md";
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out JsonElement property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private readonly record struct GridColumn(string Id, string Header, string Type);
    private readonly record struct RenderedMarkdown(string FileName, string Family, string Content);
    private readonly record struct ParsedResource(
        string ResourceId,
        string Name,
        string SubscriptionId,
        string ResourceGroup,
        string ResourceType);
    private readonly record struct ZonalPostureResource(
        string Label,
        string ResourceType,
        string Status,
        string ResourceId);
    private readonly record struct ZonalAssessmentResource(
        string Label,
        string ResourceType,
        string Status,
        IReadOnlyList<string> BlockingReasons,
        string ResourceId);
    private readonly record struct ZonalEnablementResource(
        string Label,
        string ResourceType,
        string Status,
        string Remarks,
        string ResourceId);
}
