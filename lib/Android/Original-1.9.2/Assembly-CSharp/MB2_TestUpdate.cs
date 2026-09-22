using System;
using UnityEngine;

// Token: 0x02000079 RID: 121
public class MB2_TestUpdate : MonoBehaviour
{
	// Token: 0x06000375 RID: 885 RVA: 0x0001B498 File Offset: 0x00019898
	private void Start()
	{
		this.meshbaker.AddDeleteGameObjects(this.objsToMove, null, true);
		this.meshbaker.AddDeleteGameObjects(new GameObject[] { this.objWithChangingUVs }, null, true);
		MeshFilter meshFilter = this.objWithChangingUVs.GetComponent<MeshFilter>();
		this.m = meshFilter.sharedMesh;
		this.uvs = this.m.uv;
		this.meshbaker.Apply(null);
		this.multiMeshBaker.AddDeleteGameObjects(this.objsToMove, null, true);
		this.multiMeshBaker.AddDeleteGameObjects(new GameObject[] { this.objWithChangingUVs }, null, true);
		meshFilter = this.objWithChangingUVs.GetComponent<MeshFilter>();
		this.m = meshFilter.sharedMesh;
		this.uvs = this.m.uv;
		this.multiMeshBaker.Apply(null);
	}

	// Token: 0x06000376 RID: 886 RVA: 0x0001B574 File Offset: 0x00019974
	private void LateUpdate()
	{
		this.meshbaker.UpdateGameObjects(this.objsToMove, false, true, true, true, false, false, false, false, false);
		Vector2[] array = this.m.uv;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Mathf.Sin(Time.time) * this.uvs[i];
		}
		this.m.uv = array;
		this.meshbaker.UpdateGameObjects(new GameObject[] { this.objWithChangingUVs }, true, true, true, true, true, false, false, false, false);
		this.meshbaker.Apply(false, true, true, true, true, false, false, false, false, false, false, null);
		this.multiMeshBaker.UpdateGameObjects(this.objsToMove, false, true, true, true, false, false, false, false, false);
		array = this.m.uv;
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = Mathf.Sin(Time.time) * this.uvs[j];
		}
		this.m.uv = array;
		this.multiMeshBaker.UpdateGameObjects(new GameObject[] { this.objWithChangingUVs }, true, true, true, true, true, false, false, false, false);
		this.multiMeshBaker.Apply(false, true, true, true, true, false, false, false, false, false, false, null);
	}

	// Token: 0x04000354 RID: 852
	public MB3_MeshBaker meshbaker;

	// Token: 0x04000355 RID: 853
	public MB3_MultiMeshBaker multiMeshBaker;

	// Token: 0x04000356 RID: 854
	public GameObject[] objsToMove;

	// Token: 0x04000357 RID: 855
	public GameObject objWithChangingUVs;

	// Token: 0x04000358 RID: 856
	private Vector2[] uvs;

	// Token: 0x04000359 RID: 857
	private Mesh m;
}
