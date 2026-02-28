namespace SolTempo.Effects;

/// <summary>
/// Plays the shader effect animation when the parent control appears (becomes visible).
/// We don't use it in the end. Code kept for possible future use.
/// </summary>
public class PlayOnAppearingShaderEffect : AnimatedShaderEffect
{
    public PlayOnAppearingShaderEffect()
    {
        UseBackground = PostRendererEffectUseBackgroud.Once;
    }

    public override void Attach(SkiaControl parent)
    {
        base.Attach(parent);

        parent.UseCache = SkiaCacheType.Image; //to be able to animate correctly

        parent.ParentVisibilityChanged += ParentOnVisibilityChanged;  
    }

    public override void Dettach()
    {
        if (Parent != null)
        {
            Parent.ParentVisibilityChanged -= ParentOnVisibilityChanged;
        }

        base.Dettach();
    }

    private void ParentOnVisibilityChanged(object sender, bool state)
    {
        if (state)
        {
            Play();
        }
    }

    private bool _started;
}

public class PlayOnDisappearingShaderEffect : AnimatedShaderEffect
{
    public PlayOnDisappearingShaderEffect()
    {
        UseBackground = PostRendererEffectUseBackgroud.Once;
    }

    public override void Attach(SkiaControl parent)
    {
        base.Attach(parent);

        parent.UseCache = SkiaCacheType.Image; //to be able to animate correctly

        parent.ParentVisibilityChanged += ParentOnVisibilityChanged;
    }

    public override void Dettach()
    {
        if (Parent != null)
        {
            Parent.ParentVisibilityChanged -= ParentOnVisibilityChanged;
        }

        base.Dettach();
    }

    private void ParentOnVisibilityChanged(object sender, bool state)
    {
        if (!state)
        {
            Play();
        }
    }

    /// <summary>
    /// Normalized center position of the effect (0.0–1.0 in each axis).
    /// </summary>
    public SKPoint Center { get; set; } = new SKPoint(0.5f, 0.5f);

    private bool _started;

    protected override SKRuntimeEffectUniforms CreateUniforms(SKRect destination)
    {
        var uniforms = base.CreateUniforms(destination);

        uniforms["iCenter"] = new float[] { Center.X, Center.Y };
        return uniforms;
    }
}
