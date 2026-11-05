// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;
using Xunit;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobInstanceTestData;
using static Fabric.Mcp.Tools.Core.Tests.TestSupport.ItemJobRunTestData;

namespace Fabric.Mcp.Tools.Core.Tests.Validation;

public sealed class ItemJobRunInputTests()
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("{}", null)]
    [InlineData(null, "[]")]
    [InlineData("{}", "[]")]
    [InlineData("""{"opaque":[null,1e30,true,{"x":"text"}]}""", Parameters)]
    [InlineData(null, """[{"name":"","type":"","value":null},{"name":"A","type":"FutureType","value":-1.25}]""")]
    public static void Body_PreservesOmissionAndOpaqueValues(string? execution, string? parameters)
    {
        var body = ItemJobRunInput.CreateBody(execution, parameters, TestContext.Current.CancellationToken);
        if (execution is null && parameters is null)
        {
            Assert.Null(body);
            return;
        }
        using var document = JsonDocument.Parse(body!);
        Assert.Equal(execution is not null, document.RootElement.TryGetProperty("executionData", out var actualExecution));
        Assert.Equal(parameters is not null, document.RootElement.TryGetProperty("parameters", out var actualParameters));
        if (execution is not null)
        {
            using var expected = JsonDocument.Parse(execution);
            Assert.True(JsonElement.DeepEquals(expected.RootElement, actualExecution));
        }
        if (parameters is not null)
        {
            using var expected = JsonDocument.Parse(parameters);
            Assert.True(JsonElement.DeepEquals(expected.RootElement, actualParameters));
        }
        var request = JsonSerializer.Deserialize(body!, CoreJsonContext.Default.ItemJobRunRequest);
        Assert.Equal(body, JsonSerializer.Serialize(request, CoreJsonContext.Default.ItemJobRunRequest));
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("null", null)]
    [InlineData("[]", null)]
    [InlineData("true", null)]
    [InlineData("""{"x":1,"x":2}""", null)]
    [InlineData("""{"x":{"p":1,"p":2}}""", null)]
    [InlineData("""{"private":1,}""", null)]
    [InlineData("""{"private":/*comment*/1}""", null)]
    [InlineData("""{"value":"\uD800"}""", null)]
    [InlineData("""{"\uD800":1}""", null)]
    [InlineData(null, "null")]
    [InlineData(null, "{}")]
    [InlineData(null, "true")]
    [InlineData(null, "[1]")]
    [InlineData(null, "[null]")]
    [InlineData(null, "[{}]")]
    [InlineData(null, """[{"name":"x","type":"Text"}]""")]
    [InlineData(null, """[{"name":"x","value":1}]""")]
    [InlineData(null, """[{"type":"Text","value":1}]""")]
    [InlineData(null, """[{"name":1,"type":"Text","value":1}]""")]
    [InlineData(null, """[{"name":null,"type":"Text","value":1}]""")]
    [InlineData(null, """[{"name":"x","type":null,"value":1}]""")]
    [InlineData(null, """[{"name":"x","type":false,"value":1}]""")]
    [InlineData(null, """[{"name":"\uD800","type":"Text","value":1}]""")]
    [InlineData(null, """[{"name":"x","type":"\uD800","value":1}]""")]
    [InlineData(null, """[{"name":"x","type":"Text","value":"\uD800"}]""")]
    [InlineData(null, """[{"name":"x","type":"Text","value":{"private":1,"private":2}}]""")]
    [InlineData(null, """[{"name":"x","na\u006de":"y","type":"Text","value":1}]""")]
    [InlineData(null, """[{"name":"x","type":"Text","value":1},{"name":"X","type":"Text","value":2}]""")]
    public static void Body_RejectsMalformedDuplicateAndInvalidParameterEnvelopes(string? execution, string? parameters)
    {
        var error = Assert.Throws<FabricItemInputException>(() =>
            ItemJobRunInput.CreateBody(execution, parameters, TestContext.Current.CancellationToken));
        Assert.DoesNotContain("private", error.Message);
        Assert.DoesNotContain("LineNumber", error.Message);
    }

    [Theory]
    [InlineData(256, true)]
    [InlineData(257, false)]
    public static void Body_EnforcesDocumentedNameBoundaryWithoutAnEnumAllowList(int length, bool valid)
    {
        var parameters = $$"""[{"name":"{{new string('a', length)}}","type":"NotYetDocumented","value":[1,null,"x"]}]""";
        if (valid)
        {
            Assert.NotNull(ItemJobRunInput.CreateBody(null, parameters, TestContext.Current.CancellationToken));
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() => ItemJobRunInput.CreateBody(null, parameters, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    public static void Body_EnforcesExactUtf8InputBudget(bool array, int excess)
    {
        const int limit = ItemJobRunInput.MaxRequestBytes;
        var json = new string(' ', limit - 2 + excess) + (array ? "[]" : "{}");
        if (excess == 0)
        {
            Assert.NotNull(ItemJobRunInput.CreateBody(array ? null : json, array ? json : null, TestContext.Current.CancellationToken));
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() =>
                ItemJobRunInput.CreateBody(array ? null : json, array ? json : null, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public static void Body_EnforcesExactSerializedRequestBudgetIncludingEnvelope(int excess)
    {
        const string empty = """{"executionData":{"pad":""}}""";
        var execution = $$"""{"pad":"{{new string('x', ItemJobRunInput.MaxRequestBytes - empty.Length + excess)}}"}""";
        if (excess == 0)
        {
            var body = ItemJobRunInput.CreateBody(execution, null, TestContext.Current.CancellationToken);
            Assert.Equal(ItemJobRunInput.MaxRequestBytes, Encoding.UTF8.GetByteCount(body!));
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() => ItemJobRunInput.CreateBody(execution, null, TestContext.Current.CancellationToken));
        }
    }

    [Fact]
    public static void Body_AppliesSharedBudgetToBothFieldsAndSerializedUnicode()
    {
        var execution = $$"""{"pad":"{{new string('x', 600_000)}}"}""";
        var parameters = $$"""[{"name":"x","type":"Text","value":"{{new string('x', 600_000)}}"}]""";
        Assert.Throws<FabricItemInputException>(() => ItemJobRunInput.CreateBody(execution, parameters, TestContext.Current.CancellationToken));
        var unicode = $$"""{"pad":"{{new string('\u00e9', 200_000)}}"}""";
        Assert.True(Encoding.UTF8.GetByteCount(unicode) < ItemJobRunInput.MaxRequestBytes);
        Assert.Throws<FabricItemInputException>(() => ItemJobRunInput.CreateBody(unicode, null, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(32, true)]
    [InlineData(33, false)]
    public static void Body_EnforcesDepthIncludingOpaqueValues(int depth, bool valid)
    {
        var json = "{\"x\":" + new string('[', depth - 1) + "0" + new string(']', depth - 1) + "}";
        if (valid)
        {
            Assert.NotNull(ItemJobRunInput.CreateBody(json, null, TestContext.Current.CancellationToken));
        }
        else
        {
            Assert.Throws<FabricItemInputException>(() => ItemJobRunInput.CreateBody(json, null, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".")]
    [InlineData("..")]
    [InlineData("private\r\ninput")]
    public static void Context_RejectsUnsafeJobTypeRepresentations(string jobType)
    {
        var validation = new ValidationResult();
        ItemJobRunInput.ValidateContext(WorkspaceId, ItemId, jobType, validation);
        Assert.False(validation.IsValid);
        Assert.DoesNotContain("private", string.Join(' ', validation.Errors));
    }

    [Theory]
    [InlineData(0xd800)]
    [InlineData(0xdc00)]
    public static void Context_RejectsUnpairedSurrogates(int codePoint)
    {
        var validation = new ValidationResult();
        ItemJobRunInput.ValidateContext(WorkspaceId, ItemId, ((char)codePoint).ToString(), validation);
        Assert.False(validation.IsValid);
    }

    [Fact]
    public static void Body_RejectsInvalidRawUnicodeRatherThanReplacingItDuringUtf8Conversion()
    {
        var execution = "{\"value\":\"" + (char)0xd800 + "\"}";
        Assert.Throws<FabricItemInputException>(() => ItemJobRunInput.CreateBody(execution, null, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public static void Context_EnforcesExactJobTypeUtf8Budget(bool unicode)
    {
        var jobType = unicode ? new string('\u00e9', ItemJobRunInput.MaxJobTypeBytes / 2) : new string('x', ItemJobRunInput.MaxJobTypeBytes);
        var validation = new ValidationResult();
        ItemJobRunInput.ValidateContext(WorkspaceId, ItemId, jobType, validation);
        Assert.True(validation.IsValid);
        ItemJobRunInput.ValidateContext(WorkspaceId, ItemId, jobType + "x", validation);
        Assert.False(validation.IsValid);
    }
}
