using System;
using UnityEngine;

// Token: 0x0200004A RID: 74
public class GroundCol : MonoBehaviour
{
	// Token: 0x040002AF RID: 687
	public Color groundCol = new Color(0.7f, 0.6f, 0.3f);

	// Token: 0x040002B0 RID: 688
	public GroundCol.SoundMaterial material;

	// Token: 0x0200004B RID: 75
	public enum SoundMaterial
	{
		// Token: 0x040002B2 RID: 690
		rock,
		// Token: 0x040002B3 RID: 691
		wood,
		// Token: 0x040002B4 RID: 692
		metal,
		// Token: 0x040002B5 RID: 693
		plastic,
		// Token: 0x040002B6 RID: 694
		furniture,
		// Token: 0x040002B7 RID: 695
		snow,
		// Token: 0x040002B8 RID: 696
		cardboard,
		// Token: 0x040002B9 RID: 697
		none,
		// Token: 0x040002BA RID: 698
		snake,
		// Token: 0x040002BB RID: 699
		solidmetal
	}
}
