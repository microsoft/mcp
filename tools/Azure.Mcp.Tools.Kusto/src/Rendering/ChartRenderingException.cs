// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Rendering;

/// <summary>
/// Thrown when a Kusto query result cannot be rendered as the requested <see cref="ChartType"/>.
/// </summary>
/// <remarks>
/// The message is user-facing — it describes precisely why the data did not fit (e.g. missing
/// datetime column for <see cref="ChartType.TimeSeries"/>) so the caller can correct their KQL
/// query or pick a different chart type.
/// </remarks>
public sealed class ChartRenderingException(string message, Exception? innerException = null)
    : Exception(message, innerException);
