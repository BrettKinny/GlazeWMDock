# Contributing to GlazeWM Workspaces

Thanks for your interest in improving this extension. Contributions of all kinds
are welcome: bug reports, feature ideas, docs, and code.

## Reporting bugs and requesting features

[Open an issue](../../issues) and use the appropriate template. For bugs,
include:

- Your Windows version, PowerToys / Command Palette version, and GlazeWM version.
- What you expected vs. what happened.
- Relevant lines from the diagnostic log if you have them (see below).

The extension writes a best-effort log to
`%LOCALAPPDATA%\Packages\<package-family>\LocalState\glazewmdock.log`
(connection status and exception types only, no window titles or personal data).
Attaching the tail of that file often helps.

## Development setup

See "Build from source" in the [README](README.md#build-from-source) for
prerequisites (.NET 10 SDK, Windows 11 SDK, Windows App SDK C# support, MSIX
Packaging Tools) and the deploy loop. In short:

1. Open `GlazeWMDock.sln` in Visual Studio.
2. **Build → Deploy** the Debug / x64 (or ARM64) configuration.
3. Run **Reload** in Command Palette to pick up the new build.

## Pull requests

- Keep changes focused: one logical change per PR.
- Match the existing code style. The project builds with analyzers and StyleCop
  enabled and treats trimming warnings as errors in Release, so ensure
  `dotnet build -c Release` is clean.
- Update the README and docs if you change user-facing behavior or configuration.
- Describe how you tested the change (which GlazeWM version, which Windows/CmdPal
  version).

## AI assistance

This project is built with AI assistance, out in the open; see
[AI_TRANSPARENCY.md](AI_TRANSPARENCY.md). Using an AI assistant on your
contribution is welcome. Keep the `Co-Authored-By:` trailers your tool adds
(don't strip them), note agent help in your PR description, and review the output
yourself before putting your name on it.

## Code of Conduct

This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md). By
participating you agree to abide by it.

## License

By contributing, you agree that your contributions will be licensed under the
project's [MIT License](LICENSE).
