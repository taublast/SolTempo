package crc643419788a7e43a39d;


public class KeyboardLayoutListener
	extends crc643419788a7e43a39d.GlobalLayoutListener_1
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("DrawnUi.Controls.KeyboardLayoutListener, DrawnUi.Maui", KeyboardLayoutListener.class, __md_methods);
	}

	public KeyboardLayoutListener ()
	{
		super ();
		if (getClass () == KeyboardLayoutListener.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Controls.KeyboardLayoutListener, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
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
