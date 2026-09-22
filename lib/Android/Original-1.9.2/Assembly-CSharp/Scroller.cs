using System;
using UnityEngine;

// Token: 0x02000144 RID: 324
public class Scroller : MonoBehaviour
{
	// Token: 0x060008FD RID: 2301 RVA: 0x00049CD8 File Offset: 0x000480D8
	private void Start()
	{
		if (this.canvas == null)
		{
			this.canvas = base.transform.parent.GetComponentInParent<RectTransform>();
		}
		if (this.fader == null)
		{
			this.fader = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
		this.myRect = base.GetComponent<RectTransform>();
		this.aud = base.GetComponent<AudioSource>();
		this.aud.clip = this.endgameClip;
		this.scrollDelta = 0.0071f;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Narrator");
		if (gameObject != null)
		{
			this.narrator = gameObject.GetComponent<Narrator>();
		}
	}

	// Token: 0x060008FE RID: 2302 RVA: 0x00049D8C File Offset: 0x0004818C
	private void Update()
	{
		Vector3 position = base.transform.position;
		position.y += this.scrollDelta * 60f * Time.deltaTime;
		base.transform.position = position;
		float y = this.myRect.TransformPoint(this.myRect.rect.min).y;
		float y2 = this.canvas.TransformPoint(this.canvas.rect.max).y;
		if (y > y2 && !this.didTriggerEndScene)
		{
			this.aud.Stop();
			this.fader.EndScene(ScreenFader.ScreenFaderExitType.LoadReward);
			this.didTriggerEndScene = true;
		}
	}

	// Token: 0x0400087A RID: 2170
	private RectTransform canvas;

	// Token: 0x0400087B RID: 2171
	private RectTransform myRect;

	// Token: 0x0400087C RID: 2172
	public bool inGame;

	// Token: 0x0400087D RID: 2173
	private float scrollDelta;

	// Token: 0x0400087E RID: 2174
	private AudioSource aud;

	// Token: 0x0400087F RID: 2175
	public Texture2D fadeTex;

	// Token: 0x04000880 RID: 2176
	public Narrator narrator;

	// Token: 0x04000881 RID: 2177
	public AudioClip endgameClip;

	// Token: 0x04000882 RID: 2178
	public AudioClip menuClip;

	// Token: 0x04000883 RID: 2179
	private ScreenFader fader;

	// Token: 0x04000884 RID: 2180
	private bool didTriggerEndScene;
}
