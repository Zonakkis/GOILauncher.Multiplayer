using System;
using UnityEngine;

// Token: 0x0200007D RID: 125
public class SplashControl : MonoBehaviour
{
	// Token: 0x0600036C RID: 876 RVA: 0x00032120 File Offset: 0x00030320
	private void Start()
	{
		this.myCol = base.GetComponent<BoxCollider2D>();
		this.groundHeight = this.myCol.bounds.max.y;
		this.largeParams = default(ParticleSystem.EmitParams);
		this.smallParams = default(ParticleSystem.EmitParams);
		this.smallParams.velocity = Vector3.zero;
		this.largeParams.velocity = Vector3.zero;
	}

	// Token: 0x0600036D RID: 877 RVA: 0x00032190 File Offset: 0x00030390
	private void OnTriggerEnter2D(Collider2D otherCol)
	{
		Vector2 velocity = otherCol.attachedRigidbody.velocity;
		if (otherCol.name == "PotCollider" || otherCol.name == "Player")
		{
			if (velocity.y < -2f)
			{
				for (int i = 0; i < (int)(-0.5 * (double)velocity.y); i++)
				{
					this.smallSplash.transform.position = new Vector3(otherCol.bounds.center.x + global::UnityEngine.Random.Range(-0.5f, 0.5f), this.groundHeight, 0f);
					this.smallSplash.Emit(1);
				}
				this.aud.PlayOneShot(this.smallSplashSound, 0.3f);
			}
			if (velocity.y < -8f)
			{
				this.largeSplash.transform.position = new Vector3(otherCol.bounds.center.x, this.groundHeight, 0f);
				this.largeSplash.Emit(1);
				this.aud.PlayOneShot(this.largeSplashSound, 0.3f);
			}
		}
	}

	// Token: 0x04000543 RID: 1347
	public ParticleSystem smallSplash;

	// Token: 0x04000544 RID: 1348
	public ParticleSystem largeSplash;

	// Token: 0x04000545 RID: 1349
	private ParticleSystem.EmitParams largeParams;

	// Token: 0x04000546 RID: 1350
	private ParticleSystem.EmitParams smallParams;

	// Token: 0x04000547 RID: 1351
	public AudioClip smallSplashSound;

	// Token: 0x04000548 RID: 1352
	public AudioClip largeSplashSound;

	// Token: 0x04000549 RID: 1353
	public AudioSource aud;

	// Token: 0x0400054A RID: 1354
	private BoxCollider2D myCol;

	// Token: 0x0400054B RID: 1355
	private float groundHeight;
}
