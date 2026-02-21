package crc6451b7f263ad7a2be9;


public class NativeCamera_PhotoModeCaptureCallback
	extends android.hardware.camera2.CameraCaptureSession.CaptureCallback
	implements
		mono.android.IGCUserPeer,
		android.media.ImageReader.OnImageAvailableListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCaptureCompleted:(Landroid/hardware/camera2/CameraCaptureSession;Landroid/hardware/camera2/CaptureRequest;Landroid/hardware/camera2/TotalCaptureResult;)V:GetOnCaptureCompleted_Landroid_hardware_camera2_CameraCaptureSession_Landroid_hardware_camera2_CaptureRequest_Landroid_hardware_camera2_TotalCaptureResult_Handler\n" +
			"n_onCaptureProgressed:(Landroid/hardware/camera2/CameraCaptureSession;Landroid/hardware/camera2/CaptureRequest;Landroid/hardware/camera2/CaptureResult;)V:GetOnCaptureProgressed_Landroid_hardware_camera2_CameraCaptureSession_Landroid_hardware_camera2_CaptureRequest_Landroid_hardware_camera2_CaptureResult_Handler\n" +
			"n_onImageAvailable:(Landroid/media/ImageReader;)V:GetOnImageAvailable_Landroid_media_ImageReader_Handler:Android.Media.ImageReader/IOnImageAvailableListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("DrawnUi.Camera.NativeCamera+PhotoModeCaptureCallback, DrawnUi.Maui.Camera", NativeCamera_PhotoModeCaptureCallback.class, __md_methods);
	}

	public NativeCamera_PhotoModeCaptureCallback ()
	{
		super ();
		if (getClass () == NativeCamera_PhotoModeCaptureCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.NativeCamera+PhotoModeCaptureCallback, DrawnUi.Maui.Camera", "", this, new java.lang.Object[] {  });
		}
	}

	public NativeCamera_PhotoModeCaptureCallback (crc6451b7f263ad7a2be9.NativeCamera p0)
	{
		super ();
		if (getClass () == NativeCamera_PhotoModeCaptureCallback.class) {
			mono.android.TypeManager.Activate ("DrawnUi.Camera.NativeCamera+PhotoModeCaptureCallback, DrawnUi.Maui.Camera", "DrawnUi.Camera.NativeCamera, DrawnUi.Maui.Camera", this, new java.lang.Object[] { p0 });
		}
	}

	public void onCaptureCompleted (android.hardware.camera2.CameraCaptureSession p0, android.hardware.camera2.CaptureRequest p1, android.hardware.camera2.TotalCaptureResult p2)
	{
		n_onCaptureCompleted (p0, p1, p2);
	}

	private native void n_onCaptureCompleted (android.hardware.camera2.CameraCaptureSession p0, android.hardware.camera2.CaptureRequest p1, android.hardware.camera2.TotalCaptureResult p2);

	public void onCaptureProgressed (android.hardware.camera2.CameraCaptureSession p0, android.hardware.camera2.CaptureRequest p1, android.hardware.camera2.CaptureResult p2)
	{
		n_onCaptureProgressed (p0, p1, p2);
	}

	private native void n_onCaptureProgressed (android.hardware.camera2.CameraCaptureSession p0, android.hardware.camera2.CaptureRequest p1, android.hardware.camera2.CaptureResult p2);

	public void onImageAvailable (android.media.ImageReader p0)
	{
		n_onImageAvailable (p0);
	}

	private native void n_onImageAvailable (android.media.ImageReader p0);

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
