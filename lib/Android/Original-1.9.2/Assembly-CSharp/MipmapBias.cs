using System;
using UnityEngine;

// Token: 0x02000131 RID: 305
public class MipmapBias : MonoBehaviour
{
	// Token: 0x060007C2 RID: 1986 RVA: 0x0004231C File Offset: 0x0004071C
	private void Start()
	{
		foreach (Texture texture in this.textures)
		{
			texture.mipMapBias = this.bias;
		}
	}

	// Token: 0x0400072D RID: 1837
	public Texture[] textures;

	// Token: 0x0400072E RID: 1838
	private float bias = -1f;
}
