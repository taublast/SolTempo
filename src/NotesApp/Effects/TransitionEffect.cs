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
public class TransitionEffect : AnimatedShaderEffect
{
    /// <summary>
    /// Normalized center position of the effect (0.0–1.0 in each axis).
    /// Default (0.5, 0.44) puts the center slightly above mid-screen.
    /// Change this to move where radial effects originate from.
    /// </summary>
    public SKPoint Center { get; set; } = new SKPoint(0.5f, 0.44f);

    /// <summary>
    /// Fired when progress reaches 0.5 — the image is fully hidden.
    /// Swap your UI content here; the reveal phase will show the new state.
    /// </summary>
    public event EventHandler Midpoint;

    private bool _midpointFired;

    /// <summary>
    /// Starts the hide→reveal animation on the given parent control.
    /// Safe to call multiple times — restarts from zero each time.
    /// </summary>
    public override void Play(SkiaControl parent)
    {
        Animator?.Stop();
        Animator ??= new RangeAnimator(parent);

        Progress = 0.0;
        _midpointFired = false;

        Animator.Start(
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
                    OnCompleted();
            },
            start:  0.0,
            end:    1.0,
            ms:     DurationMs,
            easing: Easing.Linear);

        Update();
    }

    protected override SKRuntimeEffectUniforms CreateUniforms(SKRect destination)
    {
        var uniforms = base.CreateUniforms(destination);

        uniforms["iCenter"] = new float[] { Center.X, Center.Y };

        return uniforms;
    }

}
