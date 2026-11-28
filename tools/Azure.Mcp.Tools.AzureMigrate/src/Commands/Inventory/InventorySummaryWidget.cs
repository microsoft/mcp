// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureMigrate.Commands.BusinessCase;
using Microsoft.Mcp.Core.Areas.Server.McpApps;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Inventory;

/// <summary>
/// POC: MCP Apps card for the inventory status tool. It renders the inventory summary ('result', Markdown) of the status result it is
/// shown with, using the business case report card in inventory mode. Like that card it does not track anything itself: the agent
/// waits for the inventory with the wait tool and then calls the status tool once, so the card appears together with the agent's
/// reply. Turned off with the same setting as the business case card.
/// </summary>
internal static class InventorySummaryWidget
{
    internal const string ResourceUri = "ui://azurecatalyst/inventory-summary-v5.html";

    internal static bool IsEnabled => BusinessCaseReportWidget.IsEnabled;

    /// <summary>Registers the card for the inventory status command when enabled.</summary>
    internal static void Register(string commandId)
    {
        if (IsEnabled)
        {
            McpAppRegistry.Register(commandId, new McpAppWidget(ResourceUri, "Azure Catalyst inventory summary", Html, []));
        }
    }

    internal static readonly string Html = BusinessCaseReportWidget.Html
        .Replace("var MODE = 'businesscase';", "var MODE = 'inventory';", StringComparison.Ordinal)
        .Replace("<title>Business case report</title>", "<title>Inventory summary</title>", StringComparison.Ordinal)
        .Replace("id=\"title\">Business case<", "id=\"title\">Inventory<", StringComparison.Ordinal)
        .Replace("Waiting for the business case...", "Waiting for the inventory...", StringComparison.Ordinal)
        .Replace("Show full report</button>", "Show full summary</button>", StringComparison.Ordinal);
}
