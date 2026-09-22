using System;
using UnityEngine;

// Token: 0x0200007E RID: 126
[ExecuteInEditMode]
public class SplineSnapper : MonoBehaviour
{
	// Token: 0x0600036F RID: 879 RVA: 0x0000265E File Offset: 0x0000085E
	private void Start()
	{
	}

	// Token: 0x06000370 RID: 880 RVA: 0x000322C4 File Offset: 0x000304C4
	private void Update()
	{
		Vector3 position = base.transform.position;
		position.z = vector.z;
		base.transform.position = position;
	}
}
