using System.Collections.Generic;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// A searchable list of the currently-active workspaces, surfaced as a
/// top-level Command Palette entry (so you can switch workspaces from the
/// palette too, not only the Dock). Rebuilds its items from the latest
/// snapshot whenever workspace state changes.
/// </summary>
internal sealed partial class WorkspacesListPage : ListPage
{
    private readonly GlazeWmClient _client;
    private IReadOnlyList<WorkspaceInfo> _snapshot = [];

    public WorkspacesListPage(GlazeWmClient client)
    {
        _client = client;
        Id = "com.brettkinny.glazewmdock.page";
        Name = "Open";
        Title = "GlazeWM Workspaces";
        Icon = new IconInfo(char.ConvertFromUtf32(0xE7F4)); // Segoe Fluent "Tiles"
        PlaceholderText = "Switch workspace...";
    }

    /// <summary>Replaces the workspace snapshot and refreshes the list.</summary>
    public void SetSnapshot(IReadOnlyList<WorkspaceInfo> workspaces)
    {
        _snapshot = workspaces;
        RaiseItemsChanged(workspaces.Count);
    }

    public override IListItem[] GetItems()
    {
        var snapshot = _snapshot;
        var items = new List<IListItem>(snapshot.Count);
        foreach (var ws in snapshot)
        {
            items.Add(new ListItem(new FocusWorkspaceCommand(_client, ws.Name))
            {
                Title = string.IsNullOrEmpty(ws.DisplayName) ? ws.Name : ws.DisplayName,
                Subtitle = ws.HasFocus
                    ? "focused"
                    : ws.WindowCount == 1 ? "1 window" : $"{ws.WindowCount} windows",
                Icon = new IconInfo(WorkspaceGlyphs.For(ws.Name, ws.HasFocus, active: true)),
            });
        }

        return [.. items];
    }
}
