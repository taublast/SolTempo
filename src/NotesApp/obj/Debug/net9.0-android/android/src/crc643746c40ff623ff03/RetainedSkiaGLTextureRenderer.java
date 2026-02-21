package crc643746c40ff623ff03;


public class RetainedSkiaGLTextureRenderer
	extends crc643746c40ff623ff03.SkiaGLTextureRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DrawnUi.RetainedSkiaGLTextureRenderer, DrawnUi.Maui", RetainedSkiaGLTextureRenderer.class, __md_methods);
	}

	public RetainedSkiaGLTextureRenderer ()
	{
		super ();
		if (getClass () == RetainedSkiaGLTextureRenderer.class) {
			mono.android.TypeManager.Activate ("DrawnUi.RetainedSkiaGLTextureRenderer, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
		}
	}

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
