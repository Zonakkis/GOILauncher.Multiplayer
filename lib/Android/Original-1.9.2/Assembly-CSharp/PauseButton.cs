using System;
using UnityEngine;

// Token: 0x020000F8 RID: 248
public class PauseButton : MonoBehaviour
{
	// Token: 0x060006A6 RID: 1702 RVA: 0x0003AC4C File Offset: 0x0003904C
	private void Start()
	{
		if (this.settingsMan == null)
		{
			this.settingsMan = GameObject.FindGameObjectWithTag("SettingsManager").GetComponent<SettingsManager>();
		}
		Vector3 vector = Camera.main.ScreenToWorldPoint(default(Vector2));
		vector.x += 1f;
		vector.y = base.transform.position.y;
		vector.z = base.transform.position.z;
		base.transform.position = vector;
		this.layerMask = LayerMask.GetMask(new string[] { "Pause" });
		this.originalButtonScale = base.transform.localScale;
	}

	// Token: 0x060006A7 RID: 1703 RVA: 0x0003AD18 File Offset: 0x00039118
	private void Update()
	{
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out this.hit, 100f, this.layerMask))
			{
				Debug.Log("[debug] OnMouseDown");
				this.wasTouched = true;
				base.transform.localScale = this.originalButtonScale * 1.2f;
			}
		}
		else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended && this.wasTouched)
		{
			Debug.Log("[debug] OnMouseUp");
			this.wasTouched = false;
			base.transform.localScale = this.originalButtonScale;
			if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out this.hit, 100f, this.layerMask))
			{
				Debug.Log("[debug] ToggleMenu");
				this.settingsMan.ToggleMenu();
			}
		}
	}

	// Token: 0x040005CF RID: 1487
	public SettingsManager settingsMan;

	// Token: 0x040005D0 RID: 1488
	private RaycastHit hit;

	// Token: 0x040005D1 RID: 1489
	private Ray ray;

	// Token: 0x040005D2 RID: 1490
	private int layerMask;

	// Token: 0x040005D3 RID: 1491
	private Vector3 originalButtonScale;

	// Token: 0x040005D4 RID: 1492
	private bool wasTouched;
}
