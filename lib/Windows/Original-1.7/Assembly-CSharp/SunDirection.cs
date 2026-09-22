using System;
using UnityEngine;

// Token: 0x02000008 RID: 8
[ExecuteInEditMode]
public class SunDirection : MonoBehaviour
{
	// Token: 0x0600002B RID: 43 RVA: 0x0000308E File Offset: 0x0000128E
	private void SetVector()
	{
		if (this._Material)
		{
			this.L = base.transform.forward;
			this._Material.SetVector("_L", -this.L);
		}
	}

	// Token: 0x0600002C RID: 44 RVA: 0x000030CE File Offset: 0x000012CE
	private void OnEnable()
	{
		this.SetVector();
	}

	// Token: 0x0600002D RID: 45 RVA: 0x000030D6 File Offset: 0x000012D6
	private void Update()
	{
		if (this.RealtimeUpdate)
		{
			this.SetVector();
		}
	}

	// Token: 0x04000036 RID: 54
	public Material _Material;

	// Token: 0x04000037 RID: 55
	public bool RealtimeUpdate;

	// Token: 0x04000038 RID: 56
	public Vector3 L;
}
