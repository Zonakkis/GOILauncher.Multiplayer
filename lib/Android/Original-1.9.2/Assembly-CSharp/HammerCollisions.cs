using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200012A RID: 298
public class HammerCollisions : MonoBehaviour
{
	// Token: 0x06000791 RID: 1937 RVA: 0x0003FC00 File Offset: 0x0003E000
	private void Start()
	{
		this.collisionPoints = new Dictionary<Collider2D, IndexedCollision>();
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

	// Token: 0x06000792 RID: 1938 RVA: 0x0003FCC0 File Offset: 0x0003E0C0
	private void LateUpdate()
	{
		this.lastSoundTimer += Time.deltaTime;
		this.lastWhooshTimer += Time.deltaTime;
		this.deltaPos = base.transform.position - this.player.transform.position;
		float magnitude = (this.deltaPos - this.oldDeltaPos).magnitude;
		if (magnitude > this.whooshThreshold)
		{
			this.PlayWhoosh(Mathf.Clamp(this.deltaPos.magnitude * 0.26f, 0.1f, 1f));
		}
		this.oldDeltaPos = this.deltaPos;
	}

	// Token: 0x06000793 RID: 1939 RVA: 0x0003FD74 File Offset: 0x0003E174
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
		foreach (KeyValuePair<Collider2D, IndexedCollision> keyValuePair in this.collisionPoints)
		{
			foreach (ContactPoint2D contactPoint2D in keyValuePair.Value.Colliders)
			{
				Gizmos.color = array[num];
				Gizmos.DrawSphere(contactPoint2D.point, 0.1f);
				num = (num + 1) % array.Length;
			}
		}
	}

	// Token: 0x06000794 RID: 1940 RVA: 0x0003FEB8 File Offset: 0x0003E2B8
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.collisionPoints == null)
		{
			return;
		}
		this.indexedCollisions.Count = coll.GetContacts(this.indexedCollisions.Colliders);
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			if (coll.relativeVelocity.magnitude > 14f)
			{
				this.player.SendMessage("HammerReturn");
			}
			if (!this.collisionPoints.ContainsKey(coll.collider))
			{
				this.collisionPoints[coll.collider] = this.indexedCollisions;
				GroundCol component = coll.collider.GetComponent<GroundCol>();
				this.impactBlur.Impact(new Vector2(this.indexedCollisions.Colliders[0].point.x, this.indexedCollisions.Colliders[0].point.y), coll.relativeVelocity.magnitude / 12f);
				bool flag = false;
				if (component != null && (component.material == GroundCol.SoundMaterial.rock || component.material == GroundCol.SoundMaterial.metal || component.material == GroundCol.SoundMaterial.solidmetal))
				{
					flag = true;
				}
				if (coll.relativeVelocity.magnitude < this.hardHitThreshold)
				{
					for (int i = 0; i < this.indexedCollisions.Count; i++)
					{
						this.debrisParams.position = this.indexedCollisions.Colliders[i].point;
						if (component != null)
						{
							this.debrisParams.startColor = component.groundCol;
						}
						float normalImpulse = this.indexedCollisions.Colliders[i].normalImpulse;
						float tangentImpulse = this.indexedCollisions.Colliders[i].tangentImpulse;
						Vector3 vector = this.indexedCollisions.Colliders[i].normal;
						Vector3 vector2 = new Vector3(vector.y, -vector.x, 0f);
						for (int j = 0; j < (int)Mathf.Min((normalImpulse + tangentImpulse) * 0.1f, 20f); j++)
						{
							this.debrisParams.velocity = vector * normalImpulse * 0.2f + vector2 * ((j % 2 != 0) ? (-1f) : 1f) * tangentImpulse * 0.1f + global::UnityEngine.Random.insideUnitSphere * 3.4f;
							this.debris.Emit(this.debrisParams, 1);
						}
					}
					if (component != null)
					{
						this.PlayHit(coll.relativeVelocity.magnitude / this.hardHitThreshold, component.material);
					}
					else
					{
						this.PlayHit(coll.relativeVelocity.magnitude / this.hardHitThreshold, GroundCol.SoundMaterial.rock);
					}
				}
				else
				{
					if (flag)
					{
						for (int k = 0; k < this.indexedCollisions.Count; k++)
						{
							this.sparkParams.position = this.indexedCollisions.Colliders[k].point;
							float normalImpulse2 = this.indexedCollisions.Colliders[k].normalImpulse;
							float tangentImpulse2 = this.indexedCollisions.Colliders[k].tangentImpulse;
							float num = normalImpulse2 + tangentImpulse2;
							Vector3 vector3 = this.indexedCollisions.Colliders[k].normal;
							Vector3 vector4 = new Vector3(vector3.y, -vector3.x, 0f);
							for (int l = 0; l < (int)Mathf.Min((normalImpulse2 + tangentImpulse2) * 0.1f, 20f); l++)
							{
								this.sparkParams.velocity = vector3 * normalImpulse2 * 0.3f + vector4 * ((l % 2 != 0) ? (-1f) : 1f) * tangentImpulse2 * 0.3f + global::UnityEngine.Random.insideUnitSphere * Mathf.Clamp(num, 1f, 60f) * 0.1f;
								this.sparks.Emit(this.sparkParams, 1);
							}
						}
					}
					else
					{
						for (int m = 0; m < this.indexedCollisions.Count; m++)
						{
							this.debrisParams.position = this.indexedCollisions.Colliders[m].point;
							if (component != null)
							{
								this.debrisParams.startColor = component.groundCol;
							}
							float normalImpulse3 = this.indexedCollisions.Colliders[m].normalImpulse;
							float tangentImpulse3 = this.indexedCollisions.Colliders[m].tangentImpulse;
							float num2 = normalImpulse3 + tangentImpulse3;
							Vector3 vector5 = this.indexedCollisions.Colliders[m].normal;
							Vector3 vector6 = new Vector3(vector5.y, -vector5.x, 0f);
							for (int n = 0; n < (int)Mathf.Min((normalImpulse3 + tangentImpulse3) * 0.1f, 20f); n++)
							{
								this.debrisParams.velocity = vector5 * normalImpulse3 * 0.1f + vector6 * ((n % 2 != 0) ? (-1f) : 1f) * tangentImpulse3 * 0.1f + global::UnityEngine.Random.insideUnitSphere * Mathf.Clamp(num2, 1f, 60f) * 0.1f;
								this.debris.Emit(this.debrisParams, 1);
							}
						}
					}
					if (component != null)
					{
						this.PlayHardHit(coll.relativeVelocity.magnitude / 4f, component.material);
					}
					else
					{
						this.PlayHardHit(coll.relativeVelocity.magnitude / 4f, GroundCol.SoundMaterial.rock);
					}
				}
			}
		}
	}

	// Token: 0x06000795 RID: 1941 RVA: 0x00040548 File Offset: 0x0003E948
	private void OnCollisionStay2D(Collision2D coll)
	{
		if (this.collisionPoints == null)
		{
			return;
		}
		this.indexedCollisions.Count = coll.GetContacts(this.indexedCollisions.Colliders);
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain") && this.collisionPoints.ContainsKey(coll.collider))
		{
			bool flag = true;
			float num = 0.3f;
			float num2 = 0f;
			for (int i = 0; i < this.indexedCollisions.Count; i++)
			{
				for (int j = 0; j < this.collisionPoints[coll.collider].Count; j++)
				{
					float magnitude = (this.collisionPoints[coll.collider].Colliders[j].point - this.indexedCollisions.Colliders[i].point).magnitude;
					num = Mathf.Min(num, magnitude);
					num2 = Mathf.Max((1f + Mathf.Abs(this.indexedCollisions.Colliders[i].tangentImpulse)) / (1f + Mathf.Abs(this.indexedCollisions.Colliders[i].normalImpulse)), num2);
					if (magnitude < this.moveThreshold)
					{
						flag = false;
						break;
					}
				}
			}
			if (this.tip.velocity.magnitude > 0.3f && num2 > 5f)
			{
				flag = true;
			}
			if (flag && coll.rigidbody == null)
			{
				this.collisionPoints[coll.collider] = this.indexedCollisions;
				GroundCol component = coll.collider.GetComponent<GroundCol>();
				if (component != null)
				{
					this.dustParams.startColor = component.groundCol;
				}
				for (int k = 0; k < this.indexedCollisions.Count; k++)
				{
					this.dustParams.position = this.indexedCollisions.Colliders[k].point;
					Vector3 vector = 0.4f * new Vector3(this.indexedCollisions.Colliders[k].normal.y, -this.indexedCollisions.Colliders[k].normal.x) * Vector2.Dot(new Vector2(this.indexedCollisions.Colliders[k].normal.y, -this.indexedCollisions.Colliders[k].normal.x), coll.relativeVelocity);
					this.dustParams.velocity = global::UnityEngine.Random.insideUnitSphere * 2f + vector;
					this.dust.Emit(this.dustParams, (int)Mathf.Max(num * 50f - 1f, 0f));
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

	// Token: 0x06000796 RID: 1942 RVA: 0x000408BC File Offset: 0x0003ECBC
	private void OnCollisionExit2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain") && this.collisionPoints.ContainsKey(coll.collider))
		{
			this.collisionPoints.Remove(coll.collider);
		}
		if (this.collisionPoints == null)
		{
			this.slide = true;
		}
		else if (this.collisionPoints.Keys.Count == 0)
		{
			this.slide = true;
		}
	}

	// Token: 0x06000797 RID: 1943 RVA: 0x0004093E File Offset: 0x0003ED3E
	private void FixedUpdate()
	{
		if (this.slide)
		{
			this.myCollider.sharedMaterial = this.slidingFriction;
		}
		else
		{
			this.myCollider.sharedMaterial = this.staticFriction;
		}
	}

	// Token: 0x06000798 RID: 1944 RVA: 0x00040972 File Offset: 0x0003ED72
	private void PlayHit(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (this.lastSoundTimer < 0.1f)
		{
			return;
		}
		this.lastSoundTimer = 0f;
		this.audioSource.PlayOneShot(this.hs.GetHit(mat), volume);
	}

	// Token: 0x06000799 RID: 1945 RVA: 0x000409A8 File Offset: 0x0003EDA8
	private void PlayHardHit(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (this.lastSoundTimer < 0.1f)
		{
			return;
		}
		this.lastSoundTimer = 0f;
		this.audioSource.PlayOneShot(this.hs.GetHardHit(mat), volume);
	}

	// Token: 0x0600079A RID: 1946 RVA: 0x000409DE File Offset: 0x0003EDDE
	private void PlayScrape(float volume = 1f, GroundCol.SoundMaterial mat = GroundCol.SoundMaterial.rock)
	{
		if (this.lastSoundTimer < 0.1f)
		{
			return;
		}
		this.lastSoundTimer = 0f;
		this.audioSource.PlayOneShot(this.hs.GetScrape(mat), volume);
	}

	// Token: 0x0600079B RID: 1947 RVA: 0x00040A14 File Offset: 0x0003EE14
	private void PlayWhoosh(float volume = 1f)
	{
		if (this.lastWhooshTimer < 1f)
		{
			return;
		}
		this.lastWhooshTimer = 0f;
		this.audioSource.PlayOneShot(this.whooshes[global::UnityEngine.Random.Range(0, this.whooshes.Length)], volume);
	}

	// Token: 0x040006D0 RID: 1744
	public ImpactBlur impactBlur;

	// Token: 0x040006D1 RID: 1745
	public Dictionary<Collider2D, IndexedCollision> collisionPoints;

	// Token: 0x040006D2 RID: 1746
	private Collider2D myCollider;

	// Token: 0x040006D3 RID: 1747
	public PhysicsMaterial2D staticFriction;

	// Token: 0x040006D4 RID: 1748
	public PhysicsMaterial2D slidingFriction;

	// Token: 0x040006D5 RID: 1749
	public MeshRenderer headMesh;

	// Token: 0x040006D6 RID: 1750
	private float moveThreshold = 0.03f;

	// Token: 0x040006D7 RID: 1751
	private bool slide;

	// Token: 0x040006D8 RID: 1752
	private ParticleSystem.EmitParams sparkParams;

	// Token: 0x040006D9 RID: 1753
	private ParticleSystem.EmitParams debrisParams;

	// Token: 0x040006DA RID: 1754
	private ParticleSystem.EmitParams dustParams;

	// Token: 0x040006DB RID: 1755
	public ParticleSystem debris;

	// Token: 0x040006DC RID: 1756
	public ParticleSystem sparks;

	// Token: 0x040006DD RID: 1757
	public ParticleSystem dust;

	// Token: 0x040006DE RID: 1758
	public AudioClip[] whooshes;

	// Token: 0x040006DF RID: 1759
	private AudioSource audioSource;

	// Token: 0x040006E0 RID: 1760
	public GameObject player;

	// Token: 0x040006E1 RID: 1761
	private float lastSoundTimer;

	// Token: 0x040006E2 RID: 1762
	private Vector2 deltaPos;

	// Token: 0x040006E3 RID: 1763
	private Vector2 oldDeltaPos;

	// Token: 0x040006E4 RID: 1764
	private float whooshThreshold = 1.5f;

	// Token: 0x040006E5 RID: 1765
	private float lastWhooshTimer;

	// Token: 0x040006E6 RID: 1766
	private Rigidbody2D tip;

	// Token: 0x040006E7 RID: 1767
	public HitSoundProvider hs;

	// Token: 0x040006E8 RID: 1768
	private float hardHitThreshold = 14f;

	// Token: 0x040006E9 RID: 1769
	private IndexedCollision indexedCollisions = new IndexedCollision();
}
