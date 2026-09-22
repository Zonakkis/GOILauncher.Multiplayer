using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x02000319 RID: 793
public class MapThumbs : Map
{
	// Token: 0x06001B59 RID: 7001 RVA: 0x000839D8 File Offset: 0x00081BD8
	public bool Load()
	{
		try
		{
			string[] array = new string[] { "Rigged Hand", "Intresto_rock055_252vert_500face_scale_1m_tex_sandstone1", "Rock (8)", "Rock (1)AAA", "SnowyRock", "rock_02 (2)", "Radio+mast+tower+set", "asteroid5 (1)" };
			GameObject.Find("Rigged Hand");
			foreach (object obj in GameObject.Find("Mountain").transform)
			{
				Transform transform = (Transform)obj;
				if (Array.IndexOf<string>(array, transform.gameObject.name) < 0)
				{
					MapTools.newThumb(transform.gameObject.transform.position + new Vector3(2f, -3f, 0f));
					MapTools.newThumb(transform.gameObject.transform.position + new Vector3(-2f, -4f, 0f));
					global::UnityEngine.Object.Destroy(transform.gameObject);
				}
			}
			MapTools.newThumb(new Vector3(28f, 78f, 0f));
			MapTools.newThumb(new Vector3(32f, 259f, 0f));
			MapTools.newThumb(new Vector3(19.1f, 275f, 0f));
			MapTools.newThumb(new Vector3(75.5f, 246f, 0f));
		}
		catch
		{
		}
		try
		{
			this.updateConfig(this.name);
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x06001B5A RID: 7002 RVA: 0x00083BC4 File Offset: 0x00081DC4
	public MapThumbs()
	{
		this.name = "Thumbs Up Map";
		this.version = "1.0";
		this.author = "anjo2";
		this.web = "twitch.tv/anjo2";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Devil's Chimney", new Rect(26.5f, 81f, 4f, 8f));
		this.splits.Add("Construction", new Rect(26f, 101f, 7f, 7f));
		this.splits.Add("Furniture", new Rect(-4f, 169f, 16f, 5f));
		this.splits.Add("Orange Hell", new Rect(21f, 215f, 6f, 12f));
		this.splits.Add("Hat Jump", new Rect(48f, 232f, 8f, 12f));
		this.splits.Add("Anvil Jump", new Rect(60f, 252f, 5f, 12f));
		this.splits.Add("Bucket Jump", new Rect(18f, 279f, 5f, 12f));
		this.splits.Add("Ice Mountain", new Rect(42f, 317f, 4f, 12f));
		this.splits.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splits.Add("Space", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("Devil's Chimney", new Rect(26.5f, 81f, 4f, 8f));
		this.splitsSideways.Add("Construction", new Rect(26f, 101f, 7f, 7f));
		this.splitsSideways.Add("Furniture", new Rect(-4f, 169f, 16f, 5f));
		this.splitsSideways.Add("Orange Hell", new Rect(21f, 215f, 6f, 12f));
		this.splitsSideways.Add("Hat Jump", new Rect(48f, 232f, 8f, 12f));
		this.splitsSideways.Add("Anvil Jump", new Rect(60f, 252f, 5f, 12f));
		this.splitsSideways.Add("Bucket Jump", new Rect(18f, 279f, 5f, 12f));
		this.splitsSideways.Add("Ice Mountain", new Rect(42f, 317f, 4f, 12f));
		this.splitsSideways.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splitsSideways.Add("Space", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x06001B5B RID: 7003 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x1700079C RID: 1948
	// (get) Token: 0x06001B5C RID: 7004 RVA: 0x00013E47 File Offset: 0x00012047
	public string name { get; }

	// Token: 0x1700079D RID: 1949
	// (get) Token: 0x06001B5D RID: 7005 RVA: 0x00013E4F File Offset: 0x0001204F
	public string version { get; }

	// Token: 0x1700079E RID: 1950
	// (get) Token: 0x06001B5E RID: 7006 RVA: 0x00013E57 File Offset: 0x00012057
	public string author { get; }

	// Token: 0x1700079F RID: 1951
	// (get) Token: 0x06001B5F RID: 7007 RVA: 0x00013E5F File Offset: 0x0001205F
	public string web { get; }

	// Token: 0x06001B60 RID: 7008 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x170007A0 RID: 1952
	// (get) Token: 0x06001B61 RID: 7009 RVA: 0x00013E67 File Offset: 0x00012067
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170007A1 RID: 1953
	// (get) Token: 0x06001B62 RID: 7010 RVA: 0x00013E6F File Offset: 0x0001206F
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170007A2 RID: 1954
	// (get) Token: 0x06001B63 RID: 7011 RVA: 0x00013E77 File Offset: 0x00012077
	// (set) Token: 0x06001B64 RID: 7012 RVA: 0x00013E7F File Offset: 0x0001207F
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001B65 RID: 7013 RVA: 0x00083F50 File Offset: 0x00082150
	private void updateConfig(string level)
	{
		string text = string.Format("modpack\\maps\\{0}.mpc", level);
		if (File.Exists(text))
		{
			try
			{
				MapTools.MapConfig mapConfig = JsonConvert.DeserializeObject<MapTools.MapConfig>(File.ReadAllText(text));
				try
				{
					if (mapConfig.SaveStates != null && mapConfig.SaveStates.Count > 0)
					{
						this.teleportSaveStates = new Dictionary<string, SaveState>();
						foreach (KeyValuePair<string, MapTools.TeleportSaveState> keyValuePair in mapConfig.SaveStates)
						{
							this.teleportSaveStates.Add(keyValuePair.Key, new SaveState
							{
								hingePos = keyValuePair.Value.hingePos,
								hingeVel = 0f,
								sliderPos = keyValuePair.Value.sliderPos,
								sliderVel = 0f,
								camPos = new Vector3(keyValuePair.Value.camPos[0], keyValuePair.Value.camPos[1], keyValuePair.Value.camPos[2]),
								playerPos = new Vector3(keyValuePair.Value.playerPos[0], keyValuePair.Value.playerPos[1], keyValuePair.Value.playerPos[2]),
								playerRot = Quaternion.Euler(keyValuePair.Value.playerRot[0], keyValuePair.Value.playerRot[1], keyValuePair.Value.playerRot[2]),
								rbLinearVelocities = new Vector2[]
								{
									Vector2.zero,
									Vector2.zero,
									Vector2.zero,
									Vector2.zero,
									Vector2.zero,
									Vector2.zero
								},
								rbAngularVelocities = new float[6],
								rbPositions = new Vector2[]
								{
									new Vector2(keyValuePair.Value.rbPositions[0][0], keyValuePair.Value.rbPositions[0][1]),
									new Vector2(keyValuePair.Value.rbPositions[1][0], keyValuePair.Value.rbPositions[1][1]),
									new Vector2(keyValuePair.Value.rbPositions[2][0], keyValuePair.Value.rbPositions[2][1]),
									new Vector2(keyValuePair.Value.rbPositions[3][0], keyValuePair.Value.rbPositions[3][1]),
									new Vector2(keyValuePair.Value.rbPositions[4][0], keyValuePair.Value.rbPositions[4][1]),
									new Vector2(keyValuePair.Value.rbPositions[5][0], keyValuePair.Value.rbPositions[5][1])
								},
								rbAngles = keyValuePair.Value.rbAngles
							});
						}
					}
				}
				catch (Exception)
				{
				}
			}
			catch
			{
			}
		}
	}

	// Token: 0x06001B66 RID: 7014 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B67 RID: 7015 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
