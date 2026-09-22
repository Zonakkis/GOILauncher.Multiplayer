using System;
using System.Collections;
using I2.Loc;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200006C RID: 108
public class RewardLogic : MonoBehaviour
{
	// Token: 0x0600029D RID: 669 RVA: 0x00026DDC File Offset: 0x00024FDC
	private void Start()
	{
		this.mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		if (SettingsManager.fastStart)
		{
			this.mostRecentTime += 6f - SettingsManager.fastStartTime;
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.mostRecentTime);
		string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D3}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		if (Application.isEditor)
		{
			text = "02h:21m:45.233s";
		}
		this.timeText.text = ScriptLocalization.CLEAR_TIME + text;
		this.winsText.text = "";
		float num;
		if (PlayerPrefs.HasKey("LastTime"))
		{
			num = PlayerPrefs.GetFloat("LastTime");
		}
		else
		{
			num = 999999f;
		}
		float num2;
		if (PlayerPrefs.HasKey("BestTime"))
		{
			num2 = PlayerPrefs.GetFloat("BestTime");
		}
		else
		{
			num2 = num;
		}
		if (num < num2)
		{
			num2 = num;
		}
		PlayerPrefs.SetFloat("BestTime", num2);
		PlayerPrefs.Save();
		base.StartCoroutine("FadeUp");
		if ((float)PlayerPrefs.GetInt("NativeWidth") / (float)PlayerPrefs.GetInt("NativeHeight") > 1.8f)
		{
			if (Screen.currentResolution.height < 720)
			{
				Screen.SetResolution(PlayerPrefs.GetInt("NativeWidth"), PlayerPrefs.GetInt("NativeHeight"), Screen.fullScreen, PlayerPrefs.GetInt("NativeRefresh"));
			}
			else if ((float)PlayerPrefs.GetInt("NativeWidth") / (float)PlayerPrefs.GetInt("NativeHeight") > 2f)
			{
				Screen.SetResolution(1920, 1080, Screen.fullScreen, PlayerPrefs.GetInt("NativeRefresh"));
			}
		}
		else
		{
			Screen.SetResolution(PlayerPrefs.GetInt("NativeWidth"), PlayerPrefs.GetInt("NativeHeight"), Screen.fullScreen, PlayerPrefs.GetInt("NativeRefresh"));
		}
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		Debug.Log("Getting Steam Stats");
		this.GetSteamStats();
	}

	// Token: 0x0600029E RID: 670 RVA: 0x00003E72 File Offset: 0x00002072
	private IEnumerator FadeUp()
	{
		this.fader.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			Color color = this.fader.color;
			color.a = f;
			this.fader.color = color;
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600029F RID: 671 RVA: 0x00026FE0 File Offset: 0x000251E0
	public void Yes()
	{
		this.nameMenu.SetActive(true);
		this.browser.SetTime(this.mostRecentTime);
		this.gatePanel.SetActive(false);
		EventSystem.current.SetSelectedGameObject(this.nameField);
		Debug.Log("Yes");
	}

	// Token: 0x060002A0 RID: 672 RVA: 0x00003E81 File Offset: 0x00002081
	public void No()
	{
		Debug.Log("No");
		SceneManager.LoadScene("Loader");
	}

	// Token: 0x060002A1 RID: 673 RVA: 0x00003E97 File Offset: 0x00002097
	public void NoConnection()
	{
		Debug.Log("No Connection");
		SceneManager.LoadScene("Reward Loader Offline");
	}

	// Token: 0x060002A2 RID: 674 RVA: 0x00027030 File Offset: 0x00025230
	public void SendEmail()
	{
		this.mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.mostRecentTime);
		Debug.Log("opening mail client");
		string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D2}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		string text2 = this.EscapeURL("I Got Over It with a time of " + text);
		Application.OpenURL("mailto:bennett.foddy@foddy.net?subject=" + text2);
	}

	// Token: 0x060002A3 RID: 675 RVA: 0x00003EAD File Offset: 0x000020AD
	private string EscapeURL(string url)
	{
		return WWW.EscapeURL(url).Replace("+", "%20");
	}

	// Token: 0x060002A4 RID: 676 RVA: 0x000270D4 File Offset: 0x000252D4
	private void GetSteamStats()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		this.m_GameID = new CGameID(SteamUtils.GetAppID());
		this.m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(new Callback<UserStatsReceived_t>.DispatchDelegate(this.OnUserStatsReceived));
		this.m_UserStatsStored = Callback<UserStatsStored_t>.Create(new Callback<UserStatsStored_t>.DispatchDelegate(this.OnUserStatsStored));
		this.m_bRequestedStats = false;
		this.m_bStatsValid = false;
		SteamUserStats.RequestCurrentStats();
	}

	// Token: 0x060002A5 RID: 677 RVA: 0x0002713C File Offset: 0x0002533C
	private void OnUserStatsReceived(UserStatsReceived_t pCallback)
	{
		if (!SteamManager.Initialized)
		{
			int @int = PlayerPrefs.GetInt("NumWins");
			this.winsText.text = ScriptLocalization.NUMWINS + " " + @int.ToString();
			return;
		}
		if (this.m_bStatsValid)
		{
			int int2 = PlayerPrefs.GetInt("NumWins");
			this.winsText.text = ScriptLocalization.NUMWINS + " " + int2.ToString();
			return;
		}
		Debug.Log("Steam Stats Received");
		if ((ulong)this.m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("Received stats and achievements from Steam\n");
				this.m_bStatsValid = true;
				SteamUserStats.GetStat("wins", out this.steamWins);
				SteamUserStats.GetStat("best_time", out this.steamBestTime);
				if (PlayerPrefs.GetInt("StoredStatsSinceLastPlay") == 0)
				{
					this.steamWins++;
					SteamUserStats.SetStat("wins", this.steamWins);
					PlayerPrefs.SetInt("StoredStatsSinceLastPlay", 1);
					PlayerPrefs.Save();
				}
				if (this.steamWins > 1)
				{
					this.winsText.text = ScriptLocalization.NUMWINS + " " + this.steamWins.ToString();
				}
				float @float = PlayerPrefs.GetFloat("BestTime");
				SteamUserStats.SetStat("best_time", Mathf.Min(@float, this.steamBestTime));
				Debug.Log("steamWins: " + this.steamWins.ToString());
				this.browser.wins = this.steamWins;
				if (this.steamWins >= 50)
				{
					SteamUserStats.SetAchievement("ASCENDED_LOTS");
				}
				SteamUserStats.StoreStats();
				return;
			}
			Debug.Log("RequestStats - failed, " + pCallback.m_eResult.ToString());
			int int3 = PlayerPrefs.GetInt("NumWins");
			this.winsText.text = ScriptLocalization.NUMWINS + " " + int3.ToString();
		}
	}

	// Token: 0x060002A6 RID: 678 RVA: 0x00027330 File Offset: 0x00025530
	private void OnUserStatsStored(UserStatsStored_t pCallback)
	{
		if ((ulong)this.m_GameID == pCallback.m_nGameID)
		{
			if (EResult.k_EResultOK == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - success");
				return;
			}
			if (EResult.k_EResultInvalidParam == pCallback.m_eResult)
			{
				Debug.Log("StoreStats - some failed to validate");
				this.OnUserStatsReceived(new UserStatsReceived_t
				{
					m_eResult = EResult.k_EResultOK,
					m_nGameID = (ulong)this.m_GameID
				});
				return;
			}
			Debug.Log("StoreStats - failed, " + pCallback.m_eResult.ToString());
		}
	}

	// Token: 0x060002A7 RID: 679 RVA: 0x00003EC4 File Offset: 0x000020C4
	public void EnableYes(bool shouldEnable)
	{
		this.yes.interactable = shouldEnable;
	}

	// Token: 0x060002A8 RID: 680 RVA: 0x000273C4 File Offset: 0x000255C4
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) && SettingsManager.timer.optionsSelect[0] > 0)
		{
			SettingsManager.timer.optionsSelect[5] = ((SettingsManager.timer.optionsSelect[5] == 1) ? 0 : 1);
			PlayerPrefs.SetInt(SettingsManager.timer.optionsTitle[5].Trim(), SettingsManager.timer.optionsSelect[5]);
			PlayerPrefs.Save();
		}
	}

	// Token: 0x060002A9 RID: 681 RVA: 0x00003ED2 File Offset: 0x000020D2
	public void GiftShop()
	{
		if (this.giftshopOpen)
		{
			return;
		}
		Debug.Log("opening website");
		Application.OpenURL("http://goo.gl/PWcJPP");
		this.giftshopOpen = true;
	}

	// Token: 0x060002AB RID: 683 RVA: 0x00003EF8 File Offset: 0x000020F8
	public void OnGUI()
	{
		if (SettingsManager.timer.optionsSelect[3] == 1 || SettingsManager.timer.optionsSelect[3] == 3)
		{
			SettingsManager.timer.Display(-1f);
		}
	}

	// Token: 0x04000446 RID: 1094
	private float mostRecentTime;

	// Token: 0x04000447 RID: 1095
	public BrowserControl browser;

	// Token: 0x04000448 RID: 1096
	public GameObject browserMenu;

	// Token: 0x04000449 RID: 1097
	public GameObject nameMenu;

	// Token: 0x0400044A RID: 1098
	public GameObject gatePanel;

	// Token: 0x0400044B RID: 1099
	public GameObject nameField;

	// Token: 0x0400044C RID: 1100
	public Toggle disclaimer;

	// Token: 0x0400044D RID: 1101
	public Button yes;

	// Token: 0x0400044E RID: 1102
	public TextMeshProUGUI timeText;

	// Token: 0x0400044F RID: 1103
	public Image fader;

	// Token: 0x04000450 RID: 1104
	public TextMeshProUGUI winsText;

	// Token: 0x04000451 RID: 1105
	private CGameID m_GameID;

	// Token: 0x04000452 RID: 1106
	private int steamWins;

	// Token: 0x04000453 RID: 1107
	private float steamBestTime;

	// Token: 0x04000454 RID: 1108
	private bool m_bRequestedStats;

	// Token: 0x04000455 RID: 1109
	private bool m_bStatsValid;

	// Token: 0x04000456 RID: 1110
	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	// Token: 0x04000457 RID: 1111
	protected Callback<UserStatsStored_t> m_UserStatsStored;

	// Token: 0x04000458 RID: 1112
	private bool giftshopOpen;
}
