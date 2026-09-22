using System;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000CA RID: 202
public class MB3_MeshBaker : MB3_MeshBakerCommon
{
	// Token: 0x1700008A RID: 138
	// (get) Token: 0x060005C9 RID: 1481 RVA: 0x00033D79 File Offset: 0x00032179
	public override MB3_MeshCombiner meshCombiner
	{
		get
		{
			return this._meshCombiner;
		}
	}

	// Token: 0x060005CA RID: 1482 RVA: 0x00033D81 File Offset: 0x00032181
	public void BuildSceneMeshObject()
	{
		this._meshCombiner.BuildSceneMeshObject(null, false);
	}

	// Token: 0x060005CB RID: 1483 RVA: 0x00033D90 File Offset: 0x00032190
	public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs)
	{
		return this._meshCombiner.ShowHideGameObjects(gos, deleteGOs);
	}

	// Token: 0x060005CC RID: 1484 RVA: 0x00033D9F File Offset: 0x0003219F
	public virtual void ApplyShowHide()
	{
		this._meshCombiner.ApplyShowHide();
	}

	// Token: 0x060005CD RID: 1485 RVA: 0x00033DAC File Offset: 0x000321AC
	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		this._meshCombiner.name = base.name + "-mesh";
		return this._meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	// Token: 0x060005CE RID: 1486 RVA: 0x00033DD7 File Offset: 0x000321D7
	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
	{
		this._meshCombiner.name = base.name + "-mesh";
		return this._meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOinstanceIDs, disableRendererInSource);
	}

	// Token: 0x040004C4 RID: 1220
	[SerializeField]
	protected MB3_MeshCombinerSingle _meshCombiner = new MB3_MeshCombinerSingle();
}
