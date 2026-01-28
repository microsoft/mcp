// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Options;

namespace Microsoft.ModelContextProtocol.HttpServer.Distributed.Abstractions;

/// <summary>
/// Validator for <see cref="SessionAffinityOptions"/> that ensures configuration is valid.
/// Uses compile-time code generation for AOT compatibility.
/// The source generator will automatically validate data annotations on the options class.
/// </summary>
[OptionsValidator]
internal sealed partial class SessionAffinityOptionsValidator
    : IValidateOptions<SessionAffinityOptions> { }
