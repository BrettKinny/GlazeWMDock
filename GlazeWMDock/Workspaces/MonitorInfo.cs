using System.Collections.Generic;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// A snapshot of one GlazeWM monitor and the workspaces bound to it, parsed
/// from the IPC "query monitors" response. Monitors are the grouping the Dock
/// strip needs: each physical display owns its own set of workspaces, exactly
/// one of which is <see cref="WorkspaceInfo.IsDisplayed"/> at a time.
///
/// <see cref="X"/>/<see cref="Y"/> are the monitor's top-left virtual-desktop
/// coordinates, used to order monitors left-to-right so the strip reads the way
/// the displays are physically arranged. <see cref="HasFocus"/> marks the single
/// monitor the user is currently on (its focused workspace is the one shown as a
/// glyph; every other monitor's group is bracketed).
///
/// <see cref="DeviceName"/> (e.g. <c>\\.\DISPLAY1</c>) and
/// <see cref="DevicePath"/> identify the display; note identical monitor models
/// share a <c>hardwareId</c>, so those two (or position) are what actually tell
/// two same-model panels apart.
/// </summary>
internal readonly record struct MonitorInfo(
    string DeviceName,
    string DevicePath,
    int X,
    int Y,
    bool HasFocus,
    IReadOnlyList<WorkspaceInfo> Workspaces);
