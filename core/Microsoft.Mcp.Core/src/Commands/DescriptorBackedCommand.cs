// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Microsoft.Mcp.Core.Commands.Descriptors;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// An <see cref="IBaseCommand"/> implementation backed by a <see cref="CommandDescriptor"/>.
/// Metadata comes from the descriptor; the underlying <see cref="IBaseCommand"/> handler is
/// resolved from DI only when <see cref="ExecuteAsync"/> is called.
/// </summary>
internal sealed class DescriptorBackedCommand(
    CommandDescriptor descriptor,
    Func<IBaseCommand> handlerFactory) : IBaseCommand
{
    private readonly Command _command = CreateCommand(descriptor);
    private IBaseCommand? _handler;

    public string Id => descriptor.Id;
    public string Name => descriptor.Name;
    public string Description => descriptor.Description;
    public string Title => descriptor.Title;

    public ToolMetadata Metadata => descriptor.Annotations?.ToToolMetadata() ?? new ToolMetadata
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    public Command GetCommand() => _command;

    public Task<CommandResponse> ExecuteAsync(
        CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        _handler ??= handlerFactory();
        return _handler.ExecuteAsync(context, parseResult, cancellationToken);
    }

    public ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        _handler ??= handlerFactory();
        return _handler.Validate(commandResult, commandResponse);
    }

    private static Command CreateCommand(CommandDescriptor descriptor)
    {
        var command = new Command(descriptor.Name, descriptor.Description);

        if (descriptor.Hidden)
        {
            command.Hidden = true;
        }

        foreach (var opt in descriptor.Options)
        {
            command.Options.Add(CreateOption(opt));
        }

        return command;
    }

    internal static Option CreateOption(OptionDescriptor opt)
    {
        Option option = opt.TypeName.ToLowerInvariant() switch
        {
            "string" => CreateTypedOption<string>(opt),
            "int32" or "int" => CreateTypedOption<int>(opt),
            "int64" or "long" => CreateTypedOption<long>(opt),
            "boolean" or "bool" => CreateTypedOption<bool>(opt),
            "double" => CreateTypedOption<double>(opt),
            _ => CreateTypedOption<string>(opt)
        };

        return option;
    }

    private static Option<T> CreateTypedOption<T>(OptionDescriptor opt)
    {
        var option = new Option<T>($"--{opt.Name}")
        {
            Description = opt.Description,
            Required = opt.Required
        };

        if (opt.Hidden)
        {
            option.Hidden = true;
        }

        if (opt.DefaultValue is not null)
        {
            option.DefaultValueFactory = _ => ParseDefault<T>(opt.DefaultValue);
        }

        return option;
    }

    private static T ParseDefault<T>(string value)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)value;
        if (typeof(T) == typeof(int) && int.TryParse(value, out var i))
            return (T)(object)i;
        if (typeof(T) == typeof(long) && long.TryParse(value, out var l))
            return (T)(object)l;
        if (typeof(T) == typeof(bool) && bool.TryParse(value, out var b))
            return (T)(object)b;
        if (typeof(T) == typeof(double) && double.TryParse(value, out var d))
            return (T)(object)d;

        return default!;
    }
}
