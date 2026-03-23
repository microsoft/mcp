// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Postgres.Validation;

/// <summary>
/// Validates PostgreSQL server parameter names against a strict allowlist of safe,
/// non-security-critical parameters. This prevents LLM agents or prompt-injected
/// attackers from weakening the security posture of PostgreSQL servers by modifying
/// parameters that control audit logging, encryption, authentication, or access rules.
/// </summary>
internal static class ServerParameterValidator
{
    /// <summary>
    /// Allowlist of PostgreSQL Flexible Server parameters that are safe to modify
    /// through the MCP tool. These are performance-tuning and operational parameters
    /// that do not affect the security posture of the server.
    /// </summary>
    private static readonly HashSet<string> AllowedParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        // Connection and resource limits
        "max_connections",
        "superuser_reserved_connections",
        "tcp_keepalives_idle",
        "tcp_keepalives_interval",
        "tcp_keepalives_count",

        // Memory settings
        "shared_buffers",
        "work_mem",
        "maintenance_work_mem",
        "effective_cache_size",
        "temp_buffers",

        // Query planning and execution
        "effective_io_concurrency",
        "random_page_cost",
        "seq_page_cost",
        "cpu_tuple_cost",
        "cpu_index_tuple_cost",
        "cpu_operator_cost",
        "default_statistics_target",
        "from_collapse_limit",
        "join_collapse_limit",
        "geqo",
        "geqo_threshold",

        // Write-ahead log (performance tuning only)
        "wal_buffers",
        "checkpoint_completion_target",
        "checkpoint_timeout",
        "max_wal_size",
        "min_wal_size",

        // Replication (read replicas)
        "max_replication_slots",
        "max_wal_senders",
        "hot_standby_feedback",

        // Autovacuum tuning
        "autovacuum",
        "autovacuum_max_workers",
        "autovacuum_naptime",
        "autovacuum_vacuum_threshold",
        "autovacuum_vacuum_scale_factor",
        "autovacuum_analyze_threshold",
        "autovacuum_analyze_scale_factor",
        "autovacuum_vacuum_cost_delay",
        "autovacuum_vacuum_cost_limit",

        // Logging (non-security - what to log, not whether to log)
        "log_min_duration_statement",
        "log_lock_waits",
        "log_temp_files",
        "log_autovacuum_min_duration",
        "deadlock_timeout",

        // Statement behavior
        "statement_timeout",
        "lock_timeout",
        "idle_in_transaction_session_timeout",
        "idle_session_timeout",

        // Locale and formatting
        "timezone",
        "datestyle",
        "intervalstyle",
        "lc_messages",
        "lc_monetary",
        "lc_numeric",
        "lc_time",
        "default_text_search_config",
        "client_encoding",

        // Parallel query
        "max_parallel_workers_per_gather",
        "max_parallel_workers",
        "max_parallel_maintenance_workers",
        "parallel_tuple_cost",
        "parallel_setup_cost",
        "min_parallel_table_scan_size",
        "min_parallel_index_scan_size",

        // Miscellaneous safe tuning
        "max_prepared_transactions",
        "max_locks_per_transaction",
        "max_pred_locks_per_transaction",
        "cursor_tuple_fraction",
        "enable_hashagg",
        "enable_hashjoin",
        "enable_indexscan",
        "enable_indexonlyscan",
        "enable_mergejoin",
        "enable_nestloop",
        "enable_seqscan",
        "enable_sort",
        "enable_tidscan",
        "enable_bitmapscan",
        "enable_material",
        "jit",
        "jit_above_cost",
        "jit_inline_above_cost",
        "jit_optimize_above_cost",
        "track_activities",
        "track_counts",
        "track_io_timing",
        "track_wal_io_timing",
        "max_stack_depth",
        "array_nulls",
        "backslash_quote",
        "escape_string_warning",
        "standard_conforming_strings",
        "synchronize_seqscans",
        "vacuum_cost_delay",
        "vacuum_cost_limit",
        "vacuum_cost_page_dirty",
        "vacuum_cost_page_hit",
        "vacuum_cost_page_miss",
        "vacuum_freeze_min_age",
        "vacuum_freeze_table_age",
        "vacuum_multixact_freeze_min_age",
        "vacuum_multixact_freeze_table_age",
        "bgwriter_delay",
        "bgwriter_lru_maxpages",
        "bgwriter_lru_multiplier",
        "max_files_per_process",
        "max_worker_processes",
        "old_snapshot_threshold",
    };

    /// <summary>
    /// Security-sensitive parameters that are explicitly blocked. This list is used
    /// to provide more informative error messages when a user tries to modify a
    /// known dangerous parameter.
    /// </summary>
    private static readonly Dictionary<string, string> BlockedParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        // Audit logging parameters - disabling these hides malicious activity
        ["log_connections"] = "Controls connection audit logging. Disabling this hides unauthorized access attempts.",
        ["log_disconnections"] = "Controls disconnection audit logging. Disabling this hides session tracking.",
        ["log_statement"] = "Controls which SQL statements are logged. Reducing this hides malicious queries.",
        ["logging_collector"] = "Controls whether the logging collector is enabled. Disabling removes audit trails.",
        ["log_destination"] = "Controls where logs are sent. Changing this can redirect or suppress audit logs.",
        ["log_directory"] = "Controls the log file directory. Changing this can redirect audit logs.",
        ["log_filename"] = "Controls the log file naming. Changing this can disrupt log collection.",
        ["log_file_mode"] = "Controls log file permissions. Weakening this exposes audit data.",
        ["log_rotation_age"] = "Controls log rotation. Manipulation can cause premature log deletion.",
        ["log_rotation_size"] = "Controls log rotation. Manipulation can cause premature log deletion.",
        ["log_truncate_on_rotation"] = "Controls log truncation. Enabling this destroys historical audit data.",

        // Encryption and TLS parameters
        ["password_encryption"] = "Controls password hashing algorithm. Downgrading to md5 weakens credential security.",
        ["ssl"] = "Controls whether SSL/TLS is enabled. Disabling exposes data in transit.",
        ["ssl_min_protocol_version"] = "Controls minimum TLS version. Downgrading enables protocol attacks.",
        ["ssl_max_protocol_version"] = "Controls maximum TLS version. Restricting can force weaker protocols.",
        ["ssl_cert_file"] = "Controls the server certificate file path.",
        ["ssl_key_file"] = "Controls the server private key file path.",
        ["ssl_ca_file"] = "Controls the CA certificate file path for client verification.",
        ["ssl_crl_file"] = "Controls the certificate revocation list file path.",
        ["ssl_ciphers"] = "Controls allowed TLS cipher suites. Weakening enables cryptographic attacks.",
        ["ssl_prefer_server_ciphers"] = "Controls cipher negotiation order.",
        ["ssl_ecdh_curve"] = "Controls ECDH curve for key exchange.",

        // Authentication parameters
        ["authentication_timeout"] = "Controls authentication timeout. Increasing enables brute-force attacks.",
        ["db_user_namespace"] = "Controls database user namespace behavior.",
        ["krb_server_keyfile"] = "Controls Kerberos keytab file location.",
        ["krb_caseins_users"] = "Controls Kerberos username case sensitivity.",

        // pg_hba.conf related and connection security
        ["pg_hba_file"] = "Controls the HBA configuration file path. Changing this alters authentication rules.",

        // Shared library loading (code execution)
        ["shared_preload_libraries"] = "Controls libraries loaded at server start. Can enable arbitrary code execution.",
        ["session_preload_libraries"] = "Controls libraries loaded per session. Can enable arbitrary code execution.",
        ["local_preload_libraries"] = "Controls locally loaded libraries. Can enable arbitrary code execution.",
        ["dynamic_library_path"] = "Controls library search path. Can redirect to malicious libraries.",

        // Data directory and file access
        ["data_directory"] = "Controls the data directory location.",
        ["config_file"] = "Controls the configuration file path.",
        ["hba_file"] = "Controls the HBA file path.",
        ["ident_file"] = "Controls the ident file path.",
        ["external_pid_file"] = "Controls the PID file location.",

        // Row-level security
        ["row_security"] = "Controls row-level security enforcement. Disabling bypasses access policies.",

        // Superuser-only parameters that affect security
        ["log_min_messages"] = "Controls logging verbosity. Reducing hides important security messages.",
        ["log_min_error_statement"] = "Controls which error statements are logged. Reducing hides SQL errors.",
    };

    /// <summary>
    /// Validates that the specified parameter name is on the allowlist of safe parameters.
    /// Throws <see cref="CommandValidationException"/> if the parameter is blocked or unknown.
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

        if (AllowedParameters.Contains(trimmed))
        {
            return;
        }

        if (BlockedParameters.TryGetValue(trimmed, out var reason))
        {
            throw new CommandValidationException(
                $"Parameter '{trimmed}' cannot be modified through this tool because it is security-sensitive. {reason}",
                HttpStatusCode.Forbidden);
        }

        throw new CommandValidationException(
            $"Parameter '{trimmed}' is not in the allowlist of safe parameters that can be modified through this tool. " +
            "Only performance-tuning and operational parameters are permitted. " +
            "Use the Azure Portal or Azure CLI directly to modify other parameters with appropriate review.",
            HttpStatusCode.Forbidden);
    }
}
