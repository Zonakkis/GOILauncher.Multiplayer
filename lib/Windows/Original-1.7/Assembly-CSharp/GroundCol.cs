using System;
using UnityEngine;

// Token: 0x02000034 RID: 52
public class GroundCol : MonoBehaviour
{
	// Token: 0x04000254 RID: 596
	public Color groundCol = new Color(0.7f, 0.6f, 0.3f);

	// Token: 0x04000255 RID: 597
	public GroundCol.SoundMaterial material;

	// Token: 0x02000230 RID: 560
	public enum SoundMaterial
	{
		// Token: 0x04000E2D RID: 3629
		rock,
		// Token: 0x04000E2E RID: 3630
		wood,
		// Token: 0x04000E2F RID: 3631
		metal,
		// Token: 0x04000E30 RID: 3632
		plastic,
		// Token: 0x04000E31 RID: 3633
		furniture,
		// Token: 0x04000E32 RID: 3634
		snow,
		// Token: 0x04000E33 RID: 3635
		cardboard,
		// Token: 0x04000E34 RID: 3636
		none,
		// Token: 0x04000E35 RID: 3637
		snake,
		// Token: 0x04000E36 RID: 3638
		solidmetal
	}
}
