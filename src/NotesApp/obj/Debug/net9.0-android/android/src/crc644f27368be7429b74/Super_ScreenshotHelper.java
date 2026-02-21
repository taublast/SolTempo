package crc644f27368be7429b74;


public class Super_ScreenshotHelper
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.PixelCopy.OnPixelCopyFinishedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPixelCopyFinished:(I)V:GetOnPixelCopyFinished_IHandler:Android.Views.PixelCopy/IOnPixelCopyFinishedListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Draw.Super+ScreenshotHelper, DrawnUi.Maui", Super_ScreenshotHelper.class, __md_methods);
	}

	public Super_ScreenshotHelper ()
	{
		super ();
		if (getClass () == Super_ScreenshotHelper.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Draw.Super+ScreenshotHelper, DrawnUi.Maui", "", this, new java.lang.Object[] {  });
		}
	}

	public Super_ScreenshotHelper (android.view.View p0, android.app.Activity p1)
	{
		super ();
		if (getClass () == Super_ScreenshotHelper.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Draw.Super+ScreenshotHelper, DrawnUi.Maui", "Android.Views.View, Mono.Android:Android.App.Activity, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public void onPixelCopyFinished (int p0)
	{
		n_onPixelCopyFinished (p0);
	}

	private native void n_onPixelCopyFinished (int p0);

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
