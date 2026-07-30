// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

/// <summary>
/// Deterministic intent-to-namespace resolver used as a fallback when MCP sampling
/// is not available (e.g. 2026-07-28 stateless clients).
///
/// Scope: stage-1 only — namespace selection.
/// The result always feeds into learn-mode (<c>ToolLearnModeAsync</c>), never directly
/// into <c>CommandModeAsync</c>. Wrong namespace picks are self-correcting: the LLM
/// receives the wrong namespace's command list, recognises the mismatch, and retries.
/// </summary>
internal static class DeterministicToolResolution
{
    /// <summary>
    /// Minimum token-overlap score required to commit to a namespace.
    /// Below this threshold <see cref="ResolveNamespace"/> returns <c>null</c> and
    /// the caller falls back to returning the full namespace list (root learn-mode).
    /// </summary>
    private const double ConfidenceThreshold = 0.25;

    /// <summary>
    /// Common words that carry no domain signal and should be excluded from scoring.
    /// Includes generic verbs, pronouns, Azure's own brand name (appears in every namespace
    /// description so it discriminates nothing), and other noise words.
    /// </summary>
    private static readonly HashSet<string> s_stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        // Articles / determiners
        "the", "a", "an",
        // Pronouns
        "i", "my", "me", "we", "us", "our", "you", "your", "they", "them", "their",
        "it", "its", "this", "that", "these", "those", "there", "here",
        // Auxiliary verbs
        "is", "are", "am", "be", "was", "were", "will", "would", "should", "could",
        "do", "does", "did", "have", "has", "had", "can",
        // Common command verbs (no domain signal)
        "show", "list", "get", "set", "create", "delete", "update", "add", "remove",
        "find", "check", "see", "tell", "let", "give", "use", "using",
        // Question / interrogative words
        "what", "how", "which", "who", "where", "when", "why",
        // Prepositions / conjunctions
        "in", "of", "to", "at", "on", "from", "with", "by", "for", "or", "and",
        "not", "no", "about", "into", "out", "all", "any", "some", "more", "much",
        // Miscellaneous
        "please", "want", "need", "now", "just", "via",
        // Azure brand (appears in every namespace description – no discriminating value)
        "azure", "microsoft",
    };

    /// <summary>
    /// Synonym rules mapping common user terms to namespace-level tokens.
    /// Each entry is (trigger words, expansion tokens added to the intent token set).
    /// Rules are checked against individual intent tokens; the first trigger match wins.
    /// </summary>
    private static readonly (string[] Triggers, string[] Expansions)[] s_synonymRules =
    [
        (["vm", "vms", "virtualmachine", "virtual", "instance", "instances"], ["compute", "virtualmachine"]),
        (["kv", "vault", "secret", "secrets", "certificate", "certificates", "cert", "certs"], ["keyvault", "secret", "key"]),
        (["blob", "blobs", "bucket", "datalake", "adls"], ["storage", "blob"]),
        (["vnet", "subnet", "nsg", "firewall", "publicip", "loadbalancer"], ["network", "nsg", "vnet"]),
        (["db", "database", "databases", "mssql", "sqlserver"], ["sql", "database"]),
        (["k8s", "kubernetes", "cluster", "clusters", "pod", "pods"], ["aks", "cluster"]),
        (["acr", "registry", "docker", "image", "images"], ["acr", "registry"]),
        (["budget", "spending", "cost", "bill", "billing", "charges", "invoice"], ["costmanagement", "budget"]),
        (["role", "rbac", "permission", "permissions", "assignment", "assignments"], ["authorization", "role"]),
        (["alert", "metric", "metrics", "diagnostic"], ["monitor", "applicationinsights"]),
        (["function", "functions", "serverless", "func"], ["functionapp", "function"]),
        (["webapp", "webapps", "site", "sites"], ["appservice", "webapp"]),
        (["resourcegroup", "rg"], ["resourcegroup"]),
        (["subscription", "subscriptions"], ["subscription"]),
        (["advisor", "recommendation", "recommendations"], ["advisor"]),
        (["openai", "gpt", "cognitive", "cognitiveservices"], ["openai", "cognitiveservices"]),
        (["backup", "backups", "restore"], ["azurebackup", "backup"]),
        (["migrate", "migration", "assessment"], ["azuremigrate"]),
        (["terraform", "bicep"], ["azureterraform"]),
        (["appconfig", "featureflag"], ["appconfig"]),
    ];

    /// <summary>
    /// Resolves the most relevant namespace from a user intent string.
    /// </summary>
    /// <param name="intent">The user's natural-language intent.</param>
    /// <param name="namespaces">Available namespace tools (name + description).</param>
    /// <returns>
    /// The name of the best-matching namespace, or <c>null</c> if no namespace
    /// exceeds <see cref="ConfidenceThreshold"/>.
    /// </returns>
    public static string? ResolveNamespace(string intent, IReadOnlyList<Tool> namespaces)
    {
        if (string.IsNullOrWhiteSpace(intent) || namespaces.Count == 0)
            return null;

        var intentTokens = Tokenize(intent);

        // Strip noise words BEFORE measuring the denominator so that intents like
        // "give me my vms" don't dilute the signal with "give", "me", "my".
        intentTokens.ExceptWith(s_stopwords);
        int meaningfulCount = intentTokens.Count;
        if (meaningfulCount == 0)
            return null;

        // Expand with synonyms AFTER recording meaningfulCount so that the added
        // expansion tokens (e.g. "compute" appended for trigger "vm") do not inflate
        // the denominator and depress the overlap score.
        ExpandWithSynonyms(intentTokens);

        double bestScore = 0;
        string? bestName = null;

        foreach (var ns in namespaces)
        {
            var nsTokens = Tokenize(ns.Name + " " + (ns.Description ?? string.Empty));
            double score = OverlapScore(intentTokens, nsTokens, meaningfulCount);

            if (score > bestScore)
            {
                bestScore = score;
                bestName = ns.Name;
            }
        }

        return bestScore >= ConfidenceThreshold ? bestName : null;
    }

    private static HashSet<string> Tokenize(string text)
    {
        var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int start = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            bool boundary = i == text.Length || !char.IsLetterOrDigit(text[i]);
            if (boundary)
            {
                if (i > start)
                {
                    ReadOnlySpan<char> slice = text.AsSpan(start, i - start);
                    if (slice.Length > 1)
                        tokens.Add(slice.ToString().ToLowerInvariant());
                }
                start = i + 1;
            }
        }

        return tokens;
    }

    private static void ExpandWithSynonyms(HashSet<string> tokens)
    {
        List<string>? toAdd = null;

        foreach (var (triggers, expansions) in s_synonymRules)
        {
            foreach (var trigger in triggers)
            {
                if (tokens.Contains(trigger))
                {
                    toAdd ??= [];
                    toAdd.AddRange(expansions);
                    break;
                }
            }
        }

        if (toAdd != null)
        {
            foreach (var t in toAdd)
                tokens.Add(t);
        }
    }

    private static double OverlapScore(HashSet<string> intentTokens, HashSet<string> namespaceTokens, int denominator)
    {
        if (denominator == 0)
            return 0;

        int overlap = 0;
        foreach (var token in intentTokens)
        {
            if (namespaceTokens.Contains(token))
                overlap++;
        }

        return (double)overlap / denominator;
    }
}
