using System;
using UnityEngine;

// Token: 0x02000151 RID: 337
public class YPbPr : MonoBehaviour
{
	// Token: 0x0600094F RID: 2383 RVA: 0x0004B8FB File Offset: 0x00049CFB
	private void Awake()
	{
		this.material = new Material(Shader.Find("Hidden/YPbPr"));
	}

	// Token: 0x06000950 RID: 2384 RVA: 0x0004B912 File Offset: 0x00049D12
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, this.material);
	}

	// Token: 0x040008E0 RID: 2272
	private Material material;
}
