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
    private readonly string _resourceName;

    public ResourceServerInstructionsProvider(Assembly assembly, string resourceName)
    {
        _assembly = assembly;
        _resourceName = resourceName;
    }

    public string GetServerInstructions()
    {
        var azureRulesContent = new StringBuilder();

        try
        {
            string content = EmbeddedResourceHelper.FindEmbeddedResource(_assembly, _resourceName);
            azureRulesContent.AppendLine(content);
            azureRulesContent.AppendLine();
        }
        catch (Exception)
        {
            // Log the error but continue processing other files
            azureRulesContent.AppendLine($"### Error loading {_resourceName}");
            azureRulesContent.AppendLine("An error occurred while loading this section.");
            azureRulesContent.AppendLine();
        }

        return azureRulesContent.ToString();
    }
}
