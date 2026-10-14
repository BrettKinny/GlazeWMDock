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
    private readonly GlazeWmClient _client;
    private readonly Func<WorkspaceInfo?> _resolve;
    private readonly bool _dismissOnSave;

    // resolve runs on every open: the page object outlives the snapshot it was
    // built from. dismissOnSave: hotkey entry has no list to go back to.
    public RenameWorkspacePage(GlazeWmClient client, string title, Func<WorkspaceInfo?> resolve, bool dismissOnSave = false)
    {
        _client = client;
        _resolve = resolve;
        _dismissOnSave = dismissOnSave;

        Name = "Rename";
        Title = title;
        Icon = new IconInfo(char.ConvertFromUtf32(0xE8AC)); // Segoe Fluent "Rename"
    }

    public override IContent[] GetContent() =>
        _resolve() is { } ws
            ? [new RenameWorkspaceForm(_client, ws, _dismissOnSave)]
            : [new MarkdownContent("Workspace not found -- is GlazeWM running?")];
}

/// <summary>
/// The adaptive-card form itself. Split out from the page so the page stays a
/// thin shell; the interesting part is <see cref="SubmitForm(string, string)"/>.
/// </summary>
internal sealed partial class RenameWorkspaceForm : FormContent
{
    private readonly GlazeWmClient _client;
    private readonly string _workspaceName;
    private readonly bool _dismissOnSave;

    public RenameWorkspaceForm(GlazeWmClient client, WorkspaceInfo workspace, bool dismissOnSave = false)
    {
        _client = client;
        _workspaceName = workspace.Name;
        _dismissOnSave = dismissOnSave;

        var current = workspace.IsRenamed ? workspace.DisplayName : string.Empty;

        // Reset only makes sense once there's a name to drop.
        var resetAction = workspace.IsRenamed
            ? """,{ "type": "Action.Submit", "title": "Remove name", "data": { "reset": true }, "associatedInputs": "none" }"""
            : string.Empty;

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
                    "type": "TextBlock",
                    "text": "Rename workspace {{Escape(_workspaceName)}}",
                    "size": "Large",
                    "weight": "Bolder"
                },
                {
                    "type": "TextBlock",
                    "text": "Shown next to the number on the Dock, e.g. {{Escape(_workspaceName)}} Teams.",
                    "wrap": true,
                    "isSubtle": true,
                    "spacing": "Small"
                },
                {
                    "type": "Input.Text",
                    "id": "displayName",
                    "label": "Name",
                    "value": "{{Escape(current)}}",
                    "placeholder": "Teams",
                    "maxLength": 32,
                    "regex": "^[^\\s-]\\S*$",
                    "errorMessage": "No spaces, and can't start with -. Try my-work.",
                    "spacing": "Large"
                },
                {
                    "type": "TextBlock",
                    "text": "No spaces (GlazeWM limitation). Resets when GlazeWM restarts or reloads its config.",
                    "wrap": true,
                    "isSubtle": true,
                    "size": "Small",
                    "spacing": "Small"
                }
            ],
            "actions": [
                { "type": "Action.Submit", "title": "Save", "style": "positive" }{{resetAction}}
            ]
        }
        """;
    }

    public override ICommandResult SubmitForm(string inputs) => SubmitForm(inputs, string.Empty);

    public override ICommandResult SubmitForm(string inputs, string data)
    {
        string requested;
        try
        {
            if (IsReset(data))
            {
                return Apply(string.Empty);
            }

            using var doc = JsonDocument.Parse(inputs);
            requested = doc.RootElement.TryGetProperty("displayName", out var el)
                ? el.GetString()?.Trim() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return CommandResult.ShowToast("Couldn't read the form");
        }

        return Apply(requested);
    }

    private CommandResult Apply(string requested)
    {
        // GlazeWM can't clear a display name over IPC; setting it back to the
        // workspace's own name renders identically to never renamed.
        var target = requested.Length == 0 ? _workspaceName : requested;

        // Card regex should already block these, but the host may not enforce
        // it. GlazeWM's IPC parser splits on whitespace with no quoting, and a
        // leading dash reads as another flag.
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
        return _dismissOnSave ? CommandResult.Dismiss() : CommandResult.GoBack();
    }

    private static bool IsReset(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.ValueKind == JsonValueKind.Object
            && doc.RootElement.TryGetProperty("reset", out var el)
            && el.ValueKind == JsonValueKind.True;
    }

    private static string Escape(string value) => JsonEncodedText.Encode(value).ToString();
}
