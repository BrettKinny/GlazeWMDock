# Store listing content — GlazeWM Workspaces

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

## Store listing → Search terms (up to 7)

`GlazeWM`, `tiling`, `window manager`, `workspaces`, `Command Palette`,
`PowerToys`, `dock`

## Store listing → Copyright / additional info

- **Copyright:** © 2026 Brett Kinny
- **Developed by / Published by:** Brett Kinny
- **Website:** https://github.com/BrettKinny/GlazeWMDock  _(if repo is public)_
- **Privacy policy URL:** required by the Store. Point at a short privacy
  statement — the README's privacy section or a `PRIVACY.md`. Content: "This app
  collects and transmits no data. It communicates only with a local GlazeWM
  instance over a loopback socket (ws://127.0.0.1:6123)."
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

> This extension only appears when its prerequisites are running. To test:
> 1. Install Microsoft PowerToys and enable the **Command Palette**.
> 2. Install and run **GlazeWM v3+** (https://github.com/glzr-io/glazewm) with at
>    least one workspace active.
> 3. Open Command Palette → **Settings → enable Dock** (Position: Top).
> 4. **Settings → Bands → enable "GlazeWM Workspaces"**. A strip of workspace
>    numbers appears on the Dock; the focused one is highlighted.
> 5. Switch workspaces (Alt+1..0) and confirm the strip updates. Click the strip
>    to open the switcher page.
>
> Without GlazeWM running the extension loads but shows no workspaces (it has
> nothing to display) — this is expected, not a failure. The app never connects
> to any remote server; the only network use is the local loopback IPC socket.

---

## Screenshots _(you — needs Windows)_

The Store requires at least one screenshot (1366×768 or larger, PNG). Capture on
Windows with the extension running:

1. **The Dock strip in context** — the Command Palette Dock at the top of the
   screen showing the workspace strip (`1 ❷ 3`), with a couple of tiled windows
   behind it so the GlazeWM context is obvious. _Caption:_ "Live workspace strip
   on the Command Palette Dock."
2. **The switcher page** — Command Palette open on the GlazeWM Workspaces
   switcher list. _Caption:_ "Click the strip to jump between workspaces."
3. _(optional)_ **Settings → Bands** with the toggle on. _Caption:_ "Enable it
   from Command Palette settings."

Also drop shots 1–2 into the README's Screenshots section while you're at it.

---

## Assets — done

Real artwork now lives in `GlazeWMDock/Assets/` (source SVGs in
`Assets/source/`). The icon is the traced Command Palette dock glyph (a monitor,
Segoe Fluent `U+E7F4`) with GlazeWM's tiled panes on the screen. All package
tiles referenced by the manifest are generated from `Assets/source/icon.svg`.
The Store listing logo (300×300) can be uploaded from
`Square150x150Logo.scale-200.png` (it's already 300×300) or re-rendered from the
SVG.
