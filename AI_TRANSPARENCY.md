# AI transparency

GlazeWMDock is built with AI assistance, and it says so.

AI coding agents are used throughout this project: writing and refactoring code,
drafting documentation, and reviewing diffs. This page describes how that
assistance is used and how you can tell when it was.

## The rule

Anything an AI agent authors is acknowledged as such. In this repo:

- Commits an AI agent helped write carry a `Co-Authored-By:` trailer naming the
  model, e.g. `Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>`. Run
  `git log` to see which model touched what.
- Pull requests opened with agent help carry a generated-with note in the body
  (e.g. *🤖 Generated with Claude Code*).
- Substantial AI-drafted documents say so rather than passing themselves off as
  hand-written.

## A human is always accountable

The AI proposes; a person decides. Every change that lands was reviewed by a
human who is answerable for it. Acknowledgement is not a way to offload
responsibility onto a tool: the maintainer's name on the merge means a human
read it, understood it, and stands behind it. The co-author trailer just records
which tool helped.

## For contributors

Using an AI assistant on your contribution is welcome and normal here. Keep the
same rule: acknowledge it. Keep the `Co-Authored-By:` trailers your tool adds
(don't strip them), note agent help in your PR description, and review the output
yourself before you put your name on it. See
[`CONTRIBUTING.md`](CONTRIBUTING.md) for the mechanics.

---

*This document was itself drafted with AI assistance and reviewed by a human.*
