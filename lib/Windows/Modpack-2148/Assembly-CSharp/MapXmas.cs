using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x0200031A RID: 794
public class MapXmas : Map
{
	// Token: 0x06001B68 RID: 7016 RVA: 0x000842B8 File Offset: 0x000824B8
	public bool Load()
	{
		try
		{
			this.present = global::UnityEngine.Object.Instantiate<GameObject>(global::UnityEngine.Object.FindObjectOfType<GiftSpawner>().gift, new Vector3(-370.3f, 3f, -12f), Quaternion.identity);
			this.present.SetActive(false);
			this.presentTimer = 1f;
			this.presentSpeed = 1f;
			global::UnityEngine.Object.Instantiate<GameObject>(GameObject.Find("Rock"), new Vector3(-37.3f, 3f, -12f), Quaternion.identity);
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock016_252vert_500face_scale_1pct"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Snake"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Cube"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Quad"));
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

	// Token: 0x06001B69 RID: 7017 RVA: 0x000843B0 File Offset: 0x000825B0
	public MapXmas()
	{
		this.name = "Christmas Map";
		this.version = "1.3";
		this.author = "anjo2";
		this.web = "twitch.tv/anjo2";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Tutorial", new Rect(-12f, 7f, 4f, 14f));
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
		this.splitsSideways.Add("Orange Hell", new Rect(18f, 216f, 6f, 14f));
		this.splitsSideways.Add("Anvil Jump", new Rect(73f, 249f, 4f, 30f));
		this.splitsSideways.Add("Bucket Jump", new Rect(18f, 281f, 200f, 14f));
		this.splitsSideways.Add("Ice Mountain", new Rect(42f, 317f, 200f, 14f));
		this.splitsSideways.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splitsSideways.Add("Space", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x06001B6A RID: 7018 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x170007A3 RID: 1955
	// (get) Token: 0x06001B6B RID: 7019 RVA: 0x00013E88 File Offset: 0x00012088
	public string name { get; }

	// Token: 0x170007A4 RID: 1956
	// (get) Token: 0x06001B6C RID: 7020 RVA: 0x00013E90 File Offset: 0x00012090
	public string version { get; }

	// Token: 0x170007A5 RID: 1957
	// (get) Token: 0x06001B6D RID: 7021 RVA: 0x00013E98 File Offset: 0x00012098
	public string author { get; }

	// Token: 0x170007A6 RID: 1958
	// (get) Token: 0x06001B6E RID: 7022 RVA: 0x00013EA0 File Offset: 0x000120A0
	public string web { get; }

	// Token: 0x06001B6F RID: 7023 RVA: 0x0008473C File Offset: 0x0008293C
	public string Update(float deltaTime, GameObject player)
	{
		try
		{
			if (player)
			{
				this.presentTimer -= Time.deltaTime;
				this.presentSpeed = Math.Max(player.transform.position.y, this.presentSpeed);
				if (this.presentTimer < 0f && player.transform.position.x > -28f)
				{
					if (player.transform.position.y < 360f)
					{
						Vector2 vector;
						if (SettingsManager.sideWay)
						{
							vector = new Vector2(player.transform.position.x + global::UnityEngine.Random.RandomRange(-4f, 5f) * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) - 8.8f * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f), player.transform.position.y + global::UnityEngine.Random.RandomRange(-4f, 5f) * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + 8.8f * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f)) + player.GetComponent<Rigidbody2D>().velocity / 5f;
						}
						else
						{
							vector = new Vector2(global::UnityEngine.Random.RandomRange(player.transform.position.x - 2f, player.transform.position.x + 3f), player.transform.position.y + 8.5f) + player.GetComponent<Rigidbody2D>().velocity / 5f;
						}
						if (Physics2D.OverlapCircleAll(vector, 0.1f).Length == 0)
						{
							GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(this.present, new Vector3(vector.x, vector.y, 0f), Quaternion.identity);
							gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(player.GetComponent<Rigidbody2D>().velocity.x / 1.5f, 0f, 0f);
							gameObject.SetActive(true);
							global::UnityEngine.Object.Destroy(gameObject, 2f);
							this.presentTimer = 0.67f / (float)Math.Log10((double)Math.Min(this.presentSpeed, 10f));
						}
					}
					else
					{
						Vector2 vector2 = new Vector2(global::UnityEngine.Random.RandomRange(player.transform.position.x - 2f, player.transform.position.x + 2f), global::UnityEngine.Random.RandomRange(player.transform.position.y + 1f, player.transform.position.y + 3f)) + player.GetComponent<Rigidbody2D>().velocity / 5f;
						if (Physics2D.OverlapCircleAll(vector2, 0.1f).Length == 0)
						{
							GameObject gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(this.present, new Vector3(vector2.x, vector2.y, 0f), Quaternion.identity);
							gameObject2.GetComponent<Rigidbody2D>().velocity = new Vector3(player.GetComponent<Rigidbody2D>().velocity.x / 1.5f, 0f, 0f);
							gameObject2.SetActive(true);
							gameObject2.GetComponent<Rigidbody2D>().freezeRotation = true;
							global::UnityEngine.Object.Destroy(gameObject2, 3f);
							this.presentTimer = 0.8f / (float)Math.Log10((double)Math.Min(this.presentSpeed, 10f));
						}
					}
				}
			}
		}
		catch (Exception)
		{
			return "";
		}
		return "";
	}

	// Token: 0x170007A7 RID: 1959
	// (get) Token: 0x06001B70 RID: 7024 RVA: 0x00013EA8 File Offset: 0x000120A8
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170007A8 RID: 1960
	// (get) Token: 0x06001B71 RID: 7025 RVA: 0x00013EB0 File Offset: 0x000120B0
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170007A9 RID: 1961
	// (get) Token: 0x06001B72 RID: 7026 RVA: 0x00013EB8 File Offset: 0x000120B8
	// (set) Token: 0x06001B73 RID: 7027 RVA: 0x00013EC0 File Offset: 0x000120C0
	public SaveState[] teleportSaves { get; set; }

	// Token: 0x170007AA RID: 1962
	// (get) Token: 0x06001B74 RID: 7028 RVA: 0x00013EC9 File Offset: 0x000120C9
	// (set) Token: 0x06001B75 RID: 7029 RVA: 0x00013ED1 File Offset: 0x000120D1
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001B76 RID: 7030 RVA: 0x00084AF0 File Offset: 0x00082CF0
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

	// Token: 0x06001B77 RID: 7031 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B78 RID: 7032 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x0400127A RID: 4730
	private GameObject present;

	// Token: 0x0400127B RID: 4731
	private float presentSpeed;

	// Token: 0x0400127C RID: 4732
	private float presentTimer;
}
