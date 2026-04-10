// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Concurrent;

namespace Microsoft.Mcp.Core.Models;

/// <summary>
/// Simple concurrent hash set implementation using ConcurrentDictionary.
/// This is not a full implementation of ISet and only supports a subset of operations needed for our use case.
/// </summary>
/// <typeparam name="T">The key type.</typeparam>
public sealed class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dictionary = new();

    /// <summary>
    /// Gets the number of items contained in the set.
    /// </summary>
    public int Count => _dictionary.Count;

    /// <summary>
    /// Adds an item to the current set and returns a value to indicate if the item was successfully added.
    /// </summary>
    /// <param name="item">The item to add to the set.</param>
    /// <returns>true if the item is added to the set; false if the item is already in the set.</returns>
    public bool Add(T item) => _dictionary.TryAdd(item, 0);

    /// <summary>
    /// Determines whether the set contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the set.</param>
    /// <returns>true if the item is found in the set; otherwise, false.</returns>
    public bool Contains(T item) => _dictionary.ContainsKey(item);

    /// <summary>
    /// Removes all items from the set.
    /// </summary>
    public void Clear() => _dictionary.Clear();

    /// <summary>
    /// Removes the specified item from the set.
    /// </summary>
    /// <param name="item">The item to remove from the set.</param>
    /// <returns>true if the item was successfully removed from the set; otherwise, false. false is also returned if the item is not in the set.</returns>
    public bool Remove(T item) => _dictionary.TryRemove(item, out _);

    /// <summary>
    /// Returns an enumerator that iterates through the set.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the set.</returns>
    public IEnumerator<T> GetEnumerator() => _dictionary.Keys.GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the set.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the set.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
