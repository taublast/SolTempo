package mono.androidx.media3.effect;


public class GlTextureProducer_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.effect.GlTextureProducer.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onTextureRendered:(Landroidx/media3/effect/GlTextureProducer;Landroidx/media3/common/GlTextureInfo;JJ)V:GetOnTextureRendered_Landroidx_media3_effect_GlTextureProducer_Landroidx_media3_common_GlTextureInfo_JJHandler:AndroidX.Media3.Effect.IGlTextureProducerListenerInvoker, Xamarin.AndroidX.Media3.Effect\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Effect.IGlTextureProducerListenerImplementor, Xamarin.AndroidX.Media3.Effect", GlTextureProducer_ListenerImplementor.class, __md_methods);
	}

	public GlTextureProducer_ListenerImplementor ()
	{
		super ();
		if (getClass () == GlTextureProducer_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Effect.IGlTextureProducerListenerImplementor, Xamarin.AndroidX.Media3.Effect", "", this, new java.lang.Object[] {  });
		}
	}

	public void onTextureRendered (androidx.media3.effect.GlTextureProducer p0, androidx.media3.common.GlTextureInfo p1, long p2, long p3)
	{
		n_onTextureRendered (p0, p1, p2, p3);
	}

	private native void n_onTextureRendered (androidx.media3.effect.GlTextureProducer p0, androidx.media3.common.GlTextureInfo p1, long p2, long p3);

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
