using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000BE RID: 190
[Serializable]
public class MB_AtlasesAndRects
{
	// Token: 0x0400049E RID: 1182
	public Texture2D[] atlases;

	// Token: 0x0400049F RID: 1183
	[NonSerialized]
	public List<MB_MaterialAndUVRect> mat2rect_map;

	// Token: 0x040004A0 RID: 1184
	public string[] texPropertyNames;
}
