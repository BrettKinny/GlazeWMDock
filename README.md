# GlazeWM Workspaces — Command Palette Dock extension

A PowerToys **Command Palette** extension that puts your **GlazeWM workspace
numbers on the Command Palette Dock** — the persistent toolbar that reserves
screen space via the Windows AppBar API. It's designed to replace a Zebar
workspace strip, so you can drop Zebar if you want.

> This is an independent, third-party companion for
> [GlazeWM](https://github.com/glzr-io/glazewm). It is not affiliated with or
> endorsed by the GlazeWM project.

It does two things:

1. **Dock band** — the whole workspace strip rendered as **live text on the
   bar** (the way the built-in clock / CPU widgets show live text), updated as
   you switch. It's a single band whose title text is the strip:
   - **focused** workspace → filled circled digit (e.g. heavy ❶-style glyph)
   - **active** workspace (displayed or has windows) → outline circled digit
   - **inactive / empty** workspace → plain digit
   - the **subtitle** spells out the focused workspace, e.g. `3 · 2 windows`.
   - Clicking the strip opens the switcher page (below). GlazeWM's own
     `Alt+1..0` keybinds still do the actual switching.
2. **Top-level palette page** — "GlazeWM Workspaces" lets you switch workspaces
   from the palette itself (type, arrow, Enter).

State comes from GlazeWM's IPC WebSocket (`ws://127.0.0.1:6123`): the extension
subscribes to workspace/focus events and re-queries `query workspaces` on each
change.

## Screenshots

<!-- Drop images at docs/images/ and reference them here, e.g.:
     ![Workspace strip on the Dock](docs/images/dock-strip.png)
     ![Switcher page](docs/images/switcher-page.png) -->
_Screenshots coming soon._

---

## Requirements

- **Windows 10 version 2004 (build 19041) or later** — Windows 11 recommended.
- **PowerToys** with **Command Palette** enabled. Command Palette **0.9 /
  PowerToys 0.98** or later (the Dock / AppBar feature this extension uses).
- **[GlazeWM](https://github.com/glzr-io/glazewm) v3.x** (the current release)
  running, with its IPC server enabled on the default port `6123`. The extension
  talks to GlazeWM's v3 IPC protocol; earlier v2 builds are not supported.

Both **x64** and **ARM64** are supported.

---

## Install

### From the Microsoft Store

Once published, install **GlazeWM Workspaces** from the Microsoft Store, or find
it inside Command Palette's built-in extension gallery. Command Palette
discovers the extension automatically after install.

### Build from source

Prerequisites for building:

- **.NET 10 SDK** (the project targets `net10.0-windows10.0.26100.0`).
- **Visual Studio 2022/2026** with the **.NET desktop** workload, the
  **Windows 11 SDK (10.0.26100)**, **Windows App SDK C# support**, and the
  **MSIX Packaging Tools** component. (Equivalent standalone SDK/build tools also
  work.)

The package versions are pinned in `Directory.Packages.props` and `.csproj` to
match the official CmdPal extension template
(`Microsoft.CommandPalette.Extensions` `0.11.260520004`). If your installed
Command Palette is a different version, bump that to match.

**Recommended — Visual Studio:**

1. Open `GlazeWMDock.sln`.
2. Set configuration **Debug** and platform **x64** (or **ARM64**).
3. **Build → Deploy GlazeWMDock**.
   - On first deploy VS creates a **self-signed test certificate** matching the
     manifest `Publisher` (`CN=GlazeWMDock Dev`) and asks to trust it. Accept.
   - *Deploy*, not just *Build* — only Deploy registers the MSIX package so
     Command Palette can discover the extension.
4. In Command Palette, run **Reload** (subtitled *"Reload Command Palette
   Extension"*).

> The **(Unpackaged)** run profile does **not** register the extension — always
> Deploy the package.

**CLI alternative (sideload):**

```powershell
# Build the signable MSIX layout
dotnet build .\GlazeWMDock\GlazeWMDock.csproj -c Debug -p:Platform=x64 `
  -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false
```

Then, from a **Developer Command Prompt** (so `signtool` is on `PATH`), sign the
produced `.msix` with a self-signed code-signing certificate whose **subject
matches the manifest `Publisher`** (`CN=GlazeWMDock Dev`), trust that cert once
(import its `.cer` into `Cert:\LocalMachine\TrustedPeople`, elevated), then
install:

```powershell
signtool sign /fd SHA256 /sha1 <your-cert-thumbprint> <path-to>.msix
Add-AppxPackage -Path <path-to>.msix
```

> Enabling **Developer Mode** (Settings → System → For developers) also lets you
> register the loose build output directly, if you prefer that loop.

#### Updating after code changes

Bump `Version` in `Package.appxmanifest`, rebuild, sign, and re-install. If
install fails with `0x80073D02 … resources it modifies are currently in use`,
Command Palette still has the old COM server running — stop it and force the
update, then run **Reload** in Command Palette:

```powershell
Get-Process GlazeWMDock -ErrorAction SilentlyContinue | Stop-Process -Force
Add-AppxPackage -Path <new .msix path> -ForceApplicationShutdown
```

---

## Enable the Dock and pin the band

1. Command Palette **Settings → enable Dock**, Position = **Top** (mirrors a
   top Zebar).
2. Command Palette **Settings → Bands** → toggle **GlazeWM Workspaces** on. This
   pins the live-text band.
   - Do **not** use *"Pin to Dock"* on the top-level *GlazeWM Workspaces*
     command — that pins the search **page**, which opens the flyout popup
     instead of showing text on the bar. If you did that, right-click it on the
     Dock → **Unpin**.
3. Switch workspaces (`Alt+1..0`) and watch the strip text update on the bar.
   Click the strip to open the switcher page.

> If the band shows an icon but no text, right-click it in Dock **Edit** mode and
> make sure **Show Titles** (and optionally **Show Subtitles**) is enabled.

---

## Configuration

> **Current limitation:** there is no settings UI yet. Configuration lives in
> source `const`s, so changing it means editing the code and rebuilding. A
> Command Palette settings form is planned.

- **Workspace set** — `WorkspaceNames` in `GlazeWMDockCommandsProvider.cs` is
  `"1".."10"`. Edit it to match your GlazeWM `workspaces:` block if you use a
  different set.
- **IPC port** — `DefaultPort` in `Workspaces/GlazeWmClient.cs` is `6123`
  (GlazeWM's default). Change it if you've moved GlazeWM's IPC port.
- **Display mode** — the `ShowOnlyActive` constant in
  `Workspaces/WorkspaceStripItem.cs` (default `true`):
  - **`true`** — hides empty/undisplayed workspaces (Zebar-like): the strip
    shows only what's live — the focused workspace plus any holding windows.
  - **`false`** — all ten digits are always in the strip; inactive ones show a
    plain digit (an i3-style persistent strip).

  The strip re-composes on every workspace change either way.

Because the Dock reserves screen space via the AppBar API, GlazeWM's tiling area
shrinks automatically — so once you switch over you can reduce any `outer_gap`
you'd reserved for a top bar in `~/.glzr/glazewm/config.yaml`.

---

## How it works

State comes from GlazeWM's IPC WebSocket: on connect the extension subscribes to
`focus_changed` / `workspace_*` events and re-queries `query workspaces` on each
change, then re-composes the strip. It auto-reconnects if GlazeWM restarts.

**Why a single text strip, not one button per workspace?** The Dock renders a
band's text from each item's **`Title`** (bound to a `TextBlock`). Live text
appears on the bar only when it lives in that `Title` and is mutated in place —
which is how the built-in clock band works. Putting the dynamic info in child
items' subtitles only shows it inside the flyout popup, never on the bar. So the
whole strip lives in one item's `Title`.

## Privacy

The extension communicates only with a **local GlazeWM instance over loopback**
(`ws://127.0.0.1:6123`). It collects **no data**, sends nothing off the device,
and has no telemetry or analytics. It writes a local, best-effort diagnostic log
(connection status and exception types only — no window titles or personal data)
under the app's `LocalState` folder, rotated at 512 KB.

## Caveats / known limitations

- **No true "active" highlight.** The Dock renders the strip as a single text
  label, so focus is shown by the **glyph** (filled vs outline vs plain) inside
  the text, not a colored pill. Visual fidelity is lower than a custom Zebar
  widget.
- **Show Titles must be on.** The strip text lives in the band item's `Title`,
  so it only appears with **Show Titles** enabled for the band. The
  focused-workspace detail is in the `Subtitle` (**Show Subtitles**).
- **One click target.** Because the strip is one label, clicking it opens the
  switcher page rather than focusing a specific workspace by click. Use
  GlazeWM's `Alt+1..0` keybinds to switch — the strip reflects state.
- **No auto-hide**, no resize/drag — the Dock is always-on and positioned only
  via its setting (Windows AppBar behavior).
- **Configuration is compile-time only** (see [Configuration](#configuration)).

---

## Publishing

Distribution has two channels: the **Microsoft Store** (drops self-signing;
lists in Command Palette's gallery) and **WinGet** (no account needed;
discoverable via CmdPal's *Search WinGet*). Both are documented in
[`docs/publishing-to-store.md`](docs/publishing-to-store.md).

## Troubleshooting the build

This project references only `Microsoft.CommandPalette.Extensions` (matching the
official template); the toolkit base classes (`CommandProvider`, `ListItem`,
`WrappedDockItem`, …) ship inside that package. If the build reports those types
as missing, add a matching toolkit reference:

- `Directory.Packages.props`:
  `<PackageVersion Include="Microsoft.CommandPalette.Extensions.Toolkit" Version="0.11.260520004" />`
- `GlazeWMDock.csproj`:
  `<PackageReference Include="Microsoft.CommandPalette.Extensions.Toolkit" />`

If the packaging files give you trouble, the most reliable path is to let
Command Palette generate a guaranteed-building skeleton (**Create a new
extension** → name it `GlazeWMDock`), then copy in the **`Workspaces/`** folder
and **`GlazeWMDockCommandsProvider.cs`** — those plus the provider are the actual
work; the rest is generated boilerplate.

---

## Project layout

```
GlazeWMDock/
  Directory.Build.props          # shared MSBuild props (from template)
  Directory.Packages.props       # central package versions (pinned to 0.11)
  nuget.config
  GlazeWMDock.sln
  GlazeWMDock/
    Program.cs                   # COM-server entry point (from template)
    GlazeWMDockExtension.cs      # IExtension; [Guid] == manifest CLSID
    GlazeWMDockCommandsProvider.cs   # TopLevelCommands + GetDockBands
    Package.appxmanifest         # MSIX + CmdPal extension registration
    app.manifest
    Log.cs                       # best-effort local diagnostic log
    Properties/ ...              # launchSettings + publish profiles
    Assets/ ...                  # MSIX logos
    Workspaces/
      GlazeWmClient.cs           # IPC WebSocket client (query + subscribe)
      WorkspaceInfo.cs           # parsed workspace snapshot
      WorkspaceGlyphs.cs         # state -> glyph mapping
      FocusWorkspaceCommand.cs   # focus --workspace N (used by the page)
      WorkspaceStripItem.cs      # the live-text Dock band (Title = the strip)
      WorkspacesListPage.cs      # top-level palette page + click-through switcher
```

## Contributing

Issues and pull requests are welcome — see
[CONTRIBUTING.md](CONTRIBUTING.md). Please also review the
[Code of Conduct](CODE_OF_CONDUCT.md). This project is built with AI assistance,
out in the open — see [AI_TRANSPARENCY.md](AI_TRANSPARENCY.md).

## License

Released under the [MIT License](LICENSE). GlazeWM is a separate project with its
own license; this extension only communicates with it over a local socket and
does not bundle any GlazeWM code.
