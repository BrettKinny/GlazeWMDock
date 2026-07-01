using System;
using System.IO;

namespace GlazeWMDock;

/// <summary>
/// Minimal append-only file log at <c>LocalState\glazewmdock.log</c>.
///
/// This extension deliberately swallows exceptions at every layer — a
/// background WebSocket loop must never crash the Command Palette host. That
/// design once hid a dead-COM-proxy failure (CmdPal.UI restarting on a monitor
/// hot-plug while this singleton extension survived) which silently froze every
/// live update until a manual reload. These few lines make that failure mode
/// visible after the fact, without attaching a debugger to a packaged COM
/// server. It is best-effort: logging never throws into its caller.
/// </summary>
internal static class Log
{
    // Cheap rotation: once the file passes this size, start fresh rather than
    // grow without bound. A single incident is only a handful of lines.
    private const long MaxBytes = 512 * 1024;

    private static readonly object Gate = new();
    private static readonly string? FilePath = Resolve();

    private static string? Resolve()
    {
        try
        {
            // Redirects to %LOCALAPPDATA%\Packages\<family>\LocalState — the same
            // place CmdPal keeps its own settings.json.
            return Path.Combine(
                Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                "glazewmdock.log");
        }
        catch
        {
            // Not running with package identity (e.g. launched directly); disable.
            return null;
        }
    }

    public static void Line(string message)
    {
        var path = FilePath;
        if (path is null)
        {
            return;
        }

        try
        {
            lock (Gate)
            {
                if (File.Exists(path) && new FileInfo(path).Length > MaxBytes)
                {
                    File.Delete(path);
                }

                File.AppendAllText(
                    path,
                    $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff} {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Best-effort only — a logging failure must never surface to the caller.
        }
    }
}
