// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using Microsoft.Mcp.Core.Commands;

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Provides static methods for registering System.CommandLine options from TOptions POCOs
/// and binding ParseResult values back to TOptions instances.
/// </summary>
public static class OptionBinder
{
    private const DynamicallyAccessedMemberTypes OptionBindingMembers =
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

    private static readonly ConcurrentDictionary<Type, OptionTypeHandler[]> s_optionTypeHandlers = new();

    /// <summary>
    /// Registers System.CommandLine options on a command based on the public properties of <typeparamref name="TOptions"/>.
    /// </summary>
    /// <param name="command">The command to register options on.</param>
    public static void RegisterOptions<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>(Command command)
        where TOptions : class
    {
        var handlers = s_optionTypeHandlers.GetOrAdd(typeof(TOptions), _ => GetOptionTypeHandlers<TOptions>());
        foreach (var handler in handlers)
        {
            command.Options.Add(handler.Option);
        }
    }

    private static OptionTypeHandler[] GetOptionTypeHandlers<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>() where TOptions : class
    {
        var descriptors = OptionDescriptor.FromType<TOptions>();
        return [.. descriptors.Select(d => new OptionTypeHandler(d))];
    }

    /// <summary>
    /// Creates a new <typeparamref name="TOptions"/> instance and populates its properties
    /// from the parsed command-line values.
    /// </summary>
    /// <param name="parseResult">The parsed command-line values.</param>
    public static TOptions BindOptions<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>(ParseResult parseResult)
        where TOptions : class
    {
        var instance = (TOptions)CreateInstance(typeof(TOptions));
        List<string> missingOptions = [];
        List<string> errors = [.. parseResult.Errors.Select(e => e.Message)];
        Dictionary<PropertyInfo, object>? parentInstances = null;
        var handlers = s_optionTypeHandlers.GetOrAdd(typeof(TOptions), _ => GetOptionTypeHandlers<TOptions>());

        foreach (var handler in handlers)
        {
            object? value;
            try
            {
                value = handler.Binder.Invoke(parseResult);
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException or OverflowException or ArgumentException)
            {
                errors.Add($"Invalid value for '{handler.Option.Name}': {ex.Message}");
                continue;
            }

            if (value is null)
            {
                if (handler.Option.Required)
                {
                    missingOptions.Add(handler.Option.Name);
                }
                continue;
            }

            if (handler.Descriptor.ParentProperty is not null)
            {
                parentInstances ??= [];
                if (!parentInstances.TryGetValue(handler.Descriptor.ParentProperty, out var parent))
                {
                    parent = CreateInstance(handler.Descriptor.ParentProperty.PropertyType);
                    parentInstances[handler.Descriptor.ParentProperty] = parent;
                }
                handler.Descriptor.TargetProperty.SetValue(parent, value);
            }
            else
            {
                handler.Descriptor.TargetProperty.SetValue(instance, value);
            }
        }

        // Set any nested parent objects that had at least one child value provided
        if (parentInstances is not null)
        {
            foreach (var (parentProp, parentObj) in parentInstances)
            {
                parentProp.SetValue(instance, parentObj);
            }
        }

        if (missingOptions.Count > 0 || errors.Count > 0)
        {
            var messages = new List<string>();
            if (missingOptions.Count > 0)
            {
                messages.Add($"Missing Required options: {string.Join(", ", missingOptions)}");
            }
            if (errors.Count > 0)
            {
                messages.AddRange(errors);
            }

            throw new CommandValidationException(
                string.Join('\n', messages),
                HttpStatusCode.BadRequest,
                missingOptions: missingOptions);
        }

        return instance;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067:UnrecognizedReflectionPattern",
        Justification = "Nested option types are rooted by the application via property references.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Nested option types use parameterless constructors rooted by the application.")]
    private static object CreateInstance(Type type)
    {
        return Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Failed to create instance of nested options type '{type.Name}'. Ensure it has a public parameterless constructor.");
    }
}
