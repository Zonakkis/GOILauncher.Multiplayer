using System;
using UnityEngine;

// Token: 0x02000012 RID: 18
[ExecuteInEditMode]
public class FogVolumeLight : MonoBehaviour
{
	// Token: 0x06000087 RID: 135 RVA: 0x00007868 File Offset: 0x00005A68
	private void OnEnable()
	{
		SphereCollider component = base.GetComponent<SphereCollider>();
		if (component != null)
		{
			Object.Destroy(component);
		}
	}

	// Token: 0x04000121 RID: 289
	public bool IsAddedToNormalLight;

	// Token: 0x04000122 RID: 290
	public bool IsPointLight;

	// Token: 0x04000123 RID: 291
	public bool Enabled = true;

	// Token: 0x04000124 RID: 292
	public Color Color = Color.white;

	// Token: 0x04000125 RID: 293
	public float Intensity = 1f;

	// Token: 0x04000126 RID: 294
	public float Range = 10f;

	// Token: 0x04000127 RID: 295
	public float Angle = 30f;
}
