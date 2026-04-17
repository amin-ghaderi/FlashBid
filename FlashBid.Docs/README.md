# FlashBid documentation

This folder contains the technical documentation for the FlashBid system: behavior, architecture, testing, and living notes (current state, issues, next steps).

Start with [01-overview](01-overview.md), then [02-architecture](02-architecture.md).

## Structure

| Doc | Topic |
|-----|--------|
| [01-overview](01-overview.md) | What the system is; goals, features, stack |
| [02-architecture](02-architecture.md) | How it is designed; projects, flows |
| [03-bidding-engine](03-bidding-engine.md) | Core bidding logic and rules |
| [04-realtime](04-realtime.md) | SignalR hub, groups, client events |
| [05-infrastructure](05-infrastructure.md) | Redis, keys, Lua, configuration |
| [06-testing](06-testing.md) | Load testing (Artillery), HTTP test bridge |
| [07-current-state](07-current-state.md) | What is implemented (living doc) |
| [08-known-issues](08-known-issues.md) | Problems and gaps (living doc) |
| [09-next-steps](09-next-steps.md) | What to build or investigate next (living doc) |

The **07–09** files are intended to stay current as the codebase evolves.

## Usage

Use these documents to:

- **Onboard** new engineers (overview → architecture → engine → realtime).
- **Continue development** with AI or human reviewers (explicit paths, file names, and constraints).
- **Debug** behavior (bidding rules, Redis fields, SignalR groups, load-test entry points).

## Layout

```
FlashBid.Docs/
├── README.md
├── 01-overview.md
├── 02-architecture.md
├── 03-bidding-engine.md
├── 04-realtime.md
├── 05-infrastructure.md
├── 06-testing.md
├── 07-current-state.md
├── 08-known-issues.md
└── 09-next-steps.md
```
