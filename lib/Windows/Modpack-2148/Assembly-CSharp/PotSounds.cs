using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000067 RID: 103
public class PotSounds : MonoBehaviour
{
	// Token: 0x0600027E RID: 638 RVA: 0x00025C2C File Offset: 0x00023E2C
	private void Start()
	{
		this.contacts = new ContactPoint2D[20];
		this.sparkParams = default(ParticleSystem.EmitParams);
		this.debrisParams = default(ParticleSystem.EmitParams);
		this.collisionPoints = new Dictionary<Collider2D, ContactPoint2D[]>();
		this.waterModule = this.water.shape;
		this.waterEmission = this.water.emission;
		this.audioSource = base.GetComponent<AudioSource>();
		this.lastContactTimer = 0f;
		this.lastClunkTimer = 0f;
		this.lastSlideTimer = 0f;
		this.lastRollTimer = 0f;
		this.lastSplashTimer = 0f;
		this.slide = true;
	}

	// Token: 0x0600027F RID: 639 RVA: 0x00025CD8 File Offset: 0x00023ED8
	private void LateUpdate()
	{
		this.lastContactTimer += Time.deltaTime;
		this.lastClunkTimer += Time.deltaTime;
		this.lastSlideTimer += Time.deltaTime;
		this.lastRollTimer += Time.deltaTime;
		this.lastSplashTimer += Time.deltaTime;
	}

	// Token: 0x06000280 RID: 640 RVA: 0x00003D50 File Offset: 0x00001F50
	private void FixedUpdate()
	{
		if (this.slide)
		{
			this.bottomCol.sharedMaterial = this.slidingFriction;
			return;
		}
		this.bottomCol.sharedMaterial = this.staticFriction;
	}

	// Token: 0x06000281 RID: 641 RVA: 0x00025D40 File Offset: 0x00023F40
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.collisionPoints == null)
		{
			return;
		}
		int num = coll.GetContacts(this.contacts);
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			if (this.collisionPoints.Count > 0 && Mathf.Abs(this.potRB.angularVelocity) > this.rollThreshold)
			{
				this.PlayRoll(Mathf.Clamp01(Mathf.Abs(this.potRB.angularVelocity) / 8f * this.rollThreshold));
			}
			if (!this.collisionPoints.ContainsKey(coll.collider))
			{
				if (this.collisionPoints.Count != 0)
				{
					return;
				}
				this.collisionPoints[coll.collider] = this.contacts;
				if (this.collisionPoints.Count > 1 && Mathf.Abs(this.potRB.angularVelocity) > this.rollThreshold)
				{
					this.PlayRoll(Mathf.Clamp01(Mathf.Abs(this.potRB.angularVelocity) / 8f * this.rollThreshold));
					this.Splash(coll.relativeVelocity.magnitude);
					return;
				}
				GroundCol component = coll.collider.GetComponent<GroundCol>();
				bool flag = false;
				if (component != null && (component.material == GroundCol.SoundMaterial.rock || component.material == GroundCol.SoundMaterial.metal || component.material == GroundCol.SoundMaterial.solidmetal))
				{
					flag = true;
				}
				float num2 = 0f;
				for (int i = 0; i < num; i++)
				{
					num2 += this.contacts[i].normalImpulse;
				}
				if (num2 > this.hardClunkThreshold * 0.3f)
				{
					if (num2 < this.hardClunkThreshold)
					{
						for (int j = 0; j < num; j++)
						{
							this.debrisParams.position = this.contacts[j].point;
							if (component != null)
							{
								this.debrisParams.startColor = component.groundCol;
							}
							Vector3 vector = 2f * new Vector3(this.contacts[j].normal.y, this.contacts[j].normal.x) * Vector2.Dot(new Vector2(this.contacts[j].normal.y, this.contacts[j].normal.x), coll.relativeVelocity);
							this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
							this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
							this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
						}
						this.PlayClunk((num2 - this.hardClunkThreshold * 0.3f) / this.hardClunkThreshold * 0.7f);
					}
					else if (flag)
					{
						for (int k = 0; k < num; k++)
						{
							this.sparkParams.position = this.contacts[k].point;
							Vector3 vector2 = 2f * new Vector3(this.contacts[k].normal.y, this.contacts[k].normal.x) * Vector2.Dot(new Vector2(this.contacts[k].normal.y, this.contacts[k].normal.x), coll.relativeVelocity);
							this.sparkParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
							this.sparkParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
							this.sparkParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
						}
						this.PlayHardClunk(num2 / (2f * this.hardClunkThreshold));
					}
					this.Splash(coll.relativeVelocity.magnitude);
				}
				this.lastContactTimer = 0f;
			}
		}
	}

	// Token: 0x06000282 RID: 642 RVA: 0x0002620C File Offset: 0x0002440C
	private void Splash(float mag)
	{
		this.water.transform.localEulerAngles = new Vector3(-90f, 0f, global::UnityEngine.Random.Range(-180f, 180f));
		this.waterModule.arcSpeed = ((global::UnityEngine.Random.value < 0.5f) ? (-2f) : 2f);
		this.waterEmission.rateOverTime = mag * 10f;
		this.water.Play();
		if (mag > 10f)
		{
			this.PlaySplash(Mathf.Clamp(mag / 18f - 0.2f, 0f, 0.8f));
		}
	}

	// Token: 0x06000283 RID: 643 RVA: 0x000262BC File Offset: 0x000244BC
	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.collisionPoints == null)
		{
			return;
		}
		int num = 0;
		Color[] array = new Color[]
		{
			Color.red,
			Color.green,
			Color.blue,
			Color.cyan,
			Color.yellow,
			Color.magenta
		};
		foreach (KeyValuePair<Collider2D, ContactPoint2D[]> keyValuePair in this.collisionPoints)
		{
			foreach (ContactPoint2D contactPoint2D in keyValuePair.Value)
			{
				Gizmos.color = array[num];
				Gizmos.DrawSphere(contactPoint2D.point + new Vector3(0f, 0f, -2f), 0.1f);
				num = (num + 1) % array.Length;
			}
		}
		if (this.slide)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(new Vector3(this.potRB.position.x, this.potRB.position.y, -2f), 0.5f);
			return;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(new Vector3(this.potRB.position.x, this.potRB.position.y, -2f), 0.5f);
	}

	// Token: 0x06000284 RID: 644 RVA: 0x0002645C File Offset: 0x0002465C
	private void OnCollisionStay2D(Collision2D coll)
	{
		if (this.collisionPoints == null)
		{
			return;
		}
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			int num = coll.GetContacts(this.contacts);
			if (this.collisionPoints.ContainsKey(coll.collider))
			{
				bool flag = true;
				float num2 = 0.3f;
				float num3 = 0f;
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < this.collisionPoints[coll.collider].Length; j++)
					{
						float magnitude = (this.collisionPoints[coll.collider][j].point - this.contacts[i].point).magnitude;
						num2 = Mathf.Min(num2, magnitude);
						num3 = Mathf.Max((1f + Mathf.Abs(this.contacts[i].tangentImpulse)) / (1f + Mathf.Abs(this.contacts[i].normalImpulse)), num3);
						if (magnitude < this.moveThreshold)
						{
							flag = false;
							break;
						}
					}
				}
				if (num3 > 6f && this.potRB.velocity.magnitude > 2f)
				{
					flag = true;
				}
				if (flag && coll.rigidbody == null)
				{
					this.collisionPoints[coll.collider] = this.contacts;
					GroundCol component = coll.collider.GetComponent<GroundCol>();
					if (component != null)
					{
						this.debrisParams.startColor = component.groundCol;
					}
					for (int k = 0; k < num; k++)
					{
						this.debrisParams.position = this.contacts[k].point;
						Vector3 vector = 0.4f * new Vector3(this.contacts[k].normal.y, -this.contacts[k].normal.x) * Vector2.Dot(new Vector2(this.contacts[k].normal.y, -this.contacts[k].normal.x), coll.relativeVelocity);
						this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
						this.debris.Emit(this.debrisParams, (int)Mathf.Max(num2 * 50f - 1f, 0f));
						this.PlayScrape(1f);
					}
				}
				this.slide = flag;
			}
		}
	}

	// Token: 0x06000285 RID: 645 RVA: 0x0002672C File Offset: 0x0002492C
	private void OnCollisionExit2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain") && this.collisionPoints.ContainsKey(coll.collider))
		{
			this.collisionPoints.Remove(coll.collider);
		}
		if (this.collisionPoints.Keys == null)
		{
			this.slide = true;
		}
		if (this.collisionPoints.Keys.Count == 0)
		{
			this.slide = true;
		}
	}

	// Token: 0x06000286 RID: 646 RVA: 0x000267A4 File Offset: 0x000249A4
	private void PlaySplash(float volume = 1f)
	{
		if (this.lastSplashTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastSplashTimer = 0f;
		this.audioSource.PlayOneShot(this.splashes[global::UnityEngine.Random.Range(0, this.splashes.Length)], volume);
	}

	// Token: 0x06000287 RID: 647 RVA: 0x000267FC File Offset: 0x000249FC
	private void PlayClunk(float volume = 1f)
	{
		if (this.lastClunkTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastClunkTimer = 0f;
		this.audioSource.PlayOneShot(this.clunks[global::UnityEngine.Random.Range(0, this.clunks.Length)], volume);
	}

	// Token: 0x06000288 RID: 648 RVA: 0x00026854 File Offset: 0x00024A54
	private void PlayHardClunk(float volume = 1f)
	{
		if (this.lastClunkTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastClunkTimer = 0f;
		this.audioSource.PlayOneShot(this.hardclunks[global::UnityEngine.Random.Range(0, this.hardclunks.Length)], volume);
	}

	// Token: 0x06000289 RID: 649 RVA: 0x000268AC File Offset: 0x00024AAC
	private void PlayScrape(float volume = 1f)
	{
		if (this.lastSlideTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastSlideTimer = 0f;
		this.audioSource.PlayOneShot(this.scrapes[global::UnityEngine.Random.Range(0, this.scrapes.Length)], volume);
	}

	// Token: 0x0600028A RID: 650 RVA: 0x00026904 File Offset: 0x00024B04
	private void PlayRoll(float volume = 1f)
	{
		if (this.lastRollTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastRollTimer = 0f;
		this.audioSource.PlayOneShot(this.rolls[global::UnityEngine.Random.Range(0, this.rolls.Length)], volume);
	}

	// Token: 0x0400040C RID: 1036
	public AudioClip[] scrapes;

	// Token: 0x0400040D RID: 1037
	public AudioClip[] clunks;

	// Token: 0x0400040E RID: 1038
	public AudioClip[] hardclunks;

	// Token: 0x0400040F RID: 1039
	public AudioClip[] rolls;

	// Token: 0x04000410 RID: 1040
	public AudioClip[] splashes;

	// Token: 0x04000411 RID: 1041
	private AudioSource audioSource;

	// Token: 0x04000412 RID: 1042
	private float moveThreshold = 0.07f;

	// Token: 0x04000413 RID: 1043
	private float rollThreshold = 50f;

	// Token: 0x04000414 RID: 1044
	private float hardClunkThreshold = 200f;

	// Token: 0x04000415 RID: 1045
	private float lastContactTimer;

	// Token: 0x04000416 RID: 1046
	private float lastClunkTimer;

	// Token: 0x04000417 RID: 1047
	private float lastRollTimer;

	// Token: 0x04000418 RID: 1048
	private float lastSlideTimer;

	// Token: 0x04000419 RID: 1049
	private float lastSplashTimer;

	// Token: 0x0400041A RID: 1050
	public PhysicsMaterial2D staticFriction;

	// Token: 0x0400041B RID: 1051
	public PhysicsMaterial2D slidingFriction;

	// Token: 0x0400041C RID: 1052
	public ParticleSystem sparks;

	// Token: 0x0400041D RID: 1053
	public ParticleSystem debris;

	// Token: 0x0400041E RID: 1054
	public ParticleSystem water;

	// Token: 0x0400041F RID: 1055
	private ParticleSystem.EmitParams sparkParams;

	// Token: 0x04000420 RID: 1056
	private ParticleSystem.EmitParams debrisParams;

	// Token: 0x04000421 RID: 1057
	private ParticleSystem.ShapeModule waterModule;

	// Token: 0x04000422 RID: 1058
	private ParticleSystem.EmissionModule waterEmission;

	// Token: 0x04000423 RID: 1059
	private Vector3 baseVel;

	// Token: 0x04000424 RID: 1060
	private float tangent;

	// Token: 0x04000425 RID: 1061
	public PolygonCollider2D bottomCol;

	// Token: 0x04000426 RID: 1062
	public Rigidbody2D potRB;

	// Token: 0x04000427 RID: 1063
	private Dictionary<Collider2D, ContactPoint2D[]> collisionPoints;

	// Token: 0x04000428 RID: 1064
	private ContactPoint2D[] contacts;

	// Token: 0x04000429 RID: 1065
	private bool slide;
}
