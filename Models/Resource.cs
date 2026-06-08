namespace TurnIt.Models;

public enum ResourceState
{
    Healthy,
    Depleting,
    Urgent
}

public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int CurrentLevelPercentage { get; set; } = 100;
    public int InitialEstimateDays { get; set; } = 30;
    
    public RosterParticipant? CurrentAssignee { get; set; }
    
    public Consumable? OriginalConsumable { get; set; }
    public DepletionForecast? Forecast { get; set; }

    public ResourceState State
    {
        get
        {
            if (CurrentLevelPercentage > 60) return ResourceState.Healthy;
            if (CurrentLevelPercentage > 25) return ResourceState.Depleting;
            return ResourceState.Urgent;
        }
    }

    public string StateColorClass => State switch
    {
        ResourceState.Healthy => "bg-state-healthy",
        ResourceState.Depleting => "bg-state-warning",
        ResourceState.Urgent => "bg-state-urgent",
        _ => "bg-surface-variant"
    };

    public string StateBorderColorClass => State switch
    {
        ResourceState.Healthy => "border-state-healthy",
        ResourceState.Depleting => "border-state-warning",
        ResourceState.Urgent => "border-state-urgent",
        _ => "border-surface-variant"
    };

    public string StateTextColorClass => State switch
    {
        ResourceState.Healthy => "text-state-healthy",
        ResourceState.Depleting => "text-state-warning",
        ResourceState.Urgent => "text-state-urgent",
        _ => "text-on-surface"
    };
}
