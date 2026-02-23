namespace ShadersCamera.Views;


public class DebugLayer : SkiaLayer
{
    public override CachedObject CreateRenderingObject(DrawingContext context, SKRect recordingArea, CachedObject reuseSurfaceFrom,
        SkiaCacheType usingCacheType, Action<DrawingContext> action)
    {
        var created =  base.CreateRenderingObject(context, recordingArea, reuseSurfaceFrom, usingCacheType, action);

        return created;
    }

    public override void DrawRenderObject(DrawingContext context, CachedObject cache)
    {
        base.DrawRenderObject(context, cache);
    }

    public override void DrawDirectInternal(DrawingContext context, SKRect drawingRect)
    {
        base.DrawDirectInternal(context, drawingRect);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
    }

    protected override void Paint(DrawingContext ctx)
    {
        base.Paint(ctx);
    }
}

/// <summary>
/// Create popups that animate when shown or closed.
/// Animated entrance and exit shaders visual effects.
/// </summary>
public class AnimatedPopup : SkiaLayer
{
    protected AnimatedShaderEffect _entrance;
    protected AnimatedShaderEffect _exit;

    protected SkiaControl Animatable;
    private SkiaCacheType _animatableCache;

    public void SetAnimatable(SkiaControl layout)
    {
        _animatableCache = layout.UseCache;
        Animatable = layout;
    }

    public bool IsBusy { get; set; }

    private object lockNav = new();

    public virtual void Show()
    {


        lock (lockNav)
        {
            if (IsBusy)
            {
                return;
            }

            if (Animatable == null)
            {
                IsVisible = true;
                return;
            }


            IsBusy = true;

            Animatable.UseCache = SkiaCacheType.Image;

            if (_entrance == null)
            {
                _entrance = new AnimatedShaderEffect()
                {
                    UseBackground = PostRendererEffectUseBackgroud.Always,
                    ShaderSource = @"Shaders\appear_orb.sksl",
                    //DurationMs = 200,
                    //ShaderSource = @"Shaders\appear_supernova_clean.sksl",
                    //ShaderSource = @"Shaders\appear_vortex.sksl",
                    //ShaderSource = @"Shaders\appear_glitch.sksl",
                    DurationMs = 300
                };
                _entrance.Completed += (sender, args) =>
                {
                    Animatable.VisualEffects.Remove(_entrance);
                    Animatable.UseCache = _animatableCache;

                    IsBusy = false;
                };
            }

            if (!Animatable.VisualEffects.Contains(_entrance))
            {
                Animatable.VisualEffects.Add(_entrance);
            }

            IsVisible = true;
            _entrance.Play();
        }

    }

    public virtual void Close()
    {
        lock (lockNav)
        {
            if (IsBusy)
            {
                return;
            }

            if (Animatable == null)
            {
                IsVisible = false;
                return;
            }

            IsBusy = true;

            Animatable.UseCache = SkiaCacheType.Image;

            if (_exit == null)
            {
                _exit = new AnimatedShaderEffect()
                {
                    UseBackground = PostRendererEffectUseBackgroud.Always,
                    ShaderSource = @"Shaders\exit_orb.sksl",
                    //DurationMs = 200,
                    //ShaderSource = @"Shaders\exit_vortex.sksl",
                    //ShaderSource = @"Shaders\exit_supernova_clean.sksl",
                    //ShaderSource = @"Shaders\exit_glitch.sksl",
                    DurationMs = 250
                };
                _exit.Completed += (sender, args) =>
                {
                    IsVisible = false;
                    Animatable.VisualEffects.Remove(_exit);

                    IsBusy = false;
                };
            }

            if (!Animatable.VisualEffects.Contains(_exit))
            {
                Animatable.VisualEffects.Add(_exit);
                _exit.Play();
            }

        }

    }

}