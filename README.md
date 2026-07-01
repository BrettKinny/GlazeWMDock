# GlazeWM Workspaces — Command Palette Dock extension

A PowerToys **Command Palette** extension that puts your **GlazeWM workspace
numbers on the Command Palette Dock** — the persistent toolbar that reserves
screen space via the Windows AppBar API. The goal: replace Zebar's workspace
strip so you can (eventually) drop Zebar.

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

> **Why a single text strip, not one button per workspace?** The Dock renders a
> band's text from each item's **`Title`** (bound to a `TextBlock`). Live text
> appears on the bar only when it lives in that `Title` and is mutated in place
> — which is exactly how the built-in `NowDockBand` clock works. The first pass
> put the dynamic info in child items' subtitles, so it only showed inside the
> flyout popup, never on the bar. Putting the strip in one item's `Title` is the
> reliable way to get Zebar-style text on the bar.

State comes from GlazeWM's IPC WebSocket (`ws://127.0.0.1:6123`): the extension
subscribes to workspace/focus events and re-queries `query workspaces` on each
change.

> **Status:** built, signed, and **installed** — now at **v0.0.1.1**
> (2026-07-01), which switches the Dock band to a **live-text workspace strip**
> (see "Dock band" above). After installing, run **Reload** in Command Palette,
> then enable the band under Settings → Bands. Your existing GlazeWM / Zebar
> config has **not** been touched — keep Zebar running until you're happy.

## Turn it on (in Command Palette)

1. Command Palette **Settings → enable Dock**, Position = **Top** (mirrors Zebar).
2. Command Palette **Settings → Bands** → toggle **GlazeWM Workspaces** on. This
   pins the live-text band. (Do **not** use *"Pin to Dock"* on the top-level
   *GlazeWM Workspaces* command — that pins the search **page**, which opens the
   flyout popup instead of showing text on the bar. If you did that on the first
   pass, right-click it on the Dock → **Unpin**.)
3. Switch workspaces (`Alt+1..0`) and watch the strip text update on the bar.
   Click the strip to open the switcher page.

> If the band shows an icon but no text, right-click it in Dock **Edit** mode and
> make sure **Show Titles** (and optionally **Show Subtitles**) is enabled.

## How it was deployed (CLI sideload, already done)

```powershell
# 1. build the signed-able MSIX layout
dotnet build .\GlazeWMDock\GlazeWMDock.csproj -c Debug -p:Platform=x64 `
  -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false
# 2. self-signed code-signing cert (Subject must equal manifest Publisher)
#    -> thumbprint DF4A07ECABB11AA191116384C5FE026D591FD8F9, in Cert:\CurrentUser\My
# 3. sign the .msix
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" sign `
  /fd SHA256 /sha1 DF4A07ECABB11AA191116384C5FE026D591FD8F9 `
  ".\GlazeWMDock\AppPackages\GlazeWMDock_0.0.1.0_x64_Debug_Test\GlazeWMDock_0.0.1.0_x64_Debug.msix"
# 4. trust the cert (elevated, one-time): import GlazeWMDock_Dev.cer into
#    Cert:\LocalMachine\TrustedPeople
# 5. install
Add-AppxPackage -Path ".\GlazeWMDock\AppPackages\GlazeWMDock_0.0.1.0_x64_Debug_Test\GlazeWMDock_0.0.1.0_x64_Debug.msix"
```

### Updating after code changes

The cert is already trusted, so no more UAC. Bump `Version` in
`Package.appxmanifest` (e.g. `0.0.1.1`), then rebuild → sign → install:

```powershell
dotnet build .\GlazeWMDock\GlazeWMDock.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe" sign /fd SHA256 /sha1 DF4A07ECABB11AA191116384C5FE026D591FD8F9 <new .msix path>
Add-AppxPackage -Path <new .msix path>   # or -ForceUpdateFromAnyVersion if you keep the same Version
```

If install fails with `0x80073D02 … resources it modifies are currently in use`,
Command Palette still has the old COM server running. Stop it and force the
update:

```powershell
Get-Process GlazeWMDock -ErrorAction SilentlyContinue | Stop-Process -Force
Add-AppxPackage -Path <new .msix path> -ForceApplicationShutdown
```

Then run **Reload** in Command Palette to re-instantiate the extension.

---

## Prerequisites

- Windows 11 with **PowerToys** installed and **Command Palette** enabled
  (you're on CmdPal 0.11 / PowerToys 0.100 — good; Dock needs ≥ 0.9 / 0.98).
- **Developer Mode** enabled (Settings → System → For developers).
- **.NET 10 SDK** — already installed on this machine (`10.0.300`). The project
  targets `net10.0-windows10.0.26100.0`, matching the SDK version your CmdPal
  0.11 install was built against. `dotnet restore` already succeeds.
- ✅ **Windows 11 SDK (10.0.26100), Windows App SDK C# support, and MSIX
  Packaging Tools** — installed 2026-06-30 into VS 2026 Professional via the VS
  Installer CLI:
  ```powershell
  & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\setup.exe" modify `
    --installPath "C:\Program Files\Microsoft Visual Studio\18\Professional" `
    --add Microsoft.VisualStudio.Component.Windows11SDK.26100 `
    --add Microsoft.VisualStudio.Component.WindowsAppSdkSupport.CSharp `
    --add Microsoft.VisualStudio.ComponentGroup.MSIX.Packaging `
    --passive --norestart
  ```
  With these present, `dotnet build` compiles the project cleanly.

These versions are pinned in `Directory.Packages.props` and the `.csproj` to
match the official extension template for CmdPal 0.11
(`Microsoft.CommandPalette.Extensions` `0.11.260520004`). If your installed
CmdPal is newer/older, bump that version to match.

---

## Build & deploy (recommended: Visual Studio)

1. Open `GlazeWMDock.sln` in Visual Studio.
2. Set the configuration to **Debug** and the platform to **x64** (or **ARM64**).
3. **Build → Deploy GlazeWMDock**.
   - On first deploy VS creates a **self-signed test certificate** matching the
     manifest `Publisher` (`CN=GlazeWMDock Dev`) and asks to trust it. Accept.
   - *Deploy*, not just *Build* — only Deploy registers the MSIX package so
     Command Palette can discover the extension.
4. In Command Palette, run **Reload** (the one subtitled *"Reload Command
   Palette Extension"*).

> Running the **(Unpackaged)** profile from VS will **not** register the
> extension — always Deploy the package.

### Enable the Dock and pin the band

1. Command Palette **Settings → enable Dock** (set Position = Top to mirror your
   current Zebar placement).
2. Command Palette **Settings → Bands** → toggle **GlazeWM Workspaces** on. The
   whole workspace strip is one live-text band. (Enable **Show Titles** on it in
   Dock **Edit** mode if the text doesn't appear.) Don't *"Pin to Dock"* the
   top-level command — that pins the search page, which opens a flyout popup.

---

## How it maps to your setup

- `WorkspaceNames` in `GlazeWMDockCommandsProvider.cs` is `"1".."10"`, mirroring
  the `workspaces:` block in `~/.glzr/glazewm/config.yaml`. Edit it if you
  change your workspace set.
- The Dock reserves screen space via the **AppBar API**, so GlazeWM's tiling
  area shrinks automatically — meaning once you switch over you can drop the
  manual `outer_gap.top: 50px` (currently reserved for Zebar) back to `10px`.
  **Don't change that yet** — only after the extension is deployed and you've
  confirmed the Dock works.

## Two display modes

Both are controlled by the `ShowOnlyActive` constant in
`Workspaces/WorkspaceStripItem.cs`:

- **Show only active workspaces (`ShowOnlyActive = true`, default).** Hides
  empty/undisplayed workspaces (Zebar's behaviour) so the strip only shows
  what's live — the focused workspace (even if empty) plus any holding windows.
- **Show all workspaces (`ShowOnlyActive = false`).** All ten digits are always
  in the strip; inactive ones show a plain digit — an i3-style persistent strip.

The strip re-composes on every workspace change either way.

---

## Publishing to the Microsoft Store

The current flow is **sideloading** (self-signed, `Add-AppxPackage`). To ship it
via the Store instead — which drops self-signing and makes it one-click from
Command Palette's gallery / `winget` — see
[`docs/publishing-to-store.md`](docs/publishing-to-store.md).

## Caveats / known limitations

- **No true "active" highlight.** The Dock renders the strip as a single text
  label, so focus is shown by the **glyph** (filled vs outline vs plain) inside
  the text, not a colored pill like a custom Zebar widget. Visual fidelity is
  lower than Zebar.
- **Show Titles must be on.** The strip text lives in the band item's `Title`,
  so it only appears with **Show Titles** enabled for the band (the default for
  text widgets; toggle it in Dock edit mode if needed). The focused-workspace
  detail is in the `Subtitle` (**Show Subtitles**).
- **One click target.** Because the strip is one label, clicking it opens the
  switcher page rather than focusing a specific workspace by click. Use
  GlazeWM's `Alt+1..0` keybinds to switch — the strip just reflects state.
- **No auto-hide**, no resize/drag — the Dock is always-on and positioned only
  via its setting. (Windows AppBar behavior.)
- This is a **headless C# MSIX extension** (no window) — heavier to build and
  deploy than the Python helpers in `~/.glzr`.

---

## If it won't compile: the `.Extensions.Toolkit` namespace

This project references only `Microsoft.CommandPalette.Extensions` (matching the
official template). The toolkit base classes (`CommandProvider`, `ListItem`,
`WrappedDockItem`, …) ship inside that package. If the build reports those types
as missing, add a matching toolkit package reference:

- In `Directory.Packages.props`:
  `<PackageVersion Include="Microsoft.CommandPalette.Extensions.Toolkit" Version="0.11.260520004" />`
- In `GlazeWMDock.csproj`:
  `<PackageReference Include="Microsoft.CommandPalette.Extensions.Toolkit" />`

## Alternative: regenerate the skeleton, keep the logic

If the packaging files give you trouble, the most reliable path is to let
Command Palette generate a guaranteed-building skeleton, then drop in the logic:

1. Command Palette → **Create a new extension** → name it `GlazeWMDock`.
2. Copy the entire **`Workspaces/`** folder and
   **`GlazeWMDockCommandsProvider.cs`** from here into the generated project.
3. Point the generated `…CommandsProvider` at this one (or replace it), and make
   the generated extension class instantiate `GlazeWMDockCommandsProvider`.

The `Workspaces/*.cs` files plus the provider are the actual work; the rest is
boilerplate the generator produces correctly for your installed SDK.

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
    Properties/ ...              # launchSettings + publish profiles
    Assets/ ...                  # MSIX logos (placeholder "W" tiles)
    Workspaces/
      GlazeWmClient.cs           # IPC WebSocket client (query + subscribe)
      WorkspaceInfo.cs           # parsed workspace snapshot
      WorkspaceGlyphs.cs         # state -> glyph mapping
      FocusWorkspaceCommand.cs   # focus --workspace N (used by the page)
      WorkspaceStripItem.cs      # the live-text Dock band (Title = the strip)
      WorkspacesListPage.cs      # top-level palette page + click-through switcher
```
