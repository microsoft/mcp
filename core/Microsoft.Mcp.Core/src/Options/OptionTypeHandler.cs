// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Reflection;
using Microsoft.Mcp.Core.Extensions;

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Handles creating and binding options.
/// </summary>
public sealed class OptionTypeHandler
{
    public OptionDescriptor Descriptor { get; }
    public Option Option { get; }
    public Func<ParseResult, object?> Binder { get; }

    public OptionTypeHandler(OptionDescriptor descriptor)
    {
        Descriptor = descriptor;
        var optionAndBinder = CreateFromDescriptor(descriptor);
        Option = optionAndBinder.Item1;
        Binder = optionAndBinder.Item2;
    }

    internal static (Option, Func<ParseResult, object?>) CreateFromDescriptor(OptionDescriptor descriptor)
    {
        var type = descriptor.Type;

        // Check for nullable types, unwrap them and track that they're optional.
        var isNullable = false;
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
        {
            type = underlyingType;
            isNullable = true;
        }
        else
        {
            var nullabilityInfo = new NullabilityInfoContext().Create(descriptor.TargetProperty);
            if (nullabilityInfo.WriteState == NullabilityState.Nullable)
            {
                type = nullabilityInfo.Type;
                isNullable = true;
            }
        }

        // Check for enumerable types, unwrap them and track that they're multi-valued.
        var isMulti = false;
        if (type.IsArray)
        {
            type = type.GetElementType()!;
            isMulti = true;
        }
        else if (type != typeof(string) && type.IsAssignableTo(typeof(IEnumerable<>)))
        {
            type = type.GetGenericArguments()[0];
            isMulti = true;
        }

        var optionAndBinder = CreateOptionAndBinder(type, isNullable, isMulti, descriptor);
        if (optionAndBinder is null)
        {
            throw new InvalidOperationException($"Unsupported option type '{descriptor.Type}'. Add handling to ${nameof(OptionTypeHandler)} or use a supported type and handle with PostBindOptions.");
        }

        var option = optionAndBinder.Value.Item1;
        option.Description = descriptor.Description;
        option.Required = descriptor.Required;
        // For array/collection types, allow multiple values after a single option token
        // e.g., --modules RedisBloom RedisJSON instead of --modules RedisBloom --modules RedisJSON
        option.AllowMultipleArgumentsPerToken = isMulti;
        option.Arity = GetArgumentArity(type, isNullable, isMulti);
        option.Hidden = descriptor.Hidden;

        if (type.IsEnum)
        {
            // Handle enum options as Option<string> with constrained values.
            var names = Enum.GetNames(type);
            var allowed = string.Join(", ", names);
            var namesSet = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            option.Validators.Add(result =>
            {
                foreach (Token token in result.Tokens)
                {
                    if (!namesSet.Contains(token.Value))
                    {
                        result.AddError($"Argument '{token.Value}' not recognized. Must be one of: {allowed}");
                    }
                }
            });

            option.CompletionSources.Add(names);
            Func<ParseResult, object?> enumBinder = parseResult =>
            {
                var result = optionAndBinder.Value.Item2.Invoke(parseResult);
                if (result is string s)
                {
                    return Enum.Parse(type, s, ignoreCase: true);
                }
                if (result is IEnumerable<string> strings)
                {
                    var parsed = strings.Select(s => Enum.Parse(type, s, ignoreCase: true)).ToArray();
#pragma warning disable IL3050 // Enum array creation shouldn't be an AoT issue here.
                    var array = Array.CreateInstance(type, parsed.Length);
#pragma warning restore IL3050
                    Array.Copy(parsed, array, parsed.Length);
                    return array;
                }
                return result;
            };
            return (option, enumBinder);
        }

        return optionAndBinder.Value;
    }

    private static (Option, Func<ParseResult, object?>)? CreateOptionAndBinder(
        Type type,
        bool isNullable,
        bool isMulti,
        OptionDescriptor descriptor)
    {
        // Enums are handled as a string with additional binding.
        if (typeof(string) == type || type.IsEnum)
        {
            Action<OptionResult>? validateEmptyOrWhiteSpace = descriptor.AllowEmptyOrWhiteSpaceString ? null : ValidateEmptyOrWhiteSpace;
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<string[]?>(descriptor, validateEmptyOrWhiteSpace);
            if (isMulti)
                return CreateOptionAndBinderHelper<string[]>(descriptor, validateEmptyOrWhiteSpace);
            if (isNullable)
                return CreateOptionAndBinderHelper<string?>(descriptor, validateEmptyOrWhiteSpace);
            return CreateOptionAndBinderHelper<string>(descriptor, validateEmptyOrWhiteSpace);
        }
        if (typeof(bool) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<bool[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<bool[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<bool?>(descriptor);
            return CreateOptionAndBinderHelper<bool>(descriptor);
        }
        if (typeof(int) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<int[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<int[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<int?>(descriptor);
            return CreateOptionAndBinderHelper<int>(descriptor);
        }
        if (typeof(long) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<long[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<long[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<long?>(descriptor);
            return CreateOptionAndBinderHelper<long>(descriptor);
        }
        if (typeof(short) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<short[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<short[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<short?>(descriptor);
            return CreateOptionAndBinderHelper<short>(descriptor);
        }
        if (typeof(byte) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<byte[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<byte[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<byte?>(descriptor);
            return CreateOptionAndBinderHelper<byte>(descriptor);
        }
        if (typeof(sbyte) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<sbyte[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<sbyte[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<sbyte?>(descriptor);
            return CreateOptionAndBinderHelper<sbyte>(descriptor);
        }
        if (typeof(ushort) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<ushort[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<ushort[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<ushort?>(descriptor);
            return CreateOptionAndBinderHelper<ushort>(descriptor);
        }
        if (typeof(uint) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<uint[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<uint[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<uint?>(descriptor);
            return CreateOptionAndBinderHelper<uint>(descriptor);
        }
        if (typeof(ulong) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<ulong[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<ulong[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<ulong?>(descriptor);
            return CreateOptionAndBinderHelper<ulong>(descriptor);
        }
        if (typeof(float) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<float[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<float[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<float?>(descriptor);
            return CreateOptionAndBinderHelper<float>(descriptor);
        }
        if (typeof(double) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<double[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<double[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<double?>(descriptor);
            return CreateOptionAndBinderHelper<double>(descriptor);
        }
        if (typeof(decimal) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<decimal[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<decimal[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<decimal?>(descriptor);
            return CreateOptionAndBinderHelper<decimal>(descriptor);
        }
        if (typeof(char) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<char[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<char[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<char?>(descriptor);
            return CreateOptionAndBinderHelper<char>(descriptor);
        }
        if (typeof(DateTime) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<DateTime[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<DateTime[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<DateTime?>(descriptor);
            return CreateOptionAndBinderHelper<DateTime>(descriptor);
        }
        if (typeof(DateTimeOffset) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<DateTimeOffset[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<DateTimeOffset[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<DateTimeOffset?>(descriptor);
            return CreateOptionAndBinderHelper<DateTimeOffset>(descriptor);
        }
        if (typeof(TimeSpan) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<TimeSpan[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<TimeSpan[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<TimeSpan?>(descriptor);
            return CreateOptionAndBinderHelper<TimeSpan>(descriptor);
        }
        if (typeof(Guid) == type)
        {
            if (isNullable && isMulti)
                return CreateOptionAndBinderHelper<Guid[]?>(descriptor);
            if (isMulti)
                return CreateOptionAndBinderHelper<Guid[]>(descriptor);
            if (isNullable)
                return CreateOptionAndBinderHelper<Guid?>(descriptor);
            return CreateOptionAndBinderHelper<Guid>(descriptor);
        }
        return null;
    }

    private static (Option, Func<ParseResult, object?>) CreateOptionAndBinderHelper<T>(
        OptionDescriptor descriptor,
        Action<OptionResult>? emptyOrWhiteSpaceValidator = null)
    {
        var option = new Option<T>($"--{descriptor.Name}", [.. descriptor.Aliases.Select(a => $"--{a}")]);
        if (descriptor.DefaultValue != null)
        {
            option.DefaultValueFactory = _ => (T)descriptor.DefaultValue;
        }
        if (emptyOrWhiteSpaceValidator != null)
        {
            option.Validators.Add(emptyOrWhiteSpaceValidator);
        }
        return (option, parseResult => parseResult.GetValueOrDefaultWithoutName(option));
    }

    private static ArgumentArity GetArgumentArity(Type type, bool isNullable, bool isMulti)
    {
        if (isMulti && isNullable)
            return ArgumentArity.ZeroOrMore;
        if (isMulti)
            return ArgumentArity.OneOrMore;
        if (isNullable)
            return ArgumentArity.ZeroOrOne;
        return typeof(bool) == type ? ArgumentArity.ZeroOrOne : ArgumentArity.ExactlyOne;
    }

    private static void ValidateEmptyOrWhiteSpace(OptionResult result)
    {
        // Calling on an Option that isn't required, has a default, or doesn't allow values to be passed skips checking.
        // This matches previous behavior.
        var option = result.Option;
        if (!option.Required || option.HasDefaultValue || option.Arity.MaximumNumberOfValues == 0)
        {
            return;
        }

        if (result.Tokens is not { Count: > 0 } || result.Tokens.Any(t => string.IsNullOrWhiteSpace(t.Value)))
        {
            result.AddError("Option was configured to require non-empty, non-whitespace values but one or more empty or whitespace values were provided.");
        }
    }
}
