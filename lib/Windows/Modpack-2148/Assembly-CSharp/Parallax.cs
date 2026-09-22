using System;
using UnityEngine;

// Token: 0x02000061 RID: 97
[ExecuteInEditMode]
public class Parallax : MonoBehaviour
{
	// Token: 0x06000243 RID: 579 RVA: 0x00023740 File Offset: 0x00021940
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

	// Token: 0x06000244 RID: 580 RVA: 0x000237B4 File Offset: 0x000219B4
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

	// Token: 0x06000245 RID: 581 RVA: 0x00003BCE File Offset: 0x00001DCE
	public void StartParallaxing()
	{
		this.doParallax = true;
	}

	// Token: 0x06000246 RID: 582 RVA: 0x00003BD7 File Offset: 0x00001DD7
	public void StopParallaxing()
	{
		this.doParallax = false;
	}

	// Token: 0x04000382 RID: 898
	[Range(0f, 1f)]
	public float distance;

	// Token: 0x04000383 RID: 899
	public Color fogColor = new Color(0.718f, 0.749f, 0.522f, 0.5f);

	// Token: 0x04000384 RID: 900
	private SpriteRenderer ren;

	// Token: 0x04000385 RID: 901
	private MaterialPropertyBlock props;

	// Token: 0x04000386 RID: 902
	public Vector3 AbsolutePos;

	// Token: 0x04000387 RID: 903
	private bool doParallax;
}
