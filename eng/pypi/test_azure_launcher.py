"""Offline regression tests for the Azure PyPI launcher."""

# cspell:ignore stopall

import hashlib
import importlib.util
import io
import json
import os
import stat
import sys
import tempfile
import unittest
import urllib.error
import zipfile
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from unittest.mock import patch


spec = importlib.util.spec_from_file_location("azure_launcher", Path(__file__).with_name("azure_launcher.py"))
launcher = importlib.util.module_from_spec(spec)
spec.loader.exec_module(launcher)
launcher.PACKAGE_NAME = "msmcp-azure"
launcher.CLI_NAME = "azmcp"
launcher.__version__ = "1.2.3b4"


class LauncherTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.addCleanup(self.temporary.cleanup)
        self.root = Path(self.temporary.name)
        self.files = {"azmcp": b"server executable", "lib/dependency.dll": b"dependency"}
        self.payload = self.make_zip(self.files)
        self.artifact = {
            "url": "https://github.com/microsoft/mcp/releases/download/Azure.Mcp.Server-1.2.3-beta.4/Azure.Mcp.Server-linux-x64.zip",
            "sha256": hashlib.sha256(self.payload).hexdigest(),
            "size": len(self.payload),
            "files": {
                name: {"sha256": hashlib.sha256(data).hexdigest(), "size": len(data)}
                for name, data in self.files.items()
            },
        }
        self.package = self.root / "package"
        self.package.mkdir()
        self.write_manifest()
        self.addCleanup(patch.stopall)
        patch.object(launcher, "__file__", str(self.package / "__init__.py")).start()
        patch.object(launcher, "_platform_key", return_value="linux-x64").start()
        patch.object(launcher, "_cache_root", return_value=self.root / "cache").start()
        self.opener = patch.object(launcher.urllib.request, "build_opener").start()
        self.opener.return_value.open.side_effect = lambda *args, **kwargs: io.BytesIO(self.payload)

    @staticmethod
    def make_zip(files):
        buffer = io.BytesIO()
        with zipfile.ZipFile(buffer, "w") as archive:
            for name, data in files.items():
                archive.writestr(name, data)
        return buffer.getvalue()

    def write_manifest(self):
        (self.package / "release.json").write_text(json.dumps({
            "version": "1.2.3-beta.4", "platforms": {"linux-x64": self.artifact},
        }), encoding="utf-8")

    def test_download_verify_cache_and_offline_reuse(self):
        with patch("sys.stderr", new_callable=io.StringIO) as stderr, patch(
            "sys.stdout", new_callable=io.StringIO
        ) as stdout:
            executable = launcher.get_executable_path()
            self.assertEqual(executable.read_bytes(), self.files["azmcp"])
            self.assertIn("1.2.3-beta.4", stderr.getvalue())
            self.assertEqual(stdout.getvalue(), "")
        if os.name != "nt":
            self.assertTrue(executable.stat().st_mode & stat.S_IXUSR)
        self.opener.return_value.open.side_effect = urllib.error.URLError("offline")
        self.assertEqual(launcher.get_executable_path(), executable)
        self.opener.return_value.open.assert_called_once()
        request = self.opener.return_value.open.call_args.args[0]
        self.assertEqual(request.full_url, self.artifact["url"])
        self.assertEqual(self.opener.return_value.open.call_args.kwargs["timeout"], launcher.DOWNLOAD_TIMEOUT)
        self.assertEqual(list(executable.parent.parent.glob(".download-*")), [])

    def test_checksum_mismatch_never_executes_or_publishes(self):
        self.artifact["sha256"] = "0" * 64
        self.write_manifest()
        with patch("subprocess.run") as run, patch("sys.stderr", new_callable=io.StringIO) as stderr:
            self.assertEqual(launcher.run_executable(["server", "start"]), 1)
            run.assert_not_called()
            self.assertIn("SHA-256", stderr.getvalue())
        self.assertEqual(list((self.root / "cache").rglob(".download-*")), [])
        self.assertFalse(any(path.name == "azmcp" for path in (self.root / "cache").rglob("*")))

    def test_truncated_and_oversized_downloads_fail(self):
        for size in (len(self.payload) - 1, len(self.payload) + 1):
            with self.subTest(size=size):
                self.artifact["size"] = size
                self.write_manifest()
                with self.assertRaisesRegex(RuntimeError, "expected size|integrity validation"):
                    launcher.get_executable_path()

    def test_failed_download_reports_error_and_can_retry(self):
        self.opener.return_value.open.side_effect = urllib.error.URLError("offline")
        with patch("sys.stderr", new_callable=io.StringIO) as stderr:
            self.assertEqual(launcher.run_executable([]), 1)
            self.assertIn("offline", stderr.getvalue())
        self.opener.return_value.open.side_effect = lambda *args, **kwargs: io.BytesIO(self.payload)
        self.assertTrue(launcher.get_executable_path().is_file())

    def test_interrupted_download_is_cleaned_up(self):
        response = self.opener.return_value.open.return_value
        self.opener.return_value.open.side_effect = None
        response.__enter__.return_value.read.side_effect = [self.payload[:10], ConnectionResetError("interrupted")]
        with patch("sys.stderr", new_callable=io.StringIO) as stderr, patch("subprocess.run") as run:
            self.assertEqual(launcher.run_executable([]), 1)
            self.assertIn("interrupted", stderr.getvalue())
            run.assert_not_called()
        self.assertEqual(list((self.root / "cache").rglob(".download-*")), [])

    def test_corrupt_missing_or_extra_cached_files_are_replaced(self):
        executable = launcher.get_executable_path()
        for name, data in (
            ("azmcp", b"tampered"),
            ("lib/dependency.dll", b"tampered dependency"),
            ("extra.dll", b"unexpected dependency"),
        ):
            with self.subTest(name=name):
                (executable.parent / name).write_bytes(data)
                self.assertEqual(launcher.get_executable_path().read_bytes(), self.files["azmcp"])
                self.assertTrue(launcher._valid_cache(executable.parent, self.artifact["files"]))
        (executable.parent / "lib" / "dependency.dll").unlink()
        launcher.get_executable_path()
        self.assertEqual(self.opener.return_value.open.call_count, 5)

    def test_cache_validation_detects_same_size_tampering(self):
        executable = launcher.get_executable_path()
        executable.write_bytes(b"x" * len(self.files["azmcp"]))
        launcher.get_executable_path()
        self.assertEqual(executable.read_bytes(), self.files["azmcp"])
        self.assertEqual(self.opener.return_value.open.call_count, 2)

    def test_concurrent_launches_download_once(self):
        with ThreadPoolExecutor(max_workers=4) as executor:
            paths = list(executor.map(lambda _: launcher.get_executable_path(), range(4)))
        self.assertTrue(all(path == paths[0] for path in paths))
        self.opener.return_value.open.assert_called_once()

    def test_lock_timeout_is_actionable(self):
        lock_path = self.root / "download.lock"
        with launcher._cache_lock(lock_path), patch.object(launcher, "LOCK_TIMEOUT", 0):
            with self.assertRaisesRegex(RuntimeError, "cache lock"):
                with launcher._cache_lock(lock_path):
                    self.fail("Second lock unexpectedly acquired")

    def test_unsupported_release_platform_does_not_download(self):
        with patch.object(launcher, "_platform_key", return_value="linux-arm64"):
            with self.assertRaisesRegex(RuntimeError, "no release"):
                launcher.get_executable_path()
        self.opener.assert_not_called()

    def test_missing_executable_is_rejected(self):
        del self.artifact["files"]["azmcp"]
        self.write_manifest()
        with self.assertRaisesRegex(RuntimeError, "does not contain azmcp"):
            launcher.get_executable_path()
        self.opener.assert_not_called()

    def test_archive_path_traversal_is_rejected(self):
        for name in ("../escape", "/absolute", "C:/escape", "lib\\escape", "lib/../escape"):
            with self.subTest(name=name):
                archive = self.root / "unsafe.zip"
                archive.write_bytes(self.make_zip({name: b"unsafe"}))
                with self.assertRaisesRegex(RuntimeError, "Unsafe release archive path"):
                    launcher._relative_path(name)
                # ZipInfo normalizes backslashes on Windows; either representation must fail.
                with self.assertRaisesRegex(RuntimeError, "Unsafe release archive path|Unexpected release archive entry"):
                    launcher._extract(archive, self.root / "out", {})
        self.assertFalse((self.root / "escape").exists())

    def test_archive_symlinks_are_rejected(self):
        archive = self.root / "symlink.zip"
        entry = zipfile.ZipInfo("azmcp")
        entry.create_system = 3
        entry.external_attr = (stat.S_IFLNK | 0o777) << 16
        with zipfile.ZipFile(archive, "w") as release:
            release.writestr(entry, "../escape")
        with self.assertRaisesRegex(RuntimeError, "symbolic link"):
            launcher._extract(archive, self.root / "out", self.artifact["files"])

    def test_missing_unexpected_and_corrupt_archive_files_are_rejected(self):
        for files, message in (
            ({"azmcp": self.files["azmcp"]}, "missing required"),
            ({**self.files, "extra": b"extra"}, "Unexpected release archive entry"),
            ({**self.files, "azmcp": b"x" * len(self.files["azmcp"])}, "integrity validation"),
        ):
            with self.subTest(message=message), tempfile.TemporaryDirectory(dir=self.root) as temporary:
                archive = Path(temporary) / "release.zip"
                archive.write_bytes(self.make_zip(files))
                output = Path(temporary) / "out"
                output.mkdir()
                with self.assertRaisesRegex(RuntimeError, message):
                    launcher._extract(archive, output, self.artifact["files"])

    def test_forwards_arguments_streams_and_exit_code(self):
        executable = launcher.get_executable_path()
        with patch("subprocess.run") as run:
            run.return_value.returncode = 17
            self.assertEqual(launcher.run_executable(["server", "start", "a b", ";"]), 17)
            run.assert_called_once_with(
                [str(executable), "server", "start", "a b", ";"],
                stdin=sys.stdin, stdout=sys.stdout, stderr=sys.stderr,
            )
        with patch("subprocess.run", side_effect=PermissionError("denied")), patch(
            "sys.stderr", new_callable=io.StringIO
        ):
            self.assertEqual(launcher.run_executable([]), 126)
        with patch("subprocess.run", side_effect=KeyboardInterrupt):
            self.assertEqual(launcher.run_executable([]), 130)

    @unittest.skipIf(os.name == "nt", "Unix executable fixture")
    def test_real_child_process_exit_status(self):
        self.files = {"azmcp": b"#!/bin/sh\nexit 17\n"}
        self.payload = self.make_zip(self.files)
        self.artifact.update(
            sha256=hashlib.sha256(self.payload).hexdigest(), size=len(self.payload),
            files={"azmcp": {"sha256": hashlib.sha256(self.files["azmcp"]).hexdigest(), "size": len(self.files["azmcp"])}},
        )
        self.write_manifest()
        self.assertEqual(launcher.run_executable(["server", "start"]), 17)


class PlatformTests(unittest.TestCase):
    def test_platform_mapping(self):
        for system, machine, expected in (
            ("Windows", "AMD64", "win32-x64"), ("Windows", "ARM64", "win32-arm64"),
            ("Darwin", "x86_64", "darwin-x64"), ("Darwin", "arm64", "darwin-arm64"),
            ("Linux", "x86_64", "linux-x64"), ("Linux", "aarch64", "linux-arm64"),
        ):
            with self.subTest(system=system, machine=machine), patch("platform.system", return_value=system), patch(
                "platform.machine", return_value=machine
            ):
                self.assertEqual(launcher._platform_key(), expected)

    def test_unsupported_platforms(self):
        for system, machine, maxsize in (("FreeBSD", "amd64", 2**63), ("Linux", "s390x", 2**63), ("Windows", "AMD64", 2**31)):
            with self.subTest(system=system, machine=machine), patch("platform.system", return_value=system), patch(
                "platform.machine", return_value=machine
            ), patch("sys.maxsize", maxsize):
                with self.assertRaisesRegex(RuntimeError, "Unsupported platform"):
                    launcher._platform_key()

    def test_cache_override(self):
        with patch.dict(os.environ, {"AZURE_MCP_CACHE_DIR": str(Path.home() / "custom-cache")}):
            self.assertEqual(launcher._cache_root(), Path.home() / "custom-cache")

    def test_rejects_insecure_or_unofficial_redirects(self):
        handler = launcher._ReleaseRedirectHandler()
        request = launcher.urllib.request.Request("https://github.com/microsoft/mcp")
        for url in ("http://github.com/asset", "https://example.com/asset"):
            with self.subTest(url=url), self.assertRaisesRegex(RuntimeError, "official HTTPS"):
                handler.redirect_request(request, None, 302, "Found", {}, url)
        redirect = handler.redirect_request(
            request, None, 302, "Found", {}, "https://release-assets.githubusercontent.com/asset"
        )
        self.assertEqual(redirect.full_url, "https://release-assets.githubusercontent.com/asset")

    def test_unix_signal_exit_code(self):
        with patch.object(launcher, "run_executable", return_value=-15), self.assertRaises(SystemExit) as exit_context:
            launcher.main()
        self.assertEqual(exit_context.exception.code, 143)


if __name__ == "__main__":
    unittest.main()
