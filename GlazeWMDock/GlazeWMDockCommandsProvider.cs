using System;
using System.Collections.Generic;
using System.Linq;
using GlazeWMDock.Workspaces;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GlazeWMDock;

/// <summary>
/// The extension's command provider. It:
///   - exposes a top-level "GlazeWM Workspaces" palette page, and
///   - provides one Dock band that shows the workspace strip as live text.
///
/// A single <see cref="GlazeWmClient"/> feeds live state to both. The Dock
/// band's text item is created once and mutated in place as state changes.
/// </summary>
public partial class GlazeWMDockCommandsProvider : CommandProvider
{
    // The workspaces shown on the Dock. These mirror the `workspaces:` block in
    // your glazewm config.yaml (names "1".."10"). All of them are shown at all
    // times; inactive ones render as a plain (un-circled) digit. To instead
    // show only *active* workspaces, flip ShowOnlyActive in WorkspaceStripItem.
    private static readonly string[] WorkspaceNames =
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10"];

    private const string ProviderId = "com.brettkinny.glazewmdock";
    private const string DockBandId = "com.brettkinny.glazewmdock.workspaces";

    private readonly GlazeWmClient _client = new();
    private readonly WorkspaceStripItem _strip;
    private readonly WrappedDockItem _dockBand;
    private readonly WorkspacesListPage _page;
    private readonly ICommandItem[] _topLevel;

    public GlazeWMDockCommandsProvider()
    {
        Id = ProviderId;
        DisplayName = "GlazeWM Workspaces";
        Icon = new IconInfo(char.ConvertFromUtf32(0xE7F4)); // Segoe Fluent "Tiles"

        _page = new WorkspacesListPage(_client);

        // One band, one child item whose Title/Subtitle *is* the live text on
        // the bar (clicking it opens the switcher page). This is the built-in
        // widget pattern — the Dock renders the child item's Title inline.
        _strip = new WorkspaceStripItem(_page, WorkspaceNames);

        // The 3rd arg is the band's label in the Dock picker / Settings → Bands
        // (NOT the on-bar text — that's the strip item's live Title). Keep it
        // distinct from the top-level palette command below so the two are easy
        // to tell apart when pinning.
        _dockBand = new WrappedDockItem([_strip], DockBandId, "GlazeWM Workspace Strip");

        _topLevel =
        [
            new CommandItem(_page)
            {
                Title = DisplayName,
                Subtitle = "Switch GlazeWM workspace",
            },
        ];

        _client.WorkspacesChanged += OnWorkspacesChanged;
        _client.Start();
    }

    public override ICommandItem[] TopLevelCommands() => _topLevel;

    public override ICommandItem[]? GetDockBands() => [_dockBand];

    private void OnWorkspacesChanged(IReadOnlyList<WorkspaceInfo> workspaces)
    {
        _strip.Update(workspaces);
        _page.SetSnapshot(workspaces);
    }

    public override void Dispose()
    {
        _client.WorkspacesChanged -= OnWorkspacesChanged;
        _client.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
