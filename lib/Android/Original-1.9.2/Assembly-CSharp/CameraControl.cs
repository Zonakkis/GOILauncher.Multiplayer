using System;
using UnityEngine;

// Token: 0x0200011A RID: 282
public class CameraControl : MonoBehaviour
{
	// Token: 0x0600075C RID: 1884 RVA: 0x0003EA0D File Offset: 0x0003CE0D
	private void OnPreRender()
	{
		RenderSettings.fog = false;
		this.lastFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = 0f;
	}

	// Token: 0x0600075D RID: 1885 RVA: 0x0003EA2A File Offset: 0x0003CE2A
	private void OnPostRender()
	{
		RenderSettings.fogDensity = this.lastFogDensity;
		RenderSettings.fog = true;
	}

	// Token: 0x0600075E RID: 1886 RVA: 0x0003EA40 File Offset: 0x0003CE40
	private void Start()
	{
		this.mainCam = Camera.main;
		this.vel = Vector3.zero;
		this.lookaheadPos = Vector3.zero;
		this.waterLevel = this.water.GetComponent<MeshRenderer>().bounds.max.y;
		if (this.player != null)
		{
		}
		this.target = this.player.transform.position + new Vector3(0f, 0f, -20f);
		this.target.y = Mathf.Max(this.waterLevel + Camera.main.orthographicSize - 2f, this.target.y);
		this.Teleport(this.target);
	}

	// Token: 0x0600075F RID: 1887 RVA: 0x0003EB12 File Offset: 0x0003CF12
	public void EnableBlur(bool enable)
	{
	}

	// Token: 0x06000760 RID: 1888 RVA: 0x0003EB14 File Offset: 0x0003CF14
	private void Update()
	{
		this.lastTF = Mathf.Lerp(this.lastTF, this.lastTFValue, 0.3f);
	}

	// Token: 0x06000761 RID: 1889 RVA: 0x0003EB60 File Offset: 0x0003CF60
	private void FixedUpdate()
	{
		if (Application.isPlaying)
		{
			if (this.player == null)
			{
				this.player = GameObject.FindGameObjectWithTag("Player");
			}
			if (this.lastTF > 1f)
			{
				this.lookaheadPos = this.player.transform.position;
			}
			else
			{
				Vector3 vector4 = 0.3333f * (vector + vector2 + vector3);
				this.lookaheadPos = Vector3.Lerp(this.lookaheadPos, vector4, 0.3f);
			}
			Vector3 vector5 = this.lookaheadPos - this.player.transform.position;
			vector5.z = 0f;
			this.target = this.player.transform.position + vector5.normalized * 2f;
			this.target.y = Mathf.Max(this.waterLevel + this.mainCam.orthographicSize - 2f, this.target.y);
			this.target.z = -20f;
			float num = 0.001f * Mathf.Sin(Time.time);
			Vector3 vector6 = new Vector3(num, num, 0f);
			this.target += vector6;
			Vector3 vector7 = this.target - base.transform.position;
			this.vel += 60f * vector7 * Time.fixedDeltaTime - 0.12f * this.vel;
			base.transform.position = base.transform.position + this.vel * Time.fixedDeltaTime;
		}
	}

	// Token: 0x06000762 RID: 1890 RVA: 0x0003ED88 File Offset: 0x0003D188
	private Vector3 SuperSmoothLerp(Vector3 x0, Vector3 y0, Vector3 yt, float t, float k)
	{
		Vector3 vector = x0 - y0 + (yt - y0) / (k * t);
		return yt - (yt - y0) / (k * t) + vector * Mathf.Exp(-k * t);
	}

	// Token: 0x06000763 RID: 1891 RVA: 0x0003EDE0 File Offset: 0x0003D1E0
	public void Teleport(Vector3 newPosition)
	{
		base.transform.position = newPosition;
		Time.timeScale = 0.01f;
		if (this.lastTF < 0f)
		{
			base.CancelInvoke();
			base.Invoke("Reteleport", 0.01f);
		}
		else
		{
			
		}
		this.vel = Vector3.zero;
	}

	// Token: 0x06000764 RID: 1892 RVA: 0x0003EEE8 File Offset: 0x0003D2E8
	public void Reteleport()
	{
		this.vel = Vector3.zero;
	}

	// Token: 0x04000684 RID: 1668
	public GameObject water;

	// Token: 0x04000685 RID: 1669
	public GameObject player;

	// Token: 0x04000686 RID: 1670
	public ScreenFader fader;

	// Token: 0x04000687 RID: 1671
	public float lastTFValue;

	// Token: 0x04000688 RID: 1672
	private Vector3 vel;

	// Token: 0x04000689 RID: 1673
	private Vector3 target;

	// Token: 0x0400068A RID: 1674
	private float waterLevel;

	// Token: 0x0400068B RID: 1675
	private int blurTimer;

	// Token: 0x0400068D RID: 1677
	private Camera mainCam;

	// Token: 0x0400068E RID: 1678
	private Vector3 lookaheadPos;

	// Token: 0x0400068F RID: 1679
	private float lastTF;

	// Token: 0x04000690 RID: 1680
	private float lastFogDensity;
}
