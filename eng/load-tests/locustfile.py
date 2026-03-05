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
MCP_SERVER_URL   : Base URL of the MCP server  (set by the pipeline)
MCP_SUBSCRIPTION : Azure subscription ID or name to pass to the tool
"""

from __future__ import annotations

import json
import os
import uuid

from locust import HttpUser, between, task


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
        """Perform the MCP session handshake before running tasks."""
        self._initialize()
        self._send_initialized()

    def _initialize(self) -> None:
        """Send the 'initialize' request and capture the session id."""
        payload = _jsonrpc_request(
            "initialize",
            {
                "protocolVersion": "2025-03-26",
                "capabilities": {},
                "clientInfo": {"name": "locust-load-test", "version": "1.0.0"},
            },
        )
        with self.client.post(
            "/mcp",
            json=payload,
            headers={"Content-Type": "application/json", "Accept": "application/json, text/event-stream"},
            name="initialize",
            catch_response=True,
        ) as resp:
            if resp.status_code == 200:
                self.session_id = resp.headers.get("Mcp-Session-Id")
                if not self.session_id:
                    resp.failure("Missing Mcp-Session-Id header in initialize response")
            else:
                resp.failure(f"initialize returned {resp.status_code}")

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
            "/mcp",
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
            "/mcp",
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
