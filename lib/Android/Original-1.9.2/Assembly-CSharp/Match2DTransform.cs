using System;
using UnityEngine;

// Token: 0x02000130 RID: 304
public class Match2DTransform : MonoBehaviour
{
	// Token: 0x060007C0 RID: 1984 RVA: 0x000422D9 File Offset: 0x000406D9
	private void LateUpdate()
	{
		base.transform.rotation = this.target.rotation;
		base.transform.position = this.target.position;
	}

	// Token: 0x0400072C RID: 1836
	public Transform target;
}
