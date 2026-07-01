namespace GlazeWMDock.Workspaces;

/// <summary>
/// A snapshot of one GlazeWM workspace, parsed from the IPC
/// "query workspaces" response. Only currently-active workspaces (those that
/// are displayed on a monitor or contain windows) appear in that response.
/// </summary>
internal readonly record struct WorkspaceInfo(
    string Name,
    string DisplayName,
    bool HasFocus,
    bool IsDisplayed,
    int WindowCount);
