# Privacy policy

**Effective date:** 10 July 2026

GlazeWM Workspaces does not collect, store, sell, or transmit personal data.
It contains no advertising, analytics, or telemetry.

The app communicates only with the GlazeWM instance running on the same device,
using the loopback WebSocket endpoint `ws://127.0.0.1:6123`. This connection is
used solely to read workspace state and send workspace-switching commands. No
information is sent to the developer or to any remote service.

For troubleshooting, the app writes a best-effort diagnostic log to its local
application-data folder. The log contains connection status and exception types,
but not window titles or other personal content. It remains on the device and is
rotated at 512 KB.

Questions about this policy can be sent to
[brett@squarewavesystems.com.au](mailto:brett@squarewavesystems.com.au).
