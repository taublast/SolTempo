using AppoMobi.Maui.Gestures;
using SolTempo;

namespace SolTempo.UI;

public class TintedShape : SkiaShape
{
    public TintedShape()
    {
        MapProperties();
    }

    bool _isTapped = false;

    public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
    {
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

}