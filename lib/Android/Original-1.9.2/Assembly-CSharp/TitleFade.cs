using System;
using UnityEngine;

// Token: 0x020000FF RID: 255
public class TitleFade : MonoBehaviour
{
	// Token: 0x060006BD RID: 1725 RVA: 0x0003B818 File Offset: 0x00039C18
	private void Start()
	{
		this.rend = base.GetComponent<SpriteRenderer>();
		this.player = GameObject.FindWithTag("Player");
	}

	// Token: 0x060006BE RID: 1726 RVA: 0x0003B836 File Offset: 0x00039C36
	public void Restart()
	{
		this.rend.enabled = true;
		this.rend.color = Color.white;
		this.done = false;
		this.prevLerpAmount = -1f;
	}

	// Token: 0x060006BF RID: 1727 RVA: 0x0003B868 File Offset: 0x00039C68
	private void Update()
	{
		if (this.done)
		{
			return;
		}
		this.pos = this.player.transform.position.x;
		if (this.pos < -42f)
		{
			this.lerpAmount = 0f;
		}
		else if (this.pos > -32f)
		{
			this.lerpAmount = 1f;
		}
		else
		{
			this.lerpAmount = (10f + (32f + this.pos)) / 10f;
		}
		if (this.lerpAmount < this.prevLerpAmount)
		{
			this.lerpAmount = this.prevLerpAmount;
		}
		this.color = Color.Lerp(Color.white, this.whiteClear, this.lerpAmount);
		this.rend.color = this.color;
		if (this.lerpAmount > 1.01f)
		{
			this.done = true;
			this.rend.enabled = false;
		}
		this.prevLerpAmount = this.lerpAmount;
	}

	// Token: 0x04000604 RID: 1540
	private SpriteRenderer rend;

	// Token: 0x04000605 RID: 1541
	private bool done;

	// Token: 0x04000606 RID: 1542
	private GameObject player;

	// Token: 0x04000607 RID: 1543
	private Color color;

	// Token: 0x04000608 RID: 1544
	private float lerpAmount;

	// Token: 0x04000609 RID: 1545
	private float prevLerpAmount = -1f;

	// Token: 0x0400060A RID: 1546
	private float pos;

	// Token: 0x0400060B RID: 1547
	private Color whiteClear = new Color(255f, 255f, 255f, 0f);
}
