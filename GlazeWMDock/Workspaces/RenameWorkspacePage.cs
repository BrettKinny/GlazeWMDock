using System;
using System.Text.Json;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// A one-field form for changing a workspace's display name, reached from the
/// "Rename" context action on a row of <see cref="WorkspacesListPage"/>.
///
/// The rename is sent to GlazeWM as
/// "command update-workspace-config --workspace &lt;name&gt; --display-name &lt;new&gt;"
/// (available since GlazeWM 3.x). That only changes GlazeWM's *runtime* state --
/// it does not rewrite config.yaml, so names revert on wm-reload-config or a WM
/// restart. Put display_name in config.yaml if you want them to stick.
///
/// The rename raises a workspace_updated event, which GlazeWmClient already
/// re-queries on, so the strip and the list refresh themselves; there's nothing
/// to push from here.
/// </summary>
internal sealed partial class RenameWorkspacePage : ContentPage
{
    private readonly RenameWorkspaceForm _form;

    public RenameWorkspacePage(GlazeWmClient client, WorkspaceInfo workspace)
    {
        _form = new RenameWorkspaceForm(client, workspace);

        Id = $"com.brettkinny.glazewmdock.rename.{workspace.Name}";
        Name = "Rename";
        Title = $"Rename workspace {workspace.Name}";
        Icon = new IconInfo(char.ConvertFromUtf32(0xE8AC)); // Segoe Fluent "Rename"
    }

    public override IContent[] GetContent() => [_form];
}

/// <summary>
/// The adaptive-card form itself. Split out from the page so the page stays a
/// thin shell; the interesting part is <see cref="SubmitForm(string)"/>.
/// </summary>
internal sealed partial class RenameWorkspaceForm : FormContent
{
    private readonly GlazeWmClient _client;
    private readonly string _workspaceName;

    public RenameWorkspaceForm(GlazeWmClient client, WorkspaceInfo workspace)
    {
        _client = client;
        _workspaceName = workspace.Name;

        // DisplayName falls back to Name in the parser, so an un-renamed
        // workspace pre-fills with its number rather than an empty box.
        var current = string.IsNullOrEmpty(workspace.DisplayName)
            ? workspace.Name
            : workspace.DisplayName;

        // Built by hand rather than serialized from a type: the project trims on
        // Release, and reflection-based serialization isn't trim-safe. Values are
        // run through JsonEncodedText so a name containing a quote or backslash
        // can't break out of the string.
        TemplateJson = $$"""
        {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.5",
            "body": [
                {
                    "type": "Input.Text",
                    "id": "displayName",
                    "label": "Display name for workspace {{Escape(_workspaceName)}}",
                    "value": "{{Escape(current)}}",
                    "placeholder": "No spaces -- try Work or my-stuff"
                },
                {
                    "type": "TextBlock",
                    "text": "Leave blank to go back to \"{{Escape(_workspaceName)}}\". Resets when GlazeWM restarts or reloads its config.",
                    "wrap": true,
                    "isSubtle": true,
                    "size": "Small"
                }
            ],
            "actions": [
                { "type": "Action.Submit", "title": "Rename" }
            ]
        }
        """;
    }

    public override ICommandResult SubmitForm(string payload)
    {
        string requested;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            requested = doc.RootElement.TryGetProperty("displayName", out var el)
                ? el.GetString()?.Trim() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return CommandResult.ShowToast("Couldn't read the form");
        }

        // Blank means "undo the rename". GlazeWM has no way to clear a display
        // name back to null over IPC, so set it to the workspace's own name --
        // which renders identically to never having been renamed.
        var target = requested.Length == 0 ? _workspaceName : requested;

        // GlazeWM's IPC command parser splits the command line on whitespace and
        // honours no quoting at all -- "--display-name My Work" comes back as
        // "unexpected argument 'Work'". Rather than silently mangling the name,
        // say so. A leading dash would likewise be read as another flag.
        foreach (var c in target)
        {
            if (char.IsWhiteSpace(c))
            {
                return CommandResult.ShowToast("GlazeWM can't take spaces in a workspace name -- use - or _ instead");
            }
        }

        if (target[0] == '-')
        {
            return CommandResult.ShowToast("A workspace name can't start with -");
        }

        _ = _client.RunCommandAsync(
            $"update-workspace-config --workspace {_workspaceName} --display-name {target}");

        // GlazeWM's workspace_updated event refreshes the list behind us.
        return CommandResult.GoBack();
    }

    private static string Escape(string value) => JsonEncodedText.Encode(value).ToString();
}
