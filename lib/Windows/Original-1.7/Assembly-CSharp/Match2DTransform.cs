using System;
using UnityEngine;

// Token: 0x0200003A RID: 58
public class Match2DTransform : MonoBehaviour
{
	// Token: 0x060001B2 RID: 434 RVA: 0x000102CA File Offset: 0x0000E4CA
	private void LateUpdate()
	{
		base.transform.rotation = this.target.rotation;
		base.transform.position = this.target.position;
	}

	// Token: 0x040002B6 RID: 694
	public Transform target;
}
