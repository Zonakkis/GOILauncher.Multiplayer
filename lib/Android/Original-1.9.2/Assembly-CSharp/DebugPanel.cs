using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000102 RID: 258
public class DebugPanel : MonoBehaviour
{
	// Token: 0x04000613 RID: 1555
	public PlayerControl player;

	// Token: 0x04000614 RID: 1556
	public Slider velocityToWorldSlider;

	// Token: 0x04000615 RID: 1557
	public Text velocityToWorldValue;

	// Token: 0x04000616 RID: 1558
	public Slider rawInputSlider;

	// Token: 0x04000617 RID: 1559
	public Text rawInputValue;

	// Token: 0x04000618 RID: 1560
	public Toggle expInputToggle;

	// Token: 0x04000619 RID: 1561
	public Toggle smoothInputToggle;

	// Token: 0x0400061A RID: 1562
	public Toggle smoothOutputToggle;

	// Token: 0x0400061B RID: 1563
	public Toggle multOutputVelocityAverage;

	// Token: 0x0400061C RID: 1564
	public Toggle multOutputTimeDelta;

	// Token: 0x0400061D RID: 1565
	public Text sensitivityValue;
}
