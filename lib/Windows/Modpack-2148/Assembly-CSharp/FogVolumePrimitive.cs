using System;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000021 RID: 33
[ExecuteInEditMode]
public class FogVolumePrimitive : MonoBehaviour
{
	// Token: 0x060000BB RID: 187 RVA: 0x00002D2E File Offset: 0x00000F2E
	public FogVolumePrimitive()
	{
		this.SphereColl = null;
		this.BoxColl = null;
	}

	// Token: 0x1700001E RID: 30
	// (get) Token: 0x060000BC RID: 188 RVA: 0x00002D56 File Offset: 0x00000F56
	public Transform GetTransform
	{
		get
		{
			return base.gameObject.transform;
		}
	}

	// Token: 0x1700001F RID: 31
	// (get) Token: 0x060000BD RID: 189 RVA: 0x0001A480 File Offset: 0x00018680
	public Vector3 GetPrimitiveScale
	{
		get
		{
			return new Vector3(Mathf.Max(this.MinScale, base.transform.lossyScale.x), Mathf.Max(this.MinScale, base.transform.lossyScale.y), Mathf.Max(this.MinScale, base.transform.lossyScale.z));
		}
	}

	// Token: 0x17000020 RID: 32
	// (get) Token: 0x060000BE RID: 190 RVA: 0x0001A4E4 File Offset: 0x000186E4
	public Bounds Bounds
	{
		get
		{
			if (this.BoxColl != null)
			{
				return this.BoxColl.bounds;
			}
			if (this.SphereColl != null)
			{
				return this.SphereColl.bounds;
			}
			return new Bounds(base.gameObject.transform.position, base.gameObject.transform.lossyScale);
		}
	}

	// Token: 0x060000BF RID: 191 RVA: 0x0001A54C File Offset: 0x0001874C
	public void AddColliderIfNeccessary(EFogVolumePrimitiveType _type)
	{
		this.Type = _type;
		switch (this.Type)
		{
		case EFogVolumePrimitiveType.Box:
			if (this.BoxColl == null)
			{
				this.BoxColl = base.gameObject.AddComponent<BoxCollider>();
				return;
			}
			break;
		case EFogVolumePrimitiveType.Sphere:
			if (this.SphereColl == null)
			{
				this.SphereColl = base.gameObject.AddComponent<SphereCollider>();
			}
			break;
		case EFogVolumePrimitiveType.None:
			break;
		default:
			return;
		}
	}

	// Token: 0x060000C0 RID: 192 RVA: 0x0001A5BC File Offset: 0x000187BC
	private void OnEnable()
	{
		this.Primitive = base.gameObject;
		this._Renderer = this.Primitive.GetComponent<MeshRenderer>();
		if (!this.PrimitiveMaterial)
		{
			this.PrimitiveMaterial = (Material)Resources.Load("PrimitiveMaterial");
		}
		this._Renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		this._Renderer.lightProbeUsage = LightProbeUsage.Off;
		this._Renderer.shadowCastingMode = ShadowCastingMode.Off;
		this._Renderer.receiveShadows = false;
		base.GetComponent<MeshRenderer>().material = this.PrimitiveMaterial;
		this.BoxColl = base.GetComponent<BoxCollider>();
		this.SphereColl = base.GetComponent<SphereCollider>();
		if (this.BoxColl == null && this.SphereColl == null)
		{
			this.BoxColl = base.gameObject.AddComponent<BoxCollider>();
			this.Type = EFogVolumePrimitiveType.Box;
			return;
		}
		if (this.BoxColl != null)
		{
			this.Type = EFogVolumePrimitiveType.Box;
			return;
		}
		if (this.SphereColl != null)
		{
			this.Type = EFogVolumePrimitiveType.Sphere;
			return;
		}
		this.Type = EFogVolumePrimitiveType.None;
	}

	// Token: 0x04000177 RID: 375
	public BoxCollider BoxColl;

	// Token: 0x04000178 RID: 376
	public SphereCollider SphereColl;

	// Token: 0x04000179 RID: 377
	public bool IsPersistent = true;

	// Token: 0x0400017A RID: 378
	public EFogVolumePrimitiveType Type;

	// Token: 0x0400017B RID: 379
	public bool IsSubtractive;

	// Token: 0x0400017C RID: 380
	public Material PrimitiveMaterial;

	// Token: 0x0400017D RID: 381
	private GameObject Primitive;

	// Token: 0x0400017E RID: 382
	private Renderer _Renderer;

	// Token: 0x0400017F RID: 383
	private readonly float MinScale = 0.0001f;
}
