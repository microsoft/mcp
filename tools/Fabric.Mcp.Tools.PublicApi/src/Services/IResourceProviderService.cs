namespace Fabric.Mcp.Tools.PublicApi.Services
{
    public enum ResourceType
    {
        File,
        Directory
    }

    public interface IResourceProviderService
    {
        Task<string> GetResource(string resourceName);

        Task<string[]> ListResourcesInPath(string path, ResourceType? filterResources = null);
    }
}
