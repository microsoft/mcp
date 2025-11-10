# Upload all files from the specified source folder (defaults to $HOME/Downloads/all_matches) to OneLake
# OneLake workspace: 47242da5-ff3b-46fb-a94f-977909b773d5 (onelakemcplh)
# OneLake item: 0e67ed13-2bb6-49be-9c87-a1105a4ea342 (lakehouse)
# Target path: Files/raw_data/all_matches

param(
    [string]$SourceFolder = (Join-Path (Join-Path $HOME "Downloads") "all_matches")
)

$sourceFolder = $SourceFolder
$workspaceId = "47242da5-ff3b-46fb-a94f-977909b773d5"
$itemId = "0e67ed13-2bb6-49be-9c87-a1105a4ea342"
$targetFolder = "raw_data/all_matches"

if (-not (Test-Path -Path $sourceFolder)) {
    throw "Source folder '$sourceFolder' does not exist. Provide a valid path using -SourceFolder."
}

# Get all files
$files = Get-ChildItem -Path $sourceFolder -File

Write-Host "Found $($files.Count) files to upload" -ForegroundColor Green

$successCount = 0
$errorCount = 0

foreach ($file in $files) {
    $targetPath = "$targetFolder/$($file.Name)"
    
    Write-Host "Uploading $($file.Name)... " -NoNewline
    
    try {
        # Use the OneLake file-write command
    & dotnet run -- onelake file-write --workspace-id $workspaceId --item-id $itemId --file-path $targetPath --local-file-path $file.FullName --overwrite
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓" -ForegroundColor Green
            $successCount++
        } else {
            Write-Host "✗ (Exit code: $LASTEXITCODE)" -ForegroundColor Red
            $errorCount++
        }
    }
    catch {
        Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        $errorCount++
    }
}

Write-Host ""
Write-Host "Upload completed!" -ForegroundColor Cyan
Write-Host "Successful uploads: $successCount" -ForegroundColor Green
Write-Host "Failed uploads: $errorCount" -ForegroundColor Red