package mono.androidx.media3.effect;


public class VideoCompositor_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.effect.VideoCompositor.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onEnded:()V:GetOnEndedHandler:AndroidX.Media3.Effect.IVideoCompositorListenerInvoker, Xamarin.AndroidX.Media3.Effect\n" +
			"n_onError:(Landroidx/media3/common/VideoFrameProcessingException;)V:GetOnError_Landroidx_media3_common_VideoFrameProcessingException_Handler:AndroidX.Media3.Effect.IVideoCompositorListenerInvoker, Xamarin.AndroidX.Media3.Effect\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Effect.IVideoCompositorListenerImplementor, Xamarin.AndroidX.Media3.Effect", VideoCompositor_ListenerImplementor.class, __md_methods);
	}

	public VideoCompositor_ListenerImplementor ()
	{
		super ();
		if (getClass () == VideoCompositor_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Effect.IVideoCompositorListenerImplementor, Xamarin.AndroidX.Media3.Effect", "", this, new java.lang.Object[] {  });
		}
	}

	public void onEnded ()
	{
		n_onEnded ();
	}

	private native void n_onEnded ();

	public void onError (androidx.media3.common.VideoFrameProcessingException p0)
	{
		n_onError (p0);
	}

	private native void n_onError (androidx.media3.common.VideoFrameProcessingException p0);

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
