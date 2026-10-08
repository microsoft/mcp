// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

export function buildServerArguments(config: { get<T>(section: string): T | undefined }): string[] {
    const args = ['server', 'start'];

    const mode = config.get<string>('serverMode') || 'namespace';
    args.push('--mode', mode);

    const enabledServices = config.get<string[]>('enabledServices');
    if (Array.isArray(enabledServices) && enabledServices.length > 0) {
        for (const service of enabledServices) {
            args.push('--namespace', service);
        }
    }

    if (config.get<boolean>('readOnly') === true) {
        args.push('--read-only');
    }

    const dangerouslyDisabledSsrfProtectionNamespaces =
        config.get<string[]>('dangerouslyDisableSsrfProtectionsByNamespace');
    if (Array.isArray(dangerouslyDisabledSsrfProtectionNamespaces)) {
        for (const toolNamespace of dangerouslyDisabledSsrfProtectionNamespaces) {
            args.push('--dangerously-disable-ssrf-protections-by-namespace', toolNamespace);
        }
    }

    const supportLogsDir = config.get<string>('dangerouslyWriteSupportLogsToDir');
    if (supportLogsDir && supportLogsDir.trim() !== '') {
        args.push('--dangerously-write-support-logs-to-dir', supportLogsDir);
    }

    return args;
}
