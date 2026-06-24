// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

package com.microsoft.mcp.azure;

import java.io.IOException;
import java.io.InputStream;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.nio.file.StandardCopyOption;
import java.nio.file.attribute.PosixFilePermission;
import java.util.EnumSet;
import java.util.Enumeration;
import java.util.Locale;
import java.util.Set;
import java.util.jar.JarEntry;
import java.util.jar.JarFile;
import java.util.stream.Stream;

/**
 * Launcher for the Azure MCP Server Java distribution.
 *
 * <p>At runtime the launcher detects the host OS/architecture, extracts the
 * bundled native {@code azmcp} platform folder from the JAR, marks the
 * executable runnable, and spawns it with inherited stdio. The process waits
 * for the child to exit and propagates its exit code.</p>
 *
 * <p>The published build is framework-dependent, so the bundled folder contains
 * the {@code azmcp} apphost together with its {@code azmcp.dll}, runtime config,
 * and dependency assemblies. The entire folder is extracted so the apphost can
 * locate its companion files.</p>
 *
 * <p>The bundled binary can be overridden by setting either the
 * {@code AZURE_MCP_EXECUTABLE_PATH} environment variable or the
 * {@code azure.mcp.executable.path} system property to the path of an existing
 * {@code azmcp} binary.</p>
 */
public final class Launcher {

    /** Environment variable that overrides the bundled binary path. */
    static final String EXECUTABLE_PATH_ENV = "AZURE_MCP_EXECUTABLE_PATH";

    /** System property that overrides the bundled binary path. */
    static final String EXECUTABLE_PATH_PROPERTY = "azure.mcp.executable.path";

    /** System property that overrides the extraction cache directory. */
    static final String CACHE_DIR_PROPERTY = "azure.mcp.cache.dir";

    private Launcher() {
    }

    public static void main(String[] args) throws IOException, InterruptedException {
        Path executable = resolveExecutable();

        ProcessBuilder processBuilder = new ProcessBuilder();
        processBuilder.command().add(executable.toString());
        for (String arg : args) {
            processBuilder.command().add(arg);
        }
        processBuilder.inheritIO();

        Process process = processBuilder.start();
        int exitCode = process.waitFor();
        System.exit(exitCode);
    }

    /**
     * Resolves the path to the {@code azmcp} executable, honoring overrides and
     * falling back to extracting the platform-specific bundled binary.
     */
    static Path resolveExecutable() throws IOException {
        Path override = resolveOverride();
        if (override != null) {
            if (!Files.exists(override)) {
                throw new IOException("Override executable not found at " + override);
            }
            debugLog("Using override executable: " + override);
            return override;
        }

        String platform = detectPlatform();
        String binaryName = isWindows() ? "azmcp.exe" : "azmcp";
        String resourcePrefix = "native/" + platform + "/";

        debugLog("Detected platform: " + platform);
        debugLog("Resource prefix: " + resourcePrefix);

        Path targetDir = getCacheDirectory().resolve(platform);
        Path executable = targetDir.resolve(binaryName);

        extractPlatformResources(resourcePrefix, targetDir, platform);

        if (!Files.exists(executable)) {
            throw new IOException("Bundled binary '" + binaryName + "' not found for platform '"
                + platform + "' after extraction. This platform may not be supported by this package.");
        }

        makeExecutable(executable);

        debugLog("Executable path: " + executable);
        return executable;
    }

    private static Path resolveOverride() {
        String fromProperty = System.getProperty(EXECUTABLE_PATH_PROPERTY);
        if (fromProperty != null && !fromProperty.isEmpty()) {
            return Paths.get(fromProperty);
        }

        String fromEnv = System.getenv(EXECUTABLE_PATH_ENV);
        if (fromEnv != null && !fromEnv.isEmpty()) {
            return Paths.get(fromEnv);
        }

        return null;
    }

    /**
     * Maps {@code os.name} / {@code os.arch} to a bundled platform key:
     * {@code windows-x86_64}, {@code windows-aarch64}, {@code linux-x86_64},
     * {@code linux-aarch64}, {@code macos-x86_64}, {@code macos-aarch64}.
     */
    static String detectPlatform() {
        String osName = System.getProperty("os.name", "").toLowerCase(Locale.ROOT);
        String osArch = System.getProperty("os.arch", "").toLowerCase(Locale.ROOT);

        String os;
        if (osName.contains("win")) {
            os = "windows";
        } else if (osName.contains("mac") || osName.contains("darwin")) {
            os = "macos";
        } else if (osName.contains("nux") || osName.contains("nix")) {
            os = "linux";
        } else {
            throw new IllegalStateException("Unsupported operating system: " + osName);
        }

        String arch;
        if (osArch.equals("aarch64") || osArch.equals("arm64")) {
            arch = "aarch64";
        } else if (osArch.equals("x86_64") || osArch.equals("amd64") || osArch.equals("x64")) {
            arch = "x86_64";
        } else {
            throw new IllegalStateException("Unsupported architecture: " + osArch);
        }

        return os + "-" + arch;
    }

    static boolean isWindows() {
        return System.getProperty("os.name", "").toLowerCase(Locale.ROOT).contains("win");
    }

    private static Path getCacheDirectory() {
        String override = System.getProperty(CACHE_DIR_PROPERTY);
        if (override != null && !override.isEmpty()) {
            return Paths.get(override);
        }

        String userHome = System.getProperty("user.home");
        String version = getVersion();
        if (userHome != null && !userHome.isEmpty()) {
            return Paths.get(userHome, ".azure-mcp", version);
        }

        return Paths.get(System.getProperty("java.io.tmpdir"), "azure-mcp", version);
    }

    private static String getVersion() {
        Package pkg = Launcher.class.getPackage();
        String version = pkg != null ? pkg.getImplementationVersion() : null;
        return (version != null && !version.isEmpty()) ? version : "dev";
    }

    /** Marker file written once a platform folder has been fully extracted. */
    private static final String EXTRACTION_MARKER = ".extracted";

    /**
     * Extracts every bundled resource under {@code resourcePrefix} into
     * {@code targetDir}. Files are written directly into the target directory and
     * a marker file is created last; the marker's presence signals a complete
     * extraction so subsequent launches can reuse it. Overwriting with
     * {@code REPLACE_EXISTING} keeps re-extraction idempotent if a prior attempt
     * was interrupted before the marker was written.
     */
    private static void extractPlatformResources(String resourcePrefix, Path targetDir, String platform)
            throws IOException {
        Path marker = targetDir.resolve(EXTRACTION_MARKER);
        if (Files.exists(marker)) {
            debugLog("Reusing cached extraction: " + targetDir);
            return;
        }

        Files.createDirectories(targetDir);

        int extracted = extractResources(resourcePrefix, targetDir, platform);
        if (extracted == 0) {
            throw new IOException("Bundled binaries not found for platform '" + platform
                + "' (resource prefix " + resourcePrefix + "). This platform may not be supported by this package.");
        }

        Files.createFile(marker);
        debugLog("Extracted platform binaries to: " + targetDir);
    }

    /**
     * Copies all resources under {@code resourcePrefix} into {@code destDir},
     * resolving whether the launcher runs from a packaged JAR or an exploded
     * classpath. Returns the number of files written.
     */
    private static int extractResources(String resourcePrefix, Path destDir, String platform) throws IOException {
        Path codeSource = locateCodeSource(platform);

        if (Files.isDirectory(codeSource)) {
            return extractFromDirectory(codeSource.resolve(resourcePrefix), destDir);
        }

        return extractFromJar(codeSource, resourcePrefix, destDir);
    }

    private static Path locateCodeSource(String platform) throws IOException {
        URL location = null;
        if (Launcher.class.getProtectionDomain() != null
                && Launcher.class.getProtectionDomain().getCodeSource() != null) {
            location = Launcher.class.getProtectionDomain().getCodeSource().getLocation();
        }

        if (location == null) {
            throw new IOException("Unable to determine launcher code source location for platform '"
                + platform + "'.");
        }

        try {
            return Paths.get(location.toURI());
        } catch (URISyntaxException ex) {
            throw new IOException("Invalid launcher code source URI: " + location, ex);
        }
    }

    private static int extractFromJar(Path jarPath, String resourcePrefix, Path destDir) throws IOException {
        int extracted = 0;
        try (JarFile jar = new JarFile(jarPath.toFile())) {
            Enumeration<JarEntry> entries = jar.entries();
            while (entries.hasMoreElements()) {
                JarEntry entry = entries.nextElement();
                String name = entry.getName();
                if (entry.isDirectory() || !name.startsWith(resourcePrefix)) {
                    continue;
                }

                String relative = name.substring(resourcePrefix.length());
                if (relative.isEmpty()) {
                    continue;
                }

                Path dest = resolveSafely(destDir, relative);
                Files.createDirectories(dest.getParent());
                try (InputStream input = jar.getInputStream(entry)) {
                    Files.copy(input, dest, StandardCopyOption.REPLACE_EXISTING);
                }
                extracted++;
            }
        }
        return extracted;
    }

    private static int extractFromDirectory(Path sourceDir, Path destDir) throws IOException {
        if (!Files.isDirectory(sourceDir)) {
            return 0;
        }

        int[] extracted = {0};
        try (Stream<Path> files = Files.walk(sourceDir)) {
            for (Path source : (Iterable<Path>) files::iterator) {
                if (Files.isDirectory(source)) {
                    continue;
                }
                String relative = sourceDir.relativize(source).toString();
                Path dest = resolveSafely(destDir, relative);
                Files.createDirectories(dest.getParent());
                Files.copy(source, dest, StandardCopyOption.REPLACE_EXISTING);
                extracted[0]++;
            }
        }
        return extracted[0];
    }

    /**
     * Resolves {@code relative} against {@code baseDir}, guarding against path
     * traversal (zip-slip) from malformed archive entry names.
     */
    private static Path resolveSafely(Path baseDir, String relative) throws IOException {
        Path normalizedBase = baseDir.normalize();
        Path dest = normalizedBase.resolve(relative).normalize();
        if (!dest.startsWith(normalizedBase)) {
            throw new IOException("Refusing to extract entry outside target directory: " + relative);
        }
        return dest;
    }

    private static void makeExecutable(Path target) {
        if (isWindows()) {
            return;
        }

        try {
            Set<PosixFilePermission> permissions = EnumSet.of(
                PosixFilePermission.OWNER_READ,
                PosixFilePermission.OWNER_WRITE,
                PosixFilePermission.OWNER_EXECUTE,
                PosixFilePermission.GROUP_READ,
                PosixFilePermission.GROUP_EXECUTE,
                PosixFilePermission.OTHERS_READ,
                PosixFilePermission.OTHERS_EXECUTE);
            Files.setPosixFilePermissions(target, permissions);
        } catch (UnsupportedOperationException | IOException ex) {
            // Fall back to the File API on file systems without POSIX support.
            target.toFile().setExecutable(true, false);
        }
    }

    private static boolean isDebugEnabled() {
        String debug = System.getenv("DEBUG");
        if (debug == null) {
            return false;
        }
        debug = debug.toLowerCase(Locale.ROOT);
        return debug.equals("true") || debug.equals("1") || debug.equals("*") || debug.contains("mcp");
    }

    private static void debugLog(String message) {
        if (isDebugEnabled()) {
            System.err.println("[azure-mcp] " + message);
        }
    }
}
