// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

global using System.CommandLine;
global using System.Net;
global using System.Text.Json;
global using Azure.Mcp.Core.Options;
global using Azure.Mcp.Tools.FileShares.Commands;
global using Azure.Mcp.Tools.FileShares.Commands.FileShare;
global using Azure.Mcp.Tools.FileShares.Commands.Informational;
global using Azure.Mcp.Tools.FileShares.Models;
global using Azure.Mcp.Tools.FileShares.Services;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Mcp.Core.Models.Command;
global using NSubstitute;
global using NSubstitute.ExceptionExtensions;
global using Xunit;
