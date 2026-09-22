using System;
using UnityEngine;

// Token: 0x020000C0 RID: 192
[Serializable]
public class MB_MaterialAndUVRect
{
	// Token: 0x06000599 RID: 1433 RVA: 0x0003258C File Offset: 0x0003098C
	public MB_MaterialAndUVRect(Material m, Rect destRect, Rect samplingRectMatAndUVTiling, Rect sourceMaterialTiling, Rect samplingEncapsulatinRect, string objName)
	{
		this.material = m;
		this.atlasRect = destRect;
		this.samplingRectMatAndUVTiling = samplingRectMatAndUVTiling;
		this.sourceMaterialTiling = sourceMaterialTiling;
		this.samplingEncapsulatinRect = samplingEncapsulatinRect;
		this.srcObjName = objName;
	}

	// Token: 0x0600059A RID: 1434 RVA: 0x000325C1 File Offset: 0x000309C1
	public override int GetHashCode()
	{
		return this.material.GetInstanceID() ^ this.samplingEncapsulatinRect.GetHashCode();
	}

	// Token: 0x0600059B RID: 1435 RVA: 0x000325E0 File Offset: 0x000309E0
	public override bool Equals(object obj)
	{
		return obj is MB_MaterialAndUVRect && this.material == ((MB_MaterialAndUVRect)obj).material && this.samplingEncapsulatinRect == ((MB_MaterialAndUVRect)obj).samplingEncapsulatinRect;
	}

	// Token: 0x040004A4 RID: 1188
	public Material material;

	// Token: 0x040004A5 RID: 1189
	public Rect atlasRect;

	// Token: 0x040004A6 RID: 1190
	public string srcObjName;

	// Token: 0x040004A7 RID: 1191
	public Rect samplingRectMatAndUVTiling;

	// Token: 0x040004A8 RID: 1192
	public Rect sourceMaterialTiling;

	// Token: 0x040004A9 RID: 1193
	public Rect samplingEncapsulatinRect;
}
