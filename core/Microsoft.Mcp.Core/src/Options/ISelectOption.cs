// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Opts a command into selecting top-level response fields, or fields of each item
/// when the result is a collection or a single-property collection wrapper.
/// </summary>
public interface ISelectOption
{
    string? Select { get; set; }
}
