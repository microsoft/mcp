// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.Extension.Services;

public interface ICliGenerateService
{
    public Task<HttpResponseMessage> GenerateAzureCLICommandAsync(
        string intent);
}
