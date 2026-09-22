using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000FA RID: 250
public class ScreenFader : MonoBehaviour
{
	// Token: 0x060006AB RID: 1707 RVA: 0x0003AE84 File Offset: 0x00039284
	private void Awake()
	{
		this.rend.transform.localScale = new Vector3(2000f, 2000f, 1f);
		ScreenFader.mat = this.rend.sharedMaterial;
		Shader.EnableKeyword("_Color");
		ScreenFader.shaderInt = Shader.PropertyToID("_Color");
		ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.black);
		this.rend.enabled = true;
	}

	// Token: 0x060006AC RID: 1708 RVA: 0x0003AEFE File Offset: 0x000392FE
	private void Start()
	{
		if (this.saveManager == null)
		{
			this.saveManager = GameObject.FindGameObjectWithTag("Player").GetComponent<Saviour>();
		}
	}

	// Token: 0x060006AD RID: 1709 RVA: 0x0003AF28 File Offset: 0x00039328
	private void FadeToClear()
	{
		this.prevColor = ScreenFader.mat.GetColor(ScreenFader.shaderInt);
		ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.Lerp(this.prevColor, Color.clear, this.fadeInProgress / this.fadeDuration));
	}

	// Token: 0x060006AE RID: 1710 RVA: 0x0003AF80 File Offset: 0x00039380
	private void FadeToBlack()
	{
		this.prevColor = ScreenFader.mat.GetColor(ScreenFader.shaderInt);
		ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.Lerp(this.prevColor, Color.black, this.fadeOutProgress / this.fadeDuration));
	}

	// Token: 0x060006AF RID: 1711 RVA: 0x0003AFD8 File Offset: 0x000393D8
	public void StartScene()
	{
		if (!this.fadingOut)
		{
			this.fadeInProgress = 0f;
			this.fadeInStartTime = Time.realtimeSinceStartup;
			ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.black);
		}
		else
		{
			this.fadeInInterrupted = true;
		}
		this.fadingIn = true;
		base.StartCoroutine("StartSceneRoutine");
	}

	// Token: 0x060006B0 RID: 1712 RVA: 0x0003B03C File Offset: 0x0003943C
	private IEnumerator StartSceneRoutine()
	{
		this.rend.enabled = true;
		for (;;)
		{
			if (this.fadingOut)
			{
				yield return null;
			}
			if (this.fadeInInterrupted)
			{
				this.fadeInInterrupted = false;
				this.fadeInProgress = 0f;
				this.fadeInStartTime = Time.realtimeSinceStartup;
				this.fadingIn = true;
				ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.black);
			}
			this.FadeToClear();
			this.fadeInProgress = Time.realtimeSinceStartup - this.fadeInStartTime;
			if (this.fadeInProgress >= 1f)
			{
				break;
			}
			yield return null;
		}
		ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.clear);
		this.rend.enabled = false;
		this.fadingIn = false;
		this.fadeInProgress = 0f;
		Time.timeScale = 1f;
		yield break;
		yield break;
	}

	// Token: 0x060006B1 RID: 1713 RVA: 0x0003B058 File Offset: 0x00039458
	public IEnumerator EndSceneRoutine(ScreenFader.ScreenFaderExitType exit)
	{
		for (;;)
		{
			if (this.fadingIn)
			{
				yield return null;
			}
			if (this.fadeOutInterrupted)
			{
				this.fadeOutInterrupted = false;
				this.fadeOutStartTime = Time.realtimeSinceStartup;
				this.fadeOutProgress = 0f;
				this.rend.enabled = true;
				ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.clear);
			}
			this.FadeToBlack();
			this.fadeOutProgress = Time.realtimeSinceStartup - this.fadeOutStartTime;
			if (this.fadeOutProgress >= 1f)
			{
				break;
			}
			yield return null;
		}
		if (exit == ScreenFader.ScreenFaderExitType.ReloadMain)
		{
			SceneManager.LoadScene("Mian");
		}
		else if (exit == ScreenFader.ScreenFaderExitType.LoadReward)
		{
			SceneManager.LoadScene("Reward Loader Mobile");
		}
		else if (exit == ScreenFader.ScreenFaderExitType.ResetPlayerButNotDialogue)
		{
			this.saveManager.ResetPlayerButNotDialogue();
		}
		yield break;
		yield break;
	}

	// Token: 0x060006B2 RID: 1714 RVA: 0x0003B07C File Offset: 0x0003947C
	public void EndScene(ScreenFader.ScreenFaderExitType exit)
	{
		if (!this.fadingIn)
		{
			this.fadeOutStartTime = Time.realtimeSinceStartup;
			this.fadeOutProgress = 0f;
			this.rend.enabled = true;
			this.fadeOutInterrupted = false;
			ScreenFader.mat.SetColor(ScreenFader.shaderInt, Color.clear);
		}
		else
		{
			this.fadeOutInterrupted = true;
		}
		this.fadingOut = true;
		base.StartCoroutine("EndSceneRoutine", exit);
	}

	// Token: 0x040005E6 RID: 1510
	public MeshRenderer rend;

	// Token: 0x040005E7 RID: 1511
	public TitleFade Title;

	// Token: 0x040005E8 RID: 1512
	public Saviour saveManager;

	// Token: 0x040005E9 RID: 1513
	public float fadeDuration = 1f;

	// Token: 0x040005EA RID: 1514
	private float fadeInStartTime;

	// Token: 0x040005EB RID: 1515
	private float fadeOutStartTime;

	// Token: 0x040005EC RID: 1516
	private float fadeInProgress;

	// Token: 0x040005ED RID: 1517
	private float fadeOutProgress;

	// Token: 0x040005EE RID: 1518
	private static Material mat;

	// Token: 0x040005EF RID: 1519
	private static int shaderInt;

	// Token: 0x040005F0 RID: 1520
	private bool fadingIn;

	// Token: 0x040005F1 RID: 1521
	private bool fadingOut;

	// Token: 0x040005F2 RID: 1522
	private bool fadeInInterrupted;

	// Token: 0x040005F3 RID: 1523
	private bool fadeOutInterrupted;

	// Token: 0x040005F4 RID: 1524
	private Color32 prevColor;

	// Token: 0x020000FB RID: 251
	public enum ScreenFaderExitType
	{
		// Token: 0x040005F6 RID: 1526
		ReloadMain,
		// Token: 0x040005F7 RID: 1527
		ResetPlayerButNotDialogue,
		// Token: 0x040005F8 RID: 1528
		LoadReward,
		// Token: 0x040005F9 RID: 1529
		Default
	}
}
