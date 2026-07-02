# Security Policy

## Reporting a vulnerability

If you discover a security issue, please **do not open a public issue**.
Instead, report it privately via GitHub's
[private vulnerability reporting](../../security/advisories/new) (Security →
Report a vulnerability). Include steps to reproduce and the affected version.

You can expect an initial response within a reasonable timeframe. Once a fix is
available, a new version will be published and the advisory disclosed.

## Scope and threat model

This extension:

- Runs as a **local, full-trust** packaged desktop COM server hosting a Command
  Palette extension.
- Communicates **only** with a local GlazeWM instance over the loopback
  interface (`ws://127.0.0.1:6123`). It makes no other network connections and
  has no telemetry.
- Collects and transmits **no** user data. It writes a local diagnostic log
  (connection status and exception types only) that never leaves the device.

Security-relevant reports are most useful around: the IPC message parsing, the
COM activation surface, and the packaged app's declared capabilities
(`runFullTrust`, `internetClient`).
