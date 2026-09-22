using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Token: 0x0200003A RID: 58
public class BatGen : MonoBehaviour
{
	// Token: 0x06000163 RID: 355 RVA: 0x0000346C File Offset: 0x0000166C
	private void Start()
	{
		this.done = false;
		this.batsSpawned = 0;
		this.source = base.GetComponent<AudioSource>();
	}

	// Token: 0x06000164 RID: 356 RVA: 0x00003488 File Offset: 0x00001688
	private void Update()
	{
		if (!this.done)
		{
			return;
		}
		if (base.transform.childCount <= 1)
		{
			this.batCam.gameObject.SetActive(false);
			global::UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	// Token: 0x06000165 RID: 357 RVA: 0x000034BD File Offset: 0x000016BD
	private IEnumerator Spawn()
	{
		while (this.batsSpawned < this.numBats)
		{
			for (int i = 0; i < 3; i++)
			{
				global::UnityEngine.Object.Instantiate<GameObject>(this.batPrefab, base.transform.position + global::UnityEngine.Random.insideUnitCircle, Quaternion.identity, base.transform).GetComponent<BatControl>().mainCam = this.batCam;
				this.batsSpawned++;
			}
			yield return null;
		}
		yield break;
	}

	// Token: 0x06000166 RID: 358 RVA: 0x0001E64C File Offset: 0x0001C84C
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

	// Token: 0x04000255 RID: 597
	public GameObject batPrefab;

	// Token: 0x04000256 RID: 598
	private bool done;

	// Token: 0x04000257 RID: 599
	private int numBats = 210;

	// Token: 0x04000258 RID: 600
	private int batsSpawned;

	// Token: 0x04000259 RID: 601
	private AudioSource source;

	// Token: 0x0400025A RID: 602
	public AudioMixerGroup DistantGroup;

	// Token: 0x0400025B RID: 603
	public Camera batCam;
}
