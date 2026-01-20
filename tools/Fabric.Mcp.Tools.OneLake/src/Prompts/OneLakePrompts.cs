// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;

namespace Fabric.Mcp.Tools.OneLake.Prompts;

[McpServerPromptType]
public sealed class OneLakePrompts
{
    [McpServerPrompt(Name = "onelake_list")]
    [Description("Guide an agent to enumerate OneLake paths safely (auth, paging, filters).")]
    public static ChatMessage[] List(
        [Description("Fabric workspace ID or name")] string workspace,
        [Description("Lakehouse name")] string lakehouse,
        [Description("Folder or table path")] string path,
        [Description("Max items to list (<=100)")] int? maxResults = null)
    {
        var header =
$@"You will list items in OneLake:
- ALWAYS call tool 'onelake_list_items' with: workspace, lakehouse, path.
- Use paging: set maxResults (<=100) and iterate cursors if provided.
- Do NOT assume paths exist; handle 404s gracefully.";

        var instruction =
$@"Now list contents at:
workspace={workspace}, lakehouse={lakehouse}, path={path}
{(maxResults is not null ? $"maxResults={maxResults}" : "maxResults not specified (default=100)")}";

        return new[]
        {
            new ChatMessage(ChatRole.User, header),
            new ChatMessage(ChatRole.User, instruction)
        };
    }

    [McpServerPrompt(Name = "onelake_query")]
    [Description("Run a safe SQL query against a lakehouse (prefer LIMIT & pagination).")]
    public static ChatMessage[] Query(
        [Description("Fabric workspace ID or name")] string workspace,
        [Description("Lakehouse name")] string lakehouse,
        [Description("SQL to execute (read-only preferred)")] string sql)
    {
        const string guide =
    """
    Use tool 'onelake_execute_sql'.
    - Validate SQL for read-only operations; avoid DDL/DML unless explicitly approved.
    - Prefer LIMIT 100 and paginate.
    """;

        var exec =
$@"Execute with:
workspace={workspace}, lakehouse={lakehouse}
sql:
{sql}";

        return new[]
        {
            new ChatMessage(ChatRole.User, guide),
            new ChatMessage(ChatRole.User, exec)
        };
    }

    [McpServerPrompt(Name = "onelake_best_practices")]
    [Description("Context & usage tips for OneLake tools: auth, partitions, paging, limits.")]
    public static ChatMessage[] BestPractices()
    {
        const string content =
    """
    When using OneLake tools:
    - Authenticate via the server's configured credential flow; do not embed secrets in prompts.
    - Prefer partition-aware reads; avoid scanning entire tables.
    - Use paging & cursors; set explicit row limits.
    - Surface schema first (columns/types) before large reads.
    """;

        return new[] { new ChatMessage(ChatRole.User, content) };
    }
}
