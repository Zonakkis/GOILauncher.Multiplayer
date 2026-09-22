using System;
using UnityEngine;

// Token: 0x0200003D RID: 61
public class CameraControl : MonoBehaviour
{
	// Token: 0x0600017A RID: 378 RVA: 0x000035AE File Offset: 0x000017AE
	private void Awake()
	{
		this.loadFinished = false;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x0001EA6C File Offset: 0x0001CC6C
	private void Start()
	{
		this.mainCam = Camera.main;
		this.vel = Vector3.zero;
		this.lookaheadPos = Vector3.zero;
		this.waterLevel = this.water.GetComponent<MeshRenderer>().bounds.max.y;
		this.player != null;
		this.fadeInTimer = 0;
		this.fadeOutTimer = 0;
		this.target = this.player.transform.position + new Vector3(0f, 0f, -20f);
		this.target.y = Mathf.Max(this.waterLevel + Camera.main.orthographicSize - 2f, this.target.y);
		this.oldTarget = this.target;
		this.alpha = 1f;
		this.FadeIn();
	}

	// Token: 0x0600017C RID: 380 RVA: 0x0000265E File Offset: 0x0000085E
	public void EnableBlur(bool enable)
	{
	}

	// Token: 0x0600017D RID: 381 RVA: 0x000035B7 File Offset: 0x000017B7
	public void FadeIn()
	{
		this.fadeOutTimer = 0;
		this.fadeInTimer = this.fadeTime + 10;
	}

	// Token: 0x0600017E RID: 382 RVA: 0x000035CF File Offset: 0x000017CF
	public void FadeOut()
	{
		this.fadeInTimer = 0;
		this.fadeOutTimer = this.fadeTime;
	}

	// Token: 0x0600017F RID: 383 RVA: 0x0000265E File Offset: 0x0000085E
	private void Update()
	{
	}

	// Token: 0x06000180 RID: 384 RVA: 0x0001EB58 File Offset: 0x0001CD58
	public void FixedUpdate()
	{
		if (!this.loadFinished)
		{
			return;
		}
		if (Application.isPlaying)
		{
			if (this.player == null)
			{
				this.player = GameObject.Find("Player");
			}
			this.lastTF = Mathf.Lerp(this.lastTF, this.progressMeter.currentTF, 0.3f);
			Vector3 vector4 = 0.3333f * (vector + vector2 + vector3);
			this.lookaheadPos = Vector3.Lerp(this.lookaheadPos, vector4, 0.3f);
			Vector3 vector5 = this.lookaheadPos - this.player.transform.position;
			vector5.z = 0f;
			this.target = this.player.transform.position + vector5.normalized * 2f;
			this.target.y = Mathf.Max(this.waterLevel + this.mainCam.orthographicSize - 2f, this.target.y);
			this.target.z = -20f;
			Vector3 vector6 = new Vector3(0.001f * Mathf.Sin(Time.time), 0.001f * Mathf.Sin(Time.time), 0f);
			this.target += vector6;
			Vector3 vector7 = this.target - base.transform.position;
			this.vel += 60f * vector7 * Time.fixedDeltaTime - 0.12f * this.vel;
			base.transform.position = base.transform.position + this.vel * Time.fixedDeltaTime;
		}
	}

	// Token: 0x06000181 RID: 385 RVA: 0x0001ED98 File Offset: 0x0001CF98
	private Vector3 SuperSmoothLerp(Vector3 x0, Vector3 y0, Vector3 yt, float t, float k)
	{
		Vector3 vector = x0 - y0 + (yt - y0) / (k * t);
		return yt - (yt - y0) / (k * t) + vector * Mathf.Exp(-k * t);
	}

	// Token: 0x06000182 RID: 386 RVA: 0x0001EDF0 File Offset: 0x0001CFF0
	private void OnGUI()
	{
		if (this.fadeInTimer > 0)
		{
			this.fadeInTimer--;
			this.alpha = Mathf.Clamp01((float)this.fadeInTimer / (float)this.fadeTime);
			if (this.fadeInTimer == 1)
			{
				Time.timeScale = 1f;
			}
		}
		else if (this.fadeOutTimer > 0)
		{
			this.fadeOutTimer--;
			this.alpha = 1f - (float)this.fadeOutTimer / (float)this.fadeTime;
		}
		if (this.alpha == 0f)
		{
			return;
		}
		GUI.color = new Color(1f - Mathf.Pow(this.alpha, 0.25f), 0f, 0f, this.alpha);
		GUI.depth = -1000;
		GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.fadeTex);
	}

	// Token: 0x06000183 RID: 387 RVA: 0x0001EEE4 File Offset: 0x0001D0E4
	public void Teleport(Vector3 newPosition)
	{
		base.transform.position = newPosition + new Vector3(0f, 0f, -20f);
		this.lookaheadPos = 0.3333f * (vector + vector2 + vector3);
		string text = "lookahead:";
		Vector3 vector4 = this.lookaheadPos;
		Debug.Log(text + vector4.ToString());
		this.vel = Vector3.zero;
	}

	// Token: 0x04000268 RID: 616
	public GameObject water;

	// Token: 0x04000269 RID: 617
	public GameObject player;

	// Token: 0x0400026A RID: 618
	public ProgressMeter progressMeter;

	// Token: 0x0400026B RID: 619
	private Vector3 vel;

	// Token: 0x0400026C RID: 620
	private Vector3 target;

	// Token: 0x0400026D RID: 621
	private float waterLevel;

	// Token: 0x0400026E RID: 622
	private int blurTimer;

	// Token: 0x0400026F RID: 623
	public bool loadFinished;

	// Token: 0x04000270 RID: 624
	public Texture2D fadeTex;

	// Token: 0x04000271 RID: 625
	private int fadeInTimer;

	// Token: 0x04000272 RID: 626
	private int fadeOutTimer;

	// Token: 0x04000273 RID: 627
	private int fadeTime = 60;

	// Token: 0x04000274 RID: 628
	private float alpha;

	// Token: 0x04000275 RID: 629
	private Vector3 oldTarget;

	// Token: 0x04000277 RID: 631
	private Camera mainCam;

	// Token: 0x04000278 RID: 632
	private Vector3 lookaheadPos;

	// Token: 0x04000279 RID: 633
	private float lastTF;
}
