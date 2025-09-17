namespace Azure.Mcp.Tools.Playwright.Models;

public class PlaywrightQuota
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public PlaywrightQuotaProperties? Properties { get; set; }
}

public class PlaywrightQuotaProperties
{
    public FreeTrialProperties? FreeTrial { get; set; }
    public string? ProvisioningState { get; set; }
}

public class FreeTrialProperties
{
    public string? WorkspaceId { get; set; }
    public string? State { get; set; }
}

public class PlaywrightWorkspaceQuota
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public PlaywrightWorkspaceQuotaProperties? Properties { get; set; }
}

public class PlaywrightWorkspaceQuotaProperties
{
    public PlaywrightWorkspaceFreeTrialProperties? FreeTrial { get; set; }
    public string? ProvisioningState { get; set; }
}

public class PlaywrightWorkspaceFreeTrialProperties
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? ExpiryAt { get; set; }
    public int? AllocatedValue { get; set; }
    public double? UsedValue { get; set; }
    public double? PercentageUsed { get; set; }
}
