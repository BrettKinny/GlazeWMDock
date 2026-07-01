using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// Minimal client for GlazeWM's IPC WebSocket server (ws://localhost:6123).
///
/// On connect it subscribes to the workspace/focus events and pulls the
/// initial workspace list. Any subscribed event triggers a re-query of the
/// authoritative "query workspaces" list, which is then surfaced via
/// <see cref="WorkspacesChanged"/>. The connection auto-reconnects.
///
/// JSON is read with <see cref="JsonDocument"/> (no reflection), which keeps
/// the extension trim/AOT-safe — the project trims on Release.
/// </summary>
internal sealed partial class GlazeWmClient : IDisposable
{
    private const int DefaultPort = 6123;
    private const string QueryWorkspaces = "query workspaces";
    private const string SubscribeWorkspaceEvents =
        "sub --events focus_changed workspace_activated workspace_deactivated workspace_updated";

    private readonly Uri _uri;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();
    private ClientWebSocket? _socket;

    /// <summary>Raised whenever a fresh workspace list arrives.</summary>
    public event Action<IReadOnlyList<WorkspaceInfo>>? WorkspacesChanged;

    public GlazeWmClient(int port = DefaultPort)
    {
        _uri = new Uri($"ws://127.0.0.1:{port}");
    }

    /// <summary>Starts the background connect/receive/reconnect loop.</summary>
    public void Start() => _ = Task.Run(() => RunAsync(_cts.Token));

    /// <summary>Sends a WM command, e.g. "focus --workspace 3".</summary>
    public async Task RunCommandAsync(string command)
    {
        try
        {
            await SendAsync($"command {command}").ConfigureAwait(false);
        }
        catch
        {
            // Best effort. If the socket is down, the reconnect loop recovers.
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var socket = new ClientWebSocket();
                _socket = socket;
                await socket.ConnectAsync(_uri, token).ConfigureAwait(false);
                Log.Line($"IPC connected to {_uri}");

                await SendAsync(SubscribeWorkspaceEvents).ConfigureAwait(false);
                await SendAsync(QueryWorkspaces).ConfigureAwait(false);

                await ReceiveLoopAsync(socket, token).ConfigureAwait(false);
                Log.Line("IPC receive loop ended (socket closed); will reconnect");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Connection failed or dropped; fall through to the retry delay.
                Log.Line($"IPC connect/receive failed: {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                _socket = null;
            }

            if (token.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken token)
    {
        var buffer = new byte[16 * 1024];
        var sb = new StringBuilder();

        while (socket.State == WebSocketState.Open && !token.IsCancellationRequested)
        {
            sb.Clear();
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token).ConfigureAwait(false);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            await HandleMessageAsync(sb.ToString()).ConfigureAwait(false);
        }
    }

    private async Task HandleMessageAsync(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("messageType", out var typeEl))
            {
                return;
            }

            var messageType = typeEl.GetString();

            if (messageType == "event_subscription")
            {
                // Something changed; ask for the authoritative workspace list.
                await SendAsync(QueryWorkspaces).ConfigureAwait(false);
                return;
            }

            if (messageType == "client_response"
                && root.TryGetProperty("clientMessage", out var cm)
                && cm.GetString() == QueryWorkspaces
                && root.TryGetProperty("success", out var ok)
                && ok.ValueKind == JsonValueKind.True
                && root.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Object
                && data.TryGetProperty("workspaces", out var wsArray)
                && wsArray.ValueKind == JsonValueKind.Array)
            {
                var list = new List<WorkspaceInfo>(wsArray.GetArrayLength());
                foreach (var ws in wsArray.EnumerateArray())
                {
                    list.Add(ParseWorkspace(ws));
                }

                WorkspacesChanged?.Invoke(list);
            }
        }
        catch
        {
            // Ignore malformed or unexpected frames.
        }
    }

    private static WorkspaceInfo ParseWorkspace(JsonElement ws)
    {
        var name = GetString(ws, "name");
        var displayName = GetString(ws, "displayName");
        return new WorkspaceInfo(
            Name: string.IsNullOrEmpty(name) ? displayName : name,
            DisplayName: string.IsNullOrEmpty(displayName) ? name : displayName,
            HasFocus: GetBool(ws, "hasFocus"),
            IsDisplayed: GetBool(ws, "isDisplayed"),
            WindowCount: CountWindows(ws));
    }

    private static int CountWindows(JsonElement container)
    {
        var count = 0;
        if (container.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in children.EnumerateArray())
            {
                if (string.Equals(GetString(child, "type"), "window", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }

                count += CountWindows(child);
            }
        }

        return count;
    }

    private static string GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? string.Empty
            : string.Empty;

    private static bool GetBool(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;

    private async Task SendAsync(string message)
    {
        var socket = _socket;
        if (socket is null || socket.State != WebSocketState.Open)
        {
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(message);
        await _sendLock.WaitAsync(_cts.Token).ConfigureAwait(false);
        try
        {
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, _cts.Token)
                .ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Dispose()
    {
        try
        {
            _cts.Cancel();
        }
        catch
        {
            // ignore
        }

        try
        {
            _socket?.Abort();
        }
        catch
        {
            // ignore
        }

        _cts.Dispose();
        _sendLock.Dispose();
    }
}
