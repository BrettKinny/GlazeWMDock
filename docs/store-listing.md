# Store listing content — GlazeWM Workspaces

> **Release status:** Version `1.0.0.0` was submitted on **2026-07-11** and
> **failed certification** under policy 10.1.2 ("The product crashes at
> launch"); it was never published. Root cause and fix are in
> [`release-checklist.md`](release-checklist.md). `1.0.1.0` carries the fix and
> is the version to resubmit. Partner Center Store ID: `9NTLS4PBWN3X`.
>
> Before resubmitting, extend **Additional Testing Information** below to cover
> the GlazeWM prerequisite — the reviewer's device has neither PowerToys nor
> GlazeWM installed.

Ready-to-paste copy for the Partner Center listing. Product: **GlazeWM
Workspaces** · Store ID **9NTLS4PBWN3X** · PFN
**BrettKinny.GlazeWMWorkspaces_qh71y33f4g84y**.

Fill the Partner Center **Store listing** and **Properties** pages from the
sections below. Anything marked _(you)_ needs a human on Windows (screenshots,
final review). Everything else is drafted here.

---

## Properties → Category

- **Category:** Developer tools
- **Subcategory:** (leave default / none)

Rationale: it's a companion to a tiling window manager, surfaced through the
Windows Command Palette — a developer/power-user utility. "Utilities & tools" is
an acceptable fallback if review pushes back.

## Properties → Pricing

- **Base price:** Free
- **Markets:** All markets
- **Age rating:** run the IARC questionnaire; this app has no user-generated
  content, no ads, no data collection → it will land at the lowest rating.

---

## Store listing → Name

GlazeWM Workspaces

## Store listing → Short description (≤ 200 chars)

> See and switch your GlazeWM workspaces straight from the Windows Command
> Palette. A live workspace strip on the Command Palette Dock, updated the
> instant you change focus.

## Store listing → Description

> **GlazeWM Workspaces** brings your [GlazeWM](https://github.com/glzr-io/glazewm)
> tiling workspaces into the Windows Command Palette (part of Microsoft
> PowerToys).
>
> It pins a live strip of workspace numbers to the Command Palette Dock — the
> focused workspace is highlighted, and the strip updates the moment you switch,
> mirroring what a bar like Zebar shows. Click the strip to open a switcher page
> and jump between workspaces without leaving the keyboard.
>
> **Features**
> - Live workspace strip on the Command Palette Dock, always in sync with GlazeWM.
> - Focused-workspace highlight so you always know where you are.
> - One-click switcher page listing every workspace.
> - Reads state directly from GlazeWM's local IPC — no polling, no lag.
> - Native, lightweight .NET extension; runs on x64 and ARM64.
>
> **Requirements**
> - [GlazeWM](https://github.com/glzr-io/glazewm) v3+ running.
> - Microsoft PowerToys with the Command Palette enabled.
>
> **How to turn it on**
> 1. In Command Palette **Settings → enable Dock**, Position = **Top**.
> 2. **Settings → Bands → toggle GlazeWM Workspaces on** to pin the live strip.
> 3. Switch workspaces (e.g. `Alt+1..0`) and watch the strip update. Click it to
>    open the switcher.
>
> GlazeWM Workspaces is a third-party companion to GlazeWM and is not affiliated
> with or endorsed by the GlazeWM project. It talks only to your local GlazeWM
> instance — it sends no data anywhere.

## Store listing → What's new in this version

> Initial release. Live GlazeWM workspace strip on the Command Palette Dock, with
> a click-through switcher page. x64 and ARM64.

(For `1.0.1.0`, keep the same copy — nothing user-facing changed beyond a
launched-directly-from-Start dialog, and it is still the first release users see.)

## Store listing → Search terms (up to 7)

`GlazeWM`, `tiling`, `window manager`, `workspaces`, `Command Palette`,
`PowerToys`, `dock`

## Store listing → Copyright / additional info

- **Copyright:** © 2026 Brett Kinny
- **Developed by / Published by:** Brett Kinny
- **Website:** https://github.com/BrettKinny/GlazeWMDock  _(if repo is public)_
- **Privacy policy URL:**
  https://github.com/BrettKinny/GlazeWMDock/blob/main/PRIVACY.md
- **Support contact:** brett@squarewavesystems.com.au

---

## Capability justifications

Partner Center asks about restricted/network capabilities during submission.
Paste these when prompted.

### `runFullTrust` (restricted capability — will be challenged)

> This is a packaged desktop (Win32) application that hosts a Command Palette
> extension as an out-of-process COM server. Full trust is required to run the
> .NET process that implements the extension and to open a local WebSocket
> connection to GlazeWM. It is not a sandboxed UWP app. No functionality depends
> on elevated privileges; full trust is needed only because Command Palette
> extensions of this type run as full-trust packaged desktop apps.

### `internetClient`

> Used solely to open a **loopback** WebSocket to the user's local GlazeWM
> instance (ws://127.0.0.1:6123) to read workspace state. The app makes no
> remote network connections and sends no telemetry or user data off the device.

---

## Supplemental info → Additional Testing Information (for certifiers)

> **Please read first — this product is a Command Palette extension, not a
> standalone app.** It has no main window of its own. Command Palette starts it
> as a background COM server and draws its output inside Command Palette's own
> UI, so there is nothing to see in the app's own process.
>
> **If you launch it directly** (Start menu, app list, or its AUMID) it displays
> an information dialog explaining that it is a Command Palette extension and
> listing the prerequisites, then closes when you dismiss it. That dialog **is**
> the expected result of a direct launch — it is not an error state, and the app
> has not crashed or failed to start. Version 1.0.0.0 of this submission was
> declined for "crashes at launch"; the cause was that a direct launch showed no
> UI at all, which this version fixes.
>
> To see the extension actually working, both prerequisites must be present:
>
> 1. Install Microsoft PowerToys and enable the **Command Palette**:
>    `winget install Microsoft.PowerToys`
> 2. Install and run **GlazeWM v3+**: `winget install glzr-io.glazewm`
>    (source: https://github.com/glzr-io/glazewm). It is a tiling window manager;
>    let it start and leave at least one workspace active. GlazeWM exposes a local
>    IPC WebSocket on `ws://127.0.0.1:6123` — that socket is the extension's only
>    data source.
> 3. Open Command Palette (default hotkey **Win+Alt+Space**). If the extension is
>    not listed yet, run Command Palette's **Reload** command, or restart it —
>    newly installed extensions are picked up on load.
> 4. Command Palette → **Settings → enable Dock** (Position: Top).
> 5. Add the **"GlazeWM Workspaces"** band to the Dock. A strip of workspace
>    numbers appears; the focused workspace is highlighted, and on a
>    multi-monitor machine workspaces are grouped per monitor.
> 6. Switch workspaces (Alt+1..0) and confirm the strip updates live. Selecting
>    the strip opens a switcher page; typing "GlazeWM" in Command Palette reaches
>    the same page.
>
> **On a machine without GlazeWM installed** (likely your test device): the
> extension still loads and Command Palette stays fully usable. It simply has no
> workspaces to display. Internally it fails to open the loopback socket, records
> the failure in its local log, and retries every few seconds — it does not throw,
> block Command Palette, or terminate. This has been verified directly by running
> the extension with nothing listening on the IPC port.
>
> Networking: the only network activity is the loopback IPC socket above. The app
> contacts no remote server and transmits no data off the device. `internetClient`
> is declared solely because a loopback WebSocket requires it.
>
> Diagnostics: a local-only log is written to the package's `LocalState` folder as
> `glazewmdock.log` (connection status and exception type names only, no user
> data). It is the fastest way to confirm the extension started.

---

## Screenshots — done

The Store requires at least one screenshot (1366×768 or larger, PNG). The
following 3440×1440 captures are ready under `docs/images/`:

1. `dock-strip-default.png` — _Caption:_ "Live workspace strip on the Command
   Palette Dock."
2. `switcher-page.png` — _Caption:_ "Click the strip to jump between
   workspaces."
3. `dock-strip-compact.png` — optional alternate showing the compact Dock.

---

## Assets — done

Real artwork now lives in `GlazeWMDock/Assets/` (source SVGs in
`Assets/source/`). The icon is the traced Command Palette dock glyph (a monitor,
Segoe Fluent `U+E7F4`) with GlazeWM's tiled panes on the screen. All package
tiles referenced by the manifest are generated from `Assets/source/icon.svg`.
The Store listing logo (300×300) can be uploaded from
`Square150x150Logo.scale-200.png` (it's already 300×300) or re-rendered from the
SVG.
