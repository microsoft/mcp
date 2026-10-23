// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.Speech.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.Speech.Tests.Services;

public sealed class SpeechEndpointValidatorTests
{
    [Theory]
    [InlineData("Public", "https://my-speech.cognitiveservices.azure.com/")]
    [InlineData("China", "https://my-speech.cognitiveservices.azure.cn/")]
    [InlineData("Government", "https://my-speech.cognitiveservices.azure.us/")]
    [InlineData("Public", "https://cognitiveservices.azure.com/")]
    public void CreateAndValidateRealtimeTranscriptionEndpoint_ConfiguredCloud_ReturnsEndpoint(
        string cloud,
        string endpoint)
    {
        Uri validatedEndpoint = SpeechEndpointValidator.CreateAndValidateRealtimeTranscriptionEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetArmEnvironment(cloud)),
            endpoint);

        Assert.Equal(endpoint, validatedEndpoint.AbsoluteUri);
    }

    [Theory]
    [InlineData("https://evil.example/")]
    [InlineData("https://my-speech.cognitiveservices.azure.com.evil.example/")]
    [InlineData("http://my-speech.cognitiveservices.azure.com/")]
    [InlineData("https://user@my-speech.cognitiveservices.azure.com/")]
    [InlineData("https://my-speech.cognitiveservices.azure.com:8443/")]
    [InlineData("https://my-speech.cognitiveservices.azure.com/path")]
    [InlineData("https://my-speech.cognitiveservices.azure.com/?query=value")]
    [InlineData("https://my-speech.cognitiveservices.azure.com/#fragment")]
    public void CreateAndValidateRealtimeTranscriptionEndpoint_InvalidEndpoint_Throws(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            SpeechEndpointValidator.CreateAndValidateRealtimeTranscriptionEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: ArmEnvironment.AzurePublicCloud),
                endpoint));
    }

    [Theory]
    [InlineData("Public", "https://my-speech.cognitiveservices.azure.cn/")]
    [InlineData("China", "https://my-speech.cognitiveservices.azure.com/")]
    [InlineData("Government", "https://my-speech.cognitiveservices.azure.com/")]
    public void CreateAndValidateRealtimeTranscriptionEndpoint_CrossCloudEndpoint_Throws(
        string cloud,
        string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            SpeechEndpointValidator.CreateAndValidateRealtimeTranscriptionEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetArmEnvironment(cloud)),
                endpoint));
    }

    [Theory]
    [InlineData("Public", "my-speech.cognitiveservices.azure.com")]
    [InlineData("China", "my-speech.cognitiveservices.azure.cn")]
    [InlineData("Government", "my-speech.cognitiveservices.azure.us")]
    public void CreateAndValidateFastTranscriptionEndpoint_UsesFixedPathAndPreservesAuthorizedHost(
        string cloud,
        string host)
    {
        Uri transcriptionEndpoint = SpeechEndpointValidator.CreateAndValidateFastTranscriptionEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetArmEnvironment(cloud)),
            $"https://{host}/");

        Assert.Equal(Uri.UriSchemeHttps, transcriptionEndpoint.Scheme);
        Assert.Equal(host, transcriptionEndpoint.Host);
        Assert.Equal("/speechtotext/transcriptions:transcribe", transcriptionEndpoint.AbsolutePath);
        Assert.Equal("?api-version=2024-11-15", transcriptionEndpoint.Query);
    }

    [Theory]
    [InlineData("Public", "https://my-speech.cognitiveservices.azure.cn/")]
    [InlineData("China", "https://my-speech.cognitiveservices.azure.com/")]
    [InlineData("Government", "https://my-speech.cognitiveservices.azure.com/")]
    public void CreateAndValidateFastTranscriptionEndpoint_CrossCloudEndpoint_Throws(
        string cloud,
        string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            SpeechEndpointValidator.CreateAndValidateFastTranscriptionEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetArmEnvironment(cloud)),
                endpoint));
    }

    [Theory]
    [InlineData("Public", "my-speech.cognitiveservices.azure.com")]
    [InlineData("China", "my-speech.cognitiveservices.azure.cn")]
    [InlineData("Government", "my-speech.cognitiveservices.azure.us")]
    public void CreateAndValidateWebSocketEndpoint_UsesFixedPathAndPreservesAuthorizedHost(
        string cloud,
        string host)
    {
        Uri websocketEndpoint = SpeechEndpointValidator.CreateAndValidateWebSocketEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetArmEnvironment(cloud)),
            $"https://{host}/");

        Assert.Equal("wss", websocketEndpoint.Scheme);
        Assert.Equal(host, websocketEndpoint.Host);
        Assert.Equal("/tts/cognitiveservices/websocket/v1", websocketEndpoint.AbsolutePath);
        Assert.Equal("?traffictype=localmcp", websocketEndpoint.Query);
    }

    [Theory]
    [InlineData("Public", "https://my-speech.cognitiveservices.azure.cn/")]
    [InlineData("China", "https://my-speech.cognitiveservices.azure.com/")]
    [InlineData("Government", "https://my-speech.cognitiveservices.azure.com/")]
    public void CreateAndValidateWebSocketEndpoint_CrossCloudEndpoint_Throws(
        string cloud,
        string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            SpeechEndpointValidator.CreateAndValidateWebSocketEndpoint(AzureServiceTestHelpers.CreateAzureService(armEnvironment: GetArmEnvironment(cloud)),
                endpoint));
    }

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "Public" => ArmEnvironment.AzurePublicCloud,
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };
}
