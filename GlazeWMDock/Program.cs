// Out-of-process COM server entry point. Command Palette launches this
// executable with "-RegisterProcessAsComServer" and talks to it over COM.
// This mirrors the official Command Palette extension template.
using Microsoft.CommandPalette.Extensions;
using Shmuelie.WinRTServer;
using Shmuelie.WinRTServer.CsWinRT;
using System;
using System.Threading;

namespace GlazeWMDock;

public class Program
{
    [MTAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "-RegisterProcessAsComServer")
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
        else
        {
            Console.WriteLine("Not being launched as an extension... exiting.");
        }
    }
}
