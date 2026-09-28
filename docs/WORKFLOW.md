# 🛠️ Developer Workflow — EcommerceDemo Microservices

This guide explains how to work through the project's tickets, track progress in Obsidian, and test after each ticket. Follow this workflow for every ticket.

---

## 📂 Files to Read Before Starting Any Task

| File | What it is | When to read it |
|------|-----------|-----------------|
| `PLAN.md` | The 9-section plan — the source of truth for what we're building | **Every session start.** Orient yourself on the destination. |
| `tracker/MAP.md` | Wayfinder map — decision tickets, blocking edges, fog-of-war | **Every session start.** Check the frontier (what's unblocked). |
| `tickets.md` | Tracer-bullet implementation tickets with acceptance criteria | **Before starting a ticket.** Read the ticket's "What to build" and acceptance criteria. |
| `docs/project-evolution.md` | Obsidian index — links to all completion notes | **Before starting a ticket.** Check what's already done and read related completion notes. |
| `tracker/tickets/TXX-*.md` | Decision ticket detail (if the implementation ticket depends on a decision) | **Before starting a ticket blocked by a decision ticket.** Read the decision's resolution. |

---

## 🔄 The Workflow: One Ticket at a Time

### Step 1 — Pick a ticket from the frontier

1. Open `tracker/MAP.md` and find the **Frontier** section (open, unblocked, unclaimed tickets).
2. If no decision tickets are blocking, open `tickets.md` and find the first implementation ticket whose blockers are all done.
3. Read the ticket's **"What to build"** and **acceptance criteria** carefully.

### Step 2 — Read the context

1. Read `PLAN.md` sections relevant to the ticket.
2. Read any completion notes in `docs/notes/` for tickets this one depends on.
3. Read the decision ticket in `tracker/tickets/` if the implementation ticket references one.

### Step 3 — Implement

1. Work through the acceptance criteria as a checklist.
2. Commit frequently with small, focused commits:
   ```bash
   git add <changed-files>
   git commit -m "docs: short description of what changed"
   ```
3. Use conventional commit prefixes:
   - `feat:` — new feature
   - `fix:` — bug fix
   - `refactor:` — code restructuring
   - `docs:` — documentation
   - `test:` — tests
   - `chore:` — tooling, config

### Step 4 — Test and verify

**Every ticket has different testing requirements.** Here's the general testing ladder:

#### Level 1 — Build verification (every ticket)
```bash
dotnet build EcommerceDemo.slnx
```
✅ Must succeed with zero errors and zero warnings.

#### Level 2 — Service runs (service tickets)
```bash
dotnet run --project EcommerceDemo/Products/Products.csproj
```

In another terminal, verify the endpoint with `curl http://localhost:5001/api/products`.
✅ Service starts, endpoint returns expected response.

#### Level 3 — RabbitMQ event verification (messaging tickets)
```bash
docker compose up -d rabbitmq
```

Start the relevant service, trigger an action, then inspect the Queues tab at `http://localhost:15672` (`guest` / `guest`).
✅ Event appears in the queue and is consumed.

#### Level 4 — Full stack verification (Docker Compose tickets)
```bash
docker compose up --build
# Wait for all services to start
# Run through the .http files or curl commands
# Frontend: http://localhost:3000
# BFF Swagger: http://localhost:5000/swagger
```
✅ All services start, all endpoints work, events flow through RabbitMQ.

#### Level 5 — Saga and tracing verification (saga/tracing tickets)
```bash
# Saga: trigger checkout, then stop Orders container, verify basket restored
docker compose stop orders
curl -X POST http://localhost:5000/api/baskets/cust-001/checkout
docker compose logs bff | findstr "saga"

# Tracing: trigger a checkout, open Jaeger
# Open http://localhost:16686 → Search → Find traces from "bff" service
```
✅ Saga compensates on failure. Jaeger shows a full distributed trace.

### Step 5 — Confirm with the developer (GATE — do not skip)

**Before committing or writing any notes, the agent must get explicit confirmation from the developer.**

The agent should:

1. Run the appropriate test level (from Step 4) and show the results.
2. Run `git status` and show the working tree state.
3. Ask the developer:

   > ✅ Testing complete. Here's what I found:
   > - Test results: [summary]
   > - Working tree: [clean / has uncommitted changes]
   >
   > **Does the project work correctly? Are you satisfied that the working tree is clean and ready to commit?**
   >
   > - Type **yes** to commit and create the completion note.
   > - Type **no** to go back and fix issues first.

4. **Only proceed to Step 6 if the developer says "yes".**
   - If the developer says "no" or reports issues, go back to Step 3 (Implement) and fix the problems.
   - Re-run tests and ask for confirmation again.
   - This gate ensures we never commit broken work or write notes for incomplete tickets.

### Step 6 — Commit the implementation work

Once the developer confirms:

```bash
git add <changed-files>
git commit -m "feat: short description of what changed"
```

Verify the working tree is now clean:
```bash
git status --short
# Should show no output (clean working tree)
```

### Step 7 — Create the completion note

1. Copy `docs/templates/ticket-completion.md` to `docs/notes/`:
   ```bash
   # Example for Ticket 1
   cp docs/templates/ticket-completion.md docs/notes/01-scaffold-solution.md
   ```

2. Fill in the template:
   - **Summary:** What was built and why
   - **What was done:** Concrete actions, files created
   - **Key decisions:** What was chosen and why
   - **Artifacts created:** File paths and descriptions
   - **Testing & verification:** Commands run and results (from Step 4)
   - **Notes for presentation:** Talking points for presenting this ticket
   - **Next steps:** What's now unblocked

3. Update `docs/project-evolution.md`:
   - Change the ticket's status from ⬜ Open to ✅ Done
   - Add a link to the completion note in the "Completion Notes" section

### Step 8 — Commit the completion note

```bash
git add docs/notes/ docs/project-evolution.md
git commit -m "docs: add ticket completion note"
```

### Step 9 — Check what's unblocked

1. Open `tracker/MAP.md` — check if completing this ticket unblocks new decision tickets.
2. Open `tickets.md` — check if completing this ticket unblocks new implementation tickets.
3. If fog-of-war items have graduated into specifiable tickets, create them.
4. Pick the next ticket from the frontier and repeat.

---

## 📊 Ticket Dependency Map

```
                    ┌─────────────────────────────────────────────────┐
                    │                                                 │
T01 (SDK/scaffold)──┼──→ T02 (MediatR) ──┐                          │
                    │                      ├──→ T04 (BFF) ──┐        │
                    ├──→ T03 (RabbitMQ) ───┘                  │      │
                    │                                          │      │
                    ├──→ T05 (Orders/Identity) ───────────────┼──→ T08 (README)
                    │                                          │      │
                    ├──→ T07 (Docker) ←── T10 (Tracing) ──────┤      │
                    │                                          │      │
                    └──→ T10 (Tracing) ───────────────────────┤      │
                                                               │      │
                    T03 (RabbitMQ) ──→ T06 (Notifications) ───┘      │
                                                                   │
                    T04 (BFF) ──→ T09 (Saga) ──────────────────────┘
```

**Implementation tickets** (from `tickets.md`) follow a similar but more granular dependency chain. See the "Blocked by" field on each ticket.

---

## 🎯 Presentation Workflow

When presenting the project's evolution:

1. Open `docs/project-evolution.md` in Obsidian.
2. Walk through the **Completion Notes** section in order — each note tells the story of one ticket.
3. Each note has a **"Notes for presentation"** section with talking points and demo suggestions.
4. Use the Obsidian graph view to show the dependency graph visually.
5. For live demos, use the `.http` files and the verification commands from each completion note.

---

## 🔑 Key Rules

1. **One ticket per session.** Don't start a new ticket until the current one is complete and committed.
2. **Always create a completion note.** If it's not in `docs/notes/`, it didn't happen.
3. **Always update the evolution index.** `docs/project-evolution.md` is the presentation surface.
4. **Test before committing.** Run the appropriate test level (1–5) before marking a ticket done.
5. **Get developer confirmation before committing.** The agent must ask the developer to confirm the project works and the working tree is clean before committing (Step 5 gate). Never commit without explicit "yes" from the developer.
6. **Commit implementation work first, then the note.** Two separate commits: `feat(ticket-N):` for the code, `docs(ticket-N):` for the completion note.
7. **Commit with conventional commits.** `feat(ticket-N):` prefix makes the git log readable.
8. **Read before you write.** Always read the plan, map, and related completion notes before starting.
