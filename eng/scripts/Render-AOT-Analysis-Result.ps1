#!/bin/env pwsh
#Requires -Version 7

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Html', 'Console')]
    [string]$OutputFormat = 'Html',
    [string]$ServerName = "Azure.Mcp.Server"
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/AOT-Config.ps1" -ServerName $ServerName
$config = Get-AOTConfig -ServerName $ServerName

$JsonReportPath = $config.JsonReportPath
$OutputPath = $config.HtmlReportPath

if (-not (Test-Path $JsonReportPath)) {
    Write-Error "JSON report not found at: $JsonReportPath"
    exit 1
}

$jsonContent = Get-Content $JsonReportPath -Raw | ConvertFrom-Json

# Calculate statistics
$totalWarnings = 0
$dllStats = @()

if ($jsonContent -is [PSCustomObject]) {
    foreach ($property in $jsonContent.PSObject.Properties) {
        $dllName = $property.Name
        $warnings = $property.Value
        $warningCount = $warnings.Count
        $totalWarnings += $warningCount
        
        $dllStats += [PSCustomObject]@{
            DllName = $dllName
            WarningCount = $warningCount
            Warnings = $warnings
        }
    }
}

# Sort DLLs by warning count (descending)
$dllStats = $dllStats | Sort-Object WarningCount -Descending

if ($OutputFormat -eq 'Console') {
    # Render to console (Useful for CI/CD pipelines).
    Write-Host "##[section]AOT Compatibility Analysis Results"
    Write-Host "Total AOT/Trimming Warnings: $totalWarnings"
    Write-Host "Affected DLLs: $($dllStats.Count)"
    
    if ($totalWarnings -gt 0) {
        Write-Host ""
        Write-Host "##[section]Full AOT Analysis Report (JSON):"
        $jsonContent | ConvertTo-Json -Depth 5 | Write-Host
        
        Write-Host ""
        Write-Host "##[warning]AOT compatibility issues found. See artifacts for details."
        Write-Host "##vso[task.logissue type=warning]$totalWarnings AOT/trimming warnings found across $($dllStats.Count) DLLs"
    } else {
        Write-Host "##[section]✅ No AOT compatibility issues found!"
    }
    return
}

# $OutputFormat -eq 'Html'
# Generate HTML (Useful for local development).
$html = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>AOT Compatibility Analysis Report</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }
        .header h1 {
            margin: 0;
            font-size: 2.5em;
            font-weight: 300;
        }
        .stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            padding: 30px;
            background: #fafafa;
        }
        .stat-card {
            background: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .stat-number {
            font-size: 2.5em;
            font-weight: bold;
            color: #667eea;
            margin: 0;
        }
        .stat-label {
            color: #666;
            margin: 5px 0 0 0;
            font-size: 0.9em;
        }
        .chart-section {
            padding: 30px;
        }
        .chart-container {
            position: relative;
            height: 400px;
            margin-bottom: 30px;
        }
        .dll-details {
            padding: 0 30px 30px;
        }
        .dll-card {
            border: 1px solid #e0e0e0;
            border-radius: 8px;
            margin-bottom: 20px;
            overflow: hidden;
            transition: all 0.3s ease;
        }
        .dll-card.highlighted {
            border-color: #667eea;
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
            transform: translateY(-2px);
        }
        .dll-header {
            background: #f8f9fa;
            padding: 15px 20px;
            border-bottom: 1px solid #e0e0e0;
            cursor: pointer;
            display: flex;
            justify-content: space-between;
            align-items: center;
            transition: background-color 0.2s ease;
        }
        .dll-header:hover {
            background: #e9ecef;
        }
        .dll-card.highlighted .dll-header {
            background: #e3f2fd;
        }
        .dll-name {
            font-weight: bold;
            color: #333;
        }
        .warning-count {
            background: #dc3545;
            color: white;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 0.8em;
            font-weight: bold;
        }
        .dll-warnings {
            padding: 20px;
            background: #fff;
            display: none;
        }
        .dll-warnings.expanded {
            display: block;
        }
        .warning-item {
            padding: 10px;
            margin: 5px 0;
            background: #f8f9fa;
            border-left: 4px solid #dc3545;
            border-radius: 0 4px 4px 0;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
            line-height: 1.4;
            word-break: break-all;
        }
        .no-warnings {
            text-align: center;
            padding: 50px;
            color: #28a745;
            font-size: 1.2em;
        }
        .expand-all {
            margin: 20px 0;
            text-align: center;
        }
        .btn {
            background: #667eea;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 4px;
            cursor: pointer;
            font-size: 1em;
        }
        .btn:hover {
            background: #5a6fd8;
        }
        .timestamp {
            text-align: center;
            color: #666;
            font-size: 0.9em;
            padding: 20px;
            border-top: 1px solid #e0e0e0;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>AOT Compatibility Analysis</h1>
            <p>Ahead-of-Time Compilation Warnings Report</p>
        </div>

        <div class="stats">
            <div class="stat-card">
                <div class="stat-number">$totalWarnings</div>
                <div class="stat-label">Total Warnings</div>
            </div>
            <div class="stat-card">
                <div class="stat-number">$($dllStats.Count)</div>
                <div class="stat-label">Affected DLLs</div>
            </div>
        </div>
"@

if ($totalWarnings -gt 0) {
    # Add chart section
    $html += @"
        <div class="chart-section">
            <h2>Warnings Distribution by DLL</h2>
            <div class="chart-container">
                <canvas id="warningsChart"></canvas>
            </div>
        </div>

        <div class="dll-details">
            <div class="expand-all">
                <button class="btn" onclick="toggleAllDlls()">Expand/Collapse All</button>
            </div>
"@

    # Add DLL details
    foreach ($dll in $dllStats) {
        $dllId = $dll.DllName -replace '[^a-zA-Z0-9]', '_'
        $html += @"
            <div class="dll-card" id="dll_$dllId">
                <div class="dll-header" onclick="toggleDll('$dllId')">
                    <span class="dll-name">$($dll.DllName)</span>
                    <span class="warning-count">$($dll.WarningCount) warnings</span>
                </div>
                <div class="dll-warnings" id="warnings_$dllId">
"@
        foreach ($warning in $dll.Warnings) {
            $escapedWarning = $warning -replace "&", "&amp;" -replace "<", "&lt;" -replace ">", "&gt;" -replace '"', "&quot;"
            $html += @"
                    <div class="warning-item">$escapedWarning</div>
"@
        }
        $html += @"
                </div>
            </div>
"@
    }
    $html += "</div>"
} else {
    $html += @"
        <div class="no-warnings">
            <h2>🎉 No AOT Compatibility Warnings Found!</h2>
            <p>Your application is AOT-ready.</p>
        </div>
"@
}

# Add JavaScript and closing HTML
$chartData = if ($totalWarnings -gt 0) {
    $labels = ($dllStats | ForEach-Object { "'$($_.DllName)'" }) -join ","
    $data = ($dllStats | ForEach-Object { $_.WarningCount }) -join ","
    $dllIds = ($dllStats | ForEach-Object { "'$(($_.DllName -replace '[^a-zA-Z0-9]', '_'))'" }) -join ","
    "labels: [$labels], data: [$data], dllIds: [$dllIds]"
} else {
    "labels: [], data: [], dllIds: []"
}

$dllIdsArray = if ($totalWarnings -gt 0) {
    ($dllStats | ForEach-Object { "'$(($_.DllName -replace '[^a-zA-Z0-9]', '_'))'" }) -join ","
} else {
    ""
}

$html += @"
        <div class="timestamp">
            Generated on: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
        </div>
    </div>

    <script>
        // Chart.js configuration
        const ctx = document.getElementById('warningsChart');
        const dllIds = [$dllIdsArray];
        
        if (ctx && $totalWarnings > 0) {
            new Chart(ctx, {
                type: 'bar',
                data: {
                    $chartData,
                    datasets: [{
                        label: 'Warning Count',
                        data: [$data],
                        backgroundColor: 'rgba(102, 126, 234, 0.8)',
                        borderColor: 'rgba(102, 126, 234, 1)',
                        borderWidth: 1,
                        hoverBackgroundColor: 'rgba(102, 126, 234, 1)',
                        hoverBorderColor: 'rgba(102, 126, 234, 1)'
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        },
                        x: {
                            ticks: {
                                maxRotation: 45,
                                minRotation: 45
                            }
                        }
                    },
                    onClick: (event, elements) => {
                        if (elements.length > 0) {
                            const dataIndex = elements[0].index;
                            const dllId = dllIds[dataIndex];
                            scrollToDll(dllId);
                        }
                    },
                    onHover: (event, elements) => {
                        event.native.target.style.cursor = elements.length > 0 ? 'pointer' : 'default';
                    }
                }
            });
        }

        // Scroll to and highlight DLL section
        function scrollToDll(dllId) {
            // Remove previous highlights
            document.querySelectorAll('.dll-card.highlighted').forEach(card => {
                card.classList.remove('highlighted');
            });
            
            // Find and highlight the target DLL
            const targetCard = document.getElementById('dll_' + dllId);
            if (targetCard) {
                targetCard.classList.add('highlighted');
                targetCard.scrollIntoView({ 
                    behavior: 'smooth', 
                    block: 'center' 
                });
                
                // Expand the warnings if not already expanded
                const warningsElement = document.getElementById('warnings_' + dllId);
                if (warningsElement && !warningsElement.classList.contains('expanded')) {
                    warningsElement.classList.add('expanded');
                }
                
                // Remove highlight after 3 seconds
                setTimeout(() => {
                    targetCard.classList.remove('highlighted');
                }, 3000);
            }
        }

        // Toggle DLL warnings visibility
        function toggleDll(dllId) {
            const element = document.getElementById('warnings_' + dllId);
            element.classList.toggle('expanded');
        }

        // Toggle all DLLs
        let allExpanded = false;
        function toggleAllDlls() {
            const allWarnings = document.querySelectorAll('.dll-warnings');
            allExpanded = !allExpanded;
            allWarnings.forEach(element => {
                if (allExpanded) {
                    element.classList.add('expanded');
                } else {
                    element.classList.remove('expanded');
                }
            });
        }
    </script>
</body>
</html>
"@

# Write HTML file
$html | Out-File -FilePath $OutputPath -Encoding utf8

Write-Host "HTML report generated: $OutputPath" -ForegroundColor Green

# Open in browser if on macOS
if ($IsMacOS -or $IsWindows) {
    if ($IsMacOS) {
        & open $OutputPath
    } elseif ($IsWindows) {
        & start $OutputPath
    }
    Write-Host "Opening report in default browser..." -ForegroundColor Cyan
}
