# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

Versioning note: the Microsoft Store reserves the 4th version field (it must stay
`0`), so releases bump the 3rd field (`1.0.0`, `1.0.1`, `1.0.2`, and so on),
which maps to package versions `1.0.0.0`, `1.0.1.0`, `1.0.2.0`.

## [Unreleased]

- Preparing the first public / Microsoft Store release: MIT license, public docs,
  community-health files, and package version restructured to `1.0.0.0`.

## [1.0.0] - first public release (pending)

### Added
- Dock band: the GlazeWM workspace strip rendered as live text on the Command
  Palette Dock (filled glyph = focused, outline = active, plain = empty), with
  the focused workspace detailed in the subtitle.
- Multi-monitor grouping: the strip now groups workspaces by monitor (ordered
  left-to-right by physical position). Each monitor's currently displayed
  workspace is the bold filled circled digit; every other workspace is a plain
  digit. Focus is shown by brackets: every monitor except the one you're on is
  bracketed, so the un-bracketed group is where you are (e.g. `❸ 5 [1 2 4 ❻]`).
  The Dock API shares one band across all monitors, so this replaces the previous
  behavior where every monitor's dock showed an identical, focus-only strip.
- Top-level "GlazeWM Workspaces" palette page to switch workspaces from the
  palette (type / arrow / Enter).
- Live updates via GlazeWM v3's IPC WebSocket (`ws://127.0.0.1:6123`): subscribes
  to workspace/focus events and re-queries on each change, with auto-reconnect.
- Two display modes via the `ShowOnlyActive` constant (active-only, Zebar-like;
  or all workspaces, i3-style).
- Best-effort local diagnostic log under the app's `LocalState` folder
  (connection status and exception types only; rotated at 512 KB).

### Fixed
- Dock strip and switcher freezing after CmdPal.UI restarts on monitor hot-plug
  (a stale COM subscriber was retained; now guarded, with the dock band rebuilt).
