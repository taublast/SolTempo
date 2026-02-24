using SolTempo;

namespace SolTempo.UI;

public class TintedShape : SkiaShape
{
    public TintedShape()
    {
        MapProperties();
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