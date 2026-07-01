using System.Globalization;

namespace GlazeWMDock.Workspaces;

/// <summary>
/// Picks a glyph for a workspace button based on its state. Because Dock
/// buttons don't expose a "selected/active" highlight, state is conveyed
/// entirely through the glyph:
///   focused           -> filled circled digit   (U+2776..U+277F)  e.g. heavy (1)
///   active, unfocused -> outline circled digit   (U+2460..U+2469)  e.g. light (1)
///   inactive / empty  -> plain digit             ("1")
///
/// The circled-digit characters are built from their Unicode code points
/// rather than embedded literally, so this source file stays pure ASCII and
/// compiles regardless of the compiler's file-encoding assumption.
/// </summary>
internal static class WorkspaceGlyphs
{
    // U+2460 is "circled digit one" (1), running consecutively up to
    // U+2469 "circled number ten" (10).
    private const int OutlineBase = 0x2460;

    // U+2776 is "dingbat negative circled digit one" (1), running consecutively
    // up to U+277F "dingbat negative circled number ten" (10).
    private const int FilledBase = 0x2776;

    // U+25CF "black circle", used as a focus marker for non-numeric names.
    private const int FocusMarker = 0x25CF;

    public static string For(string name, bool focused, bool active)
    {
        if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)
            && n >= 1 && n <= 10)
        {
            var offset = n - 1;
            if (focused)
            {
                return char.ConvertFromUtf32(FilledBase + offset);
            }

            if (active)
            {
                return char.ConvertFromUtf32(OutlineBase + offset);
            }

            return name; // inactive / empty -> plain digit
        }

        // Non-numeric workspace name: use the name, prefixed with a marker
        // when focused.
        return focused ? char.ConvertFromUtf32(FocusMarker) + " " + name : name;
    }
}
