// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Trivial change to trigger PR pipeline for live test validation
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Azure.Mcp.Tools.FunctionApp.UnitTests")]
[assembly: InternalsVisibleTo("Azure.Mcp.Tools.FunctionApp.LiveTests")]
