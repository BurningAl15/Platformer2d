# Unity project instructions — Fat Cat Game Studio

These instructions apply to Codex, Antigravity, and other coding agents working in this Unity project.

## English language standard
- Write all source code in English: identifiers, comments, XML documentation, inspector headers/tooltips, technical logs, and developer-facing documentation.
- Keep player-facing text in the game's supported localized languages. Do not translate player-facing copy as part of this standard.
- Preserve existing serialized field names unless a migration is intentional and includes a compatibility path such as `FormerlySerializedAs`.

## Start from FastTask
- Before changing anything, read `FastTask/project.json` and `FastTask/tasks.json`. Identify and reuse the task ID for the requested work; read its `handoffPath` and only the related handoffs needed for that task.
- At the start of work, move the existing task to `doing` and update `updatedAt`. When implementation is ready but awaits build/device/store verification, use `testing`. Mark `done` only when its stated completion evidence exists. Preserve IDs, completed history, and unrelated fields.
- Keep the current task, build milestones, and handoff aligned. Record build, store upload, store processing, tester availability, and device QA as separate facts. A successful build is not proof of store availability or QA.
- If FastTask cloud sync is available, sync after local task/status changes. Otherwise keep `FastTask/` current and report the cloud sync still needed.
- Before implementation, choose the lowest-usage model suited to the task using [`docs/AI/MODEL_AND_TASK_REPORTING.md`](docs/AI/MODEL_AND_TASK_REPORTING.md). Model choice must not broaden scope: use the smallest complete change, preserve existing architecture, and avoid unrelated refactors or new scripts.
- At the end of each task, give a concise report with FastTask ID/status, who performed the work, who owns the next action, whether implementation is complete, verification performed, and any testing still pending. Never imply `done` when build/device/store verification remains.

## Unity and release work
- Confirm the Unity project root and inspect its existing architecture, scenes/prefabs, and project-specific instructions before implementation. Follow existing build scripts and project conventions.
- For persistent UI, prefer serialized scene/prefab assets and existing components. Avoid creating visual hierarchy ad hoc at runtime. Validate layout at the target aspect ratios when changing UI.
- Never print, copy, commit, or document credentials or signing material. Do not inspect, regenerate, replace, or rotate Mix2Word's known-good upload keystore. Do not alter any project's signing keys unless the owner explicitly asks for that specific change.
- Do not claim a store upload, processing, tester availability, or device test without direct evidence. Use the requested test track; do not publish to production unless explicitly requested.
- Before finishing, update the relevant task and handoff, then report what changed, what evidence was obtained, and which task remains next.
