# Security-Hardened Enterprise Fork (Fabric MCP)

This fork introduces an enterprise-focused mitigation for cross-tenant authentication issues reported in:

- https://github.com/microsoft/mcp/issues/1797

## Problem Context

In multi-tenant environments, users may be prompted with incomplete or incorrect login behavior when switching tenants (including tenant-specific MFA/2FA journeys), especially when cached authentication context from another tenant is reused.

## Mitigation Implemented

### Tenant Switcher behavior via isolated `TenantAwareCredential`

File changed:

- `core/Microsoft.Mcp.Core/src/Services/Azure/Authentication/TenantAwareCredential.cs`
- `core/Microsoft.Mcp.Core/src/Services/Azure/Authentication/SingleIdentityTokenCredentialProvider.cs`

And intentionally preserved:

- `core/Microsoft.Mcp.Core/src/Services/Azure/Authentication/CustomChainedCredential.cs`

Key behavior added:

1. **Isolated tenant-specific credential path**
   - `SingleIdentityTokenCredentialProvider` continues to use `CustomChainedCredential` for default auth.
   - Only explicit tenant-scoped requests are routed to `TenantAwareCredential`.

2. **Cross-tenant cache protection**
   - `TenantAwareCredential` avoids replaying prior `AuthenticationRecord` values that may belong to a different tenant.

3. **Account picker / interactive safety for tenant switching**
   - For explicit tenant switching, `UseDefaultBrokerAccount` is suppressed and tenant-specific interactive auth is used, reducing sticky-account behavior.

4. **Production-mode guardrail preserved**
   - Default authentication flow remains intact for non-tenant-specific paths.

## Enterprise Security Rationale

- Reduces accidental token reuse across tenants.
- Encourages explicit tenant-bound login flow for MFA/2FA completion.
- Maintains secure non-interactive behavior for production workloads.

## Notes

- This change is an incremental hardening attempt for tenant-switch reliability and MFA redirect handling.
- Final behavior can vary by host OS account broker, Entra tenant policy, and credential chain settings.
