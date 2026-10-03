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
    //
    // GPT-4o (o200k_base) is used as the reference model because it is the encoding shared
    // by the current generation of OpenAI models most commonly driving MCP clients (GPT-4o,
    // GPT-4o-mini, o1/o3 families), so its token counts best approximate the context-window
    // cost real agents pay for the tool surface. o200k_base is also more token-efficient than
    // the older cl100k_base, giving a conservative (lower-bound) estimate; exact counts still
    // vary per model, so these numbers are a comparable proxy rather than an absolute budget.
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
