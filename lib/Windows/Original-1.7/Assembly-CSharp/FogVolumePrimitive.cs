using System;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000014 RID: 20
[ExecuteInEditMode]
public class FogVolumePrimitive : MonoBehaviour
{
	// Token: 0x060000A9 RID: 169 RVA: 0x00008A29 File Offset: 0x00006C29
	public FogVolumePrimitive()
	{
		this.SphereColl = null;
		this.BoxColl = null;
	}

	// Token: 0x17000017 RID: 23
	// (get) Token: 0x060000AA RID: 170 RVA: 0x00008A51 File Offset: 0x00006C51
	public Transform GetTransform
	{
		get
		{
			return base.gameObject.transform;
		}
	}

	// Token: 0x17000018 RID: 24
	// (get) Token: 0x060000AB RID: 171 RVA: 0x00008A60 File Offset: 0x00006C60
	public Vector3 GetPrimitiveScale
	{
		get
		{
			return new Vector3(Mathf.Max(this.MinScale, base.transform.lossyScale.x), Mathf.Max(this.MinScale, base.transform.lossyScale.y), Mathf.Max(this.MinScale, base.transform.lossyScale.z));
		}
	}

	// Token: 0x17000019 RID: 25
	// (get) Token: 0x060000AC RID: 172 RVA: 0x00008AC4 File Offset: 0x00006CC4
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

	// Token: 0x060000AD RID: 173 RVA: 0x00008B2C File Offset: 0x00006D2C
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

	// Token: 0x060000AE RID: 174 RVA: 0x00008B9C File Offset: 0x00006D9C
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

	// Token: 0x04000142 RID: 322
	public BoxCollider BoxColl;

	// Token: 0x04000143 RID: 323
	public SphereCollider SphereColl;

	// Token: 0x04000144 RID: 324
	public bool IsPersistent = true;

	// Token: 0x04000145 RID: 325
	public EFogVolumePrimitiveType Type;

	// Token: 0x04000146 RID: 326
	public bool IsSubtractive;

	// Token: 0x04000147 RID: 327
	public Material PrimitiveMaterial;

	// Token: 0x04000148 RID: 328
	private GameObject Primitive;

	// Token: 0x04000149 RID: 329
	private Renderer _Renderer;

	// Token: 0x0400014A RID: 330
	private readonly float MinScale = 0.0001f;
}
