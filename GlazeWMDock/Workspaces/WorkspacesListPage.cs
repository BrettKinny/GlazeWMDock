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
    private const int VirtualKeyF2 = 0x71;

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
            var windows = ws.WindowCount == 1 ? "1 window" : $"{ws.WindowCount} windows";

            items.Add(new ListItem(new FocusWorkspaceCommand(_client, ws.Name))
            {
                // Number lives in the icon; title is the name when there is one.
                Title = ws.IsRenamed ? ws.DisplayName : $"Workspace {ws.Name}",
                Subtitle = ws.IsRenamed ? $"Workspace {ws.Name} · {windows}" : windows,
                Icon = new IconInfo(WorkspaceGlyphs.For(ws.Name, ws.HasFocus, active: true)),
                Tags = ws.HasFocus ? [new Tag("Current")] : [],
                MoreCommands =
                [
                    new CommandContextItem(new RenameWorkspacePage(_client, ws))
                    {
                        RequestedShortcut = KeyChordHelpers.FromModifiers(vkey: VirtualKeyF2),
                    },
                ],
            });
        }

        return [.. items];
    }
}
