using System.Text.RegularExpressions;
using Azure.Mcp.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Services
{
    public class EmbeddedResourceProviderService(ILogger<EmbeddedResourceProviderService> logger) : IResourceProviderService
    {
        private readonly ILogger<EmbeddedResourceProviderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Task<string> GetResource(string resourceName)
        {
            _logger.LogInformation("Loading embedded resource: {ResourceName}", resourceName);

            var assembly = typeof(EmbeddedResourceProviderService).Assembly;
            string resourceFileName = EmbeddedResourceHelper.FindEmbeddedResource(assembly, resourceName);
            return Task.FromResult(EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resourceFileName));
        }

        public Task<string[]> ListResourcesInPath(string path, ResourceType? filterResources = null)
        {
            _logger.LogInformation("Listing resources in path: {Path}", path);

            var assembly = typeof(EmbeddedResourceProviderService).Assembly;

            var resourceFilterPattern = filterResources switch
            {
                ResourceType.File => @"*.*",
                ResourceType.Directory => @"*/",
                _ => "*"
            };
            string resourcePattern = Path.Combine(path, resourceFilterPattern);

            string[] names = assembly.GetManifestResourceNames();
            Regex regex;
            try
            {
                regex = new Regex(resourcePattern);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Invalid regex pattern: '{resourcePattern}'", nameof(path), ex);
            }

            return Task.FromResult(assembly.GetManifestResourceNames().Where(name => regex.IsMatch(name)).ToArray());
        }
    }
}
