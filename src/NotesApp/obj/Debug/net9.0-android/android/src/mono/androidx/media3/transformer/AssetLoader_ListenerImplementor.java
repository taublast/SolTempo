package mono.androidx.media3.transformer;


public class AssetLoader_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.transformer.AssetLoader.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onDurationUs:(J)V:GetOnDurationUs_JHandler:AndroidX.Media3.Transformer.IAssetLoaderListenerInvoker, Xamarin.AndroidX.Media3.Transformer\n" +
			"n_onError:(Landroidx/media3/transformer/ExportException;)V:GetOnError_Landroidx_media3_transformer_ExportException_Handler:AndroidX.Media3.Transformer.IAssetLoaderListenerInvoker, Xamarin.AndroidX.Media3.Transformer\n" +
			"n_onOutputFormat:(Landroidx/media3/common/Format;)Landroidx/media3/transformer/SampleConsumer;:GetOnOutputFormat_Landroidx_media3_common_Format_Handler:AndroidX.Media3.Transformer.IAssetLoaderListenerInvoker, Xamarin.AndroidX.Media3.Transformer\n" +
			"n_onTrackAdded:(Landroidx/media3/common/Format;I)Z:GetOnTrackAdded_Landroidx_media3_common_Format_IHandler:AndroidX.Media3.Transformer.IAssetLoaderListenerInvoker, Xamarin.AndroidX.Media3.Transformer\n" +
			"n_onTrackCount:(I)V:GetOnTrackCount_IHandler:AndroidX.Media3.Transformer.IAssetLoaderListenerInvoker, Xamarin.AndroidX.Media3.Transformer\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Transformer.IAssetLoaderListenerImplementor, Xamarin.AndroidX.Media3.Transformer", AssetLoader_ListenerImplementor.class, __md_methods);
	}

	public AssetLoader_ListenerImplementor ()
	{
		super ();
		if (getClass () == AssetLoader_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Transformer.IAssetLoaderListenerImplementor, Xamarin.AndroidX.Media3.Transformer", "", this, new java.lang.Object[] {  });
		}
	}

	public void onDurationUs (long p0)
	{
		n_onDurationUs (p0);
	}

	private native void n_onDurationUs (long p0);

	public void onError (androidx.media3.transformer.ExportException p0)
	{
		n_onError (p0);
	}

	private native void n_onError (androidx.media3.transformer.ExportException p0);

	public androidx.media3.transformer.SampleConsumer onOutputFormat (androidx.media3.common.Format p0)
	{
		return n_onOutputFormat (p0);
	}

	private native androidx.media3.transformer.SampleConsumer n_onOutputFormat (androidx.media3.common.Format p0);

	public boolean onTrackAdded (androidx.media3.common.Format p0, int p1)
	{
		return n_onTrackAdded (p0, p1);
	}

	private native boolean n_onTrackAdded (androidx.media3.common.Format p0, int p1);

	public void onTrackCount (int p0)
	{
		n_onTrackCount (p0);
	}

	private native void n_onTrackCount (int p0);

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
