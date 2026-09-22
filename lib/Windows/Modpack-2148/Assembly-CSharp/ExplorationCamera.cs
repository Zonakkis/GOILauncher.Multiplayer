using System;
using UnityEngine;

// Token: 0x0200002E RID: 46
public class ExplorationCamera : MonoBehaviour
{
	// Token: 0x0600012E RID: 302 RVA: 0x0001D370 File Offset: 0x0001B570
	private void Start()
	{
		try
		{
			Input.GetAxis("LookHorizontal");
		}
		catch
		{
			MonoBehaviour.print("Import the custom input to support gamepad in this camera script\n http://davidmiranda.me/files/FogVolume3/InputManager.asset");
		}
	}

	// Token: 0x0600012F RID: 303 RVA: 0x0001D3A8 File Offset: 0x0001B5A8
	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseDown)
		{
			this.lastPos = Event.current.mousePosition;
			return;
		}
		if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove)
		{
			Vector3 vector = Event.current.mousePosition - this.lastPos;
			this.Look += new Vector2(vector.x * this.sensitivity / 50f, -vector.y * this.sensitivity / 50f);
			this.lastPos = Event.current.mousePosition;
		}
	}

	// Token: 0x06000130 RID: 304 RVA: 0x00003173 File Offset: 0x00001373
	private void OnDestroy()
	{
		Cursor.visible = true;
	}

	// Token: 0x06000131 RID: 305 RVA: 0x0001D454 File Offset: 0x0001B654
	private void FixedUpdate()
	{
		try
		{
			if (Mathf.Abs(Input.GetAxis("LookHorizontal")) > 0.017f || Mathf.Abs(Input.GetAxis("LookVertical")) > 0.017f)
			{
				this.Look += new Vector2(Input.GetAxis("LookHorizontal"), -Input.GetAxis("LookVertical")) * this.sensitivity;
				if (Input.GetAxis("LookHorizontal") > 0f)
				{
					this.tilt = Mathf.Lerp(this.tilt, -this.inclination * Mathf.Abs(Input.GetAxis("LookHorizontal")) * this.sensitivity, 1f / this.tiltSmoothing);
				}
				else
				{
					this.tilt = Mathf.Lerp(this.tilt, this.inclination * Mathf.Abs(Input.GetAxis("LookHorizontal")) * this.sensitivity, 1f / this.tiltSmoothing);
				}
			}
		}
		catch
		{
		}
		this.tilt = Mathf.Lerp(this.tilt, 0f, 1f / this.tiltSmoothing);
		if (Input.GetKey(KeyCode.PageDown))
		{
			this.focalLength -= 0.05f;
		}
		if (Input.GetKey(KeyCode.PageUp))
		{
			this.focalLength += 0.05f;
		}
		if (Input.GetKey(KeyCode.End))
		{
			this.focalSize -= 0.05f;
		}
		if (Input.GetKey(KeyCode.Home))
		{
			this.focalSize += 0.05f;
		}
		this._smoothMouse.x = Mathf.Lerp(this._smoothMouse.x, this.Look.x, 1f / this.smoothing);
		this._smoothMouse.y = Mathf.Lerp(this._smoothMouse.y, this.Look.y, 1f / this.smoothing);
		base.transform.localEulerAngles = new Vector3(-this._smoothMouse.y, this._smoothMouse.x, this.tilt);
		if (Input.GetKey(KeyCode.W) || Input.GetAxis("Vertical") > 0f || this.ConstantMove)
		{
			this.MovementDirection = new Vector3(0f, 0f, 1f);
			this.speedZ = Mathf.Lerp(this.speedZ, this.Speed, 1f / this.smoothing);
		}
		if (Input.GetKey(KeyCode.S) || Input.GetAxis("Vertical") < 0f)
		{
			this.MovementDirection = new Vector3(0f, 0f, -1f);
			this.speedZ = Mathf.Lerp(this.speedZ, -this.Speed, 1f / this.smoothing);
		}
		if (Input.GetKey(KeyCode.A) || Input.GetAxis("Horizontal") < 0f)
		{
			this.MovementDirection = new Vector3(-1f, 0f, 0f);
			this.speedX = Mathf.Lerp(this.speedX, -this.Speed, 1f / this.smoothing);
		}
		if (Input.GetKey(KeyCode.D) || Input.GetAxis("Horizontal") > 0f)
		{
			this.MovementDirection = new Vector3(1f, 0f, 0f);
			this.speedX = Mathf.Lerp(this.speedX, this.Speed, 1f / this.smoothing);
		}
		if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.JoystickButton4))
		{
			this.MovementDirection = new Vector3(0f, -1f, 0f);
			this.speedY = Mathf.Lerp(this.speedY, -this.Speed, 1f / this.smoothing);
		}
		if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton5))
		{
			this.MovementDirection = new Vector3(0f, 1f, 0f);
			this.speedY = Mathf.Lerp(this.speedY, this.Speed, 1f / this.smoothing);
		}
		this.speedZ = Mathf.Lerp(this.speedZ, 0f, 1f / this.smoothing);
		this.MovementDirection.z = this.speedZ;
		this.speedY = Mathf.Lerp(this.speedY, 0f, 1f / this.smoothing);
		this.MovementDirection.y = this.speedY;
		this.speedX = Mathf.Lerp(this.speedX, 0f, 1f / this.smoothing);
		this.MovementDirection = new Vector3(this.speedX, this.speedY, this.speedZ);
		if (this.ApplyGravity)
		{
			this.MovementDirection += Physics.gravity * this.gravityAccelerationScale;
		}
		base.transform.Translate(this.MovementDirection);
		this.Speed = Mathf.Clamp(this.Speed, 0.001f, 100f);
		if (Input.GetMouseButton(1))
		{
			this.InitialSpeed += Input.GetAxis("Mouse ScrollWheel") * 0.5f;
			this.InitialSpeed = Mathf.Max(0.1f, this.InitialSpeed);
		}
		if (Input.GetMouseButton(1) & Input.GetKey(KeyCode.C) & (this.FOV > 5f))
		{
			this.FOV -= 1f;
		}
		if (Input.GetMouseButton(1) & Input.GetKey(KeyCode.Z) & (this.FOV < 120f))
		{
			this.FOV += 1f;
		}
		if (!Input.GetMouseButton(1))
		{
			this.FOV = Mathf.Lerp(this.FOV, this.initialFOV, this.FOVTransitionSpeed);
		}
		if (this.ThisCamera && !this.ThisCamera.stereoEnabled)
		{
			base.GetComponent<Camera>().fieldOfView = this.FOV;
		}
	}

	// Token: 0x06000132 RID: 306 RVA: 0x0001DA80 File Offset: 0x0001BC80
	private void Update()
	{
		try
		{
			this.TriggerValue = Mathf.Pow(Input.GetAxis("FasterCamera") * 1f / this.AccelerationSmoothing + 1f, 2f);
		}
		catch
		{
		}
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			this.Speed *= this.MaxAcceleration;
		}
		try
		{
			if (Input.GetAxis("FasterCamera") > 0f && this.Speed < this.InitialSpeed * this.MaxAcceleration)
			{
				this.Speed *= this.TriggerValue;
			}
			else if (!Input.GetKey(KeyCode.LeftShift))
			{
				this.Speed = Mathf.Lerp(this.Speed, this.InitialSpeed, 1f / this.AccelerationSmoothing);
			}
		}
		catch
		{
			if (!Input.GetKey(KeyCode.LeftShift))
			{
				this.Speed = Mathf.Lerp(this.Speed, this.InitialSpeed, 1f / this.AccelerationSmoothing);
			}
		}
		if (Input.GetKeyUp(KeyCode.LeftShift))
		{
			this.Speed /= this.MaxAcceleration;
		}
	}

	// Token: 0x06000133 RID: 307 RVA: 0x0001DBB8 File Offset: 0x0001BDB8
	private void OnEnable()
	{
		if (base.GetComponent<Camera>())
		{
			this.ThisCamera = base.GetComponent<Camera>();
		}
		this.InitialSpeed = this.Speed;
		if (this.HideCursor)
		{
			Cursor.visible = false;
		}
		if (this.ThisCamera)
		{
			this.FOV = base.gameObject.GetComponent<Camera>().fieldOfView;
		}
		this.initialFOV = this.FOV;
		this.initialLook = new Vector2(base.transform.eulerAngles.y, base.transform.eulerAngles.x * -1f);
		this.Look = this.initialLook;
		this._smoothMouse = this.initialLook;
	}

	// Token: 0x04000204 RID: 516
	[Range(1f, 50f)]
	public float sensitivity = 30f;

	// Token: 0x04000205 RID: 517
	[Range(0.1f, 50f)]
	public float smoothing = 5f;

	// Token: 0x04000206 RID: 518
	private float speedZ;

	// Token: 0x04000207 RID: 519
	private float speedX;

	// Token: 0x04000208 RID: 520
	private float speedY;

	// Token: 0x04000209 RID: 521
	[Range(0.1f, 10f)]
	public float Speed = 0.1f;

	// Token: 0x0400020A RID: 522
	[HideInInspector]
	public float InitialSpeed = 0.1f;

	// Token: 0x0400020B RID: 523
	[HideInInspector]
	public float FOV;

	// Token: 0x0400020C RID: 524
	private float initialFOV;

	// Token: 0x0400020D RID: 525
	private Vector2 initialLook;

	// Token: 0x0400020E RID: 526
	private Vector2 Look;

	// Token: 0x0400020F RID: 527
	private Vector3 MovementDirection;

	// Token: 0x04000210 RID: 528
	public float MaxAcceleration = 4f;

	// Token: 0x04000211 RID: 529
	[Range(1f, 10f)]
	public float AccelerationSmoothing = 10f;

	// Token: 0x04000212 RID: 530
	[Range(1f, 10f)]
	public float SpeedSmooth = 1f;

	// Token: 0x04000213 RID: 531
	private Vector2 _smoothMouse;

	// Token: 0x04000214 RID: 532
	public bool HideCursor;

	// Token: 0x04000215 RID: 533
	private float focalLength;

	// Token: 0x04000216 RID: 534
	private float focalSize = 1f;

	// Token: 0x04000217 RID: 535
	public bool ConstantMove;

	// Token: 0x04000218 RID: 536
	private Camera ThisCamera;

	// Token: 0x04000219 RID: 537
	[HideInInspector]
	public float tilt;

	// Token: 0x0400021A RID: 538
	private Vector2 lastPos;

	// Token: 0x0400021B RID: 539
	public float tiltSmoothing = 50f;

	// Token: 0x0400021C RID: 540
	public float FOVTransitionSpeed = 0.1f;

	// Token: 0x0400021D RID: 541
	public float inclination = 13f;

	// Token: 0x0400021E RID: 542
	public bool ApplyGravity;

	// Token: 0x0400021F RID: 543
	[Range(0f, 1f)]
	public float gravityAccelerationScale = 0.1f;

	// Token: 0x04000220 RID: 544
	private float TriggerValue;
}
