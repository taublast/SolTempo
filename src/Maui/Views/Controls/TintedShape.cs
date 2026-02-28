using AppoMobi.Maui.Gestures;
 

namespace SolTempo.UI;

/// <summary>
/// Shape with a tint color and hover/tap effects.
/// </summary>
public class TintedShape : SkiaShape
{
    public TintedShape()
    {
        MapProperties();
    }


    bool _isTapped = false;
    private SkiaGradient _gradient;
    private OuterGlowEffect _hoverEffect;

    public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
    {
        CheckHovered(args);

        if (args.Type == TouchActionResult.Down && !_isTapped)
        {
            _isTapped = true;
            var color = this.TintColor;
            TintColor = TintColor.Darken(0.33f);
            Task.Run(async () =>
            {
                await Task.Delay(250);
                TintColor = color;
                _isTapped = false;
            });
        }

        return base.ProcessGestures(args, apply);
    }

    public static readonly BindableProperty TintColorProperty = BindableProperty.Create(
        nameof(TintColor), typeof(Color), typeof(TintedShape),
        Colors.Orange, propertyChanged: OnLookChanged);

    public Color TintColor
    {
        get => (Color)GetValue(TintColorProperty);
        set => SetValue(TintColorProperty, value);
    }

    private static void OnLookChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TintedShape control)
        {
            control.MapProperties();
        }
    }

    private void MapProperties()
    {
        if (_gradient == null)
        {
            _gradient = new SkiaGradient()
            {
                Type = GradientType.Linear,
                Colors = new List<Color>()
                {
                    TintColor.Lighten(0.6f),
                    TintColor,
                    TintColor.Darken(0.6f)
                },
                ColorPositions = new List<double>()
                {
                    0.0,
                    0.2,
                    1.0,
                }
            };
        }

        FillGradient = new SkiaGradient()
        {
            Type = GradientType.Linear,
            Colors = new List<Color>()
            {
                TintColor.Lighten(0.6f),
                TintColor,
                TintColor.Darken(0.6f)
            },
            ColorPositions = new List<double>()
            {
                0.0,
                0.2,
                1.0,
            }
        };
    }


    public override bool OnHover(bool state)
    {
        if (_hoverEffect == null)
        {
            _hoverEffect = new OuterGlowEffect()
            {

            };
        }

        if (state)
        {
            this.VisualEffects.Add(_hoverEffect);
        }
        else
        {
            if (VisualEffects.Contains(_hoverEffect))
            {
                this.VisualEffects.Remove(_hoverEffect);
            }
        }

        return base.OnHover(state);
    }

    public override void OnWillDisposeWithChildren()
    {
        base.OnWillDisposeWithChildren();

        _gradient = null;

        _hoverEffect?.Dispose();
        _hoverEffect = null;
    }
}