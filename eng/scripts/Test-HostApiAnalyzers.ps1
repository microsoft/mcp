# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.
# cspell:ignore nologo bincore fullpaths nostdlib analyzerconfig additionalfile

[CmdletBinding()]
param([string] $DotnetPath = 'dotnet')

$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$project = Join-Path $repoRoot 'core\Microsoft.Mcp.Core\src\Microsoft.Mcp.Core.csproj'
$metadataJson = & $DotnetPath msbuild $project -target:ResolveReferences `
    -getProperty:MSBuildToolsPath,TargetPath -getItem:ReferencePath,Analyzer,AdditionalFiles -nologo -verbosity:quiet
if ($LASTEXITCODE -ne 0) {
    throw 'Unable to resolve compiler inputs. Build Microsoft.Mcp.Core before running this check.'
}
$metadata = ($metadataJson -join "`n") | ConvertFrom-Json
$analyzers = @($metadata.Items.Analyzer | Where-Object Identity -Match 'BannedApiAnalyzers')
if ($analyzers.Count -eq 0 -or -not (Test-Path -LiteralPath $metadata.Properties.TargetPath)) {
    throw 'Banned API analyzers or the core assembly are missing. Build Microsoft.Mcp.Core before running this check.'
}

$testRoot = Join-Path $repoRoot ".work\host-api-analyzer-tests\$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
try {
    $responsePath = Join-Path $testRoot 'compiler.rsp'
    $compiler = Join-Path $metadata.Properties.MSBuildToolsPath 'Roslyn\bincore\csc.dll'
    $arguments = @(
        '-nologo', '-fullpaths', '-target:library', '-nostdlib+', '-nullable:enable'
        "-out:`"$(Join-Path $testRoot 'Probe.dll')`""
        "-analyzerconfig:`"$(Join-Path $repoRoot '.editorconfig')`""
        "-reference:`"$($metadata.Properties.TargetPath)`""
        $metadata.Items.ReferencePath | ForEach-Object { "-reference:`"$($_.Identity)`"" }
        $analyzers | ForEach-Object { "-analyzer:`"$($_.Identity)`"" }
        $metadata.Items.AdditionalFiles | ForEach-Object { "-additionalfile:`"$($_.Identity)`"" }
    )
    Set-Content -LiteralPath $responsePath -Value $arguments

    $probes = [ordered]@{
        FileRead = '_ = File.ReadAllText("file");'
        FullyQualifiedFileWrite = 'System.IO.File.WriteAllText("file", "content");'
        DirectoryProbe = '_ = Directory.Exists("directory");'
        FileStreamConstructor = 'using var stream = new FileStream("file", FileMode.Open);'
        FileStreamOptionsConstructor = 'using var stream = new FileStream("file", new FileStreamOptions());'
        FileInfoConstructor = '_ = new FileInfo("file");'
        DirectoryInfoConstructor = '_ = new DirectoryInfo("directory");'
        DriveInfoConstructor = '_ = new DriveInfo("drive");'
        ReaderPathConstructor = 'using var reader = new StreamReader("file");'
        ReaderOptionsConstructor = 'using var reader = new StreamReader("file", new FileStreamOptions());'
        WriterPathConstructor = 'using var writer = new StreamWriter("file", true);'
        WriterOptionsConstructor = 'using var writer = new StreamWriter("file", new FileStreamOptions());'
        TemporaryPath = '_ = Path.GetTempPath();'
        TemporaryFile = '_ = Path.GetTempFileName();'
        EnvironmentRead = '_ = Environment.GetEnvironmentVariable("TOKEN");'
        EnvironmentTargetRead = '_ = Environment.GetEnvironmentVariable("TOKEN", EnvironmentVariableTarget.Process);'
        EnvironmentEnumeration = '_ = Environment.GetEnvironmentVariables();'
        EnvironmentTargetEnumeration = '_ = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);'
        EnvironmentWrite = 'Environment.SetEnvironmentVariable("TOKEN", "value");'
        EnvironmentTargetWrite = 'Environment.SetEnvironmentVariable("TOKEN", "value", EnvironmentVariableTarget.Process);'
        EnvironmentExpansion = '_ = Environment.ExpandEnvironmentVariables("%TOKEN%");'
        SpecialFolder = '_ = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);'
        SpecialFolderOption = '_ = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.None);'
        InstanceProcessStart = 'using var process = new Process(); _ = process.Start();'
        StaticProcessStart = '_ = Process.Start("executable");'
        ProcessStartArguments = '_ = Process.Start("executable", "arguments");'
        ProcessStartArgumentList = '_ = Process.Start("executable", new[] { "argument" });'
        ProcessStartInfo = '_ = Process.Start(new ProcessStartInfo("executable"));'
        FileSystemAdapter = '_ = new Microsoft.Mcp.Core.Services.Host.FileSystem(null!);'
        EnvironmentAdapter = '_ = new Microsoft.Mcp.Core.Services.Host.EnvironmentVariables(null!);'
        ProcessAdapter = '_ = new Microsoft.Mcp.Core.Services.Host.ProcessService(null!);'
    }
    $sources = foreach ($probe in $probes.GetEnumerator()) {
        $source = Join-Path $testRoot "$($probe.Key).cs"
        Set-Content -LiteralPath $source -Value @"
using System;
using System.IO;
using System.Diagnostics;
internal static class Probe_$($probe.Key) { internal static void Run() { $($probe.Value) } }
"@
        $source
    }
    $diagnostics = (& $DotnetPath $compiler "@$responsePath" @sources 2>&1) -join "`n"
    if ($LASTEXITCODE -eq 0) {
        throw 'Direct host API probes unexpectedly compiled successfully.'
    }
    if ($diagnostics -match ': error (?!RS0030\b)') {
        throw "Unexpected compiler errors in the host API probes.`n$diagnostics"
    }
    foreach ($source in $sources) {
        if ($diagnostics -notmatch "$([regex]::Escape($source))\(\d+,\d+\): error RS0030") {
            throw "Expected an RS0030 error for $source.`n$diagnostics"
        }
    }

    $allowedSource = Join-Path $testRoot 'InjectedHostAccess.cs'
    Set-Content -LiteralPath $allowedSource -Value @'
using System;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Mcp.Core.Services.Host;
internal static class InjectedHostAccess
{
    internal static void Run(IFileSystem files, IEnvironmentVariables environment, IProcessService processes, Process process)
    {
        using var stream = files.OpenRead("file");
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(new MemoryStream());
        _ = environment.GetEnvironmentVariable("TOKEN");
        _ = processes.Start(process);
        _ = Path.Combine("directory", "file");
        _ = Environment.NewLine;
    }
    [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs", Justification = "Startup-only fixture.")]
    internal static bool Startup() => File.Exists("host-configuration");
}
'@
    $diagnostics = (& $DotnetPath $compiler "@$responsePath" $allowedSource 2>&1) -join "`n"
    if ($LASTEXITCODE -ne 0) {
        throw "Injected host access, stream handling, or justified startup access was rejected.`n$diagnostics"
    }
    Write-Host "Passed: $($probes.Count) direct host API/adapter probes rejected; injected operations and justified startup access allowed."
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
