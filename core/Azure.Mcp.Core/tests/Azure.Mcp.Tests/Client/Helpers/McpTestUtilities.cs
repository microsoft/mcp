// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;

namespace Azure.Mcp.Tests.Client.Helpers;

public static class McpTestUtilities
{
    /// <summary>Gets the first text contents in the list.</summary>
    public static string? GetFirstText(IList<ContentBlock> contents)
    {
        foreach (var c in contents)
        {
            if (c is EmbeddedResourceBlock { Resource: TextResourceContents { MimeType: "application/json" } text })
            {
                return text.Text;
            }
            else if (c is TextContentBlock tc)
            {
                return tc.Text;
            }
        }

        return null;
    }
}
