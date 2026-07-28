// Out-of-process COM server entry point. Command Palette launches this
// executable with "-RegisterProcessAsComServer" and talks to it over COM.
// This mirrors the official Command Palette extension template.
using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GlazeWMDock;

public partial class Program
{
    private const string ComServerArgument = "-RegisterProcessAsComServer";

    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == ComServerArgument)
        {
            RunComServer();
        }
        else
        {
            ShowFirstRunDialog();
        }
    }

    private static void RunComServer()
    {
        global::Shmuelie.WinRTServer.ComServer server = new();

        ManualResetEvent extensionDisposedEvent = new(false);

        // Single instance of the extension is created here and returned for
        // every activation request from the host.
        GlazeWMDockExtension extensionInstance = new(extensionDisposedEvent);
        server.RegisterClass<GlazeWMDockExtension, IExtension>(() => extensionInstance);
        server.Start();
        Log.Line("COM server started; waiting for host activation");

        // Block until the extension is disposed by the host.
        extensionDisposedEvent.WaitOne();
        Log.Line("extension disposed by host; stopping COM server");
        server.Stop();
        server.UnsafeDispose();
    }

    /// <summary>
    /// Shown when the executable is launched directly rather than activated by
    /// Command Palette as a COM server — i.e. someone ran it from the Start
    /// menu or by its AUMID.
    ///
    /// This must put a real window on screen. This project is a
    /// <c>WinExe</c> with no console attached, so the previous
    /// <c>Console.WriteLine</c> here wrote nowhere and the process exited
    /// within milliseconds, leaving no UI. Store certification launches the
    /// app's entry point and watches it; a process that appears and vanishes
    /// without a window is indistinguishable from a crash, which is exactly
    /// how the 1.0.0.0 submission was rejected (policy 10.1.2, "the product
    /// crashes at launch"). A dialog explains what this package is and exits
    /// gracefully once dismissed.
    /// </summary>
    private static void ShowFirstRunDialog()
    {
        const string Caption = "GlazeWM Workspaces";
        const string Message =
            "GlazeWM Workspaces is a Command Palette extension, not a standalone app, "
            + "so there is nothing to open here.\n\n"
            + "To use it:\n"
            + "• Install PowerToys and open Command Palette (Win+Alt+Space).\n"
            + "• Run GlazeWM — the workspace strip reads its local IPC socket.\n"
            + "• In Command Palette settings, enable the Dock and add the "
            + "\"GlazeWM Workspaces\" band to show your workspaces.\n\n"
            + "You can also type \"GlazeWM\" in Command Palette to switch workspaces.";

        Log.Line("launched directly (not as a COM server); showing first-run dialog");

        try
        {
            _ = MessageBoxW(IntPtr.Zero, Message, Caption, MB_OK | MB_ICONINFORMATION | MB_SETFOREGROUND);
        }
        catch (Exception ex)
        {
            // Never let the dialog itself become the crash. Exiting quietly is
            // no worse than the behaviour this replaced.
            Log.Line($"first-run dialog failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONINFORMATION = 0x00000040;
    private const uint MB_SETFOREGROUND = 0x00010000;

    // LibraryImport (source-generated) rather than DllImport: the project
    // trims and is AOT-compatible on Release.
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
