# Fix Project References Script
# This script discovers all .csproj files, maps them by filename, and corrects all project references

Write-Host "Fixing project references in Azure MCP repository..." -ForegroundColor Green

# Step 1: Discover all .csproj files and create a filename to path mapping
Write-Host "Discovering all .csproj files..." -ForegroundColor Yellow
$projectMap = @{}
$allCsprojFiles = Get-ChildItem -Path "." -Filter "*.csproj" -Recurse

foreach ($file in $allCsprojFiles) {
    $filename = $file.Name
    $relativePath = $file.FullName.Replace((Get-Location).Path + "\", "")

    if ($projectMap.ContainsKey($filename)) {
        Write-Warning "Duplicate project file name found: $filename"
        Write-Warning "  Existing: $($projectMap[$filename])"
        Write-Warning "  New: $relativePath"
    }

    $projectMap[$filename] = $relativePath
    $projectMap[$filename.Replace("Azure.Mcp.", "AzureMcp.")] = $relativePath
    $projectMap[$filename.Replace("Azure.Mcp.Tools.", "AzureMcp.")] = $relativePath
    Write-Host "  Mapped: $filename -> $relativePath" -ForegroundColor Cyan
}

Write-Host "Found $($projectMap.Count) unique project files" -ForegroundColor Green

# Step 2: Process each .csproj file to fix project references
Write-Host "`nProcessing project files to fix references..." -ForegroundColor Yellow
$totalUpdated = 0

foreach ($csprojFile in $allCsprojFiles) {
    Write-Host "`nProcessing: $($csprojFile.FullName.Replace((Get-Location).Path + '\', ''))" -ForegroundColor White

    $content = Get-Content $csprojFile.FullName -Raw
    $updated = $false

    # Find all ProjectReference elements
    $projectRefPattern = '<ProjectReference\s+Include="([^"]+)"[^>]*/?>'
    $projectRefMatches = [regex]::Matches($content, $projectRefPattern)

    foreach ($match in $projectRefMatches) {
        $currentPath = $match.Groups[1].Value
        $referencedFileName = [System.IO.Path]::GetFileName($currentPath)

        Write-Host "  Found reference: $currentPath" -ForegroundColor Gray

        if ($projectMap.ContainsKey($referencedFileName)) {
            # Calculate the correct relative path from current file to referenced file
            $currentDir = [System.IO.Path]::GetDirectoryName($csprojFile.FullName)
            $targetFullPath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine((Get-Location).Path, $projectMap[$referencedFileName]))

            # Calculate relative path
            $relativePath = [System.IO.Path]::GetRelativePath($currentDir, $targetFullPath)

            if ($currentPath -ne $relativePath) {
                Write-Host "    Updating to: $relativePath" -ForegroundColor Green
                $content = $content.Replace($currentPath, $relativePath)
                $updated = $true
            } else {
                Write-Host "    Already correct" -ForegroundColor DarkGreen
            }
        } else {
            Write-Warning "    Referenced project not found in map: $referencedFileName"
        }
    }

    # Save the file if it was updated
    if ($updated) {
        Set-Content -Path $csprojFile.FullName -Value $content -NoNewline
        Write-Host "  ✓ Updated: $($csprojFile.Name)" -ForegroundColor Green
        $totalUpdated++
    }
}

Write-Host "`n=== Summary ===" -ForegroundColor Magenta
Write-Host "Total project files processed: $($allCsprojFiles.Count)" -ForegroundColor White
Write-Host "Files updated: $totalUpdated" -ForegroundColor Green

# Step 3: Run a test build to verify
Write-Host "`nRunning test build to verify fixes..." -ForegroundColor Yellow
& dotnet build --verbosity quiet | Out-Null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful! All project references are now correct." -ForegroundColor Green
} else {
    Write-Host "⚠ Build still has issues. Manual review may be needed." -ForegroundColor Yellow
    Write-Host "Run 'dotnet build' for detailed error information." -ForegroundColor Gray
}

Write-Host "`nScript completed!" -ForegroundColor Magenta
