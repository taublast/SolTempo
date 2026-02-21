package mono.androidx.media3.effect;


public class GlShaderProgram_OutputListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.effect.GlShaderProgram.OutputListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCurrentOutputStreamEnded:()V:GetOnCurrentOutputStreamEndedHandler:AndroidX.Media3.Effect.IGlShaderProgramOutputListener, Xamarin.AndroidX.Media3.Effect\n" +
			"n_onOutputFrameAvailable:(Landroidx/media3/common/GlTextureInfo;J)V:GetOnOutputFrameAvailable_Landroidx_media3_common_GlTextureInfo_JHandler:AndroidX.Media3.Effect.IGlShaderProgramOutputListener, Xamarin.AndroidX.Media3.Effect\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Effect.IGlShaderProgramOutputListenerImplementor, Xamarin.AndroidX.Media3.Effect", GlShaderProgram_OutputListenerImplementor.class, __md_methods);
	}

	public GlShaderProgram_OutputListenerImplementor ()
	{
		super ();
		if (getClass () == GlShaderProgram_OutputListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Effect.IGlShaderProgramOutputListenerImplementor, Xamarin.AndroidX.Media3.Effect", "", this, new java.lang.Object[] {  });
		}
	}

	public void onCurrentOutputStreamEnded ()
	{
		n_onCurrentOutputStreamEnded ();
	}

	private native void n_onCurrentOutputStreamEnded ();

	public void onOutputFrameAvailable (androidx.media3.common.GlTextureInfo p0, long p1)
	{
		n_onOutputFrameAvailable (p0, p1);
	}

	private native void n_onOutputFrameAvailable (androidx.media3.common.GlTextureInfo p0, long p1);

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
