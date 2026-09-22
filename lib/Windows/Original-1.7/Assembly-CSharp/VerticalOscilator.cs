using System;
using UnityEngine;

// Token: 0x0200000C RID: 12
public class VerticalOscilator : MonoBehaviour
{
	// Token: 0x0600003B RID: 59 RVA: 0x00003325 File Offset: 0x00001525
	private void Start()
	{
		this._Position = base.transform.position;
		this._Random = Random.Range(0.1f, 10f);
	}

	// Token: 0x0600003C RID: 60 RVA: 0x00003350 File Offset: 0x00001550
	private void Update()
	{
		this._Time += Time.deltaTime * this.Speed;
		this._Position.y = Mathf.Sin(this._Time + this._Random) * this.Amplitude + base.transform.position.y;
		base.gameObject.transform.position = this._Position;
	}

	// Token: 0x04000044 RID: 68
	private Vector3 _Position;

	// Token: 0x04000045 RID: 69
	private float _Time;

	// Token: 0x04000046 RID: 70
	private float _Random;

	// Token: 0x04000047 RID: 71
	[Range(0f, 1f)]
	public float Speed = 1f;

	// Token: 0x04000048 RID: 72
	[Range(0f, 1f)]
	public float Amplitude = 1f;
}
