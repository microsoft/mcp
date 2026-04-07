// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public class KqlQueryValidatorTests
{
    [Theory]
    [InlineData("testtable | where c1 == 'hello'")]
    [InlineData("testtable | where Age > 21 | take 10")]
    [InlineData("testtable | summarize count() by Name")]
    [InlineData("testtable | where Name == 'Alice' and City == 'Seattle'")]
    [InlineData("testtable | project Name, Age | order by Age desc")]
    public void ValidateQuerySafety_WithSafeQueries_ShouldNotThrow(string query)
    {
        KqlQueryValidator.ValidateQuerySafety(query);
    }

    [Theory]
    [InlineData("testtable | where c1=='0' or 1==1")]
    [InlineData("testtable | where c1=='0' or 1=1")]
    [InlineData("testtable | where Name == 'test' or true")]
    [InlineData("testtable | where c1=='0' or '1'=='1'")]
    public void ValidateQuerySafety_WithTautology_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(query));
        Assert.Contains("tautology", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(".drop table testtable")]
    [InlineData(".alter table testtable")]
    [InlineData(".create table testtable (c1:string)")]
    [InlineData(".delete table testtable records")]
    [InlineData(".set testtable <| testtable2")]
    [InlineData(".ingest into table testtable")]
    [InlineData(".purge table testtable")]
    [InlineData("testtable | .drop table other")]
    public void ValidateQuerySafety_WithManagementCommands_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(query));
        Assert.Contains("not allowed", ex.Message);
    }

    [Theory]
    [InlineData("testtable | take 10; .drop table testtable")]
    [InlineData("testtable; testtable2")]
    public void ValidateQuerySafety_WithStackedStatements_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(query));
        Assert.Contains("not allowed", ex.Message);
    }

    [Theory]
    [InlineData("testtable // this is a comment")]
    [InlineData("testtable | where Name == 'a' // get all")]
    public void ValidateQuerySafety_WithComments_ShouldThrow(string query)
    {
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(query));
        Assert.Contains("comments", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateQuerySafety_WithEmptyQuery_ShouldThrow()
    {
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(""));
        Assert.Contains("empty", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateQuerySafety_WithExcessiveLength_ShouldThrow()
    {
        var longQuery = "testtable | where Name == '" + new string('X', 10000) + "'";
        var ex = Assert.Throws<CommandValidationException>(() => KqlQueryValidator.ValidateQuerySafety(longQuery));
        Assert.Contains("length exceeds", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateQuerySafety_TautologyInsideStringLiteral_ShouldNotThrow()
    {
        // The tautology pattern is inside a string literal, not actual KQL logic
        KqlQueryValidator.ValidateQuerySafety("testtable | where Name == 'or 1==1'");
    }

    [Fact]
    public void ValidateQuerySafety_CommentInsideStringLiteral_ShouldNotThrow()
    {
        KqlQueryValidator.ValidateQuerySafety("testtable | where Url == 'https://example.com'");
    }

    [Fact]
    public void ValidateQuerySafety_TrailingSemicolon_ShouldNotThrow()
    {
        KqlQueryValidator.ValidateQuerySafety("testtable | take 10;");
    }
}
