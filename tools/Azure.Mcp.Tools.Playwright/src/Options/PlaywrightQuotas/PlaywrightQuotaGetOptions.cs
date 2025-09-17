namespace Azure.Mcp.Tools.Playwright.Options;

public class PlaywrightQuotaGetOptions : BasePlaywrightOptions
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("quotaName")]
    public string? QuotaName { get; set; }
}
