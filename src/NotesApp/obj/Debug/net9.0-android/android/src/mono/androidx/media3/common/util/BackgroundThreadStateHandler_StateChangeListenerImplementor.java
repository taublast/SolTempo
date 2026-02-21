package mono.androidx.media3.common.util;


public class BackgroundThreadStateHandler_StateChangeListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		androidx.media3.common.util.BackgroundThreadStateHandler.StateChangeListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onStateChanged:(Ljava/lang/Object;Ljava/lang/Object;)V:GetOnStateChanged_Ljava_lang_Object_Ljava_lang_Object_Handler:AndroidX.Media3.Common.Util.BackgroundThreadStateHandler/IStateChangeListenerInvoker, Xamarin.AndroidX.Media3.Common\n" +
			"";
		mono.android.Runtime.register ("AndroidX.Media3.Common.Util.BackgroundThreadStateHandler+IStateChangeListenerImplementor, Xamarin.AndroidX.Media3.Common", BackgroundThreadStateHandler_StateChangeListenerImplementor.class, __md_methods);
	}

	public BackgroundThreadStateHandler_StateChangeListenerImplementor ()
	{
		super ();
		if (getClass () == BackgroundThreadStateHandler_StateChangeListenerImplementor.class) {
			mono.android.TypeManager.Activate ("AndroidX.Media3.Common.Util.BackgroundThreadStateHandler+IStateChangeListenerImplementor, Xamarin.AndroidX.Media3.Common", "", this, new java.lang.Object[] {  });
		}
	}

	public void onStateChanged (java.lang.Object p0, java.lang.Object p1)
	{
		n_onStateChanged (p0, p1);
	}

	private native void n_onStateChanged (java.lang.Object p0, java.lang.Object p1);

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
