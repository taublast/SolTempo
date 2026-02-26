namespace SolTempo.UI;

/// <summary>
/// Do NOT cache, painted non-stop maybe
/// </summary>
public class LayerWIthEffects : SkiaLayer
{
    private Confetti _confetti = new ();

    bool _isConfettiActive;

    protected override void Paint(DrawingContext ctx)
    {
        base.Paint(ctx);

        if (_isConfettiActive && _confetti != null)
        {
            _confetti.DrawAndUpdate(ctx.Context.Canvas, DrawingRect, ctx.Scale, ctx.Context.FrameTimeNanos);

            // Auto-stop once all pieces have fallen out after spawning was disabled
            if (!_confetti.IsSpawning && !_confetti.HasPieces)
                _isConfettiActive = false;

            Update();
        }
    }

    public void ToggleConfetti()
    {
        bool isFullyOn = _isConfettiActive && (_confetti?.IsSpawning ?? false);
        EnableConfetti(!isFullyOn);
    }

    public void EnableConfetti(bool state)
    {
        if (state)
        {
            _confetti.IsSpawning = true;
            _confetti.ResetSpawnTimer(); // organic ramp-up every time, not an instant burst
            _isConfettiActive = true;
            Update();
        }
        else
        {
            // Stop spawning but keep rendering so existing pieces can fall and fade out
            _confetti.IsSpawning = false;
        }
    }

    public override void OnDisposing()
    {
        _confetti?.Dispose();
        _confetti = null;
        base.OnDisposing();
    }

}
 
 