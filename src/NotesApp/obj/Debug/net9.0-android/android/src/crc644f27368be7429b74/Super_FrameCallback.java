package crc644f27368be7429b74;


public class Super_FrameCallback
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.Choreographer.FrameCallback
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_doFrame:(J)V:GetDoFrame_JHandler:Android.Views.Choreographer/IFrameCallbackInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Draw.Super+FrameCallback, DrawnUi.Maui", Super_FrameCallback.class, __md_methods);
	}

	public Super_FrameCallback ()
	{
		super ();
		if (getClass () == Super_FrameCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Draw.Super+FrameCallback, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public void doFrame (long p0)
	{
		n_doFrame (p0);
	}

	private native void n_doFrame (long p0);

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
