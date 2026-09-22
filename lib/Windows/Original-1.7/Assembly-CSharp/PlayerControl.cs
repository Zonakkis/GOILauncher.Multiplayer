using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x0200003E RID: 62
public class PlayerControl : MonoBehaviour
{
	// Token: 0x060001D2 RID: 466 RVA: 0x00011ACF File Offset: 0x0000FCCF
	private void Awake()
	{
		this.loadedFromSave = false;
		this.loadFinished = false;
	}

	// Token: 0x060001D3 RID: 467 RVA: 0x00011AE0 File Offset: 0x0000FCE0
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

	// Token: 0x060001D4 RID: 468 RVA: 0x00011C55 File Offset: 0x0000FE55
	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			base.StartCoroutine(this.LockCursor());
		}
	}

	// Token: 0x060001D5 RID: 469 RVA: 0x00011C67 File Offset: 0x0000FE67
	private IEnumerator LockCursor()
	{
		yield return new WaitForSeconds(0.1f);
		Cursor.lockState = CursorLockMode.Locked;
		yield break;
	}

	// Token: 0x060001D6 RID: 470 RVA: 0x00011C6F File Offset: 0x0000FE6F
	private void GhostHammer(PolygonCollider2D otherCol)
	{
		this.ghostedCols.Add(otherCol);
		Physics2D.IgnoreCollision(this.hammerCollider, otherCol);
	}

	// Token: 0x060001D7 RID: 471 RVA: 0x00011C89 File Offset: 0x0000FE89
	public void SetSensitivity(float newSensitivity)
	{
		this.mouseSensitivity = newSensitivity;
	}

	// Token: 0x060001D8 RID: 472 RVA: 0x00011C92 File Offset: 0x0000FE92
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

	// Token: 0x060001D9 RID: 473 RVA: 0x00011CC6 File Offset: 0x0000FEC6
	public void StopAnimator()
	{
		if (this.pose == null)
		{
			this.pose = base.GetComponentInChildren<PoseControl>();
		}
		this.pose.StopAnimator();
	}

	// Token: 0x060001DA RID: 474 RVA: 0x00011CED File Offset: 0x0000FEED
	public void StartAnimator()
	{
		if (this.pose == null)
		{
			this.pose = base.GetComponentInChildren<PoseControl>();
		}
		this.pose.StartAnimator();
	}

	// Token: 0x060001DB RID: 475 RVA: 0x00011D14 File Offset: 0x0000FF14
	public void PlayOpeningAnimation()
	{
		base.GetComponentInChildren<PoseControl>().PlayOpeningAnimation();
	}

	// Token: 0x060001DC RID: 476 RVA: 0x00011D24 File Offset: 0x0000FF24
	public void HammerReturn()
	{
		if (this.tip.position.y - 0.4f < base.transform.position.y)
		{
			return;
		}
		this.mouseSnap = true;
		this.fakeCursorRB.MovePosition(this.tip.position);
	}

	// Token: 0x060001DD RID: 477 RVA: 0x00011D7C File Offset: 0x0000FF7C
	public void Pause()
	{
		this.menuPause = true;
	}

	// Token: 0x060001DE RID: 478 RVA: 0x00011D85 File Offset: 0x0000FF85
	public void UnPause()
	{
		this.inputsToSkip = 10;
		this.menuPause = false;
	}

	// Token: 0x060001DF RID: 479 RVA: 0x00011D98 File Offset: 0x0000FF98
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

	// Token: 0x060001E0 RID: 480 RVA: 0x00011E44 File Offset: 0x00010044
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
				this.mouseInput = new Vector2(this.player.GetAxis("mouseX"), this.player.GetAxis("mouseY")) * this.mouseSensitivity;
				this.mouseInput *= this.trackpadCurve.Evaluate(this.mouseInput.magnitude);
				this.mouseInput = Vector2.Lerp(this.oldMouse, this.mouseInput, 0.5f);
			}
			else
			{
				this.oldMouse = this.mouseInput;
				this.mouseInput = new Vector2(this.player.GetAxis("mouseX"), this.player.GetAxis("mouseY")) * this.mouseSensitivity;
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
			if ((base.transform.position - vector4).magnitude > 3.5f)
			{
				vector4 = base.transform.position + (vector4 - base.transform.position).normalized * 3.5f;
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
		float num4 = -Mathf.DeltaAngle(57.29578f * Mathf.Atan2(vector7.y, vector7.x) - 180f - this.hj.referenceAngle, this.hj.jointAngle);
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

	// Token: 0x040002EA RID: 746
	public HingeJoint2D hj;

	// Token: 0x040002EB RID: 747
	public SliderJoint2D sj;

	// Token: 0x040002EC RID: 748
	public Transform tip;

	// Token: 0x040002ED RID: 749
	private Vector2 mw;

	// Token: 0x040002EE RID: 750
	private JointMotor2D motor;

	// Token: 0x040002EF RID: 751
	private JointMotor2D slider;

	// Token: 0x040002F0 RID: 752
	public float deadzone;

	// Token: 0x040002F1 RID: 753
	public Transform fakeCursor;

	// Token: 0x040002F2 RID: 754
	public Transform potMesh;

	// Token: 0x040002F3 RID: 755
	private Rigidbody2D fakeCursorRB;

	// Token: 0x040002F4 RID: 756
	private Vector2 mouseInput;

	// Token: 0x040002F5 RID: 757
	private float mouseVelocityAverage;

	// Token: 0x040002F6 RID: 758
	private float oldAngle;

	// Token: 0x040002F7 RID: 759
	private float mouseSensitivity = 1f;

	// Token: 0x040002F8 RID: 760
	private float posEpsilon = 0.01f;

	// Token: 0x040002F9 RID: 761
	private float angleEpsilon = 1f;

	// Token: 0x040002FA RID: 762
	private float pauseInputTimer;

	// Token: 0x040002FB RID: 763
	private bool mouseSnap;

	// Token: 0x040002FC RID: 764
	public bool trackpad;

	// Token: 0x040002FD RID: 765
	public AnimationCurve trackpadCurve;

	// Token: 0x040002FE RID: 766
	public AnimationCurve mouseCurve;

	// Token: 0x040002FF RID: 767
	public AnimationCurve stickCurve;

	// Token: 0x04000300 RID: 768
	private bool input_enabled;

	// Token: 0x04000301 RID: 769
	public bool loadedFromSave;

	// Token: 0x04000302 RID: 770
	public bool loadFinished;

	// Token: 0x04000303 RID: 771
	private Vector3 oldHammerPos;

	// Token: 0x04000304 RID: 772
	private float[] stuckBuffer;

	// Token: 0x04000305 RID: 773
	private PolygonCollider2D hammerCollider;

	// Token: 0x04000306 RID: 774
	private HammerCollisions hammerCollisions;

	// Token: 0x04000307 RID: 775
	private List<Collider2D> ghostedCols;

	// Token: 0x04000308 RID: 776
	private float stuckTimer;

	// Token: 0x04000309 RID: 777
	private Player player;

	// Token: 0x0400030A RID: 778
	private PoseControl pose;

	// Token: 0x0400030B RID: 779
	private ReflectionProbe probe;

	// Token: 0x0400030C RID: 780
	private Vector2 oldMouse;

	// Token: 0x0400030D RID: 781
	private float mobileScreenDPIAdjust;

	// Token: 0x0400030E RID: 782
	private bool skipfirstMoveInput;

	// Token: 0x0400030F RID: 783
	private int inputsToSkip;

	// Token: 0x04000310 RID: 784
	private int numWins;

	// Token: 0x04000311 RID: 785
	private bool menuPause;
}
