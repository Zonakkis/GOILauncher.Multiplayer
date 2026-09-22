using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200004D RID: 77
public class ShadowRaycaster : MonoBehaviour
{
	// Token: 0x0600027F RID: 639 RVA: 0x00017FA4 File Offset: 0x000161A4
	private void Start()
	{
		this.ray = default(Ray);
		this.hit = default(RaycastHit);
		this.maincam = Camera.main;
		this.offset = new Vector3(-0.4f, -0.4f, 0f);
		this.skrns = base.GetComponentsInChildren<SkinnedMeshRenderer>();
		this.mrns = base.GetComponentsInChildren<MeshRenderer>();
		this.propBlock = new MaterialPropertyBlock();
		this.vectorOut = new List<Vector4>(4);
		for (int j = 0; j < 4; j++)
		{
			this.vectorOut.Add(new Vector4(0f, 0f, 0f, 0f));
		}
	}

	// Token: 0x06000280 RID: 640 RVA: 0x0001809C File Offset: 0x0001629C
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
			for (int k = 0; k < this.points.Length; k++)
			{
				float num6 = (meshRenderer.bounds.center - this.points[k]).sqrMagnitude;
				if (num6 < num5)
				{
					num5 = num6;
					num4 = num3;
					num3 = num2;
					num2 = num;
					num = k;
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

	// Token: 0x04000426 RID: 1062
	public LayerMask mask;

	// Token: 0x04000427 RID: 1063
	private Ray ray;

	// Token: 0x04000428 RID: 1064
	private RaycastHit hit;

	// Token: 0x04000429 RID: 1065
	private Camera maincam;

	// Token: 0x0400042A RID: 1066
	private Vector3 offset;

	// Token: 0x0400042B RID: 1067
	private SkinnedMeshRenderer[] skrns;

	// Token: 0x0400042C RID: 1068
	private MeshRenderer[] mrns;

	// Token: 0x0400042E RID: 1070
	private Vector3[] points;

	// Token: 0x0400042F RID: 1071
	private MaterialPropertyBlock propBlock;

	// Token: 0x04000430 RID: 1072
	private List<Vector4> vectorOut;
}
