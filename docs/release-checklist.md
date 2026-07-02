# Release checklist (maintainer)

Tracking the path to (1) making this repo public and (2) publishing to the
Microsoft Store. Store mechanics in depth live in
[`publishing-to-store.md`](publishing-to-store.md); this file is the status
tracker — check items off as you go.

Legend: `[x]` done · `[ ]` to do.

---

## Done (committed & pushed)

- [x] **Git history email scrub.** All commits (author + committer) rewritten to
  `14231653+BrettKinny@users.noreply.github.com`; `Co-Authored-By` trailers and
  file contents preserved; force-pushed while the repo was still private, so the
  old work email was never publicly exposed. Local `git config user.email` set to
  the same noreply address.
- [x] **MIT `LICENSE`.**
- [x] **README** rewritten as public-facing docs (machine-specifics removed,
  requirements stated, privacy section, compile-time-config limitation noted).
- [x] **Community-health files:** `CONTRIBUTING.md`, `SECURITY.md`,
  `CODE_OF_CONDUCT.md` (contact: `brett@squarewavesystems.com.au`),
  `.github/ISSUE_TEMPLATE/` (bug + feature), `AI_TRANSPARENCY.md`, `CHANGELOG.md`.
- [x] **`.gitignore`** hardened (`*.snk/*.pem/*.p12/*.key/.env`, `*.msixbundle`,
  `*.msixupload`).
- [x] **Package version restructured** to `1.0.0.0` and the version-numbering
  guidance in `publishing-to-store.md` corrected.

## Make the repo public

- [ ] Final skim of the files above (nothing else machine-specific slipped in).
- [ ] Flip visibility:
  ```
  gh repo edit BrettKinny/GlazeWMDock --visibility public --accept-visibility-change-consequences
  ```

## Microsoft Store submission — external / one-time

See [`publishing-to-store.md`](publishing-to-store.md) for step detail.

- [ ] **Enroll in Partner Center** — free *Individual* developer, **personal**
  Microsoft account (not a work/Entra address), ID verification (gov ID + selfie).
  Account type is irreversible.
- [ ] **Reserve the app name** (e.g. *GlazeWM Workspaces*) → copy the three
  identity values from **Product management → Product identity**:
  `Package/Identity/Name`, `Publisher` (`CN=…`), `PublisherDisplayName`.
- [ ] **Swap identity into the project** (only after the values exist):
  - `GlazeWMDock/Package.appxmanifest` — `Identity Name`, `Identity Publisher`
    (currently `CN=GlazeWMDock Dev`), `PublisherDisplayName`.
  - `GlazeWMDock/GlazeWMDock.csproj` — add `<AppxPackageIdentityName>`,
    `<AppxPackagePublisher>`, `<AppxPackageVersion>` to a condition-less
    `<PropertyGroup>`.
  - **Leave untouched:** COM CLSID `7E2D9F44-3B6A-4C1E-9A57-2F8B1D6C4E90` and the
    `com.microsoft.commandpalette` AppExtension block.
- [ ] **Real artwork** — replace the placeholder "W" tiles in
  `GlazeWMDock/Assets/` at all required sizes: Square44x44, SmallTile 71×71,
  Square150x150, LargeTile 310×310, Wide310x150, SplashScreen 620×300,
  StoreLogo 50×50. (Placeholder art is a certification blocker.)
- [ ] **Store screenshots** — capture the dock strip + switcher page (none exist
  yet). Add to the README's Screenshots section too.
- [ ] **Build the multi-arch package** — VS → *Publish → Create App Packages →
  Microsoft Store* (produces the `.msixupload`), or the `dotnet build` x64 + ARM64
  + `makeappx bundle` CLI path in `publishing-to-store.md`. The Store re-signs —
  no `signtool`/`.cer` needed.
- [ ] Run the **Windows App Certification Kit (WACK)** on the package.
- [ ] **Complete the listing** — description (lead with "…integrates with the
  Windows Command Palette to…"), screenshots, category, **free** pricing, IARC
  **age-rating** questionnaire.
- [ ] **Capability justifications** — `runFullTrust` (packaged desktop COM server
  hosting a CmdPal extension) and `internetClient` (loopback GlazeWM IPC only, no
  remote traffic). Draft wording is in `publishing-to-store.md`.
- [ ] **Reviewer testing instructions** — reviewers must have PowerToys +
  Command Palette + a **running GlazeWM v3** or they can't see the extension
  (common cert-failure cause). Add them under *Supplemental info → Additional
  Testing Information*.
- [ ] **Submit for certification.**
- [ ] After it's live: uninstall the sideloaded dev package to avoid duplicate
  providers — `Get-AppxPackage -Name GlazeWMDock | Remove-AppxPackage`.

## After publishing

- [ ] Update the "coming soon" cross-links in the **`glzr-dots`** repo — its
  `README.md` and `cmdpal/README.md` both say the Store listing is pending; point
  them at the real Store URL.
- [ ] Consider cutting a **GitHub Release** with a signed `.msix` — `glzr-dots`
  already tells users to "grab a signed `.msix` from its Releases".
- [ ] Move the `CHANGELOG.md` `[Unreleased]` items into a dated `[1.0.0]` entry.

## Key facts / gotchas

- **Version rule:** the Store reserves the **4th** version field (must stay `0`).
  Bump the **3rd** field per submission: `1.0.0.0 → 1.0.1.0 → 1.0.2.0 …`.
- **Configuration is compile-time only** (`WorkspaceNames`, `ShowOnlyActive`, IPC
  port). Documented as a limitation; a CmdPal settings form is a good fast-follow.
- **Naming / trademark:** repo `GlazeWMDock`, display name *GlazeWM Workspaces*,
  provider id `com.brettkinny.glazewmdock`. It's a third-party companion to
  GlazeWM — say so in the listing to preempt any branding question.
- **Distribution alternative:** WinGet needs no Partner Center account — see
  `publishing-to-store.md`.
