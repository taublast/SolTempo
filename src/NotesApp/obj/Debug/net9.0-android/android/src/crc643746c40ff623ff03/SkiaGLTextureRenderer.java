package crc643746c40ff623ff03;


public abstract class SkiaGLTextureRenderer
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DrawnUi.SkiaGLTextureRenderer, DrawnUi.Maui", SkiaGLTextureRenderer.class, __md_methods);
	}

	public SkiaGLTextureRenderer ()
	{
		super ();
		if (getClass () == SkiaGLTextureRenderer.class) {
			mono.android.TypeManager.Activate ("DrawnUi.SkiaGLTextureRenderer, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
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
