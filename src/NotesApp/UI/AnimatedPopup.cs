namespace ShadersCamera.Views;

/// <summary>
/// 
/// </summary>
public class AnimatedPopup : SkiaLayer
{
    protected AnimatedShaderEffect _entrance;
    protected AnimatedShaderEffect _exit;

    protected SkiaControl Animatable;   

    public void SetAnimatable(SkiaControl layout)
    {
        Animatable = layout;
    }

    public virtual void Show()
    {
        if (Animatable == null)
        {
            IsVisible = true;
            return;
        }

        if (_entrance == null)
        {
            _entrance = new AnimatedShaderEffect()
            {
                UseBackground = PostRendererEffectUseBackgroud.Always,
                //ShaderSource = @"Shaders\appear_orb.sksl",
                //DurationMs = 200,
                ShaderSource = @"Shaders\appear_supernova.sksl",
                //ShaderSource = @"Shaders\appear_vortex.sksl",
                //ShaderSource = @"Shaders\appear_glitch.sksl",
                DurationMs = 300
            };
            _entrance.Completed += (sender, args) =>
            {
                Animatable.VisualEffects.Remove(_entrance);
            };
        }

        if (!Animatable.VisualEffects.Contains(_entrance))
        {
            Animatable.VisualEffects.Add(_entrance);
        }

        IsVisible = true;
        _entrance.Play();

    }

    public virtual void Close()
    {
        if (Animatable == null)
        {
            IsVisible = false;
            return;
        }

        if (_exit == null)
        {
            _exit = new AnimatedShaderEffect()
            {
                UseBackground = PostRendererEffectUseBackgroud.Once,
                //ShaderSource = @"Shaders\exit_orb.sksl",
                //DurationMs = 200,
                //ShaderSource = @"Shaders\exit_vortex.sksl",
                ShaderSource = @"Shaders\exit_supernova.sksl",
                //ShaderSource = @"Shaders\exit_glitch.sksl",
                DurationMs = 300
            };
            _exit.Completed += (sender, args) =>
            {
                IsVisible = false;
                Animatable.VisualEffects.Remove(_exit);
            };
        }

        if (!Animatable.VisualEffects.Contains(_exit))
        {
            Animatable.VisualEffects.Add(_exit);
            _exit.Play();
        }

    }

}