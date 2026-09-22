using System;
using UnityEngine;

// Token: 0x02000006 RID: 6
[ExecuteInEditMode]
public class Fade : MonoBehaviour
{
	// Token: 0x1700000B RID: 11
	// (get) Token: 0x06000019 RID: 25 RVA: 0x00002680 File Offset: 0x00000880
	public Material FadeMaterial
	{
		get
		{
			if (this._FadeMaterial == null)
			{
				this._FadeMaterial = new Material(this._FadeShader);
				this._FadeMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return this._FadeMaterial;
		}
	}

	// Token: 0x0600001A RID: 26 RVA: 0x000026B4 File Offset: 0x000008B4
	private void OnEnable()
	{
		this._FadeShader = Shader.Find("Hidden/Fade");
		if (this._FadeShader == null)
		{
			MonoBehaviour.print("Hidden/Fade #SHADER ERROR#");
		}
	}

	// Token: 0x0600001B RID: 27 RVA: 0x000026DE File Offset: 0x000008DE
	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (this.FadeMaterial)
		{
			this.FadeMaterial.SetColor("_Color", this._Color);
			Graphics.Blit(source, destination, this.FadeMaterial);
			return;
		}
		Graphics.Blit(source, destination);
	}

	// Token: 0x0600001C RID: 28 RVA: 0x00002718 File Offset: 0x00000918
	private void OnDisable()
	{
		global::UnityEngine.Object.DestroyImmediate(this._FadeMaterial);
	}

	// Token: 0x0400000D RID: 13
	public Color _Color = Color.white;

	// Token: 0x0400000E RID: 14
	private Shader _FadeShader;

	// Token: 0x0400000F RID: 15
	[HideInInspector]
	public Material _FadeMaterial;
}
