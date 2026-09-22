using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DigitalRuby.Threading;
using UnityEngine;

// Token: 0x02000141 RID: 321
public class Saviour : MonoBehaviour
{
	// Token: 0x0600086B RID: 2155 RVA: 0x0004826C File Offset: 0x0004666C
	private void Start()
	{
		this.savesPerWrite = 100;
		if (this.sf == null)
		{
			this.sf = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenFader>();
		}
		if (this.gc == null)
		{
			this.gc = GameObject.FindGameObjectWithTag("GravityControl").GetComponent<GravityControl>();
		}
		if (this.pc == null)
		{
			this.pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
		}
		Rigidbody2D[] attachedRigidBodies = this.pc.AttachedRigidBodies;
		this.rbLinearVelocities = new Vector2[attachedRigidBodies.Length];
		this.rbAngularVelocities = new float[attachedRigidBodies.Length];
		this.rbPositions = new Vector2[attachedRigidBodies.Length];
		this.rbAngles = new float[attachedRigidBodies.Length];
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
		this.streamWriter.AutoFlush = true;
		this.serializer = new XmlSerializer(typeof(SaveState));
		this.frame = 0;
		this.exiting = false;
		if (PlayerPrefs.HasKey("NumSaves"))
		{
			if (PlayerPrefs.GetInt("NumSaves") != 0)
			{
				this.saveNum = PlayerPrefs.GetInt("NumSaves");
			}
			else
			{
				this.saveNum = 0;
				PlayerPrefs.SetInt("NumSaves", 0);
			}
		}
		else
		{
			this.saveNum = 0;
			PlayerPrefs.SetInt("NumSaves", 0);
		}
		if (this.saveNum > 0)
		{
			if (this.LoadNewestSave())
			{
				this.willPlayAnimation = false;
			}
			else
			{
				Debug.LogWarning("couldn't load save");
				this.willPlayAnimation = true;
				PlayerPrefs.SetInt("NumSaves", 0);
				if (PlayerPrefs.HasKey("SaveGame0"))
				{
					PlayerPrefs.DeleteKey("SaveGame0");
				}
				if (PlayerPrefs.HasKey("SaveGame1"))
				{
					PlayerPrefs.DeleteKey("SaveGame1");
				}
			}
		}
		else
		{
			Debug.LogWarning("no save detected");
			this.willPlayAnimation = true;
		}
		this.ApplicationVersion = Application.version;
		this.readyToGo = true;
	}

	// Token: 0x0600086C RID: 2156 RVA: 0x00048565 File Offset: 0x00046965
	private void OnApplicationQuit()
	{
		this.exiting = true;
	}

	// Token: 0x0600086D RID: 2157 RVA: 0x0004856E File Offset: 0x0004696E
	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			this.anxiousToSave = true;
		}
	}

	// Token: 0x0600086E RID: 2158 RVA: 0x00048580 File Offset: 0x00046980
	public void ResetPlayerButNotDialogue()
	{
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
			version = this.ApplicationVersion,
			keyDialogDone = Convert.ToBase64String(this.narrator.getKeyDialogDoneList()),
			observationDialogDone = Convert.ToBase64String(this.narrator.getObservationDialogDoneList()),
			condolenceDialogDone = Convert.ToBase64String(this.narrator.getCondolenceDialogDoneList())
		});
		this.sf.StartScene();
		this.willPlayAnimation = false;
	}

	// Token: 0x0600086F RID: 2159 RVA: 0x000487BE File Offset: 0x00046BBE
	private void OnApplicationPause(bool paused)
	{
		this.SaveGameNow(true);
	}

	// Token: 0x06000870 RID: 2160 RVA: 0x000487C8 File Offset: 0x00046BC8
	public void SaveGameNow(bool writeToDisk)
	{
		if (this.stream == null)
		{
			return;
		}
		if (this.dispatched)
		{
			return;
		}
		if (this.gc.creditsUp)
		{
			return;
		}
		this.currentSave = ((this.currentSave != 0) ? 0 : 1);
		this.saveString = ((this.currentSave != 0) ? this.saveString1 : this.saveString0);
		this.debugSaves[this.currentSave] = this.Save();
		if (writeToDisk)
		{
			this.dispatched = true;
			EZThread.ExecuteInBackground(new global::System.Func<object>(this.SerializeSaveThreadWrite), new Action<object>(this.SerialzeSaveThreadResultWrite));
		}
		else
		{
			this.dispatched = true;
			EZThread.ExecuteInBackground(new global::System.Func<object>(this.SerializeSaveThread), new Action<object>(this.SerialzeSaveThreadResult));
		}
	}

	// Token: 0x06000871 RID: 2161 RVA: 0x000488A0 File Offset: 0x00046CA0
	private string SerializeSaveThread()
	{
		if (this.exiting)
		{
			return null;
		}
		if (this.gc.creditsUp)
		{
			return null;
		}
		this.stream.SetLength(0L);
		if (this.exiting)
		{
			return null;
		}
		try
		{
			this.serializer.Serialize(this.streamWriter, this.debugSaves[this.currentSave]);
		}
		catch
		{
			Debug.LogError("save serialization failed");
			return null;
		}
		if (this.exiting)
		{
			return null;
		}
		try
		{
			this.buffer = this.stream.GetBuffer();
		}
		catch
		{
			Debug.LogError("save buffer get failed");
			return null;
		}
		if (this.exiting)
		{
			return null;
		}
		try
		{
			this.utfString = Encoding.UTF8.GetString(this.buffer);
		}
		catch
		{
			Debug.LogError("UTF8 encode failed");
			return null;
		}
		return this.utfString;
	}

	// Token: 0x06000872 RID: 2162 RVA: 0x000489BC File Offset: 0x00046DBC
	private void SerialzeSaveThreadResult(object result)
	{
		this.dispatched = false;
		if (this.exiting)
		{
			return;
		}
		if (this.gc.creditsUp)
		{
			return;
		}
		if (result == null)
		{
			return;
		}
		this.encodedstring = result as string;
		PlayerPrefs.SetString(this.saveString, this.encodedstring);
		PlayerPrefs.SetInt("NumSaves", this.saveNum);
	}

	// Token: 0x06000873 RID: 2163 RVA: 0x00048A24 File Offset: 0x00046E24
	private string SerializeSaveThreadWrite()
	{
		if (this.exiting)
		{
			return null;
		}
		if (this.gc.creditsUp)
		{
			return null;
		}
		this.stream.SetLength(0L);
		if (this.exiting)
		{
			return null;
		}
		try
		{
			this.serializer.Serialize(this.streamWriter, this.debugSaves[this.currentSave]);
		}
		catch
		{
			Debug.LogError("save serialization failed");
			return null;
		}
		if (this.exiting)
		{
			return null;
		}
		try
		{
			this.buffer = this.stream.GetBuffer();
		}
		catch
		{
			Debug.LogError("save buffer get failed");
			return null;
		}
		if (this.exiting)
		{
			return null;
		}
		try
		{
			this.utfString = Encoding.UTF8.GetString(this.buffer);
		}
		catch
		{
			Debug.LogError("UTF8 encode failed");
			return null;
		}
		return this.utfString;
	}

	// Token: 0x06000874 RID: 2164 RVA: 0x00048B40 File Offset: 0x00046F40
	private void SerialzeSaveThreadResultWrite(object result)
	{
		this.dispatched = false;
		if (this.exiting)
		{
			return;
		}
		if (this.gc.creditsUp)
		{
			return;
		}
		if (result == null)
		{
			return;
		}
		this.encodedstring = result as string;
		PlayerPrefs.SetString(this.saveString, this.encodedstring);
		PlayerPrefs.SetInt("NumSaves", this.saveNum);
		PlayerPrefs.Save();
	}

	// Token: 0x06000875 RID: 2165 RVA: 0x00048BAC File Offset: 0x00046FAC
	private void Update()
	{
		if (this.willPlayAnimation)
		{
			if (this.pc == null)
			{
				this.pc = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>();
			}
			if (this.pc != null)
			{
				this.pc.PlayOpeningAnimation();
				this.willPlayAnimation = false;
			}
		}
		if (this.gc.creditsUp)
		{
			return;
		}
		if (!this.readyToGo)
		{
			return;
		}
		this.frame++;
		if (this.anxiousToSave)
		{
			this.anxiousFrame++;
			if (this.pc.IsInputIdle || this.anxiousFrame > 1200)
			{
				this.SaveGameNow(true);
				this.savesSinceWrite = 0;
				this.anxiousToSave = false;
				this.frame = 0;
				this.anxiousFrame = 0;
				return;
			}
		}
		if (this.frame > 60 && Time.timeScale > 0f)
		{
			this.frame = 0;
			this.savesSinceWrite++;
			if (this.savesSinceWrite > this.savesPerWrite)
			{
				if (this.pc.IsInputIdle)
				{
					this.SaveGameNow(true);
					this.savesSinceWrite = 0;
					this.anxiousFrame = 0;
					this.anxiousToSave = false;
					return;
				}
				this.anxiousToSave = true;
				this.anxiousFrame = 0;
			}
			this.SaveGameNow(false);
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

	// Token: 0x06000876 RID: 2166 RVA: 0x00048D74 File Offset: 0x00047174
	public SaveState Save()
	{
		this.newSave = new SaveState();
		this.newSave.hingeVel = this.hinge.jointSpeed;
		this.newSave.hingePos = this.hinge.jointAngle;
		this.newSave.sliderVel = this.slider.jointSpeed;
		this.newSave.sliderPos = this.slider.jointTranslation;
		this.newSave.playerPos = this.playerTransform.position;
		this.newSave.playerRot = this.playerTransform.rotation;
		this.rbs = this.pc.AttachedRigidBodies;
		this.newSave.rbLinearVelocities = this.rbLinearVelocities;
		this.newSave.rbAngularVelocities = this.rbAngularVelocities;
		this.newSave.rbPositions = this.rbPositions;
		this.newSave.rbAngles = this.rbAngles;
		for (int i = 0; i < this.rbs.Length; i++)
		{
			this.newSave.rbLinearVelocities[i] = this.rbs[i].velocity;
			this.newSave.rbAngularVelocities[i] = this.rbs[i].angularVelocity;
			this.newSave.rbPositions[i] = this.rbs[i].position;
			this.newSave.rbAngles[i] = this.rbs[i].rotation;
		}
		this.saveNum++;
		this.newSave.saveNum = this.saveNum;
		this.newSave.camPos = Camera.main.transform.position;
		this.newSave.keyDialogDone = Convert.ToBase64String(this.narrator.getKeyDialogDoneList());
		this.newSave.observationDialogDone = Convert.ToBase64String(this.narrator.getObservationDialogDoneList());
		this.newSave.condolenceDialogDone = Convert.ToBase64String(this.narrator.getCondolenceDialogDoneList());
		this.newSave.timePlayed = this.narrator.timePlayedThisGame;
		this.newSave.speedrun = this.narrator.speedrun;
		this.newSave.version = this.ApplicationVersion;
		return this.newSave;
	}

	// Token: 0x06000877 RID: 2167 RVA: 0x00048FC8 File Offset: 0x000473C8
	public bool LoadNewestSave()
	{
		this.pc.loadedFromSave = true;
		string @string = PlayerPrefs.GetString("SaveGame0");
		string string2 = PlayerPrefs.GetString("SaveGame1");
		SaveState saveState = null;
		SaveState saveState2 = null;
		if (@string.Length > 0)
		{
			using (TextReader textReader = new StringReader(@string))
			{
				try
				{
					saveState = (SaveState)this.serializer.Deserialize(textReader);
				}
				catch
				{
					Debug.LogError("Save state 1 deserialize failed");
					saveState = null;
				}
			}
		}
		if (string2.Length > 0)
		{
			using (TextReader textReader2 = new StringReader(string2))
			{
				try
				{
					saveState2 = (SaveState)this.serializer.Deserialize(textReader2);
				}
				catch
				{
					Debug.LogError("Save state 2 deserialize failed");
					saveState2 = null;
				}
			}
		}
		if (saveState == null && saveState2 == null)
		{
			Debug.LogWarning("no save file to load");
			return false;
		}
		if (saveState == null && saveState2 != null)
		{
			Debug.LogWarning("loading save 2 since 1 was not present");
			this.Load(saveState2);
			this.saveNum = saveState2.saveNum;
		}
		else if (saveState != null && saveState2 == null)
		{
			Debug.LogWarning("loading save 1 since 2 was not present");
			this.Load(saveState);
		}
		else if (saveState.saveNum > saveState2.saveNum)
		{
			this.Load(saveState);
		}
		else
		{
			this.Load(saveState2);
		}
		return true;
	}

	// Token: 0x06000878 RID: 2168 RVA: 0x00049160 File Offset: 0x00047560
	public void Load(SaveState loadedSave)
	{
		Rigidbody2D[] componentsInChildren = this.playerTransform.GetComponentsInChildren<Rigidbody2D>();
		if (loadedSave.rbPositions.Length != componentsInChildren.Length)
		{
			return;
		}
		foreach (Rigidbody2D rigidbody2D in componentsInChildren)
		{
			rigidbody2D.Sleep();
		}
		this.spine1.localScale = Vector3.one;
		this.spine2.localScale = Vector3.one;
		this.playerTransform.gameObject.SetActive(false);
		this.playerTransform.position = loadedSave.playerPos;
		this.playerTransform.rotation = loadedSave.playerRot;
		this.playerTransform.gameObject.SetActive(true);
		Camera.main.SendMessage("Teleport", loadedSave.camPos);
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			componentsInChildren[j].position = loadedSave.rbPositions[j];
			componentsInChildren[j].rotation = loadedSave.rbAngles[j];
			componentsInChildren[j].velocity = loadedSave.rbLinearVelocities[j];
			componentsInChildren[j].angularVelocity = loadedSave.rbAngularVelocities[j];
		}
		this.cursor.position = this.hammer.position;
		this.slider.motor = this.stillSliderMotor;
		this.hinge.motor = this.stillHingeMotor;
		this.hubJoint.motor = this.stillHubMotor;
		this.narrator.setDialogDoneLists(Convert.FromBase64String(loadedSave.keyDialogDone), Convert.FromBase64String(loadedSave.observationDialogDone), Convert.FromBase64String(loadedSave.condolenceDialogDone));
		this.narrator.speedrun = loadedSave.speedrun;
		foreach (Rigidbody2D rigidbody2D2 in componentsInChildren)
		{
			rigidbody2D2.WakeUp();
		}
		Time.timeScale = 1f;
		this.narrator.timePlayedThisGame = loadedSave.timePlayed;
		this.pc.StartAnimator();
	}

	// Token: 0x0400084C RID: 2124
	public Narrator narrator;

	// Token: 0x0400084D RID: 2125
	public Rigidbody2D hammer;

	// Token: 0x0400084E RID: 2126
	public SliderJoint2D slider;

	// Token: 0x0400084F RID: 2127
	public HingeJoint2D hinge;

	// Token: 0x04000850 RID: 2128
	public Rigidbody2D cursor;

	// Token: 0x04000851 RID: 2129
	public HingeJoint2D hubJoint;

	// Token: 0x04000852 RID: 2130
	public Transform playerTransform;

	// Token: 0x04000853 RID: 2131
	private JointMotor2D stillHingeMotor;

	// Token: 0x04000854 RID: 2132
	private JointMotor2D stillSliderMotor;

	// Token: 0x04000855 RID: 2133
	private JointMotor2D stillHubMotor;

	// Token: 0x04000856 RID: 2134
	public string ApplicationVersion;

	// Token: 0x04000857 RID: 2135
	private SaveState[] debugSaves;

	// Token: 0x04000858 RID: 2136
	private int currentSave;

	// Token: 0x04000859 RID: 2137
	public PlayerControl pc;

	// Token: 0x0400085A RID: 2138
	public ScreenFader sf;

	// Token: 0x0400085B RID: 2139
	public GravityControl gc;

	// Token: 0x0400085C RID: 2140
	private MemoryStream stream;

	// Token: 0x0400085D RID: 2141
	private StreamWriter streamWriter;

	// Token: 0x0400085E RID: 2142
	private TextWriter textWriter;

	// Token: 0x0400085F RID: 2143
	private TextReader textReader;

	// Token: 0x04000860 RID: 2144
	private XmlSerializer serializer;

	// Token: 0x04000861 RID: 2145
	private int frame;

	// Token: 0x04000862 RID: 2146
	private int anxiousFrame;

	// Token: 0x04000863 RID: 2147
	private int saveNum;

	// Token: 0x04000864 RID: 2148
	private bool willPlayAnimation;

	// Token: 0x04000865 RID: 2149
	private int savesSinceWrite;

	// Token: 0x04000866 RID: 2150
	private int savesPerWrite = 20;

	// Token: 0x04000867 RID: 2151
	private bool readyToGo;

	// Token: 0x04000868 RID: 2152
	private bool exiting;

	// Token: 0x04000869 RID: 2153
	private Rigidbody2D[] rbs;

	// Token: 0x0400086A RID: 2154
	private Vector2[] rbLinearVelocities = new Vector2[6];

	// Token: 0x0400086B RID: 2155
	private float[] rbAngularVelocities = new float[6];

	// Token: 0x0400086C RID: 2156
	private Vector2[] rbPositions = new Vector2[6];

	// Token: 0x0400086D RID: 2157
	private float[] rbAngles = new float[6];

	// Token: 0x0400086E RID: 2158
	private string saveString;

	// Token: 0x0400086F RID: 2159
	private string saveString0 = "SaveGame0";

	// Token: 0x04000870 RID: 2160
	private string saveString1 = "SaveGame1";

	// Token: 0x04000871 RID: 2161
	private byte[] buffer = new byte[0];

	// Token: 0x04000872 RID: 2162
	private string encodedstring;

	// Token: 0x04000873 RID: 2163
	private bool dispatched;

	// Token: 0x04000874 RID: 2164
	private string utfString;

	// Token: 0x04000875 RID: 2165
	private bool anxiousToSave;

	// Token: 0x04000876 RID: 2166
	private SaveState newSave;

	// Token: 0x04000877 RID: 2167
	public Transform spine1;

	// Token: 0x04000878 RID: 2168
	public Transform spine2;
}
