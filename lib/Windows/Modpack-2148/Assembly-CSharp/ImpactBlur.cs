using System;
using UnityEngine;

// Token: 0x0200004E RID: 78
public class ImpactBlur : MonoBehaviour
{
	// Token: 0x060001C2 RID: 450 RVA: 0x00020AD8 File Offset: 0x0001ECD8
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

	// Token: 0x060001C3 RID: 451 RVA: 0x00020B38 File Offset: 0x0001ED38
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

	// Token: 0x060001C4 RID: 452 RVA: 0x00020C74 File Offset: 0x0001EE74
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

	// Token: 0x040002F1 RID: 753
	public GameObject[] sprites;

	// Token: 0x040002F2 RID: 754
	public float shrinkTime = 0.2f;

	// Token: 0x040002F3 RID: 755
	private MeshRenderer ren;

	// Token: 0x040002F4 RID: 756
	private float[] times;

	// Token: 0x040002F5 RID: 757
	private float[] sizes;
}
