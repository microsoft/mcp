// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Helpers;

public static class CommandHelper
{
    public static IEnumerable<KeyValuePair<string, IBaseCommand>> GetVisibleCommands(IReadOnlyDictionary<string, IBaseCommand> commands)
    {
        return commands
            .Where(kvp => kvp.Value.GetType().GetCustomAttribute<HiddenCommandAttribute>() == null)
            .OrderBy(kvp => kvp.Key);
    }
}
