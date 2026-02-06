# MSIX Assets

This folder can contain MSIX-specific visual assets. If not present, the `Pack-Msix.ps1` script
automatically falls back to using assets from the `../mcpb/` folder.

## Asset Fallback Chain

1. **MSIX-specific assets** (this folder) - Use for MSIX-specific branding
2. **MCPB servericon.png** (`../mcpb/servericon.png`) - Shared MCP bundle icon
3. **Package icon** (from `build_info.json`) - Falls back to server's package icon

## Required Images for MSIX

| File | Required Size | Description |
|------|--------------|-------------|
| `StoreLogo.png` | 50x50 | Store listing logo |
| `Square44x44Logo.png` | 44x44 | Small app icon (taskbar, etc.) |
| `Square150x150Logo.png` | 150x150 | Medium tile |
| `Wide310x150Logo.png` | 310x150 | Wide tile |
| `servericon.png` | Any (256x256 recommended) | MCP server icon for ODR |

## Current Behavior

The `Pack-Msix.ps1` script uses `servericon.png` from MCPB for all required sizes.
Windows will scale the images as needed.

## Production Assets (TODO)

For optimal visual quality, create properly sized assets and place them here:

1. **StoreLogo.png** (50x50) - Microsoft Store listings
2. **Square44x44Logo.png** (44x44) - Taskbar, Start menu
3. **Square150x150Logo.png** (150x150) - Medium tile
4. **Wide310x150Logo.png** (310x150) - Wide tile

Tools for creating assets:
- [MSIX Hero](https://msixhero.net/)
- ImageMagick for batch resizing
