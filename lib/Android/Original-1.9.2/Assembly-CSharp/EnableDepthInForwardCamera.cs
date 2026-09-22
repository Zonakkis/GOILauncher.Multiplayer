using System;
using UnityEngine;

// Token: 0x02000023 RID: 35
[ExecuteInEditMode]
public class EnableDepthInForwardCamera : MonoBehaviour
{
	// Token: 0x060000F8 RID: 248 RVA: 0x0000B812 File Offset: 0x00009C12
	private void OnEnable()
	{
		base.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}

	// Token: 0x060000F9 RID: 249 RVA: 0x0000B827 File Offset: 0x00009C27
	private void Update()
	{
		if (base.GetComponent<Camera>().depthTextureMode != DepthTextureMode.Depth)
		{
			base.GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
		}
	}
}
