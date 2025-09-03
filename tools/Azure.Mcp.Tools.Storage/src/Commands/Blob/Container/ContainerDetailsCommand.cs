// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

public sealed class ContainerDetailsCommand(ILogger<ContainerDetailsCommand> logger) : BaseContainerCommand<ContainerDetailsOptions>()
{
    private const string CommandTitle = "Get Storage Container Details";
    private readonly ILogger<ContainerDetailsCommand> _logger = logger;

    public override string Name => "details";

    public override string Description =>
        $"""
        Get detailed properties of a storage container including metadata, lease status, and access level.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var storageService = context.GetService<IStorageService>();
            var details = await storageService.GetContainerDetails(
                options.Account!,
                options.Container!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy
            );

            var result = new ContainerDetailsCommandResult(new JsonBlobContainerProperties(details));
            context.Response.Results = ResponseResult.Create(result, StorageJsonContext.Default.ContainerDetailsCommandResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting container details. Account: {Account}, Container: {Container}.", options.Account, options.Container);
            HandleException(context, ex);
            return context.Response;
        }
    }

    internal record ContainerDetailsCommandResult(JsonBlobContainerProperties Details);

    internal class JsonBlobContainerProperties
    {
        private readonly BlobContainerProperties props;

        internal JsonBlobContainerProperties(BlobContainerProperties props)
        {
            this.props = props;
        }

        public DateTimeOffset LastModified => props.LastModified;
        public LeaseStatus? LeaseStatus => props.LeaseStatus;
        public LeaseState? LeaseState => props.LeaseState;
        public LeaseDurationType? LeaseDuration => props.LeaseDuration;
        public PublicAccessType? PublicAccess => props.PublicAccess;
        public bool? HasImmutabilityPolicy => props.HasImmutabilityPolicy;
        public bool? HasLegalHold => props.HasLegalHold;
        public string DefaultEncryptionScope => props.DefaultEncryptionScope;
        public bool? PreventEncryptionScopeOverride => props.PreventEncryptionScopeOverride;
        public DateTimeOffset? DeletedOn => props.DeletedOn;
        public JsonETag ETag => new(props.ETag);
        public int? RemainingRetentionDays => props.RemainingRetentionDays;
        public IDictionary<string, string> Metadata => props.Metadata;
        public bool HasImmutableStorageWithVersioning => props.HasImmutableStorageWithVersioning;
    }

    [JsonConverter(typeof(ETagConverter))]
    internal struct JsonETag(ETag tag)
    {
        public void Write(Utf8JsonWriter writer)
        {
            var value = tag;
            if (value == default)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString("H"));
            }
        }
    }

    internal class ETagConverter : JsonConverter<JsonETag>
    {
        public override JsonETag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? s = reader.GetString();
            return new(s == null ? default : new ETag(s));
        }

        public override void Write(Utf8JsonWriter writer, JsonETag value, JsonSerializerOptions options)
        {
            value.Write(writer);
        }
    }
}
