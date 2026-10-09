// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Azure.Mcp.Tools.IoTHub.Services;

internal sealed record ArmGetResult(HttpStatusCode StatusCode, string Content)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and < 300;
}
