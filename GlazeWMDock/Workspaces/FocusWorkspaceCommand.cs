using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// When invoked, tells GlazeWM to focus a workspace by sending
/// "command focus --workspace &lt;name&gt;" over IPC. Dismiss() so the next
/// summon opens CmdPal home, not this list.
/// </summary>
internal sealed partial class FocusWorkspaceCommand : InvokableCommand
{
    private readonly GlazeWmClient _client;
    private readonly string _workspaceName;

    public FocusWorkspaceCommand(GlazeWmClient client, string workspaceName)
    {
        _client = client;
        _workspaceName = workspaceName;

        // A non-empty, stable Id is required for items that live on the Dock.
        Id = $"com.brettkinny.glazewmdock.focus.{workspaceName}";
        Name = "Switch to";
    }

    public override ICommandResult Invoke()
    {
        _ = _client.RunCommandAsync($"focus --workspace {_workspaceName}");
        return CommandResult.Dismiss();
    }
}
