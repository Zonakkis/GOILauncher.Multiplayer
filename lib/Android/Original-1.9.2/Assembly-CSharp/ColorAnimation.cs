using System;
using UnityEngine;

// Token: 0x02000022 RID: 34
[ExecuteInEditMode]
public class ColorAnimation : MonoBehaviour
{
	// Token: 0x060000F5 RID: 245 RVA: 0x0000B6A8 File Offset: 0x00009AA8
	private void OnEnable()
	{
		this.RandomRangeXYZ.x = global::UnityEngine.Random.Range(0f, 1f);
		this.RandomRangeXYZ.y = global::UnityEngine.Random.Range(0f, 1f);
		this.RandomRangeXYZ.z = global::UnityEngine.Random.Range(0f, 1f);
	}

	// Token: 0x060000F6 RID: 246 RVA: 0x0000B704 File Offset: 0x00009B04
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

	// Token: 0x040001C7 RID: 455
	private float X;

	// Token: 0x040001C8 RID: 456
	private float Y;

	// Token: 0x040001C9 RID: 457
	private float Z;

	// Token: 0x040001CA RID: 458
	[Range(0f, 10f)]
	public float _ColorSpeed = 6f;

	// Token: 0x040001CB RID: 459
	private float ColorSpeed;

	// Token: 0x040001CC RID: 460
	private Vector3 RandomRangeXYZ;

	// Token: 0x040001CD RID: 461
	[SerializeField]
	[Range(1f, 300f)]
	private float Intensity = 8f;
}
