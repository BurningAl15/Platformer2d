# Codex model and task-reporting guide

**Project:** Fox's Tale  
**Reviewed:** 2026-11-13

This guide chooses a model for the work at hand and keeps implementation, ownership, and verification status explicit. It does not authorize broader redesigns. All agents must follow `AGENTS.md` and the current FastTask record.

## Default model policy

- **Default for Unity implementation:** GPT-6.1 Sol, Medium reasoning. It is suited to sustained work across code and project files while keeping usage below Astra.
- **Use GPT-6 Luna for:** bounded inventory, mechanical edits, formatting, task metadata, and repeatable checklist/report work when the requirements are already clear. Move to Sol if the work needs architectural judgment or code changes span interacting systems.
- **Use GPT-6.1 Terra for:** screenshot/UI review and scoped computer-use workflows, especially inspecting a visible screen or store dashboard. Use Sol for code or serialized Unity changes that result from that review.
- **Reserve GPT-6 Astra for:** a genuinely difficult, evidence-conflicted root cause or a high-impact cross-system decision that Sol could not resolve with a narrow investigation. Start at a moderate reasoning level; do not default to Max, Ultra, or Fast mode.
- **Escalation rule:** do not switch to a stronger model merely because the first answer missed something. First inspect the active task, project files, handoff, evidence, and acceptance criteria. Escalate only when those are complete and the task still needs stronger reasoning.
- **Availability and usage vary by account, plan, workspace, and task.** Check the Codex model picker and `Settings > Usage` before a large session. Switching models does not restore a shared usage allowance. Official guidance: [Models in Codex](https://learn.chatgpt.com/docs/models) and [Managing usage in Work and Codex](https://help.openai.com/en/articles/20001516-managing-usage-with-gpt-6-astra).

## Recommendations for current FastTask items

Use the FastTask IDs so recommendations remain tied to the actual work item. The task's owning agent may choose a lower-cost model when the task is reduced to a clear, mechanical subtask. Keep one acceptance goal per task and do not expand the implementation scope.

| FastTask ID | Work | Recommended model |
|---|---|---|
| `3c0a75c4-c8f4-4d36-a0c8-5d8a94c13d4a` | Moving-platform rider and physics | GPT-6.1 Sol Medium; High only if the narrow reproduction leaves competing physics hypotheses |
| `9cfdc6cd-84ee-41ed-93fc-65796253e32e` | Enemy stomp versus damage | GPT-6.1 Sol Medium; use owner-supplied level/enemy reproduction before changing collider rules |
| `ab99811b-7d16-467c-9e04-f10cfe9c254f` | English/Spanish localization | GPT-6.1 Sol Medium; Luna may inventory strings before the implementation task |
| `2c67c5e7-4ee7-43e8-a34b-7f9704694ec6` | DOTween pickup and dice feedback | GPT-6.1 Sol Medium for integration; Terra for visual review of screenshots |
| `5504d8d9-6a83-4b3d-b861-9233ed4eea6e` | Haptics and keep-screen-awake settings | GPT-6 Luna for scoped platform API/config audit; GPT-6.1 Sol Medium for integrated implementation |
| `bd4253e4-e95b-4a89-b117-2e2f8ba8f123` | Level, boss, and secret-route design | GPT-6.1 Sol Medium for a structured design brief; Luna for converting approved design into checklists/data |
| `f1bc79bb-9567-4543-bbb3-36a15dfbd11c` | Replace explanatory HUD copy with icons | GPT-6.1 Terra for visual audit; GPT-6.1 Sol Medium for localized UI integration |
| `d3b51c36-9b66-42c7-88b1-45823621a81e` | CMP/LevelPlay consent and ad eligibility | GPT-6.1 Sol Medium with the relevant Unity ads skill; Astra only if a concrete privacy/platform conflict remains unresolved |
| `49b77c90-d7e5-466d-be9e-2b0b0aba03d7` | Play listing and Apple IAP metadata | GPT-6.1 Terra for store-dashboard work; GPT-6 Luna for extracting/checking supplied metadata |
| `76c214fc-b5c1-4a14-a610-9b7516314de6` | Internal builds and release verification | GPT-6.1 Sol Medium for scripts/build configuration; GPT-6.1 Terra for dashboard/status verification |
| `2414d68c-da56-40eb-9745-a03027b7bbd3` | Store, gameplay, and privacy QA matrix | GPT-6.1 Sol Medium to coordinate technical evidence; GPT-6 Luna to execute a fixed checklist and format results |
| `local-8ebeec1f-cc0e-4dcb-9e94-2532ec866d16` | Firebase Analytics and Crashlytics | GPT-6.1 Sol Medium; use Luna for repetitive package/config inventory |
| `a7da262c-5c91-40c5-a1b8-5cc37a4667c6` | Pixel-art UI redesign integration | GPT-6.1 Terra for visual inspection; GPT-6.1 Sol Medium for code/prefab changes |
| `e8d3e6d7-9548-429a-b4ad-43132cf8684f` | Model and completion-reporting workflow | GPT-6 Luna; keep the policy short and update only when model availability or project workflow changes |

## Scope guardrails

For each task:

1. Read its FastTask entry and only the relevant project handoff.
2. State the expected behavior and concrete acceptance criteria before editing.
3. Inspect the existing implementation, scene/prefab ownership, and prior attempts.
4. Change the fewest existing files/components that solve that task. Do not add scripts, abstractions, or runtime UI hierarchy unless the existing architecture cannot support the requirement; document the evidence first.
5. Keep unrelated backlog items in FastTask instead of combining them into the same implementation.
6. Never treat static review or a successful build as runtime/device verification. Run only checks authorized by the task and report their scope accurately.

## Required completion report

Every task report must contain these fields, in this order:

- **Task/status:** FastTask ID and current state (`todo`, `doing`, `testing`, or `done`).
- **Implemented by:** Codex, Antigravity, project owner, Unity Editor, or the relevant store/service, naming each contributor accurately.
- **Implementation:** complete, partial, or not started; name the material changes or say no files changed.
- **Verification:** static review, compile/build, Play Mode, device, or store evidence actually obtained.
- **Still needs:** the next action and its owner (`Codex`, project owner, or external service), including whether testing remains. Say "none" only when the task has its stated completion evidence.
- **Branch/evidence:** branch and commit for code; build/store identifier where relevant. Keep device QA separate from release availability.

Use `testing` when code is ready but compile, Play Mode, device, or store verification is still pending. Use `done` only after the task's written acceptance criteria and required evidence are satisfied. Do not ask the owner to repeat details already recorded in FastTask or the handoff.
