using System;
using UnityEngine;

// Token: 0x02000028 RID: 40
public class CameraControl : MonoBehaviour
{
	// Token: 0x06000150 RID: 336 RVA: 0x0000D6FD File Offset: 0x0000B8FD
	private void Awake()
	{
		this.loadFinished = false;
	}

	// Token: 0x06000151 RID: 337 RVA: 0x0000D708 File Offset: 0x0000B908
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

	// Token: 0x06000152 RID: 338 RVA: 0x0000D7F2 File Offset: 0x0000B9F2
	public void EnableBlur(bool enable)
	{
	}

	// Token: 0x06000153 RID: 339 RVA: 0x0000D7F4 File Offset: 0x0000B9F4
	public void FadeIn()
	{
		this.fadeOutTimer = 0;
		this.fadeInTimer = this.fadeTime + 10;
	}

	// Token: 0x06000154 RID: 340 RVA: 0x0000D80C File Offset: 0x0000BA0C
	public void FadeOut()
	{
		this.fadeInTimer = 0;
		this.fadeOutTimer = this.fadeTime;
	}

	// Token: 0x06000155 RID: 341 RVA: 0x0000D821 File Offset: 0x0000BA21
	private void Update()
	{
	}

	// Token: 0x06000156 RID: 342 RVA: 0x0000D824 File Offset: 0x0000BA24
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

	// Token: 0x06000157 RID: 343 RVA: 0x0000DA64 File Offset: 0x0000BC64
	private Vector3 SuperSmoothLerp(Vector3 x0, Vector3 y0, Vector3 yt, float t, float k)
	{
		Vector3 vector = x0 - y0 + (yt - y0) / (k * t);
		return yt - (yt - y0) / (k * t) + vector * Mathf.Exp(-k * t);
	}

	// Token: 0x06000158 RID: 344 RVA: 0x0000DABC File Offset: 0x0000BCBC
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

	// Token: 0x06000159 RID: 345 RVA: 0x0000DBB0 File Offset: 0x0000BDB0
	public void Teleport(Vector3 newPosition)
	{
		base.transform.position = newPosition + new Vector3(0f, 0f, -20f);
		this.lookaheadPos = 0.3333f * (vector + vector2 + vector3);
		string text = "lookahead:";
		Vector3 vector4 = this.lookaheadPos;
		Debug.Log(text + vector4.ToString());
		this.vel = Vector3.zero;
	}

	// Token: 0x04000212 RID: 530
	public GameObject water;

	// Token: 0x04000213 RID: 531
	public GameObject player;

	// Token: 0x04000214 RID: 532
	public ProgressMeter progressMeter;

	// Token: 0x04000215 RID: 533
	private Vector3 vel;

	// Token: 0x04000216 RID: 534
	private Vector3 target;

	// Token: 0x04000217 RID: 535
	private float waterLevel;

	// Token: 0x04000218 RID: 536
	private int blurTimer;

	// Token: 0x04000219 RID: 537
	public bool loadFinished;

	// Token: 0x0400021A RID: 538
	public Texture2D fadeTex;

	// Token: 0x0400021B RID: 539
	private int fadeInTimer;

	// Token: 0x0400021C RID: 540
	private int fadeOutTimer;

	// Token: 0x0400021D RID: 541
	private int fadeTime = 60;

	// Token: 0x0400021E RID: 542
	private float alpha;

	// Token: 0x0400021F RID: 543
	private Vector3 oldTarget;

	// Token: 0x04000221 RID: 545
	private Camera mainCam;

	// Token: 0x04000222 RID: 546
	private Vector3 lookaheadPos;

	// Token: 0x04000223 RID: 547
	private float lastTF;
}
