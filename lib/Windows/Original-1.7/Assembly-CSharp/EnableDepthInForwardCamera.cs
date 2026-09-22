using System;
using UnityEngine;

// Token: 0x0200001A RID: 26
[ExecuteInEditMode]
public class EnableDepthInForwardCamera : MonoBehaviour
{
	// Token: 0x06000109 RID: 265 RVA: 0x0000BC38 File Offset: 0x00009E38
	private void OnEnable()
	{
	}

	// Token: 0x0600010A RID: 266 RVA: 0x0000BC3A File Offset: 0x00009E3A
	private void Update()
	{
		if (base.GetComponent<Camera>().depthTextureMode != DepthTextureMode.Depth)
		{
			base.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
	}

	// Token: 0x040001B6 RID: 438
	public string Message = "Add this script to generate depth if Fog Volume rendered is not used and rendering path is forward";
}
