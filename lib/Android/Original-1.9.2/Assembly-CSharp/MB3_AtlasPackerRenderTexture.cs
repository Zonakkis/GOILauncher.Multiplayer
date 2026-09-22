using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000A1 RID: 161
[ExecuteInEditMode]
public class MB3_AtlasPackerRenderTexture : MonoBehaviour
{
	// Token: 0x06000431 RID: 1073 RVA: 0x0002133C File Offset: 0x0001F73C
	public Texture2D OnRenderAtlas(MB3_TextureCombiner combiner)
	{
		this.fastRenderer = new MB_TextureCombinerRenderTexture();
		this._doRenderAtlas = true;
		Texture2D texture2D = this.fastRenderer.DoRenderAtlas(base.gameObject, this.width, this.height, this.padding, this.rects, this.textureSets, this.indexOfTexSetToRender, this.texPropertyName, this.resultMaterialTextureBlender, this.isNormalMap, this.fixOutOfBoundsUVs, this.considerNonTextureProperties, combiner, this.LOG_LEVEL);
		this._doRenderAtlas = false;
		return texture2D;
	}

	// Token: 0x06000432 RID: 1074 RVA: 0x000213BE File Offset: 0x0001F7BE
	private void OnRenderObject()
	{
		if (this._doRenderAtlas)
		{
			this.fastRenderer.OnRenderObject();
			this._doRenderAtlas = false;
		}
	}

	// Token: 0x040003E6 RID: 998
	private MB_TextureCombinerRenderTexture fastRenderer;

	// Token: 0x040003E7 RID: 999
	private bool _doRenderAtlas;

	// Token: 0x040003E8 RID: 1000
	public int width;

	// Token: 0x040003E9 RID: 1001
	public int height;

	// Token: 0x040003EA RID: 1002
	public int padding;

	// Token: 0x040003EB RID: 1003
	public bool isNormalMap;

	// Token: 0x040003EC RID: 1004
	public bool fixOutOfBoundsUVs;

	// Token: 0x040003ED RID: 1005
	public bool considerNonTextureProperties;

	// Token: 0x040003EE RID: 1006
	public TextureBlender resultMaterialTextureBlender;

	// Token: 0x040003EF RID: 1007
	public Rect[] rects;

	// Token: 0x040003F0 RID: 1008
	public Texture2D tex1;

	// Token: 0x040003F1 RID: 1009
	public List<MB3_TextureCombiner.MB_TexSet> textureSets;

	// Token: 0x040003F2 RID: 1010
	public int indexOfTexSetToRender;

	// Token: 0x040003F3 RID: 1011
	public ShaderTextureProperty texPropertyName;

	// Token: 0x040003F4 RID: 1012
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	// Token: 0x040003F5 RID: 1013
	public Texture2D testTex;

	// Token: 0x040003F6 RID: 1014
	public Material testMat;
}
