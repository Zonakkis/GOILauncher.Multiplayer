using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

// Token: 0x02000070 RID: 112
public class Saviour : MonoBehaviour
{
	// Token: 0x060002E5 RID: 741 RVA: 0x000276C4 File Offset: 0x000258C4
	private void Start()
	{
		this.canReset = true;
		this.savesSinceWrite = 20;
		this.stillHingeMotor = this.hinge.motor;
		this.stillHingeMotor.motorSpeed = 0f;
		this.stillSliderMotor = this.hinge.motor;
		this.stillSliderMotor.motorSpeed = 0f;
		this.stillHubMotor = this.hubJoint.motor;
		this.stillHubMotor.motorSpeed = 0f;
		this.stillHubMotor.maxMotorTorque = 100f;
		this.cursor.position = this.hammer.position;
		this.slider.motor = this.stillSliderMotor;
		this.hinge.motor = this.stillHingeMotor;
		this.hubJoint.motor = this.stillHubMotor;
		this.debugSaves = new SaveState[2];
		this.debugSaves[0] = new SaveState();
		this.debugSaves[1] = new SaveState();
		this.currentSave = 0;
		this.stream = new MemoryStream();
		this.streamWriter = new StreamWriter(this.stream, Encoding.UTF8);
		this.serializer = new XmlSerializer(typeof(SaveState));
		this.frame = 0;
		Debug.Log("Application version " + float.Parse(Application.version, CultureInfo.InvariantCulture).ToString());
		if (PlayerPrefs.GetInt("NumSaves") != 0)
		{
			this.saveNum = PlayerPrefs.GetInt("NumSaves");
		}
		else
		{
			this.saveNum = 0;
			PlayerPrefs.SetInt("NumSaves", 0);
		}
		if (this.saveNum <= 0)
		{
			Debug.Log("no save detected");
			this.willPlayAnimation = true;
			return;
		}
		if (this.LoadNewestSave())
		{
			this.willPlayAnimation = false;
			return;
		}
		Debug.Log("couldn't load save");
		this.willPlayAnimation = true;
	}

	// Token: 0x060002E6 RID: 742 RVA: 0x00027898 File Offset: 0x00025A98
	public void ResetPlayerButNotDialogue()
	{
		Camera.main.SendMessage("FadeIn");
		this.Load(new SaveState
		{
			hingePos = -1109.8397f,
			hingeVel = 0f,
			sliderPos = -0.7706918f,
			sliderVel = 0f,
			camPos = new Vector3(-42.46717f, -1.342597f, -20f),
			playerPos = new Vector3(-44.292595f, -2.4218144f, 0f),
			playerRot = Quaternion.Euler(0f, 0f, 359.9106f),
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
				new Vector2(-44.292595f, -2.4218144f),
				new Vector2(-44.292595f, -1.8218141f),
				new Vector2(-44.292595f, -1.8218148f),
				new Vector2(-44.39062f, -1.7534542f),
				new Vector2(-43.430946f, -2.4262254f),
				new Vector2(-42.546608f, -3.046185f)
			},
			rbAngles = new float[] { -0.08937922f, 0f, -34.891838f, 145.10817f, 144.96793f, 144.96788f },
			saveNum = 1,
			version = Application.version,
			keyDialogDone = Convert.ToBase64String(this.narrator.getKeyDialogDoneList()),
			observationDialogDone = Convert.ToBase64String(this.narrator.getObservationDialogDoneList()),
			condolenceDialogDone = Convert.ToBase64String(this.narrator.getCondolenceDialogDoneList())
		});
		this.willPlayAnimation = false;
	}

	// Token: 0x060002E7 RID: 743 RVA: 0x000040D3 File Offset: 0x000022D3
	public void OnDestroy()
	{
		if (this.streamWriter != null)
		{
			this.streamWriter.Flush();
		}
	}

	// Token: 0x060002E8 RID: 744 RVA: 0x00027AA0 File Offset: 0x00025CA0
	public void SaveGameNow(bool writeToDisk)
	{
		this.stream = new MemoryStream();
		this.streamWriter = new StreamWriter(this.stream, Encoding.UTF8);
		if (this.stream == null)
		{
			return;
		}
		this.currentSave = ((this.currentSave == 0) ? 1 : 0);
		this.debugSaves[this.currentSave] = this.Save();
		this.serializer.Serialize(this.streamWriter, this.debugSaves[this.currentSave]);
		this.saveString = Encoding.UTF8.GetString(this.stream.GetBuffer());
		if (!this.saveString.StartsWith("<?") || !this.saveString.EndsWith(">"))
		{
			this.streamWriter.Flush();
			this.stream.SetLength(0L);
			Debug.LogWarning("malformed save: " + this.saveString);
			return;
		}
		PlayerPrefs.SetString("SaveGame" + this.currentSave.ToString(), this.saveString);
		PlayerPrefs.SetInt("NumSaves", this.saveNum);
		if (writeToDisk)
		{
			PlayerPrefs.Save();
		}
	}

	// Token: 0x060002E9 RID: 745 RVA: 0x00027BC0 File Offset: 0x00025DC0
	private void Update()
	{
		if (this.willPlayAnimation)
		{
			this.pc.PlayOpeningAnimation();
			this.willPlayAnimation = false;
			this.pc.loadFinished = true;
			this.cc.loadFinished = true;
		}
		this.frame++;
		if (this.frame > 60 && Time.timeScale > 0f)
		{
			this.frame = 0;
			this.savesSinceWrite++;
			if (this.savesSinceWrite > this.savesPerWrite)
			{
				this.SaveGameNow(true);
				this.savesSinceWrite = 0;
			}
			else
			{
				this.SaveGameNow(false);
			}
		}
		if (Application.isEditor)
		{
			if (Input.GetKeyDown(KeyCode.L))
			{
				this.LoadNewestSave();
			}
			if (Input.GetKeyDown(KeyCode.R))
			{
				this.ResetPlayerButNotDialogue();
			}
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				Debug.Log("Deleting old save");
				PlayerPrefs.DeleteKey("SaveGame0");
				PlayerPrefs.DeleteKey("SaveGame1");
			}
		}
	}

	// Token: 0x060002EA RID: 746 RVA: 0x00027CAC File Offset: 0x00025EAC
	public SaveState Save()
	{
		SaveState saveState = new SaveState();
		saveState.hingeVel = this.hinge.jointSpeed;
		saveState.hingePos = this.hinge.jointAngle;
		saveState.sliderVel = this.slider.jointSpeed;
		saveState.sliderPos = this.slider.jointTranslation;
		saveState.playerPos = this.playerTransform.position;
		saveState.playerRot = this.playerTransform.rotation;
		Rigidbody2D[] componentsInChildren = this.playerTransform.GetComponentsInChildren<Rigidbody2D>();
		saveState.rbLinearVelocities = new Vector2[componentsInChildren.Length];
		saveState.rbAngularVelocities = new float[componentsInChildren.Length];
		saveState.rbPositions = new Vector2[componentsInChildren.Length];
		saveState.rbAngles = new float[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			saveState.rbLinearVelocities[i] = componentsInChildren[i].velocity;
			saveState.rbAngularVelocities[i] = componentsInChildren[i].angularVelocity;
			saveState.rbPositions[i] = componentsInChildren[i].position;
			saveState.rbAngles[i] = componentsInChildren[i].rotation;
		}
		this.saveNum++;
		saveState.saveNum = this.saveNum;
		saveState.camPos = Camera.main.transform.position;
		this.tempByteArray = this.narrator.getKeyDialogDoneList();
		if (this.tempByteArray != null)
		{
			saveState.keyDialogDone = Convert.ToBase64String(this.tempByteArray);
		}
		else
		{
			saveState.keyDialogDone = Convert.ToBase64String(new byte[this.narrator.KeyDialog.Count]);
		}
		this.tempByteArray = this.narrator.getObservationDialogDoneList();
		if (this.tempByteArray != null)
		{
			saveState.observationDialogDone = Convert.ToBase64String(this.tempByteArray);
		}
		else
		{
			saveState.observationDialogDone = Convert.ToBase64String(new byte[this.narrator.Observations2.Count]);
		}
		this.tempByteArray = this.narrator.getCondolenceDialogDoneList();
		if (this.tempByteArray != null)
		{
			saveState.condolenceDialogDone = Convert.ToBase64String(this.tempByteArray);
		}
		else
		{
			saveState.condolenceDialogDone = Convert.ToBase64String(new byte[this.narrator.Condolences2.Count]);
		}
		saveState.timePlayed = this.narrator.timePlayedThisGame;
		saveState.speedrun = this.narrator.speedrun;
		saveState.version = Application.version;
		saveState.sidewaysEnabled = SettingsManager.sideWay;
		saveState.sidewaysAngle = SettingsManager.sideWayDegrees;
		return saveState;
	}

	// Token: 0x060002EB RID: 747 RVA: 0x00027F18 File Offset: 0x00026118
	public bool LoadNewestSave()
	{
		this.pc.loadedFromSave = true;
		string @string = PlayerPrefs.GetString("SaveGame0");
		string string2 = PlayerPrefs.GetString("SaveGame1");
		SaveState saveState = null;
		SaveState saveState2 = null;
		bool flag = true;
		bool flag2 = true;
		if (@string.Length > 0)
		{
			using (TextReader textReader = new StringReader(@string))
			{
				if (textReader.Peek() != 60)
				{
					textReader.Read();
				}
				try
				{
					saveState = (SaveState)this.serializer.Deserialize(textReader);
					goto IL_00A0;
				}
				catch (InvalidOperationException ex)
				{
					Debug.LogWarning("failed to load save position 1");
					Debug.LogWarning(ex.Message);
					Debug.LogWarning(ex.StackTrace);
					Debug.LogWarning(textReader);
					flag = false;
					goto IL_00A0;
				}
			}
		}
		flag = false;
		IL_00A0:
		if (string2.Length > 0)
		{
			using (TextReader textReader2 = new StringReader(string2))
			{
				if (textReader2.Peek() != 60)
				{
					textReader2.Read();
				}
				try
				{
					saveState2 = (SaveState)this.serializer.Deserialize(textReader2);
					goto IL_010D;
				}
				catch (InvalidOperationException ex2)
				{
					Debug.LogWarning("failed to load save position 2");
					Debug.LogWarning(ex2.Message);
					Debug.LogWarning(ex2.StackTrace);
					flag2 = false;
					goto IL_010D;
				}
			}
		}
		flag2 = false;
		IL_010D:
		Debug.Log("Save loaded at time: " + DateTime.UtcNow.ToString());
		if (flag && (double)float.Parse(saveState.version, CultureInfo.InvariantCulture) < 1.2)
		{
			return false;
		}
		if (flag2 && (double)float.Parse(saveState2.version, CultureInfo.InvariantCulture) < 1.2)
		{
			return false;
		}
		if (flag && flag2)
		{
			if (saveState.saveNum > saveState2.saveNum)
			{
				this.Load(saveState);
			}
			else
			{
				this.Load(saveState2);
			}
		}
		else if (flag2)
		{
			Debug.LogWarning("loading save 2");
			this.Load(saveState2);
			this.saveNum = saveState2.saveNum;
		}
		else
		{
			if (!flag)
			{
				Debug.LogWarning("nothing to load");
				return false;
			}
			Debug.LogWarning("loading save 1 since 2 was not present");
			this.Load(saveState);
		}
		return true;
	}

	// Token: 0x060002EC RID: 748 RVA: 0x00028134 File Offset: 0x00026334
	public void Load(SaveState loadedSave)
	{
		this.pc.PauseInput(0.5f);
		Rigidbody2D[] componentsInChildren = this.playerTransform.GetComponentsInChildren<Rigidbody2D>();
		if (loadedSave.rbPositions.Length != componentsInChildren.Length)
		{
			return;
		}
		foreach (Rigidbody2D rigidbody2D in componentsInChildren)
		{
		}
		this.spine1.localScale = Vector3.one;
		this.spine2.localScale = Vector3.one;
		Physics2D.simulationMode = SimulationMode2D.Script;
		this.playerTransform.gameObject.SetActive(false);
		this.playerTransform.position = loadedSave.playerPos;
		this.playerTransform.rotation = loadedSave.playerRot;
		this.playerTransform.gameObject.SetActive(true);
		Physics2D.Simulate(Time.fixedDeltaTime * 0.01f);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].position = loadedSave.rbPositions[j];
			componentsInChildren[j].rotation = loadedSave.rbAngles[j];
			componentsInChildren[j].velocity = loadedSave.rbLinearVelocities[j];
			componentsInChildren[j].angularVelocity = loadedSave.rbAngularVelocities[j];
		}
		Physics2D.Simulate(Time.fixedDeltaTime * 0.01f);
		for (int k = 0; k < componentsInChildren.Length; k++)
		{
		}
		this.cc.Teleport(this.playerTransform.position);
		for (int l = 0; l < 50; l++)
		{
			this.cc.FixedUpdate();
		}
		this.narrator.setDialogDoneLists(Convert.FromBase64String(loadedSave.keyDialogDone), Convert.FromBase64String(loadedSave.observationDialogDone), Convert.FromBase64String(loadedSave.condolenceDialogDone));
		this.narrator.speedrun = loadedSave.speedrun;
		Rigidbody2D[] array2 = componentsInChildren;
		for (int m = 0; m < array2.Length; m++)
		{
			array2[m].WakeUp();
		}
		this.cursor.position = this.hammer.position;
		this.narrator.timePlayedThisGame = loadedSave.timePlayed;
		this.pc.loadFinished = true;
		this.cc.loadFinished = true;
		this.pc.StartAnimator();
		Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
		this.slider.motor = this.stillSliderMotor;
		this.hinge.motor = this.stillHingeMotor;
		this.hubJoint.motor = this.stillHubMotor;
		if (loadedSave.sidewaysEnabled && !SettingsManager.sideWay)
		{
			this.playerTransform.Rotate(new Vector3(0f, 0f, -loadedSave.sidewaysAngle));
			return;
		}
		if (loadedSave.sidewaysEnabled && SettingsManager.sideWay && loadedSave.sidewaysAngle != SettingsManager.sideWayDegrees)
		{
			this.playerTransform.Rotate(new Vector3(0f, 0f, -loadedSave.sidewaysAngle));
			this.playerTransform.Rotate(new Vector3(0f, 0f, SettingsManager.sideWayDegrees));
			return;
		}
		if (!loadedSave.sidewaysEnabled && SettingsManager.sideWay)
		{
			this.playerTransform.Rotate(new Vector3(0f, 0f, SettingsManager.sideWayDegrees));
		}
	}

	// Token: 0x060002EE RID: 750 RVA: 0x0002844C File Offset: 0x0002664C
	public void LoadTeleport(SaveState loadedSave)
	{
		Rigidbody2D[] componentsInChildren = this.playerTransform.GetComponentsInChildren<Rigidbody2D>();
		if (loadedSave.rbPositions.Length == componentsInChildren.Length)
		{
			foreach (Rigidbody2D rigidbody2D in componentsInChildren)
			{
			}
			this.spine1.localScale = Vector3.one;
			this.spine2.localScale = Vector3.one;
			Physics2D.simulationMode = SimulationMode2D.Script;
			this.playerTransform.position = loadedSave.playerPos;
			this.playerTransform.rotation = loadedSave.playerRot;
			Physics2D.Simulate(Time.fixedDeltaTime * 0.01f);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].position = loadedSave.rbPositions[j];
				componentsInChildren[j].rotation = loadedSave.rbAngles[j];
				componentsInChildren[j].velocity = loadedSave.rbLinearVelocities[j];
				componentsInChildren[j].angularVelocity = loadedSave.rbAngularVelocities[j];
			}
			Physics2D.Simulate(Time.fixedDeltaTime * 0.01f);
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
			}
			this.cc.Teleport(this.playerTransform.position);
			for (int l = 0; l < 50; l++)
			{
				this.cc.FixedUpdate();
			}
			this.narrator.setDialogDoneLists(Convert.FromBase64String(loadedSave.keyDialogDone), Convert.FromBase64String(loadedSave.observationDialogDone), Convert.FromBase64String(loadedSave.condolenceDialogDone));
			this.narrator.speedrun = loadedSave.speedrun;
			Rigidbody2D[] array2 = componentsInChildren;
			for (int m = 0; m < array2.Length; m++)
			{
				array2[m].WakeUp();
			}
			this.cursor.position = this.hammer.position;
			this.narrator.timePlayedThisGame = loadedSave.timePlayed;
			this.pc.loadFinished = true;
			this.cc.loadFinished = true;
			this.pc.StartAnimator();
			Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
			this.slider.motor = this.stillSliderMotor;
			this.hinge.motor = this.stillHingeMotor;
			this.hubJoint.motor = this.stillHubMotor;
			if (loadedSave.sidewaysEnabled && !SettingsManager.sideWay)
			{
				this.playerTransform.Rotate(new Vector3(0f, 0f, -loadedSave.sidewaysAngle));
				return;
			}
			if (loadedSave.sidewaysEnabled && SettingsManager.sideWay && loadedSave.sidewaysAngle != SettingsManager.sideWayDegrees)
			{
				this.playerTransform.Rotate(new Vector3(0f, 0f, -loadedSave.sidewaysAngle));
				this.playerTransform.Rotate(new Vector3(0f, 0f, SettingsManager.sideWayDegrees));
				return;
			}
			if (!loadedSave.sidewaysEnabled && SettingsManager.sideWay)
			{
				this.playerTransform.Rotate(new Vector3(0f, 0f, SettingsManager.sideWayDegrees));
			}
		}
	}

	// Token: 0x060002EF RID: 751 RVA: 0x00028734 File Offset: 0x00026934
	public void ResetPlayer(Vector3 PlayerPos, bool Anim = false)
	{
		Camera.main.SendMessage("FadeIn");
		this.Load(new SaveState
		{
			hingePos = -1109.8397f,
			hingeVel = 0f,
			sliderPos = -0.7706918f,
			sliderVel = 0f,
			camPos = new Vector3(-42.46717f, -1.342597f, -20f),
			playerPos = PlayerPos,
			playerRot = Quaternion.Euler(0f, 0f, 359.9106f),
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
				new Vector2(-44.292595f, -2.4218144f),
				new Vector2(-44.292595f, -1.8218141f),
				new Vector2(-44.292595f, -1.8218148f),
				new Vector2(-44.39062f, -1.7534542f),
				new Vector2(-43.430946f, -2.4262254f),
				new Vector2(-42.546608f, -3.046185f)
			},
			rbAngles = new float[] { -0.08937922f, 0f, -34.891838f, 145.10817f, 144.96793f, 144.96788f },
			saveNum = 1,
			version = Application.version,
			keyDialogDone = Convert.ToBase64String(this.narrator.getKeyDialogDoneList()),
			observationDialogDone = Convert.ToBase64String(this.narrator.getObservationDialogDoneList()),
			condolenceDialogDone = Convert.ToBase64String(this.narrator.getCondolenceDialogDoneList())
		});
		this.willPlayAnimation = Anim;
	}

	// Token: 0x04000477 RID: 1143
	public Narrator narrator;

	// Token: 0x04000478 RID: 1144
	public Rigidbody2D hammer;

	// Token: 0x04000479 RID: 1145
	public SliderJoint2D slider;

	// Token: 0x0400047A RID: 1146
	public HingeJoint2D hinge;

	// Token: 0x0400047B RID: 1147
	public Rigidbody2D cursor;

	// Token: 0x0400047C RID: 1148
	public HingeJoint2D hubJoint;

	// Token: 0x0400047D RID: 1149
	public Transform playerTransform;

	// Token: 0x0400047E RID: 1150
	private JointMotor2D stillHingeMotor;

	// Token: 0x0400047F RID: 1151
	private JointMotor2D stillSliderMotor;

	// Token: 0x04000480 RID: 1152
	private JointMotor2D stillHubMotor;

	// Token: 0x04000481 RID: 1153
	private SaveState[] debugSaves;

	// Token: 0x04000482 RID: 1154
	private int currentSave;

	// Token: 0x04000483 RID: 1155
	public PlayerControl pc;

	// Token: 0x04000484 RID: 1156
	public CameraControl cc;

	// Token: 0x04000485 RID: 1157
	private MemoryStream stream;

	// Token: 0x04000486 RID: 1158
	private StreamWriter streamWriter;

	// Token: 0x04000487 RID: 1159
	private TextReader textReader;

	// Token: 0x04000488 RID: 1160
	private XmlSerializer serializer;

	// Token: 0x04000489 RID: 1161
	private int frame;

	// Token: 0x0400048A RID: 1162
	private int saveNum;

	// Token: 0x0400048B RID: 1163
	private bool willPlayAnimation;

	// Token: 0x0400048C RID: 1164
	private int savesSinceWrite;

	// Token: 0x0400048D RID: 1165
	private int savesPerWrite = 2;

	// Token: 0x0400048E RID: 1166
	public bool canReset;

	// Token: 0x0400048F RID: 1167
	private string saveString;

	// Token: 0x04000490 RID: 1168
	private byte[] tempByteArray;

	// Token: 0x04000491 RID: 1169
	public Transform spine1;

	// Token: 0x04000492 RID: 1170
	public Transform spine2;

	// Token: 0x04000493 RID: 1171
	public float targetMaxDelta = 0.06666667f;
}
