// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

public sealed class ToolCommandInfo
{
    /// <summary>
    /// Reference to the <see cref="Tool"/>'s Name. Used when sampling for tool area selection in --mode all's outer sampling request.
    /// </summary>
    public string? Tool { get; init; }

    /// <summary>
    /// Reference to the <see cref="Tool"/>'s Name. Used when sampling for tool command selection in --mode all's inner sampling request
    /// or --mode namespace's sampling request.
    /// </summary>
    public string? Command { get; init; }

    /// <summary>
    /// Description of the tool.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Input schema of the tool.
    /// </summary>
    public JsonElement? InputSchema { get; init; }

    public ToolCommandInfo(Tool tool, bool includeSchema = true)
    {
        Description = tool.Description;
        if (includeSchema)
        {
            Command = tool.Name;
            InputSchema = tool.InputSchema;
        }
        else
        {
            Tool = tool.Name;
        }
    }
}
