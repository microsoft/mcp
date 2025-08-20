# cSpell:ignore nusing

$RepositoryRoot = $PSScriptRoot
$workPath = "$RepositoryRoot\.work"

$ErrorActionPreference = "Stop"

$namespaceChanges = @{}

function ResetAndInitialize {
    git reset
    git add .\convert-repo.ps1
    git restore .
    git clean -xdf

    # Move all existing source into a temporary directory ".work"
    if(Test-Path $workPath) {
        Write-Host "Temporary work directory already exists at $workPath. Removing it..." -ForegroundColor Yellow
        Remove-Item $workPath -Recurse -Force
    }

    New-Item -ItemType Directory -Path $workPath | Out-Null
}

function MigrateAzureMcpServer {
    $projectPath = '.\Directory.Build.props'
    [xml]$project = Get-Content -Path $projectPath -Raw
    $versionElement = $project.Project.PropertyGroup.ChildNodes | Where-Object { $_.Name -eq 'Version' } | Select-Object -First 1
    $version = $versionElement.InnerText.Trim()
    $versionElement.ParentNode.RemoveChild($versionElement) | Out-Null
    $project.Save($projectPath)

    $projectPath = 'core\src\AzureMcp.Cli\AzureMcp.Cli.csproj'
    [xml]$project = Get-Content -Path $projectPath -Raw
    $firstPropertyGroup = $project.Project.PropertyGroup | Select-Object -First 1
    $firstPropertyGroup.PrependChild($project.CreateElement('Version')).InnerText = $version
    $project.Save($projectPath)

    # fix relative paths from ".\servers\Azure.Mcp.Server\src"
    $content = Get-Content -Path $projectPath -Raw
    $content = $content.Replace('"..\AzureMcp.Core\AzureMcp.Core.csproj"', '"..\..\..\core\Azure.Mcp.Core\src\Azure.Mcp.Core.csproj"')
    $content = $content.Replace('"..\..\..\areas\*\src\**\AzureMcp.*.csproj"', '"..\..\..\tools\Azure.*\src\*.csproj"')
    $content = $content.Replace('"..\..\..\areas\', '"..\..\..\tools\')
    Set-Content -Path $projectPath -Value $content

    New-Item -ItemType Directory -Path ".\servers\Azure.Mcp.Server\src" -Force | Out-Null
    Move-Item -Path $projectPath -Destination "core\src\AzureMcp.Cli\Azure.Mcp.Server.csproj" -Force
    Move-Item -Path ".\core\src\AzureMcp.Cli\*" -Destination ".\servers\Azure.Mcp.Server\src" -Force
}

function MigrateAzureMcpCore {
    $projectPath = 'core\src\AzureMcp.Core\AzureMcp.Core.csproj'
    [xml]$project = Get-Content -Path $projectPath -Raw
    $projectReferenceItemGroup = $project.Project.AppendChild($project.CreateElement('ItemGroup'))
    $projectReference = $projectReferenceItemGroup.AppendChild($project.CreateElement('ProjectReference'))
    $projectReference.SetAttribute('Include', '..\..\Microsoft.Mcp.Core\src\Microsoft.Mcp.Core.csproj')
    $project.Save($projectPath)

    New-Item -ItemType Directory -Path ".\core\Azure.Mcp.Core\src" -Force | Out-Null
    New-Item -ItemType Directory -Path ".\core\Azure.Mcp.Core\tests" -Force | Out-Null
    Move-Item -Path ".\core\src\AzureMcp.Core\*" -Destination ".\core\Azure.Mcp.Core\src" -Force
    Move-Item -Path ".\core\tests\*" -Destination ".\core\Azure.Mcp.Core\tests" -Force
}

function MigrateAreas {
    $areaDirectories = Get-ChildItem -Path ".\areas" -Directory
    foreach ($areaDirectory in $areaDirectories) {
        try {
            $areaProject = Get-ChildItem -Path "$areaDirectory\src\" -Filter "*.csproj" -Recurse | Select-Object -First 1
            $projectFileName = $areaProject.Name
            $areaName = $projectFileName.Replace('.csproj', '')
            $toolName = $areaName.Replace("AzureMcp.", "Azure.Mcp.Tools.")
            New-Item -ItemType Directory -Path ".\tools\$toolName" -Force | Out-Null
            Move-Item -Path $areaProject.Directory -Destination ".\tools\$toolName\src" -Force
            Move-Item -Path "$areaDirectory\tests" -Destination ".\tools\$toolName\tests" -Force
        }
        catch {
            Write-Host "Failed to migrate area $($areaDirectory.Name): $_" -ForegroundColor Red
            throw
        }
    }
}

function FixProjectFileNames {
    $projectFiles = Get-ChildItem -Path $RepositoryRoot -Filter "*.csproj" -Recurse
    foreach ($projectFile in $projectFiles) {
        $content = Get-Content -Path $projectFile.FullName -Raw
        $newContent = $content -replace 'AzureMcp.Cli', 'AzureMcp.Server'
        if($newContent -ne $content) {
            Set-Content -Path $projectFile.FullName -Value $newContent
        }

        $oldName = $projectFile.Name
        $newName = $oldName.Replace(".csproj", "")
        $newName = $newName.Replace("AzureMcp.", "Azure.Mcp.")

        if($projectFile.FullName.Contains("\tools\")) {
            $newName = $newName.Replace("Azure.Mcp.", "Azure.Mcp.Tools.")
        }

        if($newName -ne $oldName) {
            $newPath = Join-Path -Path $projectFile.DirectoryName -ChildPath "$newName.csproj"
            Move-Item -Path $projectFile.FullName -Destination $newPath -Force
            $namespaceChanges[$oldName] = $newName
        }

        # if the project isn't in a src folder, its folder should match the project file name
        $directoryName = $projectFile.Directory.Name
        if ($directoryName -ne 'src' -and $directoryName -ne $newName) {
            $newFullName = Join-Path -Path $projectFile.Directory.Parent.FullName -ChildPath $newName
            Move-Item $projectFile.DirectoryName -Destination $newFullName -Force
        }
    }
}

function UpdateCodeFiles {
    $excludeNamespaces = @('AzureMcp', 'Azure.Mcp')
    $fileNamespaces = @{}

    $projects = Get-ChildItem -Path $RepositoryRoot -Filter "*.csproj" -Recurse
        | Where-Object { $_.FullName -notlike '*\eng\*' }

    foreach ($project in $projects) {
        $projectDirectory = Split-Path -Path $project.FullName -Parent
        $codeFiles = Get-ChildItem -Path $projectDirectory -Recurse -Include '*.cs'

        if($codeFiles.Count -eq 0) {
            Write-Host "No code files found in $projectDirectory." -ForegroundColor Yellow
            $projects = $projects | Where-Object { $_ -ne $project }
            continue;
        }

        $rootNamespace = $project.Name.Replace('.csproj', '')
        if($project -like '*\tools\*') {
            $oldNamespace = $rootNamespace.Replace("Azure.Mcp.Tools.", "AzureMcp.")
        } else {
            $oldNamespace =$rootNamespace.Replace("Azure.Mcp.", "AzureMcp.")
        }

        $namespaceChanges[$oldNamespace] = $rootNamespace

        foreach ($file in $codeFiles) {
            if ($file.DirectoryName -eq $projectDirectory) {
                # If the file is in the root of the project, use the root namespace directly
                $newNamespace = $rootNamespace
            } else {
                # Otherwise, calculate the relative namespace based on the project directory
                $relativeNamespace = $file.DirectoryName.Substring($projectDirectory.Length + 1).Replace('\', '.')
                $newNamespace = "$rootNamespace.$relativeNamespace"
            }

            $content = Get-Content -Path $file.FullName -Raw
            $newContent = $content.Replace('using AzureMcp;', 'using Azure.Mcp;')
            $newContent = $newContent -replace 'namespace AzureMcp(\s*[;\{])', 'namespace Azure.Mcp$1'

            # Replace core namespaces (AzureMcp.Commands -> AzureMcp.Core.Commands, etc.)
            if ($newContent -match '\n( *)namespace\s+([\w\.]+)') {
                $currentNamespace = $Matches[2]

                if ($currentNamespace -ne $newNamespace -and $currentNamespace -notin $excludeNamespaces) {
                    $existingChange = $namespaceChanges[$currentNamespace]
                    if($existingChange -and $existingChange -ne $newNamespace) {
                        Write-Host "Namespace $currentNamespace already mapped to $existingChange. Overriding with $newNamespace." -ForegroundColor Yellow
                    }

                    $namespaceChanges[$currentNamespace] = $newNamespace
                    $fileNamespaces[$file.FullName] = $newNamespace
                }
            }

            $newContent = $newContent.TrimEnd(@("`n", "`r"))
            if ($newContent -ne $content) {
                Set-Content -Path $file.FullName -Value $newContent
            }
        }
    }

    foreach ($current in ($namespaceChanges.Keys | Sort-Object { $_ })) {
        $new = $namespaceChanges[$current]
        Write-Host "$current -> $new" -ForegroundColor Cyan
    }

    $sortedKeys = $namespaceChanges.Keys | Sort-Object { $_.Length } -Descending

    # apply namespace changes to all code files
    foreach ($project in $projects) {
        $projectDirectory = Split-Path -Path $project.FullName -Parent
        $codeFiles = Get-ChildItem -Path $projectDirectory -Recurse -Include '*.cs'

        foreach ($file in $codeFiles) {
            $content = Get-Content -Path $file.FullName -Raw
            $newContent = $content
            foreach ($oldNamespace in $sortedKeys) {
                $newNamespace = $namespaceChanges[$oldNamespace]
                $newContent = $newContent.Replace($oldNamespace, $newNamespace)
            }

            # if the file references IAreaSetup, it should have a using statement for AzureMcp.Core.Areas
            # if ($file.Name -ne 'IAreaSetup.cs' -and $newContent -match 'IAreaSetup') {
            #     $usingStatement = "using Microsoft.Mcp.Core.Areas;"

            #     if (-not $newContent.Contains($usingStatement)) {
            #         #Add a using statement before the namespace declaration
            #         $newContent = $newContent -replace '\n( *namespace )', "$usingStatement`n`n`$1"
            #     }
            # }

            $newNamespace = $fileNamespaces[$file.FullName]
            if($newNamespace) {
                $newContent = $newContent -replace '(\n *)namespace +([\w\.]+)', "`$1namespace $newNamespace"
            }

            $newContent = $newContent.TrimEnd(@("`n", "`r"))
            if ($newContent -ne $content) {
                Set-Content -Path $file.FullName -Value $newContent
            }
        }
    }
}

function RemoveEmptyDirectories {
    while($true)
    {
        $emptyDirectories = Get-ChildItem -Path $RepositoryRoot -Directory -Recurse | Where-Object { !(Get-ChildItem $_) }
        $emptyDirectories | Remove-Item
        if($emptyDirectories.Count -eq 0) {
            break;
        }
    }
}

Push-Location $RepositoryRoot
try {
    ResetAndInitialize

    # Create core/ projects
    MigrateAzureMcpCore

    # Create server/ projects
    MigrateAzureMcpServer

    MigrateAreas

    FixProjectFileNames

    UpdateCodeFiles

    RemoveEmptyDirectories

    .\eng\scripts\Fix-ProjectReferences.ps1

    .\eng\scripts\Update-Solution.ps1 -Flat

    Write-Host "Migration done" -ForegroundColor Green
}
finally {
    Pop-Location
}
