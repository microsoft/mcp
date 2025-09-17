namespace Azure.Mcp.Tools.Playwright.Services;

using Azure.Mcp.Tools.Playwright.Models;

public interface IPlaywrightService
{
    Task<List<PlaywrightWorkspace>> GetPlaywrightWorkspacesAsync(string subscription, string? resourceGroup = null, string? workspaceName = null, string? tenant = null);

    Task<PlaywrightWorkspace?> GetPlaywrightWorkspaceAsync(string subscription, string resourceGroup, string workspaceName, string? tenant = null);

    Task<PlaywrightWorkspace> CreateOrUpdatePlaywrightWorkspaceAsync(string subscription, string resourceGroup, PlaywrightWorkspaceCreateOrUpdateRequest request, string? tenant = null);

    Task<PlaywrightWorkspace> UpdatePlaywrightWorkspaceAsync(string subscription, string resourceGroup, string workspaceName, PlaywrightWorkspaceUpdateRequest request, string? tenant = null);

    Task DeletePlaywrightWorkspaceAsync(string subscription, string resourceGroup, string workspaceName, string? tenant = null);

    Task<List<PlaywrightQuota>> GetSubscriptionPlaywrightQuotasAsync(string subscription, string location, string? tenant = null);

    Task<PlaywrightQuota?> GetSubscriptionPlaywrightQuotaAsync(string subscription, string location, string quotaName, string? tenant = null);

    Task<List<PlaywrightWorkspaceQuota>> GetPlaywrightWorkspaceQuotasAsync(string subscription, string resourceGroup, string workspaceName, string? tenant = null);

    Task<PlaywrightWorkspaceQuota?> GetPlaywrightWorkspaceQuotaAsync(string subscription, string resourceGroup, string workspaceName, string quotaName, string? tenant = null);
}
