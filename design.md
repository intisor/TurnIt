# TurnFill Design System & UI Instructions for Stitch

## Core Identity & Vibe
- **App Name:** TurnFill
- **Purpose:** Tracks the depletion of shared resources and manages rosters/turns for refilling them.
- **Aesthetic:** Minimalist, calm, and highly functional. It should have a unique, flat look. 
- **Strict Rule:** **NO GRADIENTS**. Use only solid, pure colors.

## Color Psychology & State Management
Stitch, you must reason about and choose the specific hex codes for the color palette, but you must adhere to the following color psychology principles for actions and phases:

1. **Base/Background:** Should be dark and calming (e.g., deep slate or pure dark tones) to reduce cognitive load. 
2. **Healthy/Refilled State:** Use variations of pure colors that evoke calm, stability, and completion (e.g., soft greens, oceanic blues).
3. **Depleting/Warning State:** Use solid colors that gently grab attention without causing panic (e.g., warm ambers, soft yellows).
4. **Empty/Urgent Action State:** Use pure, solid colors that demand immediate action but remain aesthetically pleasing (e.g., flat reds, vibrant oranges).
5. **Roster/Turns:** Differentiate users or turns using distinct, flat pure colors that contrast well with the dark background.

## UI/UX Behavior
- **Fluid SPA:** The app behaves like a Single Page Application. Avoid cascading or stacking modals. Use in-place expansion or smooth page state transitions.
- **Data-Centric:** The interface must get out of the way. Data (how much is left, whose turn it is) takes center stage.

## Instructions for Stitch
Using the guidelines above, generate the UI screens and the comprehensive design system. Ensure you rely heavily on pure colors to dictate the state of the application and communicate the app's psychology.

## Required Screens & Modal Architecture

Since the app is a fluid SPA with minimal modal cascading, the architecture should be flat. Stitch should generate the following core views and interaction states:

### 1. The Dashboard (Home)
- **Purpose:** The central hub showing the status of all tracked resources at a glance.
- **Elements:** 
  - Cards or rows for each resource (e.g., Water, Gas, Coffee).
  - Each card prominently displays: Current level (using color psychology), Estimated Time to Empty, and the avatar/name of the person whose turn is next.
  - Quick action buttons on the cards (e.g., "Refill").

### 2. Resource Detail & Roster View
- **Purpose:** A focused view for a single resource.
- **Elements:**
  - Depletion forecasting data (simple, clean charts or progress indicators).
  - The complete Roster Engine: A vertical timeline or list showing the current queue of turns.
  - Actionable items on the roster (e.g., ability to swap or skip a turn).

### 3. Modal & State Changes (No Cascades)
To maintain cognitive ease, use slide-overs, bottom-sheets, or in-place expansions instead of stacking modals.
- **The "Action" Modal:** When a user clicks "Refill" or "Log Usage" on the dashboard. A single, focused overlay confirming the action.
- **Roster Interaction (Inline/Popover):** Clicking a person's turn on the roster opens a minimal, non-blocking popover to "Skip Turn" or "Swap".
- **Creation Slide-over:** Adding a new resource or a new household member should slide in from the edge of the screen to keep the context of the main dashboard visible behind it.
