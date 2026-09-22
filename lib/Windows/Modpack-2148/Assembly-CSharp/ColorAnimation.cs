using System;
using UnityEngine;

// Token: 0x0200002C RID: 44
[ExecuteInEditMode]
public class ColorAnimation : MonoBehaviour
{
	// Token: 0x06000128 RID: 296 RVA: 0x0001D20C File Offset: 0x0001B40C
	private void OnEnable()
	{
		this.RandomRangeXYZ.x = global::UnityEngine.Random.Range(0f, 1f);
		this.RandomRangeXYZ.y = global::UnityEngine.Random.Range(0f, 1f);
		this.RandomRangeXYZ.z = global::UnityEngine.Random.Range(0f, 1f);
	}

	// Token: 0x06000129 RID: 297 RVA: 0x0001D268 File Offset: 0x0001B468
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

	// Token: 0x040001FC RID: 508
	private float X;

	// Token: 0x040001FD RID: 509
	private float Y;

	// Token: 0x040001FE RID: 510
	private float Z;

	// Token: 0x040001FF RID: 511
	[Range(0f, 10f)]
	public float _ColorSpeed = 6f;

	// Token: 0x04000200 RID: 512
	private float ColorSpeed;

	// Token: 0x04000201 RID: 513
	private Vector3 RandomRangeXYZ;

	// Token: 0x04000202 RID: 514
	[SerializeField]
	[Range(1f, 300f)]
	private float Intensity = 8f;
}
