using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000041 RID: 65
public class PotSounds : MonoBehaviour
{
	// Token: 0x060001FA RID: 506 RVA: 0x00013BAC File Offset: 0x00011DAC
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

	// Token: 0x060001FB RID: 507 RVA: 0x00013C58 File Offset: 0x00011E58
	private void LateUpdate()
	{
		this.lastContactTimer += Time.deltaTime;
		this.lastClunkTimer += Time.deltaTime;
		this.lastSlideTimer += Time.deltaTime;
		this.lastRollTimer += Time.deltaTime;
		this.lastSplashTimer += Time.deltaTime;
	}

	// Token: 0x060001FC RID: 508 RVA: 0x00013CBF File Offset: 0x00011EBF
	private void FixedUpdate()
	{
		if (this.slide)
		{
			this.bottomCol.sharedMaterial = this.slidingFriction;
			return;
		}
		this.bottomCol.sharedMaterial = this.staticFriction;
	}

	// Token: 0x060001FD RID: 509 RVA: 0x00013CEC File Offset: 0x00011EEC
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
							this.debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
							this.debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
							this.debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
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
							this.sparkParams.velocity = Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
							this.sparkParams.velocity = Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
							this.sparkParams.velocity = Random.insideUnitSphere * 6f + vector2;
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

	// Token: 0x060001FE RID: 510 RVA: 0x000141B8 File Offset: 0x000123B8
	private void Splash(float mag)
	{
		this.water.transform.localEulerAngles = new Vector3(-90f, 0f, Random.Range(-180f, 180f));
		this.waterModule.arcSpeed = ((Random.value < 0.5f) ? (-2f) : 2f);
		this.waterEmission.rateOverTime = mag * 10f;
		this.water.Play();
		if (mag > 10f)
		{
			this.PlaySplash(Mathf.Clamp(mag / 18f - 0.2f, 0f, 0.8f));
		}
	}

	// Token: 0x060001FF RID: 511 RVA: 0x00014268 File Offset: 0x00012468
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

	// Token: 0x06000200 RID: 512 RVA: 0x00014408 File Offset: 0x00012608
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
						this.debrisParams.velocity = Random.insideUnitSphere * 6f + vector;
						this.debris.Emit(this.debrisParams, (int)Mathf.Max(num2 * 50f - 1f, 0f));
						this.PlayScrape(1f);
					}
				}
				this.slide = flag;
			}
		}
	}

	// Token: 0x06000201 RID: 513 RVA: 0x000146D8 File Offset: 0x000128D8
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

	// Token: 0x06000202 RID: 514 RVA: 0x00014750 File Offset: 0x00012950
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
		this.audioSource.PlayOneShot(this.splashes[Random.Range(0, this.splashes.Length)], volume);
	}

	// Token: 0x06000203 RID: 515 RVA: 0x000147A8 File Offset: 0x000129A8
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
		this.audioSource.PlayOneShot(this.clunks[Random.Range(0, this.clunks.Length)], volume);
	}

	// Token: 0x06000204 RID: 516 RVA: 0x00014800 File Offset: 0x00012A00
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
		this.audioSource.PlayOneShot(this.hardclunks[Random.Range(0, this.hardclunks.Length)], volume);
	}

	// Token: 0x06000205 RID: 517 RVA: 0x00014858 File Offset: 0x00012A58
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
		this.audioSource.PlayOneShot(this.scrapes[Random.Range(0, this.scrapes.Length)], volume);
	}

	// Token: 0x06000206 RID: 518 RVA: 0x000148B0 File Offset: 0x00012AB0
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
		this.audioSource.PlayOneShot(this.rolls[Random.Range(0, this.rolls.Length)], volume);
	}

	// Token: 0x04000369 RID: 873
	public AudioClip[] scrapes;

	// Token: 0x0400036A RID: 874
	public AudioClip[] clunks;

	// Token: 0x0400036B RID: 875
	public AudioClip[] hardclunks;

	// Token: 0x0400036C RID: 876
	public AudioClip[] rolls;

	// Token: 0x0400036D RID: 877
	public AudioClip[] splashes;

	// Token: 0x0400036E RID: 878
	private AudioSource audioSource;

	// Token: 0x0400036F RID: 879
	private float moveThreshold = 0.07f;

	// Token: 0x04000370 RID: 880
	private float rollThreshold = 50f;

	// Token: 0x04000371 RID: 881
	private float hardClunkThreshold = 200f;

	// Token: 0x04000372 RID: 882
	private float lastContactTimer;

	// Token: 0x04000373 RID: 883
	private float lastClunkTimer;

	// Token: 0x04000374 RID: 884
	private float lastRollTimer;

	// Token: 0x04000375 RID: 885
	private float lastSlideTimer;

	// Token: 0x04000376 RID: 886
	private float lastSplashTimer;

	// Token: 0x04000377 RID: 887
	public PhysicsMaterial2D staticFriction;

	// Token: 0x04000378 RID: 888
	public PhysicsMaterial2D slidingFriction;

	// Token: 0x04000379 RID: 889
	public ParticleSystem sparks;

	// Token: 0x0400037A RID: 890
	public ParticleSystem debris;

	// Token: 0x0400037B RID: 891
	public ParticleSystem water;

	// Token: 0x0400037C RID: 892
	private ParticleSystem.EmitParams sparkParams;

	// Token: 0x0400037D RID: 893
	private ParticleSystem.EmitParams debrisParams;

	// Token: 0x0400037E RID: 894
	private ParticleSystem.ShapeModule waterModule;

	// Token: 0x0400037F RID: 895
	private ParticleSystem.EmissionModule waterEmission;

	// Token: 0x04000380 RID: 896
	private Vector3 baseVel;

	// Token: 0x04000381 RID: 897
	private float tangent;

	// Token: 0x04000382 RID: 898
	public PolygonCollider2D bottomCol;

	// Token: 0x04000383 RID: 899
	public Rigidbody2D potRB;

	// Token: 0x04000384 RID: 900
	private Dictionary<Collider2D, ContactPoint2D[]> collisionPoints;

	// Token: 0x04000385 RID: 901
	private ContactPoint2D[] contacts;

	// Token: 0x04000386 RID: 902
	private bool slide;
}
