using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200004C RID: 76
public class HammerCollisions : MonoBehaviour
{
	// Token: 0x060001B2 RID: 434 RVA: 0x0001F9A4 File Offset: 0x0001DBA4
	private void Start()
	{
		this.contacts = new ContactPoint2D[5];
		this.collisionPoints = new Dictionary<Collider2D, ContactPoint2D[]>();
		this.myCollider = base.GetComponent<Collider2D>();
		this.slide = true;
		this.sparkParams = default(ParticleSystem.EmitParams);
		this.debrisParams = default(ParticleSystem.EmitParams);
		this.dustParams = default(ParticleSystem.EmitParams);
		this.audioSource = base.GetComponent<AudioSource>();
		this.lastSoundTimer = 0f;
		this.lastWhooshTimer = 0f;
		this.deltaPos = base.transform.position - this.player.transform.position;
		this.oldDeltaPos = this.deltaPos;
		this.tip = base.GetComponent<Rigidbody2D>();
	}

	// Token: 0x060001B3 RID: 435 RVA: 0x0001FA64 File Offset: 0x0001DC64
	private void LateUpdate()
	{
		this.lastSoundTimer += Time.deltaTime;
		this.lastWhooshTimer += Time.deltaTime;
		this.deltaPos = base.transform.position - this.player.transform.position;
		if ((this.deltaPos - this.oldDeltaPos).magnitude > this.whooshThreshold)
		{
			this.PlayWhoosh(Mathf.Clamp(this.deltaPos.magnitude * 0.26f, 0.1f, 1f));
		}
		this.oldDeltaPos = this.deltaPos;
	}

	// Token: 0x060001B4 RID: 436 RVA: 0x0001FB14 File Offset: 0x0001DD14
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
				Gizmos.DrawSphere(contactPoint2D.point, 0.1f);
				num = (num + 1) % array.Length;
			}
		}
	}

	// Token: 0x060001B5 RID: 437 RVA: 0x0001FC14 File Offset: 0x0001DE14
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.collisionPoints == null)
		{
			return;
		}
		int num = coll.GetContacts(this.contacts);
		if (num <= 0)
		{
			return;
		}
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			if (coll.relativeVelocity.magnitude > 14f)
			{
				this.player.SendMessage("HammerReturn");
			}
			if (!this.collisionPoints.ContainsKey(coll.collider))
			{
				this.collisionPoints[coll.collider] = this.contacts;
				GroundCol component = coll.collider.GetComponent<GroundCol>();
				this.impactBlur.Impact(new Vector2(this.contacts[0].point.x, this.contacts[0].point.y), coll.relativeVelocity.magnitude / 12f);
				bool flag = false;
				if (component != null && (component.material == GroundCol.SoundMaterial.rock || component.material == GroundCol.SoundMaterial.metal || component.material == GroundCol.SoundMaterial.solidmetal))
				{
					flag = true;
				}
				if (coll.relativeVelocity.magnitude < this.hardHitThreshold)
				{
					for (int i = 0; i < num; i++)
					{
						this.debrisParams.position = this.contacts[i].point;
						if (component != null)
						{
							this.debrisParams.startColor = component.groundCol;
						}
						float normalImpulse = this.contacts[i].normalImpulse;
						float tangentImpulse = this.contacts[i].tangentImpulse;
						Vector3 vector = this.contacts[i].normal;
						Vector3 vector2 = new Vector3(vector.y, -vector.x, 0f);
						for (int j = 0; j < (int)Mathf.Min((normalImpulse + tangentImpulse) * 0.1f, 20f); j++)
						{
							this.debrisParams.velocity = vector * normalImpulse * 0.2f + vector2 * ((j % 2 == 0) ? 1f : (-1f)) * tangentImpulse * 0.1f + global::UnityEngine.Random.insideUnitSphere * 3.4f;
							this.debris.Emit(this.debrisParams, 1);
						}
					}
					if (component != null)
					{
						this.PlayHit(coll.relativeVelocity.magnitude / this.hardHitThreshold, component.material);
						return;
					}
					this.PlayHit(coll.relativeVelocity.magnitude / this.hardHitThreshold, GroundCol.SoundMaterial.rock);
					return;
				}
				else
				{
					if (flag)
					{
						for (int k = 0; k < num; k++)
						{
							this.sparkParams.position = this.contacts[k].point;
							float normalImpulse2 = this.contacts[k].normalImpulse;
							float tangentImpulse2 = this.contacts[k].tangentImpulse;
							float num2 = normalImpulse2 + tangentImpulse2;
							Vector3 vector3 = this.contacts[k].normal;
							Vector3 vector4 = new Vector3(vector3.y, -vector3.x, 0f);
							for (int l = 0; l < (int)Mathf.Min((normalImpulse2 + tangentImpulse2) * 0.1f, 20f); l++)
							{
								this.sparkParams.velocity = vector3 * normalImpulse2 * 0.3f + vector4 * ((l % 2 == 0) ? 1f : (-1f)) * tangentImpulse2 * 0.3f + global::UnityEngine.Random.insideUnitSphere * Mathf.Clamp(num2, 1f, 60f) * 0.1f;
								this.sparks.Emit(this.sparkParams, 1);
							}
						}
					}
					else
					{
						for (int m = 0; m < num; m++)
						{
							this.debrisParams.position = this.contacts[m].point;
							if (component != null)
							{
								this.debrisParams.startColor = component.groundCol;
							}
							float normalImpulse3 = this.contacts[m].normalImpulse;
							float tangentImpulse3 = this.contacts[m].tangentImpulse;
							float num3 = normalImpulse3 + tangentImpulse3;
							Vector3 vector5 = this.contacts[m].normal;
							Vector3 vector6 = new Vector3(vector5.y, -vector5.x, 0f);
							for (int n = 0; n < (int)Mathf.Min((normalImpulse3 + tangentImpulse3) * 0.1f, 20f); n++)
							{
								this.debrisParams.velocity = vector5 * normalImpulse3 * 0.1f + vector6 * ((n % 2 == 0) ? 1f : (-1f)) * tangentImpulse3 * 0.1f + global::UnityEngine.Random.insideUnitSphere * Mathf.Clamp(num3, 1f, 60f) * 0.1f;
								this.debris.Emit(this.debrisParams, 1);
							}
						}
					}
					if (component != null)
					{
						this.PlayHardHit(coll.relativeVelocity.magnitude / 4f, component.material);
						return;
					}
					this.PlayHardHit(coll.relativeVelocity.magnitude / 4f, GroundCol.SoundMaterial.rock);
				}
			}
		}
	}

	// Token: 0x060001B6 RID: 438 RVA: 0x000201E8 File Offset: 0x0001E3E8
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
				if (this.tip.velocity.magnitude > 0.3f && num3 > 5f)
				{
					flag = true;
				}
				if (flag && coll.rigidbody == null)
				{
					this.collisionPoints[coll.collider] = this.contacts;
					GroundCol component = coll.collider.GetComponent<GroundCol>();
					if (component != null)
					{
						this.dustParams.startColor = component.groundCol;
					}
					for (int k = 0; k < this.contacts.Length; k++)
					{
						this.dustParams.position = this.contacts[k].point;
						Vector3 vector = 0.4f * new Vector3(this.contacts[k].normal.y, -this.contacts[k].normal.x) * Vector2.Dot(new Vector2(this.contacts[k].normal.y, -this.contacts[k].normal.x), coll.relativeVelocity);
						this.dustParams.velocity = global::UnityEngine.Random.insideUnitSphere * 2f + vector;
						this.dust.Emit(this.dustParams, (int)Mathf.Max(num2 * 50f - 1f, 0f));
						if (!this.audioSource.isPlaying)
						{
							if (component != null)
							{
								this.PlayScrape(1f, component.material);
							}
							else
							{
								this.PlayScrape(1f, GroundCol.SoundMaterial.rock);
							}
						}
					}
				}
				this.slide = flag;
			}
		}
	}

	// Token: 0x060001B7 RID: 439 RVA: 0x000204EC File Offset: 0x0001E6EC
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

	// Token: 0x060001B8 RID: 440 RVA: 0x00003769 File Offset: 0x00001969
	private void FixedUpdate()
	{
		if (this.slide)
		{
			this.myCollider.sharedMaterial = this.slidingFriction;
			return;
		}
		this.myCollider.sharedMaterial = this.staticFriction;
	}

	// Token: 0x060001B9 RID: 441 RVA: 0x00020564 File Offset: 0x0001E764
	private void PlayHit(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (this.lastSoundTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastSoundTimer = 0f;
		this.audioSource.PlayOneShot(this.hs.GetHit(mat), volume);
	}

	// Token: 0x060001BA RID: 442 RVA: 0x000205B4 File Offset: 0x0001E7B4
	private void PlayHardHit(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (this.lastSoundTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastSoundTimer = 0f;
		this.audioSource.PlayOneShot(this.hs.GetHardHit(mat), volume);
	}

	// Token: 0x060001BB RID: 443 RVA: 0x00020604 File Offset: 0x0001E804
	private void PlayScrape(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (this.lastSoundTimer < 0.1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastSoundTimer = 0f;
		this.audioSource.PlayOneShot(this.hs.GetScrape(mat), volume);
	}

	// Token: 0x060001BC RID: 444 RVA: 0x00020654 File Offset: 0x0001E854
	private void PlayWhoosh(float volume = 1f)
	{
		if (this.lastWhooshTimer < 1f)
		{
			return;
		}
		if (volume > 3f)
		{
			volume = 3f;
		}
		this.lastWhooshTimer = 0f;
		this.audioSource.PlayOneShot(this.whooshes[global::UnityEngine.Random.Range(0, this.whooshes.Length)], volume);
	}

	// Token: 0x040002BC RID: 700
	public ImpactBlur impactBlur;

	// Token: 0x040002BD RID: 701
	public Dictionary<Collider2D, ContactPoint2D[]> collisionPoints;

	// Token: 0x040002BE RID: 702
	private Collider2D myCollider;

	// Token: 0x040002BF RID: 703
	public PhysicsMaterial2D staticFriction;

	// Token: 0x040002C0 RID: 704
	public PhysicsMaterial2D slidingFriction;

	// Token: 0x040002C1 RID: 705
	public MeshRenderer headMesh;

	// Token: 0x040002C2 RID: 706
	private float moveThreshold = 0.03f;

	// Token: 0x040002C3 RID: 707
	private bool slide;

	// Token: 0x040002C4 RID: 708
	private ParticleSystem.EmitParams sparkParams;

	// Token: 0x040002C5 RID: 709
	private ParticleSystem.EmitParams debrisParams;

	// Token: 0x040002C6 RID: 710
	private ParticleSystem.EmitParams dustParams;

	// Token: 0x040002C7 RID: 711
	public ParticleSystem debris;

	// Token: 0x040002C8 RID: 712
	public ParticleSystem sparks;

	// Token: 0x040002C9 RID: 713
	public ParticleSystem dust;

	// Token: 0x040002CA RID: 714
	public AudioClip[] whooshes;

	// Token: 0x040002CB RID: 715
	private AudioSource audioSource;

	// Token: 0x040002CC RID: 716
	public GameObject player;

	// Token: 0x040002CD RID: 717
	private float lastSoundTimer;

	// Token: 0x040002CE RID: 718
	private Vector2 deltaPos;

	// Token: 0x040002CF RID: 719
	private Vector2 oldDeltaPos;

	// Token: 0x040002D0 RID: 720
	private float whooshThreshold = 1.5f;

	// Token: 0x040002D1 RID: 721
	private float lastWhooshTimer;

	// Token: 0x040002D2 RID: 722
	private Rigidbody2D tip;

	// Token: 0x040002D3 RID: 723
	public HitSoundProvider hs;

	// Token: 0x040002D4 RID: 724
	private float hardHitThreshold = 14f;

	// Token: 0x040002D5 RID: 725
	private ContactPoint2D[] contacts;
}
