using System;
using UnityEngine;

// Token: 0x02000020 RID: 32
public class MipmapBias : MonoBehaviour
{
	// Token: 0x06000130 RID: 304 RVA: 0x0000CF8C File Offset: 0x0000B18C
	private void Start()
	{
		Texture[] array = this.textures;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].mipMapBias = this.bias;
		}
	}

	// Token: 0x040001F2 RID: 498
	public Texture[] textures;

	// Token: 0x040001F3 RID: 499
	private float bias = -1f;
}
