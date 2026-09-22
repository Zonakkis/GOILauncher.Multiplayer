using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using ZenFulcrum.EmbeddedBrowser;

// Token: 0x0200003C RID: 60
public class BrowserControl : MonoBehaviour
{
	// Token: 0x0600016E RID: 366 RVA: 0x0001E774 File Offset: 0x0001C974
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

	// Token: 0x0600016F RID: 367 RVA: 0x0001E7F0 File Offset: 0x0001C9F0
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

	// Token: 0x06000170 RID: 368 RVA: 0x0001E910 File Offset: 0x0001CB10
	public void SetTime(float _time)
	{
		this.time = _time;
		TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.time);
		this.timeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}.{3:D2}s", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds });
	}

	// Token: 0x06000171 RID: 369 RVA: 0x0001E984 File Offset: 0x0001CB84
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

	// Token: 0x06000172 RID: 370 RVA: 0x000034FD File Offset: 0x000016FD
	private void KickMe(string kickname)
	{
		if (this.name.Equals(kickname, StringComparison.OrdinalIgnoreCase))
		{
			this.logic.SendMessage("No");
		}
	}

	// Token: 0x06000173 RID: 371 RVA: 0x0001E9E4 File Offset: 0x0001CBE4
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

	// Token: 0x06000174 RID: 372 RVA: 0x0000351E File Offset: 0x0000171E
	public void SendFromButton()
	{
		this.Send(this.inputfield.text);
		this.inputfield.text = "";
		this.inputfield.ActivateInputField();
	}

	// Token: 0x0400025F RID: 607
	private Browser browser;

	// Token: 0x04000260 RID: 608
	public TMP_InputField inputfield;

	// Token: 0x04000261 RID: 609
	public GameObject nameScreen;

	// Token: 0x04000262 RID: 610
	private new string name;

	// Token: 0x04000263 RID: 611
	private float time;

	// Token: 0x04000264 RID: 612
	private string timeString;

	// Token: 0x04000265 RID: 613
	private bool zoomSet;

	// Token: 0x04000266 RID: 614
	public RewardLogic logic;

	// Token: 0x04000267 RID: 615
	public int wins;
}
