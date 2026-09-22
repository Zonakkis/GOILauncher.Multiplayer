using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000019 RID: 25
public class FogVolumePrimitiveManager : MonoBehaviour
{
	// Token: 0x1700001C RID: 28
	// (get) Token: 0x060000A0 RID: 160 RVA: 0x00009202 File Offset: 0x00007602
	// (set) Token: 0x060000A1 RID: 161 RVA: 0x0000920A File Offset: 0x0000760A
	public int CurrentPrimitiveCount { get; private set; }

	// Token: 0x1700001D RID: 29
	// (get) Token: 0x060000A2 RID: 162 RVA: 0x00009213 File Offset: 0x00007613
	// (set) Token: 0x060000A3 RID: 163 RVA: 0x0000921B File Offset: 0x0000761B
	public int VisiblePrimitiveCount { get; private set; }

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x060000A4 RID: 164 RVA: 0x00009224 File Offset: 0x00007624
	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return this.m_pointOfInterestTf != null;
		}
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00009234 File Offset: 0x00007634
	public void FindPrimitivesInFogVolume()
	{
		this.CurrentPrimitiveCount = 0;
		this.VisiblePrimitiveCount = 0;
		this.m_primitives.Clear();
		this.m_primitivesInFrustum.Clear();
		for (int i = 0; i < 1000; i++)
		{
			this.m_primitives.Add(new FogVolumePrimitiveManager.PrimitiveData());
			this.m_primitivesInFrustum.Add(new FogVolumePrimitiveManager.PrimitiveData());
		}
		if (this.m_boxCollider == null)
		{
			this.m_boxCollider = base.gameObject.GetComponent<BoxCollider>();
		}
		Bounds bounds = this.m_boxCollider.bounds;
		foreach (FogVolumePrimitive fogVolumePrimitive in global::UnityEngine.Object.FindObjectsOfType<FogVolumePrimitive>())
		{
			if (bounds.Intersects(fogVolumePrimitive.Bounds))
			{
				if (fogVolumePrimitive.BoxColl != null)
				{
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Box;
				}
				else if (fogVolumePrimitive.SphereColl != null)
				{
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Sphere;
				}
				else
				{
					fogVolumePrimitive.BoxColl = fogVolumePrimitive.GetTransform.gameObject.AddComponent<BoxCollider>();
					fogVolumePrimitive.Type = EFogVolumePrimitiveType.Box;
				}
				if (fogVolumePrimitive.Type == EFogVolumePrimitiveType.Box)
				{
					this.AddPrimitiveBox(fogVolumePrimitive);
				}
				else if (fogVolumePrimitive.Type == EFogVolumePrimitiveType.Sphere)
				{
					this.AddPrimitiveSphere(fogVolumePrimitive);
				}
			}
		}
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x0000938C File Offset: 0x0000778C
	public bool AddPrimitiveBox(FogVolumePrimitive _box)
	{
		int num = this._FindFirstFreePrimitive();
		if (num != -1)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[num];
			this.CurrentPrimitiveCount++;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Box;
			primitiveData.Transform = _box.transform;
			primitiveData.Renderer = _box.GetComponent<Renderer>();
			primitiveData.Primitive = _box;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _box.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00009408 File Offset: 0x00007808
	public bool AddPrimitiveSphere(FogVolumePrimitive _sphere)
	{
		int num = this._FindFirstFreePrimitive();
		if (num != -1)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[num];
			this.CurrentPrimitiveCount++;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Sphere;
			primitiveData.Transform = _sphere.transform;
			primitiveData.Renderer = _sphere.GetComponent<Renderer>();
			primitiveData.Primitive = _sphere;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _sphere.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00009484 File Offset: 0x00007884
	public bool RemovePrimitive(Transform _primitiveToRemove)
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (object.ReferenceEquals(this.m_primitives[i].Transform, _primitiveToRemove))
			{
				primitiveData.Reset();
				this.CurrentPrimitiveCount--;
				return true;
			}
		}
		return false;
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x000094EF File Offset: 0x000078EF
	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		this.m_pointOfInterestTf = null;
		this.m_pointOfInterest = _pointOfInterest;
	}

	// Token: 0x060000AA RID: 170 RVA: 0x000094FF File Offset: 0x000078FF
	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		this.m_pointOfInterestTf = _pointOfInterest;
	}

	// Token: 0x060000AB RID: 171 RVA: 0x00009508 File Offset: 0x00007908
	public void OnDrawGizmos()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	// Token: 0x060000AC RID: 172 RVA: 0x00009514 File Offset: 0x00007914
	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		this.m_camera = ((!(this.m_fogVolumeData != null)) ? null : this.m_fogVolumeData.GameCamera);
		if (this.m_camera == null)
		{
			return;
		}
		this.FrustumPlanes = _frustumPlanes;
		if (this.m_boxCollider == null)
		{
			this.m_boxCollider = this.m_fogVolume.GetComponent<BoxCollider>();
		}
		this._UpdateBounds();
		this._FindPrimitivesInFrustum();
		if (this.m_primitivesInFrustum.Count > 20)
		{
			this._SortPrimitivesInFrustum();
		}
		this._PrepareShaderArrays();
	}

	// Token: 0x060000AD RID: 173 RVA: 0x000095B0 File Offset: 0x000079B0
	public void SetVisibility(bool _enabled)
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			if (this.m_primitives[i].Renderer != null)
			{
				this.m_primitives[i].Renderer.enabled = _enabled;
			}
		}
	}

	// Token: 0x060000AE RID: 174 RVA: 0x00009610 File Offset: 0x00007A10
	public void Initialize()
	{
		this.m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		this.m_fogVolumeData = global::UnityEngine.Object.FindObjectOfType<FogVolumeData>();
		this.m_camera = null;
		this.m_boxCollider = null;
		this.CurrentPrimitiveCount = 0;
		if (this.m_primitives == null)
		{
			this.m_primitives = new List<FogVolumePrimitiveManager.PrimitiveData>(1000);
			this.m_primitivesInFrustum = new List<FogVolumePrimitiveManager.PrimitiveData>();
			for (int i = 0; i < 1000; i++)
			{
				this.m_primitives.Add(new FogVolumePrimitiveManager.PrimitiveData());
				this.m_primitivesInFrustum.Add(new FogVolumePrimitiveManager.PrimitiveData());
			}
		}
	}

	// Token: 0x060000AF RID: 175 RVA: 0x000096AA File Offset: 0x00007AAA
	public void Deinitialize()
	{
		this.VisiblePrimitiveCount = 0;
	}

	// Token: 0x060000B0 RID: 176 RVA: 0x000096B3 File Offset: 0x00007AB3
	public Vector4[] GetPrimitivePositionArray()
	{
		return this.m_primitivePos;
	}

	// Token: 0x060000B1 RID: 177 RVA: 0x000096BB File Offset: 0x00007ABB
	public Vector4[] GetPrimitiveScaleArray()
	{
		return this.m_primitiveScale;
	}

	// Token: 0x060000B2 RID: 178 RVA: 0x000096C3 File Offset: 0x00007AC3
	public Matrix4x4[] GetPrimitiveTransformArray()
	{
		return this.m_primitiveTf;
	}

	// Token: 0x060000B3 RID: 179 RVA: 0x000096CB File Offset: 0x00007ACB
	public Vector4[] GetPrimitiveDataArray()
	{
		return this.m_primitiveData;
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x000096D4 File Offset: 0x00007AD4
	private void _UpdateBounds()
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (primitiveData.PrimitiveType != EFogVolumePrimitiveType.None)
			{
				if (primitiveData.Primitive == null)
				{
					this.RemovePrimitive(primitiveData.Transform);
				}
				else if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.Box)
				{
					if (primitiveData.Primitive.BoxColl == null)
					{
						Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
						primitiveData.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Box);
					}
					primitiveData.Bounds = primitiveData.Primitive.BoxColl.bounds;
				}
				else if (primitiveData.PrimitiveType == EFogVolumePrimitiveType.Sphere)
				{
					if (primitiveData.Primitive.SphereColl == null)
					{
						Debug.LogWarning("FogVolumePrimitive requires a collider.\nThe collider will be automatically created.");
						primitiveData.Primitive.AddColliderIfNeccessary(EFogVolumePrimitiveType.Sphere);
					}
					primitiveData.Bounds = primitiveData.Primitive.SphereColl.bounds;
				}
			}
		}
	}

	// Token: 0x060000B5 RID: 181 RVA: 0x000097E0 File Offset: 0x00007BE0
	private int _FindFirstFreePrimitive()
	{
		if (this.CurrentPrimitiveCount < 1000)
		{
			int count = this.m_primitives.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.m_primitives[i].PrimitiveType == EFogVolumePrimitiveType.None)
				{
					return i;
				}
			}
		}
		return -1;
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x00009838 File Offset: 0x00007C38
	private void _FindPrimitivesInFrustum()
	{
		this.m_inFrustumCount = 0;
		Vector3 position = this.m_camera.gameObject.transform.position;
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (primitiveData.Transform == null)
			{
				primitiveData.PrimitiveType = EFogVolumePrimitiveType.None;
			}
			if (primitiveData.PrimitiveType != EFogVolumePrimitiveType.None)
			{
				if (primitiveData.Primitive.IsPersistent)
				{
					Vector3 position2 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position2 - this.m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position2 - position).magnitude;
					this.m_primitivesInFrustum[this.m_inFrustumCount++] = primitiveData;
				}
				else if (GeometryUtility.TestPlanesAABB(this.FrustumPlanes, this.m_primitives[i].Bounds))
				{
					Vector3 position3 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position3 - this.m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position3 - position).magnitude;
					this.m_primitivesInFrustum[this.m_inFrustumCount++] = primitiveData;
				}
			}
		}
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x000099AC File Offset: 0x00007DAC
	private void _SortPrimitivesInFrustum()
	{
		bool flag;
		do
		{
			flag = true;
			for (int i = 0; i < this.m_inFrustumCount - 1; i++)
			{
				if (this.m_primitivesInFrustum[i].SqDistance > this.m_primitivesInFrustum[i + 1].SqDistance)
				{
					FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitivesInFrustum[i];
					this.m_primitivesInFrustum[i] = this.m_primitivesInFrustum[i + 1];
					this.m_primitivesInFrustum[i + 1] = primitiveData;
					flag = false;
				}
			}
		}
		while (!flag);
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x00009A40 File Offset: 0x00007E40
	private void _PrepareShaderArrays()
	{
		this.VisiblePrimitiveCount = 0;
		Quaternion rotation = this.m_fogVolume.gameObject.transform.rotation;
		for (int i = 0; i < 20; i++)
		{
			if (i >= this.m_inFrustumCount)
			{
				break;
			}
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitivesInFrustum[i];
			Vector3 position = primitiveData.Transform.position;
			this.m_primitivePos[i] = base.gameObject.transform.InverseTransformPoint(position);
			this.m_primitiveTf[i].SetTRS(position, Quaternion.Inverse(primitiveData.Transform.rotation) * rotation, Vector3.one);
			this.m_primitiveScale[i] = primitiveData.Primitive.GetPrimitiveScale;
			this.m_primitiveData[i] = new Vector4((primitiveData.PrimitiveType != EFogVolumePrimitiveType.Box) ? 1.5f : 0.5f, (!primitiveData.Primitive.IsSubtractive) ? 0.5f : 1.5f, 0f, 0f);
			this.VisiblePrimitiveCount++;
		}
	}

	// Token: 0x0400015D RID: 349
	private FogVolume m_fogVolume;

	// Token: 0x0400015E RID: 350
	private FogVolumeData m_fogVolumeData;

	// Token: 0x0400015F RID: 351
	private Camera m_camera;

	// Token: 0x04000160 RID: 352
	private BoxCollider m_boxCollider;

	// Token: 0x04000161 RID: 353
	private Transform m_pointOfInterestTf;

	// Token: 0x04000162 RID: 354
	private Vector3 m_pointOfInterest = Vector3.zero;

	// Token: 0x04000163 RID: 355
	private List<FogVolumePrimitiveManager.PrimitiveData> m_primitives;

	// Token: 0x04000164 RID: 356
	private List<FogVolumePrimitiveManager.PrimitiveData> m_primitivesInFrustum;

	// Token: 0x04000165 RID: 357
	private int m_inFrustumCount;

	// Token: 0x04000166 RID: 358
	private Plane[] FrustumPlanes;

	// Token: 0x04000167 RID: 359
	private readonly Vector4[] m_primitivePos = new Vector4[20];

	// Token: 0x04000168 RID: 360
	private readonly Vector4[] m_primitiveScale = new Vector4[20];

	// Token: 0x04000169 RID: 361
	private readonly Matrix4x4[] m_primitiveTf = new Matrix4x4[20];

	// Token: 0x0400016A RID: 362
	private readonly Vector4[] m_primitiveData = new Vector4[20];

	// Token: 0x0400016B RID: 363
	private const int InvalidIndex = -1;

	// Token: 0x0400016C RID: 364
	private const int MaxVisiblePrimitives = 20;

	// Token: 0x0400016D RID: 365
	private const int MaxPrimitivesCount = 1000;

	// Token: 0x0200001A RID: 26
	protected class PrimitiveData
	{
		// Token: 0x060000B9 RID: 185 RVA: 0x00009B84 File Offset: 0x00007F84
		public PrimitiveData()
		{
			this.PrimitiveType = EFogVolumePrimitiveType.None;
			this.Primitive = null;
			this.Transform = null;
			this.Renderer = null;
			this.SqDistance = 0f;
			this.Distance2Camera = 0f;
			this.Bounds = default(Bounds);
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000BA RID: 186 RVA: 0x00009BD8 File Offset: 0x00007FD8
		// (set) Token: 0x060000BB RID: 187 RVA: 0x00009BE0 File Offset: 0x00007FE0
		public EFogVolumePrimitiveType PrimitiveType { get; set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000BC RID: 188 RVA: 0x00009BE9 File Offset: 0x00007FE9
		// (set) Token: 0x060000BD RID: 189 RVA: 0x00009BF1 File Offset: 0x00007FF1
		public FogVolumePrimitive Primitive { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000BE RID: 190 RVA: 0x00009BFA File Offset: 0x00007FFA
		// (set) Token: 0x060000BF RID: 191 RVA: 0x00009C02 File Offset: 0x00008002
		public Transform Transform { get; set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000C0 RID: 192 RVA: 0x00009C0B File Offset: 0x0000800B
		// (set) Token: 0x060000C1 RID: 193 RVA: 0x00009C13 File Offset: 0x00008013
		public Renderer Renderer { get; set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x00009C1C File Offset: 0x0000801C
		// (set) Token: 0x060000C3 RID: 195 RVA: 0x00009C24 File Offset: 0x00008024
		public float SqDistance { get; set; }

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x00009C2D File Offset: 0x0000802D
		// (set) Token: 0x060000C5 RID: 197 RVA: 0x00009C35 File Offset: 0x00008035
		public float Distance2Camera { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x060000C6 RID: 198 RVA: 0x00009C3E File Offset: 0x0000803E
		// (set) Token: 0x060000C7 RID: 199 RVA: 0x00009C46 File Offset: 0x00008046
		public Bounds Bounds { get; set; }

		// Token: 0x060000C8 RID: 200 RVA: 0x00009C4F File Offset: 0x0000804F
		public void Reset()
		{
			this.PrimitiveType = EFogVolumePrimitiveType.None;
			this.Primitive = null;
			this.Transform = null;
			this.Renderer = null;
			this.SqDistance = 0f;
			this.Distance2Camera = 0f;
		}
	}
}
