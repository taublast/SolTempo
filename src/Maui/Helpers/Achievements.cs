using SolTempo.Audio;

namespace SolTempo.UI;

/// <summary>
/// Holds the registered achievement definitions and dispatches them
/// when a matching sequence event is detected.
/// </summary>
public sealed class Achievements
{
    private readonly List<Achievement> _definitions = new();

    public IReadOnlyList<Achievement> Definitions => _definitions;

    public void Register(Achievement definition)
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
