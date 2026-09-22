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

// Token: 0x02000075 RID: 117
public class SettingsManager : MonoBehaviour
{
	// Token: 0x06000301 RID: 769 RVA: 0x00028ED4 File Offset: 0x000270D4
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

	// Token: 0x06000302 RID: 770 RVA: 0x00028F28 File Offset: 0x00027128
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

	// Token: 0x06000303 RID: 771 RVA: 0x00028FE0 File Offset: 0x000271E0
	private void Awake()
	{
		if (this.ratioFitter != null)
		{
			Vector2 vector;
			if (Screen.width >= Screen.height)
			{
				vector = new Vector2((float)Screen.width / (float)Screen.height, 1f);
			}
			else
			{
				vector = new Vector2((float)Screen.width, (float)Screen.height);
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
			}
			this.ratioFitter.ratio = vector;
		}
	}

	// Token: 0x06000304 RID: 772 RVA: 0x0002908C File Offset: 0x0002728C
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
			if (SettingsManager.potGoldness < 0.0)
			{
				SettingsManager.potGoldness = (double)num2;
			}
			this.potMat.SetFloat("_Goldness", (float)SettingsManager.potGoldness);
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
					goto IL_01F4;
				}
				if (systemLanguage == SystemLanguage.English)
				{
					Debug.Log("successfully detected english");
					num3 = 0;
					goto IL_01F4;
				}
				if (systemLanguage == SystemLanguage.Japanese)
				{
					num3 = 2;
					Debug.Log("Detected language as Japanese, weirdly");
					goto IL_01F4;
				}
			}
			else if (systemLanguage <= SystemLanguage.Russian)
			{
				if (systemLanguage == SystemLanguage.Korean)
				{
					num3 = 4;
					goto IL_01F4;
				}
				if (systemLanguage == SystemLanguage.Russian)
				{
					num3 = 1;
					goto IL_01F4;
				}
			}
			else
			{
				if (systemLanguage == SystemLanguage.ChineseSimplified)
				{
					num3 = 3;
					goto IL_01F4;
				}
				if (systemLanguage == SystemLanguage.ChineseTraditional)
				{
					num3 = 3;
					goto IL_01F4;
				}
			}
			num3 = 0;
		}
		IL_01F4:
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
				SetFogQuality[] array = global::UnityEngine.Object.FindObjectsOfType<SetFogQuality>();
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
		this._whitePixel = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		this._whitePixel.SetPixel(0, 0, Color.white);
		this._whitePixel.Apply();
		this._blackPixel = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		this._blackPixel.SetPixel(0, 0, Color.black);
		this._blackPixel.Apply();
		this._greenPixel = new Texture2D(1, 1, TextureFormat.ARGB32, false);
		this._greenPixel.SetPixel(0, 0, Color.green);
		this._greenPixel.Apply();
		this._sliderBackgroundStyle = new GUIStyle();
		this._sliderBackgroundStyle.padding = new RectOffset(0, 0, 0, 0);
		this._sliderBackgroundStyle.normal.background = this._whitePixel;
		this._sliderBackgroundStyle.hover.background = this._whitePixel;
		this._sliderBackgroundStyle.active.background = this._whitePixel;
		this._sliderBackgroundStyle.focused.background = this._whitePixel;
		this._sliderThumbStyle = new GUIStyle();
		this._sliderThumbStyle.stretchHeight = true;
		this._sliderThumbStyle.fixedWidth = 20f;
		this._sliderThumbStyle.normal.background = this._blackPixel;
		this._sliderThumbStyle.hover.background = this._blackPixel;
		this._sliderThumbStyle.active.background = this._blackPixel;
		this._sliderThumbStyle.focused.background = this._blackPixel;
		this._sliderThumbStyleSelected = new GUIStyle();
		this._sliderThumbStyleSelected.stretchHeight = true;
		this._sliderThumbStyleSelected.fixedWidth = 20f;
		this._sliderThumbStyleSelected.normal.background = this._greenPixel;
		this._sliderThumbStyleSelected.hover.background = this._greenPixel;
		this._sliderThumbStyleSelected.active.background = this._greenPixel;
		this._sliderThumbStyleSelected.focused.background = this._greenPixel;
		this.debugGUI = false;
		this.showMenu = false;
		this.timeReset = 0f;
		SettingsManager.reachedSpace = false;
		if (global::UnityEngine.Object.FindObjectOfType<Narrator>() != null)
		{
			this.timeReset = global::UnityEngine.Object.FindObjectOfType<Narrator>().timePlayedThisGame * -1f;
		}
		this.updateTimer = 0f;
		this.timeModReset = 0f;
		this.upTimer = 0.01f;
		this.leftTimer = 0.01f;
		this.rightTimer = 0.01f;
		this.downTimer = 0.01f;
		this.selectedSlider = -1;
		this.mapDebug = false;
		this.mapDebugText = "";
		this.changelogLock = false;
		this.showChangelog = false;
		this.runChanged = false;
		this.debugTimerCounter = 0;
		this.debugNotifyTime = 0f;
		this.debugNotifyMessage = "";
		this.objectDetails = new List<string>();
		List<string> list = new List<string>();
		if (!SettingsManager.modpackEnabled)
		{
			SettingsManager.category = 1;
		}
		if (SceneManager.GetActiveScene().name == "Mian")
		{
			this.setCategory(-1);
			for (int k = 0; k < SettingsManager.modObjects.Length; k++)
			{
				if (SettingsManager.modObjects[k].enabled)
				{
					SettingsManager.modObjects[k].Load();
					if (SettingsManager.modObjects[k].enableSplits)
					{
						list.Add((SettingsManager.modObjects[k].options.Count > 0) ? (SettingsManager.modObjects[k].name + "[" + SettingsManager.modObjects[k].options[SettingsManager.modObjects[k].option] + "]") : SettingsManager.modObjects[k].name);
					}
				}
			}
			list.Sort();
		}
		else if (SettingsManager.timer.changed || SettingsManager.timer.pb)
		{
			SettingsManager.timer.SaveRun();
		}
		this.SetMap();
		if (SceneManager.GetActiveScene().name == "Mian" || SettingsManager.timer.run == null)
		{
			SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list);
			if (SettingsManager.timer.debug)
			{
				SettingsManager.timer.drawSplits();
			}
		}
	}

	// Token: 0x06000305 RID: 773 RVA: 0x00029BA8 File Offset: 0x00027DA8
	private void Update()
	{
		this.updateTimer = Time.realtimeSinceStartup;
		if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) && SettingsManager.timer.optionsSelect[0] > 0)
		{
			SettingsManager.timer.optionsSelect[5] = ((SettingsManager.timer.optionsSelect[5] == 1) ? 0 : 1);
			PlayerPrefs.SetInt(SettingsManager.timer.optionsTitle[5].Trim(), SettingsManager.timer.optionsSelect[5]);
			PlayerPrefs.Save();
		}
		if (!(SceneManager.GetActiveScene().name == "Mian"))
		{
			this.showMenu = false;
			if (SettingsManager.timer.enabled && (SettingsManager.timer.pb || SettingsManager.timer.changed))
			{
				SettingsManager.timer.SaveRun();
				return;
			}
		}
		else
		{
			if (this.debugNotifyTime > Time.deltaTime)
			{
				this.debugNotifyTime -= Time.deltaTime;
			}
			else if (this.debugNotifyTime != 0f)
			{
				this.debugNotifyTime = 0f;
			}
			if (Input.GetKeyDown(KeyCode.Escape) && this.showMenu && SceneManager.GetActiveScene().name == "Mian")
			{
				this.ToggleDebugMenu();
			}
			else if (Time.timeSinceLevelLoad > 1.5f && Input.GetKeyDown(KeyCode.Escape) && SceneManager.GetActiveScene().name == "Mian")
			{
				this.showMenu = false;
				this.ToggleMenu();
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.teleportSpeed = 0.2f;
			}
			else
			{
				this.teleportSpeed = 0.1f;
			}
			if (!this.hub && this.player)
			{
				this.hub = this.player.transform.Find("Hub").gameObject;
			}
			if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl) && !Input.GetKey(KeyCode.LeftAlt) && !Input.GetKey(KeyCode.RightAlt))
			{
				if (Input.GetKey(KeyCode.P))
				{
					SettingsManager.modPackManager.ResetProps();
					this.runChanged = true;
				}
				if (Input.GetKeyDown(KeyCode.Mouse1) && this.showMenu)
				{
					this.runChanged = true;
					Vector3 vector = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
					Collider2D[] array = Physics2D.OverlapCircleAll(new Vector2(vector.x, vector.y), 0.1f);
					this.objectDetails = new List<string>();
					this.clipboardText = new List<string>();
					for (int i = 0; i < array.Length; i++)
					{
						this.GetChilds(array[i].gameObject);
						string text;
						try
						{
							text = array[i].gameObject.GetComponent<MeshRenderer>().ToString();
						}
						catch
						{
							text = "None";
						}
						string text2 = string.Concat(new object[]
						{
							array[i].gameObject.name,
							" [Pos: ",
							array[i].transform.localPosition.ToString(),
							array[i].transform.position.ToString(),
							"] [Layer: ",
							array[i].gameObject.layer,
							"] [Static: ",
							array[i].gameObject.isStatic.ToString(),
							"] [Mesh: ",
							text,
							"] [Parent: ",
							array[i].gameObject.transform.parent.name,
							"]"
						});
						this.objectDetails.Add(text2);
						this.clipboardText.Add(text2);
						string text3 = array[i].gameObject.transform.parent.name;
						for (;;)
						{
							try
							{
								GameObject gameObject = GameObject.Find(text3);
								if (gameObject != null)
								{
									text2 = string.Concat(new object[]
									{
										gameObject.name,
										" [Pos: ",
										gameObject.transform.localPosition.ToString(),
										gameObject.transform.position.ToString(),
										"] [Layer: ",
										gameObject.layer,
										"] [Static: ",
										gameObject.isStatic.ToString(),
										"] [Parent: ",
										gameObject.transform.parent ? gameObject.transform.parent.name : "None",
										"]"
									});
									this.objectDetails.Add(text2);
									this.clipboardText.Add(text2);
									if (gameObject.transform.parent)
									{
										text3 = gameObject.transform.parent.name;
										continue;
									}
								}
							}
							catch (Exception ex)
							{
								this.objectDetails.Add(ex.ToString());
							}
							break;
						}
						if (Input.GetKey(KeyCode.Delete))
						{
							global::UnityEngine.Object.Destroy(array[i].gameObject);
						}
					}
					if (Input.GetKey(KeyCode.Insert))
					{
						string text4 = "";
						for (int j = 0; j < this.clipboardText.Count; j++)
						{
							text4 = string.Concat(new object[]
							{
								text4,
								"Object ",
								j,
								": ",
								this.clipboardText[j],
								"\n"
							});
						}
						SettingsManager.modPackManager.CopyToClipboard(text4);
					}
				}
				if (SettingsManager.teleportEnabled)
				{
					if ((Input.GetKey(KeyCode.Keypad1) || Input.GetKey(KeyCode.Alpha1)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 1))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list = new List<string>();
						for (int k = 0; k < SettingsManager.modObjects.Length; k++)
						{
							if (SettingsManager.modObjects[k].enabled && SettingsManager.modObjects[k].enableSplits)
							{
								list.Add((SettingsManager.modObjects[k].options.Count > 0) ? (SettingsManager.modObjects[k].name + "[" + SettingsManager.modObjects[k].options[SettingsManager.modObjects[k].option] + "]") : SettingsManager.modObjects[k].name);
							}
						}
						list.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list);
					}
					if ((Input.GetKey(KeyCode.Keypad2) || Input.GetKey(KeyCode.Alpha2)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 2))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list2 = new List<string>();
						for (int l = 0; l < SettingsManager.modObjects.Length; l++)
						{
							if (SettingsManager.modObjects[l].enabled && SettingsManager.modObjects[l].enableSplits)
							{
								list2.Add((SettingsManager.modObjects[l].options.Count > 0) ? (SettingsManager.modObjects[l].name + "[" + SettingsManager.modObjects[l].options[SettingsManager.modObjects[l].option] + "]") : SettingsManager.modObjects[l].name);
							}
						}
						list2.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list2);
					}
					if ((Input.GetKey(KeyCode.Keypad3) || Input.GetKey(KeyCode.Alpha3)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 3))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list3 = new List<string>();
						for (int m = 0; m < SettingsManager.modObjects.Length; m++)
						{
							if (SettingsManager.modObjects[m].enabled && SettingsManager.modObjects[m].enableSplits)
							{
								list3.Add((SettingsManager.modObjects[m].options.Count > 0) ? (SettingsManager.modObjects[m].name + "[" + SettingsManager.modObjects[m].options[SettingsManager.modObjects[m].option] + "]") : SettingsManager.modObjects[m].name);
							}
						}
						list3.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list3);
					}
					if ((Input.GetKey(KeyCode.Keypad4) || Input.GetKey(KeyCode.Alpha4)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 4))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list4 = new List<string>();
						for (int n = 0; n < SettingsManager.modObjects.Length; n++)
						{
							if (SettingsManager.modObjects[n].enabled && SettingsManager.modObjects[n].enableSplits)
							{
								list4.Add((SettingsManager.modObjects[n].options.Count > 0) ? (SettingsManager.modObjects[n].name + "[" + SettingsManager.modObjects[n].options[SettingsManager.modObjects[n].option] + "]") : SettingsManager.modObjects[n].name);
							}
						}
						list4.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list4);
					}
					if ((Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.Alpha5)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 5))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list5 = new List<string>();
						for (int num = 0; num < SettingsManager.modObjects.Length; num++)
						{
							if (SettingsManager.modObjects[num].enabled && SettingsManager.modObjects[num].enableSplits)
							{
								list5.Add((SettingsManager.modObjects[num].options.Count > 0) ? (SettingsManager.modObjects[num].name + "[" + SettingsManager.modObjects[num].options[SettingsManager.modObjects[num].option] + "]") : SettingsManager.modObjects[num].name);
							}
						}
						list5.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list5);
					}
					if ((Input.GetKey(KeyCode.Keypad6) || Input.GetKey(KeyCode.Alpha6)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 6))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list6 = new List<string>();
						for (int num2 = 0; num2 < SettingsManager.modObjects.Length; num2++)
						{
							if (SettingsManager.modObjects[num2].enabled && SettingsManager.modObjects[num2].enableSplits)
							{
								list6.Add((SettingsManager.modObjects[num2].options.Count > 0) ? (SettingsManager.modObjects[num2].name + "[" + SettingsManager.modObjects[num2].options[SettingsManager.modObjects[num2].option] + "]") : SettingsManager.modObjects[num2].name);
							}
						}
						list6.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list6);
					}
					if ((Input.GetKey(KeyCode.Keypad7) || Input.GetKey(KeyCode.Alpha7)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 7))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list7 = new List<string>();
						for (int num3 = 0; num3 < SettingsManager.modObjects.Length; num3++)
						{
							if (SettingsManager.modObjects[num3].enabled && SettingsManager.modObjects[num3].enableSplits)
							{
								list7.Add((SettingsManager.modObjects[num3].options.Count > 0) ? (SettingsManager.modObjects[num3].name + "[" + SettingsManager.modObjects[num3].options[SettingsManager.modObjects[num3].option] + "]") : SettingsManager.modObjects[num3].name);
							}
						}
						list7.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list7);
					}
					if ((Input.GetKey(KeyCode.Keypad8) || Input.GetKey(KeyCode.Alpha8)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 8))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list8 = new List<string>();
						for (int num4 = 0; num4 < SettingsManager.modObjects.Length; num4++)
						{
							if (SettingsManager.modObjects[num4].enabled && SettingsManager.modObjects[num4].enableSplits)
							{
								list8.Add((SettingsManager.modObjects[num4].options.Count > 0) ? (SettingsManager.modObjects[num4].name + "[" + SettingsManager.modObjects[num4].options[SettingsManager.modObjects[num4].option] + "]") : SettingsManager.modObjects[num4].name);
							}
						}
						list8.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list8);
					}
					if ((Input.GetKey(KeyCode.Keypad9) || Input.GetKey(KeyCode.Alpha9)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 9))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list9 = new List<string>();
						for (int num5 = 0; num5 < SettingsManager.modObjects.Length; num5++)
						{
							if (SettingsManager.modObjects[num5].enabled && SettingsManager.modObjects[num5].enableSplits)
							{
								list9.Add((SettingsManager.modObjects[num5].options.Count > 0) ? (SettingsManager.modObjects[num5].name + "[" + SettingsManager.modObjects[num5].options[SettingsManager.modObjects[num5].option] + "]") : SettingsManager.modObjects[num5].name);
							}
						}
						list9.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list9);
					}
					if ((Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.Alpha0)) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], 0))
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list10 = new List<string>();
						for (int num6 = 0; num6 < SettingsManager.modObjects.Length; num6++)
						{
							if (SettingsManager.modObjects[num6].enabled && SettingsManager.modObjects[num6].enableSplits)
							{
								list10.Add((SettingsManager.modObjects[num6].options.Count > 0) ? (SettingsManager.modObjects[num6].name + "[" + SettingsManager.modObjects[num6].options[SettingsManager.modObjects[num6].option] + "]") : SettingsManager.modObjects[num6].name);
							}
						}
						list10.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list10);
					}
				}
				if (Input.GetKeyDown(KeyCode.D) && SettingsManager.debugGUIEnabled)
				{
					this.debugGUI = !this.debugGUI;
				}
				if (Input.GetKeyDown(KeyCode.LeftShift))
				{
					SettingsManager.timer.shiftEnabled = true;
				}
				if (Input.GetKeyUp(KeyCode.LeftShift))
				{
					SettingsManager.timer.shiftEnabled = false;
				}
				if (Input.GetKeyDown(KeyCode.T))
				{
					if (Input.GetKey(KeyCode.LeftShift))
					{
						if (SettingsManager.timer.optionsSelect[0] > 0)
						{
							SettingsManager.timer.optionsSelect[0] = ((SettingsManager.timer.optionsSelect[0] == 1) ? 2 : 1);
							PlayerPrefs.SetInt(SettingsManager.timer.optionsTitle[0].Trim(), SettingsManager.timer.optionsSelect[0]);
							PlayerPrefs.Save();
						}
					}
					else
					{
						SettingsManager.showTimer = !SettingsManager.showTimer;
						PlayerPrefs.SetInt("ShowTimer", SettingsManager.showTimer ? 1 : 0);
						PlayerPrefs.Save();
					}
				}
				if (this.showMenu && SettingsManager.debugGUIEnabled && this.debugGUI && this.selectedSlider >= 0)
				{
					if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
					{
						switch (this.selectedSlider)
						{
						case 0:
							SettingsManager.bodyMultiplier = Math.Min(SettingsManager.bodyMultiplier + 0.01, 2.0);
							this.SetBodyMultiplier((float)SettingsManager.bodyMultiplier);
							break;
						case 1:
							SettingsManager.hammerMultiplier = Math.Min(SettingsManager.hammerMultiplier + 0.01, 8.0);
							this.SetHammerMultiplier((float)SettingsManager.hammerMultiplier);
							break;
						case 2:
							SettingsManager.potSlidingFriction = Math.Min(SettingsManager.potSlidingFriction + 0.01, 2.0);
							this.SetPotSlidingFriction((float)SettingsManager.potSlidingFriction);
							break;
						case 3:
							SettingsManager.potStaticFriction = Math.Min(SettingsManager.potStaticFriction + 0.01, 2.0);
							this.SetPotStaticFriction((float)SettingsManager.potStaticFriction);
							break;
						case 4:
							SettingsManager.hammerSlidingFriction = Math.Min(SettingsManager.hammerSlidingFriction + 0.01, 2.0);
							this.SetHammerSlidingFriction((float)SettingsManager.hammerSlidingFriction);
							break;
						case 5:
							SettingsManager.hammerStaticFriction = Math.Min(SettingsManager.hammerStaticFriction + 0.01, 2.0);
							this.SetHammerStaticFriction((float)SettingsManager.hammerStaticFriction);
							break;
						case 6:
							SettingsManager.potSlidingBounciness = Math.Min(SettingsManager.potSlidingBounciness + 0.01, 2.0);
							this.SetPotSlidingFriction((float)SettingsManager.potSlidingBounciness);
							break;
						case 7:
							SettingsManager.potStaticBounciness = Math.Min(SettingsManager.potStaticBounciness + 0.01, 2.0);
							this.SetPotStaticFriction((float)SettingsManager.potStaticBounciness);
							break;
						case 8:
							SettingsManager.hammerSlidingBounciness = Math.Min(SettingsManager.hammerSlidingBounciness + 0.01, 2.0);
							this.SetHammerSlidingFriction((float)SettingsManager.hammerSlidingBounciness);
							break;
						case 9:
							SettingsManager.hammerStaticBounciness = Math.Min(SettingsManager.hammerStaticBounciness + 0.01, 2.0);
							this.SetHammerStaticFriction((float)SettingsManager.hammerStaticBounciness);
							break;
						case 10:
							SettingsManager.gravityX = Math.Min(SettingsManager.gravityX + 0.01, 60.0);
							this.SetGravity((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
							break;
						case 11:
							SettingsManager.gravityY = Math.Min(SettingsManager.gravityY + 0.01, 60.0);
							this.SetGravity((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
							break;
						case 12:
							SettingsManager.cameraDistance = Math.Min(SettingsManager.cameraDistance + 0.01, 100.0);
							this.SetCameraDistance((float)SettingsManager.cameraDistance);
							break;
						case 13:
							SettingsManager.cursorRange = Math.Min(SettingsManager.cursorRange + 0.01, 30.0);
							this.SetCursorRange((float)SettingsManager.cursorRange);
							break;
						case 14:
							SettingsManager.potColorR = Math.Min(SettingsManager.potColorR + 0.01, 1.0);
							this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
							break;
						case 15:
							SettingsManager.potColorG = Math.Min(SettingsManager.potColorG + 0.01, 1.0);
							this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
							break;
						case 16:
							SettingsManager.potColorB = Math.Min(SettingsManager.potColorB + 0.01, 1.0);
							this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
							break;
						case 17:
							SettingsManager.potGoldness = Math.Min(SettingsManager.potGoldness + 0.01, 1.0);
							this.SetPotGoldness((float)SettingsManager.potGoldness);
							break;
						}
					}
					if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
					{
						switch (this.selectedSlider)
						{
						case 0:
							SettingsManager.bodyMultiplier = Math.Max(SettingsManager.bodyMultiplier - 0.01, 0.1);
							this.SetBodyMultiplier((float)SettingsManager.bodyMultiplier);
							break;
						case 1:
							SettingsManager.hammerMultiplier = Math.Max(SettingsManager.hammerMultiplier - 0.01, 0.5);
							this.SetHammerMultiplier((float)SettingsManager.hammerMultiplier);
							break;
						case 2:
							SettingsManager.potSlidingFriction = Math.Max(SettingsManager.potSlidingFriction - 0.01, 0.0);
							this.SetPotSlidingFriction((float)SettingsManager.potSlidingFriction);
							break;
						case 3:
							SettingsManager.potStaticFriction = Math.Max(SettingsManager.potStaticFriction - 0.01, 0.0);
							this.SetPotStaticFriction((float)SettingsManager.potStaticFriction);
							break;
						case 4:
							SettingsManager.hammerSlidingFriction = Math.Max(SettingsManager.hammerSlidingFriction - 0.01, 0.0);
							this.SetHammerSlidingFriction((float)SettingsManager.hammerSlidingFriction);
							break;
						case 5:
							SettingsManager.hammerStaticFriction = Math.Max(SettingsManager.hammerStaticFriction - 0.01, 0.0);
							this.SetHammerStaticFriction((float)SettingsManager.hammerStaticFriction);
							break;
						case 6:
							SettingsManager.potSlidingBounciness = Math.Max(SettingsManager.potSlidingBounciness - 0.01, 0.0);
							this.SetPotSlidingFriction((float)SettingsManager.potSlidingBounciness);
							break;
						case 7:
							SettingsManager.potStaticBounciness = Math.Max(SettingsManager.potStaticBounciness - 0.01, 0.0);
							this.SetPotStaticFriction((float)SettingsManager.potStaticBounciness);
							break;
						case 8:
							SettingsManager.hammerSlidingBounciness = Math.Max(SettingsManager.hammerSlidingBounciness - 0.01, 0.0);
							this.SetHammerSlidingFriction((float)SettingsManager.hammerSlidingBounciness);
							break;
						case 9:
							SettingsManager.hammerStaticBounciness = Math.Max(SettingsManager.hammerStaticBounciness - 0.01, 0.0);
							this.SetHammerStaticFriction((float)SettingsManager.hammerStaticBounciness);
							break;
						case 10:
							SettingsManager.gravityX = Math.Max(SettingsManager.gravityX - 0.01, -60.0);
							this.SetGravity((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
							break;
						case 11:
							SettingsManager.gravityY = Math.Max(SettingsManager.gravityY - 0.01, -60.0);
							this.SetGravity((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
							break;
						case 12:
							SettingsManager.cameraDistance = Math.Max(SettingsManager.cameraDistance - 0.01, 0.1);
							this.SetCameraDistance((float)SettingsManager.cameraDistance);
							break;
						case 13:
							SettingsManager.cursorRange = Math.Max(SettingsManager.cursorRange - 0.01, 1.0);
							this.SetCursorRange((float)SettingsManager.cursorRange);
							break;
						case 14:
							SettingsManager.potColorR = Math.Max(SettingsManager.potColorR - 0.01, 0.0);
							this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
							break;
						case 15:
							SettingsManager.potColorG = Math.Max(SettingsManager.potColorG - 0.01, 0.0);
							this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
							break;
						case 16:
							SettingsManager.potColorB = Math.Max(SettingsManager.potColorB - 0.01, 0.0);
							this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
							break;
						case 17:
							SettingsManager.potGoldness = Math.Max(SettingsManager.potGoldness - 0.01, 0.0);
							this.SetPotGoldness((float)SettingsManager.potGoldness);
							break;
						}
					}
				}
				if (this.showMenu && this.player)
				{
					if (Input.GetKey(KeyCode.UpArrow) && this.updateTimer > this.upTimer)
					{
						this.TeleportPos(this.player.transform.localPosition.x + Mathf.Cos((90f + SettingsManager.sideWayDegrees) * 0.017453292f) * this.teleportSpeed, this.player.transform.localPosition.y + Mathf.Sin((90f + SettingsManager.sideWayDegrees) * 0.017453292f) * this.teleportSpeed);
						this.upTimer = this.updateTimer + 0.001f;
					}
					if (Input.GetKey(KeyCode.DownArrow) && this.updateTimer > this.downTimer)
					{
						this.TeleportPos(this.player.transform.localPosition.x + Mathf.Cos((270f + SettingsManager.sideWayDegrees) * 0.017453292f) * this.teleportSpeed, this.player.transform.localPosition.y + Mathf.Sin((270f + SettingsManager.sideWayDegrees) * 0.017453292f) * this.teleportSpeed);
						this.downTimer = this.updateTimer + 0.001f;
					}
					if (Input.GetKey(KeyCode.LeftArrow) && this.updateTimer > this.leftTimer)
					{
						this.TeleportPos(this.player.transform.localPosition.x + Mathf.Cos((180f + SettingsManager.sideWayDegrees) * 0.017453292f) * this.teleportSpeed, this.player.transform.localPosition.y + Mathf.Sin((180f + SettingsManager.sideWayDegrees) * 0.017453292f) * this.teleportSpeed);
						this.leftTimer = this.updateTimer + 0.001f;
					}
					if (Input.GetKey(KeyCode.RightArrow) && this.updateTimer > this.rightTimer)
					{
						this.TeleportPos(this.player.transform.localPosition.x + Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) * this.teleportSpeed, this.player.transform.localPosition.y + Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) * this.teleportSpeed);
						this.rightTimer = this.updateTimer + 0.001f;
					}
				}
			}
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				if (Input.GetKeyDown(KeyCode.M) && SettingsManager.modpackEnabled)
				{
					this.ToggleDebugMenu();
					SettingsManager.timer.enabled = false;
				}
				if (SettingsManager.teleportEnabled && !this.showMenu)
				{
					if (Input.GetKey(KeyCode.Mouse0) && SettingsManager.modPackManager.LoadCustomState())
					{
						this.timeReset = Time.timeSinceLevelLoad;
						List<string> list11 = new List<string>();
						for (int num7 = 0; num7 < SettingsManager.modObjects.Length; num7++)
						{
							if (SettingsManager.modObjects[num7].enabled && SettingsManager.modObjects[num7].enableSplits)
							{
								list11.Add((SettingsManager.modObjects[num7].options.Count > 0) ? (SettingsManager.modObjects[num7].name + "[" + SettingsManager.modObjects[num7].options[SettingsManager.modObjects[num7].option] + "]") : SettingsManager.modObjects[num7].name);
							}
						}
						list11.Sort();
						SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list11);
					}
					if (Input.GetKeyDown(KeyCode.Mouse1))
					{
						SettingsManager.modPackManager.SaveCustomState();
					}
					if (Input.GetKeyDown(KeyCode.Mouse2))
					{
						SettingsManager.modPackManager.SaveStateToClipboardJson();
					}
				}
			}
			if (SettingsManager.timer.run != null && QualitySettings.vSyncCount == 0)
			{
				SettingsManager.timer.Update(Time.timeSinceLevelLoad - this.timeReset, this.player.transform.position);
			}
			bool activeSelf = this.menu.activeSelf;
		}
	}

	// Token: 0x06000306 RID: 774 RVA: 0x0000414C File Offset: 0x0000234C
	public void SetSFXVolume(float newVolume)
	{
		this.mixer.SetFloat("SFXVol", newVolume);
		this.mixer.SetFloat("AmbienceVol", newVolume);
		PlayerPrefs.SetFloat("SFXVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x06000307 RID: 775 RVA: 0x00004182 File Offset: 0x00002382
	public void SetVoiceoverVolume(float newVolume)
	{
		this.mixer.SetFloat("VoiceVol", newVolume);
		PlayerPrefs.SetFloat("VoiceVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x06000308 RID: 776 RVA: 0x000041A6 File Offset: 0x000023A6
	public void SetMusicVolume(float newVolume)
	{
		this.mixer.SetFloat("MusicVol", newVolume * 0.9f);
		PlayerPrefs.SetFloat("MusicVolume", newVolume);
		PlayerPrefs.Save();
	}

	// Token: 0x06000309 RID: 777 RVA: 0x0002BA1C File Offset: 0x00029C1C
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

	// Token: 0x0600030A RID: 778 RVA: 0x0002BA6C File Offset: 0x00029C6C
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

	// Token: 0x0600030B RID: 779 RVA: 0x0002BB64 File Offset: 0x00029D64
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

	// Token: 0x0600030C RID: 780 RVA: 0x000041D0 File Offset: 0x000023D0
	public void ToggleFullscreen(bool fs)
	{
		Screen.fullScreen = fs;
		PlayerPrefs.SetInt("Fullscreen", fs ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x0600030D RID: 781 RVA: 0x000041EE File Offset: 0x000023EE
	public void ToggleVsync(bool vs)
	{
		QualitySettings.vSyncCount = (vs ? 1 : 0);
		PlayerPrefs.SetInt("Vsync", vs ? 1 : 0);
		PlayerPrefs.Save();
	}

	// Token: 0x0600030E RID: 782 RVA: 0x0002BBEC File Offset: 0x00029DEC
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

	// Token: 0x0600030F RID: 783 RVA: 0x0002BC3C File Offset: 0x00029E3C
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

	// Token: 0x06000310 RID: 784 RVA: 0x0002BCE4 File Offset: 0x00029EE4
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

	// Token: 0x06000311 RID: 785 RVA: 0x0002BD38 File Offset: 0x00029F38
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

	// Token: 0x06000312 RID: 786 RVA: 0x0002BE48 File Offset: 0x0002A048
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
		SetFogQuality[] array = global::UnityEngine.Object.FindObjectsOfType<SetFogQuality>();
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

	// Token: 0x06000313 RID: 787 RVA: 0x0002BEFC File Offset: 0x0002A0FC
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

	// Token: 0x06000314 RID: 788 RVA: 0x0002C018 File Offset: 0x0002A218
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
			return;
		}
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

	// Token: 0x06000315 RID: 789 RVA: 0x00004212 File Offset: 0x00002412
	public void QuitGame()
	{
		Camera.main.SendMessage("FadeOut");
		base.StartCoroutine(this.QuitDone());
	}

	// Token: 0x06000316 RID: 790 RVA: 0x00004230 File Offset: 0x00002430
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

	// Token: 0x06000317 RID: 791 RVA: 0x0002C644 File Offset: 0x0002A844
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

	// Token: 0x06000318 RID: 792 RVA: 0x00004238 File Offset: 0x00002438
	public void Ouch(int i)
	{
		Debug.Log("Ouch! " + i.ToString());
	}

	// Token: 0x06000319 RID: 793 RVA: 0x00004250 File Offset: 0x00002450
	private IEnumerator ApplyLetterbox()
	{
		yield return null;
		yield return null;
		if (this.ratioFitter != null)
		{
			Vector2 vector;
			if (Screen.width >= Screen.height)
			{
				vector = new Vector2((float)Screen.width / (float)Screen.height, 1f);
			}
			else
			{
				vector = new Vector2((float)Screen.width, (float)Screen.height);
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
			}
			this.ratioFitter.ratio = vector;
		}
		this.ratioFitter.CalculateAndSetAllCameraRatios();
		yield break;
	}

	// Token: 0x0600031A RID: 794 RVA: 0x0000265E File Offset: 0x0000085E
	private void OnApplicationQuit()
	{
	}

	// Token: 0x0600031B RID: 795 RVA: 0x0002C6E4 File Offset: 0x0002A8E4
	public SettingsManager()
	{
		if (!SettingsManager.modpackLoad)
		{
			SettingsManager.bodyMultiplier = 1.0;
			SettingsManager.cursorRange = 3.5;
			SettingsManager.potSlidingFriction = 0.3;
			SettingsManager.potStaticFriction = 0.8;
			SettingsManager.hammerSlidingFriction = 0.7;
			SettingsManager.hammerStaticFriction = 1.0;
			SettingsManager.potSlidingBounciness = 0.0;
			SettingsManager.potStaticBounciness = 0.0;
			SettingsManager.hammerSlidingBounciness = 0.0;
			SettingsManager.hammerStaticBounciness = 0.0;
			SettingsManager.gravityX = 0.0;
			SettingsManager.gravityY = -30.0;
			SettingsManager.cameraDistance = 5.0;
			SettingsManager.potColorR = 1.0;
			SettingsManager.potColorG = 1.0;
			SettingsManager.potColorB = 1.0;
			if (PlayerPrefs.HasKey("ModPotCustomizerRGB_R"))
			{
				SettingsManager.potColorR = (double)PlayerPrefs.GetFloat("ModPotCustomizerRGB_R");
			}
			if (PlayerPrefs.HasKey("ModPotCustomizerRGB_G"))
			{
				SettingsManager.potColorG = (double)PlayerPrefs.GetFloat("ModPotCustomizerRGB_G");
			}
			if (PlayerPrefs.HasKey("ModPotCustomizerRGB_B"))
			{
				SettingsManager.potColorB = (double)PlayerPrefs.GetFloat("ModPotCustomizerRGB_B");
			}
			SettingsManager.potGoldness = -1.0;
			SettingsManager.category = 1;
			SettingsManager.categorySelect = 1;
			SettingsManager.sideWay = false;
			SettingsManager.sideWayDegrees = 0f;
			SettingsManager.fastStart = false;
			SettingsManager.fastStartTime = 6f;
			SettingsManager.showTimer = false;
			SettingsManager.debugGUIEnabled = false;
			SettingsManager.reachedSpace = false;
			if (PlayerPrefs.HasKey("ShowTimer"))
			{
				SettingsManager.showTimer = PlayerPrefs.GetInt("ShowTimer") == 1;
			}
			else
			{
				PlayerPrefs.SetInt("ShowTimer", 1);
				SettingsManager.showTimer = true;
			}
			SettingsManager.categoryNames = new string[]
			{
				"Custom", "Glitchless", "Fat%", "Small%", "No Friction", "Low Friction", "Space%", "150% Gravity", "No Gravity", "Tiny Hammer",
				"Big Hammer", "Bouncy", "ZoomOut"
			};
			SettingsManager.categoryCount = SettingsManager.categoryNames.Length;
			SettingsManager.mapPositions = new Vector2[]
			{
				new Vector2(57.55f, 330.4f),
				new Vector2(18.4f, 56.05f),
				new Vector2(12f, 110.95f),
				new Vector2(1.1f, 134.05f),
				new Vector2(-2f, 163.5f),
				new Vector2(50f, 227.5f),
				new Vector2(67.5f, 244.75f),
				new Vector2(79.8f, 262.65f),
				new Vector2(23.5f, 260.6f),
				new Vector2(20.9f, 283.23f)
			};
			SettingsManager.teleportNames = new string[] { "#1 Devil's Chimney", "#2 Slide Skip", "#3 Furniture Skip", "#4 Orange Hell", "#5 Hat Jump", "#6 Anvil Jump", "#7 Boulders", "#8 Bucket", "#9 Ice Mountain", "#0 Tower" };
			SettingsManager.teleportSelect = 0;
			SettingsManager.teleportCount = SettingsManager.teleportNames.Length;
			SettingsManager.optionsNames = new string[] { "Multiplayer", "Modpack", "Teleport" };
			SettingsManager.mapObjects = new Map[]
			{
				new MapDefault(),
				new MapBuckets(),
				new MapHard(),
				new MapXmas(),
				new MapThumbs(),
				new MapBigTree(),
				new MapYeetLand(),
				new MapImpossible()
			};
			SettingsManager.mapSelect = 0;
			SettingsManager.mapCount = SettingsManager.mapObjects.Length;
			SettingsManager.map = 0;
			SettingsManager.modObjects = new Mod[]
			{
				new ModMultiplayer(),
				new ModOranges(),
				new ModInvisible(),
				new ModOnlyHammer(),
				new ModMountainNotches(),
				new ModRollingOverIt(),
				new ModInvisibleMountain(),
				new ModSideways(),
				new ModPotCustomizer(),
				new ModDioCustomizer(),
				new ModTrails(),
				new ModFastStart()
			};
			SettingsManager.modSelect = 0;
			SettingsManager.modSelectOption = SettingsManager.modObjects[SettingsManager.modSelect].option;
			SettingsManager.modCount = SettingsManager.modObjects.Length;
			SettingsManager.modPackManager = new ModPackManager();
			SettingsManager.timer = new Timer();
			SettingsManager.timerSelect = 0;
			SettingsManager.timerSelectOption = SettingsManager.timer.optionsSelect[0];
			SettingsManager.modpackEnabled = !PlayerPrefs.HasKey("ModpackEnabled") || PlayerPrefs.GetInt("ModpackEnabled") == 1;
			SettingsManager.teleportEnabled = !PlayerPrefs.HasKey("TeleportEnabled") || PlayerPrefs.GetInt("TeleportEnabled") == 1;
			Debug.Log(string.Format("Modpack build {0} {1} by anjo2 loaded", SettingsManager.build, SettingsManager.buildNames[SettingsManager.buildType]).Replace("  ", " "));
			SettingsManager.modpackLoad = true;
			SettingsManager.updateManager.GetModpackUpdate(SettingsManager.build);
		}
	}

	// Token: 0x0600031C RID: 796 RVA: 0x0002CC9C File Offset: 0x0002AE9C
	private void FixedUpdate()
	{
		if (SceneManager.GetActiveScene().name == "Mian" && this.player && !this.menu.activeSelf && !this.showMenu)
		{
			string text = SettingsManager.mapObjects[SettingsManager.map].Update(Time.deltaTime, this.player);
			if (text.Length > 0)
			{
				this.mapDebugText = text;
			}
			for (int i = 0; i < SettingsManager.modObjects.Length; i++)
			{
				try
				{
					if (SettingsManager.modObjects[i].enabled)
					{
						SettingsManager.modObjects[i].Update(Time.deltaTime, this.player);
					}
				}
				catch
				{
				}
			}
			if (SettingsManager.timer.run != null && QualitySettings.vSyncCount == 1)
			{
				SettingsManager.timer.Update(Time.timeSinceLevelLoad - this.timeReset, this.player.transform.position);
			}
		}
	}

	// Token: 0x0600031D RID: 797 RVA: 0x0002CDAC File Offset: 0x0002AFAC
	public void OnGUI()
	{
		GUI.skin.button.normal.textColor = Color.white;
		GUI.skin.label.normal.textColor = Color.white;
		if (this.debugGUI && SettingsManager.debugGUIEnabled && SceneManager.GetActiveScene().name == "Mian" && !this.menu.activeSelf)
		{
			GUI.skin.horizontalSlider = this._sliderBackgroundStyle;
			TextAnchor alignment = GUI.skin.label.alignment;
			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;
			float num = 6f;
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label(" Body Multiplier: " + SettingsManager.bodyMultiplier.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Body Multiplier: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 0) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num2 = GUILayout.HorizontalSlider((float)SettingsManager.bodyMultiplier * 100f, 10f, 200f, new GUILayoutOption[0]) / 100f;
				if (num2 != (float)SettingsManager.bodyMultiplier)
				{
					SettingsManager.bodyMultiplier = (double)num2;
					this.SetBodyMultiplier(num2);
					this.selectedSlider = 0;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Body Multiplier: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.bodyMultiplier = 1.0;
					this.SetBodyMultiplier(1f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Hammer Multiplier: " + SettingsManager.hammerMultiplier.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Multiplier: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 1) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num3 = GUILayout.HorizontalSlider((float)SettingsManager.hammerMultiplier, 0.5f, 8f, new GUILayoutOption[0]);
				if (num3 != (float)SettingsManager.hammerMultiplier)
				{
					SettingsManager.hammerMultiplier = (double)num3;
					this.SetHammerMultiplier(num3);
					this.selectedSlider = 1;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Multiplier: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.hammerMultiplier = 1.0;
					this.SetHammerMultiplier(1f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Cursor Range: " + SettingsManager.cursorRange.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(44f + GUI.skin.label.CalcSize(new GUIContent(" Cursor Range: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 13) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num4 = GUILayout.HorizontalSlider((float)SettingsManager.cursorRange, 1f, 30f, new GUILayoutOption[0]);
				if (num4 != (float)SettingsManager.cursorRange)
				{
					SettingsManager.cursorRange = (double)num4;
					this.SetCursorRange(num4);
					this.selectedSlider = 13;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(350f + GUI.skin.label.CalcSize(new GUIContent(" Cursor Range: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.cursorRange = 3.5;
					this.SetCursorRange(3.5f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Pot Sliding Friction: " + SettingsManager.potSlidingFriction.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot Sliding Friction: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 2) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num5 = GUILayout.HorizontalSlider((float)SettingsManager.potSlidingFriction, 0f, 2f, new GUILayoutOption[0]);
				if (num5 != (float)SettingsManager.potSlidingFriction)
				{
					SettingsManager.potSlidingFriction = (double)num5;
					this.SetPotSlidingFriction(num5);
					this.selectedSlider = 2;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot Sliding Friction: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.potSlidingFriction = 0.3;
					this.SetPotSlidingFriction(0.3f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Pot Static Friction: " + SettingsManager.potStaticFriction.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot Static Friction: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 3) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num6 = GUILayout.HorizontalSlider((float)SettingsManager.potStaticFriction, 0f, 2f, new GUILayoutOption[0]);
				if (num6 != (float)SettingsManager.potStaticFriction)
				{
					SettingsManager.potStaticFriction = (double)num6;
					this.SetPotStaticFriction(num6);
					this.selectedSlider = 3;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot Static Friction: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.potStaticFriction = 0.8;
					this.SetPotStaticFriction(0.8f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Hammer Sliding Friction: " + SettingsManager.hammerSlidingFriction.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Sliding Friction: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 4) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num7 = GUILayout.HorizontalSlider((float)SettingsManager.hammerSlidingFriction, 0f, 2f, new GUILayoutOption[0]);
				if (num7 != (float)SettingsManager.hammerSlidingFriction)
				{
					SettingsManager.hammerSlidingFriction = (double)num7;
					this.SetHammerSlidingFriction(num7);
					this.selectedSlider = 4;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Sliding Friction: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.hammerSlidingFriction = 0.7;
					this.SetHammerSlidingFriction(0.7f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Hammer Static Friction: " + SettingsManager.hammerStaticFriction.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Static Friction: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 5) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num8 = GUILayout.HorizontalSlider((float)SettingsManager.hammerStaticFriction, 0f, 2f, new GUILayoutOption[0]);
				if (num8 != (float)SettingsManager.hammerStaticFriction)
				{
					SettingsManager.hammerStaticFriction = (double)num8;
					this.SetHammerStaticFriction(num8);
					this.selectedSlider = 5;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Static Friction: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.hammerStaticFriction = 1.0;
					this.SetHammerStaticFriction(1f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Pot Sliding Bounciness: " + SettingsManager.potSlidingBounciness.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot Sliding Bounciness: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 6) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num9 = GUILayout.HorizontalSlider((float)SettingsManager.potSlidingBounciness, 0f, 2f, new GUILayoutOption[0]);
				if (num9 != (float)SettingsManager.potSlidingBounciness)
				{
					SettingsManager.potSlidingBounciness = (double)num9;
					this.SetPotSlidingBounciness(num9);
					this.selectedSlider = 6;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot Sliding Bounciness: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.potSlidingBounciness = 0.0;
					this.SetPotSlidingBounciness(0f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Pot Static Bounciness: " + SettingsManager.potStaticBounciness.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot Static Bounciness: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 7) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num10 = GUILayout.HorizontalSlider((float)SettingsManager.potStaticBounciness, 0f, 2f, new GUILayoutOption[0]);
				if (num10 != (float)SettingsManager.potStaticBounciness)
				{
					SettingsManager.potStaticBounciness = (double)num10;
					this.SetPotStaticBounciness(num10);
					this.selectedSlider = 7;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot Static Bounciness: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.potStaticBounciness = 0.0;
					this.SetPotStaticBounciness(0f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Hammer Sliding Bounciness: " + SettingsManager.hammerSlidingBounciness.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Sliding Bounciness: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 8) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num11 = GUILayout.HorizontalSlider((float)SettingsManager.hammerSlidingBounciness, 0f, 2f, new GUILayoutOption[0]);
				if (num11 != (float)SettingsManager.hammerSlidingBounciness)
				{
					SettingsManager.hammerSlidingBounciness = (double)num11;
					this.SetHammerSlidingBounciness(num11);
					this.selectedSlider = 8;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Sliding Bounciness: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.hammerSlidingBounciness = 0.0;
					this.SetHammerSlidingBounciness(0f);
					this.selectedSlider = -1;
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Hammer Static Bounciness: " + SettingsManager.hammerStaticBounciness.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Static Bounciness: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 9) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num12 = GUILayout.HorizontalSlider((float)SettingsManager.hammerStaticBounciness, 0f, 2f, new GUILayoutOption[0]);
				if (num12 != (float)SettingsManager.hammerStaticBounciness)
				{
					SettingsManager.hammerStaticBounciness = (double)num12;
					this.SetHammerStaticBounciness(num12);
					this.selectedSlider = 9;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Hammer Static Bounciness: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.hammerStaticBounciness = 0.0;
					this.SetHammerStaticBounciness(0f);
					this.selectedSlider = -1;
				}
			}
			double num13 = SettingsManager.gravityX;
			double num14 = SettingsManager.gravityY;
			string text = " Gravity X: ";
			Vector2 vector = Physics2D.gravity;
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(text + vector.x.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(44f + GUI.skin.label.CalcSize(new GUIContent(" Gravity X: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 10) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				num13 = (double)GUILayout.HorizontalSlider((float)SettingsManager.gravityX, -60f, 60f, new GUILayoutOption[0]);
				GUILayout.EndArea();
			}
			string text2 = " Gravity Y: ";
			vector = Physics2D.gravity;
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(text2 + vector.y.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(44f + GUI.skin.label.CalcSize(new GUIContent(" Gravity Y: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 11) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				num14 = (double)GUILayout.HorizontalSlider((float)SettingsManager.gravityY, -60f, 60f, new GUILayoutOption[0]);
				GUILayout.EndArea();
				if (Mathf.Abs((float)SettingsManager.gravityX - (float)num13) > 0.001f)
				{
					this.selectedSlider = 10;
				}
				else if (Mathf.Abs((float)SettingsManager.gravityY - (float)num14) > 0.001f)
				{
					this.selectedSlider = 11;
				}
				if (GUI.Button(new Rect(350f + GUI.skin.label.CalcSize(new GUIContent(" Gravity X: ")).x, num - 29f, 70f, 17f), "RESET"))
				{
					num13 = (SettingsManager.sideWay ? ((double)SettingsManager.sideGravityX(0f, -30f)) : 0.0);
					SettingsManager.category = 0;
					this.selectedSlider = -1;
				}
				if (GUI.Button(new Rect(350f + GUI.skin.label.CalcSize(new GUIContent(" Gravity Y: ")).x, num - 4f, 70f, 17f), "RESET"))
				{
					num14 = (SettingsManager.sideWay ? ((double)SettingsManager.sideGravityY(0f, -30f)) : (-30.0));
					SettingsManager.category = 0;
					this.selectedSlider = -1;
				}
				if (Math.Abs((float)SettingsManager.gravityX - (float)num13) > 0.001f || Math.Abs((float)SettingsManager.gravityY - (float)num14) > 0.001f)
				{
					SettingsManager.gravityX = num13;
					SettingsManager.gravityY = num14;
					this.SetGravity((float)num13, (float)num14);
				}
			}
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(" Camera Distance: " + SettingsManager.cameraDistance.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Camera Distance: ")).x, num, 300f, 10f));
				GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 12) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
				float num15 = GUILayout.HorizontalSlider((float)SettingsManager.cameraDistance, 0.1f, 100f, new GUILayoutOption[0]);
				if (num15 != (float)SettingsManager.cameraDistance)
				{
					SettingsManager.cameraDistance = (double)num15;
					this.SetCameraDistance(num15);
					this.selectedSlider = 12;
				}
				GUILayout.EndArea();
				if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Camera Distance: ")).x, num - 4f, 65f, 17f), "RESET"))
				{
					SettingsManager.cameraDistance = 5.0;
					this.SetCameraDistance(5f);
					this.selectedSlider = -1;
				}
			}
			if (Screen.height >= 720)
			{
				num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
				GUILayout.Label(" Pot R: " + SettingsManager.potColorR.ToString("0.00"), new GUILayoutOption[0]);
				if (this.showMenu)
				{
					GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot R: ")).x, num, 300f, 10f));
					GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 14) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
					float num16 = GUILayout.HorizontalSlider((float)SettingsManager.potColorR, 0f, 1f, new GUILayoutOption[0]);
					if (num16 != (float)SettingsManager.potColorR)
					{
						SettingsManager.potColorR = (double)num16;
						this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
						this.selectedSlider = 14;
					}
					GUILayout.EndArea();
					if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot R: ")).x, num - 4f, 65f, 17f), "RESET"))
					{
						SettingsManager.potColorR = 1.0;
						this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
						this.selectedSlider = -1;
					}
				}
				num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
				GUILayout.Label(" Pot G: " + SettingsManager.potColorG.ToString("0.00"), new GUILayoutOption[0]);
				if (this.showMenu)
				{
					GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot G: ")).x, num, 300f, 10f));
					GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 15) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
					float num17 = GUILayout.HorizontalSlider((float)SettingsManager.potColorG, 0f, 1f, new GUILayoutOption[0]);
					if (num17 != (float)SettingsManager.potColorG)
					{
						SettingsManager.potColorG = (double)num17;
						this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
						this.selectedSlider = 15;
					}
					GUILayout.EndArea();
					if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot G: ")).x, num - 4f, 65f, 17f), "RESET"))
					{
						SettingsManager.potColorG = 1.0;
						this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
						this.selectedSlider = -1;
					}
				}
				num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
				GUILayout.Label(" Pot B: " + SettingsManager.potColorB.ToString("0.00"), new GUILayoutOption[0]);
				if (this.showMenu)
				{
					GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot B: ")).x, num, 300f, 10f));
					GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 16) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
					float num18 = GUILayout.HorizontalSlider((float)SettingsManager.potColorB, 0f, 1f, new GUILayoutOption[0]);
					if (num18 != (float)SettingsManager.potColorB)
					{
						SettingsManager.potColorB = (double)num18;
						this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
						this.selectedSlider = 16;
					}
					GUILayout.EndArea();
					if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot R: ")).x, num - 4f, 65f, 17f), "RESET"))
					{
						SettingsManager.potColorB = 1.0;
						this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
						this.selectedSlider = -1;
					}
				}
				num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
				GUILayout.Label(" Pot Goldness: " + SettingsManager.potGoldness.ToString("0.00"), new GUILayoutOption[0]);
				if (this.showMenu)
				{
					GUILayout.BeginArea(new Rect(34f + GUI.skin.label.CalcSize(new GUIContent(" Pot Goldness: ")).x, num, 300f, 10f));
					GUI.skin.horizontalSliderThumb = ((this.selectedSlider == 17) ? this._sliderThumbStyleSelected : this._sliderThumbStyle);
					float num19 = GUILayout.HorizontalSlider((float)SettingsManager.potGoldness, 0f, 1f, new GUILayoutOption[0]);
					if (num19 != (float)SettingsManager.potGoldness)
					{
						SettingsManager.potGoldness = (double)num19;
						this.SetPotGoldness((float)SettingsManager.potGoldness);
						this.selectedSlider = 17;
					}
					GUILayout.EndArea();
					if (GUI.Button(new Rect(340f + GUI.skin.label.CalcSize(new GUIContent(" Pot Goldness: ")).x, num - 4f, 65f, 17f), "RESET"))
					{
						float num20 = (float)Mathf.Min(PlayerPrefs.GetInt("NumWins"), 50) / 50f;
						SettingsManager.potGoldness = (double)(num20 * num20);
						this.SetPotGoldness((float)SettingsManager.potGoldness);
						this.selectedSlider = -1;
					}
				}
			}
			GUI.skin.horizontalSliderThumb = this._sliderThumbStyle;
			string text3 = " Position X: ";
			Vector3 vector2 = this.player.transform.localPosition;
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(text3 + vector2.x.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(40f + GUI.skin.label.CalcSize(new GUIContent(" Position X: ")).x, num, 300f, 10f));
				num13 = (double)GUILayout.HorizontalSlider(this.player.transform.localPosition.x, -20f, 100f, new GUILayoutOption[0]);
				GUILayout.EndArea();
			}
			string text4 = " Position Y: ";
			vector2 = this.player.transform.localPosition;
			num += (SettingsManager.modObjects[0].enabled ? 26f : 25f);
			GUILayout.Label(text4 + vector2.y.ToString("0.00"), new GUILayoutOption[0]);
			if (this.showMenu)
			{
				GUILayout.BeginArea(new Rect(40f + GUI.skin.label.CalcSize(new GUIContent(" Position X: ")).x, num, 300f, 10f));
				num14 = (double)GUILayout.HorizontalSlider(this.player.transform.localPosition.y, 0f, 500f, new GUILayoutOption[0]);
				GUILayout.EndArea();
				if ((float)num13 != this.player.transform.localPosition.x || (float)num14 != this.player.transform.localPosition.y)
				{
					this.TeleportPos((float)num13, (float)num14);
					this.selectedSlider = -1;
				}
			}
			if (PlayerPrefs.HasKey("MouseSensitivity"))
			{
				GUILayout.Label(" Mouse Sensitivity: " + PlayerPrefs.GetFloat("MouseSensitivity").ToString(), new GUILayoutOption[0]);
			}
			int num21;
			for (int i = 0; i < this.objectDetails.Count; i = num21 + 1)
			{
				GUILayout.Label(string.Concat(new object[]
				{
					" Object ",
					i,
					": ",
					this.objectDetails[i]
				}), new GUILayoutOption[0]);
				num21 = i;
			}
			if (this.mapDebug && this.mapDebugText.Length > 0)
			{
				GUILayout.Label(" Map Debug: " + this.mapDebugText, new GUILayoutOption[0]);
			}
			GUILayout.EndVertical();
		}
		else
		{
			this.selectedSlider = -1;
		}
		GUILayout.BeginVertical(new GUILayoutOption[0]);
		if (SceneManager.GetActiveScene().name != "Mian")
		{
			GUILayout.BeginArea(new Rect(5f, (float)this.currentRes.height - 235f, 450f, 100f));
			GUILayout.Label("Timer options:", new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("<=", new GUILayoutOption[0]))
			{
				if (SettingsManager.timerSelect == 0)
				{
					SettingsManager.timerSelect = SettingsManager.timer.options.Count - 1;
					SettingsManager.timerSelectOption = SettingsManager.timer.optionsSelect[SettingsManager.timerSelect];
				}
				else
				{
					SettingsManager.timerSelect--;
					SettingsManager.timerSelectOption = SettingsManager.timer.optionsSelect[SettingsManager.timerSelect];
				}
			}
			double num22 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.timer.optionsTitle[SettingsManager.timerSelect])).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			GUI.skin.button.normal.textColor = Color.green;
			GUI.skin.button.hover.textColor = Color.green;
			GUILayout.Button(new string(' ', (int)Math.Floor(num22)) + SettingsManager.timer.optionsTitle[SettingsManager.timerSelect] + new string(' ', (int)Math.Round(num22)), new GUILayoutOption[0]);
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.hover.textColor = Color.white;
			if (GUILayout.Button("=>", new GUILayoutOption[0]))
			{
				if (SettingsManager.timerSelect == SettingsManager.timer.options.Count - 1)
				{
					SettingsManager.timerSelect = 0;
					SettingsManager.timerSelectOption = SettingsManager.timer.optionsSelect[SettingsManager.timerSelect];
				}
				else
				{
					SettingsManager.timerSelect++;
					SettingsManager.timerSelectOption = SettingsManager.timer.optionsSelect[SettingsManager.timerSelect];
				}
			}
			GUILayout.Label(" ", new GUILayoutOption[0]);
			if (SettingsManager.timer.options[SettingsManager.timerSelect].Count > 0)
			{
				if (GUILayout.Button("<=", new GUILayoutOption[0]))
				{
					if (SettingsManager.timerSelectOption == 0)
					{
						SettingsManager.timerSelectOption = SettingsManager.timer.options[SettingsManager.timerSelect].Count - 1;
					}
					else
					{
						SettingsManager.timerSelectOption--;
					}
				}
				num22 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.timer.options[SettingsManager.timerSelect][SettingsManager.timerSelectOption])).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
				GUI.skin.button.normal.textColor = ((SettingsManager.timer.optionsSelect[SettingsManager.timerSelect] != SettingsManager.timerSelectOption) ? Color.white : Color.green);
				GUI.skin.button.hover.textColor = ((SettingsManager.timer.optionsSelect[SettingsManager.timerSelect] != SettingsManager.timerSelectOption) ? Color.white : Color.green);
				if (GUILayout.Button(new string(' ', (int)Math.Floor(num22)) + SettingsManager.timer.options[SettingsManager.timerSelect][SettingsManager.timerSelectOption] + new string(' ', (int)Math.Round(num22)), new GUILayoutOption[0]))
				{
					SettingsManager.timer.optionsSelect[SettingsManager.timerSelect] = SettingsManager.timerSelectOption;
					SettingsManager.timer.resetCache();
					PlayerPrefs.SetInt(SettingsManager.timer.optionsTitle[SettingsManager.timerSelect].Trim(), SettingsManager.timerSelectOption);
					PlayerPrefs.Save();
				}
				GUI.skin.button.normal.textColor = Color.white;
				GUI.skin.button.hover.textColor = Color.white;
				if (GUILayout.Button("=>", new GUILayoutOption[0]))
				{
					if (SettingsManager.timerSelectOption == SettingsManager.timer.options[SettingsManager.timerSelect].Count - 1)
					{
						SettingsManager.timerSelectOption = 0;
					}
					else
					{
						SettingsManager.timerSelectOption++;
					}
				}
			}
			else
			{
				GUILayout.Space(225f);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		if (SceneManager.GetActiveScene().name != "Mian")
		{
			GUILayout.BeginArea(new Rect(5f, (float)this.currentRes.height - 180f, 225f, 100f));
			GUILayout.Label("Enable/Disable:", new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("<=", new GUILayoutOption[0]))
			{
				if (SettingsManager.optionsSelect == 0)
				{
					SettingsManager.optionsSelect = SettingsManager.optionsNames.Length - 1;
				}
				else
				{
					SettingsManager.optionsSelect--;
				}
			}
			double num23 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.optionsNames[SettingsManager.optionsSelect])).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			bool flag = false;
			switch (SettingsManager.optionsSelect)
			{
			case 0:
				flag = SettingsManager.modObjects[0].enabled;
				break;
			case 1:
				flag = SettingsManager.modpackEnabled;
				break;
			case 2:
				flag = SettingsManager.teleportEnabled;
				break;
			}
			GUI.skin.button.normal.textColor = (flag ? Color.green : Color.white);
			GUI.skin.button.hover.textColor = (flag ? Color.green : Color.white);
			if (GUILayout.Button(new string(' ', (int)Math.Floor(num23)) + SettingsManager.optionsNames[SettingsManager.optionsSelect] + new string(' ', (int)Math.Round(num23)), new GUILayoutOption[0]))
			{
				switch (SettingsManager.optionsSelect)
				{
				case 0:
					if (SettingsManager.modObjects[0].enabled)
					{
						SettingsManager.modObjects[0].UnLoad();
					}
					else
					{
						SettingsManager.modObjects[0].Load();
					}
					break;
				case 1:
					SettingsManager.modpackEnabled = !SettingsManager.modpackEnabled;
					if (!SettingsManager.modpackEnabled)
					{
						this.SetDefaultSettings();
						SettingsManager.debugGUIEnabled = false;
					}
					PlayerPrefs.SetInt("ModpackEnabled", SettingsManager.modpackEnabled ? 1 : 0);
					PlayerPrefs.Save();
					break;
				case 2:
					SettingsManager.teleportEnabled = !SettingsManager.teleportEnabled;
					PlayerPrefs.SetInt("TeleportEnabled", SettingsManager.teleportEnabled ? 1 : 0);
					PlayerPrefs.Save();
					break;
				}
			}
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.hover.textColor = Color.white;
			if (GUILayout.Button("=>", new GUILayoutOption[0]))
			{
				if (SettingsManager.optionsSelect == SettingsManager.optionsNames.Length - 1)
				{
					SettingsManager.optionsSelect = 0;
				}
				else
				{
					SettingsManager.optionsSelect++;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
		if ((this.showMenu && SceneManager.GetActiveScene().name == "Mian" && !this.menu.activeSelf) || SceneManager.GetActiveScene().name != "Mian")
		{
			if (SettingsManager.modSelect == 0)
			{
				SettingsManager.modSelect = 1;
			}
			GUILayout.BeginArea(new Rect(5f, (float)this.currentRes.height - 125f, 225f, 100f));
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label("Choose mods:", new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("<=", new GUILayoutOption[0]))
			{
				if (SettingsManager.modSelect == 1)
				{
					SettingsManager.modSelect = SettingsManager.modCount - 1;
					SettingsManager.modSelectOption = SettingsManager.modObjects[SettingsManager.modSelect].option;
				}
				else
				{
					SettingsManager.modSelect--;
					SettingsManager.modSelectOption = SettingsManager.modObjects[SettingsManager.modSelect].option;
				}
			}
			double num24 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.modObjects[SettingsManager.modSelect].name)).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			GUI.skin.button.normal.textColor = ((!SettingsManager.modObjects[SettingsManager.modSelect].enabled) ? Color.white : Color.green);
			GUI.skin.button.hover.textColor = ((!SettingsManager.modObjects[SettingsManager.modSelect].enabled) ? Color.white : Color.green);
			if (GUILayout.Button(new string(' ', Math.Max(1, (int)Math.Floor(num24))) + SettingsManager.modObjects[SettingsManager.modSelect].name.Substring(0, Math.Min(SettingsManager.modObjects[SettingsManager.modSelect].name.Length, 20)) + new string(' ', Math.Max(1, (int)Math.Round(num24))), new GUILayoutOption[0]))
			{
				this.selectedSlider = -1;
				this.timeModReset = Time.timeSinceLevelLoad;
				if (SettingsManager.modObjects[SettingsManager.modSelect].enabled)
				{
					SettingsManager.modObjects[SettingsManager.modSelect].UnLoad();
				}
				else
				{
					SettingsManager.modObjects[SettingsManager.modSelect].Load();
				}
				if (SceneManager.GetActiveScene().name != "Mian")
				{
					List<string> list = new List<string>();
					for (int j = 0; j < SettingsManager.modObjects.Length; j++)
					{
						if (SettingsManager.modObjects[j].enabled && SettingsManager.modObjects[j].enableSplits)
						{
							list.Add((SettingsManager.modObjects[j].options.Count > 0) ? (SettingsManager.modObjects[j].name + "[" + SettingsManager.modObjects[j].options[SettingsManager.modObjects[j].option] + "]") : SettingsManager.modObjects[j].name);
						}
					}
					list.Sort();
					SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list);
				}
			}
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.hover.textColor = Color.white;
			if (GUILayout.Button("=>", new GUILayoutOption[0]))
			{
				if (SettingsManager.modSelect == SettingsManager.modCount - 1)
				{
					SettingsManager.modSelect = 1;
					SettingsManager.modSelectOption = SettingsManager.modObjects[SettingsManager.modSelect].option;
				}
				else
				{
					SettingsManager.modSelect++;
					SettingsManager.modSelectOption = SettingsManager.modObjects[SettingsManager.modSelect].option;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
			if (SettingsManager.modObjects[SettingsManager.modSelect].options.Count > 0)
			{
				GUILayout.BeginArea(new Rect(245f, (float)this.currentRes.height - 125f, 225f, 100f));
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				GUILayout.Label((SettingsManager.modObjects[SettingsManager.modSelect].optionsTitle.Length > 0) ? (SettingsManager.modObjects[SettingsManager.modSelect].optionsTitle + ":") : "", new GUILayoutOption[0]);
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				if (GUILayout.Button("<=", new GUILayoutOption[0]))
				{
					if (SettingsManager.modSelectOption == 0)
					{
						SettingsManager.modSelectOption = SettingsManager.modObjects[SettingsManager.modSelect].options.Count - 1;
					}
					else
					{
						SettingsManager.modSelectOption--;
					}
				}
				num24 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.modObjects[SettingsManager.modSelect].options[SettingsManager.modSelectOption])).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
				GUI.skin.button.normal.textColor = ((SettingsManager.modObjects[SettingsManager.modSelect].option != SettingsManager.modSelectOption) ? Color.white : Color.green);
				GUI.skin.button.hover.textColor = ((SettingsManager.modObjects[SettingsManager.modSelect].option != SettingsManager.modSelectOption) ? Color.white : Color.green);
				if (GUILayout.Button(new string(' ', Math.Max(1, (int)Math.Floor(num24))) + SettingsManager.modObjects[SettingsManager.modSelect].options[SettingsManager.modSelectOption].Substring(0, Math.Min(SettingsManager.modObjects[SettingsManager.modSelect].options[SettingsManager.modSelectOption].Length, 20)) + new string(' ', Math.Max(1, (int)Math.Round(num24))), new GUILayoutOption[0]))
				{
					this.selectedSlider = -1;
					SettingsManager.modObjects[SettingsManager.modSelect].option = SettingsManager.modSelectOption;
					this.timeModReset = Time.timeSinceLevelLoad;
					if (SettingsManager.modObjects[SettingsManager.modSelect].enabled)
					{
						SettingsManager.modObjects[SettingsManager.modSelect].Update(0f, this.player);
					}
				}
				GUI.skin.button.normal.textColor = Color.white;
				GUI.skin.button.hover.textColor = Color.white;
				if (GUILayout.Button("=>", new GUILayoutOption[0]))
				{
					if (SettingsManager.modSelectOption == SettingsManager.modObjects[SettingsManager.modSelect].options.Count - 1)
					{
						SettingsManager.modSelectOption = 0;
					}
					else
					{
						SettingsManager.modSelectOption++;
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
		}
		if (SceneManager.GetActiveScene().name != "Mian" || (this.showMenu && SceneManager.GetActiveScene().name == "Mian" && !this.menu.activeSelf))
		{
			GUILayout.BeginArea(new Rect(5f, (float)this.currentRes.height - 70f, 225f, 100f));
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label("Choose a map:", new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("<=", new GUILayoutOption[0]))
			{
				if (SettingsManager.mapSelect == 0)
				{
					SettingsManager.mapSelect = SettingsManager.mapCount - 1;
				}
				else
				{
					SettingsManager.mapSelect--;
				}
			}
			double num25 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.mapObjects[SettingsManager.mapSelect].name)).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			GUI.skin.button.normal.textColor = ((SettingsManager.map != SettingsManager.mapSelect) ? Color.white : Color.green);
			GUI.skin.button.hover.textColor = ((SettingsManager.map != SettingsManager.mapSelect) ? Color.white : Color.green);
			if (GUILayout.Button(new string(' ', (int)Math.Floor(num25)) + SettingsManager.mapObjects[SettingsManager.mapSelect].name + new string(' ', (int)Math.Round(num25)), new GUILayoutOption[0]) && SettingsManager.map != SettingsManager.mapSelect)
			{
				this.selectedSlider = -1;
				SettingsManager.map = SettingsManager.mapSelect;
				if (SceneManager.GetActiveScene().name != "Mian")
				{
					List<string> list2 = new List<string>();
					int num26;
					for (int k = 0; k < SettingsManager.modObjects.Length; k = num26 + 1)
					{
						if (SettingsManager.modObjects[k].enabled && SettingsManager.modObjects[k].enableSplits)
						{
							list2.Add((SettingsManager.modObjects[k].options.Count > 0) ? (SettingsManager.modObjects[k].name + "[" + SettingsManager.modObjects[k].options[SettingsManager.modObjects[k].option] + "]") : SettingsManager.modObjects[k].name);
						}
						num26 = k;
					}
					list2.Sort();
					SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list2);
				}
				if (SceneManager.GetActiveScene().name == "Mian")
				{
					PlayerPrefs.DeleteKey("NumSaves");
					PlayerPrefs.DeleteKey("SaveGame0");
					PlayerPrefs.DeleteKey("SaveGame1");
					PlayerPrefs.Save();
					SceneManager.LoadScene("Mian");
				}
			}
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.hover.textColor = Color.white;
			if (GUILayout.Button("=>", new GUILayoutOption[0]))
			{
				if (SettingsManager.mapSelect == SettingsManager.mapCount - 1)
				{
					SettingsManager.mapSelect = 0;
				}
				else
				{
					SettingsManager.mapSelect++;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		if (SceneManager.GetActiveScene().name != "Mian" && SettingsManager.updateManager.updateAvailable)
		{
			SettingsManager.updateButtonRect = new Rect(245f, (float)this.currentRes.height - 45f, 225f, 21f);
			GUILayout.BeginArea(SettingsManager.updateButtonRect);
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUI.skin.button.normal.textColor = Color.green;
			GUI.skin.button.hover.textColor = Color.green;
			string text5;
			if (!SettingsManager.updateManager.downloading)
			{
				text5 = string.Format("Update to {0}", SettingsManager.updateManager.update.Build);
			}
			else if (!SettingsManager.updateManager.downloadComplete)
			{
				if (!SettingsManager.updateManager.downloadFailed || (SettingsManager.updateManager.mirrorStarted && !SettingsManager.updateManager.mirrorFailed))
				{
					text5 = string.Format("Downloading {0}%", SettingsManager.updateManager.percentageDownloaded.ToString("0.00"));
				}
				else
				{
					GUI.skin.button.normal.textColor = Color.red;
					GUI.skin.button.hover.textColor = Color.red;
					text5 = "Failed to download";
				}
			}
			else if (SettingsManager.updateManager.checkingFiles && !SettingsManager.updateManager.checkingComplete)
			{
				if (!SettingsManager.updateManager.checkingFailed)
				{
					text5 = "Checking files";
				}
				else
				{
					GUI.skin.button.normal.textColor = Color.red;
					GUI.skin.button.hover.textColor = Color.red;
					if (SettingsManager.updateManager.FilesInUseFailed)
					{
						text5 = "Failed to update #1";
					}
					else
					{
						text5 = "Failed to update #2";
					}
				}
			}
			else if (SettingsManager.updateManager.updateComplete)
			{
				text5 = "Update Complete";
			}
			else if (SettingsManager.updateManager.updateFailed && SettingsManager.updateManager.mirrorFailed)
			{
				GUI.skin.button.normal.textColor = Color.red;
				GUI.skin.button.hover.textColor = Color.red;
				text5 = "Failed to update #0";
			}
			else
			{
				text5 = "Unknown Error";
			}
			double num27 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(text5)).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			if (GUILayout.Button(new GUIContent(string.Format("{0}{1}{2}", new string(' ', (int)Math.Floor(num27)), text5, new string(' ', (int)Math.Round(num27))), "Update"), new GUILayoutOption[0]))
			{
				if (!SettingsManager.updateManager.downloading && Event.current.button == 0)
				{
					SettingsManager.updateManager.DownloadModpack(false);
				}
				else
				{
					this.showChangelog = !this.showChangelog;
				}
			}
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.hover.textColor = Color.white;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		if (this.showMenu && SceneManager.GetActiveScene().name == "Mian" && !this.menu.activeSelf)
		{
			GUILayout.BeginArea(new Rect(245f, (float)this.currentRes.height - 70f, 225f, 100f));
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label("Choose a predefined setting:", new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("<=", new GUILayoutOption[0]))
			{
				if (SettingsManager.categorySelect == 1)
				{
					SettingsManager.categorySelect = SettingsManager.categoryCount - 1;
				}
				else
				{
					SettingsManager.categorySelect--;
				}
			}
			double num28 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.categoryNames[SettingsManager.categorySelect])).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			GUI.skin.button.normal.textColor = ((SettingsManager.category != SettingsManager.categorySelect) ? Color.white : Color.green);
			GUI.skin.button.hover.textColor = ((SettingsManager.category != SettingsManager.categorySelect) ? Color.white : Color.green);
			if (GUILayout.Button(new string(' ', (int)Math.Floor(num28)) + SettingsManager.categoryNames[SettingsManager.categorySelect] + new string(' ', (int)Math.Round(num28)), new GUILayoutOption[0]))
			{
				if (SettingsManager.categorySelect == 1)
				{
					if (this.debugMenuCounter < 4)
					{
						int debugMenuCounter = this.debugMenuCounter;
						this.debugMenuCounter = debugMenuCounter + 1;
					}
					else
					{
						this.debugMenuCounter = 0;
						SettingsManager.debugGUIEnabled = !SettingsManager.debugGUIEnabled;
						this.debugNotifyTime = 2f;
						this.debugNotifyMessage = (SettingsManager.debugGUIEnabled ? "Modpack debug Enabled" : "Modpack debug Disabled");
						this.debugGUI = SettingsManager.debugGUIEnabled;
					}
					if (this.debugTimerCounter == 4)
					{
						SettingsManager.timer.debug = !SettingsManager.timer.debug;
						this.debugNotifyTime = 2f;
						this.debugNotifyMessage = (SettingsManager.timer.debug ? "Splits debug Enabled" : "Splits debug Disabled");
						this.debugTimerCounter = 0;
					}
					else if (this.debugTimerCounter % 2 == 0)
					{
						int debugTimerCounter = this.debugTimerCounter;
						this.debugTimerCounter = debugTimerCounter + 1;
					}
					else
					{
						this.debugTimerCounter = 0;
					}
				}
				else if (SettingsManager.categorySelect == 2)
				{
					if (this.debugTimerCounter % 2 == 1)
					{
						int debugTimerCounter2 = this.debugTimerCounter;
						this.debugTimerCounter = debugTimerCounter2 + 1;
					}
					else
					{
						this.debugTimerCounter = 0;
					}
					this.debugMenuCounter = 0;
				}
				else
				{
					this.debugMenuCounter = 0;
					this.debugTimerCounter = 0;
				}
				this.setCategory(SettingsManager.categorySelect);
				if (SettingsManager.sideWay)
				{
					this.revertGravity(true, true, true, true);
				}
				this.selectedSlider = -1;
			}
			GUI.skin.button.normal.textColor = Color.white;
			GUI.skin.button.hover.textColor = Color.white;
			if (GUILayout.Button("=>", new GUILayoutOption[0]))
			{
				if (SettingsManager.categorySelect == SettingsManager.categoryCount - 1)
				{
					SettingsManager.categorySelect = 1;
				}
				else
				{
					SettingsManager.categorySelect++;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect(485f, (float)this.currentRes.height - 70f, 225f, 100f));
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label("Choose where to Teleport:", new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("<=", new GUILayoutOption[0]))
			{
				SettingsManager.teleportSelect = (SettingsManager.teleportSelect - 1 + SettingsManager.teleportCount) % SettingsManager.teleportCount;
			}
			num28 = (130.0 - (double)GUI.skin.label.CalcSize(new GUIContent(SettingsManager.teleportNames[SettingsManager.teleportSelect])).x) / (2.0 * (double)GUI.skin.label.CalcSize(new GUIContent(" ")).x);
			if (GUILayout.Button(new string(' ', (int)Math.Floor(num28)) + SettingsManager.teleportNames[SettingsManager.teleportSelect] + new string(' ', (int)Math.Round(num28)), new GUILayoutOption[0]) && this.TeleportMap(SettingsManager.mapObjects[SettingsManager.map], (SettingsManager.teleportSelect + 1) % SettingsManager.teleportCount))
			{
				this.ToggleDebugMenu();
				this.timeReset = Time.timeSinceLevelLoad;
				this.selectedSlider = -1;
			}
			if (GUILayout.Button("=>", new GUILayoutOption[0]))
			{
				SettingsManager.teleportSelect = (SettingsManager.teleportSelect + 1) % SettingsManager.teleportCount;
			}
			GUILayout.Label("   ", new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		GUILayout.EndVertical();
		if (SceneManager.GetActiveScene().name == "Mian" && SettingsManager.showTimer)
		{
			SettingsManager.timer.Display(Time.timeSinceLevelLoad - this.timeReset);
		}
		else if (SettingsManager.timer.optionsSelect[3] > 1 && SceneManager.GetActiveScene().name != "Mian")
		{
			SettingsManager.timer.Display(-1f);
		}
		GUILayout.BeginArea(new Rect((float)this.currentRes.width - 1010f, (float)this.currentRes.height - 400f, 1000f, 400f));
		GUILayout.BeginVertical(new GUILayoutOption[0]);
		GUILayout.FlexibleSpace();
		GUIStyle guistyle = new GUIStyle(GUI.skin.label);
		guistyle.fontSize = (int)((float)this.currentRes.height * 0.02f);
		guistyle.padding = new RectOffset(GUI.skin.label.padding.left, GUI.skin.label.padding.right, 0, 0);
		if (SceneManager.GetActiveScene().name == "Mian")
		{
			string text6 = SettingsManager.categoryNames[SettingsManager.category] + (this.runChanged ? "*" : "");
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.Label(text6, guistyle, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			if (Time.timeSinceLevelLoad - this.timeReset < 7f || Time.timeSinceLevelLoad - this.timeModReset < 7f)
			{
				int num29;
				for (int l = 0; l < SettingsManager.modObjects.Length; l = num29 + 1)
				{
					if (SettingsManager.modObjects[l].enabled)
					{
						try
						{
							GUILayout.BeginHorizontal(new GUILayoutOption[0]);
							GUILayout.FlexibleSpace();
							if (SettingsManager.modObjects[l].web.Length > 0)
							{
								if (SettingsManager.modObjects[l].options.Count > 0)
								{
									text6 = string.Concat(new string[]
									{
										SettingsManager.modObjects[l].name,
										" [",
										SettingsManager.modObjects[l].options[SettingsManager.modObjects[l].option],
										"] v",
										SettingsManager.modObjects[l].version,
										" by ",
										SettingsManager.modObjects[l].author,
										" (",
										SettingsManager.modObjects[l].web,
										")"
									});
								}
								else
								{
									text6 = string.Concat(new string[]
									{
										SettingsManager.modObjects[l].name,
										" v",
										SettingsManager.modObjects[l].version,
										" by ",
										SettingsManager.modObjects[l].author,
										" (",
										SettingsManager.modObjects[l].web,
										")"
									});
								}
							}
							else if (SettingsManager.modObjects[l].options.Count > 0)
							{
								text6 = string.Concat(new string[]
								{
									SettingsManager.modObjects[l].name,
									" [",
									SettingsManager.modObjects[l].options[SettingsManager.modObjects[l].option],
									"] v",
									SettingsManager.modObjects[l].version,
									" by ",
									SettingsManager.modObjects[l].author
								});
							}
							else
							{
								text6 = string.Concat(new string[]
								{
									SettingsManager.modObjects[l].name,
									" v",
									SettingsManager.modObjects[l].version,
									" by ",
									SettingsManager.modObjects[l].author
								});
							}
							GUILayout.Label(text6, guistyle, new GUILayoutOption[0]);
							GUILayout.EndHorizontal();
						}
						catch (Exception ex)
						{
							Debug.Log(ex.ToString());
						}
					}
					num29 = l;
				}
			}
			if (SettingsManager.map > 0)
			{
				try
				{
					if (Time.timeSinceLevelLoad < 6f && SettingsManager.mapObjects[SettingsManager.map].web.Length > 0)
					{
						text6 = string.Concat(new string[]
						{
							SettingsManager.mapObjects[SettingsManager.map].name,
							" v",
							SettingsManager.mapObjects[SettingsManager.map].version,
							" by ",
							SettingsManager.mapObjects[SettingsManager.map].author,
							" (",
							SettingsManager.mapObjects[SettingsManager.map].web,
							")"
						});
					}
					else
					{
						text6 = string.Concat(new string[]
						{
							SettingsManager.mapObjects[SettingsManager.map].name,
							" v",
							SettingsManager.mapObjects[SettingsManager.map].version,
							" by ",
							SettingsManager.mapObjects[SettingsManager.map].author
						});
					}
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					GUILayout.FlexibleSpace();
					GUILayout.Label(text6, guistyle, new GUILayoutOption[0]);
					GUILayout.EndHorizontal();
				}
				catch (Exception ex2)
				{
					Debug.Log(ex2.ToString());
				}
			}
		}
		GUILayout.BeginHorizontal(new GUILayoutOption[0]);
		GUILayout.FlexibleSpace();
		GUILayout.Label(string.Format("Modpack build {0} {1} by anjo2", SettingsManager.build, SettingsManager.buildNames[SettingsManager.buildType]).Replace("  ", " "), guistyle, new GUILayoutOption[0]);
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		if (SceneManager.GetActiveScene().name != "Mian" && SettingsManager.updateManager.updateAvailable && (SettingsManager.updateButtonRect.Contains(Event.current.mousePosition) || this.showChangelog))
		{
			SettingsManager.updateManager.showChangelog();
		}
		if (this.debugNotifyTime > 0f)
		{
			GUI.Label(new Rect((float)(Screen.width / 2) - GUI.skin.label.CalcSize(new GUIContent(this.debugNotifyMessage)).x, (float)(Screen.height / 2), 200f, 200f), this.debugNotifyMessage);
		}
		if (SceneManager.GetActiveScene().name == "Mian" && this.player)
		{
			SettingsManager.mapObjects[SettingsManager.map].onGUI(Time.deltaTime, this.player);
			for (int m = 0; m < SettingsManager.modObjects.Length; m++)
			{
				try
				{
					if (SettingsManager.modObjects[m].enabled)
					{
						SettingsManager.modObjects[m].onGUI(Time.deltaTime, this.player);
					}
				}
				catch
				{
				}
			}
		}
	}

	// Token: 0x0600031E RID: 798 RVA: 0x0000425F File Offset: 0x0000245F
	public void reapplyGravity()
	{
		Physics2D.gravity = new Vector2((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
	}

	// Token: 0x0600031F RID: 799 RVA: 0x00004277 File Offset: 0x00002477
	public static void restartMap()
	{
		PlayerPrefs.DeleteKey("NumSaves");
		PlayerPrefs.DeleteKey("SaveGame0");
		PlayerPrefs.DeleteKey("SaveGame1");
		PlayerPrefs.Save();
		SceneManager.LoadScene("Mian");
	}

	// Token: 0x06000320 RID: 800 RVA: 0x00030BC4 File Offset: 0x0002EDC4
	public void revertGravity(bool sideway, bool apply = true, bool applyX = true, bool applyY = true)
	{
		if (sideway)
		{
			double num = SettingsManager.gravityY;
			if (applyY)
			{
				SettingsManager.gravityY = SettingsManager.gravityX * (double)Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + SettingsManager.gravityY * (double)Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f);
			}
			if (applyX)
			{
				SettingsManager.gravityX = SettingsManager.gravityX * (double)Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) - num * (double)Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f);
			}
		}
		else
		{
			double num2 = SettingsManager.gravityY;
			if (applyY)
			{
				SettingsManager.gravityY = SettingsManager.gravityX * (double)Mathf.Sin(-SettingsManager.sideWayDegrees * 0.017453292f) + SettingsManager.gravityY * (double)Mathf.Cos(-SettingsManager.sideWayDegrees * 0.017453292f);
			}
			if (applyX)
			{
				SettingsManager.gravityX = SettingsManager.gravityX * (double)Mathf.Cos(-SettingsManager.sideWayDegrees * 0.017453292f) - num2 * (double)Mathf.Sin(-SettingsManager.sideWayDegrees * 0.017453292f);
			}
		}
		if (apply)
		{
			if (SettingsManager.reachedSpace)
			{
				Physics2D.gravity = new Vector2(0f, 0f);
				return;
			}
			Physics2D.gravity = new Vector2((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
		}
	}

	// Token: 0x06000321 RID: 801 RVA: 0x00030CF4 File Offset: 0x0002EEF4
	public void SetBodyMultiplier(float val)
	{
		if (this.player && this.player.transform.localScale.x != val)
		{
			this.player.transform.localScale = new Vector3(val, val, val);
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000322 RID: 802 RVA: 0x000042A6 File Offset: 0x000024A6
	public void SetCameraDistance(float val)
	{
		if (Camera.main)
		{
			Camera.main.orthographicSize = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000323 RID: 803 RVA: 0x00030D44 File Offset: 0x0002EF44
	public void SetCursorRange(float val)
	{
		try
		{
			if (this.player != null)
			{
				this.player.GetComponent<PlayerControl>().cursorRange = val;
			}
			else
			{
				SettingsManager.cursorRange = (double)val;
			}
		}
		catch
		{
			SettingsManager.cursorRange = (double)val;
		}
		SettingsManager.category = 0;
	}

	// Token: 0x06000324 RID: 804 RVA: 0x00030D9C File Offset: 0x0002EF9C
	public void SetDefaultSettings()
	{
		SettingsManager.hammerMultiplier = 1.0;
		SettingsManager.bodyMultiplier = 1.0;
		SettingsManager.cursorRange = 3.5;
		SettingsManager.potSlidingFriction = 0.3;
		SettingsManager.potStaticFriction = 0.8;
		SettingsManager.hammerSlidingFriction = 0.7;
		SettingsManager.hammerStaticFriction = 1.0;
		SettingsManager.potSlidingBounciness = 0.0;
		SettingsManager.potStaticBounciness = 0.0;
		SettingsManager.hammerSlidingBounciness = 0.0;
		SettingsManager.hammerStaticBounciness = 0.0;
		SettingsManager.gravityX = 0.0;
		SettingsManager.gravityY = -30.0;
		SettingsManager.cameraDistance = 5.0;
	}

	// Token: 0x06000325 RID: 805 RVA: 0x000042C5 File Offset: 0x000024C5
	public void SetGravity(float valX, float valY)
	{
		if (Physics2D.gravity.x != valX || Physics2D.gravity.y != valY)
		{
			Physics2D.gravity = new Vector2(valX, valY);
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000326 RID: 806 RVA: 0x00030E70 File Offset: 0x0002F070
	public void SetHammerMultiplier(float val)
	{
		if (this.player.transform.Find("Hub") && this.player.transform.Find("Hub").transform.localScale.x != val)
		{
			this.player.transform.Find("Hub").transform.localScale = new Vector3(val, val, val);
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000327 RID: 807 RVA: 0x000042F3 File Offset: 0x000024F3
	public void SetHammerSlidingBounciness(float val)
	{
		if (global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().slidingFriction.bounciness != val)
		{
			global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().slidingFriction.bounciness = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000328 RID: 808 RVA: 0x0000431D File Offset: 0x0000251D
	public void SetHammerSlidingFriction(float val)
	{
		if (global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().slidingFriction.friction != val)
		{
			global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().slidingFriction.friction = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000329 RID: 809 RVA: 0x00004347 File Offset: 0x00002547
	public void SetHammerStaticBounciness(float val)
	{
		if (global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().staticFriction.bounciness != val)
		{
			global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().staticFriction.bounciness = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x0600032A RID: 810 RVA: 0x00004371 File Offset: 0x00002571
	public void SetHammerStaticFriction(float val)
	{
		if (global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().staticFriction.friction != val)
		{
			global::UnityEngine.Object.FindObjectOfType<HammerCollisions>().staticFriction.friction = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x0600032B RID: 811 RVA: 0x00030EF0 File Offset: 0x0002F0F0
	public void SetPotColor(float R, float G, float B, float A)
	{
		try
		{
			this.player.transform.Find("Pot/Mesh").GetComponent<MeshRenderer>().material.color = new Color(R, G, B, A);
		}
		catch
		{
		}
		PlayerPrefs.SetFloat("ModPotCustomizerRGB_R", R);
		PlayerPrefs.SetFloat("ModPotCustomizerRGB_G", G);
		PlayerPrefs.SetFloat("ModPotCustomizerRGB_B", B);
	}

	// Token: 0x0600032C RID: 812 RVA: 0x00030F64 File Offset: 0x0002F164
	public void SetPotGoldness(float goldness)
	{
		if ((double)goldness >= 0.0)
		{
			try
			{
				if (this.potMat)
				{
					this.potMat.SetFloat("_Goldness", goldness);
				}
			}
			catch
			{
			}
		}
	}

	// Token: 0x0600032D RID: 813 RVA: 0x00030FB4 File Offset: 0x0002F1B4
	public void SetPotRotationFat(bool val)
	{
		if (val && this.player.GetComponent<HingeJoint2D>().limits.max == 15f)
		{
			JointAngleLimits2D jointAngleLimits2D = default(JointAngleLimits2D);
			jointAngleLimits2D.max = 20.5f;
			jointAngleLimits2D.min = -20.5f;
			this.player.GetComponent<HingeJoint2D>().limits = jointAngleLimits2D;
			return;
		}
		if (!val && this.player.GetComponent<HingeJoint2D>().limits.max == 20.5f)
		{
			JointAngleLimits2D jointAngleLimits2D2 = default(JointAngleLimits2D);
			jointAngleLimits2D2.max = 15f;
			jointAngleLimits2D2.min = -15f;
			this.player.GetComponent<HingeJoint2D>().limits = jointAngleLimits2D2;
		}
	}

	// Token: 0x0600032E RID: 814 RVA: 0x0000439B File Offset: 0x0000259B
	public void SetPotSlidingBounciness(float val)
	{
		if (global::UnityEngine.Object.FindObjectOfType<PotSounds>().slidingFriction.bounciness != val)
		{
			global::UnityEngine.Object.FindObjectOfType<PotSounds>().slidingFriction.bounciness = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x0600032F RID: 815 RVA: 0x00031068 File Offset: 0x0002F268
	public void SetPotSlidingFriction(float val)
	{
		try
		{
			if (global::UnityEngine.Object.FindObjectOfType<PotSounds>().slidingFriction.friction != val)
			{
				global::UnityEngine.Object.FindObjectOfType<PotSounds>().slidingFriction.friction = val;
				SettingsManager.category = 0;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06000330 RID: 816 RVA: 0x000043C5 File Offset: 0x000025C5
	public void SetPotStaticBounciness(float val)
	{
		if (global::UnityEngine.Object.FindObjectOfType<PotSounds>().staticFriction.bounciness != val)
		{
			global::UnityEngine.Object.FindObjectOfType<PotSounds>().staticFriction.bounciness = val;
			SettingsManager.category = 0;
		}
	}

	// Token: 0x06000331 RID: 817 RVA: 0x000310B4 File Offset: 0x0002F2B4
	public void SetPotStaticFriction(float val)
	{
		try
		{
			if (global::UnityEngine.Object.FindObjectOfType<PotSounds>().staticFriction.friction != val)
			{
				global::UnityEngine.Object.FindObjectOfType<PotSounds>().staticFriction.friction = val;
				SettingsManager.category = 0;
			}
		}
		catch
		{
		}
	}

	// Token: 0x06000332 RID: 818 RVA: 0x000043EF File Offset: 0x000025EF
	public static float sideGravityX(float x, float y)
	{
		return x * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) - y * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f);
	}

	// Token: 0x06000333 RID: 819 RVA: 0x00004416 File Offset: 0x00002616
	public static float sideGravityY(float x, float y)
	{
		return x * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + y * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f);
	}

	// Token: 0x06000334 RID: 820 RVA: 0x0000443D File Offset: 0x0000263D
	public void Teleport(int x)
	{
		SettingsManager.modPackManager.Teleport(x);
	}

	// Token: 0x06000335 RID: 821 RVA: 0x0000444A File Offset: 0x0000264A
	public bool TeleportMap(Map map, int k)
	{
		return SettingsManager.modPackManager.TeleportMap(map, k);
	}

	// Token: 0x06000336 RID: 822 RVA: 0x00031100 File Offset: 0x0002F300
	public void TeleportPos(float x, float y)
	{
		if (this.player)
		{
			this.player.transform.localPosition = new Vector3(x, y, this.player.transform.localPosition.z);
			Rigidbody2D[] componentsInChildren = this.player.GetComponentsInChildren<Rigidbody2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].velocity = new Vector2(0f, 0f);
				componentsInChildren[i].angularVelocity = 0f;
			}
			Camera.main.SendMessage("Teleport", this.player.transform.localPosition);
		}
	}

	// Token: 0x06000337 RID: 823 RVA: 0x000311AC File Offset: 0x0002F3AC
	public static void updateMapSplits()
	{
		if (SceneManager.GetActiveScene().name != "Mian")
		{
			List<string> list = new List<string>();
			for (int i = 0; i < SettingsManager.modObjects.Length; i++)
			{
				if (SettingsManager.modObjects[i].enabled && SettingsManager.modObjects[i].enableSplits)
				{
					list.Add((SettingsManager.modObjects[i].options.Count > 0) ? (SettingsManager.modObjects[i].name + "[" + SettingsManager.modObjects[i].options[SettingsManager.modObjects[i].option] + "]") : SettingsManager.modObjects[i].name);
				}
			}
			list.Sort();
			SettingsManager.timer.LoadRun(SettingsManager.mapObjects[SettingsManager.map], SettingsManager.categoryNames[SettingsManager.category], list);
		}
	}

	// Token: 0x06000338 RID: 824 RVA: 0x00031298 File Offset: 0x0002F498
	public void setCategory(int x)
	{
		int num = -1;
		if (x == -1)
		{
			x = SettingsManager.category;
		}
		if (x > 0)
		{
			this.SetDefaultSettings();
			this.SetPotRotationFat(false);
			switch (x)
			{
			case 2:
				SettingsManager.hammerMultiplier = 0.67;
				SettingsManager.bodyMultiplier = 1.5;
				this.SetPotRotationFat(true);
				break;
			case 3:
				SettingsManager.bodyMultiplier = 0.5;
				break;
			case 4:
				SettingsManager.potSlidingFriction = 0.0;
				SettingsManager.potStaticFriction = 0.0;
				SettingsManager.hammerSlidingFriction = 0.0;
				SettingsManager.hammerStaticFriction = 0.0;
				break;
			case 5:
				SettingsManager.potSlidingFriction = 0.0;
				SettingsManager.potStaticFriction = 0.0;
				SettingsManager.hammerSlidingFriction = 0.12;
				SettingsManager.hammerStaticFriction = 0.12;
				break;
			case 6:
				SettingsManager.gravityY = -10.0;
				break;
			case 7:
				SettingsManager.gravityY = -45.0;
				break;
			case 8:
				SettingsManager.gravityY = 0.0;
				break;
			case 9:
				SettingsManager.cursorRange = 3.2;
				SettingsManager.hammerMultiplier = 0.5;
				break;
			case 10:
				SettingsManager.gravityY = -10.0;
				SettingsManager.cursorRange = 10.5;
				SettingsManager.hammerMultiplier = 4.0;
				break;
			case 11:
				SettingsManager.potSlidingBounciness = 2.0;
				SettingsManager.potStaticBounciness = 2.0;
				break;
			case 12:
				SettingsManager.cameraDistance = 15.0;
				break;
			}
		}
		this.SetBodyMultiplier((float)SettingsManager.bodyMultiplier);
		this.SetHammerMultiplier((float)SettingsManager.hammerMultiplier);
		this.SetCursorRange((float)SettingsManager.cursorRange);
		this.SetPotSlidingFriction((float)SettingsManager.potSlidingFriction);
		this.SetPotStaticFriction((float)SettingsManager.potStaticFriction);
		this.SetPotSlidingBounciness((float)SettingsManager.potSlidingBounciness);
		this.SetPotStaticBounciness((float)SettingsManager.potStaticBounciness);
		this.SetHammerSlidingFriction((float)SettingsManager.hammerSlidingFriction);
		this.SetHammerStaticFriction((float)SettingsManager.hammerStaticFriction);
		this.SetHammerSlidingBounciness((float)SettingsManager.hammerSlidingBounciness);
		this.SetHammerStaticBounciness((float)SettingsManager.hammerStaticBounciness);
		if (SettingsManager.reachedSpace)
		{
			this.SetGravity(0f, 0f);
		}
		else
		{
			this.SetGravity((float)SettingsManager.gravityX, (float)SettingsManager.gravityY);
		}
		this.SetCameraDistance((float)SettingsManager.cameraDistance);
		this.SetPotColor((float)SettingsManager.potColorR, (float)SettingsManager.potColorG, (float)SettingsManager.potColorB, 1f);
		this.SetPotGoldness((float)SettingsManager.potGoldness);
		if (x >= 0)
		{
			SettingsManager.category = x;
			return;
		}
		SettingsManager.category = num;
	}

	// Token: 0x06000339 RID: 825 RVA: 0x00031558 File Offset: 0x0002F758
	public void SetMap()
	{
		SettingsManager.mapObjects[SettingsManager.map].Load();
		try
		{
			if (SettingsManager.mapObjects[SettingsManager.map].teleportSaveStates != null && SettingsManager.mapObjects[SettingsManager.map].teleportSaveStates.Count > 0)
			{
				SettingsManager.teleportNames = new string[SettingsManager.mapObjects[SettingsManager.map].teleportSaveStates.Count];
				int num = 0;
				using (Dictionary<string, SaveState>.Enumerator enumerator = SettingsManager.mapObjects[SettingsManager.map].teleportSaveStates.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, SaveState> keyValuePair = enumerator.Current;
						string text = keyValuePair.Key.ToString();
						if (text.Length > 15)
						{
							text = text.Substring(0, 15);
						}
						if (SettingsManager.mapObjects[SettingsManager.map].teleportSaveStates.Count > 10)
						{
							SettingsManager.teleportNames[num] = "#" + (num + 1).ToString() + " " + text;
						}
						else
						{
							SettingsManager.teleportNames[num] = "#" + ((num + 1) % 10).ToString() + " " + text;
						}
						num++;
					}
					goto IL_0187;
				}
			}
			SettingsManager.teleportNames = new string[] { "#1 Devil's Chimney", "#2 Slide Skip", "#3 Furniture Skip", "#4 Orange Hell", "#5 Hat Jump", "#6 Anvil Jump", "#7 Boulders", "#8 Bucket", "#9 Ice Mountain", "#0 Tower" };
			IL_0187:;
		}
		catch
		{
			SettingsManager.teleportNames = new string[] { "#1 Devil's Chimney", "#2 Slide Skip", "#3 Furniture Skip", "#4 Orange Hell", "#5 Hat Jump", "#6 Anvil Jump", "#7 Boulders", "#8 Bucket", "#9 Ice Mountain", "#0 Tower" };
		}
		SettingsManager.teleportSelect = 0;
		SettingsManager.teleportCount = SettingsManager.teleportNames.Length;
	}

	// Token: 0x0600033A RID: 826 RVA: 0x00031794 File Offset: 0x0002F994
	public void ToggleDebugMenu()
	{
		this.objectDetails = new List<string>();
		if (this.menu.activeSelf)
		{
			this.showMenu = false;
			return;
		}
		this.runChanged = true;
		this.selectedSlider = -1;
		this.debugMenuCounter = 0;
		this.showMenu = !this.showMenu;
		if (this.showMenu)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Time.timeScale = 0f;
			if (this.player)
			{
				this.player.SendMessage("Pause");
				return;
			}
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Time.timeScale = 1f;
			if (this.player)
			{
				this.player.SendMessage("UnPause");
			}
		}
	}

	// Token: 0x17000072 RID: 114
	// (get) Token: 0x0600033C RID: 828 RVA: 0x00004458 File Offset: 0x00002658
	// (set) Token: 0x0600033D RID: 829 RVA: 0x00004460 File Offset: 0x00002660
	public bool debugGUI { get; set; }

	// Token: 0x17000073 RID: 115
	// (get) Token: 0x0600033E RID: 830 RVA: 0x00004469 File Offset: 0x00002669
	// (set) Token: 0x0600033F RID: 831 RVA: 0x00004471 File Offset: 0x00002671
	public GameObject hub { get; set; }

	// Token: 0x17000074 RID: 116
	// (get) Token: 0x06000340 RID: 832 RVA: 0x0000447A File Offset: 0x0000267A
	// (set) Token: 0x06000341 RID: 833 RVA: 0x00004482 File Offset: 0x00002682
	public bool mapDebug { get; set; }

	// Token: 0x17000075 RID: 117
	// (get) Token: 0x06000342 RID: 834 RVA: 0x0000448B File Offset: 0x0000268B
	// (set) Token: 0x06000343 RID: 835 RVA: 0x00004493 File Offset: 0x00002693
	public bool showMenu { get; set; }

	// Token: 0x17000076 RID: 118
	// (get) Token: 0x06000344 RID: 836 RVA: 0x0000449C File Offset: 0x0000269C
	// (set) Token: 0x06000345 RID: 837 RVA: 0x000044A4 File Offset: 0x000026A4
	public int debugMenuCounter { get; set; }

	// Token: 0x17000077 RID: 119
	// (get) Token: 0x06000346 RID: 838 RVA: 0x000044AD File Offset: 0x000026AD
	// (set) Token: 0x06000347 RID: 839 RVA: 0x000044B5 File Offset: 0x000026B5
	public int debugTimerCounter { get; set; }

	// Token: 0x17000078 RID: 120
	// (get) Token: 0x06000348 RID: 840 RVA: 0x000044BE File Offset: 0x000026BE
	// (set) Token: 0x06000349 RID: 841 RVA: 0x000044C6 File Offset: 0x000026C6
	public float debugNotifyTime { get; set; }

	// Token: 0x17000079 RID: 121
	// (get) Token: 0x0600034A RID: 842 RVA: 0x000044CF File Offset: 0x000026CF
	// (set) Token: 0x0600034B RID: 843 RVA: 0x000044D7 File Offset: 0x000026D7
	public string debugNotifyMessage { get; set; }

	// Token: 0x0600034C RID: 844 RVA: 0x000318BC File Offset: 0x0002FABC
	public void GetChilds(GameObject gameObject)
	{
		foreach (object obj in gameObject.transform)
		{
			Transform transform = (Transform)obj;
			string text;
			try
			{
				text = transform.gameObject.GetComponent<MeshRenderer>().ToString();
			}
			catch
			{
				text = "None";
			}
			this.clipboardText.Add(string.Concat(new object[]
			{
				transform.name,
				" [Pos: ",
				transform.gameObject.transform.localPosition.ToString(),
				transform.gameObject.transform.position.ToString(),
				"] [Layer: ",
				transform.gameObject.layer,
				"] [Static: ",
				transform.gameObject.isStatic.ToString(),
				"] [Mesh: ",
				text,
				"] [Parent: ",
				transform.gameObject.transform.parent ? transform.gameObject.transform.parent.name : "None",
				"]"
			}));
			this.GetChilds(transform.gameObject);
			if (Input.GetKey(KeyCode.Delete))
			{
				global::UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
	}

	// Token: 0x040004AA RID: 1194
	public GameObject menu;

	// Token: 0x040004AB RID: 1195
	public TMP_Dropdown resolutionDropdown;

	// Token: 0x040004AC RID: 1196
	private int currentResolutionIndex;

	// Token: 0x040004AD RID: 1197
	public TextMeshProUGUI currentQualityText;

	// Token: 0x040004AE RID: 1198
	public Button currentQualityButton;

	// Token: 0x040004AF RID: 1199
	public Button currentQualityIncrementButton;

	// Token: 0x040004B0 RID: 1200
	public Button currentQualityDecrementButton;

	// Token: 0x040004B1 RID: 1201
	public Toggle cursorToggle;

	// Token: 0x040004B2 RID: 1202
	public Toggle trackpadToggle;

	// Token: 0x040004B3 RID: 1203
	public Toggle subtitleToggle;

	// Token: 0x040004B4 RID: 1204
	public Toggle fullscreenToggle;

	// Token: 0x040004B5 RID: 1205
	public Toggle motionBlurToggle;

	// Token: 0x040004B6 RID: 1206
	public Toggle vsyncToggle;

	// Token: 0x040004B7 RID: 1207
	public Button applyResolutionButton;

	// Token: 0x040004B8 RID: 1208
	public Slider mouseSensitivitySlider;

	// Token: 0x040004B9 RID: 1209
	public Slider SFXVolumeSlider;

	// Token: 0x040004BA RID: 1210
	public Slider MusicVolumeSlider;

	// Token: 0x040004BB RID: 1211
	public Slider VOVolumeSlider;

	// Token: 0x040004BC RID: 1212
	public GameObject player;

	// Token: 0x040004BD RID: 1213
	public GameObject cursor;

	// Token: 0x040004BE RID: 1214
	public GameObject narrator;

	// Token: 0x040004BF RID: 1215
	public RestartOnContact underwaterDetector;

	// Token: 0x040004C0 RID: 1216
	public GameObject menuMobile;

	// Token: 0x040004C1 RID: 1217
	public Toggle subtitleToggleMobile;

	// Token: 0x040004C2 RID: 1218
	public Slider mouseSensitivitySliderMobile;

	// Token: 0x040004C3 RID: 1219
	public Slider SFXVolumeSliderMobile;

	// Token: 0x040004C4 RID: 1220
	public Slider MusicVolumeSliderMobile;

	// Token: 0x040004C5 RID: 1221
	public Slider VOVolumeSliderMobile;

	// Token: 0x040004C6 RID: 1222
	public AudioMixer mixer;

	// Token: 0x040004C7 RID: 1223
	private float mouseSensitivity;

	// Token: 0x040004C8 RID: 1224
	public ForceCameraRatio ratioFitter;

	// Token: 0x040004C9 RID: 1225
	public PostProcessProfile bgProfile;

	// Token: 0x040004CA RID: 1226
	public PostProcessProfile bgProfile_low;

	// Token: 0x040004CB RID: 1227
	public PostProcessProfile fgProfile;

	// Token: 0x040004CC RID: 1228
	public PostProcessProfile fgProfile_low;

	// Token: 0x040004CD RID: 1229
	public PostProcessProfile fgProfile_lowest;

	// Token: 0x040004CE RID: 1230
	public Camera fgCam;

	// Token: 0x040004CF RID: 1231
	public Camera bgCam;

	// Token: 0x040004D0 RID: 1232
	public bool canMenu;

	// Token: 0x040004D1 RID: 1233
	private Resolution currentRes;

	// Token: 0x040004D2 RID: 1234
	private Resolution newRes;

	// Token: 0x040004D3 RID: 1235
	private int currentLanguageNum;

	// Token: 0x040004D4 RID: 1236
	public Toggle[] languageToggles;

	// Token: 0x040004D5 RID: 1237
	public Material potMat;

	// Token: 0x040004D6 RID: 1238
	private string[] qualityNames = new string[] { "Worst", "Bad", "Mediocre", "Good", "Great", "Extreme" };

	// Token: 0x040004D7 RID: 1239
	private static double bodyMultiplier;

	// Token: 0x040004D8 RID: 1240
	private static double hammerMultiplier = 1.0;

	// Token: 0x040004D9 RID: 1241
	private GUIStyle _sliderBackgroundStyle;

	// Token: 0x040004DA RID: 1242
	private GUIStyle _sliderThumbStyle;

	// Token: 0x040004DB RID: 1243
	private Texture2D _whitePixel;

	// Token: 0x040004DC RID: 1244
	private Texture2D _blackPixel;

	// Token: 0x040004DD RID: 1245
	private static double gravityX;

	// Token: 0x040004DE RID: 1246
	private static double gravityY;

	// Token: 0x040004DF RID: 1247
	private static string build = "2148";

	// Token: 0x040004E0 RID: 1248
	public static int category;

	// Token: 0x040004E1 RID: 1249
	private static string[] categoryNames;

	// Token: 0x040004E2 RID: 1250
	private static Vector2[] mapPositions;

	// Token: 0x040004E3 RID: 1251
	private static int categoryCount;

	// Token: 0x040004E4 RID: 1252
	private static int categorySelect;

	// Token: 0x040004E5 RID: 1253
	private static string[] teleportNames;

	// Token: 0x040004E6 RID: 1254
	private static int teleportSelect;

	// Token: 0x040004E7 RID: 1255
	private static int teleportCount;

	// Token: 0x040004E8 RID: 1256
	private float timeReset;

	// Token: 0x040004E9 RID: 1257
	private static bool showTimer;

	// Token: 0x040004EA RID: 1258
	private float updateTimer;

	// Token: 0x040004EB RID: 1259
	private float upTimer;

	// Token: 0x040004EC RID: 1260
	private float leftTimer;

	// Token: 0x040004ED RID: 1261
	private float rightTimer;

	// Token: 0x040004EE RID: 1262
	private float downTimer;

	// Token: 0x040004EF RID: 1263
	private float teleportSpeed;

	// Token: 0x040004F0 RID: 1264
	private GUIStyle _sliderThumbStyleSelected;

	// Token: 0x040004F1 RID: 1265
	private Texture2D _greenPixel;

	// Token: 0x040004F2 RID: 1266
	private int selectedSlider;

	// Token: 0x040004F3 RID: 1267
	private static double cameraDistance;

	// Token: 0x040004F4 RID: 1268
	private static double potSlidingFriction;

	// Token: 0x040004F5 RID: 1269
	private static double potStaticFriction;

	// Token: 0x040004F6 RID: 1270
	private static double potSlidingBounciness;

	// Token: 0x040004F7 RID: 1271
	private static double potStaticBounciness;

	// Token: 0x040004F8 RID: 1272
	private static double hammerSlidingFriction;

	// Token: 0x040004F9 RID: 1273
	private static double hammerStaticFriction;

	// Token: 0x040004FA RID: 1274
	private static double hammerSlidingBounciness;

	// Token: 0x040004FB RID: 1275
	private static double hammerStaticBounciness;

	// Token: 0x040004FC RID: 1276
	private static int map;

	// Token: 0x040004FD RID: 1277
	private static int mapSelect;

	// Token: 0x040004FE RID: 1278
	private static int mapCount;

	// Token: 0x040004FF RID: 1279
	private List<string> objectDetails;

	// Token: 0x04000500 RID: 1280
	private string mapDebugText;

	// Token: 0x04000501 RID: 1281
	private List<string> clipboardText;

	// Token: 0x04000502 RID: 1282
	private bool runChanged;

	// Token: 0x04000503 RID: 1283
	private static ModPackManager modPackManager;

	// Token: 0x04000504 RID: 1284
	private static Mod[] modObjects;

	// Token: 0x04000505 RID: 1285
	private static int modSelect;

	// Token: 0x04000506 RID: 1286
	private static int modSelectOption;

	// Token: 0x04000507 RID: 1287
	private static int modCount;

	// Token: 0x04000508 RID: 1288
	private static Map[] mapObjects;

	// Token: 0x04000509 RID: 1289
	public static bool debugGUIEnabled;

	// Token: 0x0400050A RID: 1290
	public static double cursorRange;

	// Token: 0x0400050B RID: 1291
	private bool changelogLock;

	// Token: 0x0400050C RID: 1292
	private bool showChangelog;

	// Token: 0x0400050D RID: 1293
	private static Rect updateButtonRect;

	// Token: 0x0400050E RID: 1294
	public static double potColorR;

	// Token: 0x0400050F RID: 1295
	public static double potColorG;

	// Token: 0x04000510 RID: 1296
	public static double potColorB;

	// Token: 0x04000511 RID: 1297
	private float timeModReset;

	// Token: 0x04000512 RID: 1298
	public static double potGoldness;

	// Token: 0x04000513 RID: 1299
	private static int timerSelect;

	// Token: 0x04000514 RID: 1300
	private static int timerSelectOption;

	// Token: 0x04000515 RID: 1301
	private static string[] optionsNames;

	// Token: 0x04000516 RID: 1302
	private static int optionsSelect;

	// Token: 0x04000517 RID: 1303
	private static bool modpackEnabled;

	// Token: 0x04000518 RID: 1304
	private static bool teleportEnabled;

	// Token: 0x04000519 RID: 1305
	public static bool sideWay;

	// Token: 0x0400051A RID: 1306
	public static bool reachedSpace;

	// Token: 0x0400051B RID: 1307
	public static float sideWayDegrees;

	// Token: 0x0400051C RID: 1308
	public static Timer timer;

	// Token: 0x0400051D RID: 1309
	private static UpdateManager updateManager = new UpdateManager();

	// Token: 0x0400051E RID: 1310
	public static bool fastStart;

	// Token: 0x0400051F RID: 1311
	public static float fastStartTime;

	// Token: 0x04000528 RID: 1320
	private static bool modpackLoad = false;

	// Token: 0x04000529 RID: 1321
	private static string[] buildNames = new string[] { "", "Nightly", "Alpha", "Beta" };

	// Token: 0x0400052A RID: 1322
	private static int buildType = 0;
}
