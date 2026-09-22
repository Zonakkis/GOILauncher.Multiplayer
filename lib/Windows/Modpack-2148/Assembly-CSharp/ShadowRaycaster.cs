using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200007A RID: 122
public class ShadowRaycaster : MonoBehaviour
{
	// Token: 0x06000365 RID: 869 RVA: 0x00031D68 File Offset: 0x0002FF68
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

	// Token: 0x06000366 RID: 870 RVA: 0x00031E60 File Offset: 0x00030060
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

	// Token: 0x04000537 RID: 1335
	public LayerMask mask;

	// Token: 0x04000538 RID: 1336
	private Ray ray;

	// Token: 0x04000539 RID: 1337
	private RaycastHit hit;

	// Token: 0x0400053A RID: 1338
	private Camera maincam;

	// Token: 0x0400053B RID: 1339
	private Vector3 offset;

	// Token: 0x0400053C RID: 1340
	private SkinnedMeshRenderer[] skrns;

	// Token: 0x0400053D RID: 1341
	private MeshRenderer[] mrns;

	// Token: 0x0400053F RID: 1343
	private Vector3[] points;

	// Token: 0x04000540 RID: 1344
	private MaterialPropertyBlock propBlock;

	// Token: 0x04000541 RID: 1345
	private List<Vector4> vectorOut;
}
