package crc644f27368be7429b74;


public class KeyboardManager_KeysListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.ViewTreeObserver.OnGlobalFocusChangeListener,
		android.view.View.OnKeyListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onGlobalFocusChanged:(Landroid/view/View;Landroid/view/View;)V:GetOnGlobalFocusChanged_Landroid_view_View_Landroid_view_View_Handler:Android.Views.ViewTreeObserver/IOnGlobalFocusChangeListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onKey:(Landroid/view/View;ILandroid/view/KeyEvent;)Z:GetOnKey_Landroid_view_View_ILandroid_view_KeyEvent_Handler:Android.Views.View/IOnKeyListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Draw.KeyboardManager+KeysListener, DrawnUi.Maui", KeyboardManager_KeysListener.class, __md_methods);
	}

	public KeyboardManager_KeysListener ()
	{
		super ();
		if (getClass () == KeyboardManager_KeysListener.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Draw.KeyboardManager+KeysListener, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public void onGlobalFocusChanged (android.view.View p0, android.view.View p1)
	{
		n_onGlobalFocusChanged (p0, p1);
	}

	private native void n_onGlobalFocusChanged (android.view.View p0, android.view.View p1);

	public boolean onKey (android.view.View p0, int p1, android.view.KeyEvent p2)
	{
		return n_onKey (p0, p1, p2);
	}

	private native boolean n_onKey (android.view.View p0, int p1, android.view.KeyEvent p2);

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
