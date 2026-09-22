using System;
using System.Collections;
using I2.Loc;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x02000046 RID: 70
public class RewardLogic : MonoBehaviour
{
	// Token: 0x06000219 RID: 537 RVA: 0x00014E94 File Offset: 0x00013094
	private void Start()
	{
		this.mostRecentTime = PlayerPrefs.GetFloat("LastTime");
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

	// Token: 0x0600021A RID: 538 RVA: 0x0001503B File Offset: 0x0001323B
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

	// Token: 0x0600021B RID: 539 RVA: 0x0001504C File Offset: 0x0001324C
	public void Yes()
	{
		this.nameMenu.SetActive(true);
		this.browser.SetTime(this.mostRecentTime);
		this.gatePanel.SetActive(false);
		EventSystem.current.SetSelectedGameObject(this.nameField);
		Debug.Log("Yes");
	}

	// Token: 0x0600021C RID: 540 RVA: 0x0001509C File Offset: 0x0001329C
	public void No()
	{
		Debug.Log("No");
		SceneManager.LoadScene("Loader");
	}

	// Token: 0x0600021D RID: 541 RVA: 0x000150B2 File Offset: 0x000132B2
	public void NoConnection()
	{
		Debug.Log("No Connection");
		SceneManager.LoadScene("Reward Loader Offline");
	}

	// Token: 0x0600021E RID: 542 RVA: 0x000150C8 File Offset: 0x000132C8
	public void SendEmail()
	{
		this.mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.mostRecentTime);
		Debug.Log("opening mail client");
		string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D2}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		string text2 = this.EscapeURL("I Got Over It with a time of " + text);
		Application.OpenURL("mailto:bennett.foddy@foddy.net?subject=" + text2);
	}

	// Token: 0x0600021F RID: 543 RVA: 0x0001516B File Offset: 0x0001336B
	private string EscapeURL(string url)
	{
		return WWW.EscapeURL(url).Replace("+", "%20");
	}

	// Token: 0x06000220 RID: 544 RVA: 0x00015184 File Offset: 0x00013384
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

	// Token: 0x06000221 RID: 545 RVA: 0x000151EC File Offset: 0x000133EC
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

	// Token: 0x06000222 RID: 546 RVA: 0x000153E0 File Offset: 0x000135E0
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

	// Token: 0x06000223 RID: 547 RVA: 0x00015472 File Offset: 0x00013672
	public void EnableYes(bool shouldEnable)
	{
		this.yes.interactable = shouldEnable;
	}

	// Token: 0x06000224 RID: 548 RVA: 0x00015480 File Offset: 0x00013680
	private void Update()
	{
	}

	// Token: 0x06000225 RID: 549 RVA: 0x00015482 File Offset: 0x00013682
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

	// Token: 0x040003A3 RID: 931
	private float mostRecentTime;

	// Token: 0x040003A4 RID: 932
	public BrowserControl browser;

	// Token: 0x040003A5 RID: 933
	public GameObject browserMenu;

	// Token: 0x040003A6 RID: 934
	public GameObject nameMenu;

	// Token: 0x040003A7 RID: 935
	public GameObject gatePanel;

	// Token: 0x040003A8 RID: 936
	public GameObject nameField;

	// Token: 0x040003A9 RID: 937
	public Toggle disclaimer;

	// Token: 0x040003AA RID: 938
	public Button yes;

	// Token: 0x040003AB RID: 939
	public TextMeshProUGUI timeText;

	// Token: 0x040003AC RID: 940
	public Image fader;

	// Token: 0x040003AD RID: 941
	public TextMeshProUGUI winsText;

	// Token: 0x040003AE RID: 942
	private CGameID m_GameID;

	// Token: 0x040003AF RID: 943
	private int steamWins;

	// Token: 0x040003B0 RID: 944
	private float steamBestTime;

	// Token: 0x040003B1 RID: 945
	private bool m_bRequestedStats;

	// Token: 0x040003B2 RID: 946
	private bool m_bStatsValid;

	// Token: 0x040003B3 RID: 947
	protected Callback<UserStatsReceived_t> m_UserStatsReceived;

	// Token: 0x040003B4 RID: 948
	protected Callback<UserStatsStored_t> m_UserStatsStored;

	// Token: 0x040003B5 RID: 949
	private bool giftshopOpen;
}
