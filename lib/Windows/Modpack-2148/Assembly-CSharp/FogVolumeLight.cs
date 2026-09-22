using System;
using UnityEngine;

// Token: 0x0200001E RID: 30
[ExecuteInEditMode]
public class FogVolumeLight : MonoBehaviour
{
	// Token: 0x0600008A RID: 138 RVA: 0x0001933C File Offset: 0x0001753C
	private void OnEnable()
	{
		SphereCollider component = base.GetComponent<SphereCollider>();
		if (component != null)
		{
			global::UnityEngine.Object.Destroy(component);
		}
	}

	// Token: 0x0400014F RID: 335
	public bool IsAddedToNormalLight;

	// Token: 0x04000150 RID: 336
	public bool IsPointLight;

	// Token: 0x04000151 RID: 337
	public bool Enabled = true;

	// Token: 0x04000152 RID: 338
	public Color Color = Color.white;

	// Token: 0x04000153 RID: 339
	public float Intensity = 1f;

	// Token: 0x04000154 RID: 340
	public float Range = 10f;

	// Token: 0x04000155 RID: 341
	public float Angle = 30f;
}
