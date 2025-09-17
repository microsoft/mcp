namespace Azure.Mcp.Tools.Playwright.Options;

public class PlaywrightWorkspaceCreateOptions : BasePlaywrightOptions
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}
