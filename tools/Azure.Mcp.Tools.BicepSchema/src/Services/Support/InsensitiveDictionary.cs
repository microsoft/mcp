// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.BicepSchema.Services.Support;

public class InsensitiveDictionary<TValue> : Dictionary<string, TValue>
{
    public static readonly InsensitiveDictionary<TValue> Empty = [];

    public InsensitiveDictionary()
        : base((IEqualityComparer<string>?)StringComparer.InvariantCultureIgnoreCase)
    {
    }

    public InsensitiveDictionary(int capacity)
        : base(capacity, (IEqualityComparer<string>?)StringComparer.InvariantCultureIgnoreCase)
    {
    }

    public InsensitiveDictionary(IDictionary<string, TValue> dictionary)
        : base(dictionary, (IEqualityComparer<string>?)StringComparer.InvariantCultureIgnoreCase)
    {
    }
}
