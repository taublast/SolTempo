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
public class CelebrationEffect : AnimatedShaderEffect
{
    public CelebrationEffect()
    {
        ShaderSource = @"Shaders\celebrate_starburst.sksl";
        UseBackground = PostRendererEffectUseBackgroud.Never;
        BlendMode = SKBlendMode.Plus; // additive — confetti/sparks glow on top of background
    }

    /// <summary>
    /// Normalized center position of the effect (0.0–1.0 in each axis).
    /// Default (0.5, 0.44) puts the center slightly above mid-screen.
    /// Change this to move where radial effects originate from.
    /// </summary>
    public SKPoint Center { get; set; } = new SKPoint(0.5f, 0.44f);

    protected override SKRuntimeEffectUniforms CreateUniforms(SKRect destination)
    {
        var uniforms = base.CreateUniforms(destination);
 
        uniforms["iCenter"] = new float[] { Center.X, Center.Y };

        return uniforms;
    }
}