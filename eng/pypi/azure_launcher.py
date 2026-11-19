"""Download, verify, cache, and run the Azure MCP Server release matching this package."""

# cspell:ignore NBLCK UNLCK

import errno
import hashlib
import json
import os
import platform
import shutil
import stat
import subprocess
import sys
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request
import zipfile
from contextlib import contextmanager
from pathlib import Path, PurePosixPath

__version__ = "{{VERSION}}"
PACKAGE_NAME = "{{PACKAGE_NAME}}"
CLI_NAME = "{{CLI_NAME}}"
DOWNLOAD_TIMEOUT = 60
LOCK_TIMEOUT = 120


def _platform_key():
    system = {"Windows": "win32", "Darwin": "darwin", "Linux": "linux"}.get(platform.system())
    machine = platform.machine().lower()
    architecture = {"amd64": "x64", "x86_64": "x64", "arm64": "arm64", "aarch64": "arm64"}.get(machine)
    # machine() can describe the host rather than a 32-bit Python process.
    if not system or not architecture or sys.maxsize <= 2**32:
        raise RuntimeError(f"Unsupported platform: {platform.system()} {machine} (64-bit Python required).")
    return f"{system}-{architecture}"


def _cache_root():
    override = os.environ.get("AZURE_MCP_CACHE_DIR")
    if override:
        return Path(override).expanduser()
    if sys.platform == "win32":
        return Path(os.environ.get("LOCALAPPDATA", Path.home() / "AppData" / "Local")) / "Microsoft" / "AzureMcp"
    if sys.platform == "darwin":
        return Path.home() / "Library" / "Caches" / "msmcp"
    return Path(os.environ.get("XDG_CACHE_HOME", Path.home() / ".cache")) / "msmcp"


def _sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def _relative_path(name):
    path = PurePosixPath(name)
    if not name or path.is_absolute() or "\\" in name or ":" in name or any(
        part in ("", ".", "..") for part in name.split("/")
    ):
        raise RuntimeError(f"Unsafe release archive path: {name!r}")
    return Path(*path.parts)


def _valid_cache(directory, files):
    if not directory.is_dir() or directory.is_symlink():
        return False
    actual = set()
    for path in directory.rglob("*"):
        if path.is_symlink():
            return False
        if path.is_file():
            actual.add(path.relative_to(directory).as_posix())
    if actual != set(files):
        return False
    return all(
        (directory / _relative_path(name)).stat().st_size == metadata["size"]
        and _sha256(directory / _relative_path(name)) == metadata["sha256"]
        for name, metadata in files.items()
    )


@contextmanager
def _cache_lock(path):
    # OS locks are released even if a downloader crashes; a stale file is not a stale lock.
    with path.open("a+b") as lock:
        deadline = time.monotonic() + LOCK_TIMEOUT
        while True:
            try:
                if os.name == "nt":
                    import msvcrt

                    lock.seek(0)
                    msvcrt.locking(lock.fileno(), msvcrt.LK_NBLCK, 1)
                else:
                    import fcntl

                    fcntl.flock(lock.fileno(), fcntl.LOCK_EX | fcntl.LOCK_NB)
                break
            except OSError as error:
                if error.errno not in (errno.EACCES, errno.EAGAIN, errno.EDEADLK):
                    raise
                if time.monotonic() >= deadline:
                    raise RuntimeError(f"Timed out waiting for the download cache lock: {path}") from error
                time.sleep(0.1)
        try:
            yield
        finally:
            if os.name == "nt":
                lock.seek(0)
                msvcrt.locking(lock.fileno(), msvcrt.LK_UNLCK, 1)
            else:
                fcntl.flock(lock.fileno(), fcntl.LOCK_UN)


class _ReleaseRedirectHandler(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, response, code, message, headers, new_url):
        target = urllib.parse.urlsplit(new_url)
        if target.scheme != "https" or target.hostname not in (
            "github.com", "release-assets.githubusercontent.com", "objects.githubusercontent.com"
        ):
            raise RuntimeError("Release download redirected outside the official HTTPS release hosts.")
        return super().redirect_request(request, response, code, message, headers, new_url)


def _download(artifact, destination):
    request = urllib.request.Request(artifact["url"], headers={"User-Agent": f"{PACKAGE_NAME}/{__version__}"})
    opener = urllib.request.build_opener(_ReleaseRedirectHandler())
    digest = hashlib.sha256()
    size = 0
    with opener.open(request, timeout=DOWNLOAD_TIMEOUT) as response, destination.open("wb") as output:
        while chunk := response.read(1024 * 1024):
            size += len(chunk)
            if size > artifact["size"]:
                raise RuntimeError("Release download exceeded its expected size.")
            digest.update(chunk)
            output.write(chunk)
    if size != artifact["size"] or digest.hexdigest() != artifact["sha256"]:
        raise RuntimeError("Release download failed SHA-256 integrity validation; no executable was installed.")


def _extract(archive, destination, files):
    with zipfile.ZipFile(archive) as release:
        seen = set()
        for entry in release.infolist():
            relative = _relative_path(entry.filename.rstrip("/") if entry.is_dir() else entry.filename)
            if stat.S_ISLNK(entry.external_attr >> 16):
                raise RuntimeError("Release archive contains a symbolic link.")
            if entry.is_dir():
                continue
            if entry.filename not in files or entry.filename in seen:
                raise RuntimeError(f"Unexpected release archive entry: {entry.filename}")
            seen.add(entry.filename)
            if entry.file_size != files[entry.filename]["size"]:
                raise RuntimeError(f"Unexpected release file size: {entry.filename}")
            target = destination / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            with release.open(entry) as source, target.open("xb") as output:
                shutil.copyfileobj(source, output)
        if seen != set(files):
            raise RuntimeError("Release archive is missing required files.")
    if not _valid_cache(destination, files):
        raise RuntimeError("Extracted release files failed SHA-256 integrity validation.")


def get_executable_path():
    """Resolve this package's pinned server, downloading it only on a cache miss."""
    key = _platform_key()
    manifest = json.loads(Path(__file__).with_name("release.json").read_text(encoding="utf-8-sig"))
    if key not in manifest["platforms"]:
        raise RuntimeError(f"{PACKAGE_NAME} {__version__} has no release for {key}.")
    artifact = manifest["platforms"][key]
    executable = CLI_NAME + (".exe" if key.startswith("win32-") else "")
    if executable not in artifact["files"]:
        raise RuntimeError(f"Release manifest does not contain {executable}.")
    # Include the archive digest so rebuilt packages cannot reuse another payload's cache.
    parent = _cache_root() / PACKAGE_NAME / __version__ / key
    parent.mkdir(parents=True, exist_ok=True, mode=0o700)
    directory = parent / artifact["sha256"]
    with _cache_lock(parent / "download.lock"):
        if not _valid_cache(directory, artifact["files"]):
            if directory.exists() or directory.is_symlink():
                print(f"Replacing invalid Azure MCP cache at {directory}", file=sys.stderr)
                if directory.is_symlink():
                    directory.unlink()
                elif directory.is_dir():
                    shutil.rmtree(directory)
                else:
                    directory.unlink()
            print(f"Downloading Azure MCP Server {manifest['version']} for {key}...", file=sys.stderr)
            with tempfile.TemporaryDirectory(prefix=".download-", dir=parent) as temporary:
                archive = Path(temporary) / "release.zip"
                staging = Path(temporary) / "server"
                staging.mkdir()
                _download(artifact, archive)
                _extract(archive, staging, artifact["files"])
                (staging / executable).chmod(0o755)
                staging.rename(directory)
        return directory / executable


def run_executable(args=None):
    """Forward the MCP streams, arguments, and exit status without shell expansion."""
    try:
        executable = get_executable_path()
        result = subprocess.run(
            [str(executable), *(sys.argv[1:] if args is None else args)],
            stdin=sys.stdin,
            stdout=sys.stdout,
            stderr=sys.stderr,
        )
        return result.returncode
    except PermissionError as error:
        print(f"Azure MCP launcher permission error: {error}", file=sys.stderr)
        return 126
    except (OSError, RuntimeError, ValueError, urllib.error.URLError, zipfile.BadZipFile) as error:
        print(
            f"Azure MCP launcher failed: {error}\n"
            "Check access to GitHub Releases and write access to AZURE_MCP_CACHE_DIR.",
            file=sys.stderr,
        )
        return 1
    except KeyboardInterrupt:
        return 130


def main():
    code = run_executable()
    # subprocess uses negative return codes for Unix signals; preserve the shell convention.
    sys.exit(128 - code if code < 0 else code)


if __name__ == "__main__":
    main()
