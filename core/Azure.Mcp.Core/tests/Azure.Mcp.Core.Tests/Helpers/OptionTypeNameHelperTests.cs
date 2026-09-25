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
    [InlineData(typeof(HashSet<int>), "array")]
    [InlineData(typeof(Queue<string>), "array")]
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

    [Theory]
    [InlineData(typeof(string[]), "string")]
    [InlineData(typeof(int[]), "integer")]
    [InlineData(typeof(long[]), "integer")]
    [InlineData(typeof(bool[]), "boolean")]
    [InlineData(typeof(double[]), "number")]
    [InlineData(typeof(SampleEnum[]), "string")]
    [InlineData(typeof(List<string>), "string")]
    [InlineData(typeof(IEnumerable<int>), "integer")]
    [InlineData(typeof(IList<bool>), "boolean")]
    [InlineData(typeof(HashSet<int>), "integer")]
    [InlineData(typeof(Queue<string>), "string")]
    [InlineData(typeof(ICollection<double>), "number")]
    public void GetElementJsonSchemaType_ReturnsElementKeyword_ForCollections(Type valueType, string expected)
    {
        var result = OptionTypeNameHelper.GetElementJsonSchemaType(valueType);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(int))]
    [InlineData(typeof(int?))]
    [InlineData(typeof(bool))]
    [InlineData(typeof(SampleEnum))]
    [InlineData(typeof(SampleEnum?))]
    public void GetElementJsonSchemaType_ReturnsNull_ForScalars(Type valueType)
    {
        var result = OptionTypeNameHelper.GetElementJsonSchemaType(valueType);

        Assert.Null(result);
    }

    [Fact]
    public void GetElementJsonSchemaType_WithNullType_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => OptionTypeNameHelper.GetElementJsonSchemaType(null!));
    }

    private enum SampleEnum
    {
        First,
        Second,
    }
}
