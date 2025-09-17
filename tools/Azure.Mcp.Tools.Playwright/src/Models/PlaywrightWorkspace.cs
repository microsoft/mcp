namespace Azure.Mcp.Tools.Playwright.Models;

public class PlaywrightWorkspace
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public string? ProvisioningState { get; set; }
    public string? DataPlaneUri { get; set; }
}

public class PlaywrightWorkspaceCreateOrUpdateRequest
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}

public class PlaywrightWorkspaceUpdateRequest
{
    public Dictionary<string, string>? Tags { get; set; }
    public string? RegionalAffinity { get; set; }
    public string? LocalAuth { get; set; }
}
