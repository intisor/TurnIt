using Microsoft.JSInterop;
using System.Text.Json;
using TurnIt.Models;

namespace TurnIt.Services;

/// <summary>
/// Manages local storage for consumables, rosters, and related data.
/// Uses JSON serialization for persistence to browser localStorage.
/// </summary>
public class StorageService
{
    private const string ConsumablesKey = "turnit_consumables";
    private const string RosterKey = "turnit_rosters";
    private const string RefillsKey = "turnit_refills";
    private const string RosterMembersKey = "turnit_roster_members";
    private const string RosterSlotsKey = "turnit_roster_slots";

    private readonly IJSRuntime _js;
    private Dictionary<string, object> _storage = new();
    private bool _isLoaded = false;

    public StorageService(IJSRuntime js)
    {
        _js = js;
    }

    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;

        _storage[ConsumablesKey] = await LoadFromJsAsync<List<Consumable>>(ConsumablesKey) ?? new List<Consumable>();
        _storage[RosterKey] = await LoadFromJsAsync<List<Roster>>(RosterKey) ?? new List<Roster>();
        _storage[RefillsKey] = await LoadFromJsAsync<List<RefillEvent>>(RefillsKey) ?? new List<RefillEvent>();
        _storage[RosterMembersKey] = await LoadFromJsAsync<List<RosterMember>>(RosterMembersKey) ?? new List<RosterMember>();
        _storage[RosterSlotsKey] = await LoadFromJsAsync<List<RosterSlot>>(RosterSlotsKey) ?? new List<RosterSlot>();

        _isLoaded = true;
    }

    private async Task<T?> LoadFromJsAsync<T>(string key)
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", key);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<T>(json);
            }
        }
        catch { /* Ignore JS interop errors during pre-rendering */ }
        return default;
    }

    private async Task SaveToJsAsync(string key, object data)
    {
        try
        {
            var json = JsonSerializer.Serialize(data);
            await _js.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch { /* Ignore errors */ }
    }

    // Consumables
    public async Task<List<Consumable>> GetConsumablesAsync(Guid spaceId)
    {
        await EnsureLoadedAsync();
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        return consumables.Where(c => c.SpaceId == spaceId).ToList();
    }

    public async Task<Consumable?> GetConsumableAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        return consumables.FirstOrDefault(c => c.Id == id);
    }

    public async Task SaveConsumableAsync(Consumable consumable)
    {
        await EnsureLoadedAsync();
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        var existing = consumables.FirstOrDefault(c => c.Id == consumable.Id);

        if (existing != null)
        {
            consumables.Remove(existing);
        }

        consumables.Add(consumable);
        _storage[ConsumablesKey] = consumables;
        await SaveToJsAsync(ConsumablesKey, consumables);
    }

    public async Task DeleteConsumableAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var consumables = _storage[ConsumablesKey] as List<Consumable> ?? new();
        consumables.RemoveAll(c => c.Id == id);
        _storage[ConsumablesKey] = consumables;
        await SaveToJsAsync(ConsumablesKey, consumables);
    }

    // Refill Events
    public async Task<List<RefillEvent>> GetRefillsAsync(Guid consumableId)
    {
        await EnsureLoadedAsync();
        var refills = _storage[RefillsKey] as List<RefillEvent> ?? new();
        return refills.Where(r => r.ConsumableId == consumableId).OrderBy(r => r.RefillDate).ToList();
    }

    public async Task SaveRefillAsync(RefillEvent refill)
    {
        await EnsureLoadedAsync();
        var refills = _storage[RefillsKey] as List<RefillEvent> ?? new();

        var existing = refills.FirstOrDefault(r => r.Id == refill.Id);
        if (existing != null)
        {
            refills.Remove(existing);
        }

        refills.Add(refill);
        _storage[RefillsKey] = refills;
        await SaveToJsAsync(RefillsKey, refills);
    }

    public async Task DeleteRefillAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var refills = _storage[RefillsKey] as List<RefillEvent> ?? new();
        refills.RemoveAll(r => r.Id == id);
        _storage[RefillsKey] = refills;
        await SaveToJsAsync(RefillsKey, refills);
    }

    // Rosters
    public async Task<List<Roster>> GetRostersAsync(Guid spaceId)
    {
        await EnsureLoadedAsync();
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        return rosters.Where(r => r.SpaceId == spaceId).ToList();
    }

    public async Task<Roster?> GetRosterAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        return rosters.FirstOrDefault(r => r.Id == id);
    }

    public async Task SaveRosterAsync(Roster roster)
    {
        await EnsureLoadedAsync();
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        var existing = rosters.FirstOrDefault(r => r.Id == roster.Id);

        if (existing != null)
        {
            rosters.Remove(existing);
        }

        rosters.Add(roster);
        _storage[RosterKey] = rosters;
        await SaveToJsAsync(RosterKey, rosters);
    }

    public async Task DeleteRosterAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var rosters = _storage[RosterKey] as List<Roster> ?? new();
        rosters.RemoveAll(r => r.Id == id);
        _storage[RosterKey] = rosters;
        await SaveToJsAsync(RosterKey, rosters);
    }

    // Roster Members
    public async Task<List<RosterMember>> GetRosterMembersAsync(Guid rosterId)
    {
        await EnsureLoadedAsync();
        var members = _storage[RosterMembersKey] as List<RosterMember> ?? new();
        return members.Where(m => m.RosterId == rosterId).ToList();
    }

    public async Task SaveRosterMemberAsync(RosterMember member)
    {
        await EnsureLoadedAsync();
        var members = _storage[RosterMembersKey] as List<RosterMember> ?? new();
        var existing = members.FirstOrDefault(m => m.Id == member.Id);

        if (existing != null)
        {
            members.Remove(existing);
        }

        members.Add(member);
        _storage[RosterMembersKey] = members;
        await SaveToJsAsync(RosterMembersKey, members);
    }

    public async Task DeleteRosterMemberAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var members = _storage[RosterMembersKey] as List<RosterMember> ?? new();
        members.RemoveAll(m => m.Id == id);
        _storage[RosterMembersKey] = members;
        await SaveToJsAsync(RosterMembersKey, members);
    }

    // Roster Slots
    public async Task<List<RosterSlot>> GetRosterSlotsAsync(Guid rosterId)
    {
        await EnsureLoadedAsync();
        var slots = _storage[RosterSlotsKey] as List<RosterSlot> ?? new();
        return slots.Where(s => s.RosterId == rosterId).OrderBy(s => s.SlotDate).ToList();
    }

    public async Task SaveRosterSlotAsync(RosterSlot slot)
    {
        await EnsureLoadedAsync();
        var slots = _storage[RosterSlotsKey] as List<RosterSlot> ?? new();
        var existing = slots.FirstOrDefault(s => s.Id == slot.Id);

        if (existing != null)
        {
            slots.Remove(existing);
        }

        slots.Add(slot);
        _storage[RosterSlotsKey] = slots;
        await SaveToJsAsync(RosterSlotsKey, slots);
    }

    public async Task DeleteRosterSlotAsync(Guid id)
    {
        await EnsureLoadedAsync();
        var slots = _storage[RosterSlotsKey] as List<RosterSlot> ?? new();
        slots.RemoveAll(s => s.Id == id);
        _storage[RosterSlotsKey] = slots;
        await SaveToJsAsync(RosterSlotsKey, slots);
    }
}
