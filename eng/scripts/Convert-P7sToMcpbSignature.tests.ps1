# Install-Module -Name Pester -Force -SkipPublisherCheck
# Invoke-Pester -Passthru eng/scripts/Convert-P7sToMcpbSignature.tests.ps1

BeforeAll {
    $ScriptPath = "$PSScriptRoot/Convert-P7sToMcpbSignature.ps1"
    $TestFolder = "$PSScriptRoot/.testruns-mcpb"
    
    # MCPB signature markers
    $Script:SIG_V1_MARKER = "MCPB_SIG_V1"
    $Script:SIG_END_MARKER = "MCPB_SIG_END"
    
    # Clean up and create test folder
    if (Test-Path $TestFolder) {
        Remove-Item -Recurse -Force $TestFolder
    }
    New-Item -ItemType Directory -Path $TestFolder | Out-Null
}

AfterAll {
    # Cleanup test folder
    $TestFolder = "$PSScriptRoot/.testruns-mcpb"
    if (Test-Path $TestFolder) {
        Remove-Item -Recurse -Force $TestFolder -ErrorAction SilentlyContinue
    }
}

Describe "Convert-P7sToMcpbSignature.ps1" {
    
    BeforeEach {
        # Create fresh test files for each test
        $Script:TestMcpbPath = Join-Path $TestFolder "test-$(Get-Random).mcpb"
        $Script:TestP7sPath = Join-Path $TestFolder "test-$(Get-Random).p7s"
        $Script:TestOutputPath = Join-Path $TestFolder "output-$(Get-Random).mcpb"
    }
    
    Context "Signature Format Validation" {
        
        It "Should create signature block with correct MCPB_SIG_V1 marker" {
            # Create test MCPB content (minimal ZIP-like content)
            $mcpbContent = [byte[]]@(0x50, 0x4B, 0x03, 0x04) + [byte[]](1..100)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            # Create test signature
            $signatureContent = [byte[]](1..50)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Verify output exists
            Test-Path $TestOutputPath | Should -Be $true
            
            # Read output and verify marker
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            
            $outputString | Should -Match $SIG_V1_MARKER
        }
        
        It "Should create signature block with correct MCPB_SIG_END marker" {
            # Create test files
            $mcpbContent = [byte[]]@(0x50, 0x4B, 0x03, 0x04) + [byte[]](1..100)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            $signatureContent = [byte[]](1..50)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Read output and verify end marker
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            
            $outputString | Should -Match $SIG_END_MARKER
        }
        
        It "Should have correct signature block structure: MCPB + MCPB_SIG_V1 + length + sig + MCPB_SIG_END" {
            # Create test MCPB content
            $mcpbContent = [byte[]]@(0x50, 0x4B, 0x03, 0x04, 0x00, 0x00)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            # Create test signature (known size for verification)
            $signatureContent = [byte[]](0xAA, 0xBB, 0xCC, 0xDD)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Verify structure
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            
            # Expected size: original (6) + MCPB_SIG_V1 (11) + length (4) + sig (4) + MCPB_SIG_END (12) = 37
            $outputBytes.Length | Should -Be 37
            
            # Verify original content is preserved at the start
            $outputBytes[0..5] | Should -Be $mcpbContent
        }
    }
    
    Context "Length Prefix Correctness" {
        
        It "Should encode length as 4-byte little-endian for small signatures" {
            # Create test files
            $mcpbContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            # 50 bytes signature
            $signatureContent = [byte[]](1..50)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Read output
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            
            # Find MCPB_SIG_V1 marker position
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            $markerIndex = $outputString.IndexOf($SIG_V1_MARKER)
            
            # Length prefix starts right after the marker (11 bytes)
            $lengthStart = $markerIndex + 11
            $lengthBytes = $outputBytes[$lengthStart..($lengthStart + 3)]
            
            # Convert from little-endian to uint32
            $length = [BitConverter]::ToUInt32($lengthBytes, 0)
            
            $length | Should -Be 50
        }
        
        It "Should encode length correctly for larger signatures (1896 bytes - typical PKCS#7)" {
            # Create test files
            $mcpbContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            # 1896 bytes signature (typical PKCS#7 size) - use modulo to stay in byte range
            $signatureContent = [byte[]]::new(1896)
            for ($i = 0; $i -lt 1896; $i++) { $signatureContent[$i] = [byte]($i % 256) }
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Read output
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            
            # Find MCPB_SIG_V1 marker position
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            $markerIndex = $outputString.IndexOf($SIG_V1_MARKER)
            
            # Length prefix starts right after the marker (11 bytes)
            $lengthStart = $markerIndex + 11
            $lengthBytes = $outputBytes[$lengthStart..($lengthStart + 3)]
            
            # Convert from little-endian to uint32
            $length = [BitConverter]::ToUInt32($lengthBytes, 0)
            
            $length | Should -Be 1896
        }
        
        It "Should handle maximum practical signature size (64KB)" {
            # Create test files
            $mcpbContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            # 65535 bytes signature - use modulo to stay in byte range
            $signatureContent = [byte[]]::new(65535)
            for ($i = 0; $i -lt 65535; $i++) { $signatureContent[$i] = [byte]($i % 256) }
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Read output
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            
            # Find and verify length
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            $markerIndex = $outputString.IndexOf($SIG_V1_MARKER)
            $lengthStart = $markerIndex + 11
            $lengthBytes = $outputBytes[$lengthStart..($lengthStart + 3)]
            $length = [BitConverter]::ToUInt32($lengthBytes, 0)
            
            $length | Should -Be 65535
        }
    }
    
    Context "Binary Integrity" {
        
        It "Should preserve original MCPB content exactly" {
            # Create test MCPB with known content
            $mcpbContent = [byte[]](0x50, 0x4B, 0x03, 0x04, 0xDE, 0xAD, 0xBE, 0xEF)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            $signatureContent = [byte[]](1..100)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Read output and verify original content
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            $preservedContent = $outputBytes[0..($mcpbContent.Length - 1)]
            
            $preservedContent | Should -Be $mcpbContent
        }
        
        It "Should preserve signature bytes exactly" {
            # Create test files
            $mcpbContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            # Signature with known pattern
            $signatureContent = [byte[]](0xCA, 0xFE, 0xBA, 0xBE, 0xDE, 0xAD, 0xBE, 0xEF)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath
            
            # Read output
            $outputBytes = [System.IO.File]::ReadAllBytes($TestOutputPath)
            
            # Find signature in output (after MCPB_SIG_V1 + length prefix)
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            $markerIndex = $outputString.IndexOf($SIG_V1_MARKER)
            $sigStart = $markerIndex + 11 + 4  # marker (11) + length (4)
            $sigEnd = $sigStart + $signatureContent.Length - 1
            
            $extractedSig = $outputBytes[$sigStart..$sigEnd]
            
            $extractedSig | Should -Be $signatureContent
        }
        
        It "Should support in-place signing (same input and output)" {
            # Create test files
            $mcpbContent = [byte[]](0x50, 0x4B, 0x03, 0x04) + [byte[]](1..100)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            $signatureContent = [byte[]](1..50)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            # Run the conversion with same input/output (in-place)
            & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestMcpbPath
            
            # Verify file was modified
            $outputBytes = [System.IO.File]::ReadAllBytes($TestMcpbPath)
            $outputString = [System.Text.Encoding]::ASCII.GetString($outputBytes)
            
            $outputString | Should -Match $SIG_V1_MARKER
            $outputString | Should -Match $SIG_END_MARKER
        }
    }
    
    Context "Error Handling" {
        
        It "Should throw if signature file does not exist" {
            $mcpbContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            { & $ScriptPath -P7sFile "nonexistent.p7s" -McpbFile $TestMcpbPath -OutputFile $TestOutputPath } | 
                Should -Throw "*not found*"
        }
        
        It "Should throw if MCPB file does not exist" {
            $signatureContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            { & $ScriptPath -P7sFile $TestP7sPath -McpbFile "nonexistent.mcpb" -OutputFile $TestOutputPath } | 
                Should -Throw "*not found*"
        }
        
        It "Should throw if MCPB is already signed" {
            # Create a pre-signed MCPB (contains MCPB_SIG_V1 marker)
            $mcpbContent = [System.Text.Encoding]::ASCII.GetBytes("PKtest content here MCPB_SIG_V1 signature MCPB_SIG_END")
            [System.IO.File]::WriteAllBytes($TestMcpbPath, $mcpbContent)
            
            $signatureContent = [byte[]](1..10)
            [System.IO.File]::WriteAllBytes($TestP7sPath, $signatureContent)
            
            { & $ScriptPath -P7sFile $TestP7sPath -McpbFile $TestMcpbPath -OutputFile $TestOutputPath } | 
                Should -Throw "*already*signed*"
        }
    }
}
