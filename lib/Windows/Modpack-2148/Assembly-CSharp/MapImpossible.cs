using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x02000318 RID: 792
public class MapImpossible : Map
{
	// Token: 0x06001B4A RID: 6986 RVA: 0x000829D8 File Offset: 0x00080BD8
	public bool Load()
	{
		try
		{
			string[] array = new string[]
			{
				"Rigged Hand", "Rock (8)", "Rock (1)AAA", "SnowyRock", "rock_02 (2)", "Radio+mast+tower+set", "asteroid5 (1)", "Intresto_rock029_502vert_1000face_scale_1pct_tex_SydSst11crot", "Intresto_rock058_52vert_100face_scale_1pct_tex_SydSst13 (1)", "Intresto_rock028_502vert_1000face_scale_1pct_tex_SydSst11brot (2)",
				"Intresto_rock055_252vert_500face_scale_1m_tex_sandstone1 (3)", "Intresto_rock055_252vert_500face_scale_1m_tex_sandstone1 (2)", "Intresto_rock027_502vert_1000face_scale_1pct_tex_SydSst11arot"
			};
			string[] array2 = new string[]
			{
				"Intresto_rock058_52vert_100face_scale_1pct_tex_SydSst13 (5)", "Intresto_rock058_52vert_100face_scale_1pct_tex_SydSst13", "fallen_dead_tree (1)", "RustyVan", "Barrel", "Intresto_rock033_252vert_500face_scale_1m_raw_tex_basalt7", "Intresto_rock030_252vert_500face_scale_1m_tex_ArmGra06c", "Intresto_rock040_252vert_500face_scale_1m", "Intresto_rock058_52vert_100face_scale_1pct_tex_SydSst13 (5)", "Intresto_rock022_252vert_500face_scale_1m_raw_tex_road05",
				"Intresto_rock028_502vert_1000face_scale_1pct_tex_SydSst11brot (1)", "Intresto_rock028_502vert_1000face_scale_1pct_tex_SydSst11brot", "Intresto_rock058_52vert_100face_scale_1pct_tex_SydSst13 (6)"
			};
			Dictionary<string, Rect> dictionary = new Dictionary<string, Rect>();
			dictionary.Add("Tutorial", new Rect(-20f, 0f, 5f, 5f));
			dictionary.Add("Chimney", new Rect(22f, 65f, 10f, 10f));
			dictionary.Add("Slide", new Rect(-40f, 100f, 50f, 56f));
			dictionary.Add("Top Furn", new Rect(11f, 166f, 20f, 25f));
			dictionary.Add("middle Furn", new Rect(12f, 149f, 10f, 10f));
			dictionary.Add("bottom Furn", new Rect(10f, 143f, 6f, 6f));
			dictionary.Add("hat jump", new Rect(60f, 225f, 6f, 10f));
			foreach (object obj in GameObject.Find("Mountain").transform)
			{
				Transform transform = (Transform)obj;
				if (Array.IndexOf<string>(array2, transform.gameObject.name) >= 0)
				{
					global::UnityEngine.Object.Destroy(transform.gameObject);
				}
				else
				{
					foreach (Rect rect in dictionary.Values)
					{
						if (rect.Contains(transform.gameObject.transform.position))
						{
							global::UnityEngine.Object.Destroy(transform.gameObject);
							break;
						}
					}
				}
			}
			foreach (object obj2 in GameObject.Find("Mountain").transform)
			{
				Transform transform2 = (Transform)obj2;
				bool flag = true;
				foreach (Rect rect2 in dictionary.Values)
				{
					if (rect2.Contains(transform2.gameObject.transform.position))
					{
						flag = false;
						break;
					}
				}
				if (flag && Array.IndexOf<string>(array, transform2.gameObject.name) < 0 && Array.IndexOf<string>(array2, transform2.gameObject.name) < 0)
				{
					MapTools.createArrow(transform2.gameObject.transform.position, global::UnityEngine.Random.Range(0f, 360f), true);
					global::UnityEngine.Object.Destroy(transform2.gameObject);
				}
			}
			MapTools.createArrow(new Vector3(-22.69f, 4.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(-17.69f, 3.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(-7.69f, 5.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(3.69f, 11.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(9.69f, 8.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(13.69f, 18.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(13.69f, 35.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(23.69f, 66.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(28.69f, 69.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(23.69f, 72.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(28.69f, 75.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(26.69f, 78.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(30.69f, 82.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(33.69f, 99.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(36.69f, 81.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(35.69f, 94.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(35.69f, 104.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(19.69f, 121.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(26.69f, 125.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(15.69f, 130.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(13.69f, 152.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(19.69f, 213.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(29.69f, 216.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(39.69f, 220.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(49.69f, 227.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(60.69f, 233.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(70.69f, 246.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(77.69f, 245.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(77.69f, 251.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(75.69f, 269.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(71.69f, 269.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(67.69f, 269.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(63.69f, 269.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(47.69f, 262.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(51.69f, 253.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(35.69f, 258.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(31.69f, 258.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(57.69f, 326.69f, 0f), global::UnityEngine.Random.RandomRange(0f, 360f), true);
			MapTools.createArrow(new Vector3(16.69f, 279.69f, 0f), 180f, true);
			MapTools.createArrow(new Vector3(-45.69f, 0.69f, 0f), 180f, false);
			for (int i = 0; i < 69; i++)
			{
				MapTools.createArrow(new Vector3(-55.69f - (float)(i * 3), -3.69f, 0f), 180f, true);
			}
			GameObject gameObject = new GameObject();
			gameObject.transform.position = new Vector3(-265.69f, -0.69f, 20f);
			gameObject.AddComponent<TextMesh>().text = "WRONG WAY";
			gameObject.GetComponent<TextMesh>().fontSize = 102;
			gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			GameObject gameObject2 = new GameObject();
			gameObject2.transform.position = new Vector3(-262.69f, -2.69f, 20f);
			gameObject2.AddComponent<TextMesh>().text = "LUL";
			gameObject2.GetComponent<TextMesh>().fontSize = 120;
			gameObject2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
			MapTools.newThumb(new Vector3(-261.69f, -6.69f, 0f));
			try
			{
				this.updateConfig(this.name);
			}
			catch
			{
			}
		}
		catch
		{
		}
		return true;
	}

	// Token: 0x06001B4B RID: 6987 RVA: 0x000835C8 File Offset: 0x000817C8
	public MapImpossible()
	{
		this.name = "Impossible";
		this.version = "%$#&%$#&$#";
		this.author = "4 /\\/ _] [] 2";
		this.web = "twitch.tv/anjo2";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("GO AWAY!!!", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("GO AWAY!!!", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x06001B4C RID: 6988 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x17000795 RID: 1941
	// (get) Token: 0x06001B4D RID: 6989 RVA: 0x00013E06 File Offset: 0x00012006
	public string name { get; }

	// Token: 0x17000796 RID: 1942
	// (get) Token: 0x06001B4E RID: 6990 RVA: 0x00013E0E File Offset: 0x0001200E
	public string version { get; }

	// Token: 0x17000797 RID: 1943
	// (get) Token: 0x06001B4F RID: 6991 RVA: 0x00013E16 File Offset: 0x00012016
	public string author { get; }

	// Token: 0x17000798 RID: 1944
	// (get) Token: 0x06001B50 RID: 6992 RVA: 0x00013E1E File Offset: 0x0001201E
	public string web { get; }

	// Token: 0x06001B51 RID: 6993 RVA: 0x000126FA File Offset: 0x000108FA
	public string Update(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x17000799 RID: 1945
	// (get) Token: 0x06001B52 RID: 6994 RVA: 0x00013E26 File Offset: 0x00012026
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x1700079A RID: 1946
	// (get) Token: 0x06001B53 RID: 6995 RVA: 0x00013E2E File Offset: 0x0001202E
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x1700079B RID: 1947
	// (get) Token: 0x06001B54 RID: 6996 RVA: 0x00013E36 File Offset: 0x00012036
	// (set) Token: 0x06001B55 RID: 6997 RVA: 0x00013E3E File Offset: 0x0001203E
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001B56 RID: 6998 RVA: 0x00083670 File Offset: 0x00081870
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

	// Token: 0x06001B57 RID: 6999 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001B58 RID: 7000 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}
}
