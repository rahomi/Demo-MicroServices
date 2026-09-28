---
ticket: "TXX"
title: "Short ticket title"
type: "research | task | prototype | grilling"
date_completed: "YYYY-MM-DD"
status: "completed"
blocked_by: []
blocks: []
tags: [ticket-completion]
---

# TXX — Short Ticket Title

## Summary

One paragraph: what was decided/built and why it matters for the project.

## What was done

- Bullet list of concrete actions taken
- Key files created or modified
- Key decisions made during resolution

## Key decisions

- **Decision 1:** What was chosen and why
- **Decision 2:** What was chosen and why

> If a decision supersedes or invalidates a prior one, link it here: [[TXX-previous-ticket]]

## Artifacts created

- `path/to/file.cs` — what it does
- `path/to/config.yml` — what it configures

## Testing & verification

- [ ] How was this verified? (build, manual test, docker compose, etc.)
- [ ] What commands were run?
- [ ] What were the results?

```
# Example test commands and output
dotnet build EcommerceDemo.slnx
docker compose up --build
```

## Dependencies

- **Blocked by:** [[TXX-previous-ticket]] (if any)
- **Unblocks:** [[TXX-next-ticket]] (if any)

## Notes for presentation

- Key talking points for presenting this ticket's evolution
- What surprised us, what was harder than expected
- What to show in a demo (screenshot, log output, trace, etc.)

## Next steps

- What tickets are now unblocked by this completion
- Any fog-of-war items that graduated into new tickets
