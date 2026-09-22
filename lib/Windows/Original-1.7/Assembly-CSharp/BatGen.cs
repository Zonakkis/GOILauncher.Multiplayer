using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Token: 0x02000026 RID: 38
public class BatGen : MonoBehaviour
{
	// Token: 0x06000141 RID: 321 RVA: 0x0000D287 File Offset: 0x0000B487
	private void Start()
	{
		this.done = false;
		this.batsSpawned = 0;
		this.source = base.GetComponent<AudioSource>();
	}

	// Token: 0x06000142 RID: 322 RVA: 0x0000D2A3 File Offset: 0x0000B4A3
	private void Update()
	{
		if (!this.done)
		{
			return;
		}
		if (base.transform.childCount <= 1)
		{
			this.batCam.gameObject.SetActive(false);
			Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000143 RID: 323 RVA: 0x0000D2D8 File Offset: 0x0000B4D8
	private IEnumerator Spawn()
	{
		while (this.batsSpawned < this.numBats)
		{
			for (int i = 0; i < 3; i++)
			{
				Object.Instantiate<GameObject>(this.batPrefab, base.transform.position + Random.insideUnitCircle, Quaternion.identity, base.transform).GetComponent<BatControl>().mainCam = this.batCam;
				this.batsSpawned++;
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x06000144 RID: 324 RVA: 0x0000D2E8 File Offset: 0x0000B4E8
	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (this.done)
		{
			return;
		}
		if (coll.name != "PotCollider")
		{
			return;
		}
		this.done = true;
		this.batCam.gameObject.SetActive(true);
		base.StartCoroutine("Spawn");
		int @int = PlayerPrefs.GetInt("NumWins");
		if (@int > 0)
		{
			this.source.outputAudioMixerGroup = this.DistantGroup;
		}
		this.source.Play();
	}

	// Token: 0x04000202 RID: 514
	public GameObject batPrefab;

	// Token: 0x04000203 RID: 515
	private bool done;

	// Token: 0x04000204 RID: 516
	private int numBats = 210;

	// Token: 0x04000205 RID: 517
	private int batsSpawned;

	// Token: 0x04000206 RID: 518
	private AudioSource source;

	// Token: 0x04000207 RID: 519
	public AudioMixerGroup DistantGroup;

	// Token: 0x04000208 RID: 520
	public Camera batCam;
}
