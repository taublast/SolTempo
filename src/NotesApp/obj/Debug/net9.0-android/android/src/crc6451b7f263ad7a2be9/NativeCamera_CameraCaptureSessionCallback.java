package crc6451b7f263ad7a2be9;


public class NativeCamera_CameraCaptureSessionCallback
	extends android.hardware.camera2.CameraCaptureSession.StateCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onConfigureFailed:(Landroid/hardware/camera2/CameraCaptureSession;)V:GetOnConfigureFailed_Landroid_hardware_camera2_CameraCaptureSession_Handler\n" +
			"n_onConfigured:(Landroid/hardware/camera2/CameraCaptureSession;)V:GetOnConfigured_Landroid_hardware_camera2_CameraCaptureSession_Handler\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Camera.NativeCamera+CameraCaptureSessionCallback, DrawnUi.Maui.Camera", NativeCamera_CameraCaptureSessionCallback.class, __md_methods);
	}

	public NativeCamera_CameraCaptureSessionCallback ()
	{
		super ();
		if (getClass () == NativeCamera_CameraCaptureSessionCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.NativeCamera+CameraCaptureSessionCallback, DrawnUi.Maui.Camera", "", this, new java.lang.Object[] {  });
		}
	}

	public NativeCamera_CameraCaptureSessionCallback (crc6451b7f263ad7a2be9.NativeCamera p0)
	{
		super ();
		if (getClass () == NativeCamera_CameraCaptureSessionCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.NativeCamera+CameraCaptureSessionCallback, DrawnUi.Maui.Camera", "DrawnUi.Camera.NativeCamera, DrawnUi.Maui.Camera", this, new java.lang.Object[] { p0 });
		}
	}

	public void onConfigureFailed (android.hardware.camera2.CameraCaptureSession p0)
	{
		n_onConfigureFailed (p0);
	}

	private native void n_onConfigureFailed (android.hardware.camera2.CameraCaptureSession p0);

	public void onConfigured (android.hardware.camera2.CameraCaptureSession p0)
	{
		n_onConfigured (p0);
	}

	private native void n_onConfigured (android.hardware.camera2.CameraCaptureSession p0);

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
