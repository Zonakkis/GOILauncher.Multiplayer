using System;
using UnityEngine;

// Token: 0x0200005B RID: 91
public class Match2DTransform : MonoBehaviour
{
	// Token: 0x0600021E RID: 542 RVA: 0x00003A02 File Offset: 0x00001C02
	private void LateUpdate()
	{
		base.transform.rotation = this.target.rotation;
		base.transform.position = this.target.position;
	}

	// Token: 0x04000348 RID: 840
	public Transform target;
}
