# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

"""
Locust load test for the Azure MCP Server.

Simulates MCP clients that:
  1. Initialize a session  (POST /mcp  –  method: initialize)
  2. Send initialized notification (POST /mcp  –  method: notifications/initialized)
  3. Repeatedly call the azmcp_storage_account_get tool (POST /mcp  –  method: tools/call)

Environment variables
---------------------
MCP_SUBSCRIPTION : Azure subscription ID or name to pass to the tool
MCP_PATH         : (optional) MCP endpoint path. Defaults to '/mcp'.

The MCP server base URL is supplied to Locust via the `host` field in
config.yaml (or `--host` on the CLI); `self.client.post(MCP_PATH, ...)` resolves
against that host automatically.
"""

from __future__ import annotations

import json
import os
import uuid

from locust import HttpUser, between, task

MCP_PATH = os.environ.get("MCP_PATH", "/mcp")


def _jsonrpc_request(method: str, params: dict | None = None, *, notify: bool = False) -> dict:
    """Build a JSON-RPC 2.0 request envelope."""
    msg: dict = {
        "jsonrpc": "2.0",
        "method": method,
    }
    if params is not None:
        msg["params"] = params
    if not notify:
        msg["id"] = str(uuid.uuid4())
    return msg


class McpUser(HttpUser):
    """Simulated MCP client that exercises the storage-account-get tool."""

    wait_time = between(1, 3)

    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self.session_id: str | None = None
        self.subscription: str = os.environ.get("MCP_SUBSCRIPTION", "")

    def on_start(self) -> None:
        """Perform the MCP session handshake before running tasks.

        If the handshake fails (no session id returned, non-200, etc.) the
        user is stopped immediately. This keeps handshake regressions from
        masquerading as `tools/call` failures, which would otherwise be the
        only symptom — every subsequent call from this user would be sent
        without `Mcp-Session-Id` and rejected by the server.
        """
        if not self._initialize():
            # Stop this user so the failure surfaces as an `initialize`
            # error rather than a cascade of `tools/call` errors. Locust
            # will spawn a replacement to keep the user count steady.
            self.stop(force=True)
            return
        self._send_initialized()

    def _initialize(self) -> bool:
        """Send the 'initialize' request and capture the session id.

        Returns True on a successful handshake (200 + ``Mcp-Session-Id``
        header populated), False otherwise.
        """
        payload = _jsonrpc_request(
            "initialize",
            {
                "protocolVersion": "2025-03-26",
                "capabilities": {},
                "clientInfo": {"name": "locust-load-test", "version": "1.0.0"},
            },
        )
        with self.client.post(
            MCP_PATH,
            json=payload,
            headers={"Content-Type": "application/json", "Accept": "application/json, text/event-stream"},
            name="initialize",
            catch_response=True,
        ) as resp:
            if resp.status_code != 200:
                resp.failure(f"initialize returned {resp.status_code}")
                return False
            self.session_id = resp.headers.get("Mcp-Session-Id")
            if not self.session_id:
                resp.failure("Missing Mcp-Session-Id header in initialize response")
                return False
            return True

    def _send_initialized(self) -> None:
        """Send the 'notifications/initialized' notification."""
        payload = _jsonrpc_request("notifications/initialized", notify=True)
        headers = {
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream",
        }
        if self.session_id:
            headers["Mcp-Session-Id"] = self.session_id

        with self.client.post(
            MCP_PATH,
            json=payload,
            headers=headers,
            name="notifications/initialized",
            catch_response=True,
        ) as resp:
            if resp.status_code not in (200, 202, 204):
                resp.failure(f"notifications/initialized returned {resp.status_code}")

    @task
    def call_storage_account_get(self) -> None:
        """Invoke the azmcp_storage_account_get tool."""
        payload = _jsonrpc_request(
            "tools/call",
            {
                "name": "azmcp_storage_account_get",
                "arguments": {
                    "subscription": self.subscription,
                },
            },
        )
        headers = {
            "Content-Type": "application/json",
            "Accept": "application/json, text/event-stream",
        }
        if self.session_id:
            headers["Mcp-Session-Id"] = self.session_id

        with self.client.post(
            MCP_PATH,
            json=payload,
            headers=headers,
            name="tools/call [azmcp_storage_account_get]",
            catch_response=True,
        ) as resp:
            if resp.status_code == 200:
                try:
                    body = resp.json() if resp.headers.get("content-type", "").startswith("application/json") else None
                    if body and body.get("error"):
                        resp.failure(f"JSON-RPC error: {json.dumps(body['error'])}")
                except Exception:
                    pass  # SSE or other streaming response – treat as success
            else:
                resp.failure(f"tools/call returned {resp.status_code}")
