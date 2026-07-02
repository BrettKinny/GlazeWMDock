# Publishing GlazeWMDock to the Microsoft Store

Notes for a later session. Today the extension is **sideloaded**: self-signed
with the `GlazeWMDock Dev` cert and installed with `Add-AppxPackage` (see the
main [README](../README.md)). Publishing to the Store replaces that
self-signing flow entirely — the Store signs the package for you — and makes the
extension one-click installable from Command Palette's built-in gallery and via
`winget`.

The single pivotal change is **swapping the package identity** to the values
Partner Center assigns you. Everything else is packaging mechanics.

---

## Two ways to publish (you don't strictly need the Store)

Command Palette extensions have two distribution channels, and only one needs a
Partner Center account:

| Channel | Partner Center? | Auto-updates | How users find it |
| --- | --- | --- | --- |
| **Microsoft Store** | required | yes | CmdPal gallery / `winget` |
| **WinGet** | **not needed** | yes | CmdPal's `Search WinGet` (tag `windows-commandpalette-extension`) |

The **WinGet** path publishes from a GitHub release (GitHub Actions builds an
installer, `wingetcreate` submits the manifest to the community `winget-pkgs`
repo) — no account, no fee, no Store certification queue — and the extension
still appears inside Command Palette and still auto-updates. Either way you can
also list in the curated **Extension Gallery** (a PR to
[`microsoft/CmdPal-Extensions`](https://github.com/microsoft/CmdPal-Extensions)),
which just links to whichever source you chose.

The rest of this doc covers the **Store** path.

---

## Prerequisites (one-time)

- A **Partner Center** account enrolled as a Windows/Store developer.
  Individual registration is now **free** (the old ~$19 fee is waived in
  Microsoft's current onboarding flow), but it now requires **ID verification**
  (government-issued ID + a selfie). Skip if you already have one.
- The project already builds both architectures (`RuntimeIdentifiers` is
  `win-x64;win-arm64` in `GlazeWMDock.csproj`), so a Store package can include
  both — no project change needed for that.

---

## Steps

### 1. Enroll in Partner Center
Go to <https://storedeveloper.microsoft.com> → **Get started for free** → choose
**Individual developer** (free), then sign in and complete ID verification
(government ID + selfie). Individual accounts are fine and **can** ship
full-trust (Win32/desktop) packaged apps like this one.

- **Use a personal Microsoft account** (Outlook/Live). Individual accounts
  require a personal MSA — a work/Entra account (e.g. an `@company.com` address)
  is **Company-only**.
- The account type (**Individual** vs **Company**) is **irreversible**. Pick
  Individual unless the extension is published under a business's name (Company
  is ~$99 and needs business verification via DUNS or documents).

### 2. Reserve the app name
Partner Center → **Apps and games → New product → MSIX/PWA app** → reserve a
name, e.g. **GlazeWM Workspaces**. Reserving hands you the three identity values
you'll need:

| Partner Center value | Goes into `Package.appxmanifest` |
| --- | --- |
| Package/Identity **Name** | `<Identity Name="…">` |
| **Publisher** (`CN=…`) | `<Identity Publisher="…">` |
| **Publisher display name** | `<Properties><PublisherDisplayName>` |

Find them under the reserved product's **Product management → Product identity**.

### 3. Swap the identity into `Package.appxmanifest` — the pivotal change
Replace the current **dev** identity:

```xml
<Identity
  Name="GlazeWMDock"
  Publisher="CN=GlazeWMDock Dev"
  Version="1.0.0.0" />
...
<PublisherDisplayName>Brett Kinny</PublisherDisplayName>
```

with the **Store-assigned** `Name`, `Publisher`, and `PublisherDisplayName`.

Because the Store signs the package, this is where self-signing goes away:

- No more `GlazeWMDock_Dev.cer`, no `signtool`, no trusting a cert — those steps
  in the README's deploy loop are **dev-only** from here on.
- The Store build installs as a **distinct package** from your sideloaded dev
  one (different publisher = different package family). After the Store version
  is live, **uninstall the dev package** so you don't run two copies:
  `Get-AppxPackage -Name GlazeWMDock | Remove-AppxPackage`.

Leave these **unchanged** — they are not tied to the Store identity:

- The COM server CLSID `7E2D9F44-3B6A-4C1E-9A57-2F8B1D6C4E90` (activation is
  keyed by this GUID, independent of package identity).
- The `com.microsoft.commandpalette` app-extension registration (see step 5).

Bump `Version` per submission (the Store requires a higher `Version` than the
previously published one). **Important:** the Store reserves the **4th (revision)
field — it must be `0`** in the package you build (e.g. `1.0.0.0`, `1.0.1.0`).
Do **not** increment the 4th field for Store submissions; bump the **3rd**
field instead. (The sideloaded dev builds happened to climb the 4th field, e.g.
`0.0.1.9` — that layout is invalid for the Store.)

### 4. Build the Store package
In Visual Studio: right-click the project → **Publish → Create App Packages… →
Microsoft Store** using your reservation. This produces a signed `.msixupload`
bundle (x64 + ARM64). **No manual `signtool` step** — the Store handles signing.

> CLI alternative: `Create App Packages` is the reliable path for the
> `.msixupload` format the Store expects. The `dotnet build
> -p:GenerateAppxPackageOnBuild=true` flow used for sideloading produces a
> single-arch `.msix`, not the multi-arch `.msixupload` bundle.

### 5. Verify the CmdPal registration survives
The `<uap3:AppExtension Name="com.microsoft.commandpalette" …>` block in the
manifest is what makes the extension appear in Command Palette's built-in
"install extensions" gallery and load its provider. It's already present (it's
why sideloading works). Just confirm it's **intact and unchanged** in the Store
build — the identity swap in step 3 must not touch it.

### 6. Submit for certification
Upload the `.msixupload`, complete the listing (description, screenshots,
category, **free** pricing), and submit. Once it passes certification and goes
live, it's one-click from Command Palette's gallery and installable via
`winget install <your-package-id>`.

---

## Things to watch during certification

- **`runFullTrust` justification.** The manifest declares
  `<rescap:Capability Name="runFullTrust" />` (required — this is a full-trust
  desktop COM server, not a sandboxed UWP app). `runFullTrust` is a *restricted*
  capability, so the submission will ask you to **justify** it. Expected
  wording: it's a packaged desktop (Win32) app hosting a Command Palette
  extension COM server; full trust is required to run the .NET process and talk
  to GlazeWM over a local WebSocket. Individual accounts are allowed to ship
  full-trust apps, but be ready to explain it.
- **`internetClient`.** Also declared. It's used only for the **loopback**
  GlazeWM IPC socket (`ws://127.0.0.1:6123`). Note that in the listing/notes if
  asked about network use — no remote/telemetry traffic.
- **Assets.** The current tiles are placeholder "W" logos. The Store enforces
  real Store-listing images (icon, screenshots); swap in proper artwork before
  submitting or certification will flag it.
- **Version monotonicity.** Each resubmission needs a strictly higher `Version`
  than the last *published* one.

---

## After it's published

- Uninstall the sideloaded dev package (above) to avoid duplicate providers in
  Command Palette (the same "shows twice" symptom we hit during development,
  except here it'd be two genuinely different packages).
- The dev sideload loop in the README still works for iterating locally; just
  keep the dev identity on a branch or stash so you don't accidentally build a
  dev-signed package with the Store identity (or vice-versa).
