using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Playwright.Options;

public class BasePlaywrightOptions : SubscriptionOptions
{
    /// <summary>
    /// The name of the Playwright workspace.
    /// </summary>
    public string? PlaywrightWorkspaceName { get; set; }
}
