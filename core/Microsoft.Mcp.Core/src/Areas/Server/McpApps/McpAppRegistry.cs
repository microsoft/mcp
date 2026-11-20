// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.McpApps;

/// <summary>
/// An MCP Apps widget (HTML view) that a host renders for a tool.
/// </summary>
/// <param name="ResourceUri">The ui:// URI of the widget resource.</param>
/// <param name="Name">The resource name.</param>
/// <param name="Html">The widget HTML.</param>
/// <param name="ConnectDomains">Origins the widget may call with fetch.</param>
public sealed record McpAppWidget(string ResourceUri, string Name, string Html, IReadOnlyList<string> ConnectDomains);

/// <summary>
/// POC: registry of MCP Apps widgets keyed by command ID. Tools whose command has a widget get
/// <c>_meta.ui.resourceUri</c>, and the widgets are served through resources/list and resources/read.
/// </summary>
public static class McpAppRegistry
{
    /// <summary>The MIME type MCP Apps hosts expect for widget HTML.</summary>
    public const string WidgetMimeType = "text/html;profile=mcp-app";

    private static readonly ConcurrentDictionary<string, McpAppWidget> s_byCommandId = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers <paramref name="widget"/> for the command with ID <paramref name="commandId"/>.</summary>
    public static void Register(string commandId, McpAppWidget widget) => s_byCommandId[commandId] = widget;

    /// <summary>Gets whether any widget is registered.</summary>
    public static bool HasWidgets => !s_byCommandId.IsEmpty;

    /// <summary>Adds <c>ui.resourceUri</c> to <paramref name="meta"/> when the command has a widget.</summary>
    public static void AddToolMeta(string? commandId, JsonObject meta)
    {
        if (commandId is not null && s_byCommandId.TryGetValue(commandId, out McpAppWidget? widget))
        {
            meta["ui"] = new JsonObject { ["resourceUri"] = widget.ResourceUri };
        }
    }

    /// <summary>Lists the registered widgets as resources.</summary>
    public static ListResourcesResult ListResources() => new()
    {
        Resources = s_byCommandId.Values
            .DistinctBy(w => w.ResourceUri)
            .Select(w => new Resource { Uri = w.ResourceUri, Name = w.Name, MimeType = WidgetMimeType })
            .ToList(),
    };

    /// <summary>Reads the widget with URI <paramref name="uri"/>.</summary>
    public static ReadResourceResult ReadResource(string? uri)
    {
        McpAppWidget widget = s_byCommandId.Values.FirstOrDefault(w => string.Equals(w.ResourceUri, uri, StringComparison.Ordinal))
            ?? throw new McpException($"Resource not found: {uri}");

        var connectDomains = new JsonArray();
        foreach (string domain in widget.ConnectDomains)
        {
            connectDomains.Add((JsonNode?)JsonValue.Create(domain));
        }

        return new()
        {
            Contents =
            [
                new TextResourceContents
                {
                    Uri = widget.ResourceUri,
                    MimeType = WidgetMimeType,
                    Text = widget.Html,
                    Meta = new JsonObject
                    {
                        ["ui"] = new JsonObject { ["csp"] = new JsonObject { ["connectDomains"] = connectDomains } },
                    },
                },
            ],
        };
    }
}
