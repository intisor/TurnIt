# Rule: Maintain Chat Context

## Context File Location
- Workspace Root: `.antigravity-context`

## Instructions
1. **At the Start of Every Chat Thread**:
   - The agent MUST read the [.antigravity-context](file:///c:/Users/DELL/Desktop/Coded/intisor/TurnIt/.antigravity-context) file to load the current project state, architecture patterns, and ongoing checklist.
2. **Before Ending the Turn or Completing a Task**:
   - The agent MUST update [.antigravity-context](file:///c:/Users/DELL/Desktop/Coded/intisor/TurnIt/.antigravity-context) with any new design decisions, completed roadmap items, or architectural changes introduced during the conversation.
   - Keep the file structured cleanly under the existing headers (`Project Overview`, `Core Pillars & Logic`, `UI Architecture`, `Directory & File Reference`, `Development & Testing Instructions`, and `Active Roadmap Checklist`).
