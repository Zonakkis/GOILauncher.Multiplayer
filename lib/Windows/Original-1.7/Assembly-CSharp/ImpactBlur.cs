using System;
using UnityEngine;

// Token: 0x02000037 RID: 55
public class ImpactBlur : MonoBehaviour
{
	// Token: 0x06000192 RID: 402 RVA: 0x0000F810 File Offset: 0x0000DA10
	private void Start()
	{
		this.times = new float[this.sprites.Length];
		this.sizes = new float[this.sprites.Length];
		for (int i = 0; i < this.times.Length; i++)
		{
			this.times[i] = 0f;
			this.sizes[i] = 0f;
		}
	}

	// Token: 0x06000193 RID: 403 RVA: 0x0000F870 File Offset: 0x0000DA70
	private void Update()
	{
		for (int i = 0; i < this.sprites.Length; i++)
		{
			if (this.sprites[i].GetComponent<MeshRenderer>().enabled)
			{
				this.times[i] -= Time.deltaTime;
				float num = (1f - this.times[i] / this.shrinkTime * (this.times[i] / this.shrinkTime)) * this.sizes[i];
				this.sprites[i].transform.localScale = new Vector3(num, num, num);
				this.sprites[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Tint", this.times[i] / this.shrinkTime);
				if (this.times[i] <= 0f || this.sprites[i].transform.localScale.x < 0.03f)
				{
					this.times[i] = 0f;
					this.sprites[i].GetComponent<MeshRenderer>().enabled = false;
					this.sprites[i].transform.localScale = new Vector3(1f, 1f, 1f);
				}
			}
		}
	}

	// Token: 0x06000194 RID: 404 RVA: 0x0000F9AC File Offset: 0x0000DBAC
	public void Impact(Vector2 pos, float size)
	{
		for (int i = 0; i < this.sprites.Length; i++)
		{
			if (!this.sprites[i].GetComponent<MeshRenderer>().enabled)
			{
				this.sprites[i].GetComponent<MeshRenderer>().enabled = true;
				this.sprites[i].GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Tint", 1f);
				this.sprites[i].transform.position = new Vector3(pos.x, pos.y, -7f);
				this.times[i] = this.shrinkTime;
				this.sizes[i] = size;
				this.sprites[i].transform.localScale = new Vector3(0.1f * size, 0.1f * size, 0.1f * size);
				return;
			}
		}
	}

	// Token: 0x0400028B RID: 651
	public GameObject[] sprites;

	// Token: 0x0400028C RID: 652
	public float shrinkTime = 0.2f;

	// Token: 0x0400028D RID: 653
	private MeshRenderer ren;

	// Token: 0x0400028E RID: 654
	private float[] times;

	// Token: 0x0400028F RID: 655
	private float[] sizes;
}
