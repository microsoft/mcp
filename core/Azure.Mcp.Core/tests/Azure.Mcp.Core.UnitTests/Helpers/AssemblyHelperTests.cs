// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Reflection.Emit;
using Azure.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Core.UnitTests.Helpers;

public class AssemblyHelperTests
{
    [Fact]
    public void GetAssemblyVersion_ValidAssembly_ReturnsVersionWithoutBuildMetadata()
    {
        // Arrange
        var assembly = typeof(AssemblyHelper).Assembly;

        // Act
        var version = AssemblyHelper.GetAssemblyVersion(assembly);

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
        Assert.DoesNotContain("+", version); // Build metadata (git hash) should be stripped
    }

    [Fact]
    public void GetFullAssemblyVersion_ValidAssembly_ReturnsVersionWithBuildMetadata()
    {
        // Arrange
        var assembly = typeof(AssemblyHelper).Assembly;

        // Act
        var version = AssemblyHelper.GetFullAssemblyVersion(assembly);

        // Assert
        Assert.NotNull(version);
        Assert.NotEmpty(version);
        // Full version should contain the git hash after '+'
        Assert.Contains("+", version);
    }

    [Fact]
    public void GetAssemblyVersion_AssemblyWithoutAttribute_ThrowsInvalidOperationException()
    {
        // Arrange
        var assembly = CreateAssemblyWithoutVersionAttribute();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => AssemblyHelper.GetAssemblyVersion(assembly));

        Assert.Contains("AssemblyInformationalVersionAttribute", exception.Message);
        Assert.Contains("required", exception.Message);
    }

    [Fact]
    public void GetFullAssemblyVersion_AssemblyWithoutAttribute_ThrowsInvalidOperationException()
    {
        // Arrange
        var assembly = CreateAssemblyWithoutVersionAttribute();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => AssemblyHelper.GetFullAssemblyVersion(assembly));

        Assert.Contains("AssemblyInformationalVersionAttribute", exception.Message);
        Assert.Contains("required", exception.Message);
    }

    [Fact]
    public void GetAssemblyVersion_ConsistentWithGetFullAssemblyVersion()
    {
        // Arrange
        var assembly = typeof(AssemblyHelper).Assembly;

        // Act
        var shortVersion = AssemblyHelper.GetAssemblyVersion(assembly);
        var fullVersion = AssemblyHelper.GetFullAssemblyVersion(assembly);

        // Assert
        // Short version should be the prefix of full version (before the '+')
        Assert.StartsWith(shortVersion, fullVersion);

        // If full version has build metadata, short version should not
        if (fullVersion.Contains('+'))
        {
            Assert.DoesNotContain("+", shortVersion);
            Assert.Equal(fullVersion.Substring(0, fullVersion.IndexOf('+')), shortVersion);
        }
    }

    private static Assembly CreateAssemblyWithoutVersionAttribute()
    {
        // Create a dynamic assembly without AssemblyInformationalVersionAttribute
        var assemblyName = new AssemblyName("TestAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.Run);

        return assemblyBuilder;
    }
}
