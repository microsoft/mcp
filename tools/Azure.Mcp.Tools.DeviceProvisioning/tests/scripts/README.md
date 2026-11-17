# DPS X.509 fleet live-test setup

`setup_x509_fleet.py` creates a disposable Azure environment for live testing:

1. Resource group.
2. IoT Hub.
3. Device Provisioning Service instance linked to the Hub.
4. Proof-of-possession-verified signing CA.
5. X.509 enrollment group.
6. CA-signed device certificates.
7. Real DPS device registrations.
8. Public-only manifest for the fleet onboarding-validation skill.

## Prerequisites

- Python 3.10 or later.
- Azure CLI authenticated with `az login`.
- `azure-iot` Azure CLI extension.
- OpenSSL.
- Contributor plus User Access Administrator, or Owner, in the selected subscription. The script
  grants the MCP identity the read-only Device Provisioning Service Data Reader role.

```bash
az extension add --name azure-iot
python3 setup_x509_fleet.py \
  --subscription <subscription> \
  --location centralus \
  --device-count 10
```

For a true pre-provisioning test, create the environment without registering devices:

```bash
python3 setup_x509_fleet.py \
  --subscription <subscription> \
  --location centralus \
  --device-count 10 \
  --skip-registration
```

Run MCP onboarding validation against the generated `fleet-manifest.public.json`. After the
preflight result has no blockers, validate the retained artifacts without making changes:

```bash
python3 setup_x509_fleet.py \
  --register-existing <output-directory> \
  --dry-run
```

Then register that exact validated cohort:

```bash
python3 setup_x509_fleet.py \
  --register-existing <output-directory>
```

`--register-existing` does not recreate Azure resources, certificates, or enrollments. It reads the
retained environment state, confirms the live DPS ID scope, verifies that the public manifest and
local certificate files still match, then uses each device's local private key for the real DPS TLS
registration. Private keys are never sent to MCP.

When Azure CLI is authenticated as a service principal or managed identity, provide the object ID
that will execute MCP validation:

```bash
python3 setup_x509_fleet.py \
  --subscription <subscription> \
  --mcp-principal-id <object-id> \
  --device-count 10
```

The script writes artifacts to a new directory under the system temporary directory unless
`--output-dir` is supplied. The important output is:

```text
fleet-manifest.public.json
```

Give that file to the `azure-device-provisioning-onboarding` skill. The skill divides larger
manifests into batches of at most 10 and calls the MCP validation tool.

Private device and CA keys remain under the generated `private/` directory. Never attach, commit,
or pass that directory to MCP. Resources are retained for validation; the script prints an explicit
resource-group cleanup command after setup completes.
