namespace Azure.Mcp.Tools.Playwright.Options;

public class PlaywrightWorkspaceQuotaGetOptions : BasePlaywrightOptions
{
    [JsonPropertyName("quotaName")]
    public string? QuotaName { get; set; }
}
