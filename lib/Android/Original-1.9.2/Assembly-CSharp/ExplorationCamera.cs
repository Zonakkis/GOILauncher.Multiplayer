using System;
using UnityEngine;

// Token: 0x02000024 RID: 36
public class ExplorationCamera : MonoBehaviour
{
	// Token: 0x060000FB RID: 251 RVA: 0x0000B8E0 File Offset: 0x00009CE0
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

	// Token: 0x060000FC RID: 252 RVA: 0x0000B920 File Offset: 0x00009D20
	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseDown)
		{
			this.lastPos = Event.current.mousePosition;
		}
		else if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove)
		{
			Vector3 vector = Event.current.mousePosition - this.lastPos;
			this.Look += new Vector2(vector.x * this.sensitivity / 50f, -vector.y * this.sensitivity / 50f);
			this.lastPos = Event.current.mousePosition;
		}
	}

	// Token: 0x060000FD RID: 253 RVA: 0x0000B9DB File Offset: 0x00009DDB
	private void OnDestroy()
	{
		Cursor.visible = true;
	}

	// Token: 0x060000FE RID: 254 RVA: 0x0000B9E4 File Offset: 0x00009DE4
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
		base.GetComponent<Camera>().fieldOfView = this.FOV;
	}

	// Token: 0x060000FF RID: 255 RVA: 0x0000C048 File Offset: 0x0000A448
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

	// Token: 0x06000100 RID: 256 RVA: 0x0000C1A0 File Offset: 0x0000A5A0
	private void OnEnable()
	{
		this.InitialSpeed = this.Speed;
		if (this.HideCursor)
		{
			Cursor.visible = false;
		}
		this.FOV = base.gameObject.GetComponent<Camera>().fieldOfView;
		this.initialFOV = this.FOV;
		this.initialLook = new Vector2(base.transform.eulerAngles.y, base.transform.eulerAngles.x * -1f);
		this.Look = this.initialLook;
		this._smoothMouse = this.initialLook;
	}

	// Token: 0x040001CE RID: 462
	[Range(1f, 50f)]
	public float sensitivity = 30f;

	// Token: 0x040001CF RID: 463
	[Range(0.1f, 50f)]
	public float smoothing = 5f;

	// Token: 0x040001D0 RID: 464
	private float speedZ;

	// Token: 0x040001D1 RID: 465
	private float speedX;

	// Token: 0x040001D2 RID: 466
	private float speedY;

	// Token: 0x040001D3 RID: 467
	[Range(0.1f, 10f)]
	public float Speed = 0.1f;

	// Token: 0x040001D4 RID: 468
	[HideInInspector]
	public float InitialSpeed = 0.1f;

	// Token: 0x040001D5 RID: 469
	[HideInInspector]
	public float FOV;

	// Token: 0x040001D6 RID: 470
	private float initialFOV;

	// Token: 0x040001D7 RID: 471
	private Vector2 initialLook;

	// Token: 0x040001D8 RID: 472
	private Vector2 Look;

	// Token: 0x040001D9 RID: 473
	private Vector3 MovementDirection;

	// Token: 0x040001DA RID: 474
	public float MaxAcceleration = 4f;

	// Token: 0x040001DB RID: 475
	[Range(1f, 10f)]
	public float AccelerationSmoothing = 10f;

	// Token: 0x040001DC RID: 476
	[Range(1f, 10f)]
	public float SpeedSmooth = 1f;

	// Token: 0x040001DD RID: 477
	private Vector2 _smoothMouse;

	// Token: 0x040001DE RID: 478
	public bool HideCursor;

	// Token: 0x040001DF RID: 479
	private float focalLength;

	// Token: 0x040001E0 RID: 480
	private float focalSize = 1f;

	// Token: 0x040001E1 RID: 481
	public bool ConstantMove;

	// Token: 0x040001E2 RID: 482
	[HideInInspector]
	public float tilt;

	// Token: 0x040001E3 RID: 483
	private Vector2 lastPos;

	// Token: 0x040001E4 RID: 484
	public float tiltSmoothing = 50f;

	// Token: 0x040001E5 RID: 485
	public float FOVTransitionSpeed = 0.1f;

	// Token: 0x040001E6 RID: 486
	public float inclination = 13f;

	// Token: 0x040001E7 RID: 487
	public bool ApplyGravity;

	// Token: 0x040001E8 RID: 488
	[Range(0f, 1f)]
	public float gravityAccelerationScale = 0.1f;

	// Token: 0x040001E9 RID: 489
	private float TriggerValue;
}
