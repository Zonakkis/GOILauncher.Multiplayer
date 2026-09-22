using System;
using UnityEngine;

// Token: 0x02000127 RID: 295
public class GroundCol : MonoBehaviour
{
	// Token: 0x040006C1 RID: 1729
	public Color groundCol = new Color(0.7f, 0.6f, 0.3f);

	// Token: 0x040006C2 RID: 1730
	public GroundCol.SoundMaterial material;

	// Token: 0x02000128 RID: 296
	public enum SoundMaterial
	{
		// Token: 0x040006C4 RID: 1732
		rock,
		// Token: 0x040006C5 RID: 1733
		wood,
		// Token: 0x040006C6 RID: 1734
		metal,
		// Token: 0x040006C7 RID: 1735
		plastic,
		// Token: 0x040006C8 RID: 1736
		furniture,
		// Token: 0x040006C9 RID: 1737
		snow,
		// Token: 0x040006CA RID: 1738
		cardboard,
		// Token: 0x040006CB RID: 1739
		none,
		// Token: 0x040006CC RID: 1740
		snake,
		// Token: 0x040006CD RID: 1741
		solidmetal
	}
}
