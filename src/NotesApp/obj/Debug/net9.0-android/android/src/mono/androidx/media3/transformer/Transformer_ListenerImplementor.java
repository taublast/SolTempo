package mono.androidx.media3.transformer;


public class Transformer_ListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.transformer.Transformer.Listener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCompleted:(Landroidx/media3/transformer/Composition;Landroidx/media3/transformer/ExportResult;)V:GetOnCompleted_Landroidx_media3_transformer_Composition_Landroidx_media3_transformer_ExportResult_Handler:AndroidX.Media3.Transformer.Transformer/IListener, Xamarin.AndroidX.Media3.Transformer\n" +
			"n_onError:(Landroidx/media3/transformer/Composition;Landroidx/media3/transformer/ExportResult;Landroidx/media3/transformer/ExportException;)V:GetOnError_Landroidx_media3_transformer_Composition_Landroidx_media3_transformer_ExportResult_Landroidx_media3_transformer_ExportException_Handler:AndroidX.Media3.Transformer.Transformer/IListener, Xamarin.AndroidX.Media3.Transformer\n" +
			"n_onFallbackApplied:(Landroidx/media3/transformer/Composition;Landroidx/media3/transformer/TransformationRequest;Landroidx/media3/transformer/TransformationRequest;)V:GetOnFallbackApplied_Landroidx_media3_transformer_Composition_Landroidx_media3_transformer_TransformationRequest_Landroidx_media3_transformer_TransformationRequest_Handler:AndroidX.Media3.Transformer.Transformer/IListener, Xamarin.AndroidX.Media3.Transformer\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Transformer.Transformer+IListenerImplementor, Xamarin.AndroidX.Media3.Transformer", Transformer_ListenerImplementor.class, __md_methods);
	}

	public Transformer_ListenerImplementor ()
	{
		super ();
		if (getClass () == Transformer_ListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Transformer.Transformer+IListenerImplementor, Xamarin.AndroidX.Media3.Transformer", "", this, new java.lang.Object[] {  });
		}
	}

	public void onCompleted (androidx.media3.transformer.Composition p0, androidx.media3.transformer.ExportResult p1)
	{
		n_onCompleted (p0, p1);
	}

	private native void n_onCompleted (androidx.media3.transformer.Composition p0, androidx.media3.transformer.ExportResult p1);

	public void onError (androidx.media3.transformer.Composition p0, androidx.media3.transformer.ExportResult p1, androidx.media3.transformer.ExportException p2)
	{
		n_onError (p0, p1, p2);
	}

	private native void n_onError (androidx.media3.transformer.Composition p0, androidx.media3.transformer.ExportResult p1, androidx.media3.transformer.ExportException p2);

	public void onFallbackApplied (androidx.media3.transformer.Composition p0, androidx.media3.transformer.TransformationRequest p1, androidx.media3.transformer.TransformationRequest p2)
	{
		n_onFallbackApplied (p0, p1, p2);
	}

	private native void n_onFallbackApplied (androidx.media3.transformer.Composition p0, androidx.media3.transformer.TransformationRequest p1, androidx.media3.transformer.TransformationRequest p2);

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
