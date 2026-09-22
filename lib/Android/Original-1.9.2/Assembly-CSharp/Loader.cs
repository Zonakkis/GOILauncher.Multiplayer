using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200012F RID: 303
public class Loader : MonoBehaviour
{
	// Token: 0x060007AA RID: 1962 RVA: 0x00041158 File Offset: 0x0003F558
	private void Start()
	{
		this.safeToClick = false;
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
		this.menuItemClicked = -1;
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
		this.titleMask.sizeDelta = new Vector2(0f, 216f);
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		this.safeToClick = true;
		this.hammerAnim.Play("HammerUp");
	}

	// Token: 0x060007AB RID: 1963 RVA: 0x000412E7 File Offset: 0x0003F6E7
	public void Resume()
	{
		this.mouseQuery.SetActive(false);
	}

	// Token: 0x060007AC RID: 1964 RVA: 0x000412F8 File Offset: 0x0003F6F8
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

	// Token: 0x060007AD RID: 1965 RVA: 0x00041528 File Offset: 0x0003F928
	public void SaveTrackpadSetting(bool trackpad)
	{
		PlayerPrefs.SetInt("Trackpad", (!trackpad) ? 0 : 1);
		PlayerPrefs.Save();
	}

	// Token: 0x060007AE RID: 1966 RVA: 0x00041548 File Offset: 0x0003F948
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

	// Token: 0x060007AF RID: 1967 RVA: 0x000415B0 File Offset: 0x0003F9B0
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

	// Token: 0x060007B0 RID: 1968 RVA: 0x00041624 File Offset: 0x0003FA24
	private IEnumerator ShuntEverythingLeft()
	{
		base.StopCoroutine("RevealMenu");
		TextMeshProUGUI[] items = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 0f; t <= 1.0001f; t += 0.05f)
		{
			this.titleMask.position = this.titleStartPos + new Vector3(Mathf.SmoothStep(0f, -10f, t), 0f, 0f);
			this.rock.position = this.rockStartPos + new Vector3(Mathf.SmoothStep(0f, -2f, t), 0f, 0f);
			for (int i = 0; i < items.Length; i++)
			{
				Color c = items[i].color;
				c.a = Mathf.Clamp01(1f - t);
				c.a *= c.a;
				items[i].color = c;
			}
			yield return null;
		}
		this.settingsMenuLeft.SetActive(true);
		this.settingsMenuRight.SetActive(true);
		this.introAnimDone = false;
		yield break;
	}

	// Token: 0x060007B1 RID: 1969 RVA: 0x0004163F File Offset: 0x0003FA3F
	public void HideSettings()
	{
		base.StartCoroutine("ShuntEverythingRight");
		this.hammerAnim.Play("HammerUp");
	}

	// Token: 0x060007B2 RID: 1970 RVA: 0x00041660 File Offset: 0x0003FA60
	private IEnumerator ShuntEverythingRight()
	{
		this.titleMask.sizeDelta = new Vector2(0f, 216f);
		this.settingsMenuLeft.SetActive(false);
		this.settingsMenuRight.SetActive(false);
		for (float t = 1f; t >= -0.0001f; t -= 0.05f)
		{
			this.titleMask.position = this.titleStartPos + new Vector3(Mathf.SmoothStep(0f, -900f, t), 0f, 0f);
			this.rock.position = this.rockStartPos + new Vector3(Mathf.SmoothStep(0f, -2f, t), 0f, 0f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x060007B3 RID: 1971 RVA: 0x0004167C File Offset: 0x0003FA7C
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

	// Token: 0x060007B4 RID: 1972 RVA: 0x000416E4 File Offset: 0x0003FAE4
	public void DoCredits()
	{
		SceneManager.LoadScene("Credits");
	}

	// Token: 0x060007B5 RID: 1973 RVA: 0x000416F0 File Offset: 0x0003FAF0
	public void ContinueGame()
	{
		SceneManager.LoadScene("Mian");
	}

	// Token: 0x060007B6 RID: 1974 RVA: 0x000416FC File Offset: 0x0003FAFC
	public void StartGame()
	{
		SceneManager.LoadScene("Mian");
	}

	// Token: 0x060007B7 RID: 1975 RVA: 0x00041708 File Offset: 0x0003FB08
	private IEnumerator FadeOutAnd(string cbName)
	{
		this.hammerAnim.Play("HammerDown");
		base.GetComponent<AudioSource>().PlayOneShot(this.releaseSound);
		this.scrapeParticles.Play();
		for (float f = 0f; f <= 1.001f; f += 0.05f)
		{
			Color c = this.fader.color;
			c.a = f;
			this.fader.color = c;
			TextMeshProUGUI[] items = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
			for (int i = 0; i < items.Length; i++)
			{
				if (i != this.menuItemClicked)
				{
					Color color = items[i].color;
					color.a = this.appearCurve.Evaluate(1f - f);
					items[i].color = color;
				}
			}
			yield return null;
		}
		this.scrapeParticles.gameObject.SetActive(false);
		this.fog.SetActive(false);
		base.SendMessage(cbName);
		yield break;
	}

	// Token: 0x060007B8 RID: 1976 RVA: 0x0004172A File Offset: 0x0003FB2A
	public void DoQuit()
	{
		Application.Quit();
	}

	// Token: 0x060007B9 RID: 1977 RVA: 0x00041734 File Offset: 0x0003FB34
	private IEnumerator FadeInTitle()
	{
		while (this.mouseQuery.activeSelf)
		{
			yield return null;
		}
		this.fader.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			Color c = this.fader.color;
			c.a = f;
			this.fader.color = c;
			yield return null;
		}
		this.safeToClick = true;
		this.hammerAnim.Play("HammerUp");
		yield break;
	}

	// Token: 0x060007BA RID: 1978 RVA: 0x00041750 File Offset: 0x0003FB50
	private IEnumerator RevealMenu()
	{
		TextMeshProUGUI[] items = this.menu.GetComponentsInChildren<TextMeshProUGUI>();
		for (float t = 0f; t <= 1.0001f + 0.1f * ((float)items.Length - 1f); t += 0.01f)
		{
			Color barColor = this.loadingGradient.Evaluate(this.lerpProgress);
			for (int i = 0; i < items.Length; i++)
			{
				float num = this.appearCurve.Evaluate(Mathf.Clamp01(t - (float)i * 0.1f));
				Color color = items[i].color;
				color.a = num;
				items[i].color = color;
				if (items[i].transform.parent == this.continueBar.transform.parent)
				{
					this.continueBar.color = barColor;
				}
				else if (items[i].transform.parent == this.newgameBar.transform.parent)
				{
					this.newgameBar.color = barColor;
				}
			}
			yield return null;
		}
		this.introAnimDone = true;
		yield break;
	}

	// Token: 0x060007BB RID: 1979 RVA: 0x0004176C File Offset: 0x0003FB6C
	private IEnumerator RevealTitle()
	{
		for (float t = 0f; t <= 1.0001f; t += 0.15f)
		{
			this.titleMask.sizeDelta = new Vector2(700f * t, 216f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x060007BC RID: 1980 RVA: 0x00041788 File Offset: 0x0003FB88
	private IEnumerator FadeInHumble()
	{
		yield return null;
		if (this.shouldShowMouseQuery)
		{
			PlayerPrefs.SetInt("Trackpad", 1);
			GameObject.FindWithTag("SettingsManager").GetComponent<SettingsManager>().SetMouseSensitivity(1f);
		}
		base.StartCoroutine("FadeInTitle");
		this.humble.gameObject.SetActive(false);
		yield break;
	}

	// Token: 0x060007BD RID: 1981 RVA: 0x000417A4 File Offset: 0x0003FBA4
	public void EndIntroAnim()
	{
		base.StartCoroutine("RevealMenu");
		base.StartCoroutine("RevealTitle");
		this.rockStartPos = this.rock.transform.position;
		this.titleStartPos = this.titleMask.position;
		base.GetComponent<AudioSource>().PlayOneShot(this.attachSound);
	}

	// Token: 0x060007BE RID: 1982 RVA: 0x00041804 File Offset: 0x0003FC04
	public void SetLanguage(int newLang)
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages(true);
		LocalizationManager.CurrentLanguage = allLanguages[newLang];
	}

	// Token: 0x0400070F RID: 1807
	private float progress;

	// Token: 0x04000710 RID: 1808
	private float lerpProgress;

	// Token: 0x04000711 RID: 1809
	public Image humble;

	// Token: 0x04000712 RID: 1810
	public Button continueButton;

	// Token: 0x04000713 RID: 1811
	public Button newgameButton;

	// Token: 0x04000714 RID: 1812
	public Image continueBar;

	// Token: 0x04000715 RID: 1813
	public Image newgameBar;

	// Token: 0x04000716 RID: 1814
	public Gradient loadingGradient;

	// Token: 0x04000717 RID: 1815
	public Image fader;

	// Token: 0x04000718 RID: 1816
	private bool canContinue;

	// Token: 0x04000719 RID: 1817
	public ParticleSystem scrapeParticles;

	// Token: 0x0400071A RID: 1818
	public Animator hammerAnim;

	// Token: 0x0400071B RID: 1819
	public RectTransform menu;

	// Token: 0x0400071C RID: 1820
	public AnimationCurve appearCurve;

	// Token: 0x0400071D RID: 1821
	public RectTransform titleMask;

	// Token: 0x0400071E RID: 1822
	public Transform rock;

	// Token: 0x0400071F RID: 1823
	public Transform hammer;

	// Token: 0x04000720 RID: 1824
	private Vector3 rockStartPos;

	// Token: 0x04000721 RID: 1825
	private Vector3 titleStartPos;

	// Token: 0x04000722 RID: 1826
	private bool introAnimDone;

	// Token: 0x04000723 RID: 1827
	private int menuItemClicked;

	// Token: 0x04000724 RID: 1828
	public GameObject fog;

	// Token: 0x04000725 RID: 1829
	public AudioClip attachSound;

	// Token: 0x04000726 RID: 1830
	public AudioClip releaseSound;

	// Token: 0x04000727 RID: 1831
	public GameObject settingsMenuLeft;

	// Token: 0x04000728 RID: 1832
	public GameObject settingsMenuRight;

	// Token: 0x04000729 RID: 1833
	public GameObject mouseQuery;

	// Token: 0x0400072A RID: 1834
	private bool shouldShowMouseQuery;

	// Token: 0x0400072B RID: 1835
	private bool safeToClick;
}
