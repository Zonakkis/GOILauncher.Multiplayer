using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

// Token: 0x02000048 RID: 72
public class Saviour : MonoBehaviour
{
	// Token: 0x06000250 RID: 592 RVA: 0x000156F8 File Offset: 0x000138F8
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

	// Token: 0x06000251 RID: 593 RVA: 0x000158CC File Offset: 0x00013ACC
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

	// Token: 0x06000252 RID: 594 RVA: 0x00015AD1 File Offset: 0x00013CD1
	public void OnDestroy()
	{
		if (this.streamWriter != null)
		{
			this.streamWriter.Flush();
		}
	}

	// Token: 0x06000253 RID: 595 RVA: 0x00015AE8 File Offset: 0x00013CE8
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

	// Token: 0x06000254 RID: 596 RVA: 0x00015C08 File Offset: 0x00013E08
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

	// Token: 0x06000255 RID: 597 RVA: 0x00015CF4 File Offset: 0x00013EF4
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
		return saveState;
	}

	// Token: 0x06000256 RID: 598 RVA: 0x00015F48 File Offset: 0x00014148
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

	// Token: 0x06000257 RID: 599 RVA: 0x00016168 File Offset: 0x00014368
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
		new int[componentsInChildren.Length];
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
		Rigidbody2D[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].WakeUp();
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
	}

	// Token: 0x040003CA RID: 970
	public Narrator narrator;

	// Token: 0x040003CB RID: 971
	public Rigidbody2D hammer;

	// Token: 0x040003CC RID: 972
	public SliderJoint2D slider;

	// Token: 0x040003CD RID: 973
	public HingeJoint2D hinge;

	// Token: 0x040003CE RID: 974
	public Rigidbody2D cursor;

	// Token: 0x040003CF RID: 975
	public HingeJoint2D hubJoint;

	// Token: 0x040003D0 RID: 976
	public Transform playerTransform;

	// Token: 0x040003D1 RID: 977
	private JointMotor2D stillHingeMotor;

	// Token: 0x040003D2 RID: 978
	private JointMotor2D stillSliderMotor;

	// Token: 0x040003D3 RID: 979
	private JointMotor2D stillHubMotor;

	// Token: 0x040003D4 RID: 980
	private SaveState[] debugSaves;

	// Token: 0x040003D5 RID: 981
	private int currentSave;

	// Token: 0x040003D6 RID: 982
	public PlayerControl pc;

	// Token: 0x040003D7 RID: 983
	public CameraControl cc;

	// Token: 0x040003D8 RID: 984
	private MemoryStream stream;

	// Token: 0x040003D9 RID: 985
	private StreamWriter streamWriter;

	// Token: 0x040003DA RID: 986
	private TextReader textReader;

	// Token: 0x040003DB RID: 987
	private XmlSerializer serializer;

	// Token: 0x040003DC RID: 988
	private int frame;

	// Token: 0x040003DD RID: 989
	private int saveNum;

	// Token: 0x040003DE RID: 990
	private bool willPlayAnimation;

	// Token: 0x040003DF RID: 991
	private int savesSinceWrite;

	// Token: 0x040003E0 RID: 992
	private int savesPerWrite = 2;

	// Token: 0x040003E1 RID: 993
	public bool canReset;

	// Token: 0x040003E2 RID: 994
	private string saveString;

	// Token: 0x040003E3 RID: 995
	private byte[] tempByteArray;

	// Token: 0x040003E4 RID: 996
	public Transform spine1;

	// Token: 0x040003E5 RID: 997
	public Transform spine2;

	// Token: 0x040003E6 RID: 998
	public float targetMaxDelta = 0.06666667f;
}
