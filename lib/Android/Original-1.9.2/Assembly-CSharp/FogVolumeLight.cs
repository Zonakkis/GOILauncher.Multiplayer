using System;
using UnityEngine;

// Token: 0x02000015 RID: 21
[ExecuteInEditMode]
public class FogVolumeLight : MonoBehaviour
{
	// Token: 0x06000069 RID: 105 RVA: 0x00007B58 File Offset: 0x00005F58
	private void OnEnable()
	{
		SphereCollider component = base.GetComponent<SphereCollider>();
		if (component != null)
		{
			global::UnityEngine.Object.Destroy(component);
		}
	}

	// Token: 0x0400012A RID: 298
	public bool IsAddedToNormalLight;

	// Token: 0x0400012B RID: 299
	public bool IsPointLight;

	// Token: 0x0400012C RID: 300
	public bool Enabled = true;

	// Token: 0x0400012D RID: 301
	public Color Color = Color.white;

	// Token: 0x0400012E RID: 302
	public float Intensity = 1f;

	// Token: 0x0400012F RID: 303
	public float Range = 10f;

	// Token: 0x04000130 RID: 304
	public float Angle = 30f;
}
