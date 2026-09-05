// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cspell:ignore externaldata getschema leftouter toscalar Тable

using Azure.Mcp.Tools.Monitor.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Log;

public sealed class LogSearchQueryValidatorTests
{
    [Theory]
    [InlineData("McpBasic_CL")]
    [InlineData("A")]
    [InlineData("Table_123")]
    public void ValidateTableIdentifier_AcceptsAsciiIdentifiers(string table)
    {
        LogSearchQueryValidator.ValidateTableIdentifier(table);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" table")]
    [InlineData("1Table")]
    [InlineData("Table Name")]
    [InlineData("Table.Name")]
    [InlineData("Table-Name")]
    [InlineData("['Table']")]
    [InlineData("Table\"")]
    [InlineData("Тable")]
    [InlineData("Table | take 1")]
    public void ValidateTableIdentifier_RejectsInvalidOrInjectedIdentifiers(string table)
    {
        Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.ValidateTableIdentifier(table));
    }

    [Theory]
    [InlineData("| where Level == 'Error' | project TimeGenerated, Message")]
    [InlineData(" \r\n\t| summarize Count=count() by bin(TimeGenerated, 1h) | order by Count desc")]
    [InlineData("| extend Parsed=parse_json(Properties), Text=tostring(Message) | take 10")]
    [InlineData("| parse Message with Prefix ':' Value | project-away Prefix")]
    [InlineData("| where isnotempty(Message) and strlen(Message) > 2 | limit 5")]
    [InlineData("| getschema")]
    [InlineData("| lookup kind=leftouter AnalyticsTable on CorrelationId")]
    [InlineData("| union kind=outer AnalyticsOne, AnalyticsTwo")]
    [InlineData("| where (Count > 1) and (Level == 'Error')")]
    [InlineData("| extend Value=ordinary_function(Message)")]
    [InlineData("| extend Length=strlen(tostring(Message)) | project Length")]
    public void ValidatePipeline_AcceptsNormalPipelines(string pipeline)
    {
        LogSearchQueryValidator.ValidatePipeline(pipeline);
    }

    /// <summary>
    /// Quoted content is blanked before structural scanning, so characters that would be rejected
    /// outside a literal must still be accepted inside one.
    /// </summary>
    [Theory]
    [InlineData("| where Message == 'https://example.test/a//b;still-a-string'")]
    [InlineData("| where Message == \"a | b; // not a comment\"")]
    [InlineData("| where Message has 'C:\\\\temp\\\\file'")]
    [InlineData("| where Message has 'it\\'s escaped'")]
    [InlineData("| where Message has @'C:\\temp\\file|pipe;semicolon'")]
    [InlineData("| where Message has @\"verbatim \"\"quoted\"\" | text\"")]
    public void ValidatePipeline_AcceptsStringLiteralContent(string pipeline)
    {
        LogSearchQueryValidator.ValidatePipeline(pipeline);
    }

    [Theory]
    [InlineData("")]
    [InlineData("McpBasic_CL | take 1")]
    [InlineData("where Level == 'Error'")]
    public void ValidatePipeline_RequiresLeadingPipe(string pipeline)
    {
        Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.ValidatePipeline(pipeline));
    }

    [Fact]
    public void ValidatePipeline_RejectsMoreThanTenThousandCharacters()
    {
        var pipeline = "|" + new string('x', 10_000);

        Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.ValidatePipeline(pipeline));
    }

    [Theory]
    [InlineData("| where true; OtherTable | take 1")]
    [InlineData("| take 1 // comment")]
    [InlineData("| take 1 /* comment */")]
    [InlineData("| let replacement = OtherTable")]
    [InlineData("| table('OtherTable')")]
    [InlineData("| workspace('other').Table")]
    [InlineData("| app('other').Table")]
    [InlineData("| resource('/subscriptions/other')")]
    [InlineData("| cluster('other').database('db').Table")]
    [InlineData("| join kind=inner OtherTable on CorrelationId")]
    [InlineData("| find where Message has 'error'")]
    [InlineData("| search 'needle'")]
    [InlineData("| externaldata(Value:string)['https://example.test/data']")]
    [InlineData("| invoke SomeStoredFunction()")]
    [InlineData("| union workspace('other').AnalyticsOne")]
    [InlineData("| lookup workspace('other').AnalyticsOne on CorrelationId")]
    [InlineData("| extend Leaked=toscalar(SensitiveTable | summarize make_list(SecretColumn)) | project Leaked")]
    [InlineData("| where (Count > 1")]
    [InlineData("| where Count > 1)")]
    [InlineData("| where Message == 'unterminated")]
    public void ValidatePipeline_RejectsUnsafeOrUnsupportedPipelines(string pipeline)
    {
        Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.ValidatePipeline(pipeline));
    }

    [Fact]
    public void ValidatePipeline_CommentContainingQuote_ReportsCommentError()
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.ValidatePipeline("| take 1 // don't return more"));

        Assert.Contains("comments are not allowed", exception.Message);
    }

    [Fact]
    public void ValidatePipeline_RejectsControlCharacters()
    {
        Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.ValidatePipeline("| where Message == 1\u0001"));
    }

    [Fact]
    public void Validate_RejectsTableInjectionBeforePipelineChecks()
    {
        var exception = Assert.Throws<CommandValidationException>(
            () => LogSearchQueryValidator.Validate("McpBasic_CL | take 1", "| take 1"));

        Assert.Contains("--table", exception.Message);
    }
}
