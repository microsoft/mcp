// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Paging;

namespace Microsoft.Mcp.Core.Options;

/// <summary>
/// Validates shared page-size bounds and continuation-token length and presence constraints.
/// </summary>
/// <remarks>
/// Token decoding, version checks, and request-scope validation are the responsibility of the service.
/// </remarks>
public static class PagingOptionValidator
{
    /// <summary>
    /// Adds paging option errors to the command's validation result.
    /// </summary>
    /// <param name="options">The bound paging options to validate.</param>
    /// <param name="validationResult">The result to which validation errors are appended.</param>
    public static void Validate(IPagingOption options, ValidationResult validationResult) =>
        Validate(options.PageSize, options.ContinuationToken, validationResult);

    /// <summary>
    /// Validates a service page request and throws when its paging values are unsupported.
    /// </summary>
    /// <param name="request">The page request to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// The page size is outside the supported range, or a supplied continuation token is empty,
    /// whitespace, or longer than <see cref="PageRequest.MaxContinuationTokenLength"/>.
    /// </exception>
    public static void Validate(PageRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = new ValidationResult();
        Validate(request.PageSize, request.ContinuationToken, validationResult);
        if (!validationResult.IsValid)
        {
            throw new ArgumentException(string.Join('\n', validationResult.Errors), nameof(request));
        }
    }

    /// <summary>
    /// Applies the shared paging constraints without decoding the continuation token.
    /// </summary>
    /// <param name="pageSize">The optional requested page size.</param>
    /// <param name="continuationToken">The optional token from a previous page.</param>
    /// <param name="validationResult">The result to which validation errors are appended.</param>
    private static void Validate(int? pageSize, string? continuationToken, ValidationResult validationResult)
    {
        if (pageSize is < 1 or > PageRequest.MaxPageSize)
        {
            validationResult.Errors.Add($"--page-size must be between 1 and {PageRequest.MaxPageSize}.");
        }

        if (continuationToken is not null &&
            (string.IsNullOrWhiteSpace(continuationToken) || continuationToken.Length > PageRequest.MaxContinuationTokenLength))
        {
            validationResult.Errors.Add($"--continuation-token must be nonempty and no longer than {PageRequest.MaxContinuationTokenLength} characters.");
        }
    }
}
