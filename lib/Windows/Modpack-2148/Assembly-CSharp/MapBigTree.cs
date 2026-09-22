using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x020002EA RID: 746
public class MapBigTree : Map
{
	// Token: 0x0600194C RID: 6476 RVA: 0x00078A4C File Offset: 0x00076C4C
	public bool Load()
	{
		try
		{
			GameObject.Find("Deadtree").transform.localScale = new Vector3(4f, 4f, 4f);
			this.updateConfig(this.name);
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x0600194D RID: 6477 RVA: 0x00078AA4 File Offset: 0x00076CA4
	public MapBigTree()
	{
		this.name = "Big Tree Map";
		this.version = "1.0";
		this.author = "fm2";
		this.web = "";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Start", new Rect(-10f, -4f, 12.5f, 2f));
		this.splits.Add("End", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("Start", new Rect(-10f, -4f, 12.5f, 2f));
		this.splitsSideways.Add("End", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x0600194E RID: 6478 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x170006C5 RID: 1733
	// (get) Token: 0x0600194F RID: 6479 RVA: 0x00013007 File Offset: 0x00011207
	public string name { get; }

	// Token: 0x170006C6 RID: 1734
	// (get) Token: 0x06001950 RID: 6480 RVA: 0x0001300F File Offset: 0x0001120F
	public string version { get; }

	// Token: 0x170006C7 RID: 1735
	// (get) Token: 0x06001951 RID: 6481 RVA: 0x00013017 File Offset: 0x00011217
	public string author { get; }

	// Token: 0x170006C8 RID: 1736
	// (get) Token: 0x06001952 RID: 6482 RVA: 0x0001301F File Offset: 0x0001121F
	public string web { get; }

	// Token: 0x06001953 RID: 6483 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x170006C9 RID: 1737
	// (get) Token: 0x06001954 RID: 6484 RVA: 0x00013027 File Offset: 0x00011227
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170006CA RID: 1738
	// (get) Token: 0x06001955 RID: 6485 RVA: 0x0001302F File Offset: 0x0001122F
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170006CB RID: 1739
	// (get) Token: 0x06001956 RID: 6486 RVA: 0x00013037 File Offset: 0x00011237
	// (set) Token: 0x06001957 RID: 6487 RVA: 0x0001303F File Offset: 0x0001123F
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001958 RID: 6488 RVA: 0x00078BA0 File Offset: 0x00076DA0
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

	// Token: 0x06001959 RID: 6489 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x0600195A RID: 6490 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
