using System;
using UnityEngine;

// Token: 0x02000066 RID: 102
public class PoseControl : MonoBehaviour
{
	// Token: 0x06000276 RID: 630 RVA: 0x00025144 File Offset: 0x00023344
	private void Awake()
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
		this.leftForearmLength = this.leftHand.localPosition.magnitude;
		this.rightForearmLength = this.rightHand.localPosition.magnitude;
		this.LateUpdate();
		this.LateUpdate();
	}

	// Token: 0x06000277 RID: 631 RVA: 0x00025280 File Offset: 0x00023480
	public void StopAnimator()
	{
		try
		{
			this.anim.gameObject.SetActive(false);
		}
		catch
		{
		}
	}

	// Token: 0x06000278 RID: 632 RVA: 0x000252B4 File Offset: 0x000234B4
	public void StartAnimator()
	{
		try
		{
			this.anim.gameObject.SetActive(true);
		}
		catch
		{
		}
		try
		{
			this.LateUpdate();
		}
		catch
		{
		}
		try
		{
			this.anim.Update(0.1f);
		}
		catch
		{
		}
	}

	// Token: 0x06000279 RID: 633 RVA: 0x00025324 File Offset: 0x00023524
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
		try
		{
			if (SettingsManager.fastStart)
			{
				this.pc.PauseInput(SettingsManager.fastStartTime);
			}
			else
			{
				this.pc.PauseInput(6f);
			}
		}
		catch
		{
		}
		try
		{
			SettingsManager.timer.started = true;
		}
		catch
		{
		}
		try
		{
			this.LateUpdate();
			if (SettingsManager.fastStart)
			{
				this.anim.Update(6f - SettingsManager.fastStartTime);
				this.potAnim.Update(6f - SettingsManager.fastStartTime);
			}
		}
		catch
		{
		}
		try
		{
			this.LateUpdate();
			if (SettingsManager.fastStart)
			{
				this.anim.Update(6f - SettingsManager.fastStartTime);
				this.potAnim.Update(6f - SettingsManager.fastStartTime);
			}
		}
		catch
		{
		}
	}

	// Token: 0x0600027A RID: 634 RVA: 0x000254B0 File Offset: 0x000236B0
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

	// Token: 0x0600027B RID: 635 RVA: 0x000255A0 File Offset: 0x000237A0
	private void LateUpdate()
	{
		if (!this.hinge)
		{
			this.hinge = this.pc.hj;
		}
		if (!this.slider)
		{
			this.slider = this.pc.sj;
		}
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

	// Token: 0x0600027C RID: 636 RVA: 0x00003D2A File Offset: 0x00001F2A
	public void EnableHammer(bool enable)
	{
		this.tip.GetComponent<Rigidbody2D>().simulated = enable;
	}

	// Token: 0x040003E5 RID: 997
	public AnimationClip wakeup;

	// Token: 0x040003E6 RID: 998
	public Animator anim;

	// Token: 0x040003E7 RID: 999
	public Animator potAnim;

	// Token: 0x040003E8 RID: 1000
	public Transform leftHand;

	// Token: 0x040003E9 RID: 1001
	public Transform rightHand;

	// Token: 0x040003EA RID: 1002
	public Transform leftElbow;

	// Token: 0x040003EB RID: 1003
	public Transform rightElbow;

	// Token: 0x040003EC RID: 1004
	public Transform center;

	// Token: 0x040003ED RID: 1005
	public Transform handle;

	// Token: 0x040003EE RID: 1006
	public Transform gripCenterLeft;

	// Token: 0x040003EF RID: 1007
	public Transform gripCenterRight;

	// Token: 0x040003F0 RID: 1008
	public Transform rightHandGripCenter;

	// Token: 0x040003F1 RID: 1009
	public Transform leftHandGripCenter;

	// Token: 0x040003F2 RID: 1010
	public SliderJoint2D slider;

	// Token: 0x040003F3 RID: 1011
	public HingeJoint2D hinge;

	// Token: 0x040003F4 RID: 1012
	public Vector3 off;

	// Token: 0x040003F5 RID: 1013
	public Transform lookTarget;

	// Token: 0x040003F6 RID: 1014
	private Transform lookOverride;

	// Token: 0x040003F7 RID: 1015
	private float timeSinceOverride;

	// Token: 0x040003F8 RID: 1016
	private Vector3 lookVel;

	// Token: 0x040003F9 RID: 1017
	private Transform[] interestingItems;

	// Token: 0x040003FA RID: 1018
	public Transform interestingItemParent;

	// Token: 0x040003FB RID: 1019
	private bool animating;

	// Token: 0x040003FC RID: 1020
	public float blendAmt;

	// Token: 0x040003FD RID: 1021
	public float handBlend;

	// Token: 0x040003FE RID: 1022
	public PlayerControl pc;

	// Token: 0x04000400 RID: 1024
	public Transform dudeMeshHub;

	// Token: 0x04000401 RID: 1025
	public Transform hammerMeshHub;

	// Token: 0x04000402 RID: 1026
	public Transform potMeshHub;

	// Token: 0x04000403 RID: 1027
	private float dudePotOffset = 0.02f;

	// Token: 0x04000404 RID: 1028
	public Transform tip;

	// Token: 0x04000405 RID: 1029
	private Vector3 leftHandReset;

	// Token: 0x04000406 RID: 1030
	private Vector3 rightHandReset;

	// Token: 0x04000407 RID: 1031
	private Vector3 leftGripCenterReset;

	// Token: 0x04000408 RID: 1032
	private Vector3 rightGripCenterReset;

	// Token: 0x04000409 RID: 1033
	private float leftForearmLength;

	// Token: 0x0400040A RID: 1034
	private float rightForearmLength;

	// Token: 0x0400040B RID: 1035
	private Vector3[] controlPoints;
}
