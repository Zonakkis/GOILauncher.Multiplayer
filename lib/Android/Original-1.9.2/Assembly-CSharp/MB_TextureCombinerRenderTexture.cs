using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000A0 RID: 160
public class MB_TextureCombinerRenderTexture
{
	// Token: 0x0600042A RID: 1066 RVA: 0x00020318 File Offset: 0x0001E718
	public Texture2D DoRenderAtlas(GameObject gameObject, int width, int height, int padding, Rect[] rss, List<MB3_TextureCombiner.MB_TexSet> textureSetss, int indexOfTexSetToRenders, ShaderTextureProperty texPropertyname, TextureBlender resultMaterialTextureBlender, bool isNormalMap, bool fixOutOfBoundsUVs, bool considerNonTextureProperties, MB3_TextureCombiner texCombiner, MB2_LogLevel LOG_LEV)
	{
		this.LOG_LEVEL = LOG_LEV;
		this.textureSets = textureSetss;
		this.indexOfTexSetToRender = indexOfTexSetToRenders;
		this._texPropertyName = texPropertyname;
		this._padding = padding;
		this._isNormalMap = isNormalMap;
		this._fixOutOfBoundsUVs = fixOutOfBoundsUVs;
		this._resultMaterialTextureBlender = resultMaterialTextureBlender;
		this.combiner = texCombiner;
		this.rs = rss;
		Shader shader;
		if (this._isNormalMap)
		{
			shader = Shader.Find("MeshBaker/NormalMapShader");
		}
		else
		{
			shader = Shader.Find("MeshBaker/AlbedoShader");
		}
		if (shader == null)
		{
			global::UnityEngine.Debug.LogError("Could not find shader for RenderTexture. Try reimporting mesh baker");
			return null;
		}
		this.mat = new Material(shader);
		this._destinationTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
		this._destinationTexture.filterMode = FilterMode.Point;
		this.myCamera = gameObject.GetComponent<Camera>();
		this.myCamera.orthographic = true;
		this.myCamera.orthographicSize = (float)(height >> 1);
		this.myCamera.aspect = (float)(width / height);
		this.myCamera.targetTexture = this._destinationTexture;
		this.myCamera.clearFlags = CameraClearFlags.Color;
		Transform component = this.myCamera.GetComponent<Transform>();
		component.localPosition = new Vector3((float)width / 2f, (float)height / 2f, 3f);
		component.localRotation = Quaternion.Euler(0f, 180f, 180f);
		this._doRenderAtlas = true;
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			global::UnityEngine.Debug.Log(string.Format("Begin Camera.Render destTex w={0} h={1} camPos={2}", width, height, component.localPosition));
		}
		this.myCamera.Render();
		this._doRenderAtlas = false;
		MB_Utility.Destroy(this.mat);
		MB_Utility.Destroy(this._destinationTexture);
		if (this.LOG_LEVEL >= MB2_LogLevel.debug)
		{
			global::UnityEngine.Debug.Log("Finished Camera.Render ");
		}
		Texture2D texture2D = this.targTex;
		this.targTex = null;
		return texture2D;
	}

	// Token: 0x0600042B RID: 1067 RVA: 0x000204FC File Offset: 0x0001E8FC
	public void OnRenderObject()
	{
		if (this._doRenderAtlas)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for (int i = 0; i < this.rs.Length; i++)
			{
				MB3_TextureCombiner.MeshBakerMaterialTexture meshBakerMaterialTexture = this.textureSets[i].ts[this.indexOfTexSetToRender];
				if (this.LOG_LEVEL >= MB2_LogLevel.trace && meshBakerMaterialTexture.t != null)
				{
					global::UnityEngine.Debug.Log(string.Concat(new object[]
					{
						"Added ",
						meshBakerMaterialTexture.t,
						" to atlas w=",
						meshBakerMaterialTexture.t.width,
						" h=",
						meshBakerMaterialTexture.t.height,
						" offset=",
						meshBakerMaterialTexture.matTilingRect.min,
						" scale=",
						meshBakerMaterialTexture.matTilingRect.size,
						" rect=",
						this.rs[i],
						" padding=",
						this._padding
					}));
					this._printTexture(meshBakerMaterialTexture.t);
				}
				this.CopyScaledAndTiledToAtlas(this.textureSets[i], meshBakerMaterialTexture, this.textureSets[i].obUVoffset, this.textureSets[i].obUVscale, this.rs[i], this._texPropertyName, this._resultMaterialTextureBlender);
			}
			stopwatch.Stop();
			stopwatch.Start();
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				global::UnityEngine.Debug.Log("Total time for Graphics.DrawTexture calls " + stopwatch.ElapsedMilliseconds.ToString("f5"));
			}
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				global::UnityEngine.Debug.Log(string.Concat(new object[]
				{
					"Copying RenderTexture to Texture2D. destW",
					this._destinationTexture.width,
					" destH",
					this._destinationTexture.height
				}));
			}
			Texture2D texture2D = new Texture2D(this._destinationTexture.width, this._destinationTexture.height, TextureFormat.ARGB32, true);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = this._destinationTexture;
			int num = this._destinationTexture.width / 512;
			int num2 = this._destinationTexture.height / 512;
			if (num == 0 || num2 == 0)
			{
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					global::UnityEngine.Debug.Log("Copying all in one shot");
				}
				texture2D.ReadPixels(new Rect(0f, 0f, (float)this._destinationTexture.width, (float)this._destinationTexture.height), 0, 0, true);
			}
			else if (this.IsOpenGL())
			{
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					global::UnityEngine.Debug.Log("OpenGL copying blocks");
				}
				for (int j = 0; j < num; j++)
				{
					for (int k = 0; k < num2; k++)
					{
						texture2D.ReadPixels(new Rect((float)(j * 512), (float)(k * 512), 512f, 512f), j * 512, k * 512, true);
					}
				}
			}
			else
			{
				if (this.LOG_LEVEL >= MB2_LogLevel.trace)
				{
					global::UnityEngine.Debug.Log("Not OpenGL copying blocks");
				}
				for (int l = 0; l < num; l++)
				{
					for (int m = 0; m < num2; m++)
					{
						texture2D.ReadPixels(new Rect((float)(l * 512), (float)(this._destinationTexture.height - 512 - m * 512), 512f, 512f), l * 512, m * 512, true);
					}
				}
			}
			RenderTexture.active = active;
			texture2D.Apply();
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				global::UnityEngine.Debug.Log("TempTexture ");
				this._printTexture(texture2D);
			}
			this.myCamera.targetTexture = null;
			RenderTexture.active = null;
			this.targTex = texture2D;
			if (this.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				global::UnityEngine.Debug.Log("Total time to copy RenderTexture to Texture2D " + stopwatch.ElapsedMilliseconds.ToString("f5"));
			}
		}
	}

	// Token: 0x0600042C RID: 1068 RVA: 0x00020968 File Offset: 0x0001ED68
	private Color32 ConvertNormalFormatFromUnity_ToStandard(Color32 c)
	{
		Vector3 zero = Vector3.zero;
		zero.x = (float)c.a * 2f - 1f;
		zero.y = (float)c.g * 2f - 1f;
		zero.z = Mathf.Sqrt(1f - zero.x * zero.x - zero.y * zero.y);
		return new Color32
		{
			a = 1,
			r = (byte)((zero.x + 1f) * 0.5f),
			g = (byte)((zero.y + 1f) * 0.5f),
			b = (byte)((zero.z + 1f) * 0.5f)
		};
	}

	// Token: 0x0600042D RID: 1069 RVA: 0x00020A44 File Offset: 0x0001EE44
	private bool IsOpenGL()
	{
		string graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
		return graphicsDeviceVersion.StartsWith("OpenGL");
	}

	// Token: 0x0600042E RID: 1070 RVA: 0x00020A64 File Offset: 0x0001EE64
	private void CopyScaledAndTiledToAtlas(MB3_TextureCombiner.MB_TexSet texSet, MB3_TextureCombiner.MeshBakerMaterialTexture source, Vector2 obUVoffset, Vector2 obUVscale, Rect rec, ShaderTextureProperty texturePropertyName, TextureBlender resultMatTexBlender)
	{
		Rect rect = rec;
		if (resultMatTexBlender != null)
		{
			this.myCamera.backgroundColor = resultMatTexBlender.GetColorIfNoTexture(texSet.matsAndGOs.mats[0].mat, texturePropertyName);
		}
		else
		{
			this.myCamera.backgroundColor = MB3_TextureCombiner.GetColorIfNoTexture(texturePropertyName);
		}
		if (source.t == null)
		{
			source.t = this.combiner._createTemporaryTexture(16, 16, TextureFormat.ARGB32, true);
		}
		rect.y = 1f - (rect.y + rect.height);
		rect.x *= (float)this._destinationTexture.width;
		rect.y *= (float)this._destinationTexture.height;
		rect.width *= (float)this._destinationTexture.width;
		rect.height *= (float)this._destinationTexture.height;
		Rect rect2 = rect;
		rect2.x -= (float)this._padding;
		rect2.y -= (float)this._padding;
		rect2.width += (float)(this._padding * 2);
		rect2.height += (float)(this._padding * 2);
		Rect rect3 = source.matTilingRect.GetRect();
		Rect rect4 = default(Rect);
		if (this._fixOutOfBoundsUVs)
		{
			Rect rect5 = new Rect(obUVoffset.x, obUVoffset.y, obUVscale.x, obUVscale.y);
			rect3 = MB3_UVTransformUtility.CombineTransforms(ref rect3, ref rect5);
			if (this.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				global::UnityEngine.Debug.Log("Fixing out of bounds UVs for tex " + source.t);
			}
		}
		Texture2D t = source.t;
		TextureWrapMode wrapMode = t.wrapMode;
		if (rect3.width == 1f && rect3.height == 1f && rect3.x == 0f && rect3.y == 0f)
		{
			t.wrapMode = TextureWrapMode.Clamp;
		}
		else
		{
			t.wrapMode = TextureWrapMode.Repeat;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.trace)
		{
			global::UnityEngine.Debug.Log(string.Concat(new object[] { "DrawTexture tex=", t.name, " destRect=", rect, " srcRect=", rect3, " Mat=", this.mat }));
		}
		Rect rect6 = default(Rect);
		rect6.x = rect3.x;
		rect6.y = rect3.y + 1f - 1f / (float)t.height;
		rect6.width = rect3.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x;
		rect4.y = rect2.y;
		rect4.width = rect.width;
		rect4.height = (float)this._padding;
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = this._destinationTexture;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y;
		rect6.width = rect3.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x;
		rect4.y = rect.y + rect.height;
		rect4.width = rect.width;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = rect3.height;
		rect4.x = rect2.x;
		rect4.y = rect.y;
		rect4.width = (float)this._padding;
		rect4.height = rect.height;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x + 1f - 1f / (float)t.width;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = rect3.height;
		rect4.x = rect.x + rect.width;
		rect4.y = rect.y;
		rect4.width = (float)this._padding;
		rect4.height = rect.height;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y + 1f - 1f / (float)t.height;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect2.x;
		rect4.y = rect2.y;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x + 1f - 1f / (float)t.width;
		rect6.y = rect3.y + 1f - 1f / (float)t.height;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x + rect.width;
		rect4.y = rect2.y;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect2.x;
		rect4.y = rect.y + rect.height;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		rect6.x = rect3.x + 1f - 1f / (float)t.width;
		rect6.y = rect3.y;
		rect6.width = 1f / (float)t.width;
		rect6.height = 1f / (float)t.height;
		rect4.x = rect.x + rect.width;
		rect4.y = rect.y + rect.height;
		rect4.width = (float)this._padding;
		rect4.height = (float)this._padding;
		Graphics.DrawTexture(rect4, t, rect6, 0, 0, 0, 0, this.mat);
		Graphics.DrawTexture(rect, t, rect3, 0, 0, 0, 0, this.mat);
		RenderTexture.active = active;
		t.wrapMode = wrapMode;
	}

	// Token: 0x0600042F RID: 1071 RVA: 0x0002125C File Offset: 0x0001F65C
	private void _printTexture(Texture2D t)
	{
		if (t.width * t.height > 100)
		{
			global::UnityEngine.Debug.Log("Not printing texture too large.");
		}
		try
		{
			Color32[] pixels = t.GetPixels32();
			string text = string.Empty;
			for (int i = 0; i < t.height; i++)
			{
				for (int j = 0; j < t.width; j++)
				{
					text = text + pixels[i * t.width + j] + ", ";
				}
				text += "\n";
			}
			global::UnityEngine.Debug.Log(text);
		}
		catch (Exception ex)
		{
			global::UnityEngine.Debug.Log("Could not print texture. texture may not be readable." + ex.ToString());
		}
	}

	// Token: 0x040003D7 RID: 983
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	// Token: 0x040003D8 RID: 984
	private Material mat;

	// Token: 0x040003D9 RID: 985
	private RenderTexture _destinationTexture;

	// Token: 0x040003DA RID: 986
	private Camera myCamera;

	// Token: 0x040003DB RID: 987
	private int _padding;

	// Token: 0x040003DC RID: 988
	private bool _isNormalMap;

	// Token: 0x040003DD RID: 989
	private bool _fixOutOfBoundsUVs;

	// Token: 0x040003DE RID: 990
	private bool _doRenderAtlas;

	// Token: 0x040003DF RID: 991
	private Rect[] rs;

	// Token: 0x040003E0 RID: 992
	private List<MB3_TextureCombiner.MB_TexSet> textureSets;

	// Token: 0x040003E1 RID: 993
	private int indexOfTexSetToRender;

	// Token: 0x040003E2 RID: 994
	private ShaderTextureProperty _texPropertyName;

	// Token: 0x040003E3 RID: 995
	private TextureBlender _resultMaterialTextureBlender;

	// Token: 0x040003E4 RID: 996
	private Texture2D targTex;

	// Token: 0x040003E5 RID: 997
	private MB3_TextureCombiner combiner;
}
