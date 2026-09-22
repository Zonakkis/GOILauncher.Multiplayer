using System;
using UnityEngine;

// Token: 0x02000019 RID: 25
[ExecuteInEditMode]
public class ColorAnimation : MonoBehaviour
{
	// Token: 0x06000106 RID: 262 RVA: 0x0000BAB8 File Offset: 0x00009CB8
	private void OnEnable()
	{
		this.RandomRangeXYZ.x = Random.Range(0f, 1f);
		this.RandomRangeXYZ.y = Random.Range(0f, 1f);
		this.RandomRangeXYZ.z = Random.Range(0f, 1f);
	}

	// Token: 0x06000107 RID: 263 RVA: 0x0000BB14 File Offset: 0x00009D14
	private void Update()
	{
		this.ColorSpeed += Time.deltaTime * this._ColorSpeed;
		this.X = Mathf.Sin(this.ColorSpeed * this.RandomRangeXYZ.x) * 0.5f + 0.5f;
		this.Y = Mathf.Sin(this.ColorSpeed * this.RandomRangeXYZ.y) * 0.5f + 0.5f;
		this.Z = Mathf.Sin(this.ColorSpeed * this.RandomRangeXYZ.z) * 0.5f + 0.5f;
		float num = Mathf.Sin(this.ColorSpeed * this.RandomRangeXYZ.z) * 0.5f + 0.5f;
		Color color = new Color(this.X, this.Y, this.Z, 1f);
		base.GetComponent<Renderer>().sharedMaterial.SetColor("_EmissionColor", num * this.Intensity * color);
	}

	// Token: 0x040001AF RID: 431
	private float X;

	// Token: 0x040001B0 RID: 432
	private float Y;

	// Token: 0x040001B1 RID: 433
	private float Z;

	// Token: 0x040001B2 RID: 434
	[Range(0f, 10f)]
	public float _ColorSpeed = 6f;

	// Token: 0x040001B3 RID: 435
	private float ColorSpeed;

	// Token: 0x040001B4 RID: 436
	private Vector3 RandomRangeXYZ;

	// Token: 0x040001B5 RID: 437
	[SerializeField]
	[Range(1f, 300f)]
	private float Intensity = 8f;
}
