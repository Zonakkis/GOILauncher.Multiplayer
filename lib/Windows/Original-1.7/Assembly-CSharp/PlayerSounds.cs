using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200003F RID: 63
public class PlayerSounds : MonoBehaviour
{
	// Token: 0x060001E2 RID: 482 RVA: 0x000125A4 File Offset: 0x000107A4
	private void Start()
	{
		this.saviour = base.GetComponent<Saviour>();
		this.lastSoundTimer = 0f;
		this.lastSoundIndex = 0;
		this.deltaVelocity = Vector2.zero;
		this.smoothDeltaVelocity = Vector2.zero;
		this.oldVelocity = Vector2.zero;
		this.audioSource = base.GetComponent<AudioSource>();
		this.rb = base.GetComponent<Rigidbody2D>();
		this.strain = 1f;
		this.oldStrain = 1f;
		this.oldDeltaStrain = 0f;
		this.deltaStrainAccumulator = 0f;
		this.fallTimer = 0f;
		this.isTouching = false;
		this.whuhThisFall = false;
		this.sadThisFall = false;
		this.oldHammerPos = this.hammerTransform.position;
		this.blinkTimer = 0f;
		this.mouthTimer = 0f;
	}

	// Token: 0x060001E3 RID: 483 RVA: 0x00012680 File Offset: 0x00010880
	private void Update()
	{
		if (this.lastSoundTimer > 0f)
		{
			this.lastSoundTimer -= Time.deltaTime;
		}
		else
		{
			this.lastSoundTimer = 0f;
		}
		if (!this.isTouching)
		{
			if (this.smoothDeltaVelocity.y < Physics2D.gravity.y * Time.fixedDeltaTime * 0.95f && Mathf.Abs(this.smoothDeltaVelocity.x) < 0.05f)
			{
				this.fallTimer += Time.deltaTime;
				RaycastHit2D raycastHit2D = Physics2D.CircleCast(this.rb.position, 0.5f, this.smoothDeltaVelocity.normalized, 100f, this.fallLayer);
				this.fallBar.rectTransform.localScale = new Vector3(this.fallTimer / this.fallTime, 0.25f, 1f);
				if (raycastHit2D.collider == null || raycastHit2D.distance > 10f)
				{
					if (this.fallTimer > this.fallTime && !this.sadThisFall)
					{
						this.PlaySadSound(1f);
						this.sadThisFall = true;
					}
				}
				else if (raycastHit2D.distance > 5f && !this.whuhThisFall)
				{
					this.PlayWhuh(1f);
					this.whuhThisFall = true;
				}
			}
			else
			{
				this.fallTimer = 0f;
			}
		}
		else
		{
			this.fallTimer = 0f;
			this.whuhThisFall = false;
			this.sadThisFall = false;
		}
		this.blinkTimer -= Time.deltaTime;
		if (this.blinkTimer < 0f)
		{
			this.blinkTimer = Random.Range(3f, 4f);
			this.skin.SetBlendShapeWeight(16, 100f);
			this.skin.SetBlendShapeWeight(19, 100f);
		}
		else
		{
			this.skin.SetBlendShapeWeight(16, Mathf.Lerp(this.skin.GetBlendShapeWeight(16), 0f, 0.5f));
			this.skin.SetBlendShapeWeight(19, Mathf.Lerp(this.skin.GetBlendShapeWeight(19), 0f, 0.5f));
		}
		this.mouthTimer -= Time.deltaTime;
		if (this.mouthTimer <= 0f)
		{
			this.mouthTimer = 0f;
		}
		this.skin.SetBlendShapeWeight(23, Mathf.Min(100f, this.mouthTimer * 60f));
		this.skin.SetBlendShapeWeight(38, Mathf.Max(0f, this.oldDeltaStrain) * 5f);
	}

	// Token: 0x060001E4 RID: 484 RVA: 0x00012928 File Offset: 0x00010B28
	private void LateUpdate()
	{
		this.neck.localRotation = Quaternion.Euler(6.784f, 0f, Random.Range(-0.1f * Mathf.Max(0f, this.oldDeltaStrain), 0.1f * Mathf.Max(0f, this.oldDeltaStrain)));
	}

	// Token: 0x060001E5 RID: 485 RVA: 0x00012980 File Offset: 0x00010B80
	private void LateFixedUpdate()
	{
	}

	// Token: 0x060001E6 RID: 486 RVA: 0x00012984 File Offset: 0x00010B84
	private void FixedUpdate()
	{
		this.deltaVelocity = this.rb.velocity - this.oldVelocity;
		this.smoothDeltaVelocity = Vector2.Lerp(this.smoothDeltaVelocity, this.deltaVelocity, 0.2f);
		if (!this.hj)
		{
			this.hj = this.saviour.hinge;
		}
		if (!this.sj)
		{
			this.sj = this.saviour.slider;
		}
		float num = Mathf.Abs(this.hj.GetReactionTorque(Time.fixedDeltaTime)) + this.sj.GetReactionForce(Time.fixedDeltaTime).magnitude;
		if (num > 7000f)
		{
			this.strain += num * 0.0009f * Time.fixedDeltaTime;
		}
		else
		{
			this.strain -= 0.026f;
		}
		this.strain = Mathf.Clamp(this.strain, 0f, 20f);
		this.strainbar.rectTransform.localScale = new Vector3(this.strain * 2f, 0.25f, 1f);
		float num2 = 100f * (this.strain - this.oldStrain);
		float num3 = Mathf.Lerp(this.oldDeltaStrain, num2, 0.25f);
		this.deltaStrainAccumulator *= 0.92f;
		this.deltaStrainAccumulator += num3 * Time.fixedDeltaTime;
		this.deltaStrainAccumulator = Mathf.Max(0f, this.deltaStrainAccumulator);
		this.strainbar.rectTransform.localScale = new Vector3(this.deltaStrainAccumulator, 0.25f, 1f);
		float magnitude = (this.hammerTransform.position - this.oldHammerPos).magnitude;
		this.oldHammerPos = this.hammerTransform.position;
		float num4 = num3 - this.oldDeltaStrain;
		this.jerkBar.rectTransform.localScale = new Vector3(num4 * 0.1f, 0.25f, 1f);
		this.deltaStrainbar.rectTransform.localScale = new Vector3(num3, 0.25f, 1f);
		if (num3 > this.jumpThreshold && num < 0.1f)
		{
			this.PlayJump(1f);
		}
		else if (num4 < this.releaseThreshold)
		{
			this.PlayReleaseSound(1f);
		}
		else if (num3 > this.hardStrainThreshold && magnitude < this.hardStrainSpeedThreshold)
		{
			this.PlayStrainSound(1f, true);
		}
		else if (this.deltaStrainAccumulator > this.strainThreshold)
		{
			this.PlayStrainSound(1f, false);
			this.deltaStrainAccumulator = 0f;
		}
		this.oldStrain = this.strain;
		this.oldDeltaStrain = num3;
		this.oldVelocity = this.rb.velocity;
		this.isTouching = false;
	}

	// Token: 0x060001E7 RID: 487 RVA: 0x00012C6C File Offset: 0x00010E6C
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (!this.rb)
		{
			Debug.Log("rb not ready to play sounds yet");
			return;
		}
		if (this.contacts == null)
		{
			this.contacts = new ContactPoint2D[20];
		}
		if (coll.GetContacts(this.contacts) <= 0)
		{
			return;
		}
		if (coll.relativeVelocity.magnitude > this.impactThreshold && this.contacts[0].normal.y > 0.5f)
		{
			this.PlayImpactSound(1f);
			return;
		}
		if (this.rb.GetPoint(this.contacts[0].point).y > 0.4f && coll.relativeVelocity.magnitude > this.hurtThreshold)
		{
			this.PlayPainSound(1f);
		}
	}

	// Token: 0x060001E8 RID: 488 RVA: 0x00012D3E File Offset: 0x00010F3E
	private void OnCollisionStay2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			this.isTouching = true;
		}
	}

	// Token: 0x060001E9 RID: 489 RVA: 0x00012D60 File Offset: 0x00010F60
	private void PlayStrainSound(float volume = 1f, bool hardStrain = false)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		if (hardStrain)
		{
			this.lastSoundIndex = Random.Range(0, this.hardStrainSounds.Length);
			this.audioSource.PlayOneShot(this.hardStrainSounds[this.lastSoundIndex], volume);
			this.lastSoundTimer = this.hardStrainSounds[this.lastSoundIndex].length * 2.5f;
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.strainSounds.Length);
		this.audioSource.PlayOneShot(this.strainSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.strainSounds[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x060001EA RID: 490 RVA: 0x00012E18 File Offset: 0x00011018
	private void PlayReleaseSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.releaseSounds.Length);
		this.audioSource.PlayOneShot(this.releaseSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.releaseSounds[this.lastSoundIndex].length;
		this.mouthTimer = this.releaseSounds[this.lastSoundIndex].length;
	}

	// Token: 0x060001EB RID: 491 RVA: 0x00012E90 File Offset: 0x00011090
	private void PlayPainSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.painSounds.Length);
		this.audioSource.PlayOneShot(this.painSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.painSounds[this.lastSoundIndex].length * 1.5f;
		this.mouthTimer = this.painSounds[this.lastSoundIndex].length;
	}

	// Token: 0x060001EC RID: 492 RVA: 0x00012F10 File Offset: 0x00011110
	private void PlayImpactSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.impactSounds.Length);
		this.audioSource.PlayOneShot(this.impactSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.impactSounds[this.lastSoundIndex].length * 2.5f;
		this.mouthTimer = this.impactSounds[this.lastSoundIndex].length;
	}

	// Token: 0x060001ED RID: 493 RVA: 0x00012F90 File Offset: 0x00011190
	private void PlaySadSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.sadSounds.Length);
		this.audioSource.PlayOneShot(this.sadSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.sadSounds[this.lastSoundIndex].length * 2.5f;
		this.mouthTimer = this.sadSounds[this.lastSoundIndex].length;
		this.saviour.SaveGameNow(false);
	}

	// Token: 0x060001EE RID: 494 RVA: 0x0001301C File Offset: 0x0001121C
	private void PlayWhuh(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.whuSounds.Length);
		this.audioSource.PlayOneShot(this.whuSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.whuSounds[this.lastSoundIndex].length * 1.5f;
		this.mouthTimer = this.whuSounds[this.lastSoundIndex].length;
	}

	// Token: 0x060001EF RID: 495 RVA: 0x0001309C File Offset: 0x0001129C
	private void PlayGrunt(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.audioSource.PlayOneShot(this.grunts[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.grunts[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x060001F0 RID: 496 RVA: 0x000130F0 File Offset: 0x000112F0
	private void PlayJump(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = Random.Range(0, this.jumpSounds.Length);
		this.audioSource.PlayOneShot(this.jumpSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.jumpSounds[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x04000312 RID: 786
	public AudioClip[] hardStrainSounds;

	// Token: 0x04000313 RID: 787
	public AudioClip[] strainSounds;

	// Token: 0x04000314 RID: 788
	public AudioClip[] releaseSounds;

	// Token: 0x04000315 RID: 789
	public AudioClip[] painSounds;

	// Token: 0x04000316 RID: 790
	public AudioClip[] impactSounds;

	// Token: 0x04000317 RID: 791
	public AudioClip[] sadSounds;

	// Token: 0x04000318 RID: 792
	public AudioClip[] grunts;

	// Token: 0x04000319 RID: 793
	public AudioClip[] jumpSounds;

	// Token: 0x0400031A RID: 794
	public AudioClip[] whuSounds;

	// Token: 0x0400031B RID: 795
	public HingeJoint2D hj;

	// Token: 0x0400031C RID: 796
	public SliderJoint2D sj;

	// Token: 0x0400031D RID: 797
	private Rigidbody2D rb;

	// Token: 0x0400031E RID: 798
	private Vector2 deltaVelocity;

	// Token: 0x0400031F RID: 799
	private Vector2 smoothDeltaVelocity;

	// Token: 0x04000320 RID: 800
	private Vector2 oldVelocity;

	// Token: 0x04000321 RID: 801
	private float lastSoundTimer;

	// Token: 0x04000322 RID: 802
	private AudioSource audioSource;

	// Token: 0x04000323 RID: 803
	private int lastSoundIndex;

	// Token: 0x04000324 RID: 804
	public Transform hammerTransform;

	// Token: 0x04000325 RID: 805
	private Vector2 oldHammerPos;

	// Token: 0x04000326 RID: 806
	private float strain;

	// Token: 0x04000327 RID: 807
	private float oldStrain;

	// Token: 0x04000328 RID: 808
	private float oldDeltaStrain;

	// Token: 0x04000329 RID: 809
	private float deltaStrainAccumulator;

	// Token: 0x0400032A RID: 810
	private float hurtThreshold = 8f;

	// Token: 0x0400032B RID: 811
	private float jumpThreshold = 10f;

	// Token: 0x0400032C RID: 812
	private float releaseThreshold = -2f;

	// Token: 0x0400032D RID: 813
	private float strainThreshold = 0.6f;

	// Token: 0x0400032E RID: 814
	private float hardStrainThreshold = 7f;

	// Token: 0x0400032F RID: 815
	private float hardStrainSpeedThreshold = 0.01f;

	// Token: 0x04000330 RID: 816
	private float impactThreshold = 20f;

	// Token: 0x04000331 RID: 817
	private float fallTime = 0.2f;

	// Token: 0x04000332 RID: 818
	private float fallTimer;

	// Token: 0x04000333 RID: 819
	private bool whuhThisFall;

	// Token: 0x04000334 RID: 820
	private bool sadThisFall;

	// Token: 0x04000335 RID: 821
	public LayerMask fallLayer;

	// Token: 0x04000336 RID: 822
	public Image strainbar;

	// Token: 0x04000337 RID: 823
	public Image deltaStrainbar;

	// Token: 0x04000338 RID: 824
	public Image fallBar;

	// Token: 0x04000339 RID: 825
	public Image jerkBar;

	// Token: 0x0400033A RID: 826
	public bool isTouching;

	// Token: 0x0400033B RID: 827
	public SkinnedMeshRenderer skin;

	// Token: 0x0400033C RID: 828
	public Transform neck;

	// Token: 0x0400033D RID: 829
	public PoseControl pose;

	// Token: 0x0400033E RID: 830
	private Saviour saviour;

	// Token: 0x0400033F RID: 831
	private float blinkTimer;

	// Token: 0x04000340 RID: 832
	private float mouthTimer;

	// Token: 0x04000341 RID: 833
	private ContactPoint2D[] contacts;
}
