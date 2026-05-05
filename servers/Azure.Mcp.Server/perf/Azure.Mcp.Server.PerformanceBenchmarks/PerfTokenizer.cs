// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Microsoft.ML.Tokenizers;

namespace Azure.Mcp.Server.Perf;

/// <summary>
/// Shared GPT-4o tokenizer instance and byte/token measurement helper used by all
/// measurement classes in this project.
/// </summary>
internal static class PerfTokenizer
{
    // GPT-4o uses o200k_base encoding. GPT-4/GPT-3.5-turbo use cl100k_base.
    internal static readonly TiktokenTokenizer Gpt4o =
        TiktokenTokenizer.CreateForModel("gpt-4o");

    /// <summary>
    /// Returns <c>(Bytes, ExactTokens, ApproxTokens)</c> for <paramref name="text"/>
    /// using the GPT-4o o200k_base encoding.
    /// <c>ApproxTokens</c> is a quick bytes÷4 estimate useful for sanity-checking.
    /// </summary>
    internal static (int Bytes, int ExactTokens, int ApproxTokens) Measure(string text)
    {
        var bytes = Encoding.UTF8.GetByteCount(text);
        return (bytes, Gpt4o.CountTokens(text), bytes / 4);
    }
}
