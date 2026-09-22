using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000D5 RID: 213
public abstract class MB3_MeshBakerRoot : MonoBehaviour
{
	// Token: 0x1700008D RID: 141
	// (get) Token: 0x06000602 RID: 1538
	// (set) Token: 0x06000603 RID: 1539
	[HideInInspector]
	public abstract MB2_TextureBakeResults textureBakeResults { get; set; }

	// Token: 0x06000604 RID: 1540 RVA: 0x000335C1 File Offset: 0x000319C1
	public virtual List<GameObject> GetObjectsToCombine()
	{
		return null;
	}

	// Token: 0x06000605 RID: 1541 RVA: 0x000335C4 File Offset: 0x000319C4
	public static bool DoCombinedValidate(MB3_MeshBakerRoot mom, MB_ObjsToCombineTypes objToCombineType, MB2_EditorMethodsInterface editorMethods, MB2_ValidationLevel validationLevel)
	{
		if (mom.textureBakeResults == null)
		{
			Debug.LogError("Need to set Texture Bake Result on " + mom);
			return false;
		}
		if (mom is MB3_MeshBakerCommon)
		{
			MB3_MeshBakerCommon mb3_MeshBakerCommon = (MB3_MeshBakerCommon)mom;
			MB3_TextureBaker textureBaker = mb3_MeshBakerCommon.GetTextureBaker();
			if (textureBaker != null && textureBaker.textureBakeResults != mom.textureBakeResults)
			{
				Debug.LogWarning("Texture Bake Result on this component is not the same as the Texture Bake Result on the MB3_TextureBaker.");
			}
		}
		Dictionary<int, MB_Utility.MeshAnalysisResult> dictionary = null;
		if (validationLevel == MB2_ValidationLevel.robust)
		{
			dictionary = new Dictionary<int, MB_Utility.MeshAnalysisResult>();
		}
		List<GameObject> objectsToCombine = mom.GetObjectsToCombine();
		for (int i = 0; i < objectsToCombine.Count; i++)
		{
			GameObject gameObject = objectsToCombine[i];
			if (gameObject == null)
			{
				Debug.LogError("The list of objects to combine contains a null at position." + i + " Select and use [shift] delete to remove");
				return false;
			}
			for (int j = i + 1; j < objectsToCombine.Count; j++)
			{
				if (objectsToCombine[i] == objectsToCombine[j])
				{
					Debug.LogError(string.Concat(new object[] { "The list of objects to combine contains duplicates at ", i, " and ", j }));
					return false;
				}
			}
			if (MB_Utility.GetGOMaterials(gameObject).Length == 0)
			{
				Debug.LogError("Object " + gameObject + " in the list of objects to be combined does not have a material");
				return false;
			}
			Mesh mesh = MB_Utility.GetMesh(gameObject);
			if (mesh == null)
			{
				Debug.LogError("Object " + gameObject + " in the list of objects to be combined does not have a mesh");
				return false;
			}
			if (mesh != null && !Application.isEditor && Application.isPlaying && mom.textureBakeResults.doMultiMaterial && validationLevel >= MB2_ValidationLevel.robust)
			{
				MB_Utility.MeshAnalysisResult meshAnalysisResult;
				if (!dictionary.TryGetValue(mesh.GetInstanceID(), out meshAnalysisResult))
				{
					MB_Utility.doSubmeshesShareVertsOrTris(mesh, ref meshAnalysisResult);
					dictionary.Add(mesh.GetInstanceID(), meshAnalysisResult);
				}
				if (meshAnalysisResult.hasOverlappingSubmeshVerts)
				{
					Debug.LogWarning("Object " + objectsToCombine[i] + " in the list of objects to combine has overlapping submeshes (submeshes share vertices). If the UVs associated with the shared vertices are important then this bake may not work. If you are using multiple materials then this object can only be combined with objects that use the exact same set of textures (each atlas contains one texture). There may be other undesirable side affects as well. Mesh Master, available in the asset store can fix overlapping submeshes.");
				}
			}
		}
		if (mom is MB3_MeshBaker)
		{
			List<GameObject> objectsToCombine2 = mom.GetObjectsToCombine();
			if (objectsToCombine2 == null || objectsToCombine2.Count == 0)
			{
				Debug.LogError("No meshes to combine. Please assign some meshes to combine.");
				return false;
			}
			if (mom is MB3_MeshBaker && ((MB3_MeshBaker)mom).meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer && !editorMethods.ValidateSkinnedMeshes(objectsToCombine2))
			{
				return false;
			}
		}
		if (editorMethods != null)
		{
			editorMethods.CheckPrefabTypes(objToCombineType, objectsToCombine);
		}
		return true;
	}

	// Token: 0x040004E6 RID: 1254
	public static bool DO_INTEGRITY_CHECKS = true;

	// Token: 0x040004E7 RID: 1255
	public Vector3 sortAxis;

	// Token: 0x020000D6 RID: 214
	public class ZSortObjects
	{
		// Token: 0x06000608 RID: 1544 RVA: 0x00033874 File Offset: 0x00031C74
		public void SortByDistanceAlongAxis(List<GameObject> gos)
		{
			if (this.sortAxis == Vector3.zero)
			{
				Debug.LogError("The sort axis cannot be the zero vector.");
				return;
			}
			Debug.Log("Z sorting meshes along axis numObjs=" + gos.Count);
			List<MB3_MeshBakerRoot.ZSortObjects.Item> list = new List<MB3_MeshBakerRoot.ZSortObjects.Item>();
			Quaternion quaternion = Quaternion.FromToRotation(this.sortAxis, Vector3.forward);
			for (int i = 0; i < gos.Count; i++)
			{
				if (gos[i] != null)
				{
					MB3_MeshBakerRoot.ZSortObjects.Item item = new MB3_MeshBakerRoot.ZSortObjects.Item();
					item.point = gos[i].transform.position;
					item.go = gos[i];
					item.point = quaternion * item.point;
					list.Add(item);
				}
			}
			list.Sort(new MB3_MeshBakerRoot.ZSortObjects.ItemComparer());
			for (int j = 0; j < gos.Count; j++)
			{
				gos[j] = list[j].go;
			}
		}

		// Token: 0x040004E8 RID: 1256
		public Vector3 sortAxis;

		// Token: 0x020000D7 RID: 215
		public class Item
		{
			// Token: 0x040004E9 RID: 1257
			public GameObject go;

			// Token: 0x040004EA RID: 1258
			public Vector3 point;
		}

		// Token: 0x020000D8 RID: 216
		public class ItemComparer : IComparer<MB3_MeshBakerRoot.ZSortObjects.Item>
		{
			// Token: 0x0600060B RID: 1547 RVA: 0x00033989 File Offset: 0x00031D89
			public int Compare(MB3_MeshBakerRoot.ZSortObjects.Item a, MB3_MeshBakerRoot.ZSortObjects.Item b)
			{
				return (int)Mathf.Sign(b.point.z - a.point.z);
			}
		}
	}
}
