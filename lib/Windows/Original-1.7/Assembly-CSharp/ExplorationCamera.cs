using System;
using UnityEngine;

// Token: 0x0200001B RID: 27
public class ExplorationCamera : MonoBehaviour
{
	// Token: 0x0600010C RID: 268 RVA: 0x0000BC70 File Offset: 0x00009E70
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

	// Token: 0x0600010D RID: 269 RVA: 0x0000BCA8 File Offset: 0x00009EA8
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

	// Token: 0x0600010E RID: 270 RVA: 0x0000BD54 File Offset: 0x00009F54
	private void OnDestroy()
	{
		Cursor.visible = true;
	}

	// Token: 0x0600010F RID: 271 RVA: 0x0000BD5C File Offset: 0x00009F5C
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

	// Token: 0x06000110 RID: 272 RVA: 0x0000C388 File Offset: 0x0000A588
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

	// Token: 0x06000111 RID: 273 RVA: 0x0000C4C0 File Offset: 0x0000A6C0
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

	// Token: 0x040001B7 RID: 439
	[Range(1f, 50f)]
	public float sensitivity = 30f;

	// Token: 0x040001B8 RID: 440
	[Range(0.1f, 50f)]
	public float smoothing = 5f;

	// Token: 0x040001B9 RID: 441
	private float speedZ;

	// Token: 0x040001BA RID: 442
	private float speedX;

	// Token: 0x040001BB RID: 443
	private float speedY;

	// Token: 0x040001BC RID: 444
	[Range(0.1f, 10f)]
	public float Speed = 0.1f;

	// Token: 0x040001BD RID: 445
	[HideInInspector]
	public float InitialSpeed = 0.1f;

	// Token: 0x040001BE RID: 446
	[HideInInspector]
	public float FOV;

	// Token: 0x040001BF RID: 447
	private float initialFOV;

	// Token: 0x040001C0 RID: 448
	private Vector2 initialLook;

	// Token: 0x040001C1 RID: 449
	private Vector2 Look;

	// Token: 0x040001C2 RID: 450
	private Vector3 MovementDirection;

	// Token: 0x040001C3 RID: 451
	public float MaxAcceleration = 4f;

	// Token: 0x040001C4 RID: 452
	[Range(1f, 10f)]
	public float AccelerationSmoothing = 10f;

	// Token: 0x040001C5 RID: 453
	[Range(1f, 10f)]
	public float SpeedSmooth = 1f;

	// Token: 0x040001C6 RID: 454
	private Vector2 _smoothMouse;

	// Token: 0x040001C7 RID: 455
	public bool HideCursor;

	// Token: 0x040001C8 RID: 456
	private float focalLength;

	// Token: 0x040001C9 RID: 457
	private float focalSize = 1f;

	// Token: 0x040001CA RID: 458
	public bool ConstantMove;

	// Token: 0x040001CB RID: 459
	private Camera ThisCamera;

	// Token: 0x040001CC RID: 460
	[HideInInspector]
	public float tilt;

	// Token: 0x040001CD RID: 461
	private Vector2 lastPos;

	// Token: 0x040001CE RID: 462
	public float tiltSmoothing = 50f;

	// Token: 0x040001CF RID: 463
	public float FOVTransitionSpeed = 0.1f;

	// Token: 0x040001D0 RID: 464
	public float inclination = 13f;

	// Token: 0x040001D1 RID: 465
	public bool ApplyGravity;

	// Token: 0x040001D2 RID: 466
	[Range(0f, 1f)]
	public float gravityAccelerationScale = 0.1f;

	// Token: 0x040001D3 RID: 467
	private float TriggerValue;
}
