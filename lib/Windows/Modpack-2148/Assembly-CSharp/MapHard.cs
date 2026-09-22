using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

// Token: 0x020002ED RID: 749
public class MapHard : Map
{
	// Token: 0x06001979 RID: 6521 RVA: 0x0007A118 File Offset: 0x00078318
	public bool Load()
	{
		try
		{
			this.rock3 = GameObject.Find("Intresto_rock062_502vert_1000face_scale_5pct_tex_SydBas13");
			global::UnityEngine.Object.Destroy(GameObject.Find("Background"));
			global::UnityEngine.Object.Destroy(GameObject.Find("LeatherCouch (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("roofsection"));
			global::UnityEngine.Object.Destroy(GameObject.Find("branch"));
			global::UnityEngine.Object.Destroy(GameObject.Find("oar1.5m"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Sans+titre"));
			global::UnityEngine.Object.Destroy(GameObject.Find("RustyVan"));
			global::UnityEngine.Object.Destroy(GameObject.Find("tunnelsegmentOBJ"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Girder"));
			global::UnityEngine.Object.Destroy(GameObject.Find("SmallExteriorLights"));
			global::UnityEngine.Object.Destroy(GameObject.Find("SmallExteriorLights (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("GiftSpawn"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Bracket"));
			global::UnityEngine.Object.Destroy(GameObject.Find("building"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Underside"));
			global::UnityEngine.Object.Destroy(GameObject.Find("hot+tub+blue"));
			global::UnityEngine.Object.Destroy(GameObject.Find("bidon_plastic_grop"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Dynamic+Wall+with+Drywall"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Bench"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Racks"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Wall"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Stairs"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Floor Lamp"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Mantel"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Orangetable"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Rigged Hand"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Snowman"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Rock (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Rock (3)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Rock (9)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Hedge"));
			global::UnityEngine.Object.Destroy(GameObject.Find("ballok"));
			global::UnityEngine.Object.Destroy(GameObject.Find("WoodenJetty"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Snake"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Cart"));
			global::UnityEngine.Object.Destroy(GameObject.Find("BLOCK (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Roof"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Anvil"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Rock_3"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Rock_3 (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("SpeakingStones_01"));
			global::UnityEngine.Object.Destroy(GameObject.Find("BLOCK"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Cupboard"));
			global::UnityEngine.Object.Destroy(GameObject.Find("WhiteCouch"));
			global::UnityEngine.Object.Destroy(GameObject.Find("White Couch"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Bed2"));
			global::UnityEngine.Object.Destroy(GameObject.Find("StairCabinet"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Toilet"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Trees"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Barrel (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("cinderblock"));
			global::UnityEngine.Object.Destroy(GameObject.Find("old+wooden+table"));
			global::UnityEngine.Object.Destroy(GameObject.Find("ConcreteDamage01b (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("ConcreteDamage01b (3)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("ConcreteDamage01b (4)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Concrete Platform"));
			global::UnityEngine.Object.Destroy(GameObject.Find("deck fg"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock037_252vert_499face_scale_1m"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock035_252vert_500face_scale_1m_tex_roadlichen"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock055_252vert_500face_scale_1m_tex_sandstone1"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock055_252vert_500face_scale_1m_tex_sandstone1 (1)"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock024_252vert_500face_scale_1m_tex_sandstone8"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock057_52vert_100face_scale_1pct_tex_SydSst13"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock002_252vert_500face_scale_1m_tex_basalt6"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock022_252vert_500face_scale_1m_raw_tex_road05"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock078_502vert_1000face_scale_1pct_tex_SydBas15"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock046_252vert_500face_scale_1m"));
			global::UnityEngine.Object.Destroy(GameObject.Find("Intresto_rock046_252vert_500face_scale_1m_tex"));
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
		this.lastDelete = 0f;
		this.rock1Deleted = false;
		this.rock2Deleted = false;
		this.rock3Deleted = false;
		return true;
	}

	// Token: 0x0600197A RID: 6522 RVA: 0x0007A5A0 File Offset: 0x000787A0
	public MapHard()
	{
		this.name = "Hard Map";
		this.version = "1.0";
		this.author = "fm2";
		this.web = "";
		this.splits = new Dictionary<string, Rect>();
		this.splits.Add("Tutorial", new Rect(-12f, 7f, 4f, 8f));
		this.splits.Add("Devil's Chimney", new Rect(33f, 81f, 4f, 8f));
		this.splits.Add("Construction", new Rect(21f, 145f, 7f, 11f));
		this.splits.Add("Furniture", new Rect(0f, 162f, 3f, 6f));
		this.splits.Add("Orange Hell", new Rect(18f, 216f, 6f, 14f));
		this.splits.Add("Anvil Jump", new Rect(73f, 249f, 4f, 10f));
		this.splits.Add("Bucket Jump", new Rect(18f, 281f, 4f, 14f));
		this.splits.Add("Ice Mountain", new Rect(42f, 317f, 4f, 14f));
		this.splits.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splits.Add("Space", new Rect(0f, 470f, 200f, 2f));
		this.splitsSideways = new Dictionary<string, Rect>();
		this.splitsSideways.Add("Tutorial", new Rect(-12f, 7f, 4f, 8f));
		this.splitsSideways.Add("Devil's Chimney", new Rect(33f, 81f, 4f, 8f));
		this.splitsSideways.Add("Construction", new Rect(21f, 145f, 7f, 11f));
		this.splitsSideways.Add("Furniture", new Rect(0f, 162f, 3f, 6f));
		this.splitsSideways.Add("Orange Hell", new Rect(18f, 216f, 6f, 14f));
		this.splitsSideways.Add("Anvil Jump", new Rect(73f, 249f, 4f, 10f));
		this.splitsSideways.Add("Bucket Jump", new Rect(18f, 281f, 4f, 14f));
		this.splitsSideways.Add("Ice Mountain", new Rect(42f, 317f, 4f, 14f));
		this.splitsSideways.Add("Tower", new Rect(0f, 359f, 200f, 2f));
		this.splitsSideways.Add("Space", new Rect(0f, 470f, 200f, 2f));
	}

	// Token: 0x0600197B RID: 6523 RVA: 0x0000750B File Offset: 0x0000570B
	public bool UnLoad()
	{
		return true;
	}

	// Token: 0x170006DA RID: 1754
	// (get) Token: 0x0600197C RID: 6524 RVA: 0x000130CA File Offset: 0x000112CA
	public string name { get; }

	// Token: 0x170006DB RID: 1755
	// (get) Token: 0x0600197D RID: 6525 RVA: 0x000130D2 File Offset: 0x000112D2
	public string version { get; }

	// Token: 0x170006DC RID: 1756
	// (get) Token: 0x0600197E RID: 6526 RVA: 0x000130DA File Offset: 0x000112DA
	public string author { get; }

	// Token: 0x170006DD RID: 1757
	// (get) Token: 0x0600197F RID: 6527 RVA: 0x000130E2 File Offset: 0x000112E2
	public string web { get; }

	// Token: 0x06001980 RID: 6528 RVA: 0x0007A92C File Offset: 0x00078B2C
	public string Update(float deltaTime, GameObject player)
	{
		string text = "whiskas";
		if (!this.rock3Deleted || !this.rock2Deleted || !this.rock1Deleted)
		{
			try
			{
				this.lastDelete += deltaTime;
				if (this.lastDelete > 0.1f)
				{
					this.lastDelete = 0f;
					this.rock1 = GameObject.Find("Intresto_rock076_500vert_1000face_scale_1pct_tex_SydBas10");
					this.rock2 = GameObject.Find("Intresto_ArmiRock01_Photosynth_mesh");
					if (this.rock1 != null)
					{
						global::UnityEngine.Object.Destroy(this.rock1);
					}
					else
					{
						this.rock1Deleted = true;
					}
					if (this.rock2 != null)
					{
						global::UnityEngine.Object.Destroy(this.rock2);
					}
					else
					{
						this.rock2Deleted = true;
					}
					if (this.rock1 == null && this.rock2 == null)
					{
						this.rock3.SetActive(false);
						this.rock3Deleted = true;
					}
				}
			}
			catch (Exception ex)
			{
				return ex.ToString();
			}
			return text;
		}
		return text;
	}

	// Token: 0x170006DE RID: 1758
	// (get) Token: 0x06001981 RID: 6529 RVA: 0x000130EA File Offset: 0x000112EA
	public Dictionary<string, Rect> splits { get; }

	// Token: 0x170006DF RID: 1759
	// (get) Token: 0x06001982 RID: 6530 RVA: 0x000130F2 File Offset: 0x000112F2
	public Dictionary<string, Rect> splitsSideways { get; }

	// Token: 0x170006E0 RID: 1760
	// (get) Token: 0x06001983 RID: 6531 RVA: 0x000130FA File Offset: 0x000112FA
	// (set) Token: 0x06001984 RID: 6532 RVA: 0x00013102 File Offset: 0x00011302
	public Dictionary<string, SaveState> teleportSaveStates { get; set; }

	// Token: 0x06001985 RID: 6533 RVA: 0x0007AA38 File Offset: 0x00078C38
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

	// Token: 0x06001986 RID: 6534 RVA: 0x000126FA File Offset: 0x000108FA
	public string FixedUpdate(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x06001987 RID: 6535 RVA: 0x000126FA File Offset: 0x000108FA
	public string onGUI(float deltaTime, GameObject player)
	{
		return "";
	}

	// Token: 0x04001141 RID: 4417
	private bool rock1Deleted;

	// Token: 0x04001142 RID: 4418
	private bool rock2Deleted;

	// Token: 0x04001143 RID: 4419
	private float lastDelete;

	// Token: 0x04001144 RID: 4420
	private GameObject rock1;

	// Token: 0x04001145 RID: 4421
	private GameObject rock2;

	// Token: 0x04001146 RID: 4422
	private GameObject rock3;

	// Token: 0x04001147 RID: 4423
	private bool rock3Deleted;
}
