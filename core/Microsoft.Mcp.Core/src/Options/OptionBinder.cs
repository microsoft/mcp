// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

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

    /// <summary>
    /// Registers System.CommandLine options on a command based on the public properties of <typeparamref name="TOptions"/>.
    /// </summary>
    /// <param name="command">The command to register options on.</param>
    /// <param name="descriptors">The option descriptors to register.</param>
    public static void RegisterOptions<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>(Command command, OptionDescriptor[] descriptors)
        where TOptions : class
    {
        foreach (var descriptor in descriptors)
        {
            var option = CreateOption(descriptor);
            command.Options.Add(option);
        }
    }

    /// <summary>
    /// Creates a new <typeparamref name="TOptions"/> instance and populates its properties
    /// from the parsed command-line values.
    /// </summary>
    /// <param name="parseResult">The parsed command-line values.</param>
    /// <param name="descriptors">The option descriptors to bind.</param>
    public static TOptions BindOptions<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>(ParseResult parseResult, OptionDescriptor[] descriptors)
        where TOptions : class
    {
        var instance = (TOptions)CreateInstance(typeof(TOptions));
        List<string> missingOptions = [];
        List<string> errors = [];
        Dictionary<PropertyInfo, object>? parentInstances = null;

        foreach (var descriptor in descriptors)
        {
            var optionName = $"--{descriptor.Name}";

            object? value;
            try
            {
                value = GetOptionValue(parseResult, descriptor.Type, optionName);
            }
            catch (Exception ex) when (ex is InvalidOperationException or FormatException or OverflowException or ArgumentException)
            {
                errors.Add($"Invalid value for '{optionName}': {ex.Message}");
                continue;
            }

            if (value is null)
            {
                if (descriptor.Required)
                {
                    missingOptions.Add(optionName);
                }
                continue;
            }

            if (descriptor.ParentProperty is not null)
            {
                parentInstances ??= [];
                if (!parentInstances.TryGetValue(descriptor.ParentProperty, out var parent))
                {
                    parent = CreateInstance(descriptor.ParentProperty.PropertyType);
                    parentInstances[descriptor.ParentProperty] = parent;
                }
                descriptor.TargetProperty.SetValue(parent, value);
            }
            else
            {
                descriptor.TargetProperty.SetValue(instance, value);
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

    private static Option CreateOption(OptionDescriptor descriptor)
    {
        var handler = GetHandler(descriptor);
        var option = handler.CreateOption(descriptor);
        option.Description = descriptor.Description;
        option.Required = descriptor.Required;
        option.Hidden = descriptor.Hidden;

        // For array/collection types, allow multiple values after a single option token
        // e.g., --modules RedisBloom RedisJSON instead of --modules RedisBloom --modules RedisJSON
        if (descriptor.Type.IsArray || (descriptor.Type != typeof(string) && descriptor.Type.IsAssignableTo(typeof(System.Collections.IEnumerable))))
        {
            option.Arity = ArgumentArity.OneOrMore;
            option.AllowMultipleArgumentsPerToken = true;
        }

        return option;
    }

    private static object? GetOptionValue(ParseResult parseResult, OptionDescriptor descriptor)
    {
        var handler = GetHandler<T>(descriptor);
        return handler.GetValue<T>(parseResult);
    }

    private static OptionTypeHandler<T> GetHandler<T>(OptionDescriptor descriptor)
    {
        if (descriptor.Type == typeof(int))
        {
            return new OptionTypeHandler<T>(descriptor, d => OptionCreators.Int(d));
        }
        if (s_typeHandlers.TryGetValue(type, out var handler))
        {
            return handler;
        }

        // Enums (and Nullable<enum>): represented as Option<string> with constrained values
        Type? underlyingEnum = GetUnderlyingEnumType(type);
        if (underlyingEnum is not null)
        {
            return s_typeHandlers.GetOrAdd(type, _ => new OptionTypeHandler(
                name =>
                {
                    var option = new Option<string>(name);
                    EnumOptionValidator.Configure(option, underlyingEnum);
                    return option;
                },
                (pr, n) =>
                {
                    var stringValue = pr.GetValueOrDefault<string>(n);

                    if (stringValue is null)
                    {
                        return null;
                    }

                    return Enum.Parse(underlyingEnum, stringValue, ignoreCase: true);
                }));
        }

        throw new InvalidOperationException(
            $"Unsupported option type '{type}'. Add a handler to s_typeHandlers in OptionBinder, or override RegisterOptions/BindOptions in the command.");
    }

    private static Type? GetUnderlyingEnumType(Type type)
    {
        if (type.IsEnum)
        {
            return type;
        }

        Type? nullable = Nullable.GetUnderlyingType(type);

        if (nullable is not null && nullable.IsEnum)
        {
            return nullable;
        }

        return null;
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

    private sealed class OptionTypeHandler(
        OptionDescriptor descriptor,
        Func<OptionDescriptor, Option> createOption)
    {
        private readonly Lazy<Option> _option = new(() => createOption(descriptor));
        public Option CreateOption(OptionDescriptor descriptor) => _option.Value;
        public object? GetValue(ParseResult parseResult) => parseResult.GetValueOrDefault(_option.Value);
    }
}
