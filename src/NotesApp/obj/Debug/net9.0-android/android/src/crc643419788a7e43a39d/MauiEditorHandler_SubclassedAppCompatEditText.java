package crc643419788a7e43a39d;


public class MauiEditorHandler_SubclassedAppCompatEditText
	extends androidx.appcompat.widget.AppCompatEditText
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onMeasure:(II)V:GetOnMeasure_IIHandler\n" +
			"n_onSelectionChanged:(II)V:GetOnSelectionChanged_IIHandler\n" +
			"n_onCreateInputConnection:(Landroid/view/inputmethod/EditorInfo;)Landroid/view/inputmethod/InputConnection;:GetOnCreateInputConnection_Landroid_view_inputmethod_EditorInfo_Handler\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Controls.MauiEditorHandler+SubclassedAppCompatEditText, DrawnUi.Maui", MauiEditorHandler_SubclassedAppCompatEditText.class, __md_methods);
	}

	public MauiEditorHandler_SubclassedAppCompatEditText (android.content.Context p0)
	{
		super (p0);
		if (getClass () == MauiEditorHandler_SubclassedAppCompatEditText.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Controls.MauiEditorHandler+SubclassedAppCompatEditText, DrawnUi.Maui", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
		}
	}

	public MauiEditorHandler_SubclassedAppCompatEditText (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == MauiEditorHandler_SubclassedAppCompatEditText.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Controls.MauiEditorHandler+SubclassedAppCompatEditText, DrawnUi.Maui", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
		}
	}

	public MauiEditorHandler_SubclassedAppCompatEditText (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == MauiEditorHandler_SubclassedAppCompatEditText.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Controls.MauiEditorHandler+SubclassedAppCompatEditText, DrawnUi.Maui", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, System.Private.CoreLib", this, new java.lang.Object[] { p0, p1, p2 });
		}
	}

	public void onMeasure (int p0, int p1)
	{
		n_onMeasure (p0, p1);
	}

	private native void n_onMeasure (int p0, int p1);

	public void onSelectionChanged (int p0, int p1)
	{
		n_onSelectionChanged (p0, p1);
	}

	private native void n_onSelectionChanged (int p0, int p1);

	public android.view.inputmethod.InputConnection onCreateInputConnection (android.view.inputmethod.EditorInfo p0)
	{
		return n_onCreateInputConnection (p0);
	}

	private native android.view.inputmethod.InputConnection n_onCreateInputConnection (android.view.inputmethod.EditorInfo p0);

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
