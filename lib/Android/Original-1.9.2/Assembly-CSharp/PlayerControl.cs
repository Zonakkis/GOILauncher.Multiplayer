using System;
using System.Collections;
using System.Collections.Generic;
using Noodle;
using UnityEngine;

// Token: 0x02000136 RID: 310
public class PlayerControl : MonoBehaviour
{
	// Token: 0x170000A8 RID: 168
	// (get) Token: 0x060007E4 RID: 2020 RVA: 0x00043F0F File Offset: 0x0004230F
	public bool IsInputIdle
	{
		get
		{
			return this.cursorInputVelocity == Vector2.zero;
		}
	}

	// Token: 0x060007E5 RID: 2021 RVA: 0x00043F24 File Offset: 0x00042324
	private void Awake()
	{
		this.loadedFromSave = false;
		PlayerControl.SaveScreenDimensions();
		PlayerControl.dpi = Screen.dpi;
		if (PlayerControl.dpi <= 0f)
		{
			if (DeviceDisplay.deviceID != "iPhone10,3")
			{
				Debug.LogError("Device " + DeviceDisplay.deviceID + " returned 0 dpi");
			}
			NoodleManager.Instance.SubmitNoodleEvent("DpiZeroError");
			PlayerControl.dpi = 400f;
		}
	}

	// Token: 0x060007E6 RID: 2022 RVA: 0x00043F9C File Offset: 0x0004239C
	private void Start()
	{
		this.ghostedCols = new List<Collider2D>();
		this.hammerCollider = this.tip.GetComponent<PolygonCollider2D>();
		this.cursorInputVelocity = Vector2.zero;
		this.hingeJointMotorValues = this.hj.motor;
		this.slider = this.sj.motor;
		this.oldAngle = 0f;
		this.fakeCursorRB = this.fakeCursor.GetComponent<Rigidbody2D>();
		this.mouseVelocityAverage = 0f;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		this.pauseInputTimer = 2f;
		this.mouseSnap = false;
		if (PlayerPrefs.HasKey("Trackpad"))
		{
			this.trackpad = PlayerPrefs.GetInt("Trackpad") == 1;
		}
		else
		{
			this.trackpad = true;
		}
		this.mobileMan = GameObject.FindGameObjectWithTag("MobileManager").GetComponent<MobileManager>();
		MobileManager.MobileScale deviceScale = this.mobileMan.getDeviceScale();
		if (Application.isEditor)
		{
			bool flag = false;
			if (flag)
			{
				PlayerControl.dpi = 600f;
			}
		}
		this.mobileScreenDPIAdjust = 300f / PlayerControl.dpi;
		this.prevCursorInputVelocity = Vector2.zero;
		this.cursorInputVelocity = Vector2.zero;
		int @int = PlayerPrefs.GetInt("mobileAccel", 1);
		if (@int == 1)
		{
			this.setStandardCurve();
		}
		else if (@int == 2)
		{
			this.setHighCurve();
		}
		else if (@int == 0)
		{
			this.setLowCurve();
		}
		else if (@int == 3)
		{
			this.setNoCurve();
		}
	}

	// Token: 0x060007E7 RID: 2023 RVA: 0x00044121 File Offset: 0x00042521
	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			base.StartCoroutine(this.LockCursor());
		}
	}

	// Token: 0x060007E8 RID: 2024 RVA: 0x00044138 File Offset: 0x00042538
	private IEnumerator LockCursor()
	{
		yield return new WaitForSeconds(0.1f);
		Cursor.lockState = CursorLockMode.Locked;
		yield break;
	}

	// Token: 0x060007E9 RID: 2025 RVA: 0x0004414C File Offset: 0x0004254C
	private void GhostHammer(PolygonCollider2D otherCol)
	{
		this.ghostedCols.Add(otherCol);
		Physics2D.IgnoreCollision(this.hammerCollider, otherCol);
	}

	// Token: 0x060007EA RID: 2026 RVA: 0x00044166 File Offset: 0x00042566
	private static void SaveScreenDimensions()
	{
		PlayerControl.originalHeight = Screen.height;
		PlayerControl.originalWidth = Screen.width;
		PlayerControl.originalDpi = Screen.dpi;
		if (PlayerControl.originalDpi < 1f)
		{
			PlayerControl.originalDpi = 400f;
		}
	}

	// Token: 0x060007EB RID: 2027 RVA: 0x000441A0 File Offset: 0x000425A0
	public static IEnumerator AdjustScreenResolution(float resRatio)
	{
		float aspectRatio = (float)PlayerControl.originalWidth / (float)PlayerControl.originalHeight;
		int newHeight = PlayerControl.originalHeight;
		if (PlayerControl.originalHeight > 720)
		{
			newHeight = Mathf.RoundToInt(Mathf.Max(720f, (float)PlayerControl.originalHeight * resRatio));
		}
		int newWidth = Mathf.RoundToInt((float)newHeight * aspectRatio);
		Debug.LogFormat("[ScreenRes] Original Screen {0}x{1}, dpi {2}", new object[]
		{
			PlayerControl.originalWidth,
			PlayerControl.originalHeight,
			PlayerControl.originalDpi
		});
		Debug.LogFormat("[ScreenRes] Changing Resolution to {0}x{1}", new object[] { newWidth, newHeight });
		Screen.SetResolution(newWidth, newHeight, true);
		yield return null;
		PlayerControl.dpi = PlayerControl.originalDpi * ((float)Screen.height / (float)PlayerControl.originalHeight);
		Debug.LogFormat("[ScreenRes] Expecting Screen.dpi to now be {0}", new object[] { PlayerControl.dpi });
		Debug.LogFormat("[ScreenRes] New Screen {0}x{1}, dpi {2}", new object[]
		{
			Screen.width,
			Screen.height,
			Screen.dpi
		});
		yield break;
	}

	// Token: 0x060007EC RID: 2028 RVA: 0x000441BB File Offset: 0x000425BB
	public void SetSensitivity(float newSensitivity)
	{
		this.mouseSensitivity = newSensitivity;
	}

	// Token: 0x060007ED RID: 2029 RVA: 0x000441C4 File Offset: 0x000425C4
	public void PauseInput(float timeToPause)
	{
		this.pauseInputTimer = timeToPause * 2f;
	}

	// Token: 0x060007EE RID: 2030 RVA: 0x000441D3 File Offset: 0x000425D3
	public void StopAnimator()
	{
		if (this.pose == null)
		{
			this.pose = base.GetComponentInChildren<PoseControl>();
		}
		this.pose.StopAnimator();
	}

	// Token: 0x060007EF RID: 2031 RVA: 0x000441FD File Offset: 0x000425FD
	public void StartAnimator()
	{
		if (this.pose == null)
		{
			this.pose = base.GetComponentInChildren<PoseControl>();
		}
		this.pose.StartAnimator();
	}

	// Token: 0x060007F0 RID: 2032 RVA: 0x00044227 File Offset: 0x00042627
	public void PlayOpeningAnimation()
	{
		base.GetComponentInChildren<PoseControl>().PlayOpeningAnimation();
	}

	// Token: 0x060007F1 RID: 2033 RVA: 0x00044234 File Offset: 0x00042634
	public void HammerReturn()
	{
		if (this.tip.position.y - 0.4f < base.transform.position.y)
		{
			return;
		}
		this.mouseSnap = true;
		this.fakeCursorRB.MovePosition(this.tip.position);
	}

	// Token: 0x060007F2 RID: 2034 RVA: 0x00044295 File Offset: 0x00042695
	public void Pause()
	{
		this.menuPause = true;
	}

	// Token: 0x060007F3 RID: 2035 RVA: 0x0004429E File Offset: 0x0004269E
	public void UnPause()
	{
		this.menuPause = false;
	}

	// Token: 0x060007F4 RID: 2036 RVA: 0x000442A8 File Offset: 0x000426A8
	private void Update()
	{
		if (!this.mobileMan.Ready)
		{
			return;
		}
		bool flag = false;
		Vector2 vector = Vector2.zero;
		float num = 0f;
		TouchPhase touchPhase = TouchPhase.Canceled;
		if (Input.touchSupported)
		{
			if (Input.touchCount == 0)
			{
				return;
			}
			Touch touch = default(Touch);
			for (int i = 0; i < Input.touchCount; i++)
			{
				touch = Input.GetTouch(i);
				if (touch.fingerId == 0)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				vector = touch.deltaPosition;
				num = touch.deltaTime;
				touchPhase = touch.phase;
			}
		}
		else
		{
			vector = Input.mousePosition - this.prevMouseInput;
			this.prevMouseInput = Input.mousePosition;
			if (Mathf.Approximately(vector.magnitude, 0f))
			{
				vector = Vector2.zero;
			}
			num = Time.deltaTime;
			touchPhase = ((!Input.GetMouseButtonDown(0)) ? ((!Input.GetMouseButtonUp(0)) ? ((!Input.GetMouseButton(0) || !(vector != Vector2.zero)) ? ((!Input.GetMouseButton(0) || !(vector == Vector2.zero)) ? TouchPhase.Canceled : TouchPhase.Stationary) : TouchPhase.Moved) : TouchPhase.Ended) : TouchPhase.Began);
			flag = touchPhase != TouchPhase.Canceled;
		}
		if (!flag)
		{
			return;
		}
		if (touchPhase == TouchPhase.Began)
		{
			this.skipfirstMoveInput = true;
		}
		else if (touchPhase == TouchPhase.Moved)
		{
			if (this.skipfirstMoveInput)
			{
				this.skipfirstMoveInput = false;
			}
			else
			{
				this.prevCursorInputVelocity = this.cursorInputVelocity;
				float num2 = PlayerControl.dpi;
				Vector2 vector2 = vector;
				Vector2 vector3 = vector2 / num2 * 2.54f * 10f;
				Vector2 vector4 = vector3 / num;
				this.cursorInputVelocity = vector4;
				this.cursorInputVelocity *= 0.05f;
				this.cursorInputVelocity *= this.mouseSensitivity + 0.9f;
				this.cursorInputVelocity *= this.rawInputScaling;
				float magnitude = this.cursorInputVelocity.magnitude;
				AnimationCurve animationCurve = this.mobileCurve60;
				if (!this.noAccelCurve)
				{
					float num3 = animationCurve.Evaluate(magnitude);
					this.cursorInputVelocity *= num3;
				}
				this.cursorInputVelocity = Vector2.Lerp(this.prevCursorInputVelocity, this.cursorInputVelocity, 0.95f);
			}
		}
		else
		{
			this.cursorInputVelocity = Vector2.zero;
		}
	}

	// Token: 0x060007F5 RID: 2037 RVA: 0x00044538 File Offset: 0x00042938
	private void FixedUpdate()
	{
		this.fixedFrameCounter += 1L;
		if (!this.mobileMan.Ready)
		{
			return;
		}
		if (this.menuPause)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		if (this.mouseSnap)
		{
			this.mouseSnap = false;
		}
		else
		{
			if (this.pauseInputTimer > 0f)
			{
				this.pauseInputTimer -= Time.fixedDeltaTime * 2f;
				if (this.pauseInputTimer < 0f)
				{
					this.pauseInputTimer = 0f;
				}
				float num3 = Mathf.Clamp01(2f * (1f - this.pauseInputTimer) - 1f);
				this.cursorInputVelocity *= num3;
				this.fakeCursor.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.8f, num3 * 0.2f);
				this.fakeCursorRB.MovePosition(this.tip.position);
				this.mouseVelocityAverage = 0f;
				this.mw = this.tip.position;
			}
			else
			{
				if (this.pauseInputTimer < 0f)
				{
					throw new InvalidOperationException("pauseInputTimer cannot be less than 0");
				}
				this.mw = this.fakeCursorRB.position;
			}
			this.mouseVelocityAverage = Mathf.Lerp(this.mouseVelocityAverage, Mathf.Max(0.1f * this.cursorInputVelocity.magnitude, 0.001f), 0.05f) + 0.005f;
			Vector2 vector = this.cursorInputVelocity;
			vector *= this.mouseVelocityAverage;
			vector *= this.cursorVelocityToWorldUnits;
			Vector2 vector2 = this.fakeCursorRB.position + vector;
			float magnitude = (base.transform.position - vector2).magnitude;
			if (magnitude > 3.5f)
			{
				vector2 = base.transform.position + (vector2 - base.transform.position).normalized * 3.5f;
			}
			Vector2 vector3 = this.tip.position - this.mw;
			Vector2 vector4 = (this.tip.position - this.hj.transform.position).normalized;
			num2 = Vector2.Dot(vector4, vector3.normalized);
			num = vector3.magnitude * num2;
			vector2 += 0.05f * vector3 * 10f * Mathf.Clamp(0.2f - Mathf.Min(this.mouseVelocityAverage, 0.2f), 0f, 0.2f);
			this.fakeCursorRB.MovePosition(vector2);
			this.fakeCursorRB.velocity = vector2 - this.fakeCursorRB.position;
		}
		Vector2 vector5 = this.hj.transform.position - this.mw;
		float num4 = 57.29578f * Mathf.Atan2(vector5.y, vector5.x) - 180f - this.hj.referenceAngle;
		float num5 = -Mathf.DeltaAngle(num4, this.hj.jointAngle);
		num5 = Mathf.Sign(num5) * Mathf.Max(Mathf.Abs(num5 / 2f), Mathf.Abs(num5 * num5));
		num5 *= Mathf.Clamp01(Mathf.Abs(num5) / this.angleEpsilon);
		this.hingeJointMotorValues.motorSpeed = Mathf.Clamp((num5 * 3f + (num5 - this.oldAngle) * 0f) * Mathf.Pow(Mathf.Clamp01(vector5.magnitude / this.deadzone), 2f), -800f, 800f);
		this.hj.motor = this.hingeJointMotorValues;
		float num6 = 16f - Mathf.Max(this.sj.reactionTorque * 0.001f, 5f);
		float num7 = Mathf.Pow(num2, 4f);
		this.slider.motorSpeed = Mathf.Clamp(-num * Mathf.Abs(num) * num6 * num7, -50f, 50f);
		this.sj.motor = this.slider;
		this.oldAngle = num5;
	}

	// Token: 0x060007F6 RID: 2038 RVA: 0x000449C8 File Offset: 0x00042DC8
	public void setHighCurve()
	{
		this.highAccel.SetActive(true);
		this.stdAccel.SetActive(false);
		this.lowAccel.SetActive(false);
		this.noAccel.SetActive(false);
		this.noAccelCurve = false;
		this.mobileCurve.RemoveKey(this.mobileCurve.keys.Length - 1);
		this.mobileCurve.RemoveKey(this.mobileCurve.keys.Length - 1);
		this.mobileCurve.AddKey(new Keyframe(80f, 2.6f));
		this.mobileCurve.AddKey(new Keyframe(100f, 2.6f));
		this.mobileCurve60.RemoveKey(this.mobileCurve60.keys.Length - 1);
		this.mobileCurve60.RemoveKey(this.mobileCurve60.keys.Length - 1);
		this.mobileCurve60.AddKey(new Keyframe(80f, 2.6f));
		this.mobileCurve60.AddKey(new Keyframe(100f, 2.6f));
		PlayerPrefs.SetInt("mobileAccel", 2);
		PlayerPrefs.Save();
	}

	// Token: 0x060007F7 RID: 2039 RVA: 0x00044AF0 File Offset: 0x00042EF0
	public void setStandardCurve()
	{
		this.stdAccel.SetActive(true);
		this.lowAccel.SetActive(false);
		this.highAccel.SetActive(false);
		this.noAccel.SetActive(false);
		this.noAccelCurve = false;
		this.mobileCurve.RemoveKey(this.mobileCurve.keys.Length - 1);
		this.mobileCurve.RemoveKey(this.mobileCurve.keys.Length - 1);
		this.mobileCurve.AddKey(new Keyframe(80f, 2.3f));
		this.mobileCurve.AddKey(new Keyframe(100f, 2.3f));
		this.mobileCurve60.RemoveKey(this.mobileCurve60.keys.Length - 1);
		this.mobileCurve60.RemoveKey(this.mobileCurve60.keys.Length - 1);
		this.mobileCurve60.AddKey(new Keyframe(80f, 2.3f));
		this.mobileCurve60.AddKey(new Keyframe(100f, 2.3f));
		PlayerPrefs.SetInt("mobileAccel", 1);
		PlayerPrefs.Save();
	}

	// Token: 0x060007F8 RID: 2040 RVA: 0x00044C18 File Offset: 0x00043018
	public void setLowCurve()
	{
		this.lowAccel.SetActive(true);
		this.stdAccel.SetActive(false);
		this.highAccel.SetActive(false);
		this.noAccel.SetActive(false);
		this.noAccelCurve = false;
		this.mobileCurve.RemoveKey(this.mobileCurve.keys.Length - 1);
		this.mobileCurve.RemoveKey(this.mobileCurve.keys.Length - 1);
		this.mobileCurve.AddKey(new Keyframe(80f, 2f));
		this.mobileCurve.AddKey(new Keyframe(100f, 2f));
		this.mobileCurve60.RemoveKey(this.mobileCurve60.keys.Length - 1);
		this.mobileCurve60.RemoveKey(this.mobileCurve60.keys.Length - 1);
		this.mobileCurve60.AddKey(new Keyframe(80f, 2f));
		this.mobileCurve60.AddKey(new Keyframe(100f, 2f));
		PlayerPrefs.SetInt("mobileAccel", 0);
		PlayerPrefs.Save();
	}

	// Token: 0x060007F9 RID: 2041 RVA: 0x00044D40 File Offset: 0x00043140
	public void setNoCurve()
	{
		this.noAccel.SetActive(true);
		this.lowAccel.SetActive(false);
		this.stdAccel.SetActive(false);
		this.highAccel.SetActive(false);
		this.noAccelCurve = true;
		PlayerPrefs.SetInt("mobileAccel", 3);
		PlayerPrefs.Save();
	}

	// Token: 0x0400076B RID: 1899
	public HingeJoint2D hj;

	// Token: 0x0400076C RID: 1900
	public SliderJoint2D sj;

	// Token: 0x0400076D RID: 1901
	public Transform tip;

	// Token: 0x0400076E RID: 1902
	private JointMotor2D hingeJointMotorValues;

	// Token: 0x0400076F RID: 1903
	private JointMotor2D slider;

	// Token: 0x04000770 RID: 1904
	public float deadzone;

	// Token: 0x04000771 RID: 1905
	public Transform fakeCursor;

	// Token: 0x04000772 RID: 1906
	public Transform potMesh;

	// Token: 0x04000773 RID: 1907
	private Rigidbody2D fakeCursorRB;

	// Token: 0x04000774 RID: 1908
	private Vector2 cursorInputVelocity;

	// Token: 0x04000775 RID: 1909
	private Vector2 prevCursorInputVelocity;

	// Token: 0x04000776 RID: 1910
	private Vector2 prevMouseInput;

	// Token: 0x04000777 RID: 1911
	private float mouseVelocityAverage;

	// Token: 0x04000778 RID: 1912
	private float oldAngle;

	// Token: 0x04000779 RID: 1913
	[NonSerialized]
	public float mouseSensitivity = 1f;

	// Token: 0x0400077A RID: 1914
	private float angleEpsilon = 1f;

	// Token: 0x0400077B RID: 1915
	private float pauseInputTimer;

	// Token: 0x0400077C RID: 1916
	private bool mouseSnap;

	// Token: 0x0400077D RID: 1917
	public bool trackpad;

	// Token: 0x0400077E RID: 1918
	public AnimationCurve trackpadCurve;

	// Token: 0x0400077F RID: 1919
	public AnimationCurve mouseCurve;

	// Token: 0x04000780 RID: 1920
	public AnimationCurve mobileCurve;

	// Token: 0x04000781 RID: 1921
	public AnimationCurve mobileCurve60;

	// Token: 0x04000782 RID: 1922
	public AnimationCurve stickCurve;

	// Token: 0x04000783 RID: 1923
	private bool input_enabled;

	// Token: 0x04000784 RID: 1924
	public bool loadedFromSave;

	// Token: 0x04000785 RID: 1925
	private PolygonCollider2D hammerCollider;

	// Token: 0x04000786 RID: 1926
	private List<Collider2D> ghostedCols;

	// Token: 0x04000787 RID: 1927
	private PoseControl pose;

	// Token: 0x04000788 RID: 1928
	private ReflectionProbe probe;

	// Token: 0x04000789 RID: 1929
	private float mobileScreenDPIAdjust;

	// Token: 0x0400078A RID: 1930
	private bool skipfirstMoveInput;

	// Token: 0x0400078B RID: 1931
	public float rawInputScaling = 0.75f;

	// Token: 0x0400078C RID: 1932
	public float cursorVelocityToWorldUnits = 1f;

	// Token: 0x0400078D RID: 1933
	private static float dpi;

	// Token: 0x0400078E RID: 1934
	[Header("-------------------------------------------------------------")]
	[Space(4f)]
	public GameObject highAccel;

	// Token: 0x0400078F RID: 1935
	public GameObject stdAccel;

	// Token: 0x04000790 RID: 1936
	public GameObject lowAccel;

	// Token: 0x04000791 RID: 1937
	public GameObject noAccel;

	// Token: 0x04000792 RID: 1938
	private bool noAccelCurve;

	// Token: 0x04000793 RID: 1939
	private MobileManager mobileMan;

	// Token: 0x04000794 RID: 1940
	private int sensitivityModifier;

	// Token: 0x04000795 RID: 1941
	public Rigidbody2D[] AttachedRigidBodies;

	// Token: 0x04000796 RID: 1942
	private static int originalHeight;

	// Token: 0x04000797 RID: 1943
	private static int originalWidth;

	// Token: 0x04000798 RID: 1944
	private static float originalDpi;

	// Token: 0x04000799 RID: 1945
	private bool menuPause;

	// Token: 0x0400079A RID: 1946
	private Vector2 mw = Vector2.zero;

	// Token: 0x0400079B RID: 1947
	private long fixedFrameCounter;
}
