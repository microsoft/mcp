// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Extension.Services;

public interface ICliGenerateService
{
    Task<HttpResponseMessage> GenerateAzureCLICommandAsync(string intent, CancellationToken cancellationToken);
}
