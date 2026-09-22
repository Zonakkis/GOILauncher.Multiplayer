using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x020000F9 RID: 249
public class ReplayKitManager : MonoBehaviour
{
	// Token: 0x040005D5 RID: 1493
	public Button StartStreamButton;

	// Token: 0x040005D6 RID: 1494
	public Button StopStreamButton;

	// Token: 0x040005D7 RID: 1495
	public Toggle camToggle;

	// Token: 0x040005D8 RID: 1496
	public Toggle micToggle;

	// Token: 0x040005D9 RID: 1497
	public GameObject shareURL;

	// Token: 0x040005DA RID: 1498
	public TextMeshProUGUI URLText;

	// Token: 0x040005DB RID: 1499
	public bool CamEnabled = true;

	// Token: 0x040005DC RID: 1500
	public bool MicEnabled = true;

	// Token: 0x040005DD RID: 1501
	private static Vector2 CameraPreviewPositionOffset = new Vector2(150f, 50f);

	// Token: 0x040005DE RID: 1502
	private float camDisableTimer;

	// Token: 0x040005DF RID: 1503
	private float camEnableTimer;

	// Token: 0x040005E0 RID: 1504
	private bool setCameraState;

	// Token: 0x040005E1 RID: 1505
	private bool broadcastCallbackFailed;

	// Token: 0x040005E2 RID: 1506
	private bool broadcastCallbackSuccess;

	// Token: 0x040005E3 RID: 1507
	private MobileManager mobileMan;

	// Token: 0x040005E4 RID: 1508
	private MobileManager.MobileScale scale;

	// Token: 0x040005E5 RID: 1509
	private float scaleModifier;
}
