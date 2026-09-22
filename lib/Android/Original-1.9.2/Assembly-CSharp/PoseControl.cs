using System;
using UnityEngine;

// Token: 0x02000138 RID: 312
public class PoseControl : MonoBehaviour
{
	// Token: 0x0600080C RID: 2060 RVA: 0x00045C5C File Offset: 0x0004405C
	private void Start()
	{
		this.lookOverride = null;
		this.timeSinceOverride = 0f;
		this.lookVel = Vector3.zero;
		this.interestingItems = this.interestingItemParent.GetComponentsInChildren<Transform>();
		this.handBlend = 1f;
		this.leftHandReset = new Vector3(0f, 0.522f, 0f);
		this.rightHandReset = new Vector3(0f, 0.522f, 0f);
		this.leftGripCenterReset = new Vector3(0f, 0.182f, 0.09f);
		this.rightGripCenterReset = new Vector3(0f, 0.171f, 0.063f);
		this.LateUpdate();
		this.LateUpdate();
	}

	// Token: 0x0600080D RID: 2061 RVA: 0x00045D6F File Offset: 0x0004416F
	public void StopAnimator()
	{
		this.anim.gameObject.SetActive(false);
	}

	// Token: 0x0600080E RID: 2062 RVA: 0x00045D82 File Offset: 0x00044182
	public void StartAnimator()
	{
		this.anim.gameObject.SetActive(true);
	}

	// Token: 0x0600080F RID: 2063 RVA: 0x00045D98 File Offset: 0x00044198
	public void PlayOpeningAnimation()
	{
		this.animating = true;
		this.blendAmt = 1f;
		this.handBlend = 0f;
		int layerIndex = this.anim.GetLayerIndex("Animation");
		for (int i = 0; i < this.anim.layerCount; i++)
		{
			if (i == layerIndex)
			{
				this.anim.SetLayerWeight(i, 1f);
			}
			else
			{
				this.anim.SetLayerWeight(i, 0f);
			}
		}
		this.anim.SetTrigger("WakeUp");
		this.potAnim.Play("Rattle");
		this.pc.PauseInput(6f);
		this.LateUpdate();
		this.LateUpdate();
	}

	// Token: 0x06000810 RID: 2064 RVA: 0x00045E5C File Offset: 0x0004425C
	private float GetNearestSplineZ(Vector3 pos)
	{
		int num = 0;
		int num2 = 0;
		float num3 = 1E+09f;
		float num4 = 9999999f;
		for (int i = 0; i < this.controlPoints.Length; i++)
		{
			float sqrMagnitude = (this.controlPoints[i] - pos).sqrMagnitude;
			if (sqrMagnitude < num4)
			{
				num2 = num;
				num3 = num4;
				num4 = sqrMagnitude;
				num = i;
			}
			else if (sqrMagnitude < num3)
			{
				num2 = i;
				num3 = sqrMagnitude;
			}
		}
		Vector2 vector = this.controlPoints[num2] - this.controlPoints[num];
		Vector2 vector2 = this.controlPoints[num] - pos;
		float num5 = -1f * (Vector2.Dot(vector2, vector) / vector.sqrMagnitude);
		return Vector3.Lerp(this.controlPoints[num], this.controlPoints[num2], num5).z - 0.5f;
	}

	// Token: 0x06000811 RID: 2065 RVA: 0x00045F90 File Offset: 0x00044390
	private void LateUpdate()
	{
		float num = 100f;
		float num2 = 2f;
		float num3 = 1f;
		float num4 = 5f;
		if (this.lookOverride == null)
		{
			this.timeSinceOverride += Time.deltaTime;
			this.lookTarget.position = Vector3.SmoothDamp(this.lookTarget.position, this.tip.position, ref this.lookVel, 0.2f);
			if (global::UnityEngine.Random.value < Time.deltaTime / num2 && this.timeSinceOverride > num4)
			{
				foreach (Transform transform in this.interestingItems)
				{
					if ((this.dudeMeshHub.position + new Vector3(2f, 2f, 0f) - transform.position).sqrMagnitude < num)
					{
						this.lookOverride = transform;
						this.timeSinceOverride = 0f;
						break;
					}
				}
			}
		}
		else
		{
			this.timeSinceOverride += Time.deltaTime;
			this.lookTarget.position = Vector3.SmoothDamp(this.lookTarget.position, this.lookOverride.position, ref this.lookVel, 0.2f);
			if ((global::UnityEngine.Random.value < Time.deltaTime / num2 && this.timeSinceOverride > num3) || (this.dudeMeshHub.position + new Vector3(2f, 2f, 0f) - this.lookOverride.position).sqrMagnitude > num * 1.5f)
			{
				this.lookOverride = null;
				this.timeSinceOverride = 0f;
			}
		}
		float jointAngle = this.hinge.jointAngle;
		float nearestSplineZ = this.GetNearestSplineZ(this.center.position);
		this.dudeMeshHub.position = new Vector3(this.dudeMeshHub.position.x, this.dudeMeshHub.position.y, nearestSplineZ);
		this.potMeshHub.position = new Vector3(this.potMeshHub.position.x, this.potMeshHub.position.y, nearestSplineZ - this.dudePotOffset);
		float num5;
		if (this.slider.jointTranslation >= 0f)
		{
			num5 = -this.slider.jointTranslation / this.slider.limits.max;
		}
		else
		{
			num5 = this.slider.jointTranslation / this.slider.limits.min;
		}
		int layerIndex = this.anim.GetLayerIndex("Animation");
		if (this.animating)
		{
			if (this.blendAmt <= 0f)
			{
				this.animating = false;
				this.anim.StopPlayback();
			}
			else
			{
				for (int j = 0; j < this.anim.layerCount; j++)
				{
					if (j == layerIndex)
					{
						this.anim.SetLayerWeight(j, this.blendAmt);
					}
					else
					{
						this.anim.SetLayerWeight(j, 1f - this.blendAmt);
					}
				}
			}
		}
		this.leftHand.localPosition = this.leftHandReset;
		this.rightHand.localPosition = this.rightHandReset;
		this.leftHandGripCenter.localPosition = this.leftGripCenterReset;
		this.rightHandGripCenter.localPosition = this.rightGripCenterReset;
		this.anim.SetFloat("Angle", Mathf.Repeat(-jointAngle + 90f - this.hinge.referenceAngle - this.hammerMeshHub.localEulerAngles.z, 360f) / 180f - 1f);
		this.anim.SetFloat("Extension", -num5 * 0.25f);
		this.anim.Update(Time.deltaTime);
		this.hammerMeshHub.position = new Vector3(this.hammerMeshHub.position.x, this.hammerMeshHub.position.y, this.rightHandGripCenter.position.z);
		Quaternion rotation = this.leftHand.rotation;
		Quaternion rotation2 = this.rightHand.rotation;
		Quaternion rotation3 = this.leftElbow.rotation;
		Quaternion rotation4 = this.rightElbow.rotation;
		Vector3 position = this.leftHand.position;
		Vector3 position2 = this.rightHand.position;
		for (int k = 0; k < 3; k++)
		{
			Vector3 vector = this.leftHandGripCenter.position - this.gripCenterLeft.position;
			this.leftHand.position -= vector;
			Vector3 vector2 = this.rightHandGripCenter.position - this.gripCenterRight.position;
			this.rightHand.position -= vector2;
			this.leftElbow.rotation *= Quaternion.FromToRotation(Vector3.up, this.leftElbow.InverseTransformPoint(this.leftHand.position));
			this.rightElbow.rotation *= Quaternion.FromToRotation(Vector3.up, this.rightElbow.InverseTransformPoint(this.rightHand.position));
			this.leftHand.rotation = rotation;
			this.rightHand.rotation = rotation2;
		}
		this.leftElbow.rotation = Quaternion.Slerp(rotation3, this.leftElbow.rotation, this.handBlend);
		this.rightElbow.rotation = Quaternion.Slerp(rotation4, this.rightElbow.rotation, this.handBlend);
		this.leftHand.position = Vector3.Lerp(position, this.leftHand.position, this.handBlend);
		this.rightHand.position = Vector3.Lerp(position2, this.rightHand.position, this.handBlend);
		this.leftHand.rotation = Quaternion.Slerp(rotation, this.leftHand.rotation, this.handBlend);
		this.rightHand.rotation = Quaternion.Slerp(rotation2, this.rightHand.rotation, this.handBlend);
	}

	// Token: 0x06000812 RID: 2066 RVA: 0x0004662C File Offset: 0x00044A2C
	public void EnableHammer(bool enable)
	{
		this.tip.GetComponent<Rigidbody2D>().simulated = enable;
	}

	// Token: 0x040007CC RID: 1996
	public AnimationClip wakeup;

	// Token: 0x040007CD RID: 1997
	public Animator anim;

	// Token: 0x040007CE RID: 1998
	public Animator potAnim;

	// Token: 0x040007CF RID: 1999
	public Transform leftHand;

	// Token: 0x040007D0 RID: 2000
	public Transform rightHand;

	// Token: 0x040007D1 RID: 2001
	public Transform leftElbow;

	// Token: 0x040007D2 RID: 2002
	public Transform rightElbow;

	// Token: 0x040007D3 RID: 2003
	public Transform center;

	// Token: 0x040007D4 RID: 2004
	public Transform handle;

	// Token: 0x040007D5 RID: 2005
	public Transform gripCenterLeft;

	// Token: 0x040007D6 RID: 2006
	public Transform gripCenterRight;

	// Token: 0x040007D7 RID: 2007
	public Transform rightHandGripCenter;

	// Token: 0x040007D8 RID: 2008
	public Transform leftHandGripCenter;

	// Token: 0x040007D9 RID: 2009
	public SliderJoint2D slider;

	// Token: 0x040007DA RID: 2010
	public HingeJoint2D hinge;

	// Token: 0x040007DB RID: 2011
	public Vector3 off;

	// Token: 0x040007DC RID: 2012
	public Transform lookTarget;

	// Token: 0x040007DD RID: 2013
	private Transform lookOverride;

	// Token: 0x040007DE RID: 2014
	private float timeSinceOverride;

	// Token: 0x040007DF RID: 2015
	private Vector3 lookVel;

	// Token: 0x040007E0 RID: 2016
	private Transform[] interestingItems;

	// Token: 0x040007E1 RID: 2017
	public Transform interestingItemParent;

	// Token: 0x040007E2 RID: 2018
	private bool animating;

	// Token: 0x040007E3 RID: 2019
	public float blendAmt;

	// Token: 0x040007E4 RID: 2020
	public float handBlend;

	// Token: 0x040007E5 RID: 2021
	public PlayerControl pc;

	// Token: 0x040007E7 RID: 2023
	public Transform dudeMeshHub;

	// Token: 0x040007E8 RID: 2024
	public Transform hammerMeshHub;

	// Token: 0x040007E9 RID: 2025
	public Transform potMeshHub;

	// Token: 0x040007EA RID: 2026
	private float dudePotOffset = 0.02f;

	// Token: 0x040007EB RID: 2027
	public Transform tip;

	// Token: 0x040007EC RID: 2028
	private Vector3 leftHandReset;

	// Token: 0x040007ED RID: 2029
	private Vector3 rightHandReset;

	// Token: 0x040007EE RID: 2030
	private Vector3 leftGripCenterReset;

	// Token: 0x040007EF RID: 2031
	private Vector3 rightGripCenterReset;

	// Token: 0x040007F0 RID: 2032
	private Vector3[] controlPoints;
}
