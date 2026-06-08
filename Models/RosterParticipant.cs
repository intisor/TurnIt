namespace TurnIt.Models;

public class RosterParticipant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string ColorClass { get; set; } = "bg-surface-variant"; // e.g. bg-roster-1
}
