# TurnIt: Consumable Depletion Intelligence + Roster Engine

A lean Blazor WebAssembly app that solves two problems:

1. **Depletion Tracking**: Predict when household consumables (fuel, LPG, medications) run out
2. **Roster Engine**: Manage fair rotating duty schedules for households and teams

---

## Quick Start

### Prerequisites
- .NET 9 SDK
- A code editor (VS Code, Rider, Visual Studio)

### Run the app

```bash
cd TurnIt
dotnet watch run
```

Navigate to `https://localhost:5001` in your browser.

---

## Project Structure

```
TurnIt/
├── src/
│   ├── Models/
│   │   └── Domain.cs          ← All domain models (Consumable, Roster, etc.)
│   ├── Algorithms/
│   │   └── Core.cs            ← Prediction & schedule generation
│   ├── Pages/
│   │   ├── Index.razor        ← Home page
│   │   ├── Consumables.razor  ← Consumables management
│   │   └── Rosters.razor      ← Roster management
│   ├── Components/
│   │   └── MainLayout.razor   ← Navigation layout
│   ├── wwwroot/
│   │   ├── index.html
│   │   └── app.css
│   ├── App.razor
│   └── Program.cs
├── TurnIt.csproj
├── REFYLA_PRD.md              ← Product requirements
└── QUICK_REFERENCE.md         ← Algorithm overview
```

---

## The Two Core Algorithms

### 1. Depletion Forecast

**Problem**: How do you predict when fuel/LPG/medication runs out?

**Solution**: Learn from refill history using weighted rolling average.

```csharp
var prediction = DepletionForecast.Predict(consumable, refillHistory);
// prediction → DateOnly (when it runs out)
```

**How it works**:
- **0 refills**: Use user's initial estimate
- **1 refill**: Use estimate from that date  
- **2+ refills**:
  1. Compute durations between refills
  2. Take last 3 durations (rolling window)
  3. Apply outlier dampening (spikes weighted at 0.25x)
  4. Apply recency bias (newer data weighted higher)
  5. Predict: last refill + weighted average

---

### 2. Roster Schedule Generator

**Problem**: How do you create fair rotating schedules?

**Solution**: Pre-generate all slots for 90 days.

```csharp
var slots = RosterGenerator.Generate(roster, members, daysForward: 90);
// slots → List<RosterSlot> (one per day)
```

**Rotation types**:
- **Sequential** (MVP): A → B → C → A → B → C → ...
- **WeeklyPattern** (Phase 2): Specific days per member
- **Custom** (Phase 2): Admin-defined patterns

---

## Features (MVP)

- ✅ Create consumables with initial estimates
- ✅ Log refill events
- ✅ Predict depletion dates
- ✅ Create rosters with sequential rotation
- ✅ Generate 90-day schedules
- ⏳ Offline storage (localStorage)
- ⏳ iCal export for calendar integration
- ⏳ Web Push notifications

---

## Domain Models

All models are immutable records in `src/Models/Domain.cs`:

```csharp
// Consumables
record Consumable(Guid Id, Guid SpaceId, string Name, string Category, string UnitOfMeasure);
record RefillEvent(Guid Id, Guid ConsumableId, DateOnly RefillDate, decimal Quantity);
record DepletionForecast(Guid Id, Guid ConsumableId, DateOnly PredictedFinishDate, int AverageDurationDays);

// Rosters
record Roster(Guid Id, Guid SpaceId, string Name, RosterRotationType RotationType);
record RosterMember(Guid Id, Guid RosterId, string Name);
record RosterSlot(Guid Id, Guid RosterId, Guid RosterMemberId, DateOnly SlotDate);
```

---

## Architecture

**Minimalist. No bloat.**

- **Single project**: Blazor WASM
- **Models**: Immutable records
- **Algorithms**: Static, deterministic methods
- **Pages**: Minimal UI scaffolding (Bootstrap)
- **State**: In-memory (ready for localStorage later)

---

## Next Steps

### Immediate
1. Add localStorage persistence for consumables & rosters
2. Implement refill logging UI
3. Display depletion predictions on consumable cards
4. Show generated roster schedule

### Phase 2
1. WeeklyPattern rotation support
2. Swap request workflow
3. Absence management
4. Price tracking

### Phase 3
1. iCal export for calendar integration
2. Web Push notifications
3. PWA (installable app)
4. Offline sync queue

---

## Key Decisions

| Decision | Why |
|----------|-----|
| **Single WASM project** | Simple, focused, fast to ship |
| **Immutable records** | Thread-safe, predictable |
| **Static algorithms** | No state, deterministic, testable |
| **Bootstrap UI** | Quick styling, responsive |
| **In-memory state** | Start simple, add persistence layer later |

---

## Testing the Algorithms

The algorithms are pure functions—easy to test:

```csharp
// Test depletion with outliers
var consumable = new Consumable(...) { InitialEstimateDays = 30 };
var refills = new[] {
    new RefillEvent { RefillDate = new DateOnly(2025, 1, 1) },
    new RefillEvent { RefillDate = new DateOnly(2025, 2, 10) }, // 40 days
    new RefillEvent { RefillDate = new DateOnly(2025, 3, 15) }, // 33 days
    new RefillEvent { RefillDate = new DateOnly(2025, 5, 15) }  // 61 days (spike)
};

var prediction = DepletionForecast.Predict(consumable, refills);
// Should be ~35-38 days (spike downweighted), not 45
```

---

## Files Reference

| File | Purpose |
|------|---------|
| `src/Models/Domain.cs` | All domain entities |
| `src/Algorithms/Core.cs` | Depletion & roster algorithms |
| `src/Pages/Index.razor` | Home page |
| `src/Pages/Consumables.razor` | Consumables management |
| `src/Pages/Rosters.razor` | Roster management |
| `src/Components/MainLayout.razor` | Layout + navigation |
| `REFYLA_PRD.md` | Product requirements |
| `QUICK_REFERENCE.md` | Algorithm overview |

---

## Context

This app solves problems for:

1. **Nigerian Households**: Managing shared LPG, fuel, generator oil
2. **Student Associations**: Managing rotating duty rosters
3. **Solo Users**: Tracking personal consumables (meds, DSTV, gym payments)

**Why existing tools fail**:
- Google Calendar tracks *when*, not *how much is left*
- Spreadsheets go stale
- WhatsApp rosters disappear
- No tool learns from consumption history

---

## License

Confidential. Intitech, 2026.
