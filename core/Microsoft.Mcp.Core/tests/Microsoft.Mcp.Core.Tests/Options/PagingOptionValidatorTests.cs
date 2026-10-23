// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Paging;
using Microsoft.Mcp.Core.Options;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Options;

public sealed class PagingOptionValidatorTests()
{
    [Theory]
    [InlineData(null)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    public void Validate_AcceptsSupportedPageSizes(int? pageSize) =>
        PagingOptionValidator.Validate(new PageRequest(pageSize));

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1001)]
    public void Validate_RejectsUnsupportedPageSizes(int pageSize) =>
        Assert.Throws<ArgumentException>(() => PagingOptionValidator.Validate(new PageRequest(pageSize)));

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Validate_RejectsEmptyContinuationTokens(string token) =>
        Assert.Throws<ArgumentException>(() => PagingOptionValidator.Validate(new PageRequest(ContinuationToken: token)));

    [Fact]
    public void Validate_RejectsOversizedContinuationTokens() =>
        Assert.Throws<ArgumentException>(() =>
            PagingOptionValidator.Validate(new PageRequest(ContinuationToken: new string('a', PageRequest.MaxContinuationTokenLength + 1))));
}
