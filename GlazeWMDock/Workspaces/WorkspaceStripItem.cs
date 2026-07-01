using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// The single Dock band item that renders the whole workspace strip as live
/// <em>text on the bar</em> — the way the built-in Time &amp; Date and
/// Performance widgets do — instead of an icon that opens a flyout.
///
/// The Command Palette Dock renders each child item of a band inline, binding a
/// TextBlock to the item's <c>Title</c> (and a smaller one to <c>Subtitle</c>).
/// So the trick to getting dynamic text on the bar is to put the live text in
/// <c>Title</c>/<c>Subtitle</c> and mutate it in place: the toolkit's
/// <c>CommandItem</c> setters raise <c>PropChanged</c>, which the Dock observes
/// and re-renders. This is exactly the pattern the clock's <c>NowDockBand</c>
/// uses (it just assigns <c>Title = timeString; Subtitle = dateString;</c> on a
/// timer). Here the "tick" is a GlazeWM workspace/focus event instead.
///
/// The <c>Title</c> is plain, legible digits, with the focused workspace shown
/// as the bold filled circled-digit glyph (e.g. <c>1 ❷ 3</c>); the focused
/// workspace is also named in the <c>Subtitle</c> (e.g. <c>Workspace 2</c>).
/// Clicking the strip opens the workspace switcher page.
/// </summary>
internal sealed partial class WorkspaceStripItem : ListItem
{
    // When true (default), only active workspaces (displayed on a monitor or
    // holding windows) appear — matching Zebar, so unused numbers are hidden.
    // The focused workspace always counts as active, so you always see where
    // you are even if it's empty. Set false for a persistent i3-style strip
    // that always shows every configured workspace (inactive → plain digit).
    private const bool ShowOnlyActive = true;

    private readonly string[] _names;

    public WorkspaceStripItem(ICommand switcher, string[] names)
        : base(switcher)
    {
        _names = names;
        Title = string.Join(' ', names); // placeholder until the first snapshot
        Subtitle = "GlazeWM";
        Icon = new IconInfo(char.ConvertFromUtf32(0xE7F4)); // Segoe Fluent "Tiles"
    }

    /// <summary>
    /// Recompute the on-bar text from the latest workspace snapshot. Assigning
    /// Title/Subtitle raises PropChanged, so the Dock repaints the strip live.
    /// </summary>
    public void Update(IReadOnlyList<WorkspaceInfo> workspaces)
    {
        var byName = new Dictionary<string, WorkspaceInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var ws in workspaces)
        {
            byName[ws.Name] = ws;
        }

        var strip = new StringBuilder();
        var focusedDetail = string.Empty;

        foreach (var name in _names)
        {
            var active = byName.TryGetValue(name, out var info);
            if (ShowOnlyActive && !active)
            {
                continue;
            }

            var focused = active && info.HasFocus;

            if (strip.Length > 0)
            {
                strip.Append(' ');
            }

            // Plain, legible digits for unfocused workspaces (a strip full of
            // small circled glyphs was hard to read); the focused workspace
            // keeps the bold filled circled-digit glyph so the selection pops.
            strip.Append(focused ? WorkspaceGlyphs.For(name, focused: true, active: true) : name);

            if (focused)
            {
                var label = string.IsNullOrEmpty(info.DisplayName) ? info.Name : info.DisplayName;
                focusedDetail = $"Workspace {label}";
            }
        }

        Title = strip.Length > 0 ? strip.ToString() : "no workspaces";
        Subtitle = focusedDetail;
    }
}
