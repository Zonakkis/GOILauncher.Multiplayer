using System;
using UnityEngine;

// Token: 0x0200000E RID: 14
public class VerticalOscilator : MonoBehaviour
{
	// Token: 0x0600003E RID: 62 RVA: 0x000028C9 File Offset: 0x00000AC9
	private void Start()
	{
		this._Position = base.transform.position;
		this._Random = global::UnityEngine.Random.Range(0.1f, 10f);
	}

	// Token: 0x0600003F RID: 63 RVA: 0x0001512C File Offset: 0x0001332C
	private void Update()
	{
		this._Time += Time.deltaTime * this.Speed;
		this._Position.y = Mathf.Sin(this._Time + this._Random) * this.Amplitude + base.transform.position.y;
		base.gameObject.transform.position = this._Position;
	}

	// Token: 0x04000048 RID: 72
	private Vector3 _Position;

	// Token: 0x04000049 RID: 73
	private float _Time;

	// Token: 0x0400004A RID: 74
	private float _Random;

	// Token: 0x0400004B RID: 75
	[Range(0f, 1f)]
	public float Speed = 1f;

	// Token: 0x0400004C RID: 76
	[Range(0f, 1f)]
	public float Amplitude = 1f;
}
