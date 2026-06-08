using TurnIt.Algorithms;
using TurnIt.Models;

namespace TurnIt.Services;

public class TurnFillDataService
{
    private readonly StorageService _storage;
    public Guid CurrentSpaceId { get; } = Guid.NewGuid(); // Mock current user space
    
    public List<Resource> Resources { get; private set; } = new();
    public List<RosterParticipant> Members { get; private set; } = new();

    public event Action? OnStateChanged;

    public TurnFillDataService(StorageService storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        var consumables = await _storage.GetConsumablesAsync(CurrentSpaceId);
        
        // We assume there's one default roster for the MVP Space if needed
        var rosters = await _storage.GetRostersAsync(CurrentSpaceId);
        var defaultRoster = rosters.FirstOrDefault();
        if (defaultRoster == null)
        {
            defaultRoster = new Roster(Guid.NewGuid(), CurrentSpaceId, "Default Roster", RosterRotationType.Sequential)
            {
                ScheduleStartDate = DateOnly.FromDateTime(DateTime.Today)
            };
            await _storage.SaveRosterAsync(defaultRoster);
            
            // Seed members
            var m1 = new RosterMember(Guid.NewGuid(), defaultRoster.Id, "Sarah M.");
            var m2 = new RosterMember(Guid.NewGuid(), defaultRoster.Id, "Mike");
            var m3 = new RosterMember(Guid.NewGuid(), defaultRoster.Id, "You");
            await _storage.SaveRosterMemberAsync(m1);
            await _storage.SaveRosterMemberAsync(m2);
            await _storage.SaveRosterMemberAsync(m3);
        }

        var rosterMembers = await _storage.GetRosterMembersAsync(defaultRoster.Id);
        var activeMembers = rosterMembers.Where(m => m.IsActive).ToList();
        
        // Map members
        var colors = new[] { "bg-roster-1", "bg-roster-2", "bg-roster-3", "bg-roster-4", "bg-roster-5" };
        Members = activeMembers.Select((m, i) => new RosterParticipant
        {
            Id = m.Id.ToString(),
            Name = m.Name,
            Initials = m.Name.Substring(0, 1).ToUpper(),
            ColorClass = colors[i % colors.Length]
        }).ToList();

        // Map consumables to resources
        var newResources = new List<Resource>();
        foreach (var c in consumables)
        {
            var refills = await _storage.GetRefillsAsync(c.Id);
            var forecast = DepletionForecaster.PredictForecast(c, refills);
            
            int level = 100;
            if (refills.Any())
            {
                var lastRefill = refills.Last().RefillDate.ToDateTime(TimeOnly.MinValue);
                var totalDays = forecast.AverageDurationDays;
                var elapsedDays = (DateTime.Today - lastRefill).Days;
                level = Math.Max(0, Math.Min(100, (int)Math.Round(100.0 * (totalDays - elapsedDays) / totalDays)));
            }
            else if (c.InitialEstimateDays.HasValue)
            {
                var elapsedDays = (DateTime.Today - c.CreatedAt).Days;
                level = Math.Max(0, Math.Min(100, (int)Math.Round(100.0 * (c.InitialEstimateDays.Value - elapsedDays) / c.InitialEstimateDays.Value)));
            }

            // Figure out roster slot
            RosterParticipant? currentAssignee = null;
            if (c.RosterId.HasValue && activeMembers.Any())
            {
                var slots = await _storage.GetRosterSlotsAsync(c.RosterId.Value);
                var activeSlot = slots.FirstOrDefault(s => !s.IsDone);
                if (activeSlot != null)
                {
                    currentAssignee = Members.FirstOrDefault(m => m.Id == activeSlot.RosterMemberId.ToString());
                }
                else
                {
                    // Generate slots if none exist
                    var newSlots = RosterGenerator.Generate(defaultRoster, activeMembers, 30);
                    foreach(var slot in newSlots) await _storage.SaveRosterSlotAsync(slot);
                    if (newSlots.Any())
                    {
                        currentAssignee = Members.FirstOrDefault(m => m.Id == newSlots.First().RosterMemberId.ToString());
                    }
                }
            }

            newResources.Add(new Resource
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                CurrentLevelPercentage = level,
                CurrentAssignee = currentAssignee,
                OriginalConsumable = c,
                Forecast = forecast
            });
        }
        
        Resources = newResources;
        NotifyStateChanged();
    }

    public async Task RefillResource(string resourceId)
    {
        var res = Resources.FirstOrDefault(r => r.Id == resourceId);
        if (res?.OriginalConsumable != null)
        {
            // Log Refill
            var refill = new RefillEvent(Guid.NewGuid(), res.OriginalConsumable.Id, DateOnly.FromDateTime(DateTime.Today), 1m)
            {
                CreatedAt = DateTime.UtcNow
            };
            await _storage.SaveRefillAsync(refill);

            // Advance roster slot if there is one
            if (res.OriginalConsumable.RosterId.HasValue)
            {
                var slots = await _storage.GetRosterSlotsAsync(res.OriginalConsumable.RosterId.Value);
                var activeSlot = slots.FirstOrDefault(s => !s.IsDone);
                if (activeSlot != null)
                {
                    var updatedSlot = activeSlot with { IsDone = true, CompletedAt = DateTime.UtcNow };
                    await _storage.SaveRosterSlotAsync(updatedSlot);
                }
            }

            await LoadDataAsync();
        }
    }

    public async Task AddResource(Resource resource)
    {
        var consumable = new Consumable(Guid.NewGuid(), CurrentSpaceId, resource.Name, "General", "unit")
        {
            InitialEstimateDays = resource.InitialEstimateDays,
            CreatedAt = DateTime.UtcNow
        };
        
        await _storage.SaveConsumableAsync(consumable);
        
        // Let's create an initial refill event so the level starts at 100% based on today
        if (resource.CurrentLevelPercentage >= 100)
        {
            var refill = new RefillEvent(Guid.NewGuid(), consumable.Id, DateOnly.FromDateTime(DateTime.Today), 1m)
            {
                CreatedAt = DateTime.UtcNow
            };
            await _storage.SaveRefillAsync(refill);
        }

        await LoadDataAsync();
    }

    public async Task AddMember(string name)
    {
        var rosters = await _storage.GetRostersAsync(CurrentSpaceId);
        var defaultRoster = rosters.FirstOrDefault();
        if (defaultRoster != null)
        {
            var m = new RosterMember(Guid.NewGuid(), defaultRoster.Id, name)
            {
                IsActive = true,
                AddedAt = DateTime.UtcNow
            };
            await _storage.SaveRosterMemberAsync(m);
            
            // Re-generate roster slots to include new member in the rotation
            var members = await _storage.GetRosterMembersAsync(defaultRoster.Id);
            var activeMembers = members.Where(mem => mem.IsActive).ToList();
            var slots = await _storage.GetRosterSlotsAsync(defaultRoster.Id);
            // Delete future incomplete slots and regenerate
            foreach(var slot in slots.Where(s => !s.IsDone))
            {
                await _storage.DeleteRosterSlotAsync(slot.Id);
            }
            var newSlots = RosterGenerator.Generate(defaultRoster, activeMembers, 30);
            foreach(var slot in newSlots) await _storage.SaveRosterSlotAsync(slot);
        }
        await LoadDataAsync();
    }

    public async Task SkipMember(string memberId)
    {
        // For MVP, skipping just means we reorder the members or mark their current slot as done.
        // Actually, just delete the member and re-add them at the end, or skip the slot.
        // Let's implement skipping a member by updating all their incomplete slots to IsDone = true
        var memberGuid = Guid.Parse(memberId);
        var rosters = await _storage.GetRostersAsync(CurrentSpaceId);
        var defaultRoster = rosters.FirstOrDefault();
        if (defaultRoster != null)
        {
            var slots = await _storage.GetRosterSlotsAsync(defaultRoster.Id);
            var activeSlot = slots.FirstOrDefault(s => s.RosterMemberId == memberGuid && !s.IsDone);
            if (activeSlot != null)
            {
                var updatedSlot = activeSlot with { IsDone = true, CompletedAt = DateTime.UtcNow };
                await _storage.SaveRosterSlotAsync(updatedSlot);
            }
        }
        await LoadDataAsync();
    }

    public async Task RemoveMember(string memberId)
    {
        var memberGuid = Guid.Parse(memberId);
        var rosters = await _storage.GetRostersAsync(CurrentSpaceId);
        var defaultRoster = rosters.FirstOrDefault();
        if (defaultRoster != null)
        {
            var members = await _storage.GetRosterMembersAsync(defaultRoster.Id);
            var member = members.FirstOrDefault(m => m.Id == memberGuid);
            if (member != null)
            {
                var updatedMember = member with { IsActive = false };
                await _storage.SaveRosterMemberAsync(updatedMember);
                
                // Also invalidate future slots
                var slots = await _storage.GetRosterSlotsAsync(defaultRoster.Id);
                foreach(var slot in slots.Where(s => !s.IsDone))
                {
                    await _storage.DeleteRosterSlotAsync(slot.Id);
                }
            }
        }
        await LoadDataAsync();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
