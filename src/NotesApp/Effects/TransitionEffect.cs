using DrawnUi.Draw;

namespace MusicNotes.Effects;

/// <summary>
/// Single-texture shader transition effect. Hides the control (progress 0→0.5),
/// fires <see cref="Midpoint"/> so you can swap content, then reveals it (0.5→1).
///
/// Usage:
///   var fx = new TransitionEffect { ShaderSource = @"Shaders\transition_iris.sksl" };
///   fx.Midpoint  += (s, e) => { /* swap your module here */ };
///   fx.Completed += (s, e) => { _backgroundImage.VisualEffects.Remove(fx); fx.Dispose(); };
///   _backgroundImage.VisualEffects.Add(fx);
///   fx.Play(_backgroundImage);
///
/// Available shaders:
///   transition_iris.sksl    — black circle iris expand/contract
///   transition_ripple.sksl  — shockwave ring sweeps out then in
///   transition_swirl.sksl   — vortex spiral fade
///   transition_zoom.sksl    — punch zoom + white flash
/// </summary>
public class TransitionEffect : SkiaShaderEffect
{
    /// <summary>
    /// Fired when progress reaches 0.5 — the image is fully hidden.
    /// Swap your UI content here; the reveal phase will show the new state.
    /// </summary>
    public event EventHandler Midpoint;

    /// <summary>
    /// Fired when the full animation (hide + reveal) has completed.
    /// Remove and dispose the effect here.
    /// </summary>
    public event EventHandler Completed;

    /// <summary>
    /// Total duration for the full hide+reveal cycle in milliseconds. Default 700.
    /// </summary>
    public uint DurationMs { get; set; } = 700;

    /// <summary>
    /// Current animation progress 0.0→1.0.
    /// 0.5 = fully hidden midpoint.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Normalized center of the effect (0–1 in each axis).
    /// Default (0.5, 0.5) = screen center.
    /// Relevant for iris, ripple, swirl, and zoom shaders.
    /// </summary>
    public SKPoint Center { get; set; } = new SKPoint(0.5f, 0.5f);

    private RangeAnimator _animator;
    private bool _midpointFired;

    /// <summary>
    /// Starts the hide→reveal animation on the given parent control.
    /// Safe to call multiple times — restarts from zero each time.
    /// </summary>
    public void Play(SkiaControl parent)
    {
        _animator?.Stop();
        _animator ??= new RangeAnimator(parent);

        Progress = 0.0;
        _midpointFired = false;

        _animator.Start(
            value =>
            {
                Progress = value;
                Update();

                if (!_midpointFired && value >= 0.5)
                {
                    _midpointFired = true;
                    Midpoint?.Invoke(this, EventArgs.Empty);
                }

                if (value >= 1.0)
                    Completed?.Invoke(this, EventArgs.Empty);
            },
            start:  0.0,
            end:    1.0,
            ms:     DurationMs,
            easing: Easing.Linear);

        Update();
    }

    /// <summary>
    /// Stops the animation without firing Completed.
    /// </summary>
    public void Stop() => _animator?.Stop();

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
