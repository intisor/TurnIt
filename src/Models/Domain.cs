namespace TurnIt.Models;

// User & Space
public record User(Guid Id, string Email, string? DisplayName);

public record Space(Guid Id, string Name, SpaceType Type)
{
    public Guid CreatedByUserId { get; init; }
    public DateTime CreatedAt { get; init; }
}

public enum SpaceType { Personal, Household, Team }

public record SpaceMember(Guid Id, Guid SpaceId, Guid UserId, SpaceRole Role)
{
    public DateTime JoinedAt { get; init; }
}

public enum SpaceRole { Owner, Admin, Member }

// Consumables
public record Consumable(Guid Id, Guid SpaceId, string Name, string Category, string UnitOfMeasure)
{
    public int? InitialEstimateDays { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record RefillEvent(Guid Id, Guid ConsumableId, DateOnly RefillDate, decimal Quantity)
{
    public decimal? Cost { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record DepletionForecast(
    Guid Id,
    Guid ConsumableId,
    DateOnly PredictedFinishDate,
    int AverageDurationDays,
    int RefillCount)
{
    public DateTime CalculatedAt { get; init; }
}

// Rosters
public record Roster(Guid Id, Guid SpaceId, string Name, RosterRotationType RotationType)
{
    public string? Description { get; init; }
    public Guid? StartMemberId { get; init; }
    public DateOnly ScheduleStartDate { get; init; }
    public DateTime CreatedAt { get; init; }
}

public enum RosterRotationType { Sequential, WeeklyPattern, Custom }

public record RosterMember(Guid Id, Guid RosterId, string Name)
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; } = true;
    public string? AvailableDaysOfWeek { get; init; }
    public DateTime AddedAt { get; init; }
}

public record RosterSlot(Guid Id, Guid RosterId, Guid RosterMemberId, DateOnly SlotDate)
{
    public bool IsDone { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SwapRequest(
    Guid Id,
    Guid RosterSlotId,
    Guid RequestingMemberId,
    Guid TargetMemberId,
    SwapRequestStatus Status)
{
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
}

public enum SwapRequestStatus { Pending, Approved, Rejected, Cancelled }
