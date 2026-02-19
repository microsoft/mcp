// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Areas;

public interface IAreaSetup
{
    /// <summary>
    /// Gets the name of the area.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the user-friendly title of the area for display purposes.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the category the command belongs to.
    /// </summary>
    CommandCategory Category => CommandCategory.AzureServices;

    /// <summary>
    /// Configure any dependencies.
    /// </summary>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Gets a tree whose root node represents the area.
    /// </summary>
    CommandGroup RegisterCommands(IServiceProvider serviceProvider);
}
