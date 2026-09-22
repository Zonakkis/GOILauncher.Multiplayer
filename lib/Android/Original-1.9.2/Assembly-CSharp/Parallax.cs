using System;
using UnityEngine;

// Token: 0x02000135 RID: 309
[ExecuteInEditMode]
public class Parallax : MonoBehaviour
{
	// Token: 0x060007DF RID: 2015 RVA: 0x00043D2C File Offset: 0x0004212C
	private void Start()
	{
		this.ren = base.GetComponent<SpriteRenderer>();
		this.props = new MaterialPropertyBlock();
		this.ren.GetPropertyBlock(this.props);
		this.fogColor.a = this.distance;
		this.props.SetColor("_FogColor", this.fogColor);
		this.ren.SetPropertyBlock(this.props);
		this.doParallax = true;
	}

	// Token: 0x060007E0 RID: 2016 RVA: 0x00043DA0 File Offset: 0x000421A0
	private void LateUpdate()
	{
		Color color = new Color(this.fogColor.r, this.fogColor.g, this.fogColor.b, this.distance);
		if (this.props == null)
		{
			this.props = new MaterialPropertyBlock();
			this.ren.GetPropertyBlock(this.props);
		}
		this.ren.sortingOrder = 5 + (int)((1f - this.distance) * 100f);
		if (this.props.GetVector("_FogColor") != color)
		{
			this.props.SetColor("_FogColor", color);
			this.ren.SetPropertyBlock(this.props);
		}
		if (Application.isPlaying)
		{
			this.doParallax = true;
		}
		if (this.doParallax)
		{
			Vector3 position = Camera.main.transform.position;
			position.z = 0f;
			base.transform.position = this.AbsolutePos + position * this.distance;
		}
	}

	// Token: 0x060007E1 RID: 2017 RVA: 0x00043EBE File Offset: 0x000422BE
	public void StartParallaxing()
	{
		this.doParallax = true;
	}

	// Token: 0x060007E2 RID: 2018 RVA: 0x00043EC7 File Offset: 0x000422C7
	public void StopParallaxing()
	{
		this.doParallax = false;
	}

	// Token: 0x04000765 RID: 1893
	[Range(0f, 1f)]
	public float distance;

	// Token: 0x04000766 RID: 1894
	public Color fogColor = new Color(0.718f, 0.749f, 0.522f, 0.5f);

	// Token: 0x04000767 RID: 1895
	private SpriteRenderer ren;

	// Token: 0x04000768 RID: 1896
	private MaterialPropertyBlock props;

	// Token: 0x04000769 RID: 1897
	public Vector3 AbsolutePos;

	// Token: 0x0400076A RID: 1898
	private bool doParallax;
}
