using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// Token: 0x02000033 RID: 51
public class GravityControl : MonoBehaviour
{
	// Token: 0x0600017A RID: 378 RVA: 0x0000E44D File Offset: 0x0000C64D
	private void Start()
	{
		Physics2D.gravity = new Vector2(0f, -30f);
		this.creditsUp = false;
	}

	// Token: 0x0600017B RID: 379 RVA: 0x0000E46A File Offset: 0x0000C66A
	private void FixedUpdate()
	{
		float y = Physics2D.gravity.y;
	}

	// Token: 0x0600017C RID: 380 RVA: 0x0000E480 File Offset: 0x0000C680
	private void OnTriggerStay2D(Collider2D coll)
	{
		if (coll.attachedRigidbody == null)
		{
			return;
		}
		if (!this.creditsUp)
		{
			foreach (Transform transform in this.gravityWells)
			{
				this.gvec = transform.position - coll.attachedRigidbody.position;
				coll.attachedRigidbody.AddForce(2500f / this.gvec.sqrMagnitude * this.gvec.normalized);
			}
		}
	}

	// Token: 0x0600017D RID: 381 RVA: 0x0000E50A File Offset: 0x0000C70A
	private void OnTriggerEnter2D(Collider2D coll)
	{
		Physics2D.gravity = new Vector2(0f, 0f);
	}

	// Token: 0x0600017E RID: 382 RVA: 0x0000E520 File Offset: 0x0000C720
	private void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.attachedRigidbody == null)
		{
			return;
		}
		if (coll.attachedRigidbody.position.y > base.GetComponent<BoxCollider2D>().bounds.max.y - 5f)
		{
			if (!this.creditsUp)
			{
				Physics2D.gravity = new Vector2(0f, 1.2f);
				Object.Instantiate<GameObject>(this.creditsPrefab, this.creditsParent);
				this.starNest.SetActive(true);
				this.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", 0f);
				base.StartCoroutine("FadeUpStarNest");
				this.creditsUp = true;
				this.fgCam.GetComponent<PostProcessVolume>().profile = this.blurOffProfile;
				this.progressMeter.Pause(true);
				this.settingsMenu.canMenu = false;
				PlayerPrefs.DeleteKey("NumSaves");
				PlayerPrefs.DeleteKey("SaveGame0");
				PlayerPrefs.DeleteKey("SaveGame1");
				PlayerPrefs.Save();
				return;
			}
		}
		else
		{
			Physics2D.gravity = new Vector2(0f, -30f);
		}
	}

	// Token: 0x0600017F RID: 383 RVA: 0x0000E644 File Offset: 0x0000C844
	private IEnumerator FadeUpStarNest()
	{
		float step = 2.0000001E-05f;
		for (float f = 0f; f <= 0.01f; f += step)
		{
			this.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", f);
			yield return null;
		}
		yield break;
	}

	// Token: 0x0400024A RID: 586
	public Transform[] gravityWells;

	// Token: 0x0400024B RID: 587
	private Vector2 gvec;

	// Token: 0x0400024C RID: 588
	public GameObject creditsPrefab;

	// Token: 0x0400024D RID: 589
	public Transform creditsParent;

	// Token: 0x0400024E RID: 590
	private bool creditsUp;

	// Token: 0x0400024F RID: 591
	public ProgressMeter progressMeter;

	// Token: 0x04000250 RID: 592
	public GameObject starNest;

	// Token: 0x04000251 RID: 593
	public PostProcessProfile blurOffProfile;

	// Token: 0x04000252 RID: 594
	public SettingsManager settingsMenu;

	// Token: 0x04000253 RID: 595
	public Camera fgCam;
}
