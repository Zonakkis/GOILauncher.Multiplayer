using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000147 RID: 327
public class ShadowRaycaster : MonoBehaviour
{
	// Token: 0x06000933 RID: 2355 RVA: 0x0004AFD0 File Offset: 0x000493D0
	private void Start()
	{
		this.skrns = base.GetComponentsInChildren<SkinnedMeshRenderer>();
		this.mrns = base.GetComponentsInChildren<MeshRenderer>();
		this.propBlock = new MaterialPropertyBlock();
		this.vectorOut = new List<Vector4>(4);
		for (int j = 0; j < 4; j++)
		{
			this.vectorOut.Add(new Vector4(0f, 0f, 0f, 0f));
		}
	}

	// Token: 0x06000934 RID: 2356 RVA: 0x0004B09C File Offset: 0x0004949C
	private void LateUpdate()
	{
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.skrns)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			float num5 = 999999f;
			for (int j = 0; j < this.points.Length; j++)
			{
				float num6 = (skinnedMeshRenderer.bounds.center - this.points[j]).sqrMagnitude;
				if (num6 < num5)
				{
					num5 = num6;
					num4 = num3;
					num3 = num2;
					num2 = num;
					num = j;
				}
			}
			skinnedMeshRenderer.GetPropertyBlock(this.propBlock);
			this.vectorOut[0] = this.points[num];
			this.vectorOut[1] = this.points[num2];
			this.vectorOut[2] = this.points[num3];
			this.vectorOut[3] = this.points[num4];
			this.propBlock.SetVectorArray("_Nearest", this.vectorOut);
			skinnedMeshRenderer.SetPropertyBlock(this.propBlock);
		}
		foreach (MeshRenderer meshRenderer in this.mrns)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			float num5 = 999999f;
			for (int l = 0; l < this.points.Length; l++)
			{
				float num6 = (meshRenderer.bounds.center - this.points[l]).sqrMagnitude;
				if (num6 < num5)
				{
					num5 = num6;
					num4 = num3;
					num3 = num2;
					num2 = num;
					num = l;
				}
			}
			Debug.DrawLine(meshRenderer.bounds.center, this.points[num], Color.cyan);
			Debug.DrawLine(meshRenderer.bounds.center, this.points[num2], Color.blue);
			meshRenderer.GetPropertyBlock(this.propBlock);
			this.vectorOut[0] = this.points[num];
			this.vectorOut[1] = this.points[num2];
			this.vectorOut[2] = this.points[num3];
			this.vectorOut[3] = this.points[num4];
			this.propBlock.SetVectorArray("_Nearest", this.vectorOut);
			meshRenderer.SetPropertyBlock(this.propBlock);
		}
	}

	// Token: 0x040008C2 RID: 2242
	private SkinnedMeshRenderer[] skrns;

	// Token: 0x040008C3 RID: 2243
	private MeshRenderer[] mrns;

	// Token: 0x040008C5 RID: 2245
	private Vector3[] points;

	// Token: 0x040008C6 RID: 2246
	private MaterialPropertyBlock propBlock;

	// Token: 0x040008C7 RID: 2247
	private List<Vector4> vectorOut;
}
