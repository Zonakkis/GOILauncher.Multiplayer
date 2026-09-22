using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000BF RID: 191
[Serializable]
public class MB_MultiMaterial
{
	// Token: 0x040004A1 RID: 1185
	public Material combinedMaterial;

	// Token: 0x040004A2 RID: 1186
	public bool considerMeshUVs;

	// Token: 0x040004A3 RID: 1187
	public List<Material> sourceMaterials = new List<Material>();
}
