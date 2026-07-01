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
    private readonly WorkspacesListPage _page;
    private readonly ICommandItem[] _topLevel;

    // The strip/band are rebuilt if the Dock's live-update channel breaks (see
    // OnWorkspacesChanged), so they aren't readonly. _dockBand is read on a host
    // COM thread (GetDockBands) and reassigned on the WebSocket receive thread,
    // hence volatile. _latest caches the newest snapshot so a rebuilt strip can
    // be seeded with current state instead of the constructor placeholder.
    private volatile WorkspaceStripItem _strip;
    private volatile WrappedDockItem _dockBand;
    private IReadOnlyList<WorkspaceInfo> _latest = [];

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
        _latest = workspaces;

        // Update the palette page FIRST and guard it independently. The page
        // re-pulls GetItems() every time it's opened, so it self-heals even
        // when no live notification reaches the host — but only if this call
        // actually runs. Previously it sat *after* _strip.Update(), so a throw
        // from the strip skipped it and froze the switcher too.
        try
        {
            _page.SetSnapshot(workspaces);
        }
        catch
        {
            // Stale/dead host proxy; ignore and keep pumping events.
        }

        // Updating the strip assigns Title/Subtitle, which raises PropChanged
        // and marshals to CmdPal.UI over COM. That UI process restarts on a
        // monitor hot-plug while this singleton extension keeps running, which
        // leaves a dead subscriber on the strip; raising into it throws. An
        // unguarded throw here used to unwind into GlazeWmClient's swallowing
        // catch and permanently freeze every future update until a manual
        // reload. Now we catch it and rebuild the band so the host can re-bind.
        try
        {
            _strip.Update(workspaces);
        }
        catch
        {
            RebuildDockBand();
        }
    }

    /// <summary>
    /// Replaces the dock band with a fresh strip (which has no dead COM
    /// subscribers) seeded with the latest snapshot, then asks the host to
    /// re-pull <see cref="GetDockBands"/> so it re-subscribes to the new item.
    /// This is how the Dock recovers its live text after CmdPal.UI restarts
    /// without a manual reload. If the host is unreachable right now the next
    /// event (or a reload) retries; a fresh strip has no subscribers so it
    /// won't throw again, so this can't loop.
    /// </summary>
    private void RebuildDockBand()
    {
        try
        {
            var strip = new WorkspaceStripItem(_page, WorkspaceNames);
            strip.Update(_latest);
            _strip = strip;
            _dockBand = new WrappedDockItem([strip], DockBandId, "GlazeWM Workspace Strip");
            RaiseItemsChanged(_topLevel.Length);
        }
        catch
        {
            // Host still unreachable; leave state as-is and let the next event retry.
        }
    }

    public override void Dispose()
    {
        _client.WorkspacesChanged -= OnWorkspacesChanged;
        _client.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
