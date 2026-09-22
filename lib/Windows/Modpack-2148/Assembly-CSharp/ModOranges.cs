using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000312 RID: 786
public class ModOranges : Mod
{
	// Token: 0x06001AE9 RID: 6889 RVA: 0x00081AA8 File Offset: 0x0007FCA8
	public bool Load()
	{
		this.presentTimer = this.optionTimes[this.option];
		this.enabled = true;
		this.orangeObj = GameObject.Find("Props").transform.Find("Orange").gameObject;
		return true;
	}

	// Token: 0x06001AEA RID: 6890 RVA: 0x00081AF4 File Offset: 0x0007FCF4
	public ModOranges()
	{
		this.name = "Orange Rain";
		this.version = "1.1";
		this.author = "anjo2";
		this.web = "twitch.tv/anjo2";
		this.optionsTitle = "Oranges per second";
		this.options = new List<string>
		{
			"5", "10", "15", "20", "25", "30", "35", "40", "45", "50",
			"60", "70", "80", "90", "100", "110", "120"
		};
		this.optionTimes = new float[]
		{
			0.2f, 0.1f, 0.0667f, 0.05f, 0.04f, 0.0333f, 0.286f, 0.025f, 0.022f, 0.02f,
			0.0167f, 0.0142f, 0.0125f, 0.0111f, 0.01f, 0.0091f, 0.00833f
		};
		this.option = 7;
		this.enableSplits = true;
		this.enabled = false;
	}

	// Token: 0x06001AEB RID: 6891 RVA: 0x00013B76 File Offset: 0x00011D76
	public bool UnLoad()
	{
		this.enabled = false;
		return true;
	}

	// Token: 0x17000768 RID: 1896
	// (get) Token: 0x06001AEC RID: 6892 RVA: 0x00013B80 File Offset: 0x00011D80
	public string name { get; }

	// Token: 0x17000769 RID: 1897
	// (get) Token: 0x06001AED RID: 6893 RVA: 0x00013B88 File Offset: 0x00011D88
	public string version { get; }

	// Token: 0x1700076A RID: 1898
	// (get) Token: 0x06001AEE RID: 6894 RVA: 0x00013B90 File Offset: 0x00011D90
	public string author { get; }

	// Token: 0x1700076B RID: 1899
	// (get) Token: 0x06001AEF RID: 6895 RVA: 0x00013B98 File Offset: 0x00011D98
	public string web { get; }

	// Token: 0x06001AF0 RID: 6896 RVA: 0x00081C34 File Offset: 0x0007FE34
	public string Update(float deltaTime, GameObject player)
	{
		try
		{
			if (player && this.enabled)
			{
				this.presentTimer -= Time.deltaTime;
				if (this.presentTimer < 0f)
				{
					Vector2 vector;
					if (SettingsManager.sideWay)
					{
						vector = new Vector2(player.transform.position.x + global::UnityEngine.Random.RandomRange(-6f, 7.5f) * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) - 8.8f * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f), player.transform.position.y + global::UnityEngine.Random.RandomRange(-6f, 7.5f) * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + 8.8f * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f)) + player.GetComponent<Rigidbody2D>().velocity / 5f;
					}
					else
					{
						vector = new Vector2(global::UnityEngine.Random.RandomRange(player.transform.position.x - 6f, player.transform.position.x + 7.5f), player.transform.position.y + 8.8f) + player.GetComponent<Rigidbody2D>().velocity / 5f;
					}
					if (Physics2D.OverlapCircleAll(vector, 0.1f).Length == 0)
					{
						GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(this.orangeObj, new Vector3(vector.x, vector.y, 0f), Quaternion.identity);
						gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(player.GetComponent<Rigidbody2D>().velocity.x / 1.5f, 0f, 0f);
						gameObject.SetActive(true);
						global::UnityEngine.Object.Destroy(gameObject, 2.5f);
						this.presentTimer = this.optionTimes[this.option];
					}
				}
			}
		}
		catch (Exception ex)
		{
			return ex.ToString();
		}
		return "";
	}

	// Token: 0x1700076C RID: 1900
	// (get) Token: 0x06001AF1 RID: 6897 RVA: 0x00013BA0 File Offset: 0x00011DA0
	// (set) Token: 0x06001AF2 RID: 6898 RVA: 0x00013BA8 File Offset: 0x00011DA8
	public int option { get; set; }

	// Token: 0x1700076D RID: 1901
	// (get) Token: 0x06001AF3 RID: 6899 RVA: 0x00013BB1 File Offset: 0x00011DB1
	public List<string> options { get; }

	// Token: 0x1700076E RID: 1902
	// (get) Token: 0x06001AF4 RID: 6900 RVA: 0x00013BB9 File Offset: 0x00011DB9
	// (set) Token: 0x06001AF5 RID: 6901 RVA: 0x00013BC1 File Offset: 0x00011DC1
	public bool enabled { get; set; }

	// Token: 0x1700076F RID: 1903
	// (get) Token: 0x06001AF6 RID: 6902 RVA: 0x00013BCA File Offset: 0x00011DCA
	// (set) Token: 0x06001AF7 RID: 6903 RVA: 0x00013BD2 File Offset: 0x00011DD2
	public string optionsTitle { get; set; }

	// Token: 0x17000770 RID: 1904
	// (get) Token: 0x06001AF8 RID: 6904 RVA: 0x00013BDB File Offset: 0x00011DDB
	// (set) Token: 0x06001AF9 RID: 6905 RVA: 0x00013BE3 File Offset: 0x00011DE3
	public bool enableSplits { get; set; }

	// Token: 0x06001AFA RID: 6906 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001AFB RID: 6907 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001238 RID: 4664
	private float presentTimer;

	// Token: 0x04001239 RID: 4665
	private float[] optionTimes;

	// Token: 0x0400123A RID: 4666
	private GameObject orangeObj;
}
