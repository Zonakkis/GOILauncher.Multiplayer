using System;
using UnityEngine;

// Token: 0x02000058 RID: 88
public class YPbPr : MonoBehaviour
{
	// Token: 0x060002A5 RID: 677 RVA: 0x0001899C File Offset: 0x00016B9C
	private void Awake()
	{
		this.material = new Material(Shader.Find("Hidden/YPbPr"));
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x000189B3 File Offset: 0x00016BB3
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, this.material);
	}

	// Token: 0x0400044A RID: 1098
	private Material material;
}
