# Release checklist (maintainer)

Tracking the path to (1) making this repo public and (2) publishing to the
Microsoft Store. The initial `1.0.0.0` submission entered certification on
**2026-07-11**. Store mechanics in depth live in
[`publishing-to-store.md`](publishing-to-store.md); this file is the status
tracker. Check items off as you go.

Legend: `[x]` done, `[ ]` to do.

---

## Done (committed and pushed)

- [x] Git history email scrub. All commits (author + committer) rewritten to
  `14231653+BrettKinny@users.noreply.github.com`; `Co-Authored-By` trailers and
  file contents preserved; force-pushed while the repo was still private, so the
  old work email was never publicly exposed. Local `git config user.email` set to
  the same noreply address.
- [x] MIT `LICENSE`.
- [x] README rewritten as public-facing docs (machine-specifics removed,
  requirements stated, privacy section, compile-time-config limitation noted).
- [x] Community-health files: `CONTRIBUTING.md`, `SECURITY.md`,
  `CODE_OF_CONDUCT.md` (contact: `brett@squarewavesystems.com.au`),
  `.github/ISSUE_TEMPLATE/` (bug + feature), `AI_TRANSPARENCY.md`, `CHANGELOG.md`.
- [x] `.gitignore` hardened (`*.snk/*.pem/*.p12/*.key/.env`, `*.msixbundle`,
  `*.msixupload`).
- [x] Package version restructured to `1.0.0.0` and the version-numbering
  guidance in `publishing-to-store.md` corrected.

## Make the repo public

- [x] Final skim of the files above (nothing else machine-specific slipped in).
- [x] Flip visibility:
  ```
  gh repo edit BrettKinny/GlazeWMDock --visibility public --accept-visibility-change-consequences
  ```

## Microsoft Store submission (external / one-time)

See [`publishing-to-store.md`](publishing-to-store.md) for step detail and
[`store-listing.md`](store-listing.md) for ready-to-paste listing copy.

**Identity assigned by Partner Center (product "GlazeWM Workspaces"):**
- Store ID: `9NTLS4PBWN3X` · PFN: `BrettKinny.GlazeWMWorkspaces_qh71y33f4g84y`
- `Package/Identity/Name`: `BrettKinny.GlazeWMWorkspaces`
- `Publisher`: `CN=990828D1-845D-4BDA-A62D-6048473196F7`
- `PublisherDisplayName`: `Brett Kinny`

- [x] Enroll in Partner Center: free Individual developer, personal Microsoft
  account, ID verification. Account type is irreversible.
- [x] Reserve the app name (GlazeWM Workspaces) and copy the three identity
  values from **Product management → Product identity** (recorded above).
- [x] Swap identity into `GlazeWMDock/Package.appxmanifest` — `Identity Name` and
  `Identity Publisher` now hold the Store values; `PublisherDisplayName` already
  matched. Left untouched (as required): COM CLSID
  `7E2D9F44-3B6A-4C1E-9A57-2F8B1D6C4E90` and the `com.microsoft.commandpalette`
  AppExtension block. **Note:** identity lives only in the manifest (single
  source of truth) — we did *not* also add `<AppxPackageIdentityName>` etc. to
  the csproj, to avoid two conflicting declarations. VS *Create App Packages →
  Microsoft Store* associates the reservation and uses the manifest identity.
- [x] Real artwork: placeholder "W" tiles replaced. Source SVGs in
  `GlazeWMDock/Assets/source/` (`icon.svg` = logo, `glyph-e7f4.svg` = traced dock
  glyph); all package PNGs regenerated from `icon.svg` via `rsvg-convert`. The
  required set is exactly the tiles the manifest references (Square44x44,
  Square150x150, Wide310x150, SplashScreen, StoreLogo, LockScreen) plus the
  `scale-200`/`targetsize-24_altform-unplated` variants — no SmallTile/LargeTile
  are used by this project. To re-render, re-run the block in
  `publishing-to-store.md` §Assets.
- [x] Store screenshots captured at 3440×1440: full-size Dock strip, compact
  Dock, and switcher page under `docs/images/`; added to the README.
- [x] Listing copy drafted — description, short description, category, search
  terms, what's-new, privacy statement text: see `store-listing.md`.
- [x] Capability justifications drafted (`runFullTrust`, `internetClient`): see
  `store-listing.md` (fuller wording than the notes in `publishing-to-store.md`).
- [x] Reviewer testing instructions drafted: see `store-listing.md`
  (Supplemental info → Additional Testing Information).
- [x] Built the x64 + ARM64 Store upload at
  `GlazeWMDock/AppPackages/GlazeWMDock_1.0.0.0_x64_ARM64_bundle.msixupload`.
  The Store re-signs it, so no test certificate is included in the upload.
- [x] Ran Windows App Certification Kit 10.0.26100.6901 against the installed
  package. Overall result: **Warning**. Required tests pass; the optional
  blocked-executable scan flags strings in the self-contained .NET runtime, and
  DPI detection warns despite `app.manifest` declaring `PerMonitorV2`.
- [x] Completed the Partner Center listing: English (Australia), three desktop
  screenshots, Developer tools category, free pricing in all markets, and
  publish automatically after certification. The IARC questionnaire produced
  the lowest ratings (Microsoft/IARC 3+, ESRB Everyone). The privacy policy is
  published at `PRIVACY.md`; the personal-information declaration is **No**
  because the extension does not access, collect, store, or transmit identifying
  data. The privacy-policy URL remains supplied because Win32/full-trust products
  must provide one.
- [x] Added the `runFullTrust` justification and reviewer setup instructions,
  including the PowerToys Command Palette and GlazeWM prerequisites.
- [x] Submitted version `1.0.0.0` for certification on 2026-07-11. Partner
  Center accepted it and began pre-processing; it will publish automatically if
  certification passes. Microsoft advises that certification normally takes a
  few hours but can take up to three business days.
- [x] Certification **failed**. Partner Center rejected `1.0.0.0` under policy
  10.1.2 ("The product crashes at launch"), observed on a Dell Inspiron 13-5379
  running OS build 26200.8457, with no error message. The listing was never
  published: `apps.microsoft.com/detail/9NTLS4PBWN3X` returns HTTP 410 and the
  DisplayCatalog API returns 404.
- [x] Root-caused. Not a real crash: the certification tester launches the
  Start-menu tile, and the no-arguments branch of `Program.cs` wrote to a console
  that a `WinExe` does not have, then exited within milliseconds with no window.
  A process that appears and vanishes reads as a crash. Reproduced locally.
- [x] Fixed in `1.0.1.0`. Direct launches now show a `MessageBoxW` explaining
  that this is a Command Palette extension and listing the PowerToys and GlazeWM
  prerequisites, then exit cleanly when dismissed. Verified both paths on a
  clean install: tile launch keeps a titled window alive and exits gracefully,
  and Command Palette still activates the windowless COM server with IPC
  connected.
- [x] Expanded Additional Testing Information in `store-listing.md`. It now leads
  with the fact that this is an extension with no main window, and that a direct
  launch shows an information dialog which **is** the expected result rather than
  a failure — the omission that caused the rejection. Also added verified winget
  IDs (`glzr-io.glazewm`, `Microsoft.PowerToys`), Command Palette's **Reload**
  step for newly installed extensions, and the log location.
- [x] Verified the graceful-degradation claim before asserting it to reviewers.
  GlazeWM runs elevated, so rather than stopping it, a throwaway build pointing at
  a dead loopback port reproduced the "GlazeWM absent" path exactly: the extension
  logs `WebSocketException: Unable to connect to the remote server` and retries
  every few seconds; Command Palette and the COM server both stay alive. The
  throwaway build was deleted — a dead-port package must never ship.
- [x] **Decided against** `AppListEntry="none"` for this resubmission. It would
  remove the Start-menu tile entirely, but Partner Center reportedly rejects such
  a package as a *headless app* without the `HeadlessAppBypass` waiver
  (storeops@microsoft.com, per-product) — and that gate is not documented on
  Microsoft Learn, so it is unverified. Chasing the waiver means an email round
  trip and a second submission cycle for no user-visible benefit, since the
  first-run dialog already resolves the launch failure. Keep the tile. Revisit
  only if certification objects to the dialog itself.
- [x] Rebuilt the x64 + ARM64 Store upload at
  `GlazeWMDock/AppPackages/GlazeWMDock_1.0.1.0_x64_ARM64_bundle.msixupload`
  (27.1 MB; x64 13.8 MB + ARM64 13.3 MB, unsigned so the Store re-signs). Bundle
  manifest verified: `BrettKinny.GlazeWMWorkspaces`,
  `CN=990828D1-845D-4BDA-A62D-6048473196F7`, `1.0.1.0`, revision field `0`.
  Confirmed the fix is in the shipped `GlazeWMDock.dll` (the `MessageBoxW` import
  and dialog text are present; the old console string is gone).
- [x] Re-ran Windows App Certification Kit **10.0.26100.8249** against the
  installed `1.0.1.0`. Overall **Warning**, unchanged from the `1.0.0.0` baseline:
  22 pass, 1 fail, 1 warning.
  - The fail is the **optional** `Blocked executables` scan (`OPTIONAL="TRUE"`),
    entirely from the bundled self-contained .NET runtime — `coreclr.dll`,
    `clrjit.dll`, `System.Private.CoreLib.dll`, `System.Net.Sockets.dll` and
    friends referencing `CreateProcessW` or containing strings like `cmd`,
    `bash`, `MSBuild` — plus the apphost's own `ShellExecuteW`. Nothing from this
    project's own code.
  - The warning is `DPIAwarenessValidation`, and it is a **false positive**: its
    own message is "Failed to process the binary … GlazeWMDock.exe", i.e. WACK
    could not parse the apphost. `PerMonitorV2` and `dpiAware true/PM` are
    confirmed embedded in the shipped exe, and querying the running process
    returns `PER_MONITOR_DPI_AWARE`. This matters more than it used to, because
    the first-run dialog is this app's only real window.
  - The new `user32.dll!MessageBoxW` import appears only in the informational
    `DEPENDENCY_INFORMATION` inventory, not in any test result.
  - Note WACK never caught the launch bug that failed certification, so a Warning
    here is not evidence the resubmission will pass — it only rules out
    regressions in the technical-compliance tests.
- [ ] Resubmit `1.0.1.0` in Partner Center: upload the `.msixupload`, paste the
  updated Additional Testing Information, keep the existing listing copy.
- [ ] Confirm certification passes and the Store listing is publicly reachable.
- [ ] After it's live: uninstall the sideloaded dev package to avoid duplicate
  providers, `Get-AppxPackage -Name GlazeWMDock | Remove-AppxPackage`.

## After publishing

- [ ] Update the "coming soon" cross-links in the `glzr-dots` repo. Its
  `README.md` and `cmdpal/README.md` both say the Store listing is pending; point
  them at the real Store URL.
- [ ] Consider cutting a GitHub Release with a signed `.msix`; `glzr-dots`
  already tells users to "grab a signed `.msix` from its Releases".
- [ ] Move the `CHANGELOG.md` `[Unreleased]` items into a dated `[1.0.0]` entry.

## Key facts / gotchas

- Version rule: the Store reserves the 4th version field (must stay `0`). Bump
  the 3rd field per submission: `1.0.0.0 → 1.0.1.0 → 1.0.2.0 …`.
- Configuration is compile-time only (`WorkspaceNames`, `ShowOnlyActive`, IPC
  port). Documented as a limitation; a CmdPal settings form is a good
  fast-follow.
- Naming / trademark: repo `GlazeWMDock`, display name GlazeWM Workspaces,
  provider id `com.brettkinny.glazewmdock`. It's a third-party companion to
  GlazeWM; say so in the listing to preempt any branding question.
- Distribution alternative: WinGet needs no Partner Center account; see
  `publishing-to-store.md`.
