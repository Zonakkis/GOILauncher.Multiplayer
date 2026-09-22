using System;
using UnityEngine;

// Token: 0x0200014B RID: 331
[ExecuteInEditMode]
public class SplineSnapper : MonoBehaviour
{
	// Token: 0x0600093D RID: 2365 RVA: 0x0004B5CB File Offset: 0x000499CB
	private void Start()
	{
	}

	// Token: 0x0600093E RID: 2366 RVA: 0x0004B5D0 File Offset: 0x000499D0
	private void Update()
	{
		Vector3 position = base.transform.position;
		base.transform.position = position;
	}
}
