package mono.androidx.media3.exoplayer.source;


public class ProgressiveMediaSource_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.exoplayer.source.ProgressiveMediaSource.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onSeekMap:(Landroidx/media3/exoplayer/source/MediaSource;Landroidx/media3/extractor/SeekMap;)V:GetOnSeekMap_Landroidx_media3_exoplayer_source_MediaSource_Landroidx_media3_extractor_SeekMap_Handler:AndroidX.Media3.ExoPlayer.Source.ProgressiveMediaSource/IListenerInvoker, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.ExoPlayer.Source.ProgressiveMediaSource+IListenerImplementor, Xamarin.AndroidX.Media3.ExoPlayer", ProgressiveMediaSource_ListenerImplementor.class, __md_methods);
	}

	public ProgressiveMediaSource_ListenerImplementor ()
	{
		super ();
		if (getClass () == ProgressiveMediaSource_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.ExoPlayer.Source.ProgressiveMediaSource+IListenerImplementor, Xamarin.AndroidX.Media3.ExoPlayer", "", this, new java.lang.Object[] {  });
		}
	}

	public void onSeekMap (androidx.media3.exoplayer.source.MediaSource p0, androidx.media3.extractor.SeekMap p1)
	{
		n_onSeekMap (p0, p1);
	}

	private native void n_onSeekMap (androidx.media3.exoplayer.source.MediaSource p0, androidx.media3.extractor.SeekMap p1);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
