// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;

namespace Azure.Mcp.Core.Tests.RecordingFramework;

internal sealed class TestPollingAzureService(IAzureService service) : BaseAzureService(service)
{
    public static Task WaitAsync(Operation operation, CancellationToken cancellationToken)
        => WaitForLroCompletionAsync(operation, cancellationToken);

    public static Task WaitAsync(Operation<string> operation, CancellationToken cancellationToken)
        => WaitForLroCompletionAsync(operation, cancellationToken);
}
