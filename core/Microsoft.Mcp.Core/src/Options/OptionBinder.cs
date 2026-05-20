// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
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
    private static readonly MethodInfo GetValueOrDefaultMethod =
        typeof(ParseResultExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(ParseResultExtensions.GetValueOrDefault)
                && m.IsGenericMethodDefinition
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType == typeof(string));

    /// <summary>
    /// Registers System.CommandLine options on a command based on the public properties of <typeparamref name="TOptions"/>.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2055:MakeGenericType",
        Justification = "Option<T> is used with property types rooted by the application.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Option<T> generic instantiation uses types rooted by the application.")]
    public static void RegisterOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions>(Command command)
        where TOptions : class
    {
        var descriptors = OptionDescriptor.FromType<TOptions>();
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
    [UnconditionalSuppressMessage("Trimming", "IL2060:MakeGenericMethod",
        Justification = "GetValueOrDefault<T> is called with property types rooted by the application.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Generic method instantiation uses types rooted by the application.")]
    public static TOptions BindOptions<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TOptions>(ParseResult parseResult)
        where TOptions : class
    {
        var instance = (TOptions)CreateInstance(typeof(TOptions));
        var descriptors = OptionDescriptor.FromType<TOptions>();
        List<string> missingOptions = [];
        List<string> errors = [];
        Dictionary<PropertyInfo, object>? parentInstances = null;

        foreach (var descriptor in descriptors)
        {
            var optionName = $"--{descriptor.Name}";
            var method = GetValueOrDefaultMethod.MakeGenericMethod(descriptor.Type);

            object? value;
            try
            {
                value = method.Invoke(null, [parseResult, optionName]);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException or FormatException or OverflowException)
            {
                errors.Add($"Invalid value for '{optionName}': {ex.InnerException.Message}");
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

    [UnconditionalSuppressMessage("Trimming", "IL2055:MakeGenericType",
        Justification = "Option<T> is used with property types rooted by the application.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "Option<T> generic instantiation uses types rooted by the application.")]
    private static Option CreateOption(OptionDescriptor descriptor)
    {
        var optionType = typeof(Option<>).MakeGenericType(descriptor.Type);
        var option = (Option)Activator.CreateInstance(optionType, $"--{descriptor.Name}")!;
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
