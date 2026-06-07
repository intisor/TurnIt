namespace TurnIt.Algorithms;

using TurnIt.Models;

/// <summary>
/// Core algorithms for TurnIt.
/// </summary>
public static class DepletionForecaster
{
    private const int MaxHistoryCount = 3;
    private const double OutlierThreshold = 2.0;
    private const double OutlierWeight = 0.25;

    /// <summary>
    /// Predict when a consumable will run out.
    /// </summary>
    public static DateOnly Predict(Consumable consumable, IEnumerable<RefillEvent> history)
    {
        var refills = history.OrderBy(r => r.RefillDate).ToList();
        var count = refills.Count;

        // No history: use initial estimate
        if (count == 0)
        {
            var days = consumable.InitialEstimateDays ?? 30;
            return DateOnly.FromDateTime(DateTime.Today).AddDays(days);
        }

        // One refill: use estimate from that date
        if (count == 1)
        {
            var days = consumable.InitialEstimateDays ?? 30;
            return refills[0].RefillDate.AddDays(days);
        }

        // 2+ refills: weighted rolling average
        var durations = ComputeDurations(refills);
        var avgDays = ComputeWeightedAverage(durations);
        return refills.Last().RefillDate.AddDays((int)Math.Round(avgDays));
    }

    /// <summary>
    /// Predict when a consumable will run out and return a full forecast.
    /// </summary>
    public static DepletionForecast PredictForecast(Consumable consumable, IEnumerable<RefillEvent> history)
    {
        var refills = history.OrderBy(r => r.RefillDate).ToList();
        var predictedDate = Predict(consumable, refills);

        int avgDays = consumable.InitialEstimateDays ?? 30;
        if (refills.Count > 1)
        {
            var durations = ComputeDurations(refills);
            if (durations.Any())
            {
                avgDays = (int)Math.Round(durations.Average());
            }
        }

        return new DepletionForecast(
            Guid.NewGuid(),
            consumable.Id,
            predictedDate,
            avgDays,
            refills.Count)
        {
            CalculatedAt = DateTime.UtcNow
        };
    }

    private static List<int> ComputeDurations(List<RefillEvent> refills)
    {
        var result = new List<int>();
        for (int i = 1; i < refills.Count; i++)
        {
            var days = (refills[i].RefillDate.ToDateTime(TimeOnly.MinValue) -
                        refills[i - 1].RefillDate.ToDateTime(TimeOnly.MinValue)).Days;
            result.Add(days);
        }
        return result;
    }

    private static double ComputeWeightedAverage(List<int> durations)
    {
        if (durations.Count == 0) return 30.0;

        var recent = durations.TakeLast(MaxHistoryCount).ToList();
        var simpleAvg = recent.Average();

        double weightedSum = 0;
        double weightSum = 0;

        for (int i = 0; i < recent.Count; i++)
        {
            var isOutlier = recent[i] > simpleAvg * OutlierThreshold;
            var baseWeight = isOutlier ? OutlierWeight : 1.0;
            var recencyWeight = 1.0 + (i * 0.1);
            var weight = baseWeight * recencyWeight;

            weightedSum += recent[i] * weight;
            weightSum += weight;
        }

        return weightedSum / weightSum;
    }
}

/// <summary>
/// Generate fair rotating duty schedules.
/// </summary>
public static class RosterGenerator
{
    public static List<RosterSlot> Generate(
        Roster roster,
        IEnumerable<RosterMember> members,
        int daysForward = 90)
    {
        var active = members.Where(m => m.IsActive).ToList();
        if (!active.Any()) return [];

        var slots = new List<RosterSlot>();
        var startDate = roster.ScheduleStartDate;
        var endDate = startDate.AddDays(daysForward);

        return roster.RotationType switch
        {
            RosterRotationType.Sequential => GenerateSequential(roster, active, startDate, endDate),
            RosterRotationType.WeeklyPattern => GenerateWeeklyPattern(roster, active, startDate, endDate),
            RosterRotationType.Custom => [],
            _ => []
        };
    }

    private static List<RosterSlot> GenerateSequential(
        Roster roster,
        List<RosterMember> members,
        DateOnly start,
        DateOnly end)
    {
        var slots = new List<RosterSlot>();
        var startIdx = roster.StartMemberId.HasValue
            ? members.FindIndex(m => m.Id == roster.StartMemberId)
            : 0;

        if (startIdx < 0) startIdx = 0;

        var date = start;
        var idx = startIdx;

        while (date < end)
        {
            slots.Add(new RosterSlot(
                Guid.NewGuid(),
                roster.Id,
                members[idx].Id,
                date)
            {
                CreatedAt = DateTime.UtcNow
            });

            date = date.AddDays(1);
            idx = (idx + 1) % members.Count;
        }

        return slots;
    }

    private static List<RosterSlot> GenerateWeeklyPattern(
        Roster roster,
        List<RosterMember> members,
        DateOnly start,
        DateOnly end)
    {
        var slots = new List<RosterSlot>();
        var date = start;

        while (date < end)
        {
            var dow = (int)date.DayOfWeek;

            foreach (var member in members)
            {
                if (string.IsNullOrEmpty(member.AvailableDaysOfWeek)) continue;

                var days = member.AvailableDaysOfWeek
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => int.Parse(d.Trim()))
                    .ToList();

                if (days.Contains(dow))
                {
                    slots.Add(new RosterSlot(
                        Guid.NewGuid(),
                        roster.Id,
                        member.Id,
                        date)
                    {
                        CreatedAt = DateTime.UtcNow
                    });
                    break;
                }
            }

            date = date.AddDays(1);
        }

        return slots;
    }
}
