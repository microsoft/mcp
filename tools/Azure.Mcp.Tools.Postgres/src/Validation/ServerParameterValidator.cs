// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Postgres.Validation;

/// <summary>
/// Validates PostgreSQL server parameter names against a blocklist of security-sensitive
/// parameters. This prevents LLM agents or prompt-injected attackers from weakening the
/// security posture of PostgreSQL servers by modifying parameters that control audit
/// logging, encryption, authentication, or access rules.
/// Parameters not on the blocklist are allowed by default.
/// </summary>
internal static class ServerParameterValidator
{
    /// <summary>
    /// Security-sensitive parameters that are blocked from modification through this tool.
    /// Each entry maps a parameter name to a human-readable reason explaining why it is blocked.
    /// </summary>
    private static readonly Dictionary<string, string> BlockedParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        // Audit logging parameters - disabling these hides malicious activity
        ["log_connections"] = "Controls connection audit logging.",
        ["log_disconnections"] = "Controls disconnection audit logging.",
        ["log_statement"] = "Controls which SQL statements are logged.",
        ["logging_collector"] = "Controls whether the logging collector is enabled.",
        ["log_destination"] = "Controls where logs are sent.",
        ["log_directory"] = "Controls the log file directory.",
        ["log_filename"] = "Controls the log file naming.",
        ["log_file_mode"] = "Controls log file permissions.",
        ["log_rotation_age"] = "Controls log rotation timing.",
        ["log_rotation_size"] = "Controls log rotation sizing.",
        ["log_truncate_on_rotation"] = "Controls log truncation on rotation.",
        ["log_min_messages"] = "Controls logging verbosity level.",
        ["log_min_error_statement"] = "Controls which error statements are logged.",

        // Encryption and TLS parameters
        ["password_encryption"] = "Controls password hashing algorithm.",
        ["ssl"] = "Controls whether SSL/TLS is enabled.",
        ["ssl_min_protocol_version"] = "Controls minimum TLS version.",
        ["ssl_max_protocol_version"] = "Controls maximum TLS version.",
        ["ssl_cert_file"] = "Controls the server certificate file path.",
        ["ssl_key_file"] = "Controls the server private key file path.",
        ["ssl_ca_file"] = "Controls the CA certificate file path.",
        ["ssl_crl_file"] = "Controls the certificate revocation list file path.",
        ["ssl_ciphers"] = "Controls allowed TLS cipher suites.",
        ["ssl_prefer_server_ciphers"] = "Controls cipher negotiation order.",
        ["ssl_ecdh_curve"] = "Controls ECDH curve for key exchange.",

        // Authentication parameters
        ["authentication_timeout"] = "Controls authentication timeout.",
        ["db_user_namespace"] = "Controls database user namespace behavior.",
        ["krb_server_keyfile"] = "Controls Kerberos keytab file location.",
        ["krb_caseins_users"] = "Controls Kerberos username case sensitivity.",

        // pg_hba.conf related and connection security
        ["pg_hba_file"] = "Controls the HBA configuration file path.",

        // Shared library loading (code execution)
        ["shared_preload_libraries"] = "Controls libraries loaded at server start.",
        ["session_preload_libraries"] = "Controls libraries loaded per session.",
        ["local_preload_libraries"] = "Controls locally loaded libraries.",
        ["dynamic_library_path"] = "Controls library search path.",

        // Data directory and file access
        ["data_directory"] = "Controls the data directory location.",
        ["config_file"] = "Controls the configuration file path.",
        ["hba_file"] = "Controls the HBA file path.",
        ["ident_file"] = "Controls the ident file path.",
        ["external_pid_file"] = "Controls the PID file location.",

        // Row-level security
        ["row_security"] = "Controls row-level security enforcement.",
    };

    /// <summary>
    /// Validates that the specified parameter name is not on the blocklist of security-sensitive parameters.
    /// Parameters not on the blocklist are allowed by default.
    /// Throws <see cref="CommandValidationException"/> if the parameter is blocked.
    /// </summary>
    public static void EnsureParameterAllowed(string? parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
        {
            throw new CommandValidationException(
                "Parameter name cannot be empty.",
                HttpStatusCode.BadRequest);
        }

        var trimmed = parameterName.Trim();

        if (BlockedParameters.TryGetValue(trimmed, out var reason))
        {
            throw new CommandValidationException(
                $"Parameter '{trimmed}' cannot be modified through this tool because it is security-sensitive. {reason} " +
                "Use the Azure Portal or Azure CLI directly to modify this parameter with appropriate review.",
                HttpStatusCode.Forbidden);
        }
    }
}
