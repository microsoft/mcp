namespace Azure.Mcp.Tools.Playwright.Options;

public class PlaywrightQuotaListBySubscriptionOptions : BasePlaywrightOptions
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }
}
