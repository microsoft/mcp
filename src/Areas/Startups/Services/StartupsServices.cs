// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace AzureMcp.Areas.Startups.Services
{
    public sealed class StartupsServices : IStartupsServices
    {
        public Task<StartupsGuidanceInfo> GetGuidanceAsync()
        {
            var info = new StartupsGuidanceInfo(
                Title: "Microsoft for Startups Guidance",
                Description: "Microsoft for Startups is a global program that helps startups succeed with access to technology, coaching, and support. Startups receive free Azure credits, technical resources, expert guidance, and opportunities to connect with Microsoft partners and customers. Learn more and apply at the website.",
                Link: "https://startups.microsoft.com/"
            );
            return Task.FromResult(info);
        }
    }
}
