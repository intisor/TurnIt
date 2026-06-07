# PRODUCT REQUIREMENTS DOCUMENT
# Refyla
## Consumable Depletion Intelligence + Roster Engine

---

## 1. Executive Summary

Google Calendar tracks **when**. Microsoft To Do tracks **what**. Nothing tracks **how much is left**.

Refyla fills that gap with two novel pillars:

- **Depletion Tracker**: Learns how fast you consume something from your own history, predicts when you'll run out, and sends calibrated early warnings
- **Roster Engine**: Manages rotating duties across households and teams, eliminating manual tracking and fairness disputes

These aren't features you can replicate by combining Google Calendar and Notion. They require domain-specific logic that generic productivity tools deliberately avoid.

Refyla does NOT try to replace your calendar. It exports to it. Depletion alerts and roster duties appear inside Google Calendar, Apple Calendar, or Outlook — where users already live.

---

## 2. Problem Statement

### 2.1 The Consumable Blindspot

Every household deals with consumables — fuel, LPG, medications, subscriptions, generator oil. The management pattern today is entirely **reactive**: you run out, then you act. There is no tool that predicts when something finishes based on actual usage history, and alerts you before it's gone.

Specifically, no existing tool:
- Learns how fast you consume something from your own history
- Predicts the depletion date dynamically and recalibrates on each refill
- Sends a calibrated early warning before you're empty
- Tracks cost-per-refill so you notice price changes over time

### 2.2 The Roster Problem

Households, student associations, and small teams run on rosters — who cooks, who cleans, who leads, who handles duty. Today these rosters live in WhatsApp messages, handwritten sheets, or manually-maintained spreadsheets that nobody actually keeps current.

- Roster expires and nobody regenerates it
- Swaps happen informally and the record is never updated
- Members don't know they're on duty until someone reminds them manually
- No history of who has done what — fairness is assumed, not verified

### 2.3 Why Existing Tools Fail

*(Intentionally left blank in original PRD)*

---

## 3. Goals & Success Metrics

### 3.1 Goals

- Ship a focused, high-quality MVP covering Depletion Tracker and Roster Engine
- Make the app installable and fully usable offline as a PWA on Android and iOS
- Export all data to Google Calendar, Apple Calendar, and Outlook via iCal
- Support personal and shared spaces (household / team)
- Build the data foundation that supports future AI-powered usage prediction

### 3.2 Success Metrics — MVP

*(To be detailed in technical spec)*

---

## 4. Target Users

### Persona A — The Organized Nigerian Household

A family or shared apartment. Multiple people depend on shared consumables (LPG, generator fuel, water) and nobody has a system. One person bears the mental load of remembering when things run out.

- **Primary use**: Depletion Tracker for household consumables
- **Secondary use**: Chore roster for shared responsibility

### Persona B — The Student Association Secretary

An AMSA/fellowship secretary managing weekly duty rosters, devotion schedules, or hall cleaning assignments. Currently using WhatsApp and paper.

- **Primary use**: Roster Engine with member management
- **Secondary use**: Calendar export so members see duties in Google Cal

### Persona C — The Solo Power User

A disciplined individual who tracks personal consumables (medications, fuel, DSTV, gym payments) and wants intelligent reminders rather than manual calendar entries.

- **Primary use**: Personal depletion tracking
- **Secondary use**: Personal recurring event reminders via iCal

---

## 5. Functional Requirements

### 5.1 Authentication & Identity

*(To be detailed in technical spec)*

### 5.2 Spaces

Every entity in Refyla belongs to a **Space**. Spaces are the isolation boundary for data and membership.

- **Personal Space**: Created automatically on sign-up; single member
- **Household/Team Space**: Created by a member; invite-based membership with roles

### 5.3 Depletion Tracker

#### 5.3.1 Consumable Management

- Create consumable with: name, category, unit of measure (L, units, etc.)
- Log refill event: date, quantity, cost (optional)
- Log usage event (Phase 2+): date, amount consumed
- Edit/delete refill events (with audit trail)
- Archive/unarchive consumables

#### 5.3.2 Depletion Estimation Engine

Two modes based on how the user tracks usage:

**Mode 1: Fixed Duration** (MVP) — User logs *when* they refill, not *how much* they use
- Algorithm — Fixed Duration rolling average:
  - **Refill 1**: use user-provided estimate (no history yet)
  - **Refill 2**: compute actual duration between refill 1 and 2; recalibrate
  - **Refill 3+**: weighted rolling average of last 3 durations (recency bias)
  - **Outlier dampening**: if a duration is >2x the average, weight it at 0.25x
  - **Predicted finish** = last refill date + weighted average duration

**Mode 2: Capacity-based** (Phase 2+) — User logs refill quantity and usage
- Requires explicit usage logging (events or meter readings)
- Algorithm — remaining capacity ÷ average consumption rate
- More accurate over time; higher data entry cost

#### 5.3.3 Alerts & Notifications

- Depletion Alert: customizable days before predicted finish (default: 3 days)
- Alert channels: in-app notification center, Web Push, iCal feed (all three from MVP)
- Recurring reminders if alert is snoozed
- Mark alert as "done" — don't snooze again (prevents alert spam)

#### 5.3.4 History & Insights

- Refill history: date, quantity, cost, notes
- Depletion prediction timeline: shows recalibration on each refill event
- Monthly spend summary (Phase 2)
- Usage trend chart (Phase 2)

### 5.4 Roster Engine

#### 5.4.1 Roster Setup

- Create roster with: name, description, member list
- Member fields: name, email, availability windows (opt-in)
- Admin assigns "start member" and start date
- Members can view roster; admins can edit/regenerate

#### 5.4.2 Rotation Types

- **Sequential**: Members rotate in a fixed order. After the last member, cycle restarts. Simplest, most common.
- **Weekly Pattern**: Each member is assigned specific days of the week (e.g. Mon–Wed = Amina, Thu–Sat = Bello). Repeats weekly.
- **Custom**: Admin manually assigns slots in a repeating block — a 4-week pattern, for example. Most flexible.

#### 5.4.3 Schedule Management

- Auto-generate schedule: 90 days forward (configurable)
- Mark slot as "done" (member can self-mark or admin can mark)
- Swap request: member requests swap with another member → appears in admin feed, approved/rejected
- Swap without request (Phase 2): admin swaps members and updates record
- Absence: block member for date range; carry-forward to next available slot
- Regenerate schedule: if members added/removed, regenerate forward from today

#### 5.4.4 Roster Notifications

- Schedule released: notify all members (in-app + Web Push)
- Duty reminder: notify member 1 day before their slot (customizable)
- Swap request: notify admin when member requests swap
- Export to calendar: iCal feed shows roster duties

### 5.5 Calendar Export

Refyla feeds into your existing calendar — not replaces it. Every depletion alert and roster duty becomes a subscribable calendar event.

- **iCal feed**: One feed per user; includes all spaces they belong to
- **Scope**: Per-user feed (contains only their own alerts + duties in spaces they're in); per-space feed (Phase 2)
- **Refresh**: Feeds update within 5 minutes of an event creation/update
- **Revocation**: iCal token can be revoked; feed stops working
- **Calendar apps**: compatible with Google Calendar, Apple Calendar, Outlook, and any iCal-compliant app

### 5.6 Offline Capability

- **App shell**: cached on first load; available offline
- **Read**: View consumables, rosters, history when offline
- **Write**: Create/edit consumables and roster slots offline; queued for sync
- **Conflict resolution**: Last-write-wins for offline edits when reconnected
- **Service worker**: background sync on reconnect; processes queue in FIFO order

---

## 6. Data Model Overview

High-level entities. Full schema (SQL DDL) to be produced in the technical spec document.

| Entity | Purpose |
|--------|---------|
| User | Account; multiple spaces |
| Space | Isolation boundary; consumables, rosters, members |
| Consumable | Item tracked; belongs to space |
| RefillEvent | Log of a refill; date, quantity, cost |
| UseEvent | Log of usage (Phase 2); quantity, date |
| DepletionForecast | Cached prediction; recalculated on RefillEvent |
| Roster | Rotating schedule; belongs to space |
| RosterMember | Person in roster; belongs to space |
| RosterSlot | Single duty assignment; date, member |
| SwapRequest | Member-to-member swap proposal |

---

## 7. Non-Functional Requirements

### 7.1 Performance

- **Blazor WASM cold start**: < 4 seconds on 3G (lazy load per module, AOT compilation)
- **Roster generation** (10 members, 90 days): < 2 seconds client-side
- **API response time** (p95): < 300ms for all standard queries
- **iCal feed generation**: < 500ms per user

### 7.2 Security

- **JWT**: 15-min access token + 7-day refresh token with rotation
- **Space data isolation**: enforced at API layer — not just UI
- **iCal tokens**: scoped (per user or per space) and revocable
- **VAPID keys**: for Web Push — private key never leaves server
- **Auth**: All endpoints require auth except `/api/auth/*`
- **Rate limiting**: on auth endpoints (brute force protection)

### 7.3 PWA & Offline

- **Service worker**: handles app shell caching and background sync queue
- **IndexedDB**: stores consumables, rosters, slots, and pending write queue
- **Sync queue**: processes on reconnect — FIFO order with conflict resolution
- **PWA manifest**: icons at 192px and 512px, theme-color, display: standalone
- **Add to Home Screen**: supported on Android Chrome and iOS Safari

### 7.4 Scalability

- **Multi-tenant**: all DB queries scoped by SpaceId, enforced via EF Core global query filters
- **DepletionForecast**: pre-computed and cached; recalculated only on new RefillEvent or UseEvent
- **RosterSlots**: generated on demand — store 90 days forward; regenerate as needed, not infinite

---

## 8. Tech Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Blazor WebAssembly (WASM) |
| **UI Framework** | Fluent UI Blazor (Microsoft) |
| **Offline/Storage** | Service Worker + IndexedDB |
| **PWA** | PWA Manifest, Web Push API |
| **Backend** | ASP.NET Core 9+ |
| **Database** | SQL Server / PostgreSQL |
| **ORM** | Entity Framework Core 9+ |
| **Auth** | JWT + OAuth 2.0 (Google) |
| **Calendar** | iCalendar (RFC 5545) |
| **Deployment** | Azure App Service / Docker |

---

## 9. Phased Roadmap

### Phase 1 — MVP (The Novel Core)

- **Auth**: email/password + Google OAuth
- **Personal + Household space creation and management**
- **Depletion Tracker**:
  - Create consumables, log refills
  - Fixed-duration estimation
  - Depletion alerts
- **Roster Engine**:
  - Sequential rotation
  - Auto-generate schedule
  - Mark done
  - Swap
- **Calendar Export**: iCal feed compatible with Google, Apple, and Outlook
- **PWA shell**: installable, offline read, basic sync queue
- **Notifications**: in-app notification center + Web Push

### Phase 2 — Depth

- **Capacity-based depletion estimation** with partial use logging
- **Weighted rolling average algorithm** — smarter predictions over time
- **Price trend charts** and monthly spend summary
- **Team/Org space** with full role system
- **Roster enhancements**:
  - Weekly pattern + custom rotation types
  - Absence management — skip member for date range with carry-forward
  - Swap request workflow with approval
- **Per-space iCal feeds**

### Phase 3 — Intelligence

- **AI-powered usage prediction** (seasonal patterns, anomaly detection)
- **Smart nudges**: 'You usually refill gas around this time'
- **WhatsApp notification channel** (Twilio or direct API)
- **MAUI Blazor Hybrid**: native mobile shell, same codebase
- **Bulk import** from CSV for existing rosters and consumable lists

---

## 10. Out of Scope — MVP

- Two-way Google Calendar sync (export only in MVP — sync is Phase 3+)
- Payment tracking or budgeting features (spend data collected; analysis deferred)
- AI prediction engine (data model designed for it; engine deferred to Phase 3)
- Native mobile app (PWA is the MVP delivery mechanism)
- Task management or to-do features (out of Refyla's scope entirely)
- Social or marketplace features
- Multi-language support (English only for MVP)

---

## 11. Open Questions

*(To be addressed in technical specification)*

---

*Refyla PRD v1.0  •  Intitech  •  Confidential  •  2026*
