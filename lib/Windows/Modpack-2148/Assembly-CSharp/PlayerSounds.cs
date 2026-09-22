using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000065 RID: 101
public class PlayerSounds : MonoBehaviour
{
	// Token: 0x06000266 RID: 614 RVA: 0x00024548 File Offset: 0x00022748
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

	// Token: 0x06000267 RID: 615 RVA: 0x00024624 File Offset: 0x00022824
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
			this.blinkTimer = global::UnityEngine.Random.Range(3f, 4f);
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

	// Token: 0x06000268 RID: 616 RVA: 0x000248CC File Offset: 0x00022ACC
	private void LateUpdate()
	{
		this.neck.localRotation = Quaternion.Euler(6.784f, 0f, global::UnityEngine.Random.Range(-0.1f * Mathf.Max(0f, this.oldDeltaStrain), 0.1f * Mathf.Max(0f, this.oldDeltaStrain)));
	}

	// Token: 0x06000269 RID: 617 RVA: 0x0000265E File Offset: 0x0000085E
	private void LateFixedUpdate()
	{
	}

	// Token: 0x0600026A RID: 618 RVA: 0x00024924 File Offset: 0x00022B24
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

	// Token: 0x0600026B RID: 619 RVA: 0x00024C0C File Offset: 0x00022E0C
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

	// Token: 0x0600026C RID: 620 RVA: 0x00003D0A File Offset: 0x00001F0A
	private void OnCollisionStay2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			this.isTouching = true;
		}
	}

	// Token: 0x0600026D RID: 621 RVA: 0x00024CE0 File Offset: 0x00022EE0
	private void PlayStrainSound(float volume = 1f, bool hardStrain = false)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		if (hardStrain)
		{
			this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.hardStrainSounds.Length);
			this.audioSource.PlayOneShot(this.hardStrainSounds[this.lastSoundIndex], volume);
			this.lastSoundTimer = this.hardStrainSounds[this.lastSoundIndex].length * 2.5f;
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.strainSounds.Length);
		this.audioSource.PlayOneShot(this.strainSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.strainSounds[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x0600026E RID: 622 RVA: 0x00024D98 File Offset: 0x00022F98
	private void PlayReleaseSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.releaseSounds.Length);
		this.audioSource.PlayOneShot(this.releaseSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.releaseSounds[this.lastSoundIndex].length;
		this.mouthTimer = this.releaseSounds[this.lastSoundIndex].length;
	}

	// Token: 0x0600026F RID: 623 RVA: 0x00024E10 File Offset: 0x00023010
	private void PlayPainSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.painSounds.Length);
		this.audioSource.PlayOneShot(this.painSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.painSounds[this.lastSoundIndex].length * 1.5f;
		this.mouthTimer = this.painSounds[this.lastSoundIndex].length;
	}

	// Token: 0x06000270 RID: 624 RVA: 0x00024E90 File Offset: 0x00023090
	private void PlayImpactSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.impactSounds.Length);
		this.audioSource.PlayOneShot(this.impactSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.impactSounds[this.lastSoundIndex].length * 2.5f;
		this.mouthTimer = this.impactSounds[this.lastSoundIndex].length;
	}

	// Token: 0x06000271 RID: 625 RVA: 0x00024F10 File Offset: 0x00023110
	private void PlaySadSound(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.sadSounds.Length);
		this.audioSource.PlayOneShot(this.sadSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.sadSounds[this.lastSoundIndex].length * 2.5f;
		this.mouthTimer = this.sadSounds[this.lastSoundIndex].length;
		this.saviour.SaveGameNow(false);
	}

	// Token: 0x06000272 RID: 626 RVA: 0x00024F9C File Offset: 0x0002319C
	private void PlayWhuh(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.whuSounds.Length);
		this.audioSource.PlayOneShot(this.whuSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.whuSounds[this.lastSoundIndex].length * 1.5f;
		this.mouthTimer = this.whuSounds[this.lastSoundIndex].length;
	}

	// Token: 0x06000273 RID: 627 RVA: 0x0002501C File Offset: 0x0002321C
	private void PlayGrunt(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.audioSource.PlayOneShot(this.grunts[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.grunts[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x06000274 RID: 628 RVA: 0x00025070 File Offset: 0x00023270
	private void PlayJump(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.jumpSounds.Length);
		this.audioSource.PlayOneShot(this.jumpSounds[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.jumpSounds[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x040003B5 RID: 949
	public AudioClip[] hardStrainSounds;

	// Token: 0x040003B6 RID: 950
	public AudioClip[] strainSounds;

	// Token: 0x040003B7 RID: 951
	public AudioClip[] releaseSounds;

	// Token: 0x040003B8 RID: 952
	public AudioClip[] painSounds;

	// Token: 0x040003B9 RID: 953
	public AudioClip[] impactSounds;

	// Token: 0x040003BA RID: 954
	public AudioClip[] sadSounds;

	// Token: 0x040003BB RID: 955
	public AudioClip[] grunts;

	// Token: 0x040003BC RID: 956
	public AudioClip[] jumpSounds;

	// Token: 0x040003BD RID: 957
	public AudioClip[] whuSounds;

	// Token: 0x040003BE RID: 958
	public HingeJoint2D hj;

	// Token: 0x040003BF RID: 959
	public SliderJoint2D sj;

	// Token: 0x040003C0 RID: 960
	private Rigidbody2D rb;

	// Token: 0x040003C1 RID: 961
	private Vector2 deltaVelocity;

	// Token: 0x040003C2 RID: 962
	private Vector2 smoothDeltaVelocity;

	// Token: 0x040003C3 RID: 963
	private Vector2 oldVelocity;

	// Token: 0x040003C4 RID: 964
	private float lastSoundTimer;

	// Token: 0x040003C5 RID: 965
	private AudioSource audioSource;

	// Token: 0x040003C6 RID: 966
	private int lastSoundIndex;

	// Token: 0x040003C7 RID: 967
	public Transform hammerTransform;

	// Token: 0x040003C8 RID: 968
	private Vector2 oldHammerPos;

	// Token: 0x040003C9 RID: 969
	private float strain;

	// Token: 0x040003CA RID: 970
	private float oldStrain;

	// Token: 0x040003CB RID: 971
	private float oldDeltaStrain;

	// Token: 0x040003CC RID: 972
	private float deltaStrainAccumulator;

	// Token: 0x040003CD RID: 973
	private float hurtThreshold = 8f;

	// Token: 0x040003CE RID: 974
	private float jumpThreshold = 10f;

	// Token: 0x040003CF RID: 975
	private float releaseThreshold = -2f;

	// Token: 0x040003D0 RID: 976
	private float strainThreshold = 0.6f;

	// Token: 0x040003D1 RID: 977
	private float hardStrainThreshold = 7f;

	// Token: 0x040003D2 RID: 978
	private float hardStrainSpeedThreshold = 0.01f;

	// Token: 0x040003D3 RID: 979
	private float impactThreshold = 20f;

	// Token: 0x040003D4 RID: 980
	private float fallTime = 0.2f;

	// Token: 0x040003D5 RID: 981
	private float fallTimer;

	// Token: 0x040003D6 RID: 982
	private bool whuhThisFall;

	// Token: 0x040003D7 RID: 983
	private bool sadThisFall;

	// Token: 0x040003D8 RID: 984
	public LayerMask fallLayer;

	// Token: 0x040003D9 RID: 985
	public Image strainbar;

	// Token: 0x040003DA RID: 986
	public Image deltaStrainbar;

	// Token: 0x040003DB RID: 987
	public Image fallBar;

	// Token: 0x040003DC RID: 988
	public Image jerkBar;

	// Token: 0x040003DD RID: 989
	public bool isTouching;

	// Token: 0x040003DE RID: 990
	public SkinnedMeshRenderer skin;

	// Token: 0x040003DF RID: 991
	public Transform neck;

	// Token: 0x040003E0 RID: 992
	public PoseControl pose;

	// Token: 0x040003E1 RID: 993
	private Saviour saviour;

	// Token: 0x040003E2 RID: 994
	private float blinkTimer;

	// Token: 0x040003E3 RID: 995
	private float mouthTimer;

	// Token: 0x040003E4 RID: 996
	private ContactPoint2D[] contacts;
}
