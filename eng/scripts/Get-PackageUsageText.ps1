param(
    [string] $BuildInfoPath,
    [string] $NpmRegistry
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$exitCode = 0


if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    exit 1
}

$OutputPath = "$RepoRoot/.work/integration-md"
Remove-Item $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
New-Item -ItemType Directory -Path $OutputPath | Out-Null

function Get-NpmHelpText {
    param(
        [hashtable] $Server
    )

    if ($NpmRegistry -match 'https://pkgs.dev.azure.com/(?<org>.+?)/(?<project>.+?)/_packaging/(?<feed>.+?)/npm/registry/')
    {
        $connectInstructions = "To connect to the feed, use the NPM connection instructions from:  `n" +
        "https://dev.azure.com/$($matches['org'])/$($matches['project'])/_artifacts/feed/$($matches['feed'])/connect`n`n"
    } else {
        $connectInstructions = ""
    }

    $serverName = $Server.name
    $package = $Server.npmPackageName
    $version = $Server.version
    $cliName = $Server.cliName

    $markdown = @"
$connectInstructions
To run the dev version of the package, you can use the following command:
``````bash
npx --yes --registry '$NpmRegistry' $package@$version --version
``````

You can also globally install the package and run it like:
``````bash
npm install --registry '$NpmRegistry' -g $package@$version

$cliName --version
``````

## mcp.json

Configure the server in ``.vscode/mcp.json`` with:
``````json
{
  "servers": {
    "$serverName": {
      "command": "npx",
      "args": [
        "-y",
        "--registry",
        "$NpmRegistry",
        "$package@$version",
        "server",
        "start"
      ]
    }
  }
}
``````
"@

    $attachmentName = "$serverName NPM"
    $file = Join-Path $OutputPath "$serverName-npm.md"
    Set-Content -Path $file -Value $markdown -Encoding utf8
    Write-Host "Uploading summary from $file as '$attachmentName'."
    Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=$attachmentName;]$file"
}

$buildInfo = Get-Content $BuildInfoPath | ConvertFrom-Json -AsHashtable
foreach ($server in $buildInfo.servers) {
    try {
        if ($server.npmPackageName) {
            Get-NpmHelpText -Server $server
        }
    } catch {
        LogError "Failed to get npm help text for server $($server.name): $_"
        $exitCode = 1
    }
}

exit $exitCode
