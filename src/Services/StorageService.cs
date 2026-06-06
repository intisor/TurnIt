using System.Text.Json;
using TurnIt.Models;

namespace TurnIt.Services;

/// <summary>
/// Manages local storage for consumables, rosters, and related data.
/// Uses JSON serialization for persistence.
/// </summary>
public class StorageService
{
    private const string ConsumablesKey = "turnit_consumables";
    private const string RosterKey = "turnit_rosters";
    private const string RefillsKey = "turnit_refills";
    private const string RosterMembersKey = "turnit_roster_members";

    // In-memory storage (will persist to localStorage in Phase 2)
    private Dictionary<string, object> _storage = new();

    public StorageService()
    {
        InitializeStorage();
    }

    private void InitializeStorage()
    {
        _storage[ConsumablesKey] = new List<Consumable>();
        _storage[RosterKey] = new List<Roster>();
        _storage[RefillsKey] = new List<RefillEvent>();
        _storage[RosterMembersKey] = new List<RosterMember>();
    }

    // Consumables
    public async Task<List<Consumable>> GetConsumablesAsync(Guid spaceId)
    {
        await Task.Delay(10); // Simulate async
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        return consumables.Where(c => c.SpaceId == spaceId).ToList();
    }

    public async Task<Consumable?> GetConsumableAsync(Guid id)
    {
        await Task.Delay(10);
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        return consumables.FirstOrDefault(c => c.Id == id);
    }

    public async Task SaveConsumableAsync(Consumable consumable)
    {
        await Task.Delay(10);
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        var existing = consumables.FirstOrDefault(c => c.Id == consumable.Id);

        if (existing != null)
        {
            consumables.Remove(existing);
        }

        consumables.Add(consumable);
        _storage[ConsumablesKey] = consumables;
    }

    public async Task DeleteConsumableAsync(Guid id)
    {
        await Task.Delay(10);
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        consumables.RemoveAll(c => c.Id == id);
        _storage[ConsumablesKey] = consumables;
    }

    // Refill Events
    public async Task<List<RefillEvent>> GetRefillsAsync(Guid consumableId)
    {
        await Task.Delay(10);
        var refills = _storage[RefillsKey] as List<RefillEvent> ?? new();
        return refills.Where(r => r.ConsumableId == consumableId).OrderBy(r => r.RefillDate).ToList();
    }

    public async Task SaveRefillAsync(RefillEvent refill)
    {
        await Task.Delay(10);
        var refills = _storage[RefillsKey] as List<RefillEvent> ?? new();

        var existing = refills.FirstOrDefault(r => r.Id == refill.Id);
        if (existing != null)
        {
            refills.Remove(existing);
        }

        refills.Add(refill);
        _storage[RefillsKey] = refills;
    }

    public async Task DeleteRefillAsync(Guid id)
    {
        await Task.Delay(10);
        var refills = _storage[RefillsKey] as List<RefillEvent> ?? new();
        refills.RemoveAll(r => r.Id == id);
        _storage[RefillsKey] = refills;
    }

    // Rosters
    public async Task<List<Roster>> GetRostersAsync(Guid spaceId)
    {
        await Task.Delay(10);
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        return rosters.Where(r => r.SpaceId == spaceId).ToList();
    }

    public async Task<Roster?> GetRosterAsync(Guid id)
    {
        await Task.Delay(10);
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        return rosters.FirstOrDefault(r => r.Id == id);
    }

    public async Task SaveRosterAsync(Roster roster)
    {
        await Task.Delay(10);
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        var existing = rosters.FirstOrDefault(r => r.Id == roster.Id);

        if (existing != null)
        {
            rosters.Remove(existing);
        }

        rosters.Add(roster);
        _storage[RosterKey] = rosters;
    }

    public async Task DeleteRosterAsync(Guid id)
    {
        await Task.Delay(10);
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        rosters.RemoveAll(r => r.Id == id);
        _storage[RosterKey] = rosters;
    }

    // Roster Members
    public async Task<List<RosterMember>> GetRosterMembersAsync(Guid rosterId)
    {
        await Task.Delay(10);
        var members = _storage[RosterMembersKey] as List<RosterMember> ?? new();
        return members.Where(m => m.RosterId == rosterId).ToList();
    }

    public async Task SaveRosterMemberAsync(RosterMember member)
    {
        await Task.Delay(10);
        var members = _storage[RosterMembersKey] as List<RosterMember> ?? new();
        var existing = members.FirstOrDefault(m => m.Id == member.Id);

        if (existing != null)
        {
            members.Remove(existing);
        }

        members.Add(member);
        _storage[RosterMembersKey] = members;
    }

    public async Task DeleteRosterMemberAsync(Guid id)
    {
        await Task.Delay(10);
        var members = _storage[RosterMembersKey] as List<RosterMember> ?? new();
        members.RemoveAll(m => m.Id == id);
        _storage[RosterMembersKey] = members;
    }
}
