using SolTempo.Audio;

namespace SolTempo.UI;

/// <summary>
/// Describes a single achievement: what musical event triggers it, a display name,
/// and the action to run when it fires.
/// </summary>
public sealed class AchievementDefinition
{
    public required NoteSequenceEventKind Trigger { get; init; }
    public required string Name { get; init; }
    public required Action OnAchieved { get; init; }
}

/// <summary>
/// Holds the registered achievement definitions and dispatches them
/// when a matching sequence event is detected.
/// </summary>
public sealed class AchievementEngine
{
    private readonly List<AchievementDefinition> _definitions = new();

    public IReadOnlyList<AchievementDefinition> Definitions => _definitions;

    public void Register(AchievementDefinition definition)
        => _definitions.Add(definition);

    /// <summary>Fires every achievement whose Trigger matches <paramref name="kind"/>.</summary>
    public void Process(NoteSequenceEventKind kind)
    {
        foreach (var def in _definitions)
        {
            if (def.Trigger == kind)
                def.OnAchieved();
        }
    }
}
