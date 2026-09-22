using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000015 RID: 21
public class FogVolumePrimitiveManager : MonoBehaviour
{
	// Token: 0x1700001A RID: 26
	// (get) Token: 0x060000AF RID: 175 RVA: 0x00008CA9 File Offset: 0x00006EA9
	// (set) Token: 0x060000B0 RID: 176 RVA: 0x00008CB1 File Offset: 0x00006EB1
	public int CurrentPrimitiveCount { get; private set; }

	// Token: 0x1700001B RID: 27
	// (get) Token: 0x060000B1 RID: 177 RVA: 0x00008CBA File Offset: 0x00006EBA
	// (set) Token: 0x060000B2 RID: 178 RVA: 0x00008CC2 File Offset: 0x00006EC2
	public int VisiblePrimitiveCount { get; private set; }

	// Token: 0x1700001C RID: 28
	// (get) Token: 0x060000B3 RID: 179 RVA: 0x00008CCB File Offset: 0x00006ECB
	public bool AlreadyUsesTransformForPoI
	{
		get
		{
			return this.m_pointOfInterestTf != null;
		}
	}

	// Token: 0x060000B4 RID: 180 RVA: 0x00008CDC File Offset: 0x00006EDC
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
		foreach (FogVolumePrimitive fogVolumePrimitive in Object.FindObjectsOfType<FogVolumePrimitive>())
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

	// Token: 0x060000B5 RID: 181 RVA: 0x00008E10 File Offset: 0x00007010
	public bool AddPrimitiveBox(FogVolumePrimitive _box)
	{
		int num = this._FindFirstFreePrimitive();
		if (num != -1)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[num];
			int currentPrimitiveCount = this.CurrentPrimitiveCount;
			this.CurrentPrimitiveCount = currentPrimitiveCount + 1;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Box;
			primitiveData.Transform = _box.transform;
			primitiveData.Renderer = _box.GetComponent<Renderer>();
			primitiveData.Primitive = _box;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _box.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	// Token: 0x060000B6 RID: 182 RVA: 0x00008E88 File Offset: 0x00007088
	public bool AddPrimitiveSphere(FogVolumePrimitive _sphere)
	{
		int num = this._FindFirstFreePrimitive();
		if (num != -1)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[num];
			int currentPrimitiveCount = this.CurrentPrimitiveCount;
			this.CurrentPrimitiveCount = currentPrimitiveCount + 1;
			primitiveData.PrimitiveType = EFogVolumePrimitiveType.Sphere;
			primitiveData.Transform = _sphere.transform;
			primitiveData.Renderer = _sphere.GetComponent<Renderer>();
			primitiveData.Primitive = _sphere;
			primitiveData.Bounds = new Bounds(primitiveData.Transform.position, _sphere.GetPrimitiveScale);
			return true;
		}
		return false;
	}

	// Token: 0x060000B7 RID: 183 RVA: 0x00008F00 File Offset: 0x00007100
	public bool RemovePrimitive(Transform _primitiveToRemove)
	{
		int count = this.m_primitives.Count;
		for (int i = 0; i < count; i++)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitives[i];
			if (this.m_primitives[i].Transform == _primitiveToRemove)
			{
				primitiveData.Reset();
				int currentPrimitiveCount = this.CurrentPrimitiveCount;
				this.CurrentPrimitiveCount = currentPrimitiveCount - 1;
				return true;
			}
		}
		return false;
	}

	// Token: 0x060000B8 RID: 184 RVA: 0x00008F5F File Offset: 0x0000715F
	public void SetPointOfInterest(Vector3 _pointOfInterest)
	{
		this.m_pointOfInterestTf = null;
		this.m_pointOfInterest = _pointOfInterest;
	}

	// Token: 0x060000B9 RID: 185 RVA: 0x00008F6F File Offset: 0x0000716F
	public void SetPointOfInterest(Transform _pointOfInterest)
	{
		this.m_pointOfInterestTf = _pointOfInterest;
	}

	// Token: 0x060000BA RID: 186 RVA: 0x00008F78 File Offset: 0x00007178
	public void OnDrawGizmos()
	{
		base.hideFlags = HideFlags.HideInInspector;
	}

	// Token: 0x060000BB RID: 187 RVA: 0x00008F84 File Offset: 0x00007184
	public void ManualUpdate(ref Plane[] _frustumPlanes)
	{
		this.m_camera = ((this.m_fogVolumeData != null) ? this.m_fogVolumeData.GameCamera : null);
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

	// Token: 0x060000BC RID: 188 RVA: 0x00009010 File Offset: 0x00007210
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

	// Token: 0x060000BD RID: 189 RVA: 0x00009068 File Offset: 0x00007268
	public void Initialize()
	{
		this.m_fogVolume = base.gameObject.GetComponent<FogVolume>();
		this.m_fogVolumeData = Object.FindObjectOfType<FogVolumeData>();
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

	// Token: 0x060000BE RID: 190 RVA: 0x000090F9 File Offset: 0x000072F9
	public void Deinitialize()
	{
		this.VisiblePrimitiveCount = 0;
	}

	// Token: 0x060000BF RID: 191 RVA: 0x00009102 File Offset: 0x00007302
	public Vector4[] GetPrimitivePositionArray()
	{
		return this.m_primitivePos;
	}

	// Token: 0x060000C0 RID: 192 RVA: 0x0000910A File Offset: 0x0000730A
	public Vector4[] GetPrimitiveScaleArray()
	{
		return this.m_primitiveScale;
	}

	// Token: 0x060000C1 RID: 193 RVA: 0x00009112 File Offset: 0x00007312
	public Matrix4x4[] GetPrimitiveTransformArray()
	{
		return this.m_primitiveTf;
	}

	// Token: 0x060000C2 RID: 194 RVA: 0x0000911A File Offset: 0x0000731A
	public Vector4[] GetPrimitiveDataArray()
	{
		return this.m_primitiveData;
	}

	// Token: 0x060000C3 RID: 195 RVA: 0x00009124 File Offset: 0x00007324
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

	// Token: 0x060000C4 RID: 196 RVA: 0x0000921C File Offset: 0x0000741C
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

	// Token: 0x060000C5 RID: 197 RVA: 0x00009268 File Offset: 0x00007468
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
					List<FogVolumePrimitiveManager.PrimitiveData> primitivesInFrustum = this.m_primitivesInFrustum;
					int num = this.m_inFrustumCount;
					this.m_inFrustumCount = num + 1;
					primitivesInFrustum[num] = primitiveData;
				}
				else if (GeometryUtility.TestPlanesAABB(this.FrustumPlanes, this.m_primitives[i].Bounds))
				{
					Vector3 position3 = primitiveData.Transform.position;
					primitiveData.SqDistance = (position3 - this.m_pointOfInterest).sqrMagnitude;
					primitiveData.Distance2Camera = (position3 - position).magnitude;
					List<FogVolumePrimitiveManager.PrimitiveData> primitivesInFrustum2 = this.m_primitivesInFrustum;
					int num = this.m_inFrustumCount;
					this.m_inFrustumCount = num + 1;
					primitivesInFrustum2[num] = primitiveData;
				}
			}
		}
	}

	// Token: 0x060000C6 RID: 198 RVA: 0x000093CC File Offset: 0x000075CC
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

	// Token: 0x060000C7 RID: 199 RVA: 0x00009454 File Offset: 0x00007654
	private void _PrepareShaderArrays()
	{
		this.VisiblePrimitiveCount = 0;
		Quaternion rotation = this.m_fogVolume.gameObject.transform.rotation;
		int num = 0;
		while (num < 20 && num < this.m_inFrustumCount)
		{
			FogVolumePrimitiveManager.PrimitiveData primitiveData = this.m_primitivesInFrustum[num];
			Vector3 position = primitiveData.Transform.position;
			this.m_primitivePos[num] = base.gameObject.transform.InverseTransformPoint(position);
			this.m_primitiveTf[num].SetTRS(position, Quaternion.Inverse(primitiveData.Transform.rotation) * rotation, Vector3.one);
			this.m_primitiveScale[num] = primitiveData.Primitive.GetPrimitiveScale;
			this.m_primitiveData[num] = new Vector4((primitiveData.PrimitiveType == EFogVolumePrimitiveType.Box) ? 0.5f : 1.5f, primitiveData.Primitive.IsSubtractive ? 1.5f : 0.5f, 0f, 0f);
			int visiblePrimitiveCount = this.VisiblePrimitiveCount;
			this.VisiblePrimitiveCount = visiblePrimitiveCount + 1;
			num++;
		}
	}

	// Token: 0x0400014D RID: 333
	private FogVolume m_fogVolume;

	// Token: 0x0400014E RID: 334
	private FogVolumeData m_fogVolumeData;

	// Token: 0x0400014F RID: 335
	private Camera m_camera;

	// Token: 0x04000150 RID: 336
	private BoxCollider m_boxCollider;

	// Token: 0x04000151 RID: 337
	private Transform m_pointOfInterestTf;

	// Token: 0x04000152 RID: 338
	private Vector3 m_pointOfInterest = Vector3.zero;

	// Token: 0x04000153 RID: 339
	private List<FogVolumePrimitiveManager.PrimitiveData> m_primitives;

	// Token: 0x04000154 RID: 340
	private List<FogVolumePrimitiveManager.PrimitiveData> m_primitivesInFrustum;

	// Token: 0x04000155 RID: 341
	private int m_inFrustumCount;

	// Token: 0x04000156 RID: 342
	private Plane[] FrustumPlanes;

	// Token: 0x04000157 RID: 343
	private readonly Vector4[] m_primitivePos = new Vector4[20];

	// Token: 0x04000158 RID: 344
	private readonly Vector4[] m_primitiveScale = new Vector4[20];

	// Token: 0x04000159 RID: 345
	private readonly Matrix4x4[] m_primitiveTf = new Matrix4x4[20];

	// Token: 0x0400015A RID: 346
	private readonly Vector4[] m_primitiveData = new Vector4[20];

	// Token: 0x0400015B RID: 347
	private const int InvalidIndex = -1;

	// Token: 0x0400015C RID: 348
	private const int MaxVisiblePrimitives = 20;

	// Token: 0x0400015D RID: 349
	private const int MaxPrimitivesCount = 1000;

	// Token: 0x02000227 RID: 551
	protected class PrimitiveData
	{
		// Token: 0x0600163C RID: 5692 RVA: 0x0006ADD4 File Offset: 0x00068FD4
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

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x0600163D RID: 5693 RVA: 0x0006AE28 File Offset: 0x00069028
		// (set) Token: 0x0600163E RID: 5694 RVA: 0x0006AE30 File Offset: 0x00069030
		public EFogVolumePrimitiveType PrimitiveType { get; set; }

		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x0600163F RID: 5695 RVA: 0x0006AE39 File Offset: 0x00069039
		// (set) Token: 0x06001640 RID: 5696 RVA: 0x0006AE41 File Offset: 0x00069041
		public FogVolumePrimitive Primitive { get; set; }

		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x06001641 RID: 5697 RVA: 0x0006AE4A File Offset: 0x0006904A
		// (set) Token: 0x06001642 RID: 5698 RVA: 0x0006AE52 File Offset: 0x00069052
		public Transform Transform { get; set; }

		// Token: 0x170005FC RID: 1532
		// (get) Token: 0x06001643 RID: 5699 RVA: 0x0006AE5B File Offset: 0x0006905B
		// (set) Token: 0x06001644 RID: 5700 RVA: 0x0006AE63 File Offset: 0x00069063
		public Renderer Renderer { get; set; }

		// Token: 0x170005FD RID: 1533
		// (get) Token: 0x06001645 RID: 5701 RVA: 0x0006AE6C File Offset: 0x0006906C
		// (set) Token: 0x06001646 RID: 5702 RVA: 0x0006AE74 File Offset: 0x00069074
		public float SqDistance { get; set; }

		// Token: 0x170005FE RID: 1534
		// (get) Token: 0x06001647 RID: 5703 RVA: 0x0006AE7D File Offset: 0x0006907D
		// (set) Token: 0x06001648 RID: 5704 RVA: 0x0006AE85 File Offset: 0x00069085
		public float Distance2Camera { get; set; }

		// Token: 0x170005FF RID: 1535
		// (get) Token: 0x06001649 RID: 5705 RVA: 0x0006AE8E File Offset: 0x0006908E
		// (set) Token: 0x0600164A RID: 5706 RVA: 0x0006AE96 File Offset: 0x00069096
		public Bounds Bounds { get; set; }

		// Token: 0x0600164B RID: 5707 RVA: 0x0006AE9F File Offset: 0x0006909F
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
