// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public class OptionTypeNameHelperTests
{
    [Theory]
    [InlineData(typeof(string), "string")]
    [InlineData(typeof(bool), "boolean")]
    [InlineData(typeof(bool?), "boolean")]
    [InlineData(typeof(int), "integer")]
    [InlineData(typeof(int?), "integer")]
    [InlineData(typeof(long), "integer")]
    [InlineData(typeof(long?), "integer")]
    [InlineData(typeof(short), "integer")]
    [InlineData(typeof(byte), "integer")]
    [InlineData(typeof(uint), "integer")]
    [InlineData(typeof(ulong), "integer")]
    [InlineData(typeof(double), "number")]
    [InlineData(typeof(double?), "number")]
    [InlineData(typeof(float), "number")]
    [InlineData(typeof(decimal), "number")]
    [InlineData(typeof(Guid), "string")]
    [InlineData(typeof(Guid?), "string")]
    [InlineData(typeof(SampleEnum), "string")]
    [InlineData(typeof(SampleEnum?), "string")]
    [InlineData(typeof(string[]), "array")]
    [InlineData(typeof(int[]), "array")]
    [InlineData(typeof(List<string>), "array")]
    [InlineData(typeof(IEnumerable<int>), "array")]
    public void GetJsonSchemaType_MapsClrTypeToKeyword(Type valueType, string expected)
    {
        var result = OptionTypeNameHelper.GetJsonSchemaType(valueType);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetJsonSchemaType_WithNullType_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OptionTypeNameHelper.GetJsonSchemaType(null!));
    }

    private enum SampleEnum
    {
        First,
        Second,
    }
}
