// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Storage.Options.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob;

public sealed class BlobDetailsCommand(ILogger<BlobDetailsCommand> logger) : BaseBlobCommand<BlobDetailsOptions>()
{
    private const string CommandTitle = "Get Storage Blob Details";
    private readonly ILogger<BlobDetailsCommand> _logger = logger;

    public override string Name => "details";

    public override string Description =>
        $"""
        Get blob properties, metadata, and general information. This tool retrieves blob configuration including metadata properties,
        approximate size, and last modification time information. Returns blob properties as JSON.
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
            var details = await storageService.GetBlobDetails(
                options.Account!,
                options.Container!,
                options.Blob!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy
            );

            var result = new BlobDetailsCommandResult(new JsonBlobProperties(details));
            context.Response.Results = ResponseResult.Create(result, StorageJsonContext.Default.BlobDetailsCommandResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting blob details. Account: {Account}, Container: {Container}, Blob: {Blob}.", options.Account, options.Container, options.Blob);
            HandleException(context, ex);
            return context.Response;
        }
    }

    internal record BlobDetailsCommandResult(JsonBlobProperties Details);

    internal class JsonBlobProperties
    {
        private readonly BlobProperties props;

        internal JsonBlobProperties(BlobProperties props)
        {
            this.props = props;
        }

        public DateTimeOffset LastModified => props.LastModified;
        public long ContentLength => props.ContentLength;
        public string ContentType => props.ContentType;
        public string ContentEncoding => props.ContentEncoding;
        public string ContentLanguage => props.ContentLanguage;
        public byte[]? ContentHash => props.ContentHash;
        public string ContentDisposition => props.ContentDisposition;
        public string CacheControl => props.CacheControl;
        public long BlobSequenceNumber => props.BlobSequenceNumber;
        public BlobType BlobType => props.BlobType;
        public LeaseStatus LeaseStatus => props.LeaseStatus;
        public LeaseState LeaseState => props.LeaseState;
        public LeaseDurationType LeaseDuration => props.LeaseDuration;
        public string CopyId => props.CopyId;
        public CopyStatus? CopyStatus => props.CopyStatus;
        public Uri CopySource => props.CopySource;
        public string CopyProgress => props.CopyProgress;
        public DateTimeOffset? CopyCompletedOn => props.CopyCompletedOn;
        public string CopyStatusDescription => props.CopyStatusDescription;
        public bool? IsIncrementalCopy => props.IsIncrementalCopy;
        public string DestinationSnapshot => props.DestinationSnapshot;
        public AccessTier? AccessTier => props.AccessTier;
        public DateTimeOffset? AccessTierChangedOn => props.AccessTierChangedOn;
        public BlobImmutabilityPolicy ImmutabilityPolicy => props.ImmutabilityPolicy;
        public bool? HasLegalHold => props.HasLegalHold;
        public IDictionary<string, string> Metadata => props.Metadata;
        public string ETag => props.ETag.ToString();
        public DateTimeOffset? CreatedOn => props.CreatedOn;
        public string ArchiveStatus => props.ArchiveStatus;
        public string EncryptionKeySha256 => props.EncryptionKeySha256;
        public bool? IsServerEncrypted => props.IsServerEncrypted;
        public string EncryptionScope => props.EncryptionScope;
        public string VersionId => props.VersionId;
        public string TagCount => props.TagCount.ToString();
    }
}
