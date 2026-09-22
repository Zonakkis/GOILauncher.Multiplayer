using System;
using UnityEngine;

// Token: 0x02000009 RID: 9
[ExecuteInEditMode]
public class SunDirection : MonoBehaviour
{
	// Token: 0x0600002C RID: 44 RVA: 0x000027AF File Offset: 0x000009AF
	private void SetVector()
	{
		if (this._Material)
		{
			this.L = base.transform.forward;
			this._Material.SetVector("_L", -this.L);
		}
	}

	// Token: 0x0600002D RID: 45 RVA: 0x000027EF File Offset: 0x000009EF
	private void OnEnable()
	{
		this.SetVector();
	}

	// Token: 0x0600002E RID: 46 RVA: 0x000027F7 File Offset: 0x000009F7
	private void Update()
	{
		if (this.RealtimeUpdate)
		{
			this.SetVector();
		}
	}

	// Token: 0x04000038 RID: 56
	public Material _Material;

	// Token: 0x04000039 RID: 57
	public bool RealtimeUpdate;

	// Token: 0x0400003A RID: 58
	public Vector3 L;
}
