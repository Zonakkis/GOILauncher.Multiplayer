using System;
using UnityEngine;

// Token: 0x0200002D RID: 45
[ExecuteInEditMode]
public class EnableDepthInForwardCamera : MonoBehaviour
{
	// Token: 0x0600012B RID: 299 RVA: 0x0000265E File Offset: 0x0000085E
	private void OnEnable()
	{
	}

	// Token: 0x0600012C RID: 300 RVA: 0x0000313D File Offset: 0x0000133D
	private void Update()
	{
		if (base.GetComponent<Camera>().depthTextureMode != DepthTextureMode.Depth)
		{
			base.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		}
	}

	// Token: 0x04000203 RID: 515
	public string Message = "Add this script to generate depth if Fog Volume rendered is not used and rendering path is forward";
}
