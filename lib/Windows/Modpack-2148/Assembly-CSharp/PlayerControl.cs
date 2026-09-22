using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x02000062 RID: 98
public class PlayerControl : MonoBehaviour
{
	// Token: 0x06000248 RID: 584 RVA: 0x00003C07 File Offset: 0x00001E07
	private void Awake()
	{
		this.loadedFromSave = false;
		this.loadFinished = false;
	}

	// Token: 0x06000249 RID: 585 RVA: 0x000238C8 File Offset: 0x00021AC8
	private void Start()
	{
		this.inputsToSkip = 0;
		this.numWins = PlayerPrefs.GetInt("NumWins", 0);
		this.oldHammerPos = this.tip.position;
		this.stuckBuffer = new float[4];
		this.ghostedCols = new List<Collider2D>();
		this.stuckTimer = 0f;
		this.hammerCollider = this.tip.GetComponent<PolygonCollider2D>();
		this.hammerCollisions = this.tip.GetComponent<HammerCollisions>();
		this.mouseInput = Vector2.zero;
		this.motor = this.hj.motor;
		this.slider = this.sj.motor;
		this.motor.motorSpeed = 0f;
		this.slider.motorSpeed = 0f;
		try
		{
			this.cursorRange = (float)SettingsManager.cursorRange;
		}
		catch
		{
			this.cursorRange = 3.5f;
		}
		this.hj.motor = this.motor;
		this.sj.motor = this.slider;
		this.oldAngle = 0f;
		this.fakeCursorRB = this.fakeCursor.GetComponent<Rigidbody2D>();
		this.mouseVelocityAverage = 0f;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		this.pauseInputTimer = 2f;
		Debug.Log("enabling input");
		this.mouseSnap = false;
		if (PlayerPrefs.HasKey("Trackpad"))
		{
			this.trackpad = PlayerPrefs.GetInt("Trackpad") == 1;
		}
		else
		{
			this.trackpad = true;
		}
		this.player = ReInput.players.GetPlayer(0);
	}

	// Token: 0x0600024A RID: 586 RVA: 0x00003C17 File Offset: 0x00001E17
	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			base.StartCoroutine(this.LockCursor());
		}
	}

	// Token: 0x0600024B RID: 587 RVA: 0x00003C29 File Offset: 0x00001E29
	private IEnumerator LockCursor()
	{
		yield return new WaitForSeconds(0.1f);
		Cursor.lockState = CursorLockMode.Locked;
		yield break;
	}

	// Token: 0x0600024C RID: 588 RVA: 0x00003C31 File Offset: 0x00001E31
	private void GhostHammer(PolygonCollider2D otherCol)
	{
		this.ghostedCols.Add(otherCol);
		Physics2D.IgnoreCollision(this.hammerCollider, otherCol);
	}

	// Token: 0x0600024D RID: 589 RVA: 0x00003C4B File Offset: 0x00001E4B
	public void SetSensitivity(float newSensitivity)
	{
		this.mouseSensitivity = newSensitivity;
	}

	// Token: 0x0600024E RID: 590 RVA: 0x00003C54 File Offset: 0x00001E54
	public void PauseInput(float timeToPause)
	{
		this.pauseInputTimer = timeToPause * 2f;
		if (timeToPause < 0f)
		{
			Debug.Log("disabling input");
			return;
		}
		if (timeToPause == 0f)
		{
			Debug.Log("enabling input");
		}
	}

	// Token: 0x0600024F RID: 591 RVA: 0x00023A68 File Offset: 0x00021C68
	public void StopAnimator()
	{
		try
		{
			if (this.pose == null)
			{
				this.pose = base.GetComponentInChildren<PoseControl>();
			}
			this.pose.StopAnimator();
		}
		catch
		{
		}
	}

	// Token: 0x06000250 RID: 592 RVA: 0x00023AB0 File Offset: 0x00021CB0
	public void StartAnimator()
	{
		try
		{
			if (this.pose == null)
			{
				this.pose = base.GetComponentInChildren<PoseControl>();
			}
			this.pose.StartAnimator();
		}
		catch
		{
		}
	}

	// Token: 0x06000251 RID: 593 RVA: 0x00023AF8 File Offset: 0x00021CF8
	public void PlayOpeningAnimation()
	{
		try
		{
			base.GetComponentInChildren<PoseControl>().PlayOpeningAnimation();
		}
		catch
		{
		}
		finally
		{
			if (SettingsManager.fastStart)
			{
				this.PauseInput(SettingsManager.fastStartTime);
			}
			else
			{
				this.PauseInput(6f);
			}
		}
	}

	// Token: 0x06000252 RID: 594 RVA: 0x00023B54 File Offset: 0x00021D54
	public void HammerReturn()
	{
		if (this.tip.position.y - 0.4f < base.transform.position.y)
		{
			return;
		}
		this.mouseSnap = true;
		this.fakeCursorRB.MovePosition(this.tip.position);
	}

	// Token: 0x06000253 RID: 595 RVA: 0x00003C88 File Offset: 0x00001E88
	public void Pause()
	{
		this.menuPause = true;
	}

	// Token: 0x06000254 RID: 596 RVA: 0x00003C91 File Offset: 0x00001E91
	public void UnPause()
	{
		this.inputsToSkip = 10;
		this.menuPause = false;
	}

	// Token: 0x06000255 RID: 597 RVA: 0x00023BAC File Offset: 0x00021DAC
	private void Update()
	{
		if (this.numWins > 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.R))
		{
			PlayerPrefs.DeleteKey("NumSaves");
			PlayerPrefs.DeleteKey("SaveGame0");
			PlayerPrefs.DeleteKey("SaveGame1");
			PlayerPrefs.Save();
			SceneManager.LoadScene("Mian");
		}
		this.fakeCursor.position = new Vector3(this.fakeCursor.position.x, this.fakeCursor.position.y, this.tip.position.z - 1f);
	}

	// Token: 0x06000256 RID: 598 RVA: 0x00023C58 File Offset: 0x00021E58
	private void FixedUpdate()
	{
		float num = 0f;
		float num2 = 0f;
		if (!this.loadFinished)
		{
			return;
		}
		if (this.mouseSnap)
		{
			this.mouseSnap = false;
		}
		else
		{
			if (this.trackpad)
			{
				this.oldMouse = this.mouseInput;
				this.mouseInput = new Vector2(this.player.GetAxis("mouseX") * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) - this.player.GetAxis("mouseY") * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f), this.player.GetAxis("mouseX") * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + this.player.GetAxis("mouseY") * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f)) * this.mouseSensitivity;
				this.mouseInput *= this.trackpadCurve.Evaluate(this.mouseInput.magnitude);
				this.mouseInput = Vector2.Lerp(this.oldMouse, this.mouseInput, 0.5f);
			}
			else
			{
				this.oldMouse = this.mouseInput;
				this.mouseInput = new Vector2(this.player.GetAxis("mouseX") * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) - this.player.GetAxis("mouseY") * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f), this.player.GetAxis("mouseX") * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + this.player.GetAxis("mouseY") * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f)) * this.mouseSensitivity;
				this.mouseInput *= this.mouseCurve.Evaluate(this.mouseInput.magnitude);
				this.mouseInput = Vector2.Lerp(this.oldMouse, this.mouseInput, 0.5f);
			}
			Vector2 vector = new Vector2(this.player.GetAxis("StickLX"), this.player.GetAxis("StickLY"));
			Vector2 vector2 = new Vector2(this.player.GetAxis("StickRX"), this.player.GetAxis("StickRY"));
			Vector2 vector3 = Vector2.zero;
			if (vector.sqrMagnitude > 0.01f || vector2.sqrMagnitude > 0.01f)
			{
				vector3 = (vector + vector2) * this.stickCurve.Evaluate(vector.magnitude + vector2.magnitude);
			}
			if (vector3.sqrMagnitude > this.mouseInput.sqrMagnitude)
			{
				this.mouseInput.x = vector3.x;
				this.mouseInput.y = vector3.y;
			}
			if (this.pauseInputTimer > 0f)
			{
				this.pauseInputTimer -= Time.fixedDeltaTime * 2f;
				if (this.pauseInputTimer < 0f)
				{
					this.pauseInputTimer = 0f;
				}
				float num3 = Mathf.Clamp01(2f * (1f - this.pauseInputTimer) - 1f);
				this.oldMouse *= num3;
				this.mouseInput *= num3;
				this.fakeCursor.GetComponent<SpriteRenderer>().color = new Color(1f, 0.9f, 0.8f, num3 * 0.2f);
				this.fakeCursorRB.MovePosition(this.tip.position);
				this.mouseVelocityAverage = 0f;
				this.mw = this.tip.position;
			}
			else if (this.pauseInputTimer < 0f)
			{
				this.mouseInput *= 0f;
				this.fakeCursorRB.MovePosition(this.tip.position);
				this.mouseVelocityAverage = 0f;
				this.mw = this.tip.position;
			}
			else if (this.inputsToSkip > 0 && this.mouseInput.sqrMagnitude > 0f)
			{
				this.mouseInput *= 0f;
				this.oldMouse = Vector2.zero;
				this.fakeCursorRB.MovePosition(this.tip.position);
				this.mouseVelocityAverage = 0f;
				this.mw = this.tip.position;
				this.inputsToSkip--;
			}
			else
			{
				this.mw = this.fakeCursorRB.position;
			}
			this.mouseVelocityAverage = Mathf.Lerp(this.mouseVelocityAverage, Mathf.Max(0.1f * this.mouseInput.magnitude, 0.001f), 0.05f) + 0.005f;
			Vector2 vector4 = this.fakeCursorRB.position + this.mouseInput * this.mouseVelocityAverage;
			if ((base.transform.position - vector4).magnitude > this.cursorRange)
			{
				vector4 = base.transform.position + (vector4 - base.transform.position).normalized * this.cursorRange;
			}
			Vector2 vector5 = this.tip.position - this.mw;
			Vector2 vector6 = (this.tip.position - this.hj.transform.position).normalized;
			new Vector2(-vector6.y, vector6.x);
			num2 = Vector2.Dot(vector6, vector5.normalized);
			num = vector5.magnitude * num2;
			this.oldHammerPos = this.tip.position;
			vector4 += 0.05f * vector5 * 10f * Mathf.Clamp(0.2f - Mathf.Min(this.mouseVelocityAverage, 0.2f), 0f, 0.2f);
			this.fakeCursorRB.MovePosition(vector4);
			this.fakeCursorRB.velocity = vector4 - this.fakeCursorRB.position;
		}
		Vector2 vector7 = this.hj.transform.position - this.mw;
		float num4 = -Mathf.DeltaAngle(57.29578f * Mathf.Atan2(-vector7.x * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f) + vector7.y * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f), vector7.x * Mathf.Cos(SettingsManager.sideWayDegrees * 0.017453292f) + vector7.y * Mathf.Sin(SettingsManager.sideWayDegrees * 0.017453292f)) - 180f - this.hj.referenceAngle, this.hj.jointAngle);
		num4 = Mathf.Sign(num4) * Mathf.Max(Mathf.Abs(num4 / 2f), Mathf.Abs(num4 * num4));
		num4 *= Mathf.Clamp01(Mathf.Abs(num4) / this.angleEpsilon);
		this.motor.motorSpeed = Mathf.Clamp((num4 * 3f + (num4 - this.oldAngle) * 0f) * Mathf.Pow(Mathf.Clamp01(vector7.magnitude / this.deadzone), 2f), -800f, 800f);
		this.hj.motor = this.motor;
		float num5 = 16f - Mathf.Max(this.sj.reactionTorque * 0.001f, 5f);
		float num6 = Mathf.Pow(num2, 4f);
		this.slider.motorSpeed = Mathf.Clamp(-num * Mathf.Abs(num) * num5 * num6, -50f, 50f);
		this.sj.motor = this.slider;
		this.oldAngle = num4;
	}

	// Token: 0x17000051 RID: 81
	// (get) Token: 0x06000258 RID: 600 RVA: 0x00003CCB File Offset: 0x00001ECB
	// (set) Token: 0x06000259 RID: 601 RVA: 0x00003CD3 File Offset: 0x00001ED3
	public float cursorRange { get; set; }

	// Token: 0x04000388 RID: 904
	public HingeJoint2D hj;

	// Token: 0x04000389 RID: 905
	public SliderJoint2D sj;

	// Token: 0x0400038A RID: 906
	public Transform tip;

	// Token: 0x0400038B RID: 907
	private Vector2 mw;

	// Token: 0x0400038C RID: 908
	private JointMotor2D motor;

	// Token: 0x0400038D RID: 909
	private JointMotor2D slider;

	// Token: 0x0400038E RID: 910
	public float deadzone;

	// Token: 0x0400038F RID: 911
	public Transform fakeCursor;

	// Token: 0x04000390 RID: 912
	public Transform potMesh;

	// Token: 0x04000391 RID: 913
	private Rigidbody2D fakeCursorRB;

	// Token: 0x04000392 RID: 914
	private Vector2 mouseInput;

	// Token: 0x04000393 RID: 915
	private float mouseVelocityAverage;

	// Token: 0x04000394 RID: 916
	private float oldAngle;

	// Token: 0x04000395 RID: 917
	private float mouseSensitivity = 1f;

	// Token: 0x04000396 RID: 918
	private float posEpsilon = 0.01f;

	// Token: 0x04000397 RID: 919
	private float angleEpsilon = 1f;

	// Token: 0x04000398 RID: 920
	private float pauseInputTimer;

	// Token: 0x04000399 RID: 921
	private bool mouseSnap;

	// Token: 0x0400039A RID: 922
	public bool trackpad;

	// Token: 0x0400039B RID: 923
	public AnimationCurve trackpadCurve;

	// Token: 0x0400039C RID: 924
	public AnimationCurve mouseCurve;

	// Token: 0x0400039D RID: 925
	public AnimationCurve stickCurve;

	// Token: 0x0400039E RID: 926
	private bool input_enabled;

	// Token: 0x0400039F RID: 927
	public bool loadedFromSave;

	// Token: 0x040003A0 RID: 928
	public bool loadFinished;

	// Token: 0x040003A1 RID: 929
	private Vector3 oldHammerPos;

	// Token: 0x040003A2 RID: 930
	private float[] stuckBuffer;

	// Token: 0x040003A3 RID: 931
	private PolygonCollider2D hammerCollider;

	// Token: 0x040003A4 RID: 932
	private HammerCollisions hammerCollisions;

	// Token: 0x040003A5 RID: 933
	private List<Collider2D> ghostedCols;

	// Token: 0x040003A6 RID: 934
	private float stuckTimer;

	// Token: 0x040003A7 RID: 935
	private Player player;

	// Token: 0x040003A8 RID: 936
	private PoseControl pose;

	// Token: 0x040003A9 RID: 937
	private ReflectionProbe probe;

	// Token: 0x040003AA RID: 938
	private Vector2 oldMouse;

	// Token: 0x040003AB RID: 939
	private float mobileScreenDPIAdjust;

	// Token: 0x040003AC RID: 940
	private bool skipfirstMoveInput;

	// Token: 0x040003AD RID: 941
	private int inputsToSkip;

	// Token: 0x040003AE RID: 942
	private bool menuPause;

	// Token: 0x040003B0 RID: 944
	private int numWins;
}
