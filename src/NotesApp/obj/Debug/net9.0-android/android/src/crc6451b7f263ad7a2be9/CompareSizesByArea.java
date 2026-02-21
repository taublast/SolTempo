package crc6451b7f263ad7a2be9;


public class CompareSizesByArea
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		java.util.Comparator
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_compare:(Ljava/lang/Object;Ljava/lang/Object;)I:GetCompare_Ljava_lang_Object_Ljava_lang_Object_Handler:Java.Util.IComparatorInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_equals:(Ljava/lang/Object;)Z:GetEquals_Ljava_lang_Object_Handler:Java.Util.IComparatorInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_reversed:()Ljava/util/Comparator;:GetReversedHandler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_thenComparing:(Ljava/util/Comparator;)Ljava/util/Comparator;:GetThenComparing_Ljava_util_Comparator_Handler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_thenComparing:(Ljava/util/function/Function;Ljava/util/Comparator;)Ljava/util/Comparator;:GetThenComparing_Ljava_util_function_Function_Ljava_util_Comparator_Handler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_thenComparing:(Ljava/util/function/Function;)Ljava/util/Comparator;:GetThenComparing_Ljava_util_function_Function_Handler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_thenComparingDouble:(Ljava/util/function/ToDoubleFunction;)Ljava/util/Comparator;:GetThenComparingDouble_Ljava_util_function_ToDoubleFunction_Handler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_thenComparingInt:(Ljava/util/function/ToIntFunction;)Ljava/util/Comparator;:GetThenComparingInt_Ljava_util_function_ToIntFunction_Handler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"n_thenComparingLong:(Ljava/util/function/ToLongFunction;)Ljava/util/Comparator;:GetThenComparingLong_Ljava_util_function_ToLongFunction_Handler:Java.Util.IComparator, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Camera.CompareSizesByArea, DrawnUi.Maui.Camera", CompareSizesByArea.class, __md_methods);
	}

	public CompareSizesByArea ()
	{
		super ();
		if (getClass () == CompareSizesByArea.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.CompareSizesByArea, DrawnUi.Maui.Camera", "", this, new java.lang.Object[] {  });
		}
	}

	public int compare (java.lang.Object p0, java.lang.Object p1)
	{
		return n_compare (p0, p1);
	}

	private native int n_compare (java.lang.Object p0, java.lang.Object p1);

	public boolean equals (java.lang.Object p0)
	{
		return n_equals (p0);
	}

	private native boolean n_equals (java.lang.Object p0);

	public java.util.Comparator reversed ()
	{
		return n_reversed ();
	}

	private native java.util.Comparator n_reversed ();

	public java.util.Comparator thenComparing (java.util.Comparator p0)
	{
		return n_thenComparing (p0);
	}

	private native java.util.Comparator n_thenComparing (java.util.Comparator p0);

	public java.util.Comparator thenComparing (java.util.function.Function p0, java.util.Comparator p1)
	{
		return n_thenComparing (p0, p1);
	}

	private native java.util.Comparator n_thenComparing (java.util.function.Function p0, java.util.Comparator p1);

	public java.util.Comparator thenComparing (java.util.function.Function p0)
	{
		return n_thenComparing (p0);
	}

	private native java.util.Comparator n_thenComparing (java.util.function.Function p0);

	public java.util.Comparator thenComparingDouble (java.util.function.ToDoubleFunction p0)
	{
		return n_thenComparingDouble (p0);
	}

	private native java.util.Comparator n_thenComparingDouble (java.util.function.ToDoubleFunction p0);

	public java.util.Comparator thenComparingInt (java.util.function.ToIntFunction p0)
	{
		return n_thenComparingInt (p0);
	}

	private native java.util.Comparator n_thenComparingInt (java.util.function.ToIntFunction p0);

	public java.util.Comparator thenComparingLong (java.util.function.ToLongFunction p0)
	{
		return n_thenComparingLong (p0);
	}

	private native java.util.Comparator n_thenComparingLong (java.util.function.ToLongFunction p0);

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
