// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Builds <see cref="CommandGroup"/> trees from <see cref="CommandGroupDescriptor"/> data,
/// injecting inherited options based on <see cref="InheritOptions"/> and creating
/// <see cref="DescriptorBackedCommand"/> instances with deferred handler resolution.
/// </summary>
internal static class DescriptorCommandBuilder
{
    /// <summary>
    /// Converts a <see cref="CommandGroupDescriptor"/> into a <see cref="CommandGroup"/>
    /// that can be used with the existing <see cref="CommandFactory"/> infrastructure.
    /// </summary>
    /// <param name="descriptor">The command group descriptor.</param>
    /// <param name="handlerResolver">
    /// Resolves an <see cref="IBaseCommand"/> by handler type name.
    /// Called lazily at execution time, not at tree-building time.
    /// </param>
    public static CommandGroup Build(
        CommandGroupDescriptor descriptor,
        Func<string, IBaseCommand> handlerResolver)
    {
        var group = new CommandGroup(descriptor.Name, descriptor.Description, descriptor.Title);

        foreach (var cmd in descriptor.Commands)
        {
            var backed = new DescriptorBackedCommand(cmd, () => handlerResolver(cmd.HandlerType));

            // Inject inherited options based on InheritOptions
            AddInheritedOptions(backed.GetCommand(), cmd.InheritOptions);

            group.Commands[cmd.Name] = backed;
        }

        foreach (var sub in descriptor.SubGroups)
        {
            var subGroup = Build(sub, handlerResolver);
            group.AddSubGroup(subGroup);
        }

        return group;
    }

    /// <summary>
    /// Adds inherited options to a command based on its <see cref="InheritOptions"/>.
    /// </summary>
    private static void AddInheritedOptions(Command command, InheritOptions inheritOptions)
    {
        if (inheritOptions >= InheritOptions.Global)
        {
            command.Options.Add(OptionDefinitions.Common.Tenant);
            command.Options.Add(OptionDefinitions.Common.AuthMethod);
            command.Options.Add(OptionDefinitions.RetryPolicy.Delay);
            command.Options.Add(OptionDefinitions.RetryPolicy.MaxDelay);
            command.Options.Add(OptionDefinitions.RetryPolicy.MaxRetries);
            command.Options.Add(OptionDefinitions.RetryPolicy.Mode);
            command.Options.Add(OptionDefinitions.RetryPolicy.NetworkTimeout);
        }

        if (inheritOptions >= InheritOptions.Subscription)
        {
            command.Options.Add(OptionDefinitions.Common.Subscription);
            command.Validators.Add(commandResult =>
            {
                if (!CommandHelper.HasSubscriptionAvailable(commandResult))
                {
                    commandResult.AddError("Missing Required options: --subscription");
                }
            });
        }
    }
}

