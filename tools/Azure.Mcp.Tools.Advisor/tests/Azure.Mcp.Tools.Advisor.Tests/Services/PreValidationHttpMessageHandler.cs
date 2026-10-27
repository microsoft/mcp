// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Tests.Services;

internal sealed class PreValidationHttpMessageHandler(
    Func<HttpRequestMessage, string?, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return responseFactory(request, body);
    }
}
