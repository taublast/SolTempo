namespace SolTempo.UI;

/// <summary>
/// Indended to preload all images at startup to avoid lag spike when switching later
/// We dont use it in the end, because we have a single background image,
/// but it can be useful if you have a lot of images to switch between and want to avoid any lag on first display.
/// </summary>
public class MutliImages : SkiaLayer
{
    private SkiaImage _imageNotes;
    private SkiaImage _imageBpm;
    private bool initialized;

    public void ToggleImage(int index)
    {
        return;

        if (!initialized)
        {
            //at startup we do a trick, paint ALL images to create cache,
            //just put the selecte one on top
            switch (index)
            {
                case 1:
                    _imageBpm.ZIndex = 10;
                    break;
                case 0:
                default:
                    _imageNotes.ZIndex = 10;
                    break;
            }

            initialized = true;
            return;
        }

        //we can now play fast switching visibility for already cached images
        switch (index)
        {
        case 1:
        _imageBpm.IsVisible = true;
        _imageNotes.IsVisible = false;
        break;
        case 0:
        default:
        _imageNotes.IsVisible = true;
        _imageBpm.IsVisible = false;
        break;
        }

    }


    public MutliImages()
    {
        UseCache = SkiaCacheType.GPU;
        VerticalOptions = LayoutOptions.Fill;

        Children = new List<SkiaControl>()
        {
            //new SkiaImage()
            //    {
            //        UseCache = SkiaCacheType.Operations,
            //        Source = @"Images\musguitar.jpg",
            //        Aspect = TransformAspect.AspectCover,
            //    }.Fill()
            //    .Assign(out _imageBpm),

            new SkiaImage()
                {
                    // https://unsplash.com/photos/music-room-with-lights-turned-on-gUK3lA3K7Yo
                    // by https://unsplash.com/@john_matychuk
                    Source = @"Images\musicback.jpg",
                    Aspect = TransformAspect.AspectCover,
                }.Fill()
                .Assign(out _imageNotes),
        };

    }

}
