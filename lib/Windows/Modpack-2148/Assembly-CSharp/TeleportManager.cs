using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

// Token: 0x020002F7 RID: 759
public class TeleportManager
{
	// Token: 0x060019DA RID: 6618 RVA: 0x0007BD18 File Offset: 0x00079F18
	public TeleportManager()
	{
		this.LoadSaviour();
		this.teleportSaves = new SaveState[]
		{
			new SaveState
			{
				hingePos = -13229.387f,
				hingeVel = 0f,
				sliderPos = -0.08596506f,
				sliderVel = 0f,
				camPos = new Vector3(48.52427f, 320.93936f, -20f),
				playerPos = new Vector3(48.05247f, 318.99615f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, 0.13296013f),
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
					new Vector2(48.05247f, 318.99615f),
					new Vector2(47.99223f, 319.34564f),
					new Vector2(47.99223f, 319.34564f),
					new Vector2(47.941746f, 319.27606f),
					new Vector2(48.632145f, 320.2231f),
					new Vector2(49.26836f, 321.09583f)
				},
				rbAngles = new float[] { 15.28136f, 0f, -13265.962f, -13085.963f, -13086.093f, -13086.093f }
			},
			new SaveState
			{
				hingePos = -8513.513f,
				hingeVel = 0f,
				sliderPos = -0.047786765f,
				sliderVel = 0f,
				camPos = new Vector3(23.6847f, 49.17829f, -20f),
				playerPos = new Vector3(24.195684f, 47.24405f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, -0.039137032f),
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
					new Vector2(24.195684f, 47.24405f),
					new Vector2(24.25719f, 47.593307f),
					new Vector2(24.25719f, 47.593307f),
					new Vector2(24.25712f, 47.54552f),
					new Vector2(24.258883f, 48.717514f),
					new Vector2(24.260506f, 49.797516f)
				},
				rbAngles = new float[] { -4.4859195f, 0f, -8550.088f, -8370.088f, -8370.086f, -8370.086f }
			},
			new SaveState
			{
				hingePos = -6733.3535f,
				hingeVel = 0f,
				sliderPos = 0.70502263f,
				sliderVel = 0f,
				camPos = new Vector3(11.390021f, 110.81839f, -20f),
				playerPos = new Vector3(13.3891945f, 111.00782f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, 0.024248483f),
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
					new Vector2(13.3891945f, 111.00782f),
					new Vector2(13.406042f, 111.36206f),
					new Vector2(13.406046f, 111.36207f),
					new Vector2(13.646355f, 112.02487f),
					new Vector2(14.024048f, 113.13437f),
					new Vector2(14.372077f, 114.15675f)
				},
				rbAngles = new float[] { 2.778944f, 0f, -6769.9287f, -6589.9287f, -6588.7993f, -6588.7993f }
			},
			new SaveState
			{
				hingePos = -8850.242f,
				hingeVel = 0f,
				sliderPos = 0.5795298f,
				sliderVel = 0f,
				camPos = new Vector3(4.495945f, 129.49283f, -20f),
				playerPos = new Vector3(2.6247187f, 130.19347f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, 0.04104381f),
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
					new Vector2(2.6247187f, 130.19347f),
					new Vector2(2.6296403f, 130.54807f),
					new Vector2(2.6296346f, 130.54807f),
					new Vector2(2.4014823f, 131.0808f),
					new Vector2(1.9549062f, 132.16449f),
					new Vector2(1.543505f, 133.16306f)
				},
				rbAngles = new float[] { 4.7045956f, 0f, -8886.818f, -8706.818f, -8707.608f, -8707.608f }
			},
			new SaveState
			{
				hingePos = -6287.871f,
				hingeVel = 0f,
				sliderPos = -0.9825442f,
				sliderVel = 0f,
				camPos = new Vector3(-1.8958615f, 165.26657f, -20f),
				playerPos = new Vector3(-1.9101009f, 163.26657f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, -0.026215924f),
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
					new Vector2(-1.9101009f, 163.26657f),
					new Vector2(-1.857645f, 163.61731f),
					new Vector2(-1.8576448f, 163.61731f),
					new Vector2(-0.9631861f, 163.2107f),
					new Vector2(-2.0318305f, 163.69193f),
					new Vector2(-3.0165854f, 164.13539f)
				},
				rbAngles = new float[] { -3.0044677f, 0f, -6324.447f, -6144.447f, -6144.2437f, -6144.2437f }
			},
			new SaveState
			{
				hingePos = -21595.078f,
				hingeVel = 0f,
				sliderPos = -1.104697f,
				sliderVel = 0f,
				camPos = new Vector3(51.275425f, 229.04633f, -20f),
				playerPos = new Vector3(50.009777f, 227.49614f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, 0.033111162f),
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
					new Vector2(50.009777f, 227.49614f),
					new Vector2(50.02034f, 227.85062f),
					new Vector2(50.02034f, 227.85062f),
					new Vector2(49.079983f, 228.43034f),
					new Vector2(50.07559f, 227.812f),
					new Vector2(50.99304f, 227.24217f)
				},
				rbAngles = new float[] { 3.7949536f, 0f, -21631.654f, -21451.654f, -21451.844f, -21451.844f }
			},
			new SaveState
			{
				hingePos = 17.361101f,
				hingeVel = 0f,
				sliderPos = -0.495323f,
				sliderVel = 0f,
				camPos = new Vector3(63.972397f, 244.76282f, -20f),
				playerPos = new Vector3(62.738464f, 243.18951f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, 0.13063595f),
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
					new Vector2(62.738464f, 243.18951f),
					new Vector2(62.679867f, 243.53926f),
					new Vector2(62.679867f, 243.53926f),
					new Vector2(62.212135f, 243.70227f),
					new Vector2(63.31742f, 243.3125f),
					new Vector2(64.335945f, 242.95331f)
				},
				rbAngles = new float[] { 15.012684f, 0f, -19.214258f, 160.78575f, 160.57533f, 160.57477f }
			},
			new SaveState
			{
				hingePos = -6598.043f,
				hingeVel = 0f,
				sliderPos = -0.7996087f,
				sliderVel = 0f,
				camPos = new Vector3(77.84483f, 262.86215f, -20f),
				playerPos = new Vector3(79.83349f, 262.64764f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, -0.01970503f),
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
					new Vector2(79.83349f, 262.64764f),
					new Vector2(79.88137f, 262.99902f),
					new Vector2(79.88137f, 262.99902f),
					new Vector2(80.6038f, 263.34177f),
					new Vector2(79.54671f, 262.83566f),
					new Vector2(78.57258f, 262.36932f)
				},
				rbAngles = new float[] { -2.258176f, 0f, -6634.618f, -6454.618f, -6454.418f, -6454.418f }
			},
			new SaveState
			{
				hingePos = -5072.0747f,
				hingeVel = 0f,
				sliderPos = -1.0281368f,
				sliderVel = 0f,
				camPos = new Vector3(21.587486f, 261.56485f, -20f),
				playerPos = new Vector3(22.65842f, 259.87573f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, -0.13552165f),
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
					new Vector2(22.65842f, 259.87573f),
					new Vector2(22.785969f, 260.20663f),
					new Vector2(22.785969f, 260.20663f),
					new Vector2(22.411663f, 261.1642f),
					new Vector2(22.836775f, 260.07202f),
					new Vector2(23.228521f, 259.06558f)
				},
				rbAngles = new float[] { -15.577572f, 0f, -5108.6504f, -4928.65f, -4928.7324f, -4928.7324f }
			},
			new SaveState
			{
				hingePos = -4580.436f,
				hingeVel = 0f,
				sliderPos = 0.4996289f,
				sliderVel = 0f,
				camPos = new Vector3(22.834429f, 284.96777f, -20f),
				playerPos = new Vector3(21.849598f, 283.22818f, 0f),
				playerRot = Quaternion.Euler(0f, 0f, 0.0023951374f),
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
					new Vector2(21.849598f, 283.22818f),
					new Vector2(21.881907f, 283.58136f),
					new Vector2(21.881907f, 283.58136f),
					new Vector2(22.108818f, 284.0265f),
					new Vector2(22.642918f, 285.0697f),
					new Vector2(23.135092f, 286.03104f)
				},
				rbAngles = new float[] { 0.2744628f, 0f, -4617.011f, -4437.011f, -4437.1113f, -4437.1113f }
			}
		};
	}

	// Token: 0x060019DB RID: 6619 RVA: 0x000132D3 File Offset: 0x000114D3
	private void LoadSaviour()
	{
		if (this.saviour == null)
		{
			this.saviour = global::UnityEngine.Object.FindObjectOfType<Saviour>();
		}
	}

	// Token: 0x060019DC RID: 6620 RVA: 0x000132EE File Offset: 0x000114EE
	public void Teleport(int x)
	{
		this.LoadSaviour();
		this.saviour.LoadTeleport(this.teleportSaves[x]);
	}

	// Token: 0x060019DD RID: 6621 RVA: 0x0007CCB0 File Offset: 0x0007AEB0
	public string GetSaveState()
	{
		this.LoadSaviour();
		string text;
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(SaveState));
			using (StringWriter stringWriter = new StringWriter())
			{
				xmlSerializer.Serialize(stringWriter, this.saviour.Save());
				text = stringWriter.ToString();
			}
		}
		catch
		{
			text = "";
		}
		return text;
	}

	// Token: 0x060019DE RID: 6622 RVA: 0x0007CD28 File Offset: 0x0007AF28
	public void SaveCustomState()
	{
		try
		{
			this.LoadSaviour();
			this.customSave = this.saviour.Save();
		}
		catch
		{
		}
	}

	// Token: 0x060019DF RID: 6623 RVA: 0x00013309 File Offset: 0x00011509
	public bool LoadCustomState()
	{
		this.LoadSaviour();
		if (this.customSave != null)
		{
			this.saviour.LoadTeleport(this.customSave);
			return true;
		}
		return false;
	}

	// Token: 0x060019E0 RID: 6624 RVA: 0x0007CD64 File Offset: 0x0007AF64
	public string GetSaveStateJson()
	{
		this.LoadSaviour();
		string text = "";
		try
		{
			if (this.customSave != null)
			{
				text = JsonConvertEx.SerializeObject<MapTools.TeleportSaveState>(new MapTools.TeleportSaveState
				{
					hingePos = this.customSave.hingePos,
					sliderPos = this.customSave.sliderPos,
					camPos = new float[]
					{
						this.customSave.camPos.x,
						this.customSave.camPos.y,
						this.customSave.camPos.z
					},
					playerPos = new float[]
					{
						this.customSave.playerPos.x,
						this.customSave.playerPos.y,
						this.customSave.playerPos.z
					},
					playerRot = new float[]
					{
						this.customSave.playerRot.x,
						this.customSave.playerRot.y,
						this.customSave.playerRot.z
					},
					rbPositions = new float[][]
					{
						new float[]
						{
							this.customSave.rbPositions[0].x,
							this.customSave.rbPositions[0].y
						},
						new float[]
						{
							this.customSave.rbPositions[1].x,
							this.customSave.rbPositions[1].y
						},
						new float[]
						{
							this.customSave.rbPositions[2].x,
							this.customSave.rbPositions[2].y
						},
						new float[]
						{
							this.customSave.rbPositions[3].x,
							this.customSave.rbPositions[3].y
						},
						new float[]
						{
							this.customSave.rbPositions[4].x,
							this.customSave.rbPositions[4].y
						},
						new float[]
						{
							this.customSave.rbPositions[5].x,
							this.customSave.rbPositions[5].y
						}
					},
					rbAngles = this.customSave.rbAngles
				});
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex);
		}
		return text;
	}

	// Token: 0x060019E1 RID: 6625 RVA: 0x0007D02C File Offset: 0x0007B22C
	public bool TeleportMap(Map map, int k)
	{
		this.LoadSaviour();
		if (map.teleportSaveStates != null && map.teleportSaveStates.Count > 0)
		{
			if (k == 0)
			{
				k = 10;
			}
			if (map.teleportSaveStates.Count >= k)
			{
				this.saviour.LoadTeleport(map.teleportSaveStates.Values.ElementAt<SaveState>((k + (map.teleportSaveStates.Count - 1)) % map.teleportSaveStates.Count));
				return true;
			}
		}
		if (map.teleportSaveStates == null || map.teleportSaveStates.Count == 0)
		{
			this.saviour.LoadTeleport(this.teleportSaves[k]);
			return true;
		}
		return false;
	}

	// Token: 0x0400117F RID: 4479
	private Saviour saviour;

	// Token: 0x04001180 RID: 4480
	private SaveState[] teleportSaves;

	// Token: 0x04001181 RID: 4481
	private SaveState customSave;
}
