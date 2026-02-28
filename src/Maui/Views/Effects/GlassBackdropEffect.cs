namespace SolTempo.Effects;

/// <summary>
/// Glass backdrop effect with customizable corner radius.
/// Applies a liquid glass shader effect that respects rounded corners.
/// </summary>
public class GlassBackdropEffect : SkiaShaderEffect
{
    public GlassBackdropEffect()
    {
        ShaderSource = @"Shaders\glass.sksl";
    }

    private readonly float[] _uniformTint = new float[4];

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius),
        typeof(float),
        typeof(GlassBackdropEffect),
        0f,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty EmbossProperty = BindableProperty.Create(
        nameof(Emboss),
        typeof(float),
        typeof(GlassBackdropEffect),
        10.0f,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty DepthProperty = BindableProperty.Create(
        nameof(Depth),
        typeof(float),
        typeof(GlassBackdropEffect),
        1.0f,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty BlurStrengthProperty = BindableProperty.Create(
        nameof(BlurStrength),
        typeof(float),
        typeof(GlassBackdropEffect),
        1.0f,
        propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty OpacityProperty = BindableProperty.Create(
        nameof(Opacity),
        typeof(float),
        typeof(GlassBackdropEffect),
        0.75f,
        propertyChanged: OnPropertyChanged);


    public static readonly BindableProperty TintProperty = BindableProperty.Create(
        nameof(Tint),
        typeof(Color),
        typeof(GlassBackdropEffect),
        Colors.Transparent,
        propertyChanged: OnPropertyChanged);


    public static readonly BindableProperty EdgeOpacityProperty = BindableProperty.Create(
        nameof(EdgeOpacity),
        typeof(float),
        typeof(GlassBackdropEffect),
        0.5f,
        propertyChanged: OnPropertyChanged);

    /// <summary>
    /// Default is 0.55
    /// </summary>
    public float EdgeOpacity
    {
        get => (float)GetValue(EdgeOpacityProperty);
        set => SetValue(EdgeOpacityProperty, value);
    }

    public static readonly BindableProperty EdgeGlowProperty = BindableProperty.Create(
        nameof(EdgeGlow),
        typeof(float),
        typeof(GlassBackdropEffect),
        0.95f,
        propertyChanged: OnPropertyChanged);

    /// <summary>
    /// Default is 0.95
    /// </summary>
    public float EdgeGlow
    {
        get => (float)GetValue(EdgeGlowProperty);
        set => SetValue(EdgeGlowProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius in points (density-independent units).
    /// The shader will automatically convert this to pixels based on screen density.
    /// </summary>
    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the emboss/refraction intensity as a percentage of the image width.
    /// Controls both background distortion and border glow width together.
    /// Density-independent: same value produces the same look on all screen densities.
    /// Range: 0.0 (flat glass) to ~30.0 (very pronounced). Default: 10.0.
    /// </summary>
    public float Emboss
    {
        get => (float)GetValue(EmbossProperty);
        set => SetValue(EmbossProperty, value);
    }

    /// <summary>
    /// Gets or sets the 3D depth/emboss intensity of the glass effect.
    /// Controls the refraction strength for the curved appearance.
    /// Range: 0.0 (flat/no distortion) to 2.0+ (very pronounced).
    /// Default: 1.0.
    /// </summary>
    public float Depth
    {
        get => (float)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    /// <summary>
    /// Gets or sets the blur intensity multiplier for the frosted glass effect.
    /// Default: 1.0 (original appearance).
    /// </summary>
    public float BlurStrength
    {
        get => (float)GetValue(BlurStrengthProperty);
        set => SetValue(BlurStrengthProperty, value);
    }

    /// <summary>
    /// Gets or sets the overall glass panel opacity.
    /// Range: 0.0 (fully transparent) to 1.0 (fully opaque).
    /// Default: 0.75.
    /// </summary>
    public float Opacity
    {
        get => (float)GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }


    /// <summary>
    /// Gets or sets the glass tint color. The alpha channel controls tint strength:
    /// fully transparent = no tint, fully opaque = solid color overlay.
    /// Default: Transparent (no tint).
    /// </summary>
    public Color Tint
    {
        get => (Color)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is GlassBackdropEffect effect)
        {
            effect.Update();
        }
    }

    protected override SKRuntimeEffectUniforms CreateUniforms(SKRect destination)
    {
        var uniforms = base.CreateUniforms(destination);

        var scale = Parent?.RenderingScale ?? 1f;
        uniforms["iCornerRadius"] = CornerRadius * scale;
        uniforms["iEmboss"] = Emboss;

        uniforms["iDepth"] = Depth;
        uniforms["iBlurStrength"] = BlurStrength;
        uniforms["iOpacity"] = Opacity;
        uniforms["iEdgeOpacity"] = EdgeOpacity;
        uniforms["iEdgeGlow"] = EdgeGlow;

        var c = Tint;
        _uniformTint[0] = (float)c.Red; _uniformTint[1] = (float)c.Green;
        _uniformTint[2] = (float)c.Blue; _uniformTint[3] = (float)c.Alpha;
        uniforms["iTint"] = _uniformTint;

        return uniforms;
    }
}
