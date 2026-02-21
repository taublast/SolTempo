package crc644965c3453d791bcd;


public class PlatformTouchEffect_TouchListener
	extends android.view.GestureDetector.SimpleOnGestureListener
	implements
		mono.android.IGCUserPeer,
		android.view.View.OnTouchListener,
		android.view.View.OnHoverListener,
		android.view.View.OnGenericMotionListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onTouch:(Landroid/view/View;Landroid/view/MotionEvent;)Z:GetOnTouch_Landroid_view_View_Landroid_view_MotionEvent_Handler:Android.Views.View/IOnTouchListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onHover:(Landroid/view/View;Landroid/view/MotionEvent;)Z:GetOnHover_Landroid_view_View_Landroid_view_MotionEvent_Handler:Android.Views.View/IOnHoverListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_onGenericMotion:(Landroid/view/View;Landroid/view/MotionEvent;)Z:GetOnGenericMotion_Landroid_view_View_Landroid_view_MotionEvent_Handler:Android.Views.View/IOnGenericMotionListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("AppoMobi.Maui.Gestures.PlatformTouchEffect+TouchListener, AppoMobi.Maui.Gestures", PlatformTouchEffect_TouchListener.class, __md_methods);
	}

	public PlatformTouchEffect_TouchListener ()
	{
		super ();
		if (getClass () == PlatformTouchEffect_TouchListener.class) {
			mono.android.TypeManager.Activate ("AppoMobi.Maui.Gestures.PlatformTouchEffect+TouchListener, AppoMobi.Maui.Gestures", "", this, new java.lang.Object[] {  });
		}
	}

	public boolean onTouch (android.view.View p0, android.view.MotionEvent p1)
	{
		return n_onTouch (p0, p1);
	}

	private native boolean n_onTouch (android.view.View p0, android.view.MotionEvent p1);

	public boolean onHover (android.view.View p0, android.view.MotionEvent p1)
	{
		return n_onHover (p0, p1);
	}

	private native boolean n_onHover (android.view.View p0, android.view.MotionEvent p1);

	public boolean onGenericMotion (android.view.View p0, android.view.MotionEvent p1)
	{
		return n_onGenericMotion (p0, p1);
	}

	private native boolean n_onGenericMotion (android.view.View p0, android.view.MotionEvent p1);

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
