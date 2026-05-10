# SupplyCoreERP Project Rules

## Core Context
- Framework: ABP Framework (.NET 10.0)
- Architecture: Clean Architecture and Domain-Driven Design (DDD)
- Database: PostgreSQL (Neon Cloud)

## Rule & Skill Loading
- Always read and follow the rules at: `./ai-rules/`
- Use specific skills at: `./.agent/skills/`

## Coding Standards
- Strictly adhere to the ABP application layer structure.
- Strictly adhere to all five SOLID principles
- Follow the specialized testing standards in the agent-tester documentation at: ./.gemini/agents/agent-tester.md
- Before editing the file, please explain your plan and wait for my confirmation.

## Language & Communication
- Reply to the user in **Vietnamese** only.
- Use English only for: class names, methods, properties, file paths, code snippets.
- Avoid vague language like "có thể", "có lẽ" , "may be" when discussing code — be definitive or clearly state why you are uncertain.

## Workflow: Before Any Code Change

For **every** code change request, follow this order strictly:

### Step 1 — Analyze
- Clarify the requirement: what, why, and which layers are affected.
- If the requirement is unclear → ask, never infer business logic.

### Step 2 — Present a Plan
Present the following plan format before writing any code:

```
## Plan

**Mục tiêu:** <mô tả ngắn gọn>

**Các file sẽ thay đổi:**
- `path/to/file.cs` — lý do thay đổi

**Các file sẽ tạo mới:**
- `path/to/newfile.cs` — mục đích

**Thứ tự thực hiện:**
1. ...
2. ...

**Rủi ro / Lưu ý:**
- ...
```

### Step 3 — Wait for Confirmation
- After presenting the plan, **stop and wait** — do not write code automatically.
- Proceed only when explicitly confirmed ("ok", "làm đi", or equivalent).
- If the plan needs revision → update and wait for confirmation again.

### Step 4 — Execute
- Follow the confirmed plan exactly — do not expand scope unilaterally.
- If an out-of-scope issue is discovered mid-execution → report it immediately, do not fix it silently.

## When Encountering Problems

- Multiple solutions exist → list all options with trade-offs, do not choose on behalf of the user.
- Bug discovered outside current scope → report separately, do not fix silently.
- Uncertain about an architectural decision → ask before proceeding.