using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Services
{
    public class EmbeddedResourceProviderService(ILogger<EmbeddedResourceProviderService> logger) : IResourceProviderService
    {
        private readonly ILogger<EmbeddedResourceProviderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public static string GetEmbeddedResource(string resourceName)
        {
            var assembly = typeof(EmbeddedResourceProviderService).Assembly;
            string resourceFileName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourceName);
            return EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceFileName);
        }

        public Task<string> GetResource(string resourceName)
        {
            _logger.LogInformation("Loading embedded resource: {ResourceName}", resourceName);

            return Task.FromResult(GetEmbeddedResource(resourceName));
        }

        public Task<string[]> ListResourcesInPath(string path, ResourceType? filterResources = null)
        {
            _logger.LogInformation("Listing resources in path: {Path}", path);

            var assembly = typeof(EmbeddedResourceProviderService).Assembly;
            string[] allResourceNames = assembly.GetManifestResourceNames();

            var resourcePath = $"Resources/{path}";

            // Filter resources that start with the prefix
            var matchingResources = allResourceNames
                .Where(name => name.StartsWith(resourcePath, StringComparison.OrdinalIgnoreCase));

            // Apply resource type filtering
            var filteredResources = filterResources switch
            {
                ResourceType.File => FilterTopLevelResourceFiles(matchingResources, resourcePath),
                ResourceType.Directory => FilterTopLevelResourceDirectories(matchingResources, resourcePath),
                _ => FilterTopLevelResourceFiles(matchingResources, resourcePath)
                        .Concat(FilterTopLevelResourceDirectories(matchingResources, resourcePath))
                        .Distinct(),
            };

            // Return the original embedded resource names
            return Task.FromResult(filteredResources.ToArray());
        }

        private static IEnumerable<string> FilterTopLevelResourceFiles(IEnumerable<string> resources, string path)
        {
            return resources
                .Where(name => name.Substring(path.Length).Count(c => c == '/') == 0)
                .Select(name => name.Substring(path.Length))
                .Distinct();
        }

        private static IEnumerable<string> FilterTopLevelResourceDirectories(IEnumerable<string> resources, string path)
        {
            return resources
                .Where(name => name.Substring(path.Length).Count(c => c == '/') > 0)
                .Select(name => name.Substring(path.Length, name.Substring(path.Length).IndexOf('/')))
                .Distinct();
        }
    }
}
