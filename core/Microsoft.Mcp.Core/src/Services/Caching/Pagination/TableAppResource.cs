// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// MCP resource that serves a single-page HTML table app for browsing
/// paginated results. The HTML is built from the <c>table-app</c> Vite project
/// using <c>@modelcontextprotocol/ext-apps</c> SDK and embedded in the assembly.
/// </summary>
public sealed class TableAppResource : McpServerResource
{
    public const string UriPrefix = "ui://tableapp";

    private static readonly ResourceTemplate s_template = new()
    {
        Name = "table-app",
        UriTemplate = "ui://tableapp",
        Description = "Interactive HTML table for browsing paginated MCP resource results.",
        MimeType = "text/html;profile=mcp-app",
    };

    private static readonly string s_htmlContent = LoadEmbeddedHtml();

    public override ResourceTemplate ProtocolResourceTemplate => s_template;

    public override IReadOnlyList<object> Metadata => [];

    public override bool IsMatch(string uri) =>
        uri.StartsWith(UriPrefix, StringComparison.OrdinalIgnoreCase);

    public override ValueTask<ReadResourceResult> ReadAsync(
        RequestContext<ReadResourceRequestParams> request,
        CancellationToken cancellationToken = default)
    {
        var result = new ReadResourceResult
        {
            Contents =
            [
                new TextResourceContents
                {
                    Uri = request.Params!.Uri,
                    MimeType = "text/html;profile=mcp-app",
                    Text = s_htmlContent,
                },
            ],
        };

        return ValueTask.FromResult(result);
    }

    private static string LoadEmbeddedHtml()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("TableApp.html");
        if (stream is null)
        {
            throw new InvalidOperationException(
                "Embedded resource 'TableApp.html' not found. Run 'npm run build' in the table-app directory.");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
