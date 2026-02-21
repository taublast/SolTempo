package mono.androidx.media3.effect;


public class GlShaderProgram_InputListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.effect.GlShaderProgram.InputListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFlush:()V:GetOnFlushHandler:AndroidX.Media3.Effect.IGlShaderProgramInputListener, Xamarin.AndroidX.Media3.Effect\n" +
			"n_onInputFrameProcessed:(Landroidx/media3/common/GlTextureInfo;)V:GetOnInputFrameProcessed_Landroidx_media3_common_GlTextureInfo_Handler:AndroidX.Media3.Effect.IGlShaderProgramInputListener, Xamarin.AndroidX.Media3.Effect\n" +
			"n_onReadyToAcceptInputFrame:()V:GetOnReadyToAcceptInputFrameHandler:AndroidX.Media3.Effect.IGlShaderProgramInputListener, Xamarin.AndroidX.Media3.Effect\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Effect.IGlShaderProgramInputListenerImplementor, Xamarin.AndroidX.Media3.Effect", GlShaderProgram_InputListenerImplementor.class, __md_methods);
	}

	public GlShaderProgram_InputListenerImplementor ()
	{
		super ();
		if (getClass () == GlShaderProgram_InputListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Effect.IGlShaderProgramInputListenerImplementor, Xamarin.AndroidX.Media3.Effect", "", this, new java.lang.Object[] {  });
		}
	}

	public void onFlush ()
	{
		n_onFlush ();
	}

	private native void n_onFlush ();

	public void onInputFrameProcessed (androidx.media3.common.GlTextureInfo p0)
	{
		n_onInputFrameProcessed (p0);
	}

	private native void n_onInputFrameProcessed (androidx.media3.common.GlTextureInfo p0);

	public void onReadyToAcceptInputFrame ()
	{
		n_onReadyToAcceptInputFrame ();
	}

	private native void n_onReadyToAcceptInputFrame ();

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
