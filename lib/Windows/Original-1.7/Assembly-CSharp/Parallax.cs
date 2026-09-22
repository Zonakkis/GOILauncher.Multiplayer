using System;
using UnityEngine;

// Token: 0x0200003D RID: 61
[ExecuteInEditMode]
public class Parallax : MonoBehaviour
{
	// Token: 0x060001CD RID: 461 RVA: 0x00011910 File Offset: 0x0000FB10
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

	// Token: 0x060001CE RID: 462 RVA: 0x00011984 File Offset: 0x0000FB84
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

	// Token: 0x060001CF RID: 463 RVA: 0x00011A96 File Offset: 0x0000FC96
	public void StartParallaxing()
	{
		this.doParallax = true;
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x00011A9F File Offset: 0x0000FC9F
	public void StopParallaxing()
	{
		this.doParallax = false;
	}

	// Token: 0x040002E4 RID: 740
	[Range(0f, 1f)]
	public float distance;

	// Token: 0x040002E5 RID: 741
	public Color fogColor = new Color(0.718f, 0.749f, 0.522f, 0.5f);

	// Token: 0x040002E6 RID: 742
	private SpriteRenderer ren;

	// Token: 0x040002E7 RID: 743
	private MaterialPropertyBlock props;

	// Token: 0x040002E8 RID: 744
	public Vector3 AbsolutePos;

	// Token: 0x040002E9 RID: 745
	private bool doParallax;
}
