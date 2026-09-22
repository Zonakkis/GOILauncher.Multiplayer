using System;
using UnityEngine;

// Token: 0x02000034 RID: 52
public class MipmapBias : MonoBehaviour
{
	// Token: 0x06000152 RID: 338 RVA: 0x0001E4B4 File Offset: 0x0001C6B4
	private void Start()
	{
		Texture[] array = this.textures;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mipMapBias = this.bias;
		}
	}

	// Token: 0x04000245 RID: 581
	public Texture[] textures;

	// Token: 0x04000246 RID: 582
	private float bias = -1f;
}
