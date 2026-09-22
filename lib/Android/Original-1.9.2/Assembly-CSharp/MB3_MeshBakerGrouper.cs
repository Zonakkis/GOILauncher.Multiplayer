using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000CC RID: 204
public class MB3_MeshBakerGrouper : MonoBehaviour
{
	// Token: 0x060005E6 RID: 1510 RVA: 0x00033E2C File Offset: 0x0003222C
	private void OnDrawGizmosSelected()
	{
		if (this.grouper == null)
		{
			this.grouper = this.CreateGrouper(this.clusterType, this.data);
		}
		if (this.grouper.d == null)
		{
			this.grouper.d = this.data;
		}
		this.grouper.DrawGizmos(this.sourceObjectBounds);
	}

	// Token: 0x060005E7 RID: 1511 RVA: 0x00033E90 File Offset: 0x00032290
	public MB3_MeshBakerGrouperCore CreateGrouper(MB3_MeshBakerGrouper.ClusterType t, GrouperData data)
	{
		if (t == MB3_MeshBakerGrouper.ClusterType.grid)
		{
			this.grouper = new MB3_MeshBakerGrouperGrid(data);
		}
		if (t == MB3_MeshBakerGrouper.ClusterType.pie)
		{
			this.grouper = new MB3_MeshBakerGrouperPie(data);
		}
		if (t == MB3_MeshBakerGrouper.ClusterType.agglomerative)
		{
			MB3_TextureBaker component = base.GetComponent<MB3_TextureBaker>();
			List<GameObject> list;
			if (component != null)
			{
				list = component.GetObjectsToCombine();
			}
			else
			{
				list = new List<GameObject>();
			}
			this.grouper = new MB3_MeshBakerGrouperCluster(data, list);
		}
		if (t == MB3_MeshBakerGrouper.ClusterType.none)
		{
			this.grouper = new MB3_MeshBakerGrouperNone(data);
		}
		return this.grouper;
	}

	// Token: 0x040004CA RID: 1226
	public MB3_MeshBakerGrouperCore grouper;

	// Token: 0x040004CB RID: 1227
	public MB3_MeshBakerGrouper.ClusterType clusterType;

	// Token: 0x040004CC RID: 1228
	public GrouperData data = new GrouperData();

	// Token: 0x040004CD RID: 1229
	[HideInInspector]
	public Bounds sourceObjectBounds = new Bounds(Vector3.zero, Vector3.one);

	// Token: 0x020000CD RID: 205
	public enum ClusterType
	{
		// Token: 0x040004CF RID: 1231
		none,
		// Token: 0x040004D0 RID: 1232
		grid,
		// Token: 0x040004D1 RID: 1233
		pie,
		// Token: 0x040004D2 RID: 1234
		agglomerative
	}
}
