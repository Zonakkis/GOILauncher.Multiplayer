using System;
using UnityEngine;

// Token: 0x0200014A RID: 330
public class SplashControl : MonoBehaviour
{
	// Token: 0x0600093A RID: 2362 RVA: 0x0004B400 File Offset: 0x00049800
	private void Start()
	{
		this.myCol = base.GetComponent<BoxCollider2D>();
		this.groundHeight = this.myCol.bounds.max.y;
		this.largeParams = default(ParticleSystem.EmitParams);
		this.smallParams = default(ParticleSystem.EmitParams);
		this.smallParams.velocity = Vector3.zero;
		this.largeParams.velocity = Vector3.zero;
	}

	// Token: 0x0600093B RID: 2363 RVA: 0x0004B478 File Offset: 0x00049878
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

	// Token: 0x040008C9 RID: 2249
	public ParticleSystem smallSplash;

	// Token: 0x040008CA RID: 2250
	public ParticleSystem largeSplash;

	// Token: 0x040008CB RID: 2251
	private ParticleSystem.EmitParams largeParams;

	// Token: 0x040008CC RID: 2252
	private ParticleSystem.EmitParams smallParams;

	// Token: 0x040008CD RID: 2253
	public AudioClip smallSplashSound;

	// Token: 0x040008CE RID: 2254
	public AudioClip largeSplashSound;

	// Token: 0x040008CF RID: 2255
	public AudioSource aud;

	// Token: 0x040008D0 RID: 2256
	private BoxCollider2D myCol;

	// Token: 0x040008D1 RID: 2257
	private float groundHeight;
}
