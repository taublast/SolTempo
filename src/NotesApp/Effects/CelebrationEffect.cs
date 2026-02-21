using DrawnUi.Draw;

namespace MusicNotes.Effects;

/// <summary>
/// Fullscreen celebration shader effect driven by a RangeAnimator (progress 0→1).
/// Attach to a SkiaImage (or any cached control), call Play(), remove on Completed.
///
/// Usage:
///   var fx = new CelebrationEffect { ShaderSource = @"Shaders\celebrate_starburst.sksl" };
///   fx.Completed += (s, e) => { _backgroundImage.FX.Remove(fx); fx.Dispose(); };
///   _backgroundImage.FX.Add(fx);
///   fx.Play(_backgroundImage);
/// </summary>
public class CelebrationEffect : SkiaShaderEffect
{
    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);
    }


    public event EventHandler Completed;

    /// <summary>
    /// Total animation duration in milliseconds. Default 2500.
    /// </summary>
    public uint DurationMs { get; set; } = 2500;

    /// <summary>
    /// Current animation progress 0.0 → 1.0.
    /// Set automatically by Play(); can also be set manually for debugging.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Normalized center position of the effect (0.0–1.0 in each axis).
    /// Default (0.5, 0.44) puts the center slightly above mid-screen.
    /// Change this to move where radial effects originate from.
    /// </summary>
    public SKPoint Center { get; set; } = new SKPoint(0.5f, 0.44f);

    private RangeAnimator _animator;

    /// <summary>
    /// Starts the celebration animation on the given parent control.
    /// Safe to call multiple times — restarts from zero each time.
    /// </summary>
    public void Play(SkiaControl parent)
    {
        _animator?.Stop();
        _animator ??= new RangeAnimator(parent);

        Progress = 0.0;

        _animator.Start(
            value =>
            {
                Progress = value;
                Update();

                if (value >= 1.0)
                {
                    Completed?.Invoke(this, EventArgs.Empty);
                }
            },
            start:   0.0,
            end:     1.0,
            ms:      DurationMs,
            easing:  Easing.Linear);

        Update();
    }

    /// <summary>
    /// Stops the animation without firing Completed.
    /// </summary>
    public void Stop()
    {
        _animator?.Stop();
    }

    protected override SKRuntimeEffectUniforms CreateUniforms(SKRect destination)
    {
        var uniforms = base.CreateUniforms(destination);
        uniforms["progress"] = (float)Progress;
        uniforms["iCenter"]  = new float[] { Center.X, Center.Y };
        return uniforms;
    }

    protected override void OnDisposing()
    {
        _animator?.Stop();
        _animator = null;
        base.OnDisposing();
    }
}
