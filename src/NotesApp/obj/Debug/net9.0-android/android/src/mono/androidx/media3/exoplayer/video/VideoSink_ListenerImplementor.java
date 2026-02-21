package mono.androidx.media3.exoplayer.video;


public class VideoSink_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.exoplayer.video.VideoSink.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onError:(Landroidx/media3/exoplayer/video/VideoSink$VideoSinkException;)V:GetOnError_Landroidx_media3_exoplayer_video_VideoSink_VideoSinkException_Handler:AndroidX.Media3.ExoPlayer.Video.IVideoSinkListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onFirstFrameRendered:()V:GetOnFirstFrameRenderedHandler:AndroidX.Media3.ExoPlayer.Video.IVideoSinkListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onFrameAvailableForRendering:()V:GetOnFrameAvailableForRenderingHandler:AndroidX.Media3.ExoPlayer.Video.IVideoSinkListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onFrameDropped:()V:GetOnFrameDroppedHandler:AndroidX.Media3.ExoPlayer.Video.IVideoSinkListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"n_onVideoSizeChanged:(Landroidx/media3/common/VideoSize;)V:GetOnVideoSizeChanged_Landroidx_media3_common_VideoSize_Handler:AndroidX.Media3.ExoPlayer.Video.IVideoSinkListener, Xamarin.AndroidX.Media3.ExoPlayer\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.ExoPlayer.Video.IVideoSinkListenerImplementor, Xamarin.AndroidX.Media3.ExoPlayer", VideoSink_ListenerImplementor.class, __md_methods);
	}

	public VideoSink_ListenerImplementor ()
	{
		super ();
		if (getClass () == VideoSink_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.ExoPlayer.Video.IVideoSinkListenerImplementor, Xamarin.AndroidX.Media3.ExoPlayer", "", this, new java.lang.Object[] {  });
		}
	}

	public void onError (androidx.media3.exoplayer.video.VideoSink.VideoSinkException p0)
	{
		n_onError (p0);
	}

	private native void n_onError (androidx.media3.exoplayer.video.VideoSink.VideoSinkException p0);

	public void onFirstFrameRendered ()
	{
		n_onFirstFrameRendered ();
	}

	private native void n_onFirstFrameRendered ();

	public void onFrameAvailableForRendering ()
	{
		n_onFrameAvailableForRendering ();
	}

	private native void n_onFrameAvailableForRendering ();

	public void onFrameDropped ()
	{
		n_onFrameDropped ();
	}

	private native void n_onFrameDropped ();

	public void onVideoSizeChanged (androidx.media3.common.VideoSize p0)
	{
		n_onVideoSizeChanged (p0);
	}

	private native void n_onVideoSizeChanged (androidx.media3.common.VideoSize p0);

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
