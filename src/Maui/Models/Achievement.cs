using SolTempo.Audio;

namespace SolTempo.UI;

/// <summary>
/// Describes a single achievement: what musical event triggers it, a display name,
/// and the action to run when it fires.
/// </summary>
public sealed class Achievement
{
    public required NoteSequenceEventKind Trigger { get; init; }
    public required string Name { get; init; }
    public required Action OnAchieved { get; init; }
}