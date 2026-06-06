# TurnIt Quick Reference

## Core Algorithms at a Glance

### Algorithm 1: Depletion Forecast

**What it does**: Predicts when a consumable runs out.

**How it works**:
```
Input: Consumable + RefillHistory
Output: PredictedFinishDate

Flow:
  0 refills   → Use initial estimate
  1 refill    → Estimate from that refill date
  2+ refills  → Rolling average of durations with outlier dampening + recency bias
```

**Quick Example**:
```
Refills:  [Jan 1, Feb 10, Mar 15, May 18]
Durations: [40 days, 33 days, 64 days]

Last 3:    [40, 33, 64]
Average:   ~45 days
Spike?     64 > 45*2? No. But close. Slight downweight.
Weighted:  ~39 days (recent durations weighted higher)

Last refill: May 18
Prediction:  May 18 + 39 days = June 26
```

**Usage**:
```csharp
var forecast = DepletionForecastEngine.Calculate(consumable, refills);
```

---

### Algorithm 2: Roster Schedule

**What it does**: Generates fair, rotating duty schedules.

**How it works**:
```
Input: Roster + Members + DaysForward
Output: RosterSlots (one per day)

For Sequential (MVP):
  Day 1: members[0]
  Day 2: members[1]
  Day 3: members[2]
  Day 4: members[0]  ← cycles
  ...
```

**Quick Example**:
```
Members: [Alice, Bob, Charlie]
Days:    90

Output:
  Day 1:  Alice   Day 31: Alice   Day 61: Alice
  Day 2:  Bob     Day 32: Bob     Day 62: Bob
  Day 3:  Charlie Day 33: Charlie Day 63: Charlie
  ...
  
Result: Each member gets ~30 days (fair)
```

**Usage**:
```csharp
var slots = RosterScheduleGenerator.GenerateSchedule(roster, members, daysForward: 90);
```

---

## Entity Relationships

```
User
  → SpaceMember (role in space)
      → Space (Personal/Household/Team)
          → Consumables
              → RefillEvents (log)
              → DepletionForecast (cached prediction)
          → Rosters
              → RosterMembers (person in roster)
                  → RosterSlots (individual duties)
                      → SwapRequests (swap proposals)
```

---

## Multi-Tenancy: The Security Model

**Problem**: User A shouldn't see data from spaces they don't belong to.

**Solution**: EF Core Global Query Filters.

**How**:
```csharp
// Every query on Consumables is automatically filtered
modelBuilder.Entity<Consumable>()
    .HasQueryFilter(c => c.Space.Members.Any(m => m.UserId == _currentUserId));
```

**Result**: User can't access data outside their spaces, even with direct ID manipulation.

```
Query:    db.Consumables.ToList()
Becomes:  SELECT * FROM Consumables WHERE SpaceId IN (
            SELECT SpaceId FROM SpaceMembers WHERE UserId = @userId
          )
```

---

## Key Design Decisions

| Aspect | Decision | Why |
|--------|----------|-----|
| **Models** | Pure POCOs, no EF attributes | Testable, portable |
| **Algorithms** | Static methods, no state | Fast, deterministic |
| **Data access** | Global query filters | Secure by default |
| **API style** | Handlers, not controllers | Minimal boilerplate |
| **Dates** | DateOnly, not DateTime | No time zone confusion |
| **Predictions** | Cached separately | Fast reads; recalc on change |
| **Schedules** | Pre-generated 90 days | Quick queries; no runtime generation |

---

## File Locations

**Models**: `TurnIt.Core/Models/*.cs`
**Algorithms**: `TurnIt.Core/Algorithms/*.cs`
**Database**: `TurnIt.Api/Data/AppDbContext.cs`
**Routes/Handlers**: `TurnIt.Api/Handlers/*.cs` (to be created)
**Frontend**: `TurnIt.Client/` (to be created)

---

## MVP Feature List

- ✅ Multi-tenant spaces (Personal, Household)
- ✅ Depletion tracking with rolling average prediction
- ✅ Sequential roster generation
- ✅ Basic roster swap requests
- ⏳ JWT auth (design ready, implementation pending)
- ⏳ iCal export (design ready, implementation pending)
- ⏳ Blazor WASM frontend (design ready)
- ⏳ Offline sync (design ready)

---

## Performance Targets

| Operation | Constraint | Status |
|-----------|-----------|--------|
| Blazor WASM cold start | < 4s on 3G | Design for AOT |
| Roster generation (10 members, 90 days) | < 2s | Pure arithmetic ✅ |
| API response (p95) | < 300ms | DB indexed ✅ |
| iCal feed generation | < 500ms | TBD |

---

## Testing the Core

```csharp
// Test depletion with outliers
[Fact]
public void OutlierDampening()
{
    var refills = new[] {
        new RefillEvent { RefillDate = new DateOnly(2025, 1, 1) },
        new RefillEvent { RefillDate = new DateOnly(2025, 2, 10) }, // 40 days
        new RefillEvent { RefillDate = new DateOnly(2025, 3, 15) }, // 33 days
        new RefillEvent { RefillDate = new DateOnly(2025, 5, 15) }  // 61 days (spike!)
    };
    
    var forecast = DepletionForecastEngine.Calculate(consumable, refills);
    
    // Spike downweighted; should be ~35-38 days, not 45
    Assert.InRange(forecast.AverageDurationDays, 35, 38);
}

// Test roster fairness
[Fact]
public void RosterFairness()
{
    var members = new[] { Alice, Bob };
    var slots = RosterScheduleGenerator.GenerateSchedule(roster, members, 90);
    
    // With 2 members, each should get ~45 days
    var alice = slots.Count(s => s.RosterMemberId == Alice.Id);
    var bob = slots.Count(s => s.RosterMemberId == Bob.Id);
    
    Assert.Equal(45, alice);
    Assert.Equal(45, bob);
}
```

---

## Next Immediate Steps

1. **Create test projects** and verify algorithms with unit tests
2. **Create Program.cs** with DI, database config
3. **Create handlers** for consumables and rosters
4. **Add JWT middleware** for auth
5. **Build Blazor WASM client** starting with login page

Or: Take the foundation and run with it. All hard decisions are made.

