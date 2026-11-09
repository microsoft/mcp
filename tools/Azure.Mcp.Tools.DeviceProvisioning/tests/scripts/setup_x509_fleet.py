#!/usr/bin/env python3
"""Create or register a disposable DPS X.509 fleet for MCP live validation.

The script intentionally uses only Python's standard library plus the Azure CLI,
the azure-iot CLI extension, and OpenSSL. It creates a resource group, IoT Hub,
DPS instance, verified signing CA, X.509 enrollment group, and CA-signed device
certificates. It then performs real DPS HTTPS registrations and writes a
public-only fleet manifest accepted by the onboarding-validation skill.

Private keys remain in the output directory and are never added to the manifest.
Resources are retained so the MCP validation can run afterward.

Use --skip-registration for a pre-provisioning validation environment, then use
--register-existing with the retained output directory to register that exact
validated cohort without recreating resources or certificates.
"""

from __future__ import annotations

import argparse
import concurrent.futures
import datetime as dt
import hashlib
import http.client
import json
import os
from pathlib import Path
import re
import secrets
import shutil
import ssl
import subprocess
import sys
import tempfile
import time
import urllib.parse
from typing import Any


DEVICE_REGISTER_API_VERSION = "2019-03-31"
DEFAULT_PROVISIONING_HOST = "global.azure-devices-provisioning.net"
DPS_DATA_READER_ROLE_ID = "10745317-c249-44a1-a5ce-3a4353c0bbd8"
PRIVATE_KEY_MARKERS = (
    "BEGIN PRIVATE KEY",
    "BEGIN RSA PRIVATE KEY",
    "BEGIN EC PRIVATE KEY",
    "BEGIN ENCRYPTED PRIVATE KEY",
)


class ScriptError(RuntimeError):
    """Expected setup or registration failure."""


def parse_args() -> argparse.Namespace:
    timestamp = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%d%H%M%S")
    suffix = secrets.token_hex(3)
    default_prefix = f"mcpx509{timestamp[-8:]}{suffix}"
    parser = argparse.ArgumentParser(
        description=(
            "Create a disposable DPS X.509 enrollment-group fleet, or register "
            "a previously generated fleet after MCP preflight validation."
        )
    )
    parser.add_argument(
        "--subscription",
        help="Azure subscription ID or name. Defaults to the current Azure CLI subscription.",
    )
    parser.add_argument("--location", default="centralus", help="Azure resource location.")
    parser.add_argument(
        "--prefix",
        default=default_prefix,
        help="Lowercase alphanumeric naming prefix used for all generated resources.",
    )
    parser.add_argument(
        "--device-count",
        type=int,
        default=10,
        help="Number of X.509 devices to create and register. Default: 10.",
    )
    parser.add_argument(
        "--registration-prefix",
        default="mcp-fleet-device",
        help="Registration ID prefix. Default: mcp-fleet-device.",
    )
    parser.add_argument(
        "--output-dir",
        type=Path,
        help="Artifact directory. Defaults to a new directory under the system temp folder.",
    )
    parser.add_argument(
        "--max-workers",
        type=int,
        default=5,
        help="Maximum concurrent DPS registrations. Default: 5.",
    )
    parser.add_argument(
        "--registration-timeout-seconds",
        type=int,
        default=300,
        help="Per-device DPS registration timeout. Default: 300.",
    )
    parser.add_argument(
        "--skip-registration",
        action="store_true",
        help="Create resources and certificates without performing device registration.",
    )
    parser.add_argument(
        "--register-existing",
        type=Path,
        metavar="OUTPUT_DIR",
        help=(
            "Register a previously generated cohort from OUTPUT_DIR without recreating "
            "resources or certificates."
        ),
    )
    parser.add_argument(
        "--dry-run",
        action="store_true",
        help=(
            "With --register-existing, validate the retained environment and local "
            "artifacts without registering devices."
        ),
    )
    parser.add_argument(
        "--mcp-principal-id",
        help=(
            "Object ID that will run MCP validation. Defaults to the signed-in Azure CLI user. "
            "The script grants it Device Provisioning Service Data Reader on the new DPS instance."
        ),
    )
    return parser.parse_args()


def validate_args(args: argparse.Namespace) -> None:
    if args.register_existing:
        if args.skip_registration:
            raise ScriptError(
                "--register-existing cannot be combined with --skip-registration."
            )
        if args.output_dir:
            raise ScriptError(
                "--register-existing cannot be combined with --output-dir; "
                "pass the retained output directory to --register-existing."
            )
    elif args.dry_run:
        raise ScriptError("--dry-run requires --register-existing.")
    if args.device_count < 1:
        raise ScriptError("--device-count must be at least 1.")
    if args.max_workers < 1:
        raise ScriptError("--max-workers must be at least 1.")
    if args.registration_timeout_seconds < 30:
        raise ScriptError("--registration-timeout-seconds must be at least 30.")
    if not re.fullmatch(r"[a-z][a-z0-9]{2,23}", args.prefix):
        raise ScriptError(
            "--prefix must be 3-24 lowercase alphanumeric characters and start with a letter."
        )
    if not re.fullmatch(r"[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?", args.registration_prefix):
        raise ScriptError(
            "--registration-prefix must use lowercase letters, numbers, or hyphens and "
            "start and end with a letter or number."
        )


def require_executable(name: str) -> None:
    if shutil.which(name) is None:
        raise ScriptError(f"Required executable '{name}' was not found on PATH.")


def redact(text: str) -> str:
    patterns = (
        r"(SharedAccessKey=)[^;\s]+",
        r"(SharedAccessSignature\s+)[^\s]+",
        r'("primaryKey"\s*:\s*")[^"]+',
        r'("secondaryKey"\s*:\s*")[^"]+',
    )
    redacted = text
    for pattern in patterns:
        redacted = re.sub(pattern, r"\1<redacted>", redacted, flags=re.IGNORECASE)
    return redacted


def run(
    command: list[str],
    *,
    capture_output: bool = True,
    env: dict[str, str] | None = None,
) -> subprocess.CompletedProcess[str]:
    result = subprocess.run(
        command,
        check=False,
        capture_output=capture_output,
        text=True,
        encoding="utf-8",
        env=env,
    )
    if result.returncode != 0:
        detail = redact((result.stderr or result.stdout or "").strip())
        raise ScriptError(f"Command failed ({command[0]}): {detail[:1200]}")
    return result


def az_json(args: argparse.Namespace, *arguments: str) -> Any:
    command = ["az", *arguments]
    if args.subscription:
        command += ["--subscription", args.subscription]
    command += ["--only-show-errors", "--output", "json"]
    result = run(command)
    if not result.stdout.strip():
        return None
    return json.loads(result.stdout)


def az_tsv(args: argparse.Namespace, *arguments: str) -> str:
    command = ["az", *arguments]
    if args.subscription:
        command += ["--subscription", args.subscription]
    command += ["--only-show-errors", "--output", "tsv"]
    return run(command).stdout.strip()


def openssl(*arguments: str) -> None:
    run(["openssl", *arguments])


def create_output_directory(args: argparse.Namespace) -> Path:
    if args.output_dir:
        output_dir = args.output_dir.expanduser().resolve()
        if output_dir.exists() and any(output_dir.iterdir()):
            raise ScriptError(f"Output directory must be empty: {output_dir}")
        output_dir.mkdir(parents=True, exist_ok=True)
    else:
        output_dir = Path(
            tempfile.mkdtemp(prefix=f"dps-mcp-x509-{args.prefix}-")
        ).resolve()
    os.chmod(output_dir, 0o700)
    (output_dir / "private").mkdir()
    (output_dir / "public").mkdir()
    os.chmod(output_dir / "private", 0o700)
    return output_dir


def read_json_object(path: Path, description: str) -> dict[str, Any]:
    if not path.is_file():
        raise ScriptError(f"{description} was not found: {path}")
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        raise ScriptError(f"{description} is not valid JSON: {path}") from exc
    if not isinstance(value, dict):
        raise ScriptError(f"{description} must contain a JSON object: {path}")
    return value


def require_string(value: Any, field: str, source: Path) -> str:
    if not isinstance(value, str) or not value.strip():
        raise ScriptError(f"{source} is missing required string field '{field}'.")
    return value


def ensure_azure_prerequisites(args: argparse.Namespace) -> str:
    require_executable("az")
    require_executable("openssl")
    account = az_json(args, "account", "show")
    if not isinstance(account, dict) or not account.get("id"):
        raise ScriptError("Azure CLI is not authenticated. Run 'az login' first.")
    extension = az_json(
        argparse.Namespace(subscription=None),
        "extension",
        "show",
        "--name",
        "azure-iot",
    )
    if not isinstance(extension, dict):
        raise ScriptError(
            "The azure-iot CLI extension is required. Install it with "
            "'az extension add --name azure-iot'."
        )
    return str(account["id"])


def load_existing_fleet(
    args: argparse.Namespace,
) -> tuple[
    Path,
    dict[str, Any],
    dict[str, Any],
    list[dict[str, Any]],
    str,
    str,
]:
    output_dir = args.register_existing.expanduser().resolve()
    if not output_dir.is_dir():
        raise ScriptError(f"Existing fleet output directory was not found: {output_dir}")

    state_path = output_dir / "test-environment.json"
    manifest_path = output_dir / "fleet-manifest.public.json"
    state = read_json_object(state_path, "Environment state")
    manifest = read_json_object(manifest_path, "Public fleet manifest")

    subscription = require_string(state.get("subscription"), "subscription", state_path)
    resource_group = require_string(state.get("resourceGroup"), "resourceGroup", state_path)
    dps_name = require_string(state.get("dps"), "dps", state_path)
    enrollment_group = require_string(
        state.get("enrollmentGroupId"),
        "enrollmentGroupId",
        state_path,
    )
    id_scope = require_string(state.get("idScope"), "idScope", state_path)
    hub_hostname = require_string(state.get("hubHostname"), "hubHostname", state_path)

    expected_context = {
        "subscription": subscription,
        "resourceGroup": resource_group,
        "service": dps_name,
        "enrollmentGroupId": enrollment_group,
        "intendedHub": hub_hostname,
    }
    for field, expected in expected_context.items():
        actual = manifest.get(field)
        if actual != expected:
            raise ScriptError(
                f"Manifest field '{field}' does not match retained environment state: "
                f"expected '{expected}', found '{actual}'."
            )

    manifest_digest = hashlib.sha256(manifest_path.read_bytes()).hexdigest()
    expected_digest = state.get("publicManifestSha256")
    if expected_digest and expected_digest != manifest_digest:
        raise ScriptError(
            "The public fleet manifest changed after setup. Start a new environment "
            "instead of registering modified evidence."
        )

    manifest_devices = manifest.get("devices")
    if not isinstance(manifest_devices, list) or not manifest_devices:
        raise ScriptError("The public fleet manifest must contain at least one device.")

    devices: list[dict[str, Any]] = []
    seen_registration_ids: set[str] = set()
    for item in manifest_devices:
        if not isinstance(item, dict):
            raise ScriptError("Every manifest device must be a JSON object.")
        registration_id = require_string(
            item.get("registrationId"),
            "devices[].registrationId",
            manifest_path,
        )
        if not re.fullmatch(
            r"[A-Za-z0-9](?:[A-Za-z0-9._-]{0,126}[A-Za-z0-9])?",
            registration_id,
        ):
            raise ScriptError(
                f"Unsafe or invalid registration ID in retained manifest: {registration_id}"
            )
        normalized_id = registration_id.casefold()
        if normalized_id in seen_registration_ids:
            raise ScriptError(f"Duplicate registration ID in retained manifest: {registration_id}")
        seen_registration_ids.add(normalized_id)

        certificate_chain = require_string(
            item.get("certificateChain"),
            "devices[].certificateChain",
            manifest_path,
        )
        if any(marker in certificate_chain for marker in PRIVATE_KEY_MARKERS):
            raise ScriptError(
                f"Public manifest contains private-key material for {registration_id}."
            )

        chain_path = (output_dir / "public" / f"{registration_id}-chain.pem").resolve()
        key_path = (output_dir / "private" / f"{registration_id}-key.pem").resolve()
        if not chain_path.is_relative_to(output_dir / "public") or not key_path.is_relative_to(
            output_dir / "private"
        ):
            raise ScriptError(f"Device artifact path escaped the output directory: {registration_id}")
        if not chain_path.is_file():
            raise ScriptError(f"Public certificate chain was not found: {chain_path}")
        if not key_path.is_file():
            raise ScriptError(f"Private device key was not found: {key_path}")
        if chain_path.read_text(encoding="utf-8") != certificate_chain:
            raise ScriptError(
                f"Public certificate chain does not match the retained manifest: {registration_id}"
            )
        private_key = key_path.read_text(encoding="utf-8")
        if not any(marker in private_key for marker in PRIVATE_KEY_MARKERS):
            raise ScriptError(f"Private device key is not a supported PEM key: {key_path}")

        devices.append(
            {
                "registrationId": registration_id,
                "certificateChain": certificate_chain,
                "_certPath": str(chain_path),
                "_keyPath": str(key_path),
            }
        )

    args.subscription = args.subscription or subscription
    live_id_scope = az_tsv(
        args,
        "iot",
        "dps",
        "show",
        "--name",
        dps_name,
        "--resource-group",
        resource_group,
        "--query",
        "properties.idScope",
    )
    if live_id_scope != id_scope:
        raise ScriptError(
            "The live DPS ID scope does not match the retained environment state. "
            "Do not register against a replaced or different DPS instance."
        )

    return output_dir, state, manifest, devices, id_scope, hub_hostname


def write_json_atomic(path: Path, value: dict[str, Any]) -> None:
    temporary_path = path.with_suffix(path.suffix + ".tmp")
    temporary_path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    temporary_path.replace(path)


def resource_names(prefix: str) -> dict[str, str]:
    return {
        "resourceGroup": f"rg-mcp-dps-{prefix}",
        "hub": f"{prefix}-hub",
        "dps": f"{prefix}-dps",
        "certificate": f"{prefix}-root",
        "enrollmentGroup": f"{prefix}-group",
    }


def provision_resources(
    args: argparse.Namespace,
    names: dict[str, str],
) -> tuple[str, str]:
    print(f"Creating resource group {names['resourceGroup']}...")
    az_json(
        args,
        "group",
        "create",
        "--name",
        names["resourceGroup"],
        "--location",
        args.location,
        "--tags",
        "purpose=mcp-dps-fleet-e2e",
        "doNotDelete=true",
    )

    print(f"Creating IoT Hub {names['hub']}...")
    az_json(
        args,
        "iot",
        "hub",
        "create",
        "--name",
        names["hub"],
        "--resource-group",
        names["resourceGroup"],
        "--location",
        args.location,
        "--sku",
        "S1",
        "--unit",
        "1",
        "--partition-count",
        "2",
        "--disable-local-auth",
        "false",
    )

    print(f"Creating DPS instance {names['dps']}...")
    az_json(
        args,
        "iot",
        "dps",
        "create",
        "--name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--location",
        args.location,
        "--sku",
        "S1",
        "--unit",
        "1",
        "--disable-local-auth",
        "false",
    )

    print("Linking IoT Hub to DPS...")
    az_json(
        args,
        "iot",
        "dps",
        "linked-hub",
        "create",
        "--dps-name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--hub-name",
        names["hub"],
        "--hub-resource-group",
        names["resourceGroup"],
        "--authentication-type",
        "KeyBased",
        "--hostname-type",
        "classic",
    )

    id_scope = az_tsv(
        args,
        "iot",
        "dps",
        "show",
        "--name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--query",
        "properties.idScope",
    )
    hub_hostname = az_tsv(
        args,
        "iot",
        "hub",
        "show",
        "--name",
        names["hub"],
        "--resource-group",
        names["resourceGroup"],
        "--query",
        "properties.hostName",
    )
    if not id_scope or not hub_hostname:
        raise ScriptError("Azure resource setup did not return the DPS ID scope and Hub hostname.")
    return id_scope, hub_hostname


def grant_mcp_data_reader(
    args: argparse.Namespace,
    names: dict[str, str],
) -> str:
    principal_id = args.mcp_principal_id
    if not principal_id:
        principal_id = az_tsv(
            argparse.Namespace(subscription=None),
            "ad",
            "signed-in-user",
            "show",
            "--query",
            "id",
        )
    if not principal_id:
        raise ScriptError(
            "Could not determine the MCP principal. Pass --mcp-principal-id when Azure CLI "
            "is authenticated as a service principal or managed identity."
        )
    dps_resource_id = az_tsv(
        args,
        "iot",
        "dps",
        "show",
        "--name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--query",
        "id",
    )
    if not dps_resource_id:
        raise ScriptError("DPS resource lookup did not return a resource ID.")
    print("Granting the MCP identity read-only DPS data-plane access...")
    az_json(
        args,
        "role",
        "assignment",
        "create",
        "--assignee-object-id",
        principal_id,
        "--role",
        DPS_DATA_READER_ROLE_ID,
        "--scope",
        dps_resource_id,
    )
    return principal_id


def generate_root_ca(output_dir: Path, prefix: str) -> tuple[Path, Path]:
    cert_path = output_dir / "public" / f"{prefix}-root-cert.pem"
    key_path = output_dir / "private" / f"{prefix}-root-key.pem"
    openssl(
        "req",
        "-x509",
        "-newkey",
        "rsa:2048",
        "-nodes",
        "-keyout",
        str(key_path),
        "-out",
        str(cert_path),
        "-sha256",
        "-days",
        "30",
        "-subj",
        f"/CN={prefix}-root",
        "-addext",
        "basicConstraints=critical,CA:TRUE",
        "-addext",
        "keyUsage=critical,keyCertSign,cRLSign",
    )
    os.chmod(key_path, 0o600)
    return cert_path, key_path


def verify_root_ca(
    args: argparse.Namespace,
    names: dict[str, str],
    output_dir: Path,
    root_cert: Path,
    root_key: Path,
) -> None:
    print("Uploading and verifying the DPS signing CA...")
    created = az_json(
        args,
        "iot",
        "dps",
        "certificate",
        "create",
        "--dps-name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--name",
        names["certificate"],
        "--path",
        str(root_cert),
    )
    etag = str((created or {}).get("etag", ""))
    if not etag:
        raise ScriptError("DPS certificate creation did not return an ETag.")

    generated = az_json(
        args,
        "iot",
        "dps",
        "certificate",
        "generate-verification-code",
        "--dps-name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--name",
        names["certificate"],
        "--etag",
        etag,
    )
    properties = (generated or {}).get("properties") or {}
    verification_code = str(properties.get("verificationCode", ""))
    verification_etag = str((generated or {}).get("etag") or etag)
    if not verification_code:
        raise ScriptError("DPS certificate verification-code response contained no code.")

    verification_key = output_dir / "private" / "verification-key.pem"
    verification_csr = output_dir / "private" / "verification.csr"
    verification_cert = output_dir / "public" / "verification-cert.pem"
    openssl("genrsa", "-out", str(verification_key), "2048")
    openssl(
        "req",
        "-new",
        "-key",
        str(verification_key),
        "-out",
        str(verification_csr),
        "-subj",
        f"/CN={verification_code}",
    )
    openssl(
        "x509",
        "-req",
        "-in",
        str(verification_csr),
        "-CA",
        str(root_cert),
        "-CAkey",
        str(root_key),
        "-set_serial",
        "2",
        "-out",
        str(verification_cert),
        "-days",
        "2",
        "-sha256",
    )
    os.chmod(verification_key, 0o600)
    az_json(
        args,
        "iot",
        "dps",
        "certificate",
        "verify",
        "--dps-name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--name",
        names["certificate"],
        "--path",
        str(verification_cert),
        "--etag",
        verification_etag,
    )


def create_enrollment_group(
    args: argparse.Namespace,
    names: dict[str, str],
    hub_hostname: str,
) -> None:
    print(f"Creating X.509 enrollment group {names['enrollmentGroup']}...")
    az_json(
        args,
        "iot",
        "dps",
        "enrollment-group",
        "create",
        "--dps-name",
        names["dps"],
        "--resource-group",
        names["resourceGroup"],
        "--enrollment-id",
        names["enrollmentGroup"],
        "--ca-name",
        names["certificate"],
        "--allocation-policy",
        "static",
        "--iot-hubs",
        hub_hostname,
        "--provisioning-status",
        "enabled",
    )


def generate_devices(
    args: argparse.Namespace,
    output_dir: Path,
    root_cert: Path,
    root_key: Path,
) -> list[dict[str, Any]]:
    extension_file = output_dir / "private" / "device-extensions.cnf"
    extension_file.write_text(
        "[device]\n"
        "basicConstraints=critical,CA:FALSE\n"
        "keyUsage=critical,digitalSignature\n"
        "extendedKeyUsage=clientAuth\n",
        encoding="utf-8",
    )
    devices: list[dict[str, Any]] = []
    root_pem = root_cert.read_text(encoding="utf-8")
    for index in range(1, args.device_count + 1):
        registration_id = f"{args.registration_prefix}-{index:04d}"
        key_path = output_dir / "private" / f"{registration_id}-key.pem"
        csr_path = output_dir / "private" / f"{registration_id}.csr"
        leaf_path = output_dir / "public" / f"{registration_id}-leaf.pem"
        chain_path = output_dir / "public" / f"{registration_id}-chain.pem"
        openssl("ecparam", "-name", "prime256v1", "-genkey", "-noout", "-out", str(key_path))
        openssl(
            "req",
            "-new",
            "-key",
            str(key_path),
            "-out",
            str(csr_path),
            "-subj",
            f"/CN={registration_id}",
        )
        openssl(
            "x509",
            "-req",
            "-in",
            str(csr_path),
            "-CA",
            str(root_cert),
            "-CAkey",
            str(root_key),
            "-set_serial",
            str(1000 + index),
            "-out",
            str(leaf_path),
            "-days",
            "7",
            "-sha256",
            "-extfile",
            str(extension_file),
            "-extensions",
            "device",
        )
        os.chmod(key_path, 0o600)
        leaf_pem = leaf_path.read_text(encoding="utf-8")
        chain_pem = leaf_pem + root_pem
        if any(marker in chain_pem for marker in PRIVATE_KEY_MARKERS):
            raise ScriptError(f"Public chain unexpectedly contains private-key material: {chain_path}")
        chain_path.write_text(chain_pem, encoding="utf-8")
        devices.append(
            {
                "registrationId": registration_id,
                "certificateChain": chain_pem,
                "_certPath": str(chain_path),
                "_keyPath": str(key_path),
            }
        )
    return devices


def request_json(
    method: str,
    host: str,
    path: str,
    context: ssl.SSLContext,
    body: dict[str, Any] | None = None,
) -> tuple[int, dict[str, str], Any]:
    connection = http.client.HTTPSConnection(host, timeout=30, context=context)
    encoded = json.dumps(body).encode("utf-8") if body is not None else None
    headers = {"Content-Type": "application/json; charset=utf-8"}
    try:
        connection.request(method, path, body=encoded, headers=headers)
        response = connection.getresponse()
        payload = response.read()
        try:
            parsed = json.loads(payload) if payload else None
        except json.JSONDecodeError:
            if response.status < 400:
                raise
            parsed = payload.decode("utf-8", errors="replace")
        return response.status, {key.lower(): value for key, value in response.getheaders()}, parsed
    finally:
        connection.close()


def retry_after(headers: dict[str, str], default: float) -> float:
    try:
        return max(0.0, float(headers.get("retry-after", default)))
    except ValueError:
        return default


def register_device(
    device: dict[str, Any],
    *,
    id_scope: str,
    provisioning_host: str,
    timeout_seconds: int,
) -> dict[str, Any]:
    registration_id = str(device["registrationId"])
    context = ssl.create_default_context()
    context.minimum_version = ssl.TLSVersion.TLSv1_2
    context.load_cert_chain(
        certfile=str(device["_certPath"]),
        keyfile=str(device["_keyPath"]),
    )
    encoded_scope = urllib.parse.quote(id_scope, safe="")
    encoded_registration = urllib.parse.quote(registration_id, safe="")
    base_path = f"/{encoded_scope}/registrations/{encoded_registration}"
    register_path = f"{base_path}/register?api-version={DEVICE_REGISTER_API_VERSION}"
    deadline = time.monotonic() + timeout_seconds

    while True:
        try:
            status, headers, payload = request_json(
                "PUT",
                provisioning_host,
                register_path,
                context,
                {"registrationId": registration_id},
            )
        except (OSError, ssl.SSLError, TimeoutError) as exc:
            if time.monotonic() + 5 >= deadline:
                raise ScriptError(f"{registration_id}: registration transport failure: {exc}") from exc
            time.sleep(5)
            continue
        if status in (200, 202):
            break
        if status == 429 or status >= 500:
            delay = retry_after(headers, 5)
            if time.monotonic() + delay >= deadline:
                raise ScriptError(f"{registration_id}: registration timed out after HTTP {status}.")
            time.sleep(delay)
            continue
        raise ScriptError(
            f"{registration_id}: DPS register returned HTTP {status}: "
            f"{redact(json.dumps(payload))[:500]}"
        )

    operation_id = str((payload or {}).get("operationId", ""))
    if not operation_id:
        raise ScriptError(f"{registration_id}: DPS register response omitted operationId.")
    operation = urllib.parse.quote(operation_id, safe="")
    poll_path = (
        f"{base_path}/operations/{operation}?api-version={DEVICE_REGISTER_API_VERSION}"
    )

    while time.monotonic() < deadline:
        time.sleep(retry_after(headers, 3))
        try:
            status, headers, payload = request_json(
                "GET",
                provisioning_host,
                poll_path,
                context,
            )
        except (OSError, ssl.SSLError, TimeoutError):
            continue
        if status == 429 or status >= 500:
            continue
        if status >= 400:
            raise ScriptError(
                f"{registration_id}: DPS operation poll returned HTTP {status}: "
                f"{redact(json.dumps(payload))[:500]}"
            )
        operation_status = str((payload or {}).get("status", ""))
        if operation_status == "assigning":
            continue
        state = (payload or {}).get("registrationState") or {}
        result = {
            "registrationId": registration_id,
            "status": operation_status,
            "assignedHub": state.get("assignedHub"),
            "deviceId": state.get("deviceId"),
            "errorCode": state.get("errorCode"),
            "errorMessage": state.get("errorMessage"),
            "operationId": operation_id,
        }
        if operation_status != "assigned":
            raise ScriptError(
                f"{registration_id}: DPS registration ended as {operation_status}: "
                f"{state.get('errorCode')} {state.get('errorMessage')}"
            )
        return result
    raise ScriptError(f"{registration_id}: DPS registration did not settle before timeout.")


def register_devices(
    args: argparse.Namespace,
    devices: list[dict[str, Any]],
    id_scope: str,
) -> tuple[list[dict[str, Any]], list[dict[str, str]]]:
    if args.skip_registration:
        return [], []
    print(f"Registering {len(devices)} devices through DPS...")
    results: list[dict[str, Any]] = []
    failures: list[dict[str, str]] = []
    workers = min(args.max_workers, len(devices))
    with concurrent.futures.ThreadPoolExecutor(max_workers=workers) as executor:
        future_to_device = {
            executor.submit(
                register_device,
                device,
                id_scope=id_scope,
                provisioning_host=DEFAULT_PROVISIONING_HOST,
                timeout_seconds=args.registration_timeout_seconds,
            ): device
            for device in devices
        }
        for future in concurrent.futures.as_completed(future_to_device):
            device = future_to_device[future]
            registration_id = str(device["registrationId"])
            try:
                result = future.result()
            except Exception as exc:  # noqa: BLE001 - aggregate independent device failures
                failures.append(
                    {"registrationId": registration_id, "error": redact(str(exc))[:1000]}
                )
                print(f"  FAILED {registration_id}: {redact(str(exc))}")
            else:
                results.append(result)
                print(f"  assigned {registration_id} -> {result['assignedHub']}")
    results.sort(key=lambda item: item["registrationId"])
    failures.sort(key=lambda item: item["registrationId"])
    return results, failures


def register_existing_fleet(args: argparse.Namespace) -> int:
    ensure_azure_prerequisites(args)
    (
        output_dir,
        state,
        _,
        devices,
        id_scope,
        hub_hostname,
    ) = load_existing_fleet(args)

    print()
    print("Existing fleet registration plan")
    print(f"  DPS:              {state['dps']}")
    print(f"  Enrollment group: {state['enrollmentGroupId']}")
    print(f"  Intended Hub:     {hub_hostname}")
    print(f"  Device count:     {len(devices)}")
    print(f"  Artifact folder:  {output_dir}")
    print()
    if args.dry_run:
        print("Dry run completed. No device registrations were attempted.")
        return 0

    started_at = dt.datetime.now(dt.timezone.utc)
    registration_results, registration_failures = register_devices(
        args,
        devices,
        id_scope,
    )
    completed_at = dt.datetime.now(dt.timezone.utc)
    results_path = output_dir / "registration-results.json"
    write_json_atomic(
        results_path,
        {
            "schemaVersion": 1,
            "mode": "registerExisting",
            "startedAtUtc": started_at.isoformat(),
            "completedAtUtc": completed_at.isoformat(),
            "idScope": id_scope,
            "provisioningHost": DEFAULT_PROVISIONING_HOST,
            "succeeded": registration_results,
            "failed": registration_failures,
        },
    )

    state["registeredDeviceCount"] = len(registration_results)
    state["failedRegistrationCount"] = len(registration_failures)
    state["registrationResults"] = str(results_path)
    state["lastRegistrationMode"] = "registerExisting"
    state["lastRegistrationAttemptAtUtc"] = completed_at.isoformat()
    write_json_atomic(output_dir / "test-environment.json", state)

    print()
    print("Registration summary")
    print(f"  Assigned: {len(registration_results)}/{len(devices)}")
    print(f"  Failed:   {len(registration_failures)}/{len(devices)}")
    print(f"  Hub:      {hub_hostname}")
    print(f"  Results:  {results_path}")
    if registration_failures:
        print()
        print("Failed devices")
        for failure in registration_failures:
            print(f"  - {failure['registrationId']}: {failure['error']}")
    else:
        print()
        print("All devices were assigned. Rerun MCP onboarding validation with")
        print("registration-state lookup enabled to compare preflight predictions")
        print("with the actual DPS assignments.")

    return 1 if registration_failures else 0


def write_artifacts(
    args: argparse.Namespace,
    output_dir: Path,
    subscription_id: str,
    names: dict[str, str],
    id_scope: str,
    hub_hostname: str,
    devices: list[dict[str, Any]],
    registration_results: list[dict[str, Any]],
    registration_failures: list[dict[str, str]],
    mcp_principal_id: str,
) -> None:
    public_devices = [
        {
            "registrationId": device["registrationId"],
            "certificateChain": device["certificateChain"],
        }
        for device in devices
    ]
    manifest = {
        "subscription": subscription_id,
        "resourceGroup": names["resourceGroup"],
        "service": names["dps"],
        "enrollmentGroupId": names["enrollmentGroup"],
        "intendedHub": hub_hostname,
        "includeRegistrationState": True,
        "devices": public_devices,
    }
    manifest_path = output_dir / "fleet-manifest.public.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")

    results_path = output_dir / "registration-results.json"
    results_path.write_text(
        json.dumps(
            {
                "schemaVersion": 1,
                "mode": "setup",
                "idScope": id_scope,
                "provisioningHost": DEFAULT_PROVISIONING_HOST,
                "succeeded": registration_results,
                "failed": registration_failures,
            },
            indent=2,
        )
        + "\n",
        encoding="utf-8",
    )

    state = {
        "schemaVersion": 1,
        "subscription": subscription_id,
        "location": args.location,
        "resourceGroup": names["resourceGroup"],
        "hub": names["hub"],
        "hubHostname": hub_hostname,
        "dps": names["dps"],
        "idScope": id_scope,
        "certificate": names["certificate"],
        "enrollmentGroupId": names["enrollmentGroup"],
        "deviceCount": len(devices),
        "registeredDeviceCount": len(registration_results),
        "failedRegistrationCount": len(registration_failures),
        "mcpPrincipalId": mcp_principal_id,
        "mcpRole": "Device Provisioning Service Data Reader",
        "publicManifest": str(manifest_path),
        "registrationResults": str(results_path),
        "privateArtifactsDirectory": str(output_dir / "private"),
        "cleanupCommand": (
            f"az group delete --subscription {subscription_id} "
            f"--name {names['resourceGroup']} --yes"
        ),
    }
    digest = hashlib.sha256(manifest_path.read_bytes()).hexdigest()
    state["publicManifestSha256"] = digest
    state_path = output_dir / "test-environment.json"
    write_json_atomic(state_path, state)
    print()
    print(f"Public MCP fleet manifest: {manifest_path}")
    print(f"Manifest SHA-256: {digest}")
    print(f"Registration results: {results_path}")
    print(f"Environment details: {state_path}")
    print(f"Private keys (do not publish): {output_dir / 'private'}")
    print()
    print("Resources were retained for MCP validation.")
    print(f"Cleanup when finished: {state['cleanupCommand']}")


def main() -> int:
    args = parse_args()
    output_dir: Path | None = None
    try:
        validate_args(args)
        if args.register_existing:
            output_dir = args.register_existing.expanduser().resolve()
            return register_existing_fleet(args)
        output_dir = create_output_directory(args)
        subscription_id = ensure_azure_prerequisites(args)
        names = resource_names(args.prefix)
        id_scope, hub_hostname = provision_resources(args, names)
        mcp_principal_id = grant_mcp_data_reader(args, names)
        root_cert, root_key = generate_root_ca(output_dir, args.prefix)
        verify_root_ca(args, names, output_dir, root_cert, root_key)
        create_enrollment_group(args, names, hub_hostname)
        devices = generate_devices(args, output_dir, root_cert, root_key)
        registration_results, registration_failures = register_devices(args, devices, id_scope)
        write_artifacts(
            args,
            output_dir,
            subscription_id,
            names,
            id_scope,
            hub_hostname,
            devices,
            registration_results,
            registration_failures,
            mcp_principal_id,
        )
        return 1 if registration_failures else 0
    except (ScriptError, json.JSONDecodeError) as exc:
        print(f"ERROR: {redact(str(exc))}", file=sys.stderr)
        if output_dir:
            print(f"Artifacts retained at: {output_dir}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    sys.exit(main())
