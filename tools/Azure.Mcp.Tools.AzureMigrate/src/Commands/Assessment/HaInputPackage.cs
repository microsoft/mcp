// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.Assessment;

/// <summary>
/// Builds and uploads the HA input "mini zip": a zip holding only manifest.json with the read SAS URLs of the inventory and the
/// assessment output. The HA skill downloads the files itself, so azmcp never transfers them. SAS query strings are never logged.
/// </summary>
internal static class HaInputPackage
{
    internal const string ManifestEntryName = "manifest.json";
    internal const int MaxUploadAttempts = 3;
    internal static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(1);

    /// <summary>Returns the mini zip bytes with manifest.json { inventorySasUri, assessmentSasUri }.</summary>
    internal static byte[] Build(HaInput input)
    {
        using MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            ZipArchiveEntry entry = archive.CreateEntry(ManifestEntryName, CompressionLevel.Optimal);
            using Stream entryStream = entry.Open();
            using Utf8JsonWriter writer = new(entryStream);
            writer.WriteStartObject();
            writer.WriteString("inventorySasUri", input.InventorySasUri);
            writer.WriteString("assessmentSasUri", input.AssessmentSasUri);
            writer.WriteEndObject();
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Uploads the mini zip to the HA write SAS URL as a block blob in one PUT. The PUT overwrites, so throttling, server and network
    /// errors are retried up to <see cref="MaxUploadAttempts"/> times. Returns the last HTTP status (null on a network error) and an
    /// error, which is null on success.
    /// </summary>
    internal static async Task<(int? Status, string? Error)> UploadAsync(
        HttpClient http,
        string writeSasUrl,
        byte[] zip,
        ILogger logger,
        Func<TimeSpan, CancellationToken, Task> delayAsync,
        CancellationToken cancellationToken)
    {
        string logUrl = writeSasUrl.Split('?')[0];
        int? status = null;
        string? error = null;
        for (int attempt = 0; attempt < MaxUploadAttempts; attempt++)
        {
            if (attempt > 0)
            {
                await delayAsync(InitialRetryDelay * Math.Pow(2, attempt - 1), cancellationToken);
            }

            try
            {
                using HttpRequestMessage request = new(HttpMethod.Put, writeSasUrl) { Content = new ByteArrayContent(zip) };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                request.Headers.Add("x-ms-blob-type", "BlockBlob");

                using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
                status = (int)response.StatusCode;
                logger.LogInformation("AzureMigrate HA: upload input {Url} returned {Status} (attempt {Attempt}).", logUrl, status, attempt + 1);
                if (response.IsSuccessStatusCode)
                {
                    return (status, null);
                }

                string body = await response.Content.ReadAsStringAsync(cancellationToken);
                error = $"Uploading the HA input failed (HTTP {status}): {(body.Length > 500 ? body[..500] : body)}";
                if (!IsTransient(response.StatusCode))
                {
                    return (status, error);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                status = null;
                error = $"Uploading the HA input failed: {ex.GetType().Name}: {ex.Message}";
                logger.LogWarning("AzureMigrate HA: upload input {Url} failed: {Error} (attempt {Attempt}).", logUrl, ex.Message, attempt + 1);
            }
        }

        return (status, error);
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests || (int)status >= 500;
}
