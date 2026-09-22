using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using LetterboxCamera;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200004C RID: 76
public class SettingsManager : MonoBehaviour
{
	// Token: 0x06000264 RID: 612 RVA: 0x000168AC File Offset: 0x00014AAC
	public void SetLanguage(int newLang)
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages(true);
		if (LocalizationManager.HasLanguage(allLanguages[newLang], true, true, true))
		{
			LocalizationManager.CurrentLanguage = allLanguages[newLang];
			Debug.Log("setting language to: " + allLanguages[newLang]);
			return;
		}
		Debug.Log("couldn't set language");
	}

	// Token: 0x06000265 RID: 613 RVA: 0x00016900 File Offset: 0x00014B00
	public void OnLanguageChanged(int newLanguage)
	{
		if (newLanguage == this.currentLanguageNum)
		{
			return;
		}
		this.qualityNames[0] = ScriptLocalization.QUALITY_WORST;
		this.qualityNames[1] = ScriptLocalization.QUALITY_BAD;
		this.qualityNames[2] = ScriptLocalization.QUALITY_MEDIOCRE;
		this.qualityNames[3] = ScriptLocalization.QUALITY_GOOD;
		this.qualityNames[4] = ScriptLocalization.QUALITY_GREAT;
		this.qualityNames[5] = ScriptLocalization.QUALITY_EXTREME;
		this.currentQualityText.text = this.qualityNames[QualitySettings.GetQualityLevel()];
		this.currentLanguageNum = newLanguage;
		PlayerPrefs.SetInt("Language", newLanguage);
		PlayerPrefs.Save();
		if (this.narrator != null)
		{
			this.narrator.SendMessage("SetLanguage", newLanguage);
		}
	}

	// Token: 0x06000266 RID: 614 RVA: 0x000169B8 File Offset: 0x00014BB8
	private void Awake()
	{
		if (this.ratioFitter != null)
		{
			Vector2 vector = new Vector2((float)Screen.width, (float)Screen.height);
			if (vector.x / vector.y >= 1.7f)
			{
				vector.x = 16f;
				vector.y = 9f;
			}
			else
			{
				vector.x = 16f;
				vector.y = 10f;
			}
			this.ratioFitter.ratio = vector;
		}
	}

	// Token: 0x06000267 RID: 615 RVA: 0x00016A38 File Offset: 0x00014C38
	private void Start()
	{
		int num = PlayerPrefs.GetInt("NumWins");
		PlayerPrefs.SetInt("StoredStatsSinceLastPlay", 0);
		if (Application.isEditor)
		{
			num = 0;
		}
		if (SceneManager.GetActiveScene().name == "Mian" && this.potMat)
		{
			float num2 = (float)Mathf.Min(num, 50) / 50f;
			num2 *= num2;
			this.potMat.SetFloat("_Goldness", num2);
		}
		if (SceneManager.GetActiveScene().name == "Mian")
		{
			this.menu.SetActive(true);
		}
		else
		{
			foreach (object obj in this.menu.transform)
			{
				((Transform)obj).gameObject.SetActive(true);
			}
		}
		PlayerPrefs.SetInt("NativeWidth", Screen.currentResolution.width);
		PlayerPrefs.SetInt("NativeHeight", Screen.currentResolution.height);
		PlayerPrefs.SetInt("NativeRefresh", Screen.currentResolution.refreshRate);
		this.canMenu = true;
		Debug.Log("detected system language as :" + Application.systemLanguage.ToString());
		this.currentLanguageNum = -1;
		int num3 = PlayerPrefs.GetInt("Language", -1);
		if (num3 < 0)
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if (systemLanguage <= SystemLanguage.Japanese)
			{
				if (systemLanguage == SystemLanguage.Chinese)
				{
					num3 = 3;
					goto IL_01D9;
				}
				if (systemLanguage == SystemLanguage.English)
				{
					Debug.Log("successfully detected english");
					num3 = 0;
					goto IL_01D9;
				}
				if (systemLanguage == SystemLanguage.Japanese)
				{
					num3 = 2;
					Debug.Log("Detected language as Japanese, weirdly");
					goto IL_01D9;
				}
			}
			else if (systemLanguage <= SystemLanguage.Russian)
			{
				if (systemLanguage == SystemLanguage.Korean)
				{
					num3 = 4;
					goto IL_01D9;
				}
				if (systemLanguage == SystemLanguage.Russian)
				{
					num3 = 1;
					goto IL_01D9;
				}
			}
			else
			{
				if (systemLanguage == SystemLanguage.ChineseSimplified)
				{
					num3 = 3;
					goto IL_01D9;
				}
				if (systemLanguage == SystemLanguage.ChineseTraditional)
				{
					num3 = 3;
					goto IL_01D9;
				}
			}
			num3 = 0;
		}
		IL_01D9:
		this.SetLanguage(num3);
		this.OnLanguageChanged(num3);
		if (this.languageToggles[0] != null)
		{
			for (int i = 0; i < this.languageToggles.Length; i++)
			{
				if (i != num3)
				{
					this.languageToggles[i].isOn = false;
				}
			}
			this.languageToggles[num3].isOn = true;
		}
		if (this.motionBlurToggle != null)
		{
			if (PlayerPrefs.HasKey("MotionBlur"))
			{
				this.motionBlurToggle.isOn = PlayerPrefs.GetInt("MotionBlur") == 1;
				this.ToggleMotionBlur(this.motionBlurToggle.isOn);
			}
			else
			{
				PlayerPrefs.SetInt("MotionBlur", 1);
				this.cursorToggle.isOn = true;
				this.ToggleMotionBlur(this.motionBlurToggle.isOn);
			}
			if (PlayerPrefs.HasKey("CursorOn"))
			{
				this.cursorToggle.isOn = PlayerPrefs.GetInt("CursorOn") == 1;
				this.ToggleCursor();
			}
			else
			{
				PlayerPrefs.SetInt("CursorOn", 1);
				this.cursorToggle.isOn = true;
				this.ToggleCursor();
			}
			if (PlayerPrefs.HasKey("Trackpad"))
			{
				bool flag = PlayerPrefs.GetInt("Trackpad") == 1;
				this.trackpadToggle.isOn = flag;
			}
			else
			{
				PlayerPrefs.SetInt("Trackpad", 0);
				this.trackpadToggle.isOn = false;
			}
			if (PlayerPrefs.HasKey("MouseSensitivity"))
			{
				this.mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
				this.mouseSensitivity = Mathf.Clamp(this.mouseSensitivity, 0.1f, 2.4f);
				this.SetMouseSensitivity(this.mouseSensitivity);
				this.mouseSensitivitySlider.value = this.mouseSensitivity;
			}
			else
			{
				this.mouseSensitivity = 1f;
				if (this.trackpadToggle != null && !this.trackpadToggle.isOn)
				{
					this.mouseSensitivity = 1.5f;
				}
				PlayerPrefs.SetFloat("MouseSensitivity", this.mouseSensitivity);
				this.SetMouseSensitivity(this.mouseSensitivity);
				this.mouseSensitivitySlider.value = this.mouseSensitivity;
			}
			if (PlayerPrefs.HasKey("SubtitlesOn"))
			{
				this.subtitleToggle.isOn = PlayerPrefs.GetInt("SubtitlesOn") == 1;
				this.ToggleSubtitles();
			}
			else
			{
				PlayerPrefs.SetInt("SubtitlesOn", 1);
				this.subtitleToggle.isOn = true;
				this.ToggleSubtitles();
			}
			if (PlayerPrefs.HasKey("SFXVolume"))
			{
				this.mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVolume"));
				this.mixer.SetFloat("AmbienceVol", PlayerPrefs.GetFloat("SFXVolume"));
				this.SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
			}
			else
			{
				PlayerPrefs.SetFloat("SFXVolume", this.SFXVolumeSlider.value);
			}
			if (PlayerPrefs.HasKey("MusicVolume"))
			{
				this.mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVolume") * 0.9f);
				this.MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
			}
			else
			{
				PlayerPrefs.SetFloat("MusicVolume", this.MusicVolumeSlider.value);
			}
			if (PlayerPrefs.HasKey("VoiceVolume"))
			{
				this.mixer.SetFloat("VoiceVol", PlayerPrefs.GetFloat("VoiceVolume") * 0.8f);
				this.VOVolumeSlider.value = PlayerPrefs.GetFloat("VoiceVolume");
			}
			else
			{
				PlayerPrefs.SetFloat("VoiceVolume", this.VOVolumeSlider.value);
			}
			if (this.resolutionDropdown != null)
			{
				this.LoadResolution();
			}
			if (PlayerPrefs.HasKey("Quality"))
			{
				QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality"));
				this.currentQualityText.text = this.qualityNames[PlayerPrefs.GetInt("Quality")];
				SetFogQuality[] array = Object.FindObjectsOfType<SetFogQuality>();
				for (int j = 0; j < array.Length; j++)
				{
					array[j].SetQuality(PlayerPrefs.GetInt("Quality"));
				}
				this.SetPostFXQuality(PlayerPrefs.GetInt("Quality"));
			}
			else
			{
				QualitySettings.SetQualityLevel(4);
				PlayerPrefs.SetInt("Quality", 4);
				this.currentQualityText.text = this.qualityNames[4];
				this.SetPostFXQuality(4);
			}
			if (PlayerPrefs.HasKey("Vsync"))
			{
				QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
			}
			if (SceneManager.GetActiveScene().name == "Mian")
			{
				this.menu.SetActive(false);
			}
			else
			{
				foreach (object obj2 in this.menu.transform)
				{
					((Transform)obj2).gameObject.SetActive(false);
				}
			}
			PlayerPrefs.Save();
		}
	}

	// Token: 0x06000268 RID: 616 RVA: 0x00017100 File Offset: 0x00015300
	private void Update()
	{
		if (Time.timeSinceLevelLoad > 1.5f && Input.GetKeyDown(KeyCode.Escape) && this.underwaterDetector != null && !this.underwaterDetector.resetting && SceneManager.GetActiveScene().name == "Mian")
		{
			this.ToggleMenu();
		}
	}

	// Token: 0x06000269 RID: 617 RVA: 0x0001715C File Offset: 0x0001535C
	public void SetSFXVolume(float newVolume)
	{
		this.mixer.SetFloat("SFXVol", newVolume);
		this.mixer.SetFloat("AmbienceVol", newVolume);
		PlayerPrefs.SetFloat("SFXVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x0600026A RID: 618 RVA: 0x00017192 File Offset: 0x00015392
	public void SetVoiceoverVolume(float newVolume)
	{
		this.mixer.SetFloat("VoiceVol", newVolume);
		PlayerPrefs.SetFloat("VoiceVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x0600026B RID: 619 RVA: 0x000171B6 File Offset: 0x000153B6
	public void SetMusicVolume(float newVolume)
	{
		this.mixer.SetFloat("MusicVol", newVolume * 0.9f);
		PlayerPrefs.SetFloat("MusicVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x0600026C RID: 620 RVA: 0x000171E0 File Offset: 0x000153E0
	public void SetMouseSensitivity(float newSensitivity)
	{
		if (this.player != null)
		{
			this.player.SendMessage("SetSensitivity", newSensitivity);
		}
		this.mouseSensitivitySlider.value = newSensitivity;
		PlayerPrefs.SetFloat("MouseSensitivity", newSensitivity);
		PlayerPrefs.Save();
	}

	// Token: 0x0600026D RID: 621 RVA: 0x00017230 File Offset: 0x00015430
	public void ToggleMenu()
	{
		if (!this.canMenu)
		{
			return;
		}
		this.menu.SetActive(!this.menu.activeSelf);
		Cursor.lockState = (this.menu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked);
		Cursor.visible = this.menu.activeSelf;
		Time.timeScale = (this.menu.activeSelf ? 0f : 1f);
		this.resolutionDropdown.Hide();
		if (this.menu.activeSelf && this.narrator != null)
		{
			this.narrator.SendMessage("Pause");
		}
		else
		{
			this.narrator.SendMessage("UnPause");
		}
		if (this.menu.activeSelf && this.player != null)
		{
			this.player.SendMessage("Pause");
			return;
		}
		this.player.SendMessage("UnPause");
	}

	// Token: 0x0600026E RID: 622 RVA: 0x00017328 File Offset: 0x00015528
	public void ToggleMotionBlur(bool mb)
	{
		PlayerPrefs.SetInt("MotionBlur", mb ? 1 : 0);
		PlayerPrefs.Save();
		MotionBlur setting = this.fgProfile.GetSetting<MotionBlur>();
		if (setting == null)
		{
			return;
		}
		setting.active = mb;
		this.fgProfile.RemoveSettings<MotionBlur>();
		this.fgProfile_low.RemoveSettings<MotionBlur>();
		this.fgProfile_lowest.RemoveSettings<MotionBlur>();
		this.fgProfile.AddSettings(setting);
		this.fgProfile_low.AddSettings(setting);
		this.fgProfile_lowest.AddSettings(setting);
	}

	// Token: 0x0600026F RID: 623 RVA: 0x000173B0 File Offset: 0x000155B0
	public void ToggleFullscreen(bool fs)
	{
		Screen.fullScreen = fs;
		PlayerPrefs.SetInt("Fullscreen", fs ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x06000270 RID: 624 RVA: 0x000173CE File Offset: 0x000155CE
	public void ToggleVsync(bool vs)
	{
		QualitySettings.vSyncCount = (vs ? 1 : 0);
		PlayerPrefs.SetInt("Vsync", vs ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x06000271 RID: 625 RVA: 0x000173F4 File Offset: 0x000155F4
	public void ToggleCursor()
	{
		bool isOn = this.cursorToggle.isOn;
		if (this.cursor != null)
		{
			this.cursor.GetComponent<SpriteRenderer>().enabled = isOn;
		}
		PlayerPrefs.SetInt("CursorOn", isOn ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x06000272 RID: 626 RVA: 0x00017444 File Offset: 0x00015644
	public void ToggleTrackpad(bool shouldEnable)
	{
		if (shouldEnable)
		{
			Debug.Log("trackpad enabled");
		}
		else
		{
			Debug.Log("trackpad disabled");
		}
		this.trackpadToggle.isOn = shouldEnable;
		if (PlayerPrefs.GetInt("Trackpad") == 1 != shouldEnable)
		{
			if (shouldEnable)
			{
				this.SetMouseSensitivity(PlayerPrefs.GetFloat("MouseSensitivity") * 0.666f);
			}
			else
			{
				this.SetMouseSensitivity(PlayerPrefs.GetFloat("MouseSensitivity") * 1.5f);
			}
		}
		if (this.player != null)
		{
			this.player.GetComponent<PlayerControl>().trackpad = shouldEnable;
		}
		PlayerPrefs.SetInt("Trackpad", shouldEnable ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x06000273 RID: 627 RVA: 0x000174F0 File Offset: 0x000156F0
	public void ToggleSubtitles()
	{
		bool isOn = this.subtitleToggle.isOn;
		if (this.narrator != null)
		{
			this.narrator.SendMessage("ToggleSubtitles", isOn);
		}
		PlayerPrefs.SetInt("SubtitlesOn", isOn ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x06000274 RID: 628 RVA: 0x00017544 File Offset: 0x00015744
	public void ApplyResolution()
	{
		Debug.Log(string.Concat(new string[]
		{
			"setting res to: ",
			this.newRes.width.ToString(),
			" ",
			this.newRes.height.ToString(),
			" ",
			this.newRes.refreshRate.ToString()
		}));
		Screen.SetResolution(this.newRes.width, this.newRes.height, Screen.fullScreen, this.newRes.refreshRate);
		base.StartCoroutine(this.ApplyLetterbox());
		PlayerPrefs.SetInt("ResolutionWidth", this.newRes.width);
		PlayerPrefs.SetInt("ResolutionHeight", this.newRes.height);
		PlayerPrefs.SetInt("ResolutionRefresh", this.newRes.refreshRate);
		PlayerPrefs.Save();
		this.currentRes = this.newRes;
		this.applyResolutionButton.gameObject.SetActive(false);
	}

	// Token: 0x06000275 RID: 629 RVA: 0x00017654 File Offset: 0x00015854
	public void ChangeQuality(int changeBy = 1)
	{
		if (changeBy == 1 && QualitySettings.GetQualityLevel() == this.qualityNames.Length - 1)
		{
			return;
		}
		if (changeBy == -1 && QualitySettings.GetQualityLevel() == 0)
		{
			return;
		}
		if (changeBy == 1)
		{
			QualitySettings.IncreaseLevel();
		}
		else if (changeBy == -1)
		{
			QualitySettings.DecreaseLevel();
		}
		int qualityLevel = QualitySettings.GetQualityLevel();
		this.currentQualityText.text = this.qualityNames[qualityLevel];
		SetFogQuality[] array = Object.FindObjectsOfType<SetFogQuality>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetQuality(qualityLevel);
		}
		this.SetPostFXQuality(PlayerPrefs.GetInt("Quality"));
		if (PlayerPrefs.HasKey("Vsync"))
		{
			QualitySettings.vSyncCount = PlayerPrefs.GetInt("Vsync");
		}
		PlayerPrefs.SetInt("Quality", qualityLevel);
		PlayerPrefs.Save();
	}

	// Token: 0x06000276 RID: 630 RVA: 0x00017708 File Offset: 0x00015908
	private void SetPostFXQuality(int newQuality)
	{
		if (this.bgCam == null || this.fgCam == null)
		{
			return;
		}
		if (newQuality < QualitySettings.names.Length - 2)
		{
			this.bgCam.GetComponent<PostProcessVolume>().profile = this.bgProfile_low;
		}
		else
		{
			this.bgCam.GetComponent<PostProcessVolume>().profile = this.bgProfile;
		}
		if (newQuality < QualitySettings.names.Length / 3)
		{
			this.fgCam.GetComponent<PostProcessVolume>().profile = this.fgProfile_lowest;
			this.fgCam.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.None;
		}
		else if (newQuality < QualitySettings.names.Length - 2)
		{
			this.fgCam.GetComponent<PostProcessVolume>().profile = this.fgProfile_low;
			this.fgCam.GetComponent<PostProcessLayer>().fastApproximateAntialiasing.keepAlpha = true;
			this.fgCam.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
		}
		else
		{
			this.fgCam.GetComponent<PostProcessVolume>().profile = this.fgProfile;
			this.fgCam.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
		}
		this.fgCam.GetComponent<PostProcessLayer>().finalBlitToCameraTarget = false;
	}

	// Token: 0x06000277 RID: 631 RVA: 0x00017824 File Offset: 0x00015A24
	private void LoadResolution()
	{
		List<string> list = new List<string>();
		foreach (Resolution resolution in Screen.resolutions)
		{
			list.Add(string.Concat(new string[]
			{
				resolution.width.ToString(),
				"x",
				resolution.height.ToString(),
				" ",
				(resolution.refreshRate > 0) ? (resolution.refreshRate.ToString() + "Hz") : ""
			}));
		}
		this.resolutionDropdown.AddOptions(list);
		bool flag;
		if (PlayerPrefs.HasKey("Fullscreen"))
		{
			flag = PlayerPrefs.GetInt("Fullscreen") == 1;
			Screen.fullScreen = flag;
		}
		else
		{
			flag = Screen.fullScreen;
			PlayerPrefs.SetInt("Fullscreen", flag ? 1 : 0);
			PlayerPrefs.Save();
		}
		this.fullscreenToggle.isOn = flag;
		int num;
		if (PlayerPrefs.HasKey("Vsync"))
		{
			num = PlayerPrefs.GetInt("Vsync");
			QualitySettings.vSyncCount = num;
		}
		else
		{
			num = QualitySettings.vSyncCount;
			PlayerPrefs.SetInt("Vsync", num);
			PlayerPrefs.Save();
		}
		this.vsyncToggle.isOn = num == 1;
		if (PlayerPrefs.HasKey("ResolutionWidth"))
		{
			int @int = PlayerPrefs.GetInt("ResolutionWidth");
			int int2 = PlayerPrefs.GetInt("ResolutionHeight");
			int int3 = PlayerPrefs.GetInt("ResolutionRefresh");
			bool flag2 = false;
			for (int j = 0; j < Screen.resolutions.Length; j++)
			{
				Resolution resolution2 = Screen.resolutions[j];
				if (resolution2.width == @int && resolution2.height == int2 && resolution2.refreshRate == int3)
				{
					this.currentResolutionIndex = j;
					flag2 = true;
					Screen.SetResolution(resolution2.width, resolution2.height, flag, resolution2.refreshRate);
					this.resolutionDropdown.value = this.currentResolutionIndex;
					base.StartCoroutine(this.ApplyLetterbox());
					this.currentRes = resolution2;
					this.newRes = resolution2;
					break;
				}
			}
			if (!flag2)
			{
				for (int k = 0; k < Screen.resolutions.Length; k++)
				{
					Resolution resolution3 = Screen.resolutions[k];
					if (resolution3.width == @int && resolution3.height == int2)
					{
						flag2 = true;
						this.currentResolutionIndex = k;
						Screen.SetResolution(resolution3.width, resolution3.height, flag, resolution3.refreshRate);
						this.resolutionDropdown.value = this.currentResolutionIndex;
						base.StartCoroutine(this.ApplyLetterbox());
						this.currentRes = resolution3;
						this.newRes = resolution3;
						break;
					}
				}
			}
			if (!flag2)
			{
				PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.width);
				PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);
				PlayerPrefs.SetInt("ResolutionRefresh", Screen.currentResolution.refreshRate);
				PlayerPrefs.Save();
				Debug.Log("created new save key for screen res: " + Screen.currentResolution.width.ToString() + "x" + Screen.currentResolution.height.ToString());
				if (Screen.currentResolution.refreshRate != 0)
				{
					int refreshRate = Screen.currentResolution.refreshRate;
				}
				this.currentRes = Screen.currentResolution;
				this.newRes = this.currentRes;
				for (int l = 0; l < Screen.resolutions.Length; l++)
				{
					Resolution resolution4 = Screen.resolutions[l];
					if (resolution4.width == this.currentRes.width && resolution4.height == this.currentRes.height && (resolution4.refreshRate == this.currentRes.refreshRate || this.currentRes.refreshRate == 0))
					{
						this.resolutionDropdown.value = l;
						this.currentResolutionIndex = l;
						break;
					}
				}
			}
			Debug.Log(string.Concat(new string[]
			{
				"loaded resolution from save: ",
				@int.ToString(),
				"x",
				int2.ToString(),
				" with success: ",
				flag2.ToString()
			}));
			if (int3 == 0)
			{
				return;
			}
		}
		else
		{
			for (int m = Screen.resolutions.Length - 1; m >= 0; m--)
			{
				Resolution resolution5 = Screen.resolutions[m];
				if (resolution5.width == Screen.currentResolution.width && resolution5.height == Screen.currentResolution.height && (resolution5.refreshRate == Screen.currentResolution.refreshRate || Screen.currentResolution.refreshRate == 0))
				{
					this.currentResolutionIndex = m;
					break;
				}
			}
			if (Screen.currentResolution.width > 1920)
			{
				for (int n = Screen.resolutions.Length - 1; n >= 0; n--)
				{
					Resolution resolution6 = Screen.resolutions[n];
					if (resolution6.width <= 1920)
					{
						this.currentRes = resolution6;
						Screen.SetResolution(resolution6.width, resolution6.height, flag);
						base.StartCoroutine(this.ApplyLetterbox());
						this.currentResolutionIndex = n;
						break;
					}
				}
			}
			this.resolutionDropdown.value = this.currentResolutionIndex;
			PlayerPrefs.SetInt("ResolutionWidth", Screen.currentResolution.width);
			PlayerPrefs.SetInt("ResolutionHeight", Screen.currentResolution.height);
			PlayerPrefs.SetInt("ResolutionRefresh", Screen.currentResolution.refreshRate);
			PlayerPrefs.Save();
			Debug.Log("created new save key for screen res: " + Screen.currentResolution.width.ToString() + "x" + Screen.currentResolution.height.ToString());
			if (Screen.currentResolution.refreshRate != 0)
			{
				int refreshRate2 = Screen.currentResolution.refreshRate;
			}
			this.currentRes = Screen.currentResolution;
			this.newRes = this.currentRes;
		}
	}

	// Token: 0x06000278 RID: 632 RVA: 0x00017E5F File Offset: 0x0001605F
	public void QuitGame()
	{
		Camera.main.SendMessage("FadeOut");
		base.StartCoroutine(this.QuitDone());
	}

	// Token: 0x06000279 RID: 633 RVA: 0x00017E7D File Offset: 0x0001607D
	private IEnumerator QuitDone()
	{
		int wait = 60;
		while (wait > 0)
		{
			int num = wait;
			wait = num - 1;
			yield return null;
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Time.timeScale = 1f;
		SceneManager.LoadScene("Loader");
		yield break;
	}

	// Token: 0x0600027A RID: 634 RVA: 0x00017E88 File Offset: 0x00016088
	public void SetResByIndex(int changeTo = 0)
	{
		if (changeTo < 0 || changeTo >= Screen.resolutions.Length)
		{
			return;
		}
		this.newRes = Screen.resolutions[changeTo];
		if (this.newRes.width != this.currentRes.width || this.newRes.height != this.currentRes.height || this.newRes.refreshRate != this.currentRes.refreshRate)
		{
			this.applyResolutionButton.gameObject.SetActive(true);
		}
		if (this.newRes.refreshRate == 0)
		{
			this.newRes.refreshRate = 60;
		}
	}

	// Token: 0x0600027B RID: 635 RVA: 0x00017F28 File Offset: 0x00016128
	public void Ouch(int i)
	{
		Debug.Log("Ouch! " + i.ToString());
	}

	// Token: 0x0600027C RID: 636 RVA: 0x00017F40 File Offset: 0x00016140
	private IEnumerator ApplyLetterbox()
	{
		yield return null;
		yield return null;
		if (this.ratioFitter != null)
		{
			Vector2 vector = new Vector2((float)Screen.width, (float)Screen.height);
			if (vector.x / vector.y >= 1.7f)
			{
				vector.x = 16f;
				vector.y = 9f;
			}
			else
			{
				vector.x = 16f;
				vector.y = 10f;
			}
			this.ratioFitter.ratio = vector;
		}
		this.ratioFitter.CalculateAndSetAllCameraRatios();
		yield break;
	}

	// Token: 0x0600027D RID: 637 RVA: 0x00017F4F File Offset: 0x0001614F
	private void OnApplicationQuit()
	{
	}

	// Token: 0x040003F8 RID: 1016
	public GameObject menu;

	// Token: 0x040003F9 RID: 1017
	public TMP_Dropdown resolutionDropdown;

	// Token: 0x040003FA RID: 1018
	private int currentResolutionIndex;

	// Token: 0x040003FB RID: 1019
	public TextMeshProUGUI currentQualityText;

	// Token: 0x040003FC RID: 1020
	public Button currentQualityButton;

	// Token: 0x040003FD RID: 1021
	public Button currentQualityIncrementButton;

	// Token: 0x040003FE RID: 1022
	public Button currentQualityDecrementButton;

	// Token: 0x040003FF RID: 1023
	private int currentQualityIndex;

	// Token: 0x04000400 RID: 1024
	public Toggle cursorToggle;

	// Token: 0x04000401 RID: 1025
	public Toggle trackpadToggle;

	// Token: 0x04000402 RID: 1026
	public Toggle subtitleToggle;

	// Token: 0x04000403 RID: 1027
	public Toggle fullscreenToggle;

	// Token: 0x04000404 RID: 1028
	public Toggle motionBlurToggle;

	// Token: 0x04000405 RID: 1029
	public Toggle vsyncToggle;

	// Token: 0x04000406 RID: 1030
	public Button applyResolutionButton;

	// Token: 0x04000407 RID: 1031
	public Slider mouseSensitivitySlider;

	// Token: 0x04000408 RID: 1032
	public Slider SFXVolumeSlider;

	// Token: 0x04000409 RID: 1033
	public Slider MusicVolumeSlider;

	// Token: 0x0400040A RID: 1034
	public Slider VOVolumeSlider;

	// Token: 0x0400040B RID: 1035
	public GameObject player;

	// Token: 0x0400040C RID: 1036
	public GameObject cursor;

	// Token: 0x0400040D RID: 1037
	public GameObject narrator;

	// Token: 0x0400040E RID: 1038
	public RestartOnContact underwaterDetector;

	// Token: 0x0400040F RID: 1039
	public GameObject menuMobile;

	// Token: 0x04000410 RID: 1040
	public Toggle subtitleToggleMobile;

	// Token: 0x04000411 RID: 1041
	public Slider mouseSensitivitySliderMobile;

	// Token: 0x04000412 RID: 1042
	public Slider SFXVolumeSliderMobile;

	// Token: 0x04000413 RID: 1043
	public Slider MusicVolumeSliderMobile;

	// Token: 0x04000414 RID: 1044
	public Slider VOVolumeSliderMobile;

	// Token: 0x04000415 RID: 1045
	public AudioMixer mixer;

	// Token: 0x04000416 RID: 1046
	private float mouseSensitivity;

	// Token: 0x04000417 RID: 1047
	public ForceCameraRatio ratioFitter;

	// Token: 0x04000418 RID: 1048
	public PostProcessProfile bgProfile;

	// Token: 0x04000419 RID: 1049
	public PostProcessProfile bgProfile_low;

	// Token: 0x0400041A RID: 1050
	public PostProcessProfile fgProfile;

	// Token: 0x0400041B RID: 1051
	public PostProcessProfile fgProfile_low;

	// Token: 0x0400041C RID: 1052
	public PostProcessProfile fgProfile_lowest;

	// Token: 0x0400041D RID: 1053
	public Camera fgCam;

	// Token: 0x0400041E RID: 1054
	public Camera bgCam;

	// Token: 0x0400041F RID: 1055
	public bool canMenu;

	// Token: 0x04000420 RID: 1056
	private Resolution currentRes;

	// Token: 0x04000421 RID: 1057
	private Resolution newRes;

	// Token: 0x04000422 RID: 1058
	private int currentLanguageNum;

	// Token: 0x04000423 RID: 1059
	public Toggle[] languageToggles;

	// Token: 0x04000424 RID: 1060
	public Material potMat;

	// Token: 0x04000425 RID: 1061
	private string[] qualityNames = new string[] { "Worst", "Bad", "Mediocre", "Good", "Great", "Extreme" };
}
