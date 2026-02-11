// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Azure.Mcp.Core.Services.Azure.Authentication;

/// <summary>
/// Provides window handle information for native authentication dialogs.
/// </summary>
public static partial class WindowHandleProvider
{
    /// <summary>
    /// Get window handle across platforms
    /// </summary>
    public static IntPtr GetWindowHandle()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetForegroundWindow();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                IntPtr display = XOpenDisplay(":1");
                Console.WriteLine(display == IntPtr.Zero
                    ? "No X display available. Running in headless mode."
                    : "X display is available.");
                return display;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        return IntPtr.Zero;
    }

    [SupportedOSPlatform("windows")]
    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr GetForegroundWindow();

    [SupportedOSPlatform("linux")]
    [LibraryImport("libX11.so.6")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr XOpenDisplay([MarshalAs(UnmanagedType.LPUTF8Str)] string display);

    [SupportedOSPlatform("linux")]
    [LibraryImport("libX11.so.6")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr XRootWindow(IntPtr display, int screen);

    [SupportedOSPlatform("linux")]
    [LibraryImport("libX11.so.6")]
    [return: MarshalAs(UnmanagedType.SysInt)]
    private static partial IntPtr XDefaultRootWindow(IntPtr display);
}
