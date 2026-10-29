using System;

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
    int WindowCount)
{
    // Parser falls back DisplayName -> Name, and clearing a rename sets it back
    // to Name, so "differs from Name" is the only reliable test.
    public bool IsRenamed =>
        !string.IsNullOrEmpty(DisplayName) && !string.Equals(DisplayName, Name, StringComparison.Ordinal);

    /// <summary>"1 · Teams" when renamed, else "1".</summary>
    public string Label => IsRenamed ? $"{Name} · {DisplayName}" : Name;
}
