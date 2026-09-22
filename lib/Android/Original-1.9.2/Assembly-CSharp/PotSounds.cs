using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000139 RID: 313
public class PotSounds : MonoBehaviour
{
	// Token: 0x06000814 RID: 2068 RVA: 0x00046674 File Offset: 0x00044A74
	private void Start()
	{
		this.sparkParams = default(ParticleSystem.EmitParams);
		this.debrisParams = default(ParticleSystem.EmitParams);
		this.sparksPC = this.sparks.collision;
		this.debrisPC = this.debris.collision;
		this.waterPC = this.water.collision;
		this.sparksPC.enabled = false;
		this.debrisPC.enabled = false;
		this.waterPC.enabled = false;
		this.collisionPoints = new Dictionary<Collider2D, IndexedCollision>();
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

	// Token: 0x06000815 RID: 2069 RVA: 0x00046770 File Offset: 0x00044B70
	private void LateUpdate()
	{
		this.lastContactTimer += Time.deltaTime;
		this.lastClunkTimer += Time.deltaTime;
		this.lastSlideTimer += Time.deltaTime;
		this.lastRollTimer += Time.deltaTime;
		this.lastSplashTimer += Time.deltaTime;
	}

	// Token: 0x06000816 RID: 2070 RVA: 0x000467D7 File Offset: 0x00044BD7
	private void FixedUpdate()
	{
		if (this.slide)
		{
			this.bottomCol.sharedMaterial = this.slidingFriction;
		}
		else
		{
			this.bottomCol.sharedMaterial = this.staticFriction;
		}
	}

	// Token: 0x06000817 RID: 2071 RVA: 0x0004680C File Offset: 0x00044C0C
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (this.collisionPoints == null)
		{
			return;
		}
		this.indexedCollisions.Count = coll.GetContacts(this.indexedCollisions.Colliders);
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
				this.collisionPoints[coll.collider] = this.indexedCollisions;
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
				float num = 0f;
				for (int i = 0; i < this.indexedCollisions.Count; i++)
				{
					num += this.indexedCollisions.Colliders[i].normalImpulse;
				}
				if (num > this.hardClunkThreshold * 0.3f)
				{
					if (num < this.hardClunkThreshold)
					{
						for (int j = 0; j < this.indexedCollisions.Count; j++)
						{
							this.debrisParams.position = this.indexedCollisions.Colliders[j].point;
							if (component != null)
							{
								this.debrisParams.startColor = component.groundCol;
							}
							Vector3 vector = 2f * new Vector3(this.indexedCollisions.Colliders[j].normal.y, this.indexedCollisions.Colliders[j].normal.x) * Vector2.Dot(new Vector2(this.indexedCollisions.Colliders[j].normal.y, this.indexedCollisions.Colliders[j].normal.x), coll.relativeVelocity);
							this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
							this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
							this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
							this.debris.Emit(this.debrisParams, 1);
						}
						this.PlayClunk((num - this.hardClunkThreshold * 0.3f) / this.hardClunkThreshold * 0.7f);
					}
					else if (flag)
					{
						for (int k = 0; k < this.indexedCollisions.Count; k++)
						{
							this.sparkParams.position = this.indexedCollisions.Colliders[k].point;
							Vector3 vector2 = 2f * new Vector3(this.indexedCollisions.Colliders[k].normal.y, this.indexedCollisions.Colliders[k].normal.x) * Vector2.Dot(new Vector2(this.indexedCollisions.Colliders[k].normal.y, this.indexedCollisions.Colliders[k].normal.x), coll.relativeVelocity);
							this.sparkParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
							this.sparkParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
							this.sparkParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector2;
							this.sparks.Emit(this.sparkParams, 1);
						}
						this.PlayHardClunk(num / (2f * this.hardClunkThreshold));
					}
					this.Splash(coll.relativeVelocity.magnitude);
				}
				this.lastContactTimer = 0f;
			}
		}
	}

	// Token: 0x06000818 RID: 2072 RVA: 0x00046D84 File Offset: 0x00045184
	private void Splash(float mag)
	{
		this.water.transform.localEulerAngles = new Vector3(-90f, 0f, global::UnityEngine.Random.Range(-180f, 180f));
		this.waterModule.arcSpeed = ((global::UnityEngine.Random.value >= 0.5f) ? 2f : (-2f));
		this.waterEmission.rateOverTime = mag * 10f;
		this.water.Play();
		if (mag > 10f)
		{
			this.PlaySplash(Mathf.Clamp(mag / 18f - 0.2f, 0f, 0.8f));
		}
	}

	// Token: 0x06000819 RID: 2073 RVA: 0x00046E3C File Offset: 0x0004523C
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
			for (int i = 0; i < keyValuePair.Value.Count; i++)
			{
				Gizmos.color = array[num];
				Gizmos.DrawSphere(keyValuePair.Value.Colliders[i].point + new Vector3(0f, 0f, -2f), 0.1f);
				num = (num + 1) % array.Length;
			}
		}
		if (this.slide)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(new Vector3(this.potRB.position.x, this.potRB.position.y, -2f), 0.5f);
		}
		else
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(new Vector3(this.potRB.position.x, this.potRB.position.y, -2f), 0.5f);
		}
	}

	// Token: 0x0600081A RID: 2074 RVA: 0x00047030 File Offset: 0x00045430
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
			if (num2 > 6f && this.potRB.velocity.magnitude > 2f)
			{
				flag = true;
			}
			if (flag && coll.rigidbody == null)
			{
				this.collisionPoints[coll.collider] = this.indexedCollisions;
				GroundCol component = coll.collider.GetComponent<GroundCol>();
				if (component != null)
				{
					this.debrisParams.startColor = component.groundCol;
				}
				for (int k = 0; k < this.indexedCollisions.Count; k++)
				{
					this.debrisParams.position = this.indexedCollisions.Colliders[k].point;
					Vector3 vector = 0.4f * new Vector3(this.indexedCollisions.Colliders[k].normal.y, -this.indexedCollisions.Colliders[k].normal.x) * Vector2.Dot(new Vector2(this.indexedCollisions.Colliders[k].normal.y, -this.indexedCollisions.Colliders[k].normal.x), coll.relativeVelocity);
					this.debrisParams.velocity = global::UnityEngine.Random.insideUnitSphere * 6f + vector;
					this.debris.Emit(this.debrisParams, (int)Mathf.Max(num * 50f - 1f, 0f));
					this.PlayScrape(1f);
				}
			}
			this.slide = flag;
		}
	}

	// Token: 0x0600081B RID: 2075 RVA: 0x00047370 File Offset: 0x00045770
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

	// Token: 0x0600081C RID: 2076 RVA: 0x000473F2 File Offset: 0x000457F2
	private void PlaySplash(float volume = 1f)
	{
		if (this.lastSplashTimer < 0.1f)
		{
			return;
		}
		this.lastSplashTimer = 0f;
		this.audioSource.PlayOneShot(this.splashes[global::UnityEngine.Random.Range(0, this.splashes.Length)], volume);
	}

	// Token: 0x0600081D RID: 2077 RVA: 0x00047431 File Offset: 0x00045831
	private void PlayClunk(float volume = 1f)
	{
		if (this.lastClunkTimer < 0.1f)
		{
			return;
		}
		this.lastClunkTimer = 0f;
		this.audioSource.PlayOneShot(this.clunks[global::UnityEngine.Random.Range(0, this.clunks.Length)], volume);
	}

	// Token: 0x0600081E RID: 2078 RVA: 0x00047470 File Offset: 0x00045870
	private void PlayHardClunk(float volume = 1f)
	{
		if (this.lastClunkTimer < 0.1f)
		{
			return;
		}
		this.lastClunkTimer = 0f;
		this.audioSource.PlayOneShot(this.hardclunks[global::UnityEngine.Random.Range(0, this.hardclunks.Length)], volume);
	}

	// Token: 0x0600081F RID: 2079 RVA: 0x000474AF File Offset: 0x000458AF
	private void PlayScrape(float volume = 1f)
	{
		if (this.lastSlideTimer < 0.1f)
		{
			return;
		}
		this.lastSlideTimer = 0f;
		this.audioSource.PlayOneShot(this.scrapes[global::UnityEngine.Random.Range(0, this.scrapes.Length)], volume);
	}

	// Token: 0x06000820 RID: 2080 RVA: 0x000474EE File Offset: 0x000458EE
	private void PlayRoll(float volume = 1f)
	{
		if (this.lastRollTimer < 0.1f)
		{
			return;
		}
		this.lastRollTimer = 0f;
		this.audioSource.PlayOneShot(this.rolls[global::UnityEngine.Random.Range(0, this.rolls.Length)], volume);
	}

	// Token: 0x040007F1 RID: 2033
	public AudioClip[] scrapes;

	// Token: 0x040007F2 RID: 2034
	public AudioClip[] clunks;

	// Token: 0x040007F3 RID: 2035
	public AudioClip[] hardclunks;

	// Token: 0x040007F4 RID: 2036
	public AudioClip[] rolls;

	// Token: 0x040007F5 RID: 2037
	public AudioClip[] splashes;

	// Token: 0x040007F6 RID: 2038
	private AudioSource audioSource;

	// Token: 0x040007F7 RID: 2039
	private float moveThreshold = 0.07f;

	// Token: 0x040007F8 RID: 2040
	private float rollThreshold = 50f;

	// Token: 0x040007F9 RID: 2041
	private float hardClunkThreshold = 200f;

	// Token: 0x040007FA RID: 2042
	private float lastContactTimer;

	// Token: 0x040007FB RID: 2043
	private float lastClunkTimer;

	// Token: 0x040007FC RID: 2044
	private float lastRollTimer;

	// Token: 0x040007FD RID: 2045
	private float lastSlideTimer;

	// Token: 0x040007FE RID: 2046
	private float lastSplashTimer;

	// Token: 0x040007FF RID: 2047
	public PhysicsMaterial2D staticFriction;

	// Token: 0x04000800 RID: 2048
	public PhysicsMaterial2D slidingFriction;

	// Token: 0x04000801 RID: 2049
	public ParticleSystem sparks;

	// Token: 0x04000802 RID: 2050
	public ParticleSystem debris;

	// Token: 0x04000803 RID: 2051
	public ParticleSystem water;

	// Token: 0x04000804 RID: 2052
	private ParticleSystem.EmitParams sparkParams;

	// Token: 0x04000805 RID: 2053
	private ParticleSystem.EmitParams debrisParams;

	// Token: 0x04000806 RID: 2054
	private ParticleSystem.ShapeModule waterModule;

	// Token: 0x04000807 RID: 2055
	private ParticleSystem.EmissionModule waterEmission;

	// Token: 0x04000808 RID: 2056
	private ParticleSystem.CollisionModule sparksPC;

	// Token: 0x04000809 RID: 2057
	private ParticleSystem.CollisionModule debrisPC;

	// Token: 0x0400080A RID: 2058
	private ParticleSystem.CollisionModule waterPC;

	// Token: 0x0400080B RID: 2059
	private Vector3 baseVel;

	// Token: 0x0400080C RID: 2060
	private float tangent;

	// Token: 0x0400080D RID: 2061
	public PolygonCollider2D bottomCol;

	// Token: 0x0400080E RID: 2062
	public Rigidbody2D potRB;

	// Token: 0x0400080F RID: 2063
	private Dictionary<Collider2D, IndexedCollision> collisionPoints;

	// Token: 0x04000810 RID: 2064
	private IndexedCollision indexedCollisions = new IndexedCollision();

	// Token: 0x04000811 RID: 2065
	private bool slide;
}
