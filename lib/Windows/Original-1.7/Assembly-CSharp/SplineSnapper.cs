using System;
using UnityEngine;

// Token: 0x02000051 RID: 81
[ExecuteInEditMode]
public class SplineSnapper : MonoBehaviour
{
	// Token: 0x06000289 RID: 649 RVA: 0x00018560 File Offset: 0x00016760
	private void Start()
	{
	}

	// Token: 0x0600028A RID: 650 RVA: 0x00018564 File Offset: 0x00016764
	private void Update()
	{
		Vector3 position = base.transform.position;
		position.z = vector.z;
		base.transform.position = position;
	}
}
