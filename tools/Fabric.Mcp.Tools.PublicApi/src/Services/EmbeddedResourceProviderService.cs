using System.IO;
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

            // Normalize path to use underscores (embedded resources use underscores instead of slashes)
            string normalizedName = resourceName.Replace('/', '.').Replace('\\', '.').Replace('-', '_');

            return Task.FromResult(GetEmbeddedResource(normalizedName));
        }

        public Task<string[]> ListResourcesInPath(string path, ResourceType? filterResources = null)
        {
            _logger.LogInformation("Listing resources in path: {Path}", path);

            var assembly = typeof(EmbeddedResourceProviderService).Assembly;
            string[] allResourceNames = assembly.GetManifestResourceNames();

            // Normalize path to use underscores (embedded resources use underscores instead of slashes)
            string normalizedPath = path.Replace('/', '.').Replace('\\', '.').Replace('-', '_');

            // Build the expected resource name prefix
            string resourcePrefix = string.IsNullOrEmpty(normalizedPath)
                ? $"{assembly.GetName().Name}.Resources."
                : $"{assembly.GetName().Name}.Resources.{normalizedPath}";

            // Filter resources that start with the prefix
            var matchingResources = allResourceNames
                .Where(name => name.StartsWith(resourcePrefix, StringComparison.OrdinalIgnoreCase));

            // Apply resource type filtering
            var filteredResources = filterResources switch
            {
                ResourceType.File => FilterTopLevelResourceFiles(matchingResources, resourcePrefix),
                ResourceType.Directory => FilterTopLevelResourceDirectories(matchingResources, resourcePrefix),
                _ => FilterTopLevelResourceFiles(matchingResources, resourcePrefix)
                        .Concat(FilterTopLevelResourceDirectories(matchingResources, resourcePrefix))
                        .Distinct(),
            };

            // Return the original embedded resource names
            return Task.FromResult(filteredResources.ToArray());
        }

        private static IEnumerable<string> FilterTopLevelResourceFiles(IEnumerable<string> resources, string resourcePrefix)
        {
            return resources
                .Where(name => name.Substring(resourcePrefix.Length).Count(c => c == '.') == 1)
                .Select(name => name.Substring(resourcePrefix.Length))
                .Distinct();
        }

        private static IEnumerable<string> FilterTopLevelResourceDirectories(IEnumerable<string> resources, string resourcePrefix)
        {
            return resources
                .Where(name => name.Substring(resourcePrefix.Length).Count(c => c == '.') > 1)
                .Select(name => name.Substring(resourcePrefix.Length, name.Substring(resourcePrefix.Length).IndexOf('.')))
                .Distinct();
        }
    }
}
