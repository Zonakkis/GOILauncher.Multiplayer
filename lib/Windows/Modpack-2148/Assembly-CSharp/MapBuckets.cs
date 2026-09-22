using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x020002EB RID: 747
public class MapBuckets : Map
{
	// Token: 0x0600195B RID: 6491 RVA: 0x00078F08 File Offset: 0x00077108
	public bool Load()
	{
		try
		{
			GameObject.Find("Deadtree").transform.localScale = new Vector3(7f, 7f, 7f);
			GameObject.Find("Deadtree").transform.localPosition = new Vector3(-58f, -54f, 0f);
			GameObject gameObject = GameObject.Find("Rope4");
			GameObject gameObject2 = GameObject.Find("Deadtree");
			GameObject gameObject3 = GameObject.Find("Coffee+Cup+Takeaway");
			GameObject gameObject4 = GameObject.Find("roofsection");
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-28f, 11f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-28f, 21f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-30f, 33f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-30f, 43f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-27f, 53f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-27f, 63f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-24f, 70f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-24f, 80f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-16f, 88f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-16f, 98f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(-34.5f, 30f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(-5f, 90f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(3f, 90f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(11f, 90f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(-12f, 91f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(-7f, 91f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(-2f, 91f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject3, new Vector3(2f, 91f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(-8f, 92f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject4, new Vector3(11f, 160f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-5f, 147f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-5f, 157f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(-5f, 167f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject2, new Vector3(28.5f, 260f, 0f), Quaternion.identity).transform.localScale = new Vector3(2f, 2f, 2f);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(72f, 280f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(72f, 290f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(76f, 305f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(78f, 318f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(76f, 328f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(76f, 338f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(68f, 348f, 0f), Quaternion.identity);
			global::UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector3(68f, 359f, 0f), Quaternion.identity);
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

	// Token: 0x0600195C RID: 6492 RVA: 0x000793F8 File Offset: 0x000775F8
	public MapBuckets()
	{
		this.name = "Buckets Map";
		this.version = "1.0";
		this.author = "Codyumm";
		this.web = "youtube.com/codyumm";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Tree", new Rect(-15f, 90f, 4f, 11f));
		this.splits.Add("Slide", new Rect(-4.5f, 163f, 8f, 8f));
		this.splits.Add("Orange Hell", new Rect(18f, 216f, 6f, 14f));
		this.splits.Add("Anvil Jump", new Rect(73f, 249f, 4f, 10f));
		this.splits.Add("Easter Egg", new Rect(65f, 329f, 4f, 6f));
		this.splits.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splits.Add("Space", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("Tree", new Rect(-15f, 90f, 4f, 11f));
		this.splitsSideways.Add("Slide", new Rect(-4.5f, 163f, 8f, 8f));
		this.splitsSideways.Add("Orange Hell", new Rect(18f, 216f, 6f, 14f));
		this.splitsSideways.Add("Anvil Jump", new Rect(73f, 249f, 4f, 10f));
		this.splitsSideways.Add("Easter Egg", new Rect(65f, 329f, 4f, 6f));
		this.splitsSideways.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splitsSideways.Add("Space", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x0600195D RID: 6493 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x170006CC RID: 1740
	// (get) Token: 0x0600195E RID: 6494 RVA: 0x00013048 File Offset: 0x00011248
	public string name { get; }

	// Token: 0x170006CD RID: 1741
	// (get) Token: 0x0600195F RID: 6495 RVA: 0x00013050 File Offset: 0x00011250
	public string version { get; }

	// Token: 0x170006CE RID: 1742
	// (get) Token: 0x06001960 RID: 6496 RVA: 0x00013058 File Offset: 0x00011258
	public string author { get; }

	// Token: 0x170006CF RID: 1743
	// (get) Token: 0x06001961 RID: 6497 RVA: 0x00013060 File Offset: 0x00011260
	public string web { get; }

	// Token: 0x06001962 RID: 6498 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x170006D0 RID: 1744
	// (get) Token: 0x06001963 RID: 6499 RVA: 0x00013068 File Offset: 0x00011268
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170006D1 RID: 1745
	// (get) Token: 0x06001964 RID: 6500 RVA: 0x00013070 File Offset: 0x00011270
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170006D2 RID: 1746
	// (get) Token: 0x06001965 RID: 6501 RVA: 0x00013078 File Offset: 0x00011278
	// (set) Token: 0x06001966 RID: 6502 RVA: 0x00013080 File Offset: 0x00011280
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001967 RID: 6503 RVA: 0x0007968C File Offset: 0x0007788C
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

	// Token: 0x06001968 RID: 6504 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001969 RID: 6505 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
