// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Fabric.Mcp.Tools.PublicApi.Tests.Services;

public class FabricPublicApiServiceTests
{
    private readonly ILogger<FabricPublicApiService> _logger;
    private readonly IResourceProviderService _resourceProvider;
    private readonly FabricPublicApiService _service;

    public FabricPublicApiServiceTests()
    {
        _logger = Substitute.For<ILogger<FabricPublicApiService>>();
        _resourceProvider = Substitute.For<IResourceProviderService>();
        _service = new FabricPublicApiService(_logger, _resourceProvider);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FabricPublicApiService(null!, _resourceProvider));
    }

    [Fact]
    public void Constructor_WithNullResourceProvider_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FabricPublicApiService(_logger, null!));
    }

    #endregion

    #region GetFabricWorkloadPublicApis Tests

    [Fact]
    public async Task GetFabricWorkloadPublicApis_WithValidWorkload_ReturnsApi()
    {
        // Arrange
        var workload = "notebook";
        var expectedSpec = "{ \"swagger\": \"2.0\" }";
        var expectedDefinitions = new Dictionary<string, string> { { "definitions.json", "{ \"definitions\": {} }" } };

        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/swagger.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSpec));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { "definitions.json" }));
        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/definitions.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedDefinitions["definitions.json"]));

        // Act
        var result = await _service.GetWorkloadPublicApis(workload, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSpec, result.apiSpecification);
        Assert.Equal(expectedDefinitions, result.apiModelDefinitions);
        await _resourceProvider.Received(1).GetResource("fabric-rest-api-specs/contents/notebook/swagger.json", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFabricWorkloadPublicApis_WithDefinitionsDirectory_ReturnsApiWithDefinitions()
    {
        // Arrange
        var workload = "platform";
        var expectedSpec = "{ \"swagger\": \"2.0\" }";

        _resourceProvider.GetResource("fabric-rest-api-specs/contents/platform/swagger.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedSpec));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/platform/", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { "definitions/" }));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/platform/definitions/", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new[] { "model1.json", "model2.json" }));
        _resourceProvider.GetResource("definitions/model1.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("{ \"model1\": {} }"));
        _resourceProvider.GetResource("definitions/model2.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult("{ \"model2\": {} }"));

        // Act
        var result = await _service.GetWorkloadPublicApis(workload, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedSpec, result.apiSpecification);
        Assert.Equal(2, result.apiModelDefinitions.Count);
        Assert.Contains("definitions/model1.json", result.apiModelDefinitions.Keys);
        Assert.Contains("definitions/model2.json", result.apiModelDefinitions.Keys);
    }

    [Fact]
    public async Task GetFabricWorkloadPublicApis_WithNullSpec_ReturnsEmptySpec()
    {
        // Arrange
        var workload = "notebook";

        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/swagger.json", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(string.Empty));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Array.Empty<string>()));

        // Act
        var result = await _service.GetWorkloadPublicApis(workload, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.apiSpecification);
        Assert.Empty(result.apiModelDefinitions);
    }

    [Fact]
    public async Task GetFabricWorkloadPublicApis_WithException_PropagatesException()
    {
        // Arrange
        var workload = "notebook";
        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/swagger.json", Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Resource not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetWorkloadPublicApis(workload, TestContext.Current.CancellationToken));
    }

    #endregion

    #region ListFabricWorkloadsAsync Tests

    [Fact]
    public async Task ListFabricWorkloadsAsync_ReturnsWorkloads()
    {
        // Arrange
        var expectedWorkloads = new[] { "notebook", "report", "platform" };
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/", ResourceType.Directory, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(expectedWorkloads));

        // Act
        var result = await _service.ListWorkloadsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(expectedWorkloads, result);
        await _resourceProvider.Received(1).ListResourcesInPath("fabric-rest-api-specs/contents/", ResourceType.Directory, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ListFabricWorkloadsAsync_WithException_PropagatesException()
    {
        // Arrange
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/", ResourceType.Directory, TestContext.Current.CancellationToken)
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ListWorkloadsAsync(TestContext.Current.CancellationToken));
    }

    #endregion

    #region GetExamplesAsync Tests

    [Fact]
    public async Task GetExamplesAsync_WithValidWorkload_ReturnsExamples()
    {
        // Arrange
        var workloadType = "notebook";
        var expectedFiles = new[] { "example1.json", "example2.json" };

        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/", ResourceType.File, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(expectedFiles));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/", ResourceType.Directory, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(Array.Empty<string>()));
        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/examples/example1.json", TestContext.Current.CancellationToken)
            .Returns(Task.FromResult("{ \"example1\": \"content\" }"));
        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/examples/example2.json", TestContext.Current.CancellationToken)
            .Returns(Task.FromResult("{ \"example2\": \"content\" }"));

        // Act
        var result = await _service.GetWorkloadExamplesAsync(workloadType, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("example1.json", result.Keys);
        Assert.Contains("example2.json", result.Keys);
        Assert.Equal("{ \"example1\": \"content\" }", result["example1.json"]);
        Assert.Equal("{ \"example2\": \"content\" }", result["example2.json"]);
    }

    [Fact]
    public async Task GetExamplesAsync_WithSubDirectories_ReturnsAllExamples()
    {
        // Arrange
        var workloadType = "notebook";

        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/", ResourceType.File, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(new[] { "root.json" }));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/", ResourceType.Directory, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(new[] { "subdir1" }));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/subdir1/", ResourceType.File, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(new[] { "sub1.json" }));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/subdir1/", ResourceType.Directory, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(Array.Empty<string>()));

        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/examples/root.json", TestContext.Current.CancellationToken)
            .Returns(Task.FromResult("{ \"root\": \"content\" }"));
        _resourceProvider.GetResource("fabric-rest-api-specs/contents/notebook/examples/subdir1/sub1.json", TestContext.Current.CancellationToken)
            .Returns(Task.FromResult("{ \"sub1\": \"content\" }"));

        // Act
        var result = await _service.GetWorkloadExamplesAsync(workloadType, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("root.json", result.Keys);
        Assert.Contains("subdir1sub1.json", result.Keys);
    }

    [Fact]
    public async Task GetExamplesAsync_WithNoFiles_ReturnsEmptyDictionary()
    {
        // Arrange
        var workloadType = "notebook";

        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/", ResourceType.File, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(Array.Empty<string>()));
        _resourceProvider.ListResourcesInPath("fabric-rest-api-specs/contents/notebook/examples/", ResourceType.Directory, TestContext.Current.CancellationToken)
            .Returns(Task.FromResult(Array.Empty<string>()));

        // Act
        var result = await _service.GetWorkloadExamplesAsync(workloadType, TestContext.Current.CancellationToken);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region GetFabricWorkloadItemDefinition Tests

    [Fact]
    public void GetFabricWorkloadItemDefinition_WithValidWorkload_ReturnsDefinition()
    {
        // Arrange
        var workloadType = "notebook";

        // Act
        var result = _service.GetWorkloadItemDefinition(workloadType);

        // Assert
        // Since this calls a static method, we can only test that it doesn't throw
        Assert.NotNull(result);
    }

    #endregion

    #region GetTopicBestPractices Tests

    [Fact]
    public void GetTopicBestPractices_WithValidTopic_ReturnsPractices()
    {
        // Arrange
        var topic = "pagination";

        // Act
        var result = _service.GetTopicBestPractices(topic);

        // Assert
        // Since this calls a static method, we can only test that it doesn't throw
        Assert.NotNull(result);
        Assert.Single(result);
    }

    #endregion
}
