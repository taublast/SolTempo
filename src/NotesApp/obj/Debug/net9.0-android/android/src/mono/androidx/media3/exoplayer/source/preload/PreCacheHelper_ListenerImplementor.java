package mono.androidx.media3.exoplayer.source.preload;


public class PreCacheHelper_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.exoplayer.source.preload.PreCacheHelper.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onDownloadError:(Landroidx/media3/common/MediaItem;Ljava/io/IOException;)V:GetOnDownloadError_Landroidx_media3_common_MediaItem_Ljava_io_IOException_Handler:AndroidX.Media3.ExoPlayer.Source.Preload.PreCacheHelper/IListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onPreCacheProgress:(Landroidx/media3/common/MediaItem;JJF)V:GetOnPreCacheProgress_Landroidx_media3_common_MediaItem_JJFHandler:AndroidX.Media3.ExoPlayer.Source.Preload.PreCacheHelper/IListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onPrepareError:(Landroidx/media3/common/MediaItem;Ljava/io/IOException;)V:GetOnPrepareError_Landroidx_media3_common_MediaItem_Ljava_io_IOException_Handler:AndroidX.Media3.ExoPlayer.Source.Preload.PreCacheHelper/IListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onPrepared:(Landroidx/media3/common/MediaItem;Landroidx/media3/common/MediaItem;)V:GetOnPrepared_Landroidx_media3_common_MediaItem_Landroidx_media3_common_MediaItem_Handler:AndroidX.Media3.ExoPlayer.Source.Preload.PreCacheHelper/IListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.ExoPlayer.Source.Preload.PreCacheHelper+IListenerImplementor, Xamarin.AndroidX.Media3.ExoPlayer", PreCacheHelper_ListenerImplementor.class, __md_methods);
	}

	public PreCacheHelper_ListenerImplementor ()
	{
		super ();
		if (getClass () == PreCacheHelper_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.ExoPlayer.Source.Preload.PreCacheHelper+IListenerImplementor, Xamarin.AndroidX.Media3.ExoPlayer", "", this, new java.lang.Object[] {  });
		}
	}

	public void onDownloadError (androidx.media3.common.MediaItem p0, java.io.IOException p1)
	{
		n_onDownloadError (p0, p1);
	}

	private native void n_onDownloadError (androidx.media3.common.MediaItem p0, java.io.IOException p1);

	public void onPreCacheProgress (androidx.media3.common.MediaItem p0, long p1, long p2, float p3)
	{
		n_onPreCacheProgress (p0, p1, p2, p3);
	}

	private native void n_onPreCacheProgress (androidx.media3.common.MediaItem p0, long p1, long p2, float p3);

	public void onPrepareError (androidx.media3.common.MediaItem p0, java.io.IOException p1)
	{
		n_onPrepareError (p0, p1);
	}

	private native void n_onPrepareError (androidx.media3.common.MediaItem p0, java.io.IOException p1);

	public void onPrepared (androidx.media3.common.MediaItem p0, androidx.media3.common.MediaItem p1)
	{
		n_onPrepared (p0, p1);
	}

	private native void n_onPrepared (androidx.media3.common.MediaItem p0, androidx.media3.common.MediaItem p1);

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
