package crc6451b7f263ad7a2be9;


public class NativeCamera_StillPhotoCaptureFinishedCallback
	extends android.hardware.camera2.CameraCaptureSession.CaptureCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCaptureCompleted:(Landroid/hardware/camera2/CameraCaptureSession;Landroid/hardware/camera2/CaptureRequest;Landroid/hardware/camera2/TotalCaptureResult;)V:GetOnCaptureCompleted_Landroid_hardware_camera2_CameraCaptureSession_Landroid_hardware_camera2_CaptureRequest_Landroid_hardware_camera2_TotalCaptureResult_Handler\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Camera.NativeCamera+StillPhotoCaptureFinishedCallback, DrawnUi.Maui.Camera", NativeCamera_StillPhotoCaptureFinishedCallback.class, __md_methods);
	}

	public NativeCamera_StillPhotoCaptureFinishedCallback ()
	{
		super ();
		if (getClass () == NativeCamera_StillPhotoCaptureFinishedCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.NativeCamera+StillPhotoCaptureFinishedCallback, DrawnUi.Maui.Camera", "", this, new java.lang.Object[] {  });
		}
	}

	public NativeCamera_StillPhotoCaptureFinishedCallback (crc6451b7f263ad7a2be9.NativeCamera p0)
	{
		super ();
		if (getClass () == NativeCamera_StillPhotoCaptureFinishedCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.NativeCamera+StillPhotoCaptureFinishedCallback, DrawnUi.Maui.Camera", "DrawnUi.Camera.NativeCamera, DrawnUi.Maui.Camera", this, new java.lang.Object[] { p0 });
		}
	}

	public void onCaptureCompleted (android.hardware.camera2.CameraCaptureSession p0, android.hardware.camera2.CaptureRequest p1, android.hardware.camera2.TotalCaptureResult p2)
	{
		n_onCaptureCompleted (p0, p1, p2);
	}

	private native void n_onCaptureCompleted (android.hardware.camera2.CameraCaptureSession p0, android.hardware.camera2.CaptureRequest p1, android.hardware.camera2.TotalCaptureResult p2);

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
