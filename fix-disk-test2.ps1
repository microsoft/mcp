$file = 'tools/Azure.Mcp.Tools.AzureBackup/tests/Azure.Mcp.Tools.AzureBackup.LiveTests/AzureBackupCommandTests.cs'
$content = [System.IO.File]::ReadAllText($file)

$old = '                { "weekly-retention-weeks", "12" },
                { "monthly-retention-months", "12" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    // CosmosDB policy create test skipped'

$new = '                { "weekly-retention-weeks", "12" },
                { "weekly-retention-days-of-week", "Sunday" },
                { "monthly-retention-months", "12" },
                { "monthly-retention-days-of-month", "1" }
            });

        var opResult = result.AssertProperty("result");
        Assert.Equal("Succeeded", opResult.AssertProperty("status").GetString());
    }

    // CosmosDB policy create test skipped'

if ($content.Contains($old)) {
    $content = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($file, $content)
    Write-Host "Replaced successfully"
} else {
    Write-Host "ERROR: Pattern not found"
}
