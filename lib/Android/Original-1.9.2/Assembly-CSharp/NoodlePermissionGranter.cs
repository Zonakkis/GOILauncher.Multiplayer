using System;
using UnityEngine;

// Token: 0x0200010E RID: 270
public class NoodlePermissionGranter : MonoBehaviour
{
	// Token: 0x0600070E RID: 1806 RVA: 0x0003DF89 File Offset: 0x0003C389
	public static void GrantPermission(NoodlePermissionGranter.NoodleAndroidPermission permission)
	{
		if (!NoodlePermissionGranter.initialized)
		{
			NoodlePermissionGranter.initialize();
		}
		NoodlePermissionGranter.noodlePermissionGranterClass.CallStatic("grantPermission", new object[]
		{
			NoodlePermissionGranter.activity,
			(int)permission
		});
	}

	// Token: 0x0600070F RID: 1807 RVA: 0x0003DFC0 File Offset: 0x0003C3C0
	public void Awake()
	{
		NoodlePermissionGranter.instance = this;
		global::UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (base.name != "NoodlePermissionGranter")
		{
			base.name = "NoodlePermissionGranter";
		}
	}

	// Token: 0x06000710 RID: 1808 RVA: 0x0003DFF4 File Offset: 0x0003C3F4
	private static void initialize()
	{
		if (NoodlePermissionGranter.instance == null)
		{
			GameObject gameObject = new GameObject();
			NoodlePermissionGranter.instance = gameObject.AddComponent<NoodlePermissionGranter>();
			gameObject.name = "NoodlePermissionGranter";
		}
		NoodlePermissionGranter.noodlePermissionGranterClass = new AndroidJavaClass("com.noodlecake.unityplugins.NoodlePermissionGranter");
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		NoodlePermissionGranter.activity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		NoodlePermissionGranter.initialized = true;
	}

	// Token: 0x06000711 RID: 1809 RVA: 0x0003E060 File Offset: 0x0003C460
	private void permissionRequestCallbackInternal(string message)
	{
		bool flag = message == "PERMISSION_GRANTED";
		if (NoodlePermissionGranter.PermissionRequestCallback != null)
		{
			NoodlePermissionGranter.PermissionRequestCallback(flag);
		}
	}

	// Token: 0x04000661 RID: 1633
	public static Action<bool> PermissionRequestCallback;

	// Token: 0x04000662 RID: 1634
	private static NoodlePermissionGranter instance;

	// Token: 0x04000663 RID: 1635
	private static bool initialized;

	// Token: 0x04000664 RID: 1636
	private static AndroidJavaClass noodlePermissionGranterClass;

	// Token: 0x04000665 RID: 1637
	private static AndroidJavaObject activity;

	// Token: 0x04000666 RID: 1638
	private const string WRITE_EXTERNAL_STORAGE = "WRITE_EXTERNAL_STORAGE";

	// Token: 0x04000667 RID: 1639
	private const string PERMISSION_GRANTED = "PERMISSION_GRANTED";

	// Token: 0x04000668 RID: 1640
	private const string PERMISSION_DENIED = "PERMISSION_DENIED";

	// Token: 0x04000669 RID: 1641
	private const string NOODLE_PERMISSION_GRANTER = "NoodlePermissionGranter";

	// Token: 0x0200010F RID: 271
	public enum NoodleAndroidPermission
	{
		// Token: 0x0400066B RID: 1643
		WRITE_EXTERNAL_STORAGE
	}
}
