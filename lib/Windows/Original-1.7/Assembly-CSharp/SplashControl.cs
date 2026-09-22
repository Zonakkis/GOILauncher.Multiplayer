using System;
using UnityEngine;

// Token: 0x02000050 RID: 80
public class SplashControl : MonoBehaviour
{
	// Token: 0x06000286 RID: 646 RVA: 0x000183B4 File Offset: 0x000165B4
	private void Start()
	{
		this.myCol = base.GetComponent<BoxCollider2D>();
		this.groundHeight = this.myCol.bounds.max.y;
		this.largeParams = default(ParticleSystem.EmitParams);
		this.smallParams = default(ParticleSystem.EmitParams);
		this.smallParams.velocity = Vector3.zero;
		this.largeParams.velocity = Vector3.zero;
	}

	// Token: 0x06000287 RID: 647 RVA: 0x00018424 File Offset: 0x00016624
	private void OnTriggerEnter2D(Collider2D otherCol)
	{
		Vector2 velocity = otherCol.attachedRigidbody.velocity;
		if (otherCol.name == "PotCollider" || otherCol.name == "Player")
		{
			if (velocity.y < -2f)
			{
				for (int i = 0; i < (int)(-0.5 * (double)velocity.y); i++)
				{
					this.smallSplash.transform.position = new Vector3(otherCol.bounds.center.x + Random.Range(-0.5f, 0.5f), this.groundHeight, 0f);
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

	// Token: 0x04000432 RID: 1074
	public ParticleSystem smallSplash;

	// Token: 0x04000433 RID: 1075
	public ParticleSystem largeSplash;

	// Token: 0x04000434 RID: 1076
	private ParticleSystem.EmitParams largeParams;

	// Token: 0x04000435 RID: 1077
	private ParticleSystem.EmitParams smallParams;

	// Token: 0x04000436 RID: 1078
	public AudioClip smallSplashSound;

	// Token: 0x04000437 RID: 1079
	public AudioClip largeSplashSound;

	// Token: 0x04000438 RID: 1080
	public AudioSource aud;

	// Token: 0x04000439 RID: 1081
	private BoxCollider2D myCol;

	// Token: 0x0400043A RID: 1082
	private float groundHeight;
}
