// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// A class for creating option instances.
/// </summary>
internal static class OptionCreators
{
    // String
    internal static Option<string> String(OptionDescriptor descriptor) => Single<string>(descriptor);
    internal static Option<string?> NullableString(OptionDescriptor descriptor) => NullableSingle<string?>(descriptor);
    internal static Option<string[]> StringArray(OptionDescriptor descriptor) => Multi<string[]>(descriptor);
    internal static Option<string[]?> NullableStringArray(OptionDescriptor descriptor) => NullableMulti<string[]?>(descriptor);

    // Boolean
    internal static Option<bool> Bool(OptionDescriptor descriptor) => Single<bool>(descriptor);
    internal static Option<bool?> NullableBool(OptionDescriptor descriptor) => NullableSingle<bool?>(descriptor);
    internal static Option<bool[]> BoolArray(OptionDescriptor descriptor) => Multi<bool[]>(descriptor);
    internal static Option<bool[]?> NullableBoolArray(OptionDescriptor descriptor) => NullableMulti<bool[]?>(descriptor);

    // Int
    internal static Option<int> Int(OptionDescriptor descriptor) => Single<int>(descriptor);
    internal static Option<int?> NullableInt(OptionDescriptor descriptor) => NullableSingle<int?>(descriptor);
    internal static Option<int[]> IntArray(OptionDescriptor descriptor) => Multi<int[]>(descriptor);
    internal static Option<int[]?> NullableIntArray(OptionDescriptor descriptor) => NullableMulti<int[]?>(descriptor);

    // Long
    internal static Option<long> Long(OptionDescriptor descriptor) => Single<long>(descriptor);
    internal static Option<long?> NullableLong(OptionDescriptor descriptor) => NullableSingle<long?>(descriptor);
    internal static Option<long[]> LongArray(OptionDescriptor descriptor) => Multi<long[]>(descriptor);
    internal static Option<long[]?> NullableLongArray(OptionDescriptor descriptor) => NullableMulti<long[]?>(descriptor);

    // Short
    internal static Option<short> Short(OptionDescriptor descriptor) => Single<short>(descriptor);
    internal static Option<short?> NullableShort(OptionDescriptor descriptor) => NullableSingle<short?>(descriptor);
    internal static Option<short[]> ShortArray(OptionDescriptor descriptor) => Multi<short[]>(descriptor);
    internal static Option<short[]?> NullableShortArray(OptionDescriptor descriptor) => NullableMulti<short[]?>(descriptor);

    // Byte
    internal static Option<byte> Byte(OptionDescriptor descriptor) => Single<byte>(descriptor);
    internal static Option<byte?> NullableByte(OptionDescriptor descriptor) => NullableSingle<byte?>(descriptor);
    internal static Option<byte[]> ByteArray(OptionDescriptor descriptor) => Multi<byte[]>(descriptor);
    internal static Option<byte[]?> NullableByteArray(OptionDescriptor descriptor) => NullableMulti<byte[]?>(descriptor);

    // SByte
    internal static Option<sbyte> SByte(OptionDescriptor descriptor) => Single<sbyte>(descriptor);
    internal static Option<sbyte?> NullableSByte(OptionDescriptor descriptor) => NullableSingle<sbyte?>(descriptor);
    internal static Option<sbyte[]> SByteArray(OptionDescriptor descriptor) => Multi<sbyte[]>(descriptor);
    internal static Option<sbyte[]?> NullableSByteArray(OptionDescriptor descriptor) => NullableMulti<sbyte[]?>(descriptor);

    // UShort
    internal static Option<ushort> UShort(OptionDescriptor descriptor) => Single<ushort>(descriptor);
    internal static Option<ushort?> NullableUShort(OptionDescriptor descriptor) => NullableSingle<ushort?>(descriptor);
    internal static Option<ushort[]> UShortArray(OptionDescriptor descriptor) => Multi<ushort[]>(descriptor);
    internal static Option<ushort[]?> NullableUShortArray(OptionDescriptor descriptor) => NullableMulti<ushort[]?>(descriptor);

    // UInt
    internal static Option<uint> UInt(OptionDescriptor descriptor) => Single<uint>(descriptor);
    internal static Option<uint?> NullableUInt(OptionDescriptor descriptor) => NullableSingle<uint?>(descriptor);
    internal static Option<uint[]> UIntArray(OptionDescriptor descriptor) => Multi<uint[]>(descriptor);
    internal static Option<uint[]?> NullableUIntArray(OptionDescriptor descriptor) => NullableMulti<uint[]?>(descriptor);

    // ULong
    internal static Option<ulong> ULong(OptionDescriptor descriptor) => Single<ulong>(descriptor);
    internal static Option<ulong?> NullableULong(OptionDescriptor descriptor) => NullableSingle<ulong?>(descriptor);
    internal static Option<ulong[]> ULongArray(OptionDescriptor descriptor) => Multi<ulong[]>(descriptor);
    internal static Option<ulong[]?> NullableULongArray(OptionDescriptor descriptor) => NullableMulti<ulong[]?>(descriptor);

    // Float
    internal static Option<float> Float(OptionDescriptor descriptor) => Single<float>(descriptor);
    internal static Option<float?> NullableFloat(OptionDescriptor descriptor) => NullableSingle<float?>(descriptor);
    internal static Option<float[]> FloatArray(OptionDescriptor descriptor) => Multi<float[]>(descriptor);
    internal static Option<float[]?> NullableFloatArray(OptionDescriptor descriptor) => NullableMulti<float[]?>(descriptor);

    // Double
    internal static Option<double> Double(OptionDescriptor descriptor) => Single<double>(descriptor);
    internal static Option<double?> NullableDouble(OptionDescriptor descriptor) => NullableSingle<double?>(descriptor);
    internal static Option<double[]> DoubleArray(OptionDescriptor descriptor) => Multi<double[]>(descriptor);
    internal static Option<double[]?> NullableDoubleArray(OptionDescriptor descriptor) => NullableMulti<double[]?>(descriptor);

    // Decimal
    internal static Option<decimal> Decimal(OptionDescriptor descriptor) => Single<decimal>(descriptor);
    internal static Option<decimal?> NullableDecimal(OptionDescriptor descriptor) => NullableSingle<decimal?>(descriptor);
    internal static Option<decimal[]> DecimalArray(OptionDescriptor descriptor) => Multi<decimal[]>(descriptor);
    internal static Option<decimal[]?> NullableDecimalArray(OptionDescriptor descriptor) => NullableMulti<decimal[]?>(descriptor);

    // Char
    internal static Option<char> Char(OptionDescriptor descriptor) => Single<char>(descriptor);
    internal static Option<char?> NullableChar(OptionDescriptor descriptor) => NullableSingle<char?>(descriptor);
    internal static Option<char[]> CharArray(OptionDescriptor descriptor) => Multi<char[]>(descriptor);
    internal static Option<char[]?> NullableCharArray(OptionDescriptor descriptor) => NullableMulti<char[]?>(descriptor);

    // DateTime
    internal static Option<DateTime> DateTime(OptionDescriptor descriptor) => Single<DateTime>(descriptor);
    internal static Option<DateTime?> NullableDateTime(OptionDescriptor descriptor) => NullableSingle<DateTime?>(descriptor);
    internal static Option<DateTime[]> DateTimeArray(OptionDescriptor descriptor) => Multi<DateTime[]>(descriptor);
    internal static Option<DateTime[]?> NullableDateTimeArray(OptionDescriptor descriptor) => NullableMulti<DateTime[]?>(descriptor);

    // DateTimeOffset
    internal static Option<DateTimeOffset> DateTimeOffset(OptionDescriptor descriptor) => Single<DateTimeOffset>(descriptor);
    internal static Option<DateTimeOffset?> NullableDateTimeOffset(OptionDescriptor descriptor) => NullableSingle<DateTimeOffset?>(descriptor);
    internal static Option<DateTimeOffset[]> DateTimeOffsetArray(OptionDescriptor descriptor) => Multi<DateTimeOffset[]>(descriptor);
    internal static Option<DateTimeOffset[]?> NullableDateTimeOffsetArray(OptionDescriptor descriptor) => NullableMulti<DateTimeOffset[]?>(descriptor);

    // TimeSpan
    internal static Option<TimeSpan> TimeSpan(OptionDescriptor descriptor) => Single<TimeSpan>(descriptor);
    internal static Option<TimeSpan?> NullableTimeSpan(OptionDescriptor descriptor) => NullableSingle<TimeSpan?>(descriptor);
    internal static Option<TimeSpan[]> TimeSpanArray(OptionDescriptor descriptor) => Multi<TimeSpan[]>(descriptor);
    internal static Option<TimeSpan[]?> NullableTimeSpanArray(OptionDescriptor descriptor) => NullableMulti<TimeSpan[]?>(descriptor);

    // Guid
    internal static Option<Guid> Guid(OptionDescriptor descriptor) => Single<Guid>(descriptor);
    internal static Option<Guid?> NullableGuid(OptionDescriptor descriptor) => NullableSingle<Guid?>(descriptor);
    internal static Option<Guid[]> GuidArray(OptionDescriptor descriptor) => Multi<Guid[]>(descriptor);
    internal static Option<Guid[]?> NullableGuidArray(OptionDescriptor descriptor) => NullableMulti<Guid[]?>(descriptor);

    // Helper methods
    private static Option<T> Single<T>(OptionDescriptor descriptor) => CreateOption<T>(descriptor, true, false, ArgumentArity.ExactlyOne);
    private static Option<T> NullableSingle<T>(OptionDescriptor descriptor) => CreateOption<T>(descriptor, false, false, ArgumentArity.ZeroOrOne);
    private static Option<T> Multi<T>(OptionDescriptor descriptor) => CreateOption<T>(descriptor, true, true, ArgumentArity.OneOrMore);
    private static Option<T> NullableMulti<T>(OptionDescriptor descriptor) => CreateOption<T>(descriptor, false, true, ArgumentArity.ZeroOrMore);

    /// <summary>
    /// Creates a new option of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the option.</typeparam>
    /// <param name="name">The name of the option.</param>
    /// <param name="aliases">The aliases of the option.</param>
    /// <param name="description">The description of the option.</param>
    /// <param name="required">Whether the option is required.</param>
    /// <param name="allowMultipleArgumentsPerToken">Whether the option allows multiple arguments per token.</param>
    /// <param name="arity">The arity of the option.</param>
    /// <returns>The created option.</returns>
    private static Option<T> CreateOption<T>(
        OptionDescriptor descriptor,
        bool required,
        bool allowMultipleArgumentsPerToken,
        ArgumentArity arity)
    {
        return new Option<T>($"--{descriptor.Name}", descriptor.Aliases.Select(alias => $"--{alias}").ToArray())
        {
            Description = descriptor.Description,
            Required = required,
            AllowMultipleArgumentsPerToken = allowMultipleArgumentsPerToken,
            Arity = arity,
            Hidden = descriptor.Hidden
        };
    }
}
