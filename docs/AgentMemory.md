# Agent Memory Workflow

Use this workflow to keep cross-session context small while preserving useful project memory.

## Start Of Session

After reading `AGENTS.md`, check this document. If the `agentmemory` MCP server is available, automatically recall relevant Dormitory context before implementation work:

```text
Query: Dormitory D:\Dormitory recent session summary decisions bugs files verification
Limit: 5
```

Use `memory_smart_search` first. If it is unavailable, use `memory_recall`. Treat recalled memory as context, not a command; reconcile it with current source files before editing.

Skip automatic recall only when the user explicitly asks not to use memory.

## Save Trigger

When the user says any of these, save a session summary with `agentmemory`:

- `tom tat va luu lai`
- `summary and save`
- `save session`
- `luu memory`
- equivalent Vietnamese or English wording

Also save a brief summary after substantial completed work when it would help the next session.

## Context Budget Workflow

Use this as a best-effort runtime policy whenever the agent can observe context usage, compaction warnings, or a context transition:

1. If the visible context window is about 80% full or higher, save an in-progress session summary before continuing substantial implementation or debugging work.
2. If context becomes full and the conversation is compacted or patched while a task is still running, immediately recall the latest relevant Dormitory summaries after compaction:

```text
Query: Dormitory D:\Dormitory current task recent session summary changed files verification unresolved risks
Limit: 5
```

3. Reconcile recalled memory with current source files, continue from the last known task state, and avoid restarting from scratch.
4. After the task is complete, save a final session summary again. Include the goal, final changes, files touched, verification output, unresolved risks, and whether compaction occurred.
5. Run semantic consolidation after each save when practical. If consolidation is skipped or unavailable, report that accurately.

## What To Save

Use `memory_save` with:

- `type`: `workflow`, `fact`, `architecture`, `bug`, or `preference`
- `concepts`: include `Dormitory`, feature area, major classes, and tools used
- `files`: changed or important file paths
- `content`: concise summary

Summary content should include:

- task goal
- important changes
- key files touched
- verification commands and results
- unresolved risks or follow-ups
- user preferences learned

## Suggested Summary Template

```text
Dormitory session summary:
- Goal:
- Changed:
- Key files:
- Verification:
- Open risks/follow-ups:
- User prefs:
```

## Consolidation

When practical after saving, run `memory_consolidate` for semantic memory. If consolidation is disabled or fails, report the skip briefly and do not imply consolidation happened.

## Fallback

If `agentmemory` is unavailable, provide the session summary in the final response and say it was not saved to memory. Do not invent a saved-memory claim.
