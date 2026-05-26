# Domain Docs

How the engineering skills should consume this repo's domain documentation.

## Before exploring, read these

- **`CONTEXT-MAP.md`** at the repo root — points to one `CONTEXT.md` per demo project. Read each one relevant to the topic.
- Per-project `docs/adr/` directories — read ADRs that touch the area you're working in.

If any of these files don't exist, **proceed silently**. Don't flag their absence.

## File structure

Multi-context layout — each demo subdirectory is its own bounded context:

```
/
├── CONTEXT-MAP.md                     ← root index
├── 1.TwoConsumers/
│   ├── CONTEXT.md
│   └── docs/adr/
├── 2.RequestResponse/
│   ├── CONTEXT.md
│   └── docs/adr/
├── 3.CompetingComsumers/
│   ├── CONTEXT.md
│   └── docs/adr/
├── 4.ActionType/
│   ├── CONTEXT.md                     ← currently exists
│   └── docs/adr/
└── MasstransitRabbitMQ/
    ├── CONTEXT.md
    └── docs/adr/
```

## Use the glossary's vocabulary

When naming a domain concept in an issue title, refactor proposal, or test name, use the term as defined in the relevant `CONTEXT.md`.

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding.
