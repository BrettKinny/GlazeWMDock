using System;
using System.Collections.Generic;
using System.Linq;
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
    private volatile IReadOnlyList<WorkspaceInfo> _snapshot = [];
    private volatile IListItem[] _items = [];

    public WorkspacesListPage(GlazeWmClient client)
    {
        _client = client;
        Id = "com.brettkinny.glazewmdock.page";
        Name = "Open";
        Title = "GlazeWM Workspaces";
        Icon = new IconInfo(char.ConvertFromUtf32(0xE7F4)); // Segoe Fluent "Tiles"
        PlaceholderText = "Switch workspace...";
    }

    /// <summary>Replaces the workspace snapshot and refreshes the list if it changed.</summary>
    public void SetSnapshot(IReadOnlyList<WorkspaceInfo> workspaces)
    {
        // Every GlazeWM event (incl. the focus change from opening the palette)
        // lands here. Rebuilding rows on a no-op wipes the selected row's
        // lazily-loaded context menu, so F2 misses -- only raise on real change.
        if (_snapshot.SequenceEqual(workspaces))
        {
            return;
        }

        _snapshot = workspaces;
        _items = [.. workspaces.Select(BuildItem)];
        RaiseItemsChanged(workspaces.Count);
    }

    /// <summary>Live lookup, so a rename form opened from an old row isn't stale.</summary>
    public WorkspaceInfo? Find(string name) =>
        _snapshot.Where(ws => string.Equals(ws.Name, name, StringComparison.Ordinal))
            .Cast<WorkspaceInfo?>()
            .FirstOrDefault();

    // Same objects across calls: host keys row view models by reference.
    public override IListItem[] GetItems() => _items;

    private ListItem BuildItem(WorkspaceInfo ws)
    {
        var windows = ws.WindowCount == 1 ? "1 window" : $"{ws.WindowCount} windows";
        var name = ws.Name;

        return new ListItem(new FocusWorkspaceCommand(_client, name))
        {
            // Number lives in the icon; title is the name when there is one.
            Title = ws.IsRenamed ? ws.DisplayName : $"Workspace {name}",
            Subtitle = ws.IsRenamed ? $"Workspace {name} · {windows}" : windows,
            Icon = new IconInfo(WorkspaceGlyphs.For(name, ws.HasFocus, active: true)),
            Tags = ws.HasFocus ? [new Tag("Current")] : [],
            MoreCommands =
            [
                new CommandContextItem(new RenameWorkspacePage(_client, $"Rename workspace {name}", () => Find(name)))
                {
                    RequestedShortcut = KeyChordHelpers.FromModifiers(vkey: VirtualKeyF2),
                },
            ],
        };
    }
}
