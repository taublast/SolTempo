namespace MusicNotes.UI;

/// <summary>
/// Do NOT cache, painted non-stop maybe
/// </summary>
public class LayerWIthEffects : SkiaLayer
{
    private ConfettiSystem _confetti = new ();

    bool _isConfettiActive;
    protected override void Paint(DrawingContext ctx)
    {
        base.Paint(ctx);

        if (_isConfettiActive)
        {
            _confetti.DrawAndUpdate(ctx.Context.Canvas, DrawingRect, ctx.Scale, ctx.Context.FrameTimeNanos);
        }
    }

    public void ToggleConfetti()
    {
        _isConfettiActive = !_isConfettiActive;
    }

    void EnableConfetti(bool state)
    {
        _isConfettiActive = state;
    }

}
 
 