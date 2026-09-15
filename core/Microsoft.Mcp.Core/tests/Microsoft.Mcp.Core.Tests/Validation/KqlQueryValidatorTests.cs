// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Validation;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Validation;

public class KqlQueryValidatorTests
{
    [Theory]
    [InlineData("testtable | where c1 == 'hello'")]
    [InlineData("testtable | where Age > 21 | take 10")]
    [InlineData("testtable | summarize count() by Name")]
    [InlineData("testtable | where Name == 'Alice' and City == 'Seattle'")]
    [InlineData("testtable | project Name, Age | order by Age desc")]
    [InlineData(".show version | project Version")]
    [InlineData("testtable | where c1=='0' or 1==1")]
    [InlineData("testtable | where Name == 'test' or true")]
    [InlineData(".drop table testtable")]
    [InlineData(".set testtable <| testtable2")]
    public void ValidateQuerySafety_WithValidQueries_ShouldNotThrow(string query)
    {
        KqlQueryValidator.ValidateQuerySafety(query);
    }

    [Fact]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrow()
    {
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(string.Empty));
        Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateQuerySafety_WithExcessiveLength_ShouldThrow()
    {
        var longQuery = "testtable | where Name == '" + new string('X', 10000) + "'";
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(longQuery));
        Assert.Contains("length exceeds", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
