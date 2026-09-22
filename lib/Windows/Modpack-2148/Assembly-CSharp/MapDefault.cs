using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x020002EC RID: 748
public class MapDefault : Map
{
	// Token: 0x0600196A RID: 6506 RVA: 0x000799F4 File Offset: 0x00077BF4
	public bool Load()
	{
		try
		{
			this.updateConfig(this.name);
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x0600196B RID: 6507 RVA: 0x00079A24 File Offset: 0x00077C24
	public MapDefault()
	{
		this.name = "Default";
		this.version = "1.0";
		this.author = "Bennett Foddy";
		this.web = "www.foddy.net";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Tutorial", new Rect(-12f, 7f, 4f, 15f));
		this.splits.Add("Devil's Chimney", new Rect(24f, 82f, 4f, 14f));
		this.splits.Add("Slide Skip", new Rect(12f, 123f, 6f, 5f));
		this.splits.Add("Furniture", new Rect(4f, 162f, 2f, 5f));
		this.splits.Add("Orange Hell", new Rect(18f, 216f, 6f, 14f));
		this.splits.Add("Anvil Jump", new Rect(73f, 249f, 4f, 10f));
		this.splits.Add("Bucket Jump", new Rect(18f, 281f, 4f, 14f));
		this.splits.Add("Ice Mountain", new Rect(42f, 317f, 4f, 14f));
		this.splits.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splits.Add("Space", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("Tutorial", new Rect(-12f, 7f, 4f, 25f));
		this.splitsSideways.Add("Devil's Chimney", new Rect(24f, 82f, 4f, 17f));
		this.splitsSideways.Add("Slide Skip", new Rect(12f, 123f, 6f, 5f));
		this.splitsSideways.Add("Furniture", new Rect(4f, 162f, 2f, 5f));
		this.splitsSideways.Add("Orange Hell", new Rect(18f, 216f, 6f, 16f));
		this.splitsSideways.Add("Anvil Jump", new Rect(73f, 249f, 4f, 30f));
		this.splitsSideways.Add("Bucket Jump", new Rect(18f, 281f, 200f, 8f));
		this.splitsSideways.Add("Ice Mountain", new Rect(42f, 317f, 200f, 8f));
		this.splitsSideways.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splitsSideways.Add("Space", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x0600196C RID: 6508 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x170006D3 RID: 1747
	// (get) Token: 0x0600196D RID: 6509 RVA: 0x00013089 File Offset: 0x00011289
	public string name { get; }

	// Token: 0x170006D4 RID: 1748
	// (get) Token: 0x0600196E RID: 6510 RVA: 0x00013091 File Offset: 0x00011291
	public string version { get; }

	// Token: 0x170006D5 RID: 1749
	// (get) Token: 0x0600196F RID: 6511 RVA: 0x00013099 File Offset: 0x00011299
	public string author { get; }

	// Token: 0x170006D6 RID: 1750
	// (get) Token: 0x06001970 RID: 6512 RVA: 0x000130A1 File Offset: 0x000112A1
	public string web { get; }

	// Token: 0x06001971 RID: 6513 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x170006D7 RID: 1751
	// (get) Token: 0x06001972 RID: 6514 RVA: 0x000130A9 File Offset: 0x000112A9
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170006D8 RID: 1752
	// (get) Token: 0x06001973 RID: 6515 RVA: 0x000130B1 File Offset: 0x000112B1
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170006D9 RID: 1753
	// (get) Token: 0x06001974 RID: 6516 RVA: 0x000130B9 File Offset: 0x000112B9
	// (set) Token: 0x06001975 RID: 6517 RVA: 0x000130C1 File Offset: 0x000112C1
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001976 RID: 6518 RVA: 0x00079DB0 File Offset: 0x00077FB0
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

	// Token: 0x06001977 RID: 6519 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001978 RID: 6520 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
