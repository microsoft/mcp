// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Text;
using Azure.Mcp.Core.Helpers;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

// This is intentionally placed after the namespace declaration to avoid
// conflicts with Microsoft.Mcp.Core.Areas.Server.Options
public class ResourceServerInstructionsProvider : IServerInstructionsProvider
{
    private readonly Assembly _assembly;
    private readonly string _resourcePattern;

    public ResourceServerInstructionsProvider(Assembly assembly, string resourcePattern)
    {
        _assembly = assembly;
        _resourcePattern = resourcePattern;
    }

    public string GetServerInstructions()
    {
        var azureRulesContent = new StringBuilder();

        try
        {
            string resourceName = EmbeddedResourceHelper.FindEmbeddedResource(_assembly, _resourcePattern);
            string content = EmbeddedResourceHelper.ReadEmbeddedResource(_assembly, resourceName);
            azureRulesContent.AppendLine(content);
            azureRulesContent.AppendLine();
        }
        catch (Exception)
        {
            // Log the error but continue processing other files
            azureRulesContent.AppendLine($"### Error loading {_resourcePattern}");
            azureRulesContent.AppendLine("An error occurred while loading this section.");
            azureRulesContent.AppendLine();
        }

        return azureRulesContent.ToString();
    }
}
