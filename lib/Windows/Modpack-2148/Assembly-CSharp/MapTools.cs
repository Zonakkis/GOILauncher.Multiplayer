using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020002E6 RID: 742
public static class MapTools
{
	// Token: 0x06001928 RID: 6440 RVA: 0x00078620 File Offset: 0x00076820
	public static bool createArrow(Vector3 pos, float angle, bool colliders)
	{
		try
		{
			GameObject gameObject = GameObject.Find("Props").transform.Find("Orange").gameObject;
			if (gameObject != null)
			{
				GameObject gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, pos, Quaternion.identity);
				gameObject2.GetComponent<Rigidbody2D>().isKinematic = true;
				if (!colliders)
				{
					gameObject2.GetComponent<Collider2D>().enabled = false;
				}
				for (int i = 1; i < 9; i++)
				{
					Vector3 vector = pos + new Vector3(-0.3f * (float)i, 0f, 0f);
					Vector3 vector2 = Quaternion.Euler(new Vector3(0f, 0f, angle)) * (vector - pos) + pos;
					gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, vector2, Quaternion.identity);
					gameObject2.GetComponent<Rigidbody2D>().isKinematic = true;
					if (!colliders)
					{
						gameObject2.GetComponent<Collider2D>().enabled = false;
					}
					for (int j = 1; j <= ((i < 4) ? i : 1); j++)
					{
						vector = pos + new Vector3(-0.3f * (float)i, 0.3f * (float)j, 0f);
						vector2 = Quaternion.Euler(new Vector3(0f, 0f, angle)) * (vector - pos) + pos;
						gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, vector2, Quaternion.identity);
						gameObject2.GetComponent<Rigidbody2D>().isKinematic = true;
						if (!colliders)
						{
							gameObject2.GetComponent<Collider2D>().enabled = false;
						}
						vector = pos + new Vector3(-0.3f * (float)i, -0.3f * (float)j, 0f);
						vector2 = Quaternion.Euler(new Vector3(0f, 0f, angle)) * (vector - pos) + pos;
						gameObject2 = global::UnityEngine.Object.Instantiate<GameObject>(gameObject, vector2, Quaternion.identity);
						gameObject2.GetComponent<Rigidbody2D>().isKinematic = true;
						if (!colliders)
						{
							gameObject2.GetComponent<Collider2D>().enabled = false;
						}
					}
				}
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	// Token: 0x06001929 RID: 6441 RVA: 0x00078834 File Offset: 0x00076A34
	public static bool duplicateChilds(GameObject child, Vector3 diff)
	{
		try
		{
			foreach (object obj in child.gameObject.transform)
			{
				Transform transform = (Transform)obj;
				MapTools.duplicateChilds(global::UnityEngine.Object.Instantiate<GameObject>(transform.gameObject, transform.transform.position + diff, Quaternion.identity), diff);
			}
			return true;
		}
		catch
		{
		}
		return false;
	}

	// Token: 0x0600192A RID: 6442 RVA: 0x000788CC File Offset: 0x00076ACC
	public static bool newThumb(Vector3 old)
	{
		try
		{
			GameObject gameObject = GameObject.Find("Rigged Hand");
			if (gameObject != null)
			{
				return MapTools.duplicateChilds(global::UnityEngine.Object.Instantiate<GameObject>(gameObject.gameObject, old, Quaternion.identity), old - gameObject.gameObject.transform.position);
			}
		}
		catch
		{
		}
		return false;
	}

	// Token: 0x0600192B RID: 6443 RVA: 0x00078934 File Offset: 0x00076B34
	public static void CreateBox(string name, Rect rect)
	{
		LineRenderer lineRenderer = new GameObject(name).AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
		lineRenderer.SetColors(Color.white, Color.white);
		lineRenderer.useWorldSpace = true;
		lineRenderer.SetVertexCount(5);
		lineRenderer.SetWidth(0.03f, 0.03f);
		lineRenderer.SetPosition(0, new Vector3(rect.x, rect.y, 0f));
		lineRenderer.SetPosition(1, new Vector3(rect.x + rect.width, rect.y, 0f));
		lineRenderer.SetPosition(2, new Vector3(rect.x + rect.width, rect.y + rect.height, 0f));
		lineRenderer.SetPosition(3, new Vector3(rect.x, rect.y + rect.height, 0f));
		lineRenderer.SetPosition(4, new Vector3(rect.x, rect.y, 0f));
	}

	// Token: 0x020002E7 RID: 743
	public class TeleportSaveState
	{
		// Token: 0x170006B5 RID: 1717
		// (get) Token: 0x0600192C RID: 6444 RVA: 0x00012F66 File Offset: 0x00011166
		// (set) Token: 0x0600192D RID: 6445 RVA: 0x00012F6E File Offset: 0x0001116E
		public float hingePos { get; set; }

		// Token: 0x170006B6 RID: 1718
		// (get) Token: 0x0600192E RID: 6446 RVA: 0x00012F77 File Offset: 0x00011177
		// (set) Token: 0x0600192F RID: 6447 RVA: 0x00012F7F File Offset: 0x0001117F
		public float sliderPos { get; set; }

		// Token: 0x170006B7 RID: 1719
		// (get) Token: 0x06001930 RID: 6448 RVA: 0x00012F88 File Offset: 0x00011188
		// (set) Token: 0x06001931 RID: 6449 RVA: 0x00012F90 File Offset: 0x00011190
		public float[] rbAngles { get; set; }

		// Token: 0x170006B8 RID: 1720
		// (get) Token: 0x06001932 RID: 6450 RVA: 0x00012F99 File Offset: 0x00011199
		// (set) Token: 0x06001933 RID: 6451 RVA: 0x00012FA1 File Offset: 0x000111A1
		public float[] playerPos { get; set; }

		// Token: 0x170006B9 RID: 1721
		// (get) Token: 0x06001934 RID: 6452 RVA: 0x00012FAA File Offset: 0x000111AA
		// (set) Token: 0x06001935 RID: 6453 RVA: 0x00012FB2 File Offset: 0x000111B2
		public float[] playerRot { get; set; }

		// Token: 0x170006BA RID: 1722
		// (get) Token: 0x06001936 RID: 6454 RVA: 0x00012FBB File Offset: 0x000111BB
		// (set) Token: 0x06001937 RID: 6455 RVA: 0x00012FC3 File Offset: 0x000111C3
		public float[][] rbPositions { get; set; }

		// Token: 0x170006BB RID: 1723
		// (get) Token: 0x06001938 RID: 6456 RVA: 0x00012FCC File Offset: 0x000111CC
		// (set) Token: 0x06001939 RID: 6457 RVA: 0x00012FD4 File Offset: 0x000111D4
		public float[] camPos { get; set; }
	}

	// Token: 0x020002E8 RID: 744
	public class MapConfig
	{
		// Token: 0x170006BC RID: 1724
		// (get) Token: 0x0600193B RID: 6459 RVA: 0x00012FE5 File Offset: 0x000111E5
		// (set) Token: 0x0600193C RID: 6460 RVA: 0x00012FED File Offset: 0x000111ED
		public Dictionary<string, float[]> Splits { get; set; }

		// Token: 0x170006BD RID: 1725
		// (get) Token: 0x0600193D RID: 6461 RVA: 0x00012FF6 File Offset: 0x000111F6
		// (set) Token: 0x0600193E RID: 6462 RVA: 0x00012FFE File Offset: 0x000111FE
		public Dictionary<string, MapTools.TeleportSaveState> SaveStates { get; set; }
	}
}
