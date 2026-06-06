# Getting Started with TurnIt

## What You Have

A **single Blazor WebAssembly project** with:
- ✅ Domain models (immutable records)
- ✅ Core algorithms (depletion forecast & roster generation)
- ✅ Basic Razor pages (home, consumables, rosters)
- ✅ Navigation layout
- ✅ Bootstrap styling

## Run It Now

```bash
cd TurnIt
dotnet watch run
```

Open `https://localhost:5001` in your browser.

---

## Project Layout

```
TurnIt/
├── src/
│   ├── Models/Domain.cs           ← All models (User, Space, Consumable, Roster, etc.)
│   ├── Algorithms/Core.cs         ← Prediction & schedule generation
│   ├── App.razor                  ← Router
│   ├── Program.cs                 ← Entry point
│   ├── Pages/
│   │   ├── Index.razor            ← Home
│   │   ├── Consumables.razor      ← Consumables CRUD
│   │   └── Rosters.razor          ← Rosters CRUD
│   ├── Components/
│   │   └── MainLayout.razor       ← Navigation
│   ├── Services/                  ← (Empty, ready for localStorage)
│   └── wwwroot/
│       ├── index.html
│       └── app.css
├── TurnIt.csproj                  ← Single project file
├── README.md
├── REFYLA_PRD.md                  ← Product requirements
└── QUICK_REFERENCE.md             ← Algorithm reference
```

---

## What the Algorithms Do

### DepletionForecast.Predict()

Predicts when a consumable runs out.

```csharp
using TurnIt.Algorithms;

var consumable = new Consumable(...) { InitialEstimateDays = 30 };
var refills = new[] {
    new RefillEvent { RefillDate = new DateOnly(2025, 1, 1), Quantity = 1 },
    new RefillEvent { RefillDate = new DateOnly(2025, 2, 10), Quantity = 1 },
};

var prediction = DepletionForecast.Predict(consumable, refills);
// prediction = DateOnly (e.g., 2025-03-18)
```

**Algorithm**:
- 0 refills → Use estimate
- 1 refill → Estimate from that date
- 2+ refills → Weighted rolling average of durations + outlier dampening

---

### RosterGenerator.Generate()

Creates fair 90-day duty schedules.

```csharp
using TurnIt.Algorithms;

var roster = new Roster(
    id: Guid.NewGuid(),
    spaceId: Guid.NewGuid(),
    name: "Cleaning",
    rotationType: RosterRotationType.Sequential)
{
    ScheduleStartDate = DateOnly.FromDateTime(DateTime.Today)
};

var members = new[] {
    new RosterMember(Guid.NewGuid(), roster.Id, "Alice"),
    new RosterMember(Guid.NewGuid(), roster.Id, "Bob"),
    new RosterMember(Guid.NewGuid(), roster.Id, "Charlie"),
};

var slots = RosterGenerator.Generate(roster, members, daysForward: 90);
// slots = [90 RosterSlot objects, one per day]
```

**Result**: Each member gets ~30 days (fair rotation).

---

## Next: Build Features

### 1. Add localStorage Persistence

Create `src/Services/StorageService.cs`:

```csharp
using System.Text.Json;

namespace TurnIt.Services;

public class StorageService
{
    private static readonly string ConsumablesKey = "consumables";
    private static readonly string RostersKey = "rosters";

    public async Task SaveConsumables(List<Consumable> consumables)
    {
        var json = JsonSerializer.Serialize(consumables);
        // Use localStorage via JS interop
    }

    public async Task<List<Consumable>> LoadConsumables()
    {
        // Load from localStorage
        return new();
    }
}
```

Then in `Program.cs`:
```csharp
builder.Services.AddScoped<StorageService>();
```

### 2. Implement Refill Logging

In `Consumables.razor`, add a detail page showing:
- Consumption history
- Depletion prediction
- Option to log new refill

```razor
@page "/consumables/{id:guid}"
@inject StorageService Storage

<h2>@consumable.Name</h2>

@if (forecast != null)
{
    <div class="alert alert-info">
        Predicted finish: <strong>@forecast.PredictedFinishDate</strong>
        (in @((forecast.PredictedFinishDate - DateOnly.FromDateTime(DateTime.Today)).Days) days)
    </div>
}

<h5>Refill History</h5>
<table class="table">
    <thead>
        <tr><th>Date</th><th>Quantity</th></tr>
    </thead>
    <tbody>
        @foreach (var refill in refills)
        {
            <tr><td>@refill.RefillDate</td><td>@refill.Quantity</td></tr>
        }
    </tbody>
</table>

<button class="btn btn-primary" @onclick="LogRefill">Log Refill</button>

@code {
    [Parameter]
    public Guid Id { get; set; }

    private Consumable? consumable;
    private List<RefillEvent> refills = new();
    private DepletionForecast? forecast;

    protected override async Task OnInitializedAsync()
    {
        consumable = (await Storage.LoadConsumables()).FirstOrDefault(c => c.Id == Id);
        refills = (await Storage.LoadRefills()).Where(r => r.ConsumableId == Id).ToList();
        
        if (consumable != null)
        {
            forecast = DepletionForecast.Predict(consumable, refills);
        }
    }

    private async Task LogRefill()
    {
        // Show form, save to storage, recalculate forecast
    }
}
```

### 3. Show Roster Schedule

In `Rosters.razor`, add detail view:

```razor
@page "/rosters/{id:guid}"
@using TurnIt.Algorithms

@code {
    private List<RosterSlot> slots = new();

    protected override void OnInitialized()
    {
        var roster = GetRoster(Id);
        var members = GetRosterMembers(Id);
        
        slots = RosterGenerator.Generate(roster, members, daysForward: 90);
    }
}
```

Then display slots grouped by month or week.

---

## Testing Your Changes

The algorithms are pure functions—trivial to test:

Create a test project:
```bash
dotnet new xunit -n TurnIt.Tests
cd TurnIt.Tests
dotnet add reference ../TurnIt/TurnIt.csproj
```

Then:
```csharp
using TurnIt.Models;
using TurnIt.Algorithms;

public class DepletionForecastTests
{
    [Fact]
    public void Predict_WithOutliers_DampensSpikes()
    {
        var consumable = new Consumable(Guid.NewGuid(), Guid.NewGuid(), "Fuel", "Gas", "L") 
        { 
            InitialEstimateDays = 30 
        };

        var refills = new[] {
            new RefillEvent(Guid.NewGuid(), consumable.Id, new DateOnly(2025, 1, 1), 1),
            new RefillEvent(Guid.NewGuid(), consumable.Id, new DateOnly(2025, 2, 10), 1), // 40 days
            new RefillEvent(Guid.NewGuid(), consumable.Id, new DateOnly(2025, 3, 15), 1), // 33 days
            new RefillEvent(Guid.NewGuid(), consumable.Id, new DateOnly(2025, 5, 15), 1), // 61 days (spike!)
        };

        var forecast = DepletionForecast.Predict(consumable, refills);
        
        // Spike should be downweighted; avg should be ~35-38 days, not 45
        Assert.InRange(
            (forecast.PredictedFinishDate - refills.Last().RefillDate).Days,
            35, 38
        );
    }
}
```

Run tests:
```bash
dotnet test
```

---

## Common Tasks

### Add a new page
1. Create `src/Pages/MyPage.razor`
2. Add `@page "/mypage"`
3. Link in `MainLayout.razor`

### Add a service
1. Create `src/Services/MyService.cs`
2. Register in `Program.cs`: `builder.Services.AddScoped<MyService>();`
3. Inject in pages: `@inject MyService Service`

### Modify algorithms
Edit `src/Algorithms/Core.cs`:
- `DepletionForecast` class for prediction logic
- `RosterGenerator` class for schedule logic

### Add styling
Edit `src/wwwroot/app.css` or add inline `<style>` blocks in `.razor` files.

---

## Key Files

| File | What It Does |
|------|-------------|
| `src/Models/Domain.cs` | All domain objects (Consumable, Roster, etc.) |
| `src/Algorithms/Core.cs` | Prediction & schedule generation algorithms |
| `src/Pages/Index.razor` | Home page |
| `src/Pages/Consumables.razor` | Consumables list & CRUD |
| `src/Pages/Rosters.razor` | Rosters list & CRUD |
| `src/Components/MainLayout.razor` | Navigation & layout |
| `src/Program.cs` | App startup |
| `TurnIt.csproj` | Project config |

---

## Roadmap

### Now (In Progress)
- [ ] localStorage persistence for consumables
- [ ] localStorage persistence for rosters
- [ ] Detail pages with refill history
- [ ] Display predictions on consumable cards
- [ ] Show generated roster schedules

### Phase 1 (MVP)
- [ ] Refill logging UI
- [ ] Roster member management
- [ ] Swap request workflow
- [ ] Web Push notifications
- [ ] iCal export

### Phase 2
- [ ] WeeklyPattern rotation type
- [ ] Custom rotation support
- [ ] Absence management
- [ ] Price tracking

### Phase 3
- [ ] Offline sync queue
- [ ] PWA installation
- [ ] AI predictions

---

## Questions?

- Algorithms unclear? See `QUICK_REFERENCE.md`
- Product scope? See `REFYLA_PRD.md`
- Models? See `src/Models/Domain.cs`

Good luck! 🚀
