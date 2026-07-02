# Release checklist (maintainer)

Tracking the path to (1) making this repo public and (2) publishing to the
Microsoft Store. Store mechanics in depth live in
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

- [ ] Final skim of the files above (nothing else machine-specific slipped in).
- [ ] Flip visibility:
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
- [ ] Store screenshots: capture the dock strip + switcher page (none exist yet).
  Shot list + captions in `store-listing.md`. Add to the README too. **(Windows)**
- [x] Listing copy drafted — description, short description, category, search
  terms, what's-new, privacy statement text: see `store-listing.md`.
- [x] Capability justifications drafted (`runFullTrust`, `internetClient`): see
  `store-listing.md` (fuller wording than the notes in `publishing-to-store.md`).
- [x] Reviewer testing instructions drafted: see `store-listing.md`
  (Supplemental info → Additional Testing Information).
- [ ] Build the multi-arch package: VS → *Publish → Create App Packages →
  Microsoft Store* (produces the `.msixupload`). The Store re-signs, so no
  `signtool`/`.cer` needed. **(Windows-only — the one remaining hard gate.)**
- [ ] Run the Windows App Certification Kit (WACK) on the package. **(Windows)**
- [ ] Complete the listing in Partner Center by pasting from `store-listing.md`;
  run the IARC age-rating questionnaire; set free pricing + all markets. Add a
  privacy policy URL (Store requires one).
- [ ] Submit for certification.
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
