using System;
using System.Linq;
using UnityEngine;

// Token: 0x02000101 RID: 257
public class CheatDragRuntime : MonoBehaviour
{
	// Token: 0x060006C5 RID: 1733 RVA: 0x0003BB38 File Offset: 0x00039F38
	private void Update()
	{
		if (Input.touchCount > 1)
		{
			if (!this.isDragging)
			{
				this.player.GetComponent<Rigidbody2D>().gravityScale = 0f;
				this.fingerId = new int?(Input.GetTouch(1).fingerId);
			}
			this.isDragging = true;
			Touch touch = Input.touches.FirstOrDefault<Touch>((Touch x) => x.fingerId == this.fingerId);
			if (touch.fingerId != this.fingerId)
			{
				return;
			}
			Vector3 position = this.player.transform.position;
			this.targetPlayerWorldPoint = Camera.main.ScreenToWorldPoint(touch.position);
			this.targetPlayerWorldPoint = new Vector3(this.targetPlayerWorldPoint.x, this.targetPlayerWorldPoint.y, position.z);
		}
		else
		{
			if (Input.touchCount == 1)
			{
				int? num = this.fingerId;
				if (num != null && Input.touches.Any<Touch>((Touch x) => x.fingerId == this.fingerId))
				{
					Touch touch2 = Input.touches.FirstOrDefault<Touch>((Touch x) => x.fingerId == this.fingerId);
					if (touch2.fingerId != this.fingerId)
					{
						return;
					}
					Vector3 position2 = this.player.transform.position;
					this.targetPlayerWorldPoint = Camera.main.ScreenToWorldPoint(touch2.position);
					this.targetPlayerWorldPoint = new Vector3(this.targetPlayerWorldPoint.x, this.targetPlayerWorldPoint.y, position2.z);
					return;
				}
			}
			if (this.isDragging)
			{
				this.player.GetComponent<Rigidbody2D>().gravityScale = 1f;
			}
			this.isDragging = false;
			this.fingerId = null;
		}
	}

	// Token: 0x060006C6 RID: 1734 RVA: 0x0003BD3C File Offset: 0x0003A13C
	private void FixedUpdate()
	{
		if (this.isDragging)
		{
			Vector3 position = this.player.transform.position;
			Vector3 vector = Vector3.Lerp(position, this.targetPlayerWorldPoint, Time.fixedDeltaTime * this.Power);
			this.player.GetComponent<Rigidbody2D>().MovePosition(vector);
		}
	}

	// Token: 0x0400060E RID: 1550
	public PlayerControl player;

	// Token: 0x0400060F RID: 1551
	public float Power = 2f;

	// Token: 0x04000610 RID: 1552
	public bool isDragging;

	// Token: 0x04000611 RID: 1553
	private Vector3 targetPlayerWorldPoint;

	// Token: 0x04000612 RID: 1554
	private int? fingerId;
}
