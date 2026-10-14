using System;
using System.Collections.Generic;
using Windows.Storage;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// Workspace renames, kept in the package's LocalSettings so they survive
/// GlazeWM restarts and reboots. GlazeWM only holds display names at runtime;
/// <see cref="GlazeWmClient"/> re-applies these whenever they drift.
/// </summary>
internal static class SavedWorkspaceNames
{
    private const string ContainerName = "WorkspaceNames";

    public static void Set(string workspace, string displayName) =>
        Try(c => c.Values[workspace] = displayName);

    public static void Remove(string workspace) =>
        Try(c => c.Values.Remove(workspace));

    public static IReadOnlyDictionary<string, string> All()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        Try(c =>
        {
            foreach (var (key, value) in c.Values)
            {
                if (value is string s && s.Length > 0)
                {
                    result[key] = s;
                }
            }
        });
        return result;
    }

    private static void Try(Action<ApplicationDataContainer> action)
    {
        try
        {
            action(ApplicationData.Current.LocalSettings.CreateContainer(
                ContainerName, ApplicationDataCreateDisposition.Always));
        }
        catch (Exception ex)
        {
            // No package identity (launched directly) -> renames just don't persist.
            Log.Line($"saved names unavailable: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
