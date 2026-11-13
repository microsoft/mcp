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
    private const string PortalOrigin = "https://portal.azure.com";
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
        string artifactName,
        string? description,
        string? format,
        JsonElement data)
    {
        if (string.Equals(format, "datagrid", StringComparison.OrdinalIgnoreCase)
            && TryRenderDataGrid(data, out RenderedMarkdown grid))
        {
            return new AgentArtifact(
                grid.FileName,
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
                resiliency.FileName,
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
            .Append(string.Join(" | ", columns.Select(column => EscapeTableCell(column.Header))))
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
            || ReadString(content, "resiliencyAgentArtifactType") is not string artifactType
            || !content.TryGetProperty("data", out JsonElement data)
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
                .Append(EscapeTableCell(ResourceTypeDisplayName(type)))
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
                    .Append(EscapeTableCell(resource.SubscriptionId))
                    .Append(" | ")
                    .Append(EscapeTableCell(resource.ResourceGroup))
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
                    ? EscapeTableCell(label)
                    : $"[{EscapeLinkLabel(label)}]({AbsolutePortalLink(deepLink)})";
            }

            if (string.Equals(type, "iconLabel", StringComparison.OrdinalIgnoreCase))
            {
                string? icon = ReadString(cell, "icon");
                return string.IsNullOrWhiteSpace(icon)
                    ? EscapeTableCell(label)
                    : $"{EscapeTableCell(IconText(icon))} {EscapeTableCell(label)}";
            }
        }

        return EscapeTableCell(Scalar(cell));
    }

    private static string RenderResourceLink(string label, string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return EscapeTableCell(label);
        }

        return $"[{EscapeLinkLabel(label)}]({BuildPortalResourceUrl(resourceId)})";
    }

    private static string AbsolutePortalLink(string link) =>
        link.StartsWith('#')
            ? $"{PortalOrigin}/{link}"
            : link;

    private static string BuildPortalResourceUrl(string resourceId)
    {
        string[] segments = resourceId
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 4
            && string.Equals(segments[0], "providers", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[1], "Microsoft.Management", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[2], "serviceGroups", StringComparison.OrdinalIgnoreCase))
        {
            if (Guid.TryParse(segments[3], out _))
            {
                string encodedResourceId = Uri.EscapeDataString("/" + string.Join("/", segments));
                return $"{PortalOrigin}/#view/Microsoft_Azure_Resources/ServiceGroupsBrowse.ReactView/" +
                    $"id/{encodedResourceId}";
            }

            return $"{PortalOrigin}/#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/overview/" +
                $"serviceGroupId/{Uri.EscapeDataString(segments[3])}";
        }

        return $"{PortalOrigin}/#@/resource/{string.Join("/", segments.Select(Uri.EscapeDataString))}";
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
        int providerIndex = Array.FindIndex(
            segments,
            segment => string.Equals(segment, "providers", StringComparison.OrdinalIgnoreCase));
        string resourceType = providerIndex >= 0 && providerIndex + 2 < segments.Length
            ? $"{segments[providerIndex + 1]}/{segments[providerIndex + 2]}"
            : "Other";
        resource = new ParsedResource(
            "/" + string.Join("/", segments),
            segments[^1],
            subscriptionId,
            resourceGroup,
            resourceType);
        return true;
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

    private static string EscapeTableCell(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("`", "\\`", StringComparison.Ordinal)
            .Replace("*", "\\*", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal)
            .Replace("[", "\\[", StringComparison.Ordinal)
            .Replace("]", "\\]", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    private static string EscapeLinkLabel(string value) =>
        EscapeTableCell(value)
            .Replace("[", "\\[", StringComparison.Ordinal)
            .Replace("]", "\\]", StringComparison.Ordinal);

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
}
