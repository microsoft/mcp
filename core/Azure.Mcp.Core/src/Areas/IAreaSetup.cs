// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Core.Areas
{
    public interface IAreaSetup
    {
        /// <summary>
        /// Gets the name of the area.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Configure any dependencies.
        /// </summary>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Gets a tree whose root node represents the area.
        /// </summary>
        CommandGroup RegisterCommands(IServiceProvider serviceProvider);
    }
}
