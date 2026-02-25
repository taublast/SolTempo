namespace SolTempo.Effects;

/// <summary>
/// Fullscreen celebration shader effect driven by a RangeAnimator (progress 0→1).
/// </summary>
public class AchievementEffect : AnimatedShaderEffect
{
    public AchievementEffect()
    {
        UseBackground = PostRendererEffectUseBackgroud.Once;
        BlendMode = SKBlendMode.Plus;
        ShaderSource = @"Shaders\celebrate_starburst_fast.sksl";
        Center = new SKPoint(0.5f, 0.33f);
        DurationMs = 5000;
    }

    /// <summary>
    /// Normalized center position of the effect (0.0–1.0 in each axis).
    /// Default (0.5, 0.44) puts the center slightly above mid-screen.
    /// Change this to move where radial effects originate from.
    /// </summary>
    public SKPoint Center { get; set; } = new SKPoint(0.5f, 0.5f);

    protected override SKRuntimeEffectUniforms CreateUniforms(SKRect destination)
    {
        var uniforms = base.CreateUniforms(destination);
 
        uniforms["iCenter"] = new float[] { Center.X, Center.Y };

        return uniforms;
    }
}