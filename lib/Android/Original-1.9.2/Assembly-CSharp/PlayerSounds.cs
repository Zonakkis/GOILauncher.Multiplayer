using System;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x02000137 RID: 311
public class PlayerSounds : MonoBehaviour
{
	// Token: 0x060007FC RID: 2044 RVA: 0x00045070 File Offset: 0x00043470
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

	// Token: 0x060007FD RID: 2045 RVA: 0x0004514C File Offset: 0x0004354C
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
						Debug.Log("falling over 10m");
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

	// Token: 0x060007FE RID: 2046 RVA: 0x0004542C File Offset: 0x0004382C
	private void LateUpdate()
	{
		this.neck.localRotation = Quaternion.Euler(6.784f, 0f, global::UnityEngine.Random.Range(-0.1f * Mathf.Max(0f, this.oldDeltaStrain), 0.1f * Mathf.Max(0f, this.oldDeltaStrain)));
	}

	// Token: 0x060007FF RID: 2047 RVA: 0x00045484 File Offset: 0x00043884
	private void LateFixedUpdate()
	{
	}

	// Token: 0x06000800 RID: 2048 RVA: 0x00045488 File Offset: 0x00043888
	private void FixedUpdate()
	{
		this.deltaVelocity = this.rb.velocity - this.oldVelocity;
		this.smoothDeltaVelocity = Vector2.Lerp(this.smoothDeltaVelocity, this.deltaVelocity, 0.2f);
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

	// Token: 0x06000801 RID: 2049 RVA: 0x00045754 File Offset: 0x00043B54
	private void OnCollisionEnter2D(Collision2D coll)
	{
		coll.GetContacts(this.contactPoints);
		if (coll.relativeVelocity.magnitude > this.impactThreshold && this.contactPoints[0].normal.y > 0.5f)
		{
			this.PlayImpactSound(1f);
		}
		else if (this.rb.GetPoint(this.contactPoints[0].point).y > 0.4f && coll.relativeVelocity.magnitude > this.hurtThreshold)
		{
			this.PlayPainSound(1f);
		}
	}

	// Token: 0x06000802 RID: 2050 RVA: 0x0004580C File Offset: 0x00043C0C
	private void OnCollisionStay2D(Collision2D coll)
	{
		if (coll.gameObject.layer == LayerMask.NameToLayer("Terrain"))
		{
			this.isTouching = true;
		}
	}

	// Token: 0x06000803 RID: 2051 RVA: 0x00045830 File Offset: 0x00043C30
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
		}
		else
		{
			this.lastSoundIndex = global::UnityEngine.Random.Range(0, this.strainSounds.Length);
			this.audioSource.PlayOneShot(this.strainSounds[this.lastSoundIndex], volume);
			this.lastSoundTimer = this.strainSounds[this.lastSoundIndex].length * 1.5f;
		}
	}

	// Token: 0x06000804 RID: 2052 RVA: 0x000458F0 File Offset: 0x00043CF0
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

	// Token: 0x06000805 RID: 2053 RVA: 0x0004596C File Offset: 0x00043D6C
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

	// Token: 0x06000806 RID: 2054 RVA: 0x000459F0 File Offset: 0x00043DF0
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

	// Token: 0x06000807 RID: 2055 RVA: 0x00045A74 File Offset: 0x00043E74
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

	// Token: 0x06000808 RID: 2056 RVA: 0x00045B04 File Offset: 0x00043F04
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

	// Token: 0x06000809 RID: 2057 RVA: 0x00045B88 File Offset: 0x00043F88
	private void PlayGrunt(float volume = 1f)
	{
		if (this.lastSoundTimer > 0f)
		{
			return;
		}
		this.audioSource.PlayOneShot(this.grunts[this.lastSoundIndex], volume);
		this.lastSoundTimer = this.grunts[this.lastSoundIndex].length * 1.5f;
	}

	// Token: 0x0600080A RID: 2058 RVA: 0x00045BE0 File Offset: 0x00043FE0
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

	// Token: 0x0400079C RID: 1948
	public AudioClip[] hardStrainSounds;

	// Token: 0x0400079D RID: 1949
	public AudioClip[] strainSounds;

	// Token: 0x0400079E RID: 1950
	public AudioClip[] releaseSounds;

	// Token: 0x0400079F RID: 1951
	public AudioClip[] painSounds;

	// Token: 0x040007A0 RID: 1952
	public AudioClip[] impactSounds;

	// Token: 0x040007A1 RID: 1953
	public AudioClip[] sadSounds;

	// Token: 0x040007A2 RID: 1954
	public AudioClip[] grunts;

	// Token: 0x040007A3 RID: 1955
	public AudioClip[] jumpSounds;

	// Token: 0x040007A4 RID: 1956
	public AudioClip[] whuSounds;

	// Token: 0x040007A5 RID: 1957
	public HingeJoint2D hj;

	// Token: 0x040007A6 RID: 1958
	public SliderJoint2D sj;

	// Token: 0x040007A7 RID: 1959
	private Rigidbody2D rb;

	// Token: 0x040007A8 RID: 1960
	private Vector2 deltaVelocity;

	// Token: 0x040007A9 RID: 1961
	private Vector2 smoothDeltaVelocity;

	// Token: 0x040007AA RID: 1962
	private Vector2 oldVelocity;

	// Token: 0x040007AB RID: 1963
	private float lastSoundTimer;

	// Token: 0x040007AC RID: 1964
	private AudioSource audioSource;

	// Token: 0x040007AD RID: 1965
	private int lastSoundIndex;

	// Token: 0x040007AE RID: 1966
	public Transform hammerTransform;

	// Token: 0x040007AF RID: 1967
	private Vector2 oldHammerPos;

	// Token: 0x040007B0 RID: 1968
	private float strain;

	// Token: 0x040007B1 RID: 1969
	private float oldStrain;

	// Token: 0x040007B2 RID: 1970
	private float oldDeltaStrain;

	// Token: 0x040007B3 RID: 1971
	private float deltaStrainAccumulator;

	// Token: 0x040007B4 RID: 1972
	private float hurtThreshold = 8f;

	// Token: 0x040007B5 RID: 1973
	private float jumpThreshold = 10f;

	// Token: 0x040007B6 RID: 1974
	private float releaseThreshold = -2f;

	// Token: 0x040007B7 RID: 1975
	private float strainThreshold = 0.6f;

	// Token: 0x040007B8 RID: 1976
	private float hardStrainThreshold = 7f;

	// Token: 0x040007B9 RID: 1977
	private float hardStrainSpeedThreshold = 0.01f;

	// Token: 0x040007BA RID: 1978
	private float impactThreshold = 20f;

	// Token: 0x040007BB RID: 1979
	private float fallTime = 0.2f;

	// Token: 0x040007BC RID: 1980
	private float fallTimer;

	// Token: 0x040007BD RID: 1981
	private bool whuhThisFall;

	// Token: 0x040007BE RID: 1982
	private bool sadThisFall;

	// Token: 0x040007BF RID: 1983
	public LayerMask fallLayer;

	// Token: 0x040007C0 RID: 1984
	public Image strainbar;

	// Token: 0x040007C1 RID: 1985
	public Image deltaStrainbar;

	// Token: 0x040007C2 RID: 1986
	public Image fallBar;

	// Token: 0x040007C3 RID: 1987
	public Image jerkBar;

	// Token: 0x040007C4 RID: 1988
	public bool isTouching;

	// Token: 0x040007C5 RID: 1989
	public SkinnedMeshRenderer skin;

	// Token: 0x040007C6 RID: 1990
	public Transform neck;

	// Token: 0x040007C7 RID: 1991
	public PoseControl pose;

	// Token: 0x040007C8 RID: 1992
	private Saviour saviour;

	// Token: 0x040007C9 RID: 1993
	private float blinkTimer;

	// Token: 0x040007CA RID: 1994
	private float mouthTimer;

	// Token: 0x040007CB RID: 1995
	private ContactPoint2D[] contactPoints = new ContactPoint2D[20];
}
