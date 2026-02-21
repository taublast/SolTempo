package mono.androidx.media3.transformer;


public class DefaultDecoderFactory_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.transformer.DefaultDecoderFactory.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCodecInitialized:(Ljava/lang/String;Ljava/util/List;)V:GetOnCodecInitialized_Ljava_lang_String_Ljava_util_List_Handler:AndroidX.Media3.Transformer.DefaultDecoderFactory/IListenerInvoker, Xamarin.AndroidX.Media3.Transformer\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Transformer.DefaultDecoderFactory+IListenerImplementor, Xamarin.AndroidX.Media3.Transformer", DefaultDecoderFactory_ListenerImplementor.class, __md_methods);
	}

	public DefaultDecoderFactory_ListenerImplementor ()
	{
		super ();
		if (getClass () == DefaultDecoderFactory_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Transformer.DefaultDecoderFactory+IListenerImplementor, Xamarin.AndroidX.Media3.Transformer", "", this, new java.lang.Object[] {  });
		}
	}

	public void onCodecInitialized (java.lang.String p0, java.util.List p1)
	{
		n_onCodecInitialized (p0, p1);
	}

	private native void n_onCodecInitialized (java.lang.String p0, java.util.List p1);

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
