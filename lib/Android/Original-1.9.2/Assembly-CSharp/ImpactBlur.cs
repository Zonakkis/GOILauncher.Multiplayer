using System;
using UnityEngine;

// Token: 0x0200012D RID: 301
public class ImpactBlur : MonoBehaviour
{
	// Token: 0x060007A3 RID: 1955 RVA: 0x00040EB4 File Offset: 0x0003F2B4
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

	// Token: 0x060007A4 RID: 1956 RVA: 0x00040F1C File Offset: 0x0003F31C
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

	// Token: 0x060007A5 RID: 1957 RVA: 0x00041060 File Offset: 0x0003F460
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
				break;
			}
		}
	}

	// Token: 0x04000707 RID: 1799
	public GameObject[] sprites;

	// Token: 0x04000708 RID: 1800
	public float shrinkTime = 0.2f;

	// Token: 0x04000709 RID: 1801
	private MeshRenderer ren;

	// Token: 0x0400070A RID: 1802
	private float[] times;

	// Token: 0x0400070B RID: 1803
	private float[] sizes;
}
