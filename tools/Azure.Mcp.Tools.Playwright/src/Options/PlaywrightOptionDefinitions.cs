namespace Azure.Mcp.Tools.Playwright.Options;

public static class PlaywrightOptionDefinitions
{
    public static readonly Option<string> PlaywrightWorkspace = new("--workspace")
    {
        Description = "The Playwright workspace name",
    };

    public static readonly Option<string> QuotaName = new("--quota-name")
    {
        Description = "The quota name (e.g., ExecutionMinutes)",
    };
}
