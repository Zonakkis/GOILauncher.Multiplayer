using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000CB RID: 203
public abstract class MB3_MeshBakerCommon : MB3_MeshBakerRoot
{
	// Token: 0x1700008B RID: 139
	// (get) Token: 0x060005D0 RID: 1488
	public abstract MB3_MeshCombiner meshCombiner { get; }

	// Token: 0x1700008C RID: 140
	// (get) Token: 0x060005D1 RID: 1489 RVA: 0x000339BE File Offset: 0x00031DBE
	// (set) Token: 0x060005D2 RID: 1490 RVA: 0x000339CB File Offset: 0x00031DCB
	public override MB2_TextureBakeResults textureBakeResults
	{
		get
		{
			return this.meshCombiner.textureBakeResults;
		}
		set
		{
			this.meshCombiner.textureBakeResults = value;
		}
	}

	// Token: 0x060005D3 RID: 1491 RVA: 0x000339DC File Offset: 0x00031DDC
	public override List<GameObject> GetObjectsToCombine()
	{
		if (!this.useObjsToMeshFromTexBaker)
		{
			if (this.objsToMesh == null)
			{
				this.objsToMesh = new List<GameObject>();
			}
			return this.objsToMesh;
		}
		MB3_TextureBaker mb3_TextureBaker = base.gameObject.GetComponent<MB3_TextureBaker>();
		if (mb3_TextureBaker == null)
		{
			mb3_TextureBaker = base.gameObject.transform.parent.GetComponent<MB3_TextureBaker>();
		}
		if (mb3_TextureBaker != null)
		{
			return mb3_TextureBaker.GetObjectsToCombine();
		}
		Debug.LogWarning("Use Objects To Mesh From Texture Baker was checked but no texture baker");
		return new List<GameObject>();
	}

	// Token: 0x060005D4 RID: 1492 RVA: 0x00033A64 File Offset: 0x00031E64
	public void EnableDisableSourceObjectRenderers(bool show)
	{
		for (int i = 0; i < this.GetObjectsToCombine().Count; i++)
		{
			GameObject gameObject = this.GetObjectsToCombine()[i];
			if (gameObject != null)
			{
				Renderer renderer = MB_Utility.GetRenderer(gameObject);
				if (renderer != null)
				{
					renderer.enabled = show;
				}
				LODGroup componentInParent = renderer.GetComponentInParent<LODGroup>();
				if (componentInParent != null)
				{
					bool flag = true;
					LOD[] lods = componentInParent.GetLODs();
					for (int j = 0; j < lods.Length; j++)
					{
						for (int k = 0; k < lods[j].renderers.Length; k++)
						{
							if (lods[j].renderers[k] != renderer)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						componentInParent.enabled = show;
					}
				}
			}
		}
	}

	// Token: 0x060005D5 RID: 1493 RVA: 0x00033B4E File Offset: 0x00031F4E
	public virtual void ClearMesh()
	{
		this.meshCombiner.ClearMesh();
	}

	// Token: 0x060005D6 RID: 1494 RVA: 0x00033B5B File Offset: 0x00031F5B
	public virtual void DestroyMesh()
	{
		this.meshCombiner.DestroyMesh();
	}

	// Token: 0x060005D7 RID: 1495 RVA: 0x00033B68 File Offset: 0x00031F68
	public virtual void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
	{
		this.meshCombiner.DestroyMeshEditor(editorMethods);
	}

	// Token: 0x060005D8 RID: 1496 RVA: 0x00033B76 File Offset: 0x00031F76
	public virtual int GetNumObjectsInCombined()
	{
		return this.meshCombiner.GetNumObjectsInCombined();
	}

	// Token: 0x060005D9 RID: 1497 RVA: 0x00033B83 File Offset: 0x00031F83
	public virtual int GetNumVerticesFor(GameObject go)
	{
		return this.meshCombiner.GetNumVerticesFor(go);
	}

	// Token: 0x060005DA RID: 1498 RVA: 0x00033B94 File Offset: 0x00031F94
	public MB3_TextureBaker GetTextureBaker()
	{
		MB3_TextureBaker component = base.GetComponent<MB3_TextureBaker>();
		if (component != null)
		{
			return component;
		}
		if (base.transform.parent != null)
		{
			return base.transform.parent.GetComponent<MB3_TextureBaker>();
		}
		return null;
	}

	// Token: 0x060005DB RID: 1499
	public abstract bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true);

	// Token: 0x060005DC RID: 1500
	public abstract bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource = true);

	// Token: 0x060005DD RID: 1501 RVA: 0x00033BDE File Offset: 0x00031FDE
	public virtual void Apply(MB3_MeshCombiner.GenerateUV2Delegate uv2GenerationMethod = null)
	{
		this.meshCombiner.name = base.name + "-mesh";
		this.meshCombiner.Apply(uv2GenerationMethod);
	}

	// Token: 0x060005DE RID: 1502 RVA: 0x00033C08 File Offset: 0x00032008
	public virtual void Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool colors, bool bones = false, bool blendShapesFlag = false, MB3_MeshCombiner.GenerateUV2Delegate uv2GenerationMethod = null)
	{
		this.meshCombiner.name = base.name + "-mesh";
		this.meshCombiner.Apply(triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, colors, bones, blendShapesFlag, uv2GenerationMethod);
	}

	// Token: 0x060005DF RID: 1503 RVA: 0x00033C50 File Offset: 0x00032050
	public virtual bool CombinedMeshContains(GameObject go)
	{
		return this.meshCombiner.CombinedMeshContains(go);
	}

	// Token: 0x060005E0 RID: 1504 RVA: 0x00033C60 File Offset: 0x00032060
	public virtual void UpdateGameObjects(GameObject[] gos, bool recalcBounds = true, bool updateVertices = true, bool updateNormals = true, bool updateTangents = true, bool updateUV = false, bool updateUV1 = false, bool updateUV2 = false, bool updateColors = false, bool updateSkinningInfo = false)
	{
		this.meshCombiner.name = base.name + "-mesh";
		this.meshCombiner.UpdateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV1, updateUV2, updateColors, updateSkinningInfo, false);
	}

	// Token: 0x060005E1 RID: 1505 RVA: 0x00033CA5 File Offset: 0x000320A5
	public virtual void UpdateSkinnedMeshApproximateBounds()
	{
		if (this._ValidateForUpdateSkinnedMeshBounds())
		{
			this.meshCombiner.UpdateSkinnedMeshApproximateBounds();
		}
	}

	// Token: 0x060005E2 RID: 1506 RVA: 0x00033CBD File Offset: 0x000320BD
	public virtual void UpdateSkinnedMeshApproximateBoundsFromBones()
	{
		if (this._ValidateForUpdateSkinnedMeshBounds())
		{
			this.meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBones();
		}
	}

	// Token: 0x060005E3 RID: 1507 RVA: 0x00033CD5 File Offset: 0x000320D5
	public virtual void UpdateSkinnedMeshApproximateBoundsFromBounds()
	{
		if (this._ValidateForUpdateSkinnedMeshBounds())
		{
			this.meshCombiner.UpdateSkinnedMeshApproximateBoundsFromBounds();
		}
	}

	// Token: 0x060005E4 RID: 1508 RVA: 0x00033CF0 File Offset: 0x000320F0
	protected virtual bool _ValidateForUpdateSkinnedMeshBounds()
	{
		if (this.meshCombiner.outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
		{
			Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBounds when output type is bakeMeshAssetsInPlace");
			return false;
		}
		if (this.meshCombiner.resultSceneObject == null)
		{
			Debug.LogWarning("Result Scene Object does not exist. No point in calling UpdateSkinnedMeshApproximateBounds.");
			return false;
		}
		SkinnedMeshRenderer componentInChildren = this.meshCombiner.resultSceneObject.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren == null)
		{
			Debug.LogWarning("No SkinnedMeshRenderer on result scene object.");
			return false;
		}
		return true;
	}

	// Token: 0x040004C5 RID: 1221
	public List<GameObject> objsToMesh;

	// Token: 0x040004C6 RID: 1222
	public bool useObjsToMeshFromTexBaker = true;

	// Token: 0x040004C7 RID: 1223
	public bool clearBuffersAfterBake = true;

	// Token: 0x040004C8 RID: 1224
	public string bakeAssetsInPlaceFolderPath;

	// Token: 0x040004C9 RID: 1225
	[HideInInspector]
	public GameObject resultPrefab;
}
