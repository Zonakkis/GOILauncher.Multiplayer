using System;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000D9 RID: 217
public class MB3_MultiMeshBaker : MB3_MeshBakerCommon
{
	// Token: 0x1700008E RID: 142
	// (get) Token: 0x0600060D RID: 1549 RVA: 0x000356DF File Offset: 0x00033ADF
	public override MB3_MeshCombiner meshCombiner
	{
		get
		{
			return this._meshCombiner;
		}
	}

	// Token: 0x0600060E RID: 1550 RVA: 0x000356E8 File Offset: 0x00033AE8
	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
	{
		if (this._meshCombiner.resultSceneObject == null)
		{
			this._meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		this.meshCombiner.name = base.name + "-mesh";
		return this._meshCombiner.AddDeleteGameObjects(gos, deleteGOs, disableRendererInSource);
	}

	// Token: 0x0600060F RID: 1551 RVA: 0x00035754 File Offset: 0x00033B54
	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOs, bool disableRendererInSource)
	{
		if (this._meshCombiner.resultSceneObject == null)
		{
			this._meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		this.meshCombiner.name = base.name + "-mesh";
		return this._meshCombiner.AddDeleteGameObjectsByID(gos, deleteGOs, disableRendererInSource);
	}

	// Token: 0x040004EB RID: 1259
	[SerializeField]
	protected MB3_MultiMeshCombiner _meshCombiner = new MB3_MultiMeshCombiner();
}
