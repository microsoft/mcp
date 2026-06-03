// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
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
    private const DynamicallyAccessedMemberTypes OptionBindingMembers =
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

    /// <summary>
    /// To prevent native AOT builds from trimming away the filled generic Options<T> methods, we have to maintain a
    /// centralized factory pattern. Each entry provides both the option factory (for registration) and value binder
    /// (for parsing). If a type is not in this dictionary and is not an enum, it is unsupported and will be rejected
    /// at option registration time.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, OptionTypeHandler> s_typeHandlers = new()
    {
        // String
        [typeof(string)] = new(name => new Option<string>(name), (pr, n) => pr.GetValueOrDefault<string>(n)),
        [typeof(string[])] = new(name => new Option<string[]>(name), (pr, n) => pr.GetValueOrDefault<string[]>(n)),

        // Boolean
        [typeof(bool)] = new(name => new Option<bool>(name), (pr, n) => pr.GetValueOrDefault<bool>(n)),
        [typeof(bool?)] = new(name => new Option<bool?>(name), (pr, n) => pr.GetValueOrDefault<bool?>(n)),
        [typeof(bool[])] = new(name => new Option<bool[]>(name), (pr, n) => pr.GetValueOrDefault<bool[]>(n)),

        // Int
        [typeof(int)] = new(name => new Option<int>(name), (pr, n) => pr.GetValueOrDefault<int>(n)),
        [typeof(int?)] = new(name => new Option<int?>(name), (pr, n) => pr.GetValueOrDefault<int?>(n)),
        [typeof(int[])] = new(name => new Option<int[]>(name), (pr, n) => pr.GetValueOrDefault<int[]>(n)),

        // Long
        [typeof(long)] = new(name => new Option<long>(name), (pr, n) => pr.GetValueOrDefault<long>(n)),
        [typeof(long?)] = new(name => new Option<long?>(name), (pr, n) => pr.GetValueOrDefault<long?>(n)),
        [typeof(long[])] = new(name => new Option<long[]>(name), (pr, n) => pr.GetValueOrDefault<long[]>(n)),

        // Short
        [typeof(short)] = new(name => new Option<short>(name), (pr, n) => pr.GetValueOrDefault<short>(n)),
        [typeof(short?)] = new(name => new Option<short?>(name), (pr, n) => pr.GetValueOrDefault<short?>(n)),
        [typeof(short[])] = new(name => new Option<short[]>(name), (pr, n) => pr.GetValueOrDefault<short[]>(n)),

        // Byte
        [typeof(byte)] = new(name => new Option<byte>(name), (pr, n) => pr.GetValueOrDefault<byte>(n)),
        [typeof(byte?)] = new(name => new Option<byte?>(name), (pr, n) => pr.GetValueOrDefault<byte?>(n)),
        [typeof(byte[])] = new(name => new Option<byte[]>(name), (pr, n) => pr.GetValueOrDefault<byte[]>(n)),

        // SByte
        [typeof(sbyte)] = new(name => new Option<sbyte>(name), (pr, n) => pr.GetValueOrDefault<sbyte>(n)),
        [typeof(sbyte?)] = new(name => new Option<sbyte?>(name), (pr, n) => pr.GetValueOrDefault<sbyte?>(n)),
        [typeof(sbyte[])] = new(name => new Option<sbyte[]>(name), (pr, n) => pr.GetValueOrDefault<sbyte[]>(n)),

        // UShort
        [typeof(ushort)] = new(name => new Option<ushort>(name), (pr, n) => pr.GetValueOrDefault<ushort>(n)),
        [typeof(ushort?)] = new(name => new Option<ushort?>(name), (pr, n) => pr.GetValueOrDefault<ushort?>(n)),
        [typeof(ushort[])] = new(name => new Option<ushort[]>(name), (pr, n) => pr.GetValueOrDefault<ushort[]>(n)),

        // UInt
        [typeof(uint)] = new(name => new Option<uint>(name), (pr, n) => pr.GetValueOrDefault<uint>(n)),
        [typeof(uint?)] = new(name => new Option<uint?>(name), (pr, n) => pr.GetValueOrDefault<uint?>(n)),
        [typeof(uint[])] = new(name => new Option<uint[]>(name), (pr, n) => pr.GetValueOrDefault<uint[]>(n)),

        // ULong
        [typeof(ulong)] = new(name => new Option<ulong>(name), (pr, n) => pr.GetValueOrDefault<ulong>(n)),
        [typeof(ulong?)] = new(name => new Option<ulong?>(name), (pr, n) => pr.GetValueOrDefault<ulong?>(n)),
        [typeof(ulong[])] = new(name => new Option<ulong[]>(name), (pr, n) => pr.GetValueOrDefault<ulong[]>(n)),

        // Float
        [typeof(float)] = new(name => new Option<float>(name), (pr, n) => pr.GetValueOrDefault<float>(n)),
        [typeof(float?)] = new(name => new Option<float?>(name), (pr, n) => pr.GetValueOrDefault<float?>(n)),
        [typeof(float[])] = new(name => new Option<float[]>(name), (pr, n) => pr.GetValueOrDefault<float[]>(n)),

        // Double
        [typeof(double)] = new(name => new Option<double>(name), (pr, n) => pr.GetValueOrDefault<double>(n)),
        [typeof(double?)] = new(name => new Option<double?>(name), (pr, n) => pr.GetValueOrDefault<double?>(n)),
        [typeof(double[])] = new(name => new Option<double[]>(name), (pr, n) => pr.GetValueOrDefault<double[]>(n)),

        // Decimal
        [typeof(decimal)] = new(name => new Option<decimal>(name), (pr, n) => pr.GetValueOrDefault<decimal>(n)),
        [typeof(decimal?)] = new(name => new Option<decimal?>(name), (pr, n) => pr.GetValueOrDefault<decimal?>(n)),
        [typeof(decimal[])] = new(name => new Option<decimal[]>(name), (pr, n) => pr.GetValueOrDefault<decimal[]>(n)),

        // Char
        [typeof(char)] = new(name => new Option<char>(name), (pr, n) => pr.GetValueOrDefault<char>(n)),
        [typeof(char?)] = new(name => new Option<char?>(name), (pr, n) => pr.GetValueOrDefault<char?>(n)),
        [typeof(char[])] = new(name => new Option<char[]>(name), (pr, n) => pr.GetValueOrDefault<char[]>(n)),

        // DateTime
        [typeof(DateTime)] = new(name => new Option<DateTime>(name), (pr, n) => pr.GetValueOrDefault<DateTime>(n)),
        [typeof(DateTime?)] = new(name => new Option<DateTime?>(name), (pr, n) => pr.GetValueOrDefault<DateTime?>(n)),
        [typeof(DateTime[])] = new(name => new Option<DateTime[]>(name), (pr, n) => pr.GetValueOrDefault<DateTime[]>(n)),

        // DateTimeOffset
        [typeof(DateTimeOffset)] = new(name => new Option<DateTimeOffset>(name), (pr, n) => pr.GetValueOrDefault<DateTimeOffset>(n)),
        [typeof(DateTimeOffset?)] = new(name => new Option<DateTimeOffset?>(name), (pr, n) => pr.GetValueOrDefault<DateTimeOffset?>(n)),
        [typeof(DateTimeOffset[])] = new(name => new Option<DateTimeOffset[]>(name), (pr, n) => pr.GetValueOrDefault<DateTimeOffset[]>(n)),

        // TimeSpan
        [typeof(TimeSpan)] = new(name => new Option<TimeSpan>(name), (pr, n) => pr.GetValueOrDefault<TimeSpan>(n)),
        [typeof(TimeSpan?)] = new(name => new Option<TimeSpan?>(name), (pr, n) => pr.GetValueOrDefault<TimeSpan?>(n)),
        [typeof(TimeSpan[])] = new(name => new Option<TimeSpan[]>(name), (pr, n) => pr.GetValueOrDefault<TimeSpan[]>(n)),

        // Guid
        [typeof(Guid)] = new(name => new Option<Guid>(name), (pr, n) => pr.GetValueOrDefault<Guid>(n)),
        [typeof(Guid?)] = new(name => new Option<Guid?>(name), (pr, n) => pr.GetValueOrDefault<Guid?>(n)),
        [typeof(Guid[])] = new(name => new Option<Guid[]>(name), (pr, n) => pr.GetValueOrDefault<Guid[]>(n)),
    };

    /// <summary>
    /// Registers System.CommandLine options on a command based on the public properties of <typeparamref name="TOptions"/>.
    /// </summary>
    public static void RegisterOptions<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>(Command command)
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
    public static TOptions BindOptions<[DynamicallyAccessedMembers(OptionBindingMembers)] TOptions>(ParseResult parseResult)
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
        var name = $"--{descriptor.Name}";
        var handler = GetHandler(descriptor.Type);
        var option = handler.CreateOption(name);
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

    private static object? GetOptionValue(ParseResult parseResult, Type type, string optionName)
    {
        var handler = GetHandler(type);
        return handler.GetValue(parseResult, optionName);
    }

    private static OptionTypeHandler GetHandler(Type type)
    {
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
        Func<string, Option> createOption,
        Func<ParseResult, string, object?> getValue)
    {
        public Option CreateOption(string name) => createOption(name);
        public object? GetValue(ParseResult parseResult, string optionName) => getValue(parseResult, optionName);
    }
}
