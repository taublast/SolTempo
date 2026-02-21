package crc643746c40ff623ff03;


public class SkiaGLTexture_InternalRenderer
	extends crc643746c40ff623ff03.RetainedSkiaGLTextureRenderer
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DrawnUi.SkiaGLTexture+InternalRenderer, DrawnUi.Maui", SkiaGLTexture_InternalRenderer.class, __md_methods);
	}

	public SkiaGLTexture_InternalRenderer ()
	{
		super ();
		if (getClass () == SkiaGLTexture_InternalRenderer.class) {
			mono.android.TypeManager.Activate ("DrawnUi.SkiaGLTexture+InternalRenderer, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
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
