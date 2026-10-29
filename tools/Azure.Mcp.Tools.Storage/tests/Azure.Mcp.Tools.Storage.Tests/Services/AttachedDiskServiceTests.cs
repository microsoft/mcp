// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Storage.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Services;

public class AttachedDiskServiceTests
{
    private const string SubscriptionId = "00000000-0000-0000-0000-000000000001";
    private const string ResourceGroup = "test-rg";
    private const string VmResourceId = $"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup}/providers/Microsoft.Compute/virtualMachines/test-vm";
    private const string VmssResourceId = $"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup}/providers/Microsoft.Compute/virtualMachineScaleSets/test-vmss";
    private const string VmssVmResourceId = $"{VmssResourceId}/virtualMachines/instance-a";
    private const string OsDiskResourceId = $"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup}/providers/Microsoft.Compute/disks/os-disk";
    private const string DataDiskResourceId = $"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup}/providers/Microsoft.Compute/disks/data-disk";

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000002")]
    public async Task ResolveDiskNamesAsync_StandaloneVm_UsesVirtualMachineCollection(string? tenant)
    {
        var handler = new RecordingHttpMessageHandler(
            CreateJsonResponse(ResourceGroupResponse),
            CreateJsonResponse(VirtualMachineResponse));
        (AttachedDiskService service, IAzureService azureService) = CreateService(handler, tenant);

        string[] result = await service.ResolveDiskNamesAsync(
            VmResourceId,
            ["data-disk", "os-disk"],
            tenant,
            TestContext.Current.CancellationToken);

        Assert.Equal([DataDiskResourceId, OsDiskResourceId], result);
        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal(
            $"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroup}",
            handler.Requests[0].AbsolutePath,
            ignoreCase: true);
        Assert.Contains("/virtualMachines/test-vm", handler.Requests[1].AbsolutePath);
        Assert.DoesNotContain("/virtualMachineScaleSets/", handler.Requests[1].AbsolutePath);
        await azureService.Received(1).GetSubscription(
            SubscriptionId, tenant, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000002")]
    public async Task ResolveDiskNamesAsync_VmssInstance_UsesParentScaleSetAndInstanceCollection(string? tenant)
    {
        var handler = new RecordingHttpMessageHandler(
            CreateJsonResponse(ResourceGroupResponse),
            CreateJsonResponse(VirtualMachineScaleSetResponse),
            CreateJsonResponse(VirtualMachineScaleSetVmResponse));
        (AttachedDiskService service, IAzureService azureService) = CreateService(handler, tenant);

        string[] result = await service.ResolveDiskNamesAsync(
            VmssVmResourceId,
            ["os-disk", "data-disk"],
            tenant,
            TestContext.Current.CancellationToken);

        Assert.Equal([OsDiskResourceId, DataDiskResourceId], result);
        Assert.Equal(3, handler.Requests.Count);
        Assert.Contains("/virtualMachineScaleSets/test-vmss", handler.Requests[1].AbsolutePath);
        Assert.DoesNotContain("/virtualMachines/", handler.Requests[1].AbsolutePath);
        Assert.Contains(
            "/virtualMachineScaleSets/test-vmss/virtualMachines/instance-a",
            handler.Requests[2].AbsolutePath);
        await azureService.Received(1).GetSubscription(
            SubscriptionId, tenant, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("00000000-0000-0000-0000-000000000002")]
    public async Task ResolveFriendlySelectorAsync_UsesTenantForSubscription(string? tenant)
    {
        var handler = new RecordingHttpMessageHandler(
            CreateJsonResponse(ResourceGroupResponse),
            CreateJsonResponse(VirtualMachineResponse));
        (AttachedDiskService service, IAzureService azureService) = CreateService(handler, tenant);

        (string vmResourceId, string[]? diskResourceIds) = await service.ResolveFriendlySelectorAsync(
            SubscriptionId,
            ResourceGroup,
            "test-vm",
            ["os-disk"],
            tenant,
            TestContext.Current.CancellationToken);

        Assert.Equal(VmResourceId, vmResourceId, ignoreCase: true);
        Assert.NotNull(diskResourceIds);
        Assert.Equal([OsDiskResourceId], diskResourceIds);
        await azureService.Received(1).GetSubscription(
            SubscriptionId, tenant, Arg.Any<CancellationToken>());
    }

    private static (AttachedDiskService Service, IAzureService AzureService) CreateService(
        HttpMessageHandler handler,
        string? tenant)
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        var options = new ArmClientOptions
        {
            Transport = new HttpClientTransport(new HttpClient(handler))
        };
        var armClient = new ArmClient(credential, SubscriptionId, options);
        var subscriptionResource = armClient.GetSubscriptionResource(
            SubscriptionResource.CreateResourceIdentifier(SubscriptionId));
        var azureService = Substitute.For<IAzureService>();
        azureService.GetSubscription(SubscriptionId, tenant, Arg.Any<CancellationToken>())
            .Returns(subscriptionResource);
        return (new AttachedDiskService(azureService), azureService);
    }

    private static HttpResponseMessage CreateJsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

    private sealed class RecordingHttpMessageHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            return Task.FromResult(_responses.Dequeue());
        }
    }

    private const string ResourceGroupResponse = $$"""
        {
          "id": "/subscriptions/{{SubscriptionId}}/resourceGroups/{{ResourceGroup}}",
          "name": "{{ResourceGroup}}",
          "type": "Microsoft.Resources/resourceGroups",
          "location": "eastus",
          "properties": {
            "provisioningState": "Succeeded"
          }
        }
        """;

    private const string VirtualMachineResponse = $$"""
        {
          "id": "{{VmResourceId}}",
          "name": "test-vm",
          "type": "Microsoft.Compute/virtualMachines",
          "location": "eastus",
          "properties": {
            "storageProfile": {
              "osDisk": {
                "name": "os-disk",
                "createOption": "FromImage",
                "managedDisk": {
                  "id": "{{OsDiskResourceId}}"
                }
              },
              "dataDisks": [
                {
                  "lun": 0,
                  "name": "data-disk",
                  "createOption": "Attach",
                  "managedDisk": {
                    "id": "{{DataDiskResourceId}}"
                  }
                }
              ]
            }
          }
        }
        """;

    private const string VirtualMachineScaleSetResponse = $$"""
        {
          "id": "{{VmssResourceId}}",
          "name": "test-vmss",
          "type": "Microsoft.Compute/virtualMachineScaleSets",
          "location": "eastus",
          "properties": {
            "provisioningState": "Succeeded"
          }
        }
        """;

    private const string VirtualMachineScaleSetVmResponse = $$"""
        {
          "id": "{{VmssVmResourceId}}",
          "name": "instance-a",
          "type": "Microsoft.Compute/virtualMachineScaleSets/virtualMachines",
          "location": "eastus",
          "properties": {
            "storageProfile": {
              "osDisk": {
                "name": "os-disk",
                "createOption": "FromImage",
                "managedDisk": {
                  "id": "{{OsDiskResourceId}}"
                }
              },
              "dataDisks": [
                {
                  "lun": 0,
                  "name": "data-disk",
                  "createOption": "Attach",
                  "managedDisk": {
                    "id": "{{DataDiskResourceId}}"
                  }
                }
              ]
            }
          }
        }
        """;
}
