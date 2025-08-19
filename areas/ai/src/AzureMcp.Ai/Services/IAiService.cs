using AzureMcp.Ai.Models;
using AzureMcp.Ai.Options.Completions;

namespace AzureMcp.Ai.Services;

public interface IAiService
{
    Task<CompletionsCreateResult> CreateCompletionAsync(
        CompletionsCreateOptions options,
        CancellationToken cancellationToken = default);
}