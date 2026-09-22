using System;
using UnityEngine;

// Token: 0x02000085 RID: 133
public class YPbPr : MonoBehaviour
{
	// Token: 0x0600038B RID: 907 RVA: 0x000046C8 File Offset: 0x000028C8
	private void Awake()
	{
		this.material = new Material(Shader.Find("Hidden/YPbPr"));
	}

	// Token: 0x0600038C RID: 908 RVA: 0x000046DF File Offset: 0x000028DF
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit(source, destination, this.material);
	}

	// Token: 0x0400055B RID: 1371
	private Material material;
}
