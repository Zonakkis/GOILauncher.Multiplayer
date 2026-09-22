using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000146 RID: 326
public class SettingsManager : MonoBehaviour
{
	// Token: 0x06000904 RID: 2308 RVA: 0x0004A008 File Offset: 0x00048408
	public void SetLanguage(int newLang)
	{
		List<string> allLanguages = LocalizationManager.GetAllLanguages(true);
		if (LocalizationManager.HasLanguage(allLanguages[newLang], true, true, true))
		{
			LocalizationManager.CurrentLanguage = allLanguages[newLang];
		}
		this.OnLanguageChanged(newLang);
	}

	// Token: 0x06000905 RID: 2309 RVA: 0x0004A048 File Offset: 0x00048448
	public void OnLanguageChanged(int newLanguage)
	{
		if (newLanguage == this.currentLanguageNum)
		{
			return;
		}
		this.currentLanguageNum = newLanguage;
		PlayerPrefs.SetInt("Language", newLanguage);
		PlayerPrefs.Save();
		if (this.narrator != null)
		{
			this.narrator.SendMessage("SetLanguage", newLanguage);
		}
		this.LanguageDropdown.value = newLanguage;
	}

	// Token: 0x06000906 RID: 2310 RVA: 0x0004A0AC File Offset: 0x000484AC
	public void UpdateGoldPot()
	{
		if (PlayerPrefs.HasKey("NumWins"))
		{
			int @int = PlayerPrefs.GetInt("NumWins");
			if (this.potMat != null)
			{
				float num = (float)Mathf.Min(@int, 50) / 50f;
				num *= num;
				this.potMat.SetProceduralFloat("Goldness", num);
				this.potMat.RebuildTextures();
			}
		}
	}

	// Token: 0x06000907 RID: 2311 RVA: 0x0004A114 File Offset: 0x00048514
	private void Start()
	{
		this.mobileMan = GameObject.FindGameObjectWithTag("MobileManager").GetComponent<MobileManager>();
		this.UpdateGoldPot();
		this.menu = this.menuMobile;
		this.subtitleToggle = this.subtitleToggleMobile;
		this.mouseSensitivitySlider = this.mouseSensitivitySliderMobile;
		this.SFXVolumeSlider = this.SFXVolumeSliderMobile;
		this.MusicVolumeSlider = this.MusicVolumeSliderMobile;
		this.VOVolumeSlider = this.VOVolumeSliderMobile;
		this.DisableSkipCredits();
		if (this.ScreenFader == null)
		{
			this.ScreenFader = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
		PlayerPrefs.SetInt("NativeWidth", Screen.currentResolution.width);
		PlayerPrefs.SetInt("NativeHeight", Screen.currentResolution.height);
		PlayerPrefs.SetInt("NativeRefresh", Screen.currentResolution.refreshRate);
		this.canMenu = true;
		this.currentLanguageNum = -1;
		int num;
		if (PlayerPrefs.HasKey("Language"))
		{
			num = PlayerPrefs.GetInt("Language");
		}
		else
		{
			SystemLanguage systemLanguage = Application.systemLanguage;
			if (systemLanguage != SystemLanguage.Japanese)
			{
				if (systemLanguage != SystemLanguage.Korean)
				{
					if (systemLanguage != SystemLanguage.ChineseSimplified)
					{
						if (systemLanguage != SystemLanguage.ChineseTraditional)
						{
							if (systemLanguage != SystemLanguage.Chinese)
							{
								if (systemLanguage != SystemLanguage.English)
								{
									if (systemLanguage != SystemLanguage.Russian)
									{
										num = 0;
									}
									else
									{
										num = 1;
									}
								}
								else
								{
									num = 0;
								}
							}
							else
							{
								num = 3;
							}
						}
						else
						{
							num = 0;
						}
					}
					else
					{
						num = 3;
					}
				}
				else
				{
					num = 4;
				}
			}
			else
			{
				num = 2;
			}
			PlayerPrefs.SetInt("Language", num);
		}
		this.SetLanguage(num);
		this.LanguageDropdown.onValueChanged.AddListener(delegate(int index)
		{
			this.SetLanguage(index);
		});
		if (PlayerPrefs.HasKey("MouseSensitivity"))
		{
			this.mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity");
			this.SetMouseSensitivity(this.mouseSensitivity);
			this.mouseSensitivitySlider.value = this.mouseSensitivity;
		}
		else
		{
			this.mouseSensitivity = 1f;
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
		PlayerPrefs.Save();
	}

	// Token: 0x06000908 RID: 2312 RVA: 0x0004A4DF File Offset: 0x000488DF
	public void SetSFXVolume(float newVolume)
	{
		this.mixer.SetFloat("SFXVol", newVolume);
		this.mixer.SetFloat("AmbienceVol", newVolume);
		PlayerPrefs.SetFloat("SFXVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x06000909 RID: 2313 RVA: 0x0004A515 File Offset: 0x00048915
	public void SetVoiceoverVolume(float newVolume)
	{
		this.mixer.SetFloat("VoiceVol", newVolume * 0.8f);
		PlayerPrefs.SetFloat("VoiceVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x0600090A RID: 2314 RVA: 0x0004A53F File Offset: 0x0004893F
	public void SetMusicVolume(float newVolume)
	{
		this.mixer.SetFloat("MusicVol", newVolume * 0.9f);
		PlayerPrefs.SetFloat("MusicVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x0600090B RID: 2315 RVA: 0x0004A569 File Offset: 0x00048969
	public void showReset1()
	{
		if (this.resetGame1 != null && this.resetGame2 != null)
		{
			this.resetGame1.SetActive(true);
			this.resetGame2.SetActive(false);
		}
	}

	// Token: 0x0600090C RID: 2316 RVA: 0x0004A5A5 File Offset: 0x000489A5
	public void showReset2()
	{
		if (this.resetGame1 != null && this.resetGame2 != null)
		{
			this.resetGame2.SetActive(true);
			this.resetGame1.SetActive(false);
		}
	}

	// Token: 0x0600090D RID: 2317 RVA: 0x0004A5E4 File Offset: 0x000489E4
	public void newGame()
	{
		PlayerPrefs.DeleteKey("NumSaves");
		PlayerPrefs.DeleteKey("SaveGame0");
		PlayerPrefs.DeleteKey("SaveGame1");
		PlayerPrefs.Save();
		Time.timeScale = 0f;
		this.ScreenFader.EndScene(ScreenFader.ScreenFaderExitType.ReloadMain);
		this.menu.SetActive(!this.menu.activeSelf);
	}

	// Token: 0x0600090E RID: 2318 RVA: 0x0004A644 File Offset: 0x00048A44
	private IEnumerator ResetDone()
	{
		int wait = 30;
		while (wait > 0)
		{
			wait--;
			yield return null;
		}
		Time.timeScale = 1f;
		yield break;
	}

	// Token: 0x0600090F RID: 2319 RVA: 0x0004A658 File Offset: 0x00048A58
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

	// Token: 0x1700013C RID: 316
	// (get) Token: 0x06000910 RID: 2320 RVA: 0x0004A6A8 File Offset: 0x00048AA8
	public bool IsMenuActive
	{
		get
		{
			return this.menu.activeSelf;
		}
	}

	// Token: 0x06000911 RID: 2321 RVA: 0x0004A6B8 File Offset: 0x00048AB8
	public void ToggleMenu()
	{
		if (!this.canMenu)
		{
			return;
		}
		this.menu.SetActive(!this.menu.activeSelf);
		Cursor.lockState = ((!this.menu.activeSelf) ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = this.menu.activeSelf;
		Time.timeScale = ((!this.menu.activeSelf) ? 1f : 0f);
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
		}
		else
		{
			this.player.SendMessage("UnPause");
		}
		if (this.menu.activeSelf)
		{
			this.progressLabel.text = this.progressMeter.progress.ToString("0.0");
			this.showBestWinsIfWon();
			if (this.RatePopupWindow.ShouldPrompt())
			{
				this.RatePopupWindow.gameObject.SetActive(true);
			}
		}
		this.showReset1();
	}

	// Token: 0x06000912 RID: 2322 RVA: 0x0004A824 File Offset: 0x00048C24
	private void showBestWinsIfWon()
	{
		int @int = PlayerPrefs.GetInt("NumWins", 0);
		if (@int > 0)
		{
			this.NumberOfWins.gameObject.SetActive(true);
			this.NumberOfWins.text = @int.ToString();
			float @float = PlayerPrefs.GetFloat("BestTime", 0f);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)@float);
			string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
			this.BestTime.gameObject.SetActive(true);
			this.BestTime.text = text;
		}
		else
		{
			this.NumberOfWins.gameObject.SetActive(false);
			this.BestTime.gameObject.SetActive(false);
		}
		this.NumberOfWinsLabel.SetActive(this.NumberOfWins.gameObject.activeSelf);
		this.BestTimeLabel.SetActive(this.BestTime.gameObject.activeSelf);
	}

	// Token: 0x06000913 RID: 2323 RVA: 0x0004A930 File Offset: 0x00048D30
	public void ToggleSubtitles()
	{
		bool isOn = this.subtitleToggle.isOn;
		if (this.narrator != null)
		{
			this.narrator.SendMessage("ToggleSubtitles", isOn);
		}
		PlayerPrefs.SetInt("SubtitlesOn", (!isOn) ? 0 : 1);
		PlayerPrefs.Save();
	}

	// Token: 0x06000914 RID: 2324 RVA: 0x0004A98C File Offset: 0x00048D8C
	public void OpenQuitConfirmationWindow()
	{
		this.QuitConfirmationWindow.SetActive(true);
	}

	// Token: 0x06000915 RID: 2325 RVA: 0x0004A99A File Offset: 0x00048D9A
	public void CloseQuitConfirmationWindow()
	{
		this.QuitConfirmationWindow.SetActive(false);
	}

	// Token: 0x1700013D RID: 317
	// (get) Token: 0x06000916 RID: 2326 RVA: 0x0004A9A8 File Offset: 0x00048DA8
	public bool IsQuitConfirmationWindowOpen
	{
		get
		{
			return this.QuitConfirmationWindow.activeSelf;
		}
	}

	// Token: 0x06000917 RID: 2327 RVA: 0x0004A9B5 File Offset: 0x00048DB5
	public void QuitGame()
	{
		Application.Quit();
	}

	// Token: 0x06000918 RID: 2328 RVA: 0x0004A9BC File Offset: 0x00048DBC
	public void OpenResetGameConfirmationWindow()
	{
		this.ResetGameConfirmationWindow.SetActive(true);
	}

	// Token: 0x06000919 RID: 2329 RVA: 0x0004A9CA File Offset: 0x00048DCA
	public void CloseResetGameConfirmationWindow()
	{
		this.ResetGameConfirmationWindow.SetActive(false);
	}

	// Token: 0x1700013E RID: 318
	// (get) Token: 0x0600091A RID: 2330 RVA: 0x0004A9D8 File Offset: 0x00048DD8
	public bool IsResetGameConfirmationWindowOpen
	{
		get
		{
			return this.ResetGameConfirmationWindow.activeSelf;
		}
	}

	// Token: 0x0600091B RID: 2331 RVA: 0x0004A9E5 File Offset: 0x00048DE5
	public void ResetGame()
	{
		this.CloseResetGameConfirmationWindow();
		this.newGame();
	}

	// Token: 0x1700013F RID: 319
	// (get) Token: 0x0600091C RID: 2332 RVA: 0x0004A9F3 File Offset: 0x00048DF3
	public bool IsRatePopupWindowOpen
	{
		get
		{
			return this.RatePopupWindow.gameObject.activeSelf;
		}
	}

	// Token: 0x0600091D RID: 2333 RVA: 0x0004AA05 File Offset: 0x00048E05
	public void CloseRatePopupWindow()
	{
		this.RatePopupWindow.gameObject.SetActive(false);
	}

	// Token: 0x0600091E RID: 2334 RVA: 0x0004AA18 File Offset: 0x00048E18
	public void EnableSkipCredits()
	{
		this.TrySkipCreditsButton.SetActive(true);
	}

	// Token: 0x0600091F RID: 2335 RVA: 0x0004AA26 File Offset: 0x00048E26
	public void DisableSkipCredits()
	{
		this.TrySkipCreditsButton.SetActive(false);
		this.SkipCreditsButton.SetActive(false);
	}

	// Token: 0x06000920 RID: 2336 RVA: 0x0004AA40 File Offset: 0x00048E40
	public void TrySkipCredits()
	{
		this.TrySkipCreditsButton.SetActive(false);
		this.SkipCreditsButton.SetActive(true);
	}

	// Token: 0x06000921 RID: 2337 RVA: 0x0004AA5A File Offset: 0x00048E5A
	public void SkipCredits()
	{
		SceneManager.LoadScene("Reward Loader Mobile");
	}

	// Token: 0x06000922 RID: 2338 RVA: 0x0004AA68 File Offset: 0x00048E68
	public void goToInput()
	{
		this.inputPanel.SetActive(true);
		this.AVPanel.SetActive(false);
		this.streamingPanel.SetActive(false);
		this.inputText.color = Color.white;
		this.AVText.color = new Color(0.7f, 0.7f, 0.7f);
		this.streamingText.color = new Color(0.7f, 0.7f, 0.7f);
	}

	// Token: 0x06000923 RID: 2339 RVA: 0x0004AAE8 File Offset: 0x00048EE8
	public void goToAV()
	{
		this.inputPanel.SetActive(false);
		this.AVPanel.SetActive(true);
		this.streamingPanel.SetActive(false);
		this.inputText.color = new Color(0.7f, 0.7f, 0.7f);
		this.AVText.color = Color.white;
		this.streamingText.color = new Color(0.7f, 0.7f, 0.7f);
	}

	// Token: 0x06000924 RID: 2340 RVA: 0x0004AB68 File Offset: 0x00048F68
	public void goToStreaming()
	{
		this.inputPanel.SetActive(false);
		this.AVPanel.SetActive(false);
		this.streamingPanel.SetActive(true);
		this.inputText.color = new Color(0.7f, 0.7f, 0.7f);
		this.AVText.color = new Color(0.7f, 0.7f, 0.7f);
		this.streamingText.color = Color.white;
	}

	// Token: 0x06000925 RID: 2341 RVA: 0x0004ABE7 File Offset: 0x00048FE7
	public void setBeautiful()
	{
		base.StartCoroutine(this.SwitchToBatterySaverGraphics());
	}

	// Token: 0x06000926 RID: 2342 RVA: 0x0004ABF6 File Offset: 0x00048FF6
	public void setSixty()
	{
		base.StartCoroutine(this.SwitchToBeautifulGraphics());
	}

	// Token: 0x06000927 RID: 2343 RVA: 0x0004AC05 File Offset: 0x00049005
	public void setBatterySave()
	{
		base.StartCoroutine(this.SwitchToSixtyFramesGraphics());
	}

	// Token: 0x06000928 RID: 2344 RVA: 0x0004AC14 File Offset: 0x00049014
	private IEnumerator SwitchToSixtyFramesGraphics()
	{
		this.WaitOverlay.SetActive(true);
		yield return null;
		try
		{
			this.mobileMan.SixtyFramesOn();
		}
		finally
		{
			this.WaitOverlay.SetActive(false);
		}
		yield break;
	}

	// Token: 0x06000929 RID: 2345 RVA: 0x0004AC30 File Offset: 0x00049030
	private IEnumerator SwitchToBatterySaverGraphics()
	{
		this.WaitOverlay.SetActive(true);
		yield return null;
		try
		{
			this.mobileMan.BatterySaverOn();
		}
		finally
		{
			this.WaitOverlay.SetActive(false);
		}
		yield break;
	}

	// Token: 0x0600092A RID: 2346 RVA: 0x0004AC4C File Offset: 0x0004904C
	private IEnumerator SwitchToBeautifulGraphics()
	{
		this.WaitOverlay.SetActive(true);
		yield return null;
		try
		{
			this.mobileMan.BeautifulOn();
		}
		finally
		{
			this.WaitOverlay.SetActive(false);
		}
		yield break;
	}

	// Token: 0x0600092B RID: 2347 RVA: 0x0004AC67 File Offset: 0x00049067
	public void toggleCamera(bool camera)
	{
		this.camToggle.isOn = camera;
	}

	// Token: 0x0600092C RID: 2348 RVA: 0x0004AC75 File Offset: 0x00049075
	public void toggleMicrophone(bool microphone)
	{
		this.micToggle.isOn = microphone;
	}

	// Token: 0x0600092D RID: 2349 RVA: 0x0004AC83 File Offset: 0x00049083
	public void startStreaming()
	{
	}

	// Token: 0x0600092E RID: 2350 RVA: 0x0004AC85 File Offset: 0x00049085
	public void stopStreaming()
	{
	}

	// Token: 0x0600092F RID: 2351 RVA: 0x0004AC87 File Offset: 0x00049087
	public void shareURL()
	{
		ios_sharesheet.show(this.URLText.text);
	}

	// Token: 0x06000930 RID: 2352 RVA: 0x0004AC99 File Offset: 0x00049099
	public void showLeaderboards()
	{
		this.gcManager.showLeaderboards();
	}

	// Token: 0x0400088C RID: 2188
	public GameObject menu;

	// Token: 0x0400088D RID: 2189
	public Toggle subtitleToggle;

	// Token: 0x0400088E RID: 2190
	public Slider mouseSensitivitySlider;

	// Token: 0x0400088F RID: 2191
	public Slider SFXVolumeSlider;

	// Token: 0x04000890 RID: 2192
	public Slider MusicVolumeSlider;

	// Token: 0x04000891 RID: 2193
	public Slider VOVolumeSlider;

	// Token: 0x04000892 RID: 2194
	public GameObject player;

	// Token: 0x04000893 RID: 2195
	public GameObject cursor;

	// Token: 0x04000894 RID: 2196
	public Narrator narrator;

	// Token: 0x04000895 RID: 2197
	public GameObject menuMobile;

	// Token: 0x04000896 RID: 2198
	public Toggle subtitleToggleMobile;

	// Token: 0x04000897 RID: 2199
	public Slider mouseSensitivitySliderMobile;

	// Token: 0x04000898 RID: 2200
	public Slider SFXVolumeSliderMobile;

	// Token: 0x04000899 RID: 2201
	public Slider MusicVolumeSliderMobile;

	// Token: 0x0400089A RID: 2202
	public Slider VOVolumeSliderMobile;

	// Token: 0x0400089B RID: 2203
	public TMP_Dropdown LanguageDropdown;

	// Token: 0x0400089C RID: 2204
	public AudioMixer mixer;

	// Token: 0x0400089D RID: 2205
	private float mouseSensitivity;

	// Token: 0x0400089E RID: 2206
	public GameObject resetGame1;

	// Token: 0x0400089F RID: 2207
	public GameObject resetGame2;

	// Token: 0x040008A0 RID: 2208
	public TextMeshProUGUI progressLabel;

	// Token: 0x040008A1 RID: 2209
	public ProgressMeter progressMeter;

	// Token: 0x040008A2 RID: 2210
	public Camera fgCam;

	// Token: 0x040008A3 RID: 2211
	public Camera bgCam;

	// Token: 0x040008A4 RID: 2212
	public bool canMenu;

	// Token: 0x040008A5 RID: 2213
	private Resolution currentRes;

	// Token: 0x040008A6 RID: 2214
	private Resolution newRes;

	// Token: 0x040008A7 RID: 2215
	private int currentLanguageNum;

	// Token: 0x040008A8 RID: 2216
	public Toggle[] languageToggles;

	// Token: 0x040008A9 RID: 2217
	[Header("-------------------------------------------------------------")]
	[Space(4f)]
	public TextMeshProUGUI inputText;

	// Token: 0x040008AA RID: 2218
	public TextMeshProUGUI AVText;

	// Token: 0x040008AB RID: 2219
	public TextMeshProUGUI streamingText;

	// Token: 0x040008AC RID: 2220
	public GameObject inputPanel;

	// Token: 0x040008AD RID: 2221
	public GameObject AVPanel;

	// Token: 0x040008AE RID: 2222
	public GameObject streamingPanel;

	// Token: 0x040008AF RID: 2223
	public GameObject SkipCreditsButton;

	// Token: 0x040008B0 RID: 2224
	public GameObject TrySkipCreditsButton;

	// Token: 0x040008B1 RID: 2225
	public Toggle camToggle;

	// Token: 0x040008B2 RID: 2226
	public Toggle micToggle;

	// Token: 0x040008B3 RID: 2227
	public ReplayKitManager replayKit;

	// Token: 0x040008B4 RID: 2228
	public TextMeshProUGUI URLText;

	// Token: 0x040008B5 RID: 2229
	public GamecenterManager gcManager;

	// Token: 0x040008B6 RID: 2230
	private MobileManager mobileMan;

	// Token: 0x040008B7 RID: 2231
	private ScreenFader ScreenFader;

	// Token: 0x040008B8 RID: 2232
	public Button QuitButton;

	// Token: 0x040008B9 RID: 2233
	public GameObject QuitConfirmationWindow;

	// Token: 0x040008BA RID: 2234
	public GameObject ResetGameConfirmationWindow;

	// Token: 0x040008BB RID: 2235
	public RatePopup RatePopupWindow;

	// Token: 0x040008BC RID: 2236
	public TextMeshProUGUI BestTime;

	// Token: 0x040008BD RID: 2237
	public GameObject BestTimeLabel;

	// Token: 0x040008BE RID: 2238
	public TextMeshProUGUI NumberOfWins;

	// Token: 0x040008BF RID: 2239
	public GameObject NumberOfWinsLabel;

	// Token: 0x040008C0 RID: 2240
	public GameObject WaitOverlay;

	// Token: 0x040008C1 RID: 2241
	[Header("-------------------------------------------------------------")]
	[Space(4f)]
	public ProceduralMaterial potMat;
}
