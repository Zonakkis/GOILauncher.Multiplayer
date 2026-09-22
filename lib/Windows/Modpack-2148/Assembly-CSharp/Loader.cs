using System;
using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000050 RID: 80
public class Loader : MonoBehaviour
{
	// Token: 0x060001C9 RID: 457 RVA: 0x00020D54 File Offset: 0x0001EF54
	private void Start()
	{
		this.safeToClick = false;
		this.hideLogo = PlayerPrefs.GetInt("HideLogo") == 1;
		if (!PlayerPrefs.HasKey("Trackpad"))
		{
			this.shouldShowMouseQuery = true;
		}
		else if (Application.isEditor)
		{
			this.shouldShowMouseQuery = true;
		}
		else
		{
			this.shouldShowMouseQuery = false;
		}
		this.progress = 0f;
		this.lerpProgress = 0f;
		this.loadFinished = false;
		this.menuItemClicked = -1;
		base.StartCoroutine(this.LoadNewScene());
		this.canContinue = false;
		if (PlayerPrefs.GetString("SaveGame0").Length > 0)
		{
			this.canContinue = true;
		}
		if (PlayerPrefs.GetString("SaveGame1").Length > 0)
		{
			this.canContinue = true;
		}
		TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Color color = componentsInChildren[i].color;
			color.a = 0f;
			componentsInChildren[i].color = color;
		}
		this.continueBar.color = new Color(0f, 0f, 0f, 0f);
		this.newgameBar.color = new Color(0f, 0f, 0f, 0f);
		this.scrapeParticles.gameObject.SetActive(true);
		this.fog.SetActive(true);
		this.introAnimDone = false;
		this.humble.gameObject.SetActive(true);
		this.titleMask.sizeDelta = new Vector2(0f, 216f);
		if (this.hideLogo)
		{
			this.fader.color = new Color(0f, 0f, 0f, 1f);
			this.humble.gameObject.SetActive(false);
			if (this.shouldShowMouseQuery)
			{
				this.mouseQuery.SetActive(true);
				this.languageBar.SetActive(true);
				this.languageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 826.7f);
			}
			base.StartCoroutine("FadeInTitle");
		}
		else
		{
			base.StartCoroutine("FadeInHumble");
			PlayerPrefs.SetInt("HideLogo", 1);
			PlayerPrefs.Save();
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	// Token: 0x060001CA RID: 458 RVA: 0x000037D2 File Offset: 0x000019D2
	public void Resume()
	{
		this.mouseQuery.SetActive(false);
		this.languageBar.SetActive(true);
		this.languageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
	}

	// Token: 0x060001CB RID: 459 RVA: 0x00020F90 File Offset: 0x0001F190
	private void Update()
	{
		if (!this.canContinue)
		{
			this.continueButton.interactable = false;
			this.continueButton.gameObject.SetActive(false);
		}
		this.lerpProgress = Mathf.Lerp(this.lerpProgress, this.progress, 0.1f);
		if (this.progress < 1f)
		{
			if (this.canContinue)
			{
				this.continueButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.LOADING;
				this.continueButton.interactable = true;
				this.continueBar.rectTransform.localScale = new Vector3(this.lerpProgress, 1f, 1f);
				if (this.introAnimDone)
				{
					this.continueBar.color = this.loadingGradient.Evaluate(this.lerpProgress);
				}
			}
			this.newgameButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.LOADING;
			this.newgameButton.interactable = false;
			this.newgameBar.rectTransform.localScale = new Vector3(this.lerpProgress, 1f, 1f);
			if (this.introAnimDone)
			{
				this.newgameBar.color = this.loadingGradient.Evaluate(this.lerpProgress);
				return;
			}
		}
		else
		{
			if (this.canContinue)
			{
				this.continueButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.CONTINUE_GAME;
				this.continueButton.interactable = true;
				this.continueBar.rectTransform.localScale = new Vector3(this.lerpProgress, 1f, 1f);
				if (this.introAnimDone)
				{
					this.continueBar.color = this.loadingGradient.Evaluate(this.lerpProgress);
				}
			}
			this.newgameButton.GetComponentInChildren<TextMeshProUGUI>().text = ScriptLocalization.START_NEW_GAME;
			this.newgameButton.interactable = true;
			this.newgameBar.rectTransform.localScale = new Vector3(this.lerpProgress, 1f, 1f);
			if (this.introAnimDone)
			{
				this.newgameBar.color = this.loadingGradient.Evaluate(this.lerpProgress);
			}
		}
	}

	// Token: 0x060001CC RID: 460 RVA: 0x00003802 File Offset: 0x00001A02
	public void SaveTrackpadSetting(bool trackpad)
	{
		PlayerPrefs.SetInt("Trackpad", trackpad ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x060001CD RID: 461 RVA: 0x0000381A File Offset: 0x00001A1A
	private void LoadReady()
	{
		this.loadFinished = true;
	}

	// Token: 0x060001CE RID: 462 RVA: 0x000211AC File Offset: 0x0001F3AC
	public void QuitGame()
	{
		if (!this.safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == "Quit")
			{
				this.menuItemClicked = i;
			}
		}
		base.StartCoroutine("FadeOutAnd", "DoQuit");
	}

	// Token: 0x060001CF RID: 463 RVA: 0x00021208 File Offset: 0x0001F408
	public void ShowSettings()
	{
		if (!this.safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == "Settings")
			{
				this.menuItemClicked = i;
			}
		}
		base.StartCoroutine("ShuntEverythingLeft");
		this.hammerAnim.Play("HammerDown");
	}

	// Token: 0x060001D0 RID: 464 RVA: 0x00003823 File Offset: 0x00001A23
	private IEnumerator ShuntEverythingLeft()
	{
		base.StopCoroutine("RevealMenu");
		TextMeshProUGUI[] items = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 0f; t <= 1.0001f; t += 0.05f)
		{
			this.titleMask.position = this.titleStartPos + new Vector3(Mathf.SmoothStep(0f, -9f, t), 0f, 0f);
			this.rock.position = this.rockStartPos + new Vector3(Mathf.SmoothStep(0f, -2f, t), 0f, 0f);
			for (int i = 0; i < items.Length; i++)
			{
				Color color = items[i].color;
				color.a = Mathf.Clamp01(1f - t);
				color.a *= color.a;
				items[i].color = color;
			}
			if (!this.languageBar.activeSelf)
			{
				this.languageBar.SetActive(true);
			}
			this.languageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t * 826.7f);
			yield return null;
		}
		this.settingsMenuLeft.SetActive(true);
		this.settingsMenuRight.SetActive(true);
		this.menu.gameObject.SetActive(false);
		this.introAnimDone = false;
		yield break;
	}

	// Token: 0x060001D1 RID: 465 RVA: 0x00003832 File Offset: 0x00001A32
	public void HideSettings()
	{
		base.StartCoroutine("ShuntEverythingRight");
		this.hammerAnim.Play("HammerUp");
	}

	// Token: 0x060001D2 RID: 466 RVA: 0x00003850 File Offset: 0x00001A50
	private IEnumerator ShuntEverythingRight()
	{
		this.titleMask.sizeDelta = new Vector2(0f, 216f);
		this.settingsMenuLeft.SetActive(false);
		this.settingsMenuRight.SetActive(false);
		this.menu.gameObject.SetActive(true);
		this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 1f; t >= -0.0001f; t -= 0.05f)
		{
			this.titleMask.position = this.titleStartPos + new Vector3(Mathf.SmoothStep(0f, -900f, t), 0f, 0f);
			this.rock.position = this.rockStartPos + new Vector3(Mathf.SmoothStep(0f, -2f, t), 0f, 0f);
			this.languageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t * 826.7f);
			yield return null;
		}
		if (this.languageBar.activeSelf)
		{
			this.languageBar.SetActive(false);
		}
		yield break;
	}

	// Token: 0x060001D3 RID: 467 RVA: 0x00021270 File Offset: 0x0001F470
	public void ShowCredits()
	{
		if (!this.safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == "Credits")
			{
				this.menuItemClicked = i;
			}
		}
		base.StartCoroutine("FadeOutAnd", "DoCredits");
	}

	// Token: 0x060001D4 RID: 468 RVA: 0x0000385F File Offset: 0x00001A5F
	public void DoCredits()
	{
		SceneManager.LoadScene("Credits");
	}

	// Token: 0x060001D5 RID: 469 RVA: 0x000212CC File Offset: 0x0001F4CC
	public void ContinueGame()
	{
		if (!this.safeToClick)
		{
			return;
		}
		if (this.loadFinished)
		{
			if (this.menuItemClicked == -1)
			{
				TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (componentsInChildren[i].text == ScriptLocalization.CONTINUE_GAME)
					{
						this.menuItemClicked = i;
					}
				}
			}
			base.StartCoroutine("FadeOutAnd", "DoStart");
		}
	}

	// Token: 0x060001D6 RID: 470 RVA: 0x0002133C File Offset: 0x0001F53C
	public void StartGame()
	{
		if (!this.safeToClick)
		{
			return;
		}
		TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text == ScriptLocalization.START_NEW_GAME)
			{
				this.menuItemClicked = i;
			}
		}
		PlayerPrefs.DeleteKey("NumSaves");
		PlayerPrefs.DeleteKey("SaveGame0");
		PlayerPrefs.DeleteKey("SaveGame1");
		PlayerPrefs.Save();
		this.ContinueGame();
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x0000386B File Offset: 0x00001A6B
	private IEnumerator FadeOutAnd(string cbName)
	{
		this.hammerAnim.Play("HammerDown");
		base.GetComponent<AudioSource>().PlayOneShot(this.releaseSound);
		this.scrapeParticles.Play();
		for (float f = 0f; f <= 1.001f; f += 0.05f)
		{
			Color color = this.fader.color;
			color.a = f;
			this.fader.color = color;
			TextMeshProUGUI[] componentsInChildren = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (i != this.menuItemClicked)
				{
					Color color2 = componentsInChildren[i].color;
					color2.a = this.appearCurve.Evaluate(1f - f);
					componentsInChildren[i].color = color2;
				}
			}
			yield return null;
		}
		this.scrapeParticles.gameObject.SetActive(false);
		this.fog.SetActive(false);
		base.SendMessage(cbName);
		yield break;
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x00003881 File Offset: 0x00001A81
	public void DoQuit()
	{
		Application.Quit();
	}

	// Token: 0x060001D9 RID: 473 RVA: 0x00003888 File Offset: 0x00001A88
	public void DoStart()
	{
		this.async.allowSceneActivation = true;
		if (this.async.isDone)
		{
			this.variants.WarmUp();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName("Mian"));
		}
	}

	// Token: 0x060001DA RID: 474 RVA: 0x000038BE File Offset: 0x00001ABE
	private IEnumerator FadeInTitle()
	{
		while (this.mouseQuery.activeSelf)
		{
			yield return null;
		}
		this.fader.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			Color color = this.fader.color;
			color.a = f;
			this.fader.color = color;
			yield return null;
		}
		this.safeToClick = true;
		this.hammerAnim.Play("HammerUp");
		yield break;
	}

	// Token: 0x060001DB RID: 475 RVA: 0x000038CD File Offset: 0x00001ACD
	private IEnumerator RevealMenu()
	{
		TextMeshProUGUI[] items = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 0f; t <= 1.0001f + 0.1f * ((float)items.Length - 1f); t += 0.01f)
		{
			Color color = this.loadingGradient.Evaluate(this.lerpProgress);
			for (int i = 0; i < items.Length; i++)
			{
				float num = this.appearCurve.Evaluate(Mathf.Clamp01(t - (float)i * 0.1f));
				Color color2 = items[i].color;
				color2.a = num;
				items[i].color = color2;
				if (items[i].transform.parent == this.continueBar.transform.parent)
				{
					this.continueBar.color = color;
				}
				else if (items[i].transform.parent == this.newgameBar.transform.parent)
				{
					this.newgameBar.color = color;
				}
			}
			yield return null;
		}
		this.introAnimDone = true;
		yield break;
	}

	// Token: 0x060001DC RID: 476 RVA: 0x000038DC File Offset: 0x00001ADC
	private IEnumerator RevealTitle()
	{
		for (float t = 0f; t <= 1.0001f; t += 0.15f)
		{
			this.titleMask.sizeDelta = new Vector2(700f * t, 216f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x060001DD RID: 477 RVA: 0x000038EB File Offset: 0x00001AEB
	private IEnumerator FadeInHumble()
	{
		this.humble.gameObject.SetActive(true);
		this.fader.color = new Color(0f, 0f, 0f, 1f);
		this.humble.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 0f; f <= 3.001f; f += 0.05f)
		{
			float num = Mathf.Clamp01(f);
			Color color = new Color(num, num, num, 1f);
			this.fader.color = color;
			this.humble.color = color;
			yield return null;
		}
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			float num2 = Mathf.Clamp01(f);
			this.fader.color = new Color(num2, num2, num2, 1f);
			this.humble.color = new Color(num2, num2, num2, num2);
			yield return null;
		}
		this.fader.color = new Color(0f, 0f, 0f, 1f);
		if (this.shouldShowMouseQuery)
		{
			this.mouseQuery.SetActive(true);
			this.languageBar.SetActive(true);
			this.languageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 826.7f);
		}
		base.StartCoroutine("FadeInTitle");
		this.humble.gameObject.SetActive(false);
		yield break;
	}

	// Token: 0x060001DE RID: 478 RVA: 0x000038FA File Offset: 0x00001AFA
	private IEnumerator LoadNewScene()
	{
		this.async = SceneManager.LoadSceneAsync("Mian");
		this.async.allowSceneActivation = false;
		float t = 0f;
		while (!this.async.isDone)
		{
			t += 0.005f;
			if (Application.isEditor)
			{
				this.progress = Mathf.Min(this.async.progress / 0.9f, t);
			}
			else
			{
				this.progress = this.async.progress / 0.9f;
			}
			if (this.progress >= 0.9f)
			{
				this.LoadReady();
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x060001DF RID: 479 RVA: 0x000213B0 File Offset: 0x0001F5B0
	public void EndIntroAnim()
	{
		base.StartCoroutine("RevealMenu");
		base.StartCoroutine("RevealTitle");
		this.hammerStartPos = this.hammer.transform.position;
		this.rockStartPos = this.rock.transform.position;
		this.titleStartPos = this.titleMask.position;
		base.GetComponent<AudioSource>().PlayOneShot(this.attachSound);
	}

	// Token: 0x060001E0 RID: 480 RVA: 0x00003909 File Offset: 0x00001B09
	public void SetLanguage(int newLang)
	{
		LocalizationManager.CurrentLanguage = LocalizationManager.GetAllLanguages(true)[newLang];
	}

	// Token: 0x040002F9 RID: 761
	private float progress;

	// Token: 0x040002FA RID: 762
	private float lerpProgress;

	// Token: 0x040002FB RID: 763
	private bool loadFinished;

	// Token: 0x040002FC RID: 764
	private AsyncOperation async;

	// Token: 0x040002FD RID: 765
	public Image humble;

	// Token: 0x040002FE RID: 766
	public Button continueButton;

	// Token: 0x040002FF RID: 767
	public Button newgameButton;

	// Token: 0x04000300 RID: 768
	public Image continueBar;

	// Token: 0x04000301 RID: 769
	public Image newgameBar;

	// Token: 0x04000302 RID: 770
	public Gradient loadingGradient;

	// Token: 0x04000303 RID: 771
	public Image fader;

	// Token: 0x04000304 RID: 772
	private bool canContinue;

	// Token: 0x04000305 RID: 773
	public ParticleSystem scrapeParticles;

	// Token: 0x04000306 RID: 774
	public Animator hammerAnim;

	// Token: 0x04000307 RID: 775
	public RectTransform menu;

	// Token: 0x04000308 RID: 776
	public AnimationCurve appearCurve;

	// Token: 0x04000309 RID: 777
	public RectTransform titleMask;

	// Token: 0x0400030A RID: 778
	public Transform rock;

	// Token: 0x0400030B RID: 779
	public Transform hammer;

	// Token: 0x0400030C RID: 780
	private Vector3 rockStartPos;

	// Token: 0x0400030D RID: 781
	private Vector3 hammerStartPos;

	// Token: 0x0400030E RID: 782
	private Vector3 titleStartPos;

	// Token: 0x0400030F RID: 783
	private bool introAnimDone;

	// Token: 0x04000310 RID: 784
	private int menuItemClicked;

	// Token: 0x04000311 RID: 785
	public GameObject fog;

	// Token: 0x04000312 RID: 786
	public AudioClip attachSound;

	// Token: 0x04000313 RID: 787
	public AudioClip releaseSound;

	// Token: 0x04000314 RID: 788
	public GameObject settingsMenuLeft;

	// Token: 0x04000315 RID: 789
	public GameObject settingsMenuRight;

	// Token: 0x04000316 RID: 790
	public GameObject mouseQuery;

	// Token: 0x04000317 RID: 791
	private bool shouldShowMouseQuery;

	// Token: 0x04000318 RID: 792
	private bool hideLogo;

	// Token: 0x04000319 RID: 793
	private bool safeToClick;

	// Token: 0x0400031A RID: 794
	public GameObject languageBar;

	// Token: 0x0400031B RID: 795
	public ShaderVariantCollection variants;
}
