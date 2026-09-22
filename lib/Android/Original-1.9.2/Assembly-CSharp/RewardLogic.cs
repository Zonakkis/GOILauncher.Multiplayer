using System;
using System.Collections;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x0200013F RID: 319
public class RewardLogic : MonoBehaviour
{
	// Token: 0x06000836 RID: 2102 RVA: 0x00047AFC File Offset: 0x00045EFC
	private void Start()
	{
		this.mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.mostRecentTime);
		string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D3}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		this.timeText.text = ScriptLocalization.CLEAR_TIME + text;
		base.StartCoroutine("FadeUp");
		Screen.SetResolution(PlayerPrefs.GetInt("NativeWidth"), PlayerPrefs.GetInt("NativeHeight"), Screen.fullScreen, PlayerPrefs.GetInt("NativeRefresh"));
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		this.saveBestTime();
	}

	// Token: 0x06000837 RID: 2103 RVA: 0x00047BD0 File Offset: 0x00045FD0
	public void saveBestTime()
	{
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
	}

	// Token: 0x06000838 RID: 2104 RVA: 0x00047C3C File Offset: 0x0004603C
	public void No()
	{
		SceneManager.LoadScene("Loader");
	}

	// Token: 0x06000839 RID: 2105 RVA: 0x00047C48 File Offset: 0x00046048
	public void NoMobile()
	{
		base.StartCoroutine("FadeToQuit");
	}

	// Token: 0x0600083A RID: 2106 RVA: 0x00047C58 File Offset: 0x00046058
	public void SendEmail()
	{
		this.mostRecentTime = PlayerPrefs.GetFloat("LastTime");
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.mostRecentTime);
		string text = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D2}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
		Application.OpenURL(string.Format("mailto:{0}?subject={1}", "bennett.foddy@foddy.net", "I%20Got%20Over%20It%20with%20a%20time%20of%20" + WWW.EscapeURL(text)));
	}

	// Token: 0x0600083B RID: 2107 RVA: 0x00047CF3 File Offset: 0x000460F3
	public void ReviewThisGame()
	{
		Application.OpenURL("https://play.google.com/store/apps/details?id=com.noodlecake." + Application.identifier);
	}

	// Token: 0x0600083C RID: 2108 RVA: 0x00047D0C File Offset: 0x0004610C
	private IEnumerator FadeUp()
	{
		this.fader.color = new Color(0f, 0f, 0f, 1f);
		for (float f = 1f; f >= -0.001f; f -= 0.05f)
		{
			Color c = this.fader.color;
			c.a = f;
			this.fader.color = c;
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600083D RID: 2109 RVA: 0x00047D28 File Offset: 0x00046128
	private IEnumerator FadeToQuit()
	{
		this.fader.color = new Color(0f, 0f, 0f, 0f);
		for (float f = 0f; f <= 1.01f; f += 0.05f)
		{
			Color c = this.fader.color;
			c.a = f;
			this.fader.color = c;
			yield return null;
		}
		SceneManager.LoadScene("Mian");
		yield break;
	}

	// Token: 0x0600083E RID: 2110 RVA: 0x00047D43 File Offset: 0x00046143
	public void EnableYes(bool shouldEnable)
	{
		this.yes.interactable = shouldEnable;
	}

	// Token: 0x0600083F RID: 2111 RVA: 0x00047D51 File Offset: 0x00046151
	private void Update()
	{
	}

	// Token: 0x06000840 RID: 2112 RVA: 0x00047D53 File Offset: 0x00046153
	public void GiftShop()
	{
		if (this.giftshopOpen)
		{
			return;
		}
		Application.OpenURL("http://goo.gl/PWcJPP");
		this.giftshopOpen = true;
	}

	// Token: 0x0400082F RID: 2095
	public GameObject browserMenu;

	// Token: 0x04000830 RID: 2096
	public GameObject nameMenu;

	// Token: 0x04000831 RID: 2097
	public GameObject gatePanel;

	// Token: 0x04000832 RID: 2098
	public Toggle disclaimer;

	// Token: 0x04000833 RID: 2099
	public Button yes;

	// Token: 0x04000834 RID: 2100
	public TextMeshProUGUI timeText;

	// Token: 0x04000835 RID: 2101
	public Image fader;

	// Token: 0x04000836 RID: 2102
	private float mostRecentTime;

	// Token: 0x04000837 RID: 2103
	private bool giftshopOpen;
}
