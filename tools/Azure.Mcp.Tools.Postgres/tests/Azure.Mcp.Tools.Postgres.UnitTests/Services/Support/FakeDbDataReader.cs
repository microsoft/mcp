using System.Collections;
using System.Data.Common;
using System.Globalization;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Services.Support;

/// <summary>
/// In-memory <see cref="DbDataReader"/> for tests supporting heterogeneous column types.
/// </summary>
internal sealed class FakeDbDataReader(object[][] rows,
                                       string[] columnNames,
                                       Type[]? columnTypes = null,
                                       string[]? dataTypeNames = null)
    : DbDataReader
{
    private readonly object[][] _rows = rows;
    private readonly string[] _columnNames = columnNames;
    private readonly Type[] _columnTypes = columnTypes ?? Enumerable.Repeat(typeof(string), columnNames.Length).ToArray();
    private readonly string[] _dataTypeNames = dataTypeNames ??
                                               columnTypes?.Select(t => GetFriendlyTypeName(t)).ToArray() ??
                                               Enumerable.Repeat("text", columnNames.Length).ToArray();

    private int _index = -1;
    private bool _isClosed;

    /// <summary>
    /// Backwards-compatible convenience ctor for all-string data.
    /// </summary>
    public FakeDbDataReader(string[][] stringRows, string[] columnNames)
        : this(stringRows.Select(r => r.Cast<object>().ToArray()).ToArray(),
               columnNames,
               Enumerable.Repeat(typeof(string), columnNames.Length).ToArray(),
               Enumerable.Repeat("text", columnNames.Length).ToArray())
    {
    }

    public override int FieldCount => _columnNames.Length;
    public override bool HasRows => _rows.Length > 0;
    public override bool IsClosed => _isClosed;
    public override int RecordsAffected => 0;
    public override int Depth => 0;

    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public override string GetName(int ordinal) => _columnNames[ordinal];

    public override int GetOrdinal(string name)
    {
        for (int i = 0; i < _columnNames.Length; i++)
        {
            if (string.Equals(_columnNames[i], name, StringComparison.Ordinal))
            {
                return i;
            }
        }
        throw new IndexOutOfRangeException($"Column '{name}' not found.");
    }

    public override string GetDataTypeName(int ordinal) => _dataTypeNames[ordinal];
    public override Type GetFieldType(int ordinal) => _columnTypes[ordinal];

    public override object GetValue(int ordinal)
    {
        EnsurePositioned();
        return _rows[_index][ordinal]!;
    }

    public override int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, FieldCount);
        for (int i = 0; i < count; i++)
            values[i] = GetValue(i)!;
        return count;
    }

    public override bool IsDBNull(int ordinal) => GetValue(ordinal) is null or DBNull;

    // Typed getters with safe conversion fallback
    public override string GetString(int ordinal) => ConvertTo<string>(ordinal);
    public override bool GetBoolean(int ordinal) => ConvertTo<bool>(ordinal);
    public override short GetInt16(int ordinal) => ConvertTo<short>(ordinal);
    public override int GetInt32(int ordinal) => ConvertTo<int>(ordinal);
    public override long GetInt64(int ordinal) => ConvertTo<long>(ordinal);
    public override float GetFloat(int ordinal) => ConvertTo<float>(ordinal);
    public override double GetDouble(int ordinal) => ConvertTo<double>(ordinal);
    public override decimal GetDecimal(int ordinal) => ConvertTo<decimal>(ordinal);
    public override DateTime GetDateTime(int ordinal) => ConvertTo<DateTime>(ordinal);
    public override Guid GetGuid(int ordinal)
    {
        var v = GetValue(ordinal);
        return v switch
        {
            Guid g => g,
            string s when Guid.TryParse(s, out var g2) => g2,
            _ => throw new InvalidCastException(GetInvalidCastMessage(ordinal, typeof(Guid), v))
        };
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException("Binary data not supported in FakeDbDataReader.");

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
        throw new NotSupportedException("Char streaming not supported in FakeDbDataReader.");

    public override char GetChar(int ordinal) =>
        throw new NotSupportedException("GetChar not implemented for FakeDbDataReader.");

    public override byte GetByte(int ordinal) => ConvertTo<byte>(ordinal);

    public override bool Read()
    {
        if (_index + 1 >= _rows.Length)
            return false;
        _index++;
        return true;
    }

    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        return Read();
    }

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);
    public override bool NextResult() => false;

    public override IEnumerator GetEnumerator() => _rows.GetEnumerator();

    public override void Close() => _isClosed = true;
    protected override void Dispose(bool disposing) => _isClosed = true;

#if NET8_0_OR_GREATER
    public override ValueTask DisposeAsync()
    {
        _isClosed = true;
        return ValueTask.CompletedTask;
    }
#endif

    private void EnsurePositioned()
    {
        if (_index < 0 || _index >= _rows.Length)
        {
            throw new InvalidOperationException("The reader is not positioned on a valid row. Call Read() first.");
        }
    }

    private T ConvertTo<T>(int ordinal)
    {
        var v = GetValue(ordinal);
        if (v is null or DBNull)
        {
            throw new InvalidCastException(GetInvalidCastMessage(ordinal, typeof(T), v));
        }

        if (v is T tv)
            return tv;

        try
        {
            // Handle string conversions explicitly for Guid, DateTime etc already handled above where needed.
            if (typeof(T) == typeof(string))
            {
                return (T)(object)v.ToString()!;
            }
            return (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            throw new InvalidCastException(GetInvalidCastMessage(ordinal, typeof(T), v), ex);
        }
    }

    private string GetInvalidCastMessage(int ordinal, Type target, object? value) =>
        $"Cannot convert column '{GetName(ordinal)}' (ordinal {ordinal}, type '{GetFieldType(ordinal).Name}') value '{value ?? "NULL"}' to {target.Name}.";

    private static string GetFriendlyTypeName(Type t) =>
        t == typeof(string) ? "text" :
        t == typeof(int) ? "int4" :
        t == typeof(long) ? "int8" :
        t == typeof(short) ? "int2" :
        t == typeof(bool) ? "bool" :
        t == typeof(decimal) ? "numeric" :
        t == typeof(double) ? "float8" :
        t == typeof(float) ? "float4" :
        t == typeof(DateTime) ? "timestamp" :
        t == typeof(Guid) ? "uuid" :
        t.Name.ToLowerInvariant();
}
