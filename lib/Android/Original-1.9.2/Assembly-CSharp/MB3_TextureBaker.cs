using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using UnityEngine;

// Token: 0x020000DA RID: 218
public class MB3_TextureBaker : MB3_MeshBakerRoot
{
	// Token: 0x1700008F RID: 143
	// (get) Token: 0x06000611 RID: 1553 RVA: 0x0003583C File Offset: 0x00033C3C
	// (set) Token: 0x06000612 RID: 1554 RVA: 0x00035844 File Offset: 0x00033C44
	public override MB2_TextureBakeResults textureBakeResults
	{
		get
		{
			return this._textureBakeResults;
		}
		set
		{
			this._textureBakeResults = value;
		}
	}

	// Token: 0x17000090 RID: 144
	// (get) Token: 0x06000613 RID: 1555 RVA: 0x0003584D File Offset: 0x00033C4D
	// (set) Token: 0x06000614 RID: 1556 RVA: 0x00035855 File Offset: 0x00033C55
	public virtual int atlasPadding
	{
		get
		{
			return this._atlasPadding;
		}
		set
		{
			this._atlasPadding = value;
		}
	}

	// Token: 0x17000091 RID: 145
	// (get) Token: 0x06000615 RID: 1557 RVA: 0x0003585E File Offset: 0x00033C5E
	// (set) Token: 0x06000616 RID: 1558 RVA: 0x00035866 File Offset: 0x00033C66
	public virtual int maxAtlasSize
	{
		get
		{
			return this._maxAtlasSize;
		}
		set
		{
			this._maxAtlasSize = value;
		}
	}

	// Token: 0x17000092 RID: 146
	// (get) Token: 0x06000617 RID: 1559 RVA: 0x0003586F File Offset: 0x00033C6F
	// (set) Token: 0x06000618 RID: 1560 RVA: 0x00035877 File Offset: 0x00033C77
	public virtual bool resizePowerOfTwoTextures
	{
		get
		{
			return this._resizePowerOfTwoTextures;
		}
		set
		{
			this._resizePowerOfTwoTextures = value;
		}
	}

	// Token: 0x17000093 RID: 147
	// (get) Token: 0x06000619 RID: 1561 RVA: 0x00035880 File Offset: 0x00033C80
	// (set) Token: 0x0600061A RID: 1562 RVA: 0x00035888 File Offset: 0x00033C88
	public virtual bool fixOutOfBoundsUVs
	{
		get
		{
			return this._fixOutOfBoundsUVs;
		}
		set
		{
			this._fixOutOfBoundsUVs = value;
		}
	}

	// Token: 0x17000094 RID: 148
	// (get) Token: 0x0600061B RID: 1563 RVA: 0x00035891 File Offset: 0x00033C91
	// (set) Token: 0x0600061C RID: 1564 RVA: 0x00035899 File Offset: 0x00033C99
	public virtual int maxTilingBakeSize
	{
		get
		{
			return this._maxTilingBakeSize;
		}
		set
		{
			this._maxTilingBakeSize = value;
		}
	}

	// Token: 0x17000095 RID: 149
	// (get) Token: 0x0600061D RID: 1565 RVA: 0x000358A2 File Offset: 0x00033CA2
	// (set) Token: 0x0600061E RID: 1566 RVA: 0x000358AA File Offset: 0x00033CAA
	public virtual MB2_PackingAlgorithmEnum packingAlgorithm
	{
		get
		{
			return this._packingAlgorithm;
		}
		set
		{
			this._packingAlgorithm = value;
		}
	}

	// Token: 0x17000096 RID: 150
	// (get) Token: 0x0600061F RID: 1567 RVA: 0x000358B3 File Offset: 0x00033CB3
	// (set) Token: 0x06000620 RID: 1568 RVA: 0x000358BB File Offset: 0x00033CBB
	public bool meshBakerTexturePackerForcePowerOfTwo
	{
		get
		{
			return this._meshBakerTexturePackerForcePowerOfTwo;
		}
		set
		{
			this._meshBakerTexturePackerForcePowerOfTwo = value;
		}
	}

	// Token: 0x17000097 RID: 151
	// (get) Token: 0x06000621 RID: 1569 RVA: 0x000358C4 File Offset: 0x00033CC4
	// (set) Token: 0x06000622 RID: 1570 RVA: 0x000358CC File Offset: 0x00033CCC
	public virtual List<ShaderTextureProperty> customShaderProperties
	{
		get
		{
			return this._customShaderProperties;
		}
		set
		{
			this._customShaderProperties = value;
		}
	}

	// Token: 0x17000098 RID: 152
	// (get) Token: 0x06000623 RID: 1571 RVA: 0x000358D5 File Offset: 0x00033CD5
	// (set) Token: 0x06000624 RID: 1572 RVA: 0x000358DD File Offset: 0x00033CDD
	public virtual List<string> customShaderPropNames
	{
		get
		{
			return this._customShaderPropNames_Depricated;
		}
		set
		{
			this._customShaderPropNames_Depricated = value;
		}
	}

	// Token: 0x17000099 RID: 153
	// (get) Token: 0x06000625 RID: 1573 RVA: 0x000358E6 File Offset: 0x00033CE6
	// (set) Token: 0x06000626 RID: 1574 RVA: 0x000358EE File Offset: 0x00033CEE
	public virtual bool doMultiMaterial
	{
		get
		{
			return this._doMultiMaterial;
		}
		set
		{
			this._doMultiMaterial = value;
		}
	}

	// Token: 0x1700009A RID: 154
	// (get) Token: 0x06000627 RID: 1575 RVA: 0x000358F7 File Offset: 0x00033CF7
	// (set) Token: 0x06000628 RID: 1576 RVA: 0x000358FF File Offset: 0x00033CFF
	public virtual bool doMultiMaterialSplitAtlasesIfTooBig
	{
		get
		{
			return this._doMultiMaterialSplitAtlasesIfTooBig;
		}
		set
		{
			this._doMultiMaterialSplitAtlasesIfTooBig = value;
		}
	}

	// Token: 0x1700009B RID: 155
	// (get) Token: 0x06000629 RID: 1577 RVA: 0x00035908 File Offset: 0x00033D08
	// (set) Token: 0x0600062A RID: 1578 RVA: 0x00035910 File Offset: 0x00033D10
	public virtual bool doMultiMaterialSplitAtlasesIfOBUVs
	{
		get
		{
			return this._doMultiMaterialSplitAtlasesIfOBUVs;
		}
		set
		{
			this._doMultiMaterialSplitAtlasesIfOBUVs = value;
		}
	}

	// Token: 0x1700009C RID: 156
	// (get) Token: 0x0600062B RID: 1579 RVA: 0x00035919 File Offset: 0x00033D19
	// (set) Token: 0x0600062C RID: 1580 RVA: 0x00035921 File Offset: 0x00033D21
	public virtual Material resultMaterial
	{
		get
		{
			return this._resultMaterial;
		}
		set
		{
			this._resultMaterial = value;
		}
	}

	// Token: 0x1700009D RID: 157
	// (get) Token: 0x0600062D RID: 1581 RVA: 0x0003592A File Offset: 0x00033D2A
	// (set) Token: 0x0600062E RID: 1582 RVA: 0x00035932 File Offset: 0x00033D32
	public bool considerNonTextureProperties
	{
		get
		{
			return this._considerNonTextureProperties;
		}
		set
		{
			this._considerNonTextureProperties = value;
		}
	}

	// Token: 0x1700009E RID: 158
	// (get) Token: 0x0600062F RID: 1583 RVA: 0x0003593B File Offset: 0x00033D3B
	// (set) Token: 0x06000630 RID: 1584 RVA: 0x00035943 File Offset: 0x00033D43
	public bool doSuggestTreatment
	{
		get
		{
			return this._doSuggestTreatment;
		}
		set
		{
			this._doSuggestTreatment = value;
		}
	}

	// Token: 0x1700009F RID: 159
	// (get) Token: 0x06000631 RID: 1585 RVA: 0x0003594C File Offset: 0x00033D4C
	public MB3_TextureBaker.CreateAtlasesCoroutineResult CoroutineResult
	{
		get
		{
			return this._coroutineResult;
		}
	}

	// Token: 0x06000632 RID: 1586 RVA: 0x00035954 File Offset: 0x00033D54
	public override List<GameObject> GetObjectsToCombine()
	{
		if (this.objsToMesh == null)
		{
			this.objsToMesh = new List<GameObject>();
		}
		return this.objsToMesh;
	}

	// Token: 0x06000633 RID: 1587 RVA: 0x00035972 File Offset: 0x00033D72
	public MB_AtlasesAndRects[] CreateAtlases()
	{
		return this.CreateAtlases(null, false, null);
	}

	// Token: 0x06000634 RID: 1588 RVA: 0x00035980 File Offset: 0x00033D80
	public IEnumerator CreateAtlasesCoroutine(ProgressUpdateDelegate progressInfo, MB3_TextureBaker.CreateAtlasesCoroutineResult coroutineResult, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null, float maxTimePerFrame = 0.01f)
	{
		MBVersionConcrete mbv = new MBVersionConcrete();
		if (!MB3_TextureCombiner._RunCorutineWithoutPauseIsRunning && (mbv.GetMajorVersion() < 5 || (mbv.GetMajorVersion() == 5 && mbv.GetMinorVersion() < 3)))
		{
			Debug.LogError("Running the texture combiner as a coroutine only works in Unity 5.3 and higher");
			coroutineResult.success = false;
			yield break;
		}
		this.OnCombinedTexturesCoroutineAtlasesAndRects = null;
		if (maxTimePerFrame <= 0f)
		{
			Debug.LogError("maxTimePerFrame must be a value greater than zero");
			coroutineResult.isFinished = true;
			yield break;
		}
		MB2_ValidationLevel vl = ((!Application.isPlaying) ? MB2_ValidationLevel.robust : MB2_ValidationLevel.quick);
		if (!MB3_MeshBakerRoot.DoCombinedValidate(this, MB_ObjsToCombineTypes.dontCare, null, vl))
		{
			coroutineResult.isFinished = true;
			yield break;
		}
		if (this._doMultiMaterial && !this._ValidateResultMaterials())
		{
			coroutineResult.isFinished = true;
			yield break;
		}
		if (!this._doMultiMaterial)
		{
			if (this._resultMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				coroutineResult.isFinished = true;
				yield break;
			}
			Shader shader = this._resultMaterial.shader;
			for (int j = 0; j < this.objsToMesh.Count; j++)
			{
				foreach (Material material in MB_Utility.GetGOMaterials(this.objsToMesh[j]))
				{
					if (material != null && material.shader != shader)
					{
						Debug.LogWarning(string.Concat(new object[]
						{
							"Game object ",
							this.objsToMesh[j],
							" does not use shader ",
							shader,
							" it may not have the required textures. If not small solid color textures will be generated."
						}));
					}
				}
			}
		}
		MB3_TextureCombiner combiner = this.CreateAndConfigureTextureCombiner();
		combiner.saveAtlasesAsAssets = saveAtlasesAsAssets;
		int numResults = 1;
		if (this._doMultiMaterial)
		{
			numResults = this.resultMaterials.Length;
		}
		this.OnCombinedTexturesCoroutineAtlasesAndRects = new MB_AtlasesAndRects[numResults];
		for (int l = 0; l < this.OnCombinedTexturesCoroutineAtlasesAndRects.Length; l++)
		{
			this.OnCombinedTexturesCoroutineAtlasesAndRects[l] = new MB_AtlasesAndRects();
		}
		for (int i = 0; i < this.OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
		{
			Material resMatToPass = null;
			List<Material> sourceMats = null;
			if (this._doMultiMaterial)
			{
				sourceMats = this.resultMaterials[i].sourceMaterials;
				resMatToPass = this.resultMaterials[i].combinedMaterial;
				combiner.fixOutOfBoundsUVs = this.resultMaterials[i].considerMeshUVs;
			}
			else
			{
				resMatToPass = this._resultMaterial;
			}
			Debug.Log(string.Format("Creating atlases for result material {0} using shader {1}", resMatToPass, resMatToPass.shader));
			MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult coroutineResult2 = new MB3_TextureCombiner.CombineTexturesIntoAtlasesCoroutineResult();
			yield return combiner.CombineTexturesIntoAtlasesCoroutine(progressInfo, this.OnCombinedTexturesCoroutineAtlasesAndRects[i], resMatToPass, this.objsToMesh, sourceMats, editorMethods, coroutineResult2, maxTimePerFrame, null, false);
			coroutineResult.success = coroutineResult2.success;
			if (!coroutineResult.success)
			{
				coroutineResult.isFinished = true;
				yield break;
			}
		}
		this.unpackMat2RectMap(this.textureBakeResults);
		this.textureBakeResults.doMultiMaterial = this._doMultiMaterial;
		if (this._doMultiMaterial)
		{
			this.textureBakeResults.resultMaterials = this.resultMaterials;
		}
		else
		{
			MB_MultiMaterial[] array = new MB_MultiMaterial[]
			{
				new MB_MultiMaterial()
			};
			array[0].combinedMaterial = this._resultMaterial;
			array[0].considerMeshUVs = this._fixOutOfBoundsUVs;
			array[0].sourceMaterials = new List<Material>();
			array[0].sourceMaterials.AddRange(this.textureBakeResults.materials);
			this.textureBakeResults.resultMaterials = array;
		}
		MB3_MeshBakerCommon[] mb = base.GetComponentsInChildren<MB3_MeshBakerCommon>();
		for (int m = 0; m < mb.Length; m++)
		{
			mb[m].textureBakeResults = this.textureBakeResults;
		}
		if (this.LOG_LEVEL >= MB2_LogLevel.info)
		{
			Debug.Log("Created Atlases");
		}
		coroutineResult.isFinished = true;
		if (coroutineResult.success && this.onBuiltAtlasesSuccess != null)
		{
			this.onBuiltAtlasesSuccess();
		}
		if (!coroutineResult.success && this.onBuiltAtlasesFail != null)
		{
			this.onBuiltAtlasesFail();
		}
		yield break;
	}

	// Token: 0x06000635 RID: 1589 RVA: 0x000359C0 File Offset: 0x00033DC0
	public MB_AtlasesAndRects[] CreateAtlases(ProgressUpdateDelegate progressInfo, bool saveAtlasesAsAssets = false, MB2_EditorMethodsInterface editorMethods = null)
	{
		MB_AtlasesAndRects[] array = null;
		try
		{
			this._coroutineResult = new MB3_TextureBaker.CreateAtlasesCoroutineResult();
			MB3_TextureCombiner.RunCorutineWithoutPause(this.CreateAtlasesCoroutine(progressInfo, this._coroutineResult, saveAtlasesAsAssets, editorMethods, 1000f), 0);
			if (this._coroutineResult.success && this.textureBakeResults != null)
			{
				array = this.OnCombinedTexturesCoroutineAtlasesAndRects;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex);
		}
		finally
		{
			if (saveAtlasesAsAssets && array != null)
			{
				foreach (MB_AtlasesAndRects mb_AtlasesAndRects in array)
				{
					if (mb_AtlasesAndRects != null && mb_AtlasesAndRects.atlases != null)
					{
						for (int j = 0; j < mb_AtlasesAndRects.atlases.Length; j++)
						{
							if (mb_AtlasesAndRects.atlases[j] != null)
							{
								if (editorMethods != null)
								{
									editorMethods.Destroy(mb_AtlasesAndRects.atlases[j]);
								}
								else
								{
									MB_Utility.Destroy(mb_AtlasesAndRects.atlases[j]);
								}
							}
						}
					}
				}
			}
		}
		return array;
	}

	// Token: 0x06000636 RID: 1590 RVA: 0x00035ADC File Offset: 0x00033EDC
	private void unpackMat2RectMap(MB2_TextureBakeResults tbr)
	{
		List<Material> list = new List<Material>();
		List<MB_MaterialAndUVRect> list2 = new List<MB_MaterialAndUVRect>();
		List<Rect> list3 = new List<Rect>();
		for (int i = 0; i < this.OnCombinedTexturesCoroutineAtlasesAndRects.Length; i++)
		{
			MB_AtlasesAndRects mb_AtlasesAndRects = this.OnCombinedTexturesCoroutineAtlasesAndRects[i];
			List<MB_MaterialAndUVRect> mat2rect_map = mb_AtlasesAndRects.mat2rect_map;
			if (mat2rect_map != null)
			{
				for (int j = 0; j < mat2rect_map.Count; j++)
				{
					list2.Add(mat2rect_map[j]);
					list.Add(mat2rect_map[j].material);
					list3.Add(mat2rect_map[j].atlasRect);
				}
			}
		}
		tbr.materials = list.ToArray();
		tbr.materialsAndUVRects = list2.ToArray();
	}

	// Token: 0x06000637 RID: 1591 RVA: 0x00035B9C File Offset: 0x00033F9C
	public MB3_TextureCombiner CreateAndConfigureTextureCombiner()
	{
		return new MB3_TextureCombiner
		{
			LOG_LEVEL = this.LOG_LEVEL,
			atlasPadding = this._atlasPadding,
			maxAtlasSize = this._maxAtlasSize,
			customShaderPropNames = this._customShaderProperties,
			fixOutOfBoundsUVs = this._fixOutOfBoundsUVs,
			maxTilingBakeSize = this._maxTilingBakeSize,
			packingAlgorithm = this._packingAlgorithm,
			meshBakerTexturePackerForcePowerOfTwo = this._meshBakerTexturePackerForcePowerOfTwo,
			resizePowerOfTwoTextures = this._resizePowerOfTwoTextures,
			considerNonTextureProperties = this._considerNonTextureProperties
		};
	}

	// Token: 0x06000638 RID: 1592 RVA: 0x00035C28 File Offset: 0x00034028
	public static void ConfigureNewMaterialToMatchOld(Material newMat, Material original)
	{
		if (original == null)
		{
			Debug.LogWarning(string.Concat(new object[] { "Original material is null, could not copy properties to ", newMat, ". Setting shader to ", newMat.shader }));
			return;
		}
		newMat.shader = original.shader;
		newMat.CopyPropertiesFromMaterial(original);
		ShaderTextureProperty[] shaderTexPropertyNames = MB3_TextureCombiner.shaderTexPropertyNames;
		for (int i = 0; i < shaderTexPropertyNames.Length; i++)
		{
			Vector2 one = Vector2.one;
			Vector2 zero = Vector2.zero;
			if (newMat.HasProperty(shaderTexPropertyNames[i].name))
			{
				newMat.SetTextureOffset(shaderTexPropertyNames[i].name, zero);
				newMat.SetTextureScale(shaderTexPropertyNames[i].name, one);
			}
		}
	}

	// Token: 0x06000639 RID: 1593 RVA: 0x00035CDC File Offset: 0x000340DC
	private string PrintSet(HashSet<Material> s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Material material in s)
		{
			stringBuilder.Append(material + ",");
		}
		return stringBuilder.ToString();
	}

	// Token: 0x0600063A RID: 1594 RVA: 0x00035D4C File Offset: 0x0003414C
	private bool _ValidateResultMaterials()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		for (int i = 0; i < this.objsToMesh.Count; i++)
		{
			if (this.objsToMesh[i] != null)
			{
				Material[] gomaterials = MB_Utility.GetGOMaterials(this.objsToMesh[i]);
				for (int j = 0; j < gomaterials.Length; j++)
				{
					if (gomaterials[j] != null)
					{
						hashSet.Add(gomaterials[j]);
					}
				}
			}
		}
		HashSet<Material> hashSet2 = new HashSet<Material>();
		for (int k = 0; k < this.resultMaterials.Length; k++)
		{
			MB_MultiMaterial mb_MultiMaterial = this.resultMaterials[k];
			if (mb_MultiMaterial.combinedMaterial == null)
			{
				Debug.LogError("Combined Material is null please create and assign a result material.");
				return false;
			}
			Shader shader = mb_MultiMaterial.combinedMaterial.shader;
			for (int l = 0; l < mb_MultiMaterial.sourceMaterials.Count; l++)
			{
				if (mb_MultiMaterial.sourceMaterials[l] == null)
				{
					Debug.LogError("There are null entries in the list of Source Materials");
					return false;
				}
				if (shader != mb_MultiMaterial.sourceMaterials[l].shader)
				{
					Debug.LogWarning(string.Concat(new object[]
					{
						"Source material ",
						mb_MultiMaterial.sourceMaterials[l],
						" does not use shader ",
						shader,
						" it may not have the required textures. If not empty textures will be generated."
					}));
				}
				if (hashSet2.Contains(mb_MultiMaterial.sourceMaterials[l]))
				{
					Debug.LogError("A Material " + mb_MultiMaterial.sourceMaterials[l] + " appears more than once in the list of source materials in the source material to combined mapping. Each source material must be unique.");
					return false;
				}
				hashSet2.Add(mb_MultiMaterial.sourceMaterials[l]);
			}
		}
		if (hashSet.IsProperSubsetOf(hashSet2))
		{
			hashSet2.ExceptWith(hashSet);
			Debug.LogWarning("There are materials in the mapping that are not used on your source objects: " + this.PrintSet(hashSet2));
		}
		if (this.resultMaterials != null && this.resultMaterials.Length > 0 && hashSet2.IsProperSubsetOf(hashSet))
		{
			hashSet.ExceptWith(hashSet2);
			Debug.LogError("There are materials on the objects to combine that are not in the mapping: " + this.PrintSet(hashSet));
			return false;
		}
		return true;
	}

	// Token: 0x040004EC RID: 1260
	public MB2_LogLevel LOG_LEVEL = MB2_LogLevel.info;

	// Token: 0x040004ED RID: 1261
	[SerializeField]
	protected MB2_TextureBakeResults _textureBakeResults;

	// Token: 0x040004EE RID: 1262
	[SerializeField]
	protected int _atlasPadding = 1;

	// Token: 0x040004EF RID: 1263
	[SerializeField]
	protected int _maxAtlasSize = 4096;

	// Token: 0x040004F0 RID: 1264
	[SerializeField]
	protected bool _resizePowerOfTwoTextures;

	// Token: 0x040004F1 RID: 1265
	[SerializeField]
	protected bool _fixOutOfBoundsUVs;

	// Token: 0x040004F2 RID: 1266
	[SerializeField]
	protected int _maxTilingBakeSize = 1024;

	// Token: 0x040004F3 RID: 1267
	[SerializeField]
	protected MB2_PackingAlgorithmEnum _packingAlgorithm = MB2_PackingAlgorithmEnum.MeshBakerTexturePacker;

	// Token: 0x040004F4 RID: 1268
	[SerializeField]
	protected bool _meshBakerTexturePackerForcePowerOfTwo = true;

	// Token: 0x040004F5 RID: 1269
	[SerializeField]
	protected List<ShaderTextureProperty> _customShaderProperties = new List<ShaderTextureProperty>();

	// Token: 0x040004F6 RID: 1270
	[SerializeField]
	protected List<string> _customShaderPropNames_Depricated = new List<string>();

	// Token: 0x040004F7 RID: 1271
	[SerializeField]
	protected bool _doMultiMaterial;

	// Token: 0x040004F8 RID: 1272
	[SerializeField]
	protected bool _doMultiMaterialSplitAtlasesIfTooBig = true;

	// Token: 0x040004F9 RID: 1273
	[SerializeField]
	protected bool _doMultiMaterialSplitAtlasesIfOBUVs = true;

	// Token: 0x040004FA RID: 1274
	[SerializeField]
	protected Material _resultMaterial;

	// Token: 0x040004FB RID: 1275
	[SerializeField]
	protected bool _considerNonTextureProperties;

	// Token: 0x040004FC RID: 1276
	[SerializeField]
	protected bool _doSuggestTreatment = true;

	// Token: 0x040004FD RID: 1277
	private MB3_TextureBaker.CreateAtlasesCoroutineResult _coroutineResult;

	// Token: 0x040004FE RID: 1278
	public MB_MultiMaterial[] resultMaterials = new MB_MultiMaterial[0];

	// Token: 0x040004FF RID: 1279
	public List<GameObject> objsToMesh;

	// Token: 0x04000500 RID: 1280
	public MB3_TextureBaker.OnCombinedTexturesCoroutineSuccess onBuiltAtlasesSuccess;

	// Token: 0x04000501 RID: 1281
	public MB3_TextureBaker.OnCombinedTexturesCoroutineFail onBuiltAtlasesFail;

	// Token: 0x04000502 RID: 1282
	public MB_AtlasesAndRects[] OnCombinedTexturesCoroutineAtlasesAndRects;

	// Token: 0x020000DB RID: 219
	// (Invoke) Token: 0x0600063C RID: 1596
	public delegate void OnCombinedTexturesCoroutineSuccess();

	// Token: 0x020000DC RID: 220
	// (Invoke) Token: 0x06000640 RID: 1600
	public delegate void OnCombinedTexturesCoroutineFail();

	// Token: 0x020000DD RID: 221
	public class CreateAtlasesCoroutineResult
	{
		// Token: 0x04000503 RID: 1283
		public bool success = true;

		// Token: 0x04000504 RID: 1284
		public bool isFinished;
	}
}
