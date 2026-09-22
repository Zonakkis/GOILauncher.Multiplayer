using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using ZenFulcrum.EmbeddedBrowser;

// Token: 0x02000027 RID: 39
public class BrowserControl : MonoBehaviour
{
	// Token: 0x06000146 RID: 326 RVA: 0x0000D374 File Offset: 0x0000B574
	private void Start()
	{
		this.browser = base.GetComponent<Browser>();
		this.browser.Zoom = 4f;
		this.zoomSet = false;
		this.browser.CookieManager.GetCookies();
		this.name = "name not set";
		this.time = 1000000f;
		this.timeString = "not loaded";
		this.browser.onFetchError += delegate(JSONNode error)
		{
			Debug.Log("fetch error: " + error.AsJSON);
			this.logic.SendMessage("NoConnection");
		};
	}

	// Token: 0x06000147 RID: 327 RVA: 0x0000D3F0 File Offset: 0x0000B5F0
	public void DismissNameField(string _name)
	{
		if (_name.Length == 0)
		{
			return;
		}
		this.name = _name;
		if (this.name == "Bennett" || this.name == "bennett" || this.name == "bennet" || this.name == "Bennet")
		{
			this.name += " (no relation)";
		}
		char[] array = new char[] { ' ', '\t' };
		this.nameScreen.SetActive(false);
		bool flag = this.wins <= 1;
		this.browser.EvalJS(string.Concat(new string[]
		{
			"socket.emit('set name', '",
			this.name,
			"', '",
			this.timeString,
			"', '",
			flag.ToString(),
			"');"
		}), "scripted command");
		this.inputfield.Select();
		EventSystem.current.SetSelectedGameObject(this.inputfield.gameObject, null);
	}

	// Token: 0x06000148 RID: 328 RVA: 0x0000D510 File Offset: 0x0000B710
	public void SetTime(float _time)
	{
		this.time = _time;
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.time);
		this.timeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D2}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
	}

	// Token: 0x06000149 RID: 329 RVA: 0x0000D584 File Offset: 0x0000B784
	private void Update()
	{
		if (!this.zoomSet && this.browser.IsReady)
		{
			this.browser.Zoom = 2f;
			this.zoomSet = true;
			Debug.Log("setting browser zoom");
			this.browser.RegisterFunction("kick", delegate(JSONNode args)
			{
				this.KickMe(args[0]);
			});
		}
	}

	// Token: 0x0600014A RID: 330 RVA: 0x0000D5E3 File Offset: 0x0000B7E3
	private void KickMe(string kickname)
	{
		if (this.name.Equals(kickname, StringComparison.OrdinalIgnoreCase))
		{
			this.logic.SendMessage("No");
		}
	}

	// Token: 0x0600014B RID: 331 RVA: 0x0000D604 File Offset: 0x0000B804
	public void Send(string message)
	{
		if (message.Length == 0)
		{
			return;
		}
		message = message.Replace("\\", "/");
		message = message.Replace("'", "\\'");
		message = message.Replace("\"", "\\\"");
		this.browser.EvalJS("socket.emit('chat message', '" + message + "');", "scripted command");
		this.inputfield.text = "";
		this.inputfield.ActivateInputField();
	}

	// Token: 0x0600014C RID: 332 RVA: 0x0000D68C File Offset: 0x0000B88C
	public void SendFromButton()
	{
		this.Send(this.inputfield.text);
		this.inputfield.text = "";
		this.inputfield.ActivateInputField();
	}

	// Token: 0x04000209 RID: 521
	private Browser browser;

	// Token: 0x0400020A RID: 522
	public TMP_InputField inputfield;

	// Token: 0x0400020B RID: 523
	public GameObject nameScreen;

	// Token: 0x0400020C RID: 524
	private new string name;

	// Token: 0x0400020D RID: 525
	private float time;

	// Token: 0x0400020E RID: 526
	private string timeString;

	// Token: 0x0400020F RID: 527
	private bool zoomSet;

	// Token: 0x04000210 RID: 528
	public RewardLogic logic;

	// Token: 0x04000211 RID: 529
	public int wins;
}
