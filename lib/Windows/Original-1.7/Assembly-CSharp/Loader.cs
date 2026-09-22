using System;
using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000039 RID: 57
public class Loader : MonoBehaviour
{
	// Token: 0x06000199 RID: 409 RVA: 0x0000FAA8 File Offset: 0x0000DCA8
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

	// Token: 0x0600019A RID: 410 RVA: 0x0000FCE1 File Offset: 0x0000DEE1
	public void Resume()
	{
		this.mouseQuery.SetActive(false);
		this.languageBar.SetActive(true);
		this.languageBar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
	}

	// Token: 0x0600019B RID: 411 RVA: 0x0000FD14 File Offset: 0x0000DF14
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

	// Token: 0x0600019C RID: 412 RVA: 0x0000FF2E File Offset: 0x0000E12E
	public void SaveTrackpadSetting(bool trackpad)
	{
		PlayerPrefs.SetInt("Trackpad", trackpad ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x0600019D RID: 413 RVA: 0x0000FF46 File Offset: 0x0000E146
	private void LoadReady()
	{
		this.loadFinished = true;
	}

	// Token: 0x0600019E RID: 414 RVA: 0x0000FF50 File Offset: 0x0000E150
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

	// Token: 0x0600019F RID: 415 RVA: 0x0000FFAC File Offset: 0x0000E1AC
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

	// Token: 0x060001A0 RID: 416 RVA: 0x00010013 File Offset: 0x0000E213
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

	// Token: 0x060001A1 RID: 417 RVA: 0x00010022 File Offset: 0x0000E222
	public void HideSettings()
	{
		base.StartCoroutine("ShuntEverythingRight");
		this.hammerAnim.Play("HammerUp");
	}

	// Token: 0x060001A2 RID: 418 RVA: 0x00010040 File Offset: 0x0000E240
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

	// Token: 0x060001A3 RID: 419 RVA: 0x00010050 File Offset: 0x0000E250
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

	// Token: 0x060001A4 RID: 420 RVA: 0x000100AC File Offset: 0x0000E2AC
	public void DoCredits()
	{
		SceneManager.LoadScene("Credits");
	}

	// Token: 0x060001A5 RID: 421 RVA: 0x000100B8 File Offset: 0x0000E2B8
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

	// Token: 0x060001A6 RID: 422 RVA: 0x00010128 File Offset: 0x0000E328
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

	// Token: 0x060001A7 RID: 423 RVA: 0x0001019C File Offset: 0x0000E39C
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

	// Token: 0x060001A8 RID: 424 RVA: 0x000101B2 File Offset: 0x0000E3B2
	public void DoQuit()
	{
		Application.Quit();
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x000101B9 File Offset: 0x0000E3B9
	public void DoStart()
	{
		this.async.allowSceneActivation = true;
		if (this.async.isDone)
		{
			this.variants.WarmUp();
			SceneManager.SetActiveScene(SceneManager.GetSceneByName("Mian"));
		}
	}

	// Token: 0x060001AA RID: 426 RVA: 0x000101EF File Offset: 0x0000E3EF
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

	// Token: 0x060001AB RID: 427 RVA: 0x000101FE File Offset: 0x0000E3FE
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

	// Token: 0x060001AC RID: 428 RVA: 0x0001020D File Offset: 0x0000E40D
	private IEnumerator RevealTitle()
	{
		for (float t = 0f; t <= 1.0001f; t += 0.15f)
		{
			this.titleMask.sizeDelta = new Vector2(700f * t, 216f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x060001AD RID: 429 RVA: 0x0001021C File Offset: 0x0000E41C
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

	// Token: 0x060001AE RID: 430 RVA: 0x0001022B File Offset: 0x0000E42B
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

	// Token: 0x060001AF RID: 431 RVA: 0x0001023C File Offset: 0x0000E43C
	public void EndIntroAnim()
	{
		base.StartCoroutine("RevealMenu");
		base.StartCoroutine("RevealTitle");
		this.hammerStartPos = this.hammer.transform.position;
		this.rockStartPos = this.rock.transform.position;
		this.titleStartPos = this.titleMask.position;
		base.GetComponent<AudioSource>().PlayOneShot(this.attachSound);
	}

	// Token: 0x060001B0 RID: 432 RVA: 0x000102AF File Offset: 0x0000E4AF
	public void SetLanguage(int newLang)
	{
		LocalizationManager.CurrentLanguage = LocalizationManager.GetAllLanguages(true)[newLang];
	}

	// Token: 0x04000293 RID: 659
	private float progress;

	// Token: 0x04000294 RID: 660
	private float lerpProgress;

	// Token: 0x04000295 RID: 661
	private bool loadFinished;

	// Token: 0x04000296 RID: 662
	private AsyncOperation async;

	// Token: 0x04000297 RID: 663
	public Image humble;

	// Token: 0x04000298 RID: 664
	public Button continueButton;

	// Token: 0x04000299 RID: 665
	public Button newgameButton;

	// Token: 0x0400029A RID: 666
	public Image continueBar;

	// Token: 0x0400029B RID: 667
	public Image newgameBar;

	// Token: 0x0400029C RID: 668
	public Gradient loadingGradient;

	// Token: 0x0400029D RID: 669
	public Image fader;

	// Token: 0x0400029E RID: 670
	private bool canContinue;

	// Token: 0x0400029F RID: 671
	public ParticleSystem scrapeParticles;

	// Token: 0x040002A0 RID: 672
	public Animator hammerAnim;

	// Token: 0x040002A1 RID: 673
	public RectTransform menu;

	// Token: 0x040002A2 RID: 674
	public AnimationCurve appearCurve;

	// Token: 0x040002A3 RID: 675
	public RectTransform titleMask;

	// Token: 0x040002A4 RID: 676
	public Transform rock;

	// Token: 0x040002A5 RID: 677
	public Transform hammer;

	// Token: 0x040002A6 RID: 678
	private Vector3 rockStartPos;

	// Token: 0x040002A7 RID: 679
	private Vector3 hammerStartPos;

	// Token: 0x040002A8 RID: 680
	private Vector3 titleStartPos;

	// Token: 0x040002A9 RID: 681
	private bool introAnimDone;

	// Token: 0x040002AA RID: 682
	private int menuItemClicked;

	// Token: 0x040002AB RID: 683
	public GameObject fog;

	// Token: 0x040002AC RID: 684
	public AudioClip attachSound;

	// Token: 0x040002AD RID: 685
	public AudioClip releaseSound;

	// Token: 0x040002AE RID: 686
	public GameObject settingsMenuLeft;

	// Token: 0x040002AF RID: 687
	public GameObject settingsMenuRight;

	// Token: 0x040002B0 RID: 688
	public GameObject mouseQuery;

	// Token: 0x040002B1 RID: 689
	private bool shouldShowMouseQuery;

	// Token: 0x040002B2 RID: 690
	private bool hideLogo;

	// Token: 0x040002B3 RID: 691
	private bool safeToClick;

	// Token: 0x040002B4 RID: 692
	public GameObject languageBar;

	// Token: 0x040002B5 RID: 693
	public ShaderVariantCollection variants;
}
