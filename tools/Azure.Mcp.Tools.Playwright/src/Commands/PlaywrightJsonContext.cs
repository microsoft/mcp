using Azure.Mcp.Tools.Playwright.Commands.PlaywrightQuotas;
using Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaceQuotas;
using Azure.Mcp.Tools.Playwright.Commands.PlaywrightWorkspaces;
using Azure.Mcp.Tools.Playwright.Models;

namespace Azure.Mcp.Tools.Playwright.Commands;

[JsonSerializable(typeof(PlaywrightWorkspaceListCommand.PlaywrightWorkspaceListResult))]
[JsonSerializable(typeof(PlaywrightWorkspace))]
[JsonSerializable(typeof(PlaywrightQuotaListBySubscriptionCommand.PlaywrightQuotaListResult))]
[JsonSerializable(typeof(PlaywrightQuota))]
[JsonSerializable(typeof(PlaywrightWorkspaceQuotaListCommand.PlaywrightWorkspaceQuotaListResult))]
[JsonSerializable(typeof(PlaywrightWorkspaceQuota))]
internal sealed partial class PlaywrightJsonContext : JsonSerializerContext;
