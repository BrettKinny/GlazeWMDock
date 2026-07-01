// The IExtension implementation. The [Guid] MUST match the CLSID used in
// Package.appxmanifest (com:Class Id and CreateInstance ClassId).
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.CommandPalette.Extensions;

namespace GlazeWMDock;

[Guid("7E2D9F44-3B6A-4C1E-9A57-2F8B1D6C4E90")]
public sealed partial class GlazeWMDockExtension : IExtension, IDisposable
{
    private readonly ManualResetEvent _extensionDisposedEvent;

    private readonly GlazeWMDockCommandsProvider _provider = new();

    public GlazeWMDockExtension(ManualResetEvent extensionDisposedEvent)
    {
        _extensionDisposedEvent = extensionDisposedEvent;
    }

    public object? GetProvider(ProviderType providerType)
    {
        return providerType switch
        {
            ProviderType.Commands => _provider,
            _ => null,
        };
    }

    public void Dispose()
    {
        _provider.Dispose();
        _extensionDisposedEvent.Set();
    }
}
