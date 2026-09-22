using System;
using UnityEngine;

// Token: 0x02000040 RID: 64
public class PoseControl : MonoBehaviour
{
	// Token: 0x060001F2 RID: 498 RVA: 0x000131C4 File Offset: 0x000113C4
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

	// Token: 0x060001F3 RID: 499 RVA: 0x000132FE File Offset: 0x000114FE
	public void StopAnimator()
	{
		this.anim.gameObject.SetActive(false);
	}

	// Token: 0x060001F4 RID: 500 RVA: 0x00013311 File Offset: 0x00011511
	public void StartAnimator()
	{
		this.anim.gameObject.SetActive(true);
		this.LateUpdate();
		this.anim.Update(0.1f);
	}

	// Token: 0x060001F5 RID: 501 RVA: 0x0001333C File Offset: 0x0001153C
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

	// Token: 0x060001F6 RID: 502 RVA: 0x000133F4 File Offset: 0x000115F4
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

	// Token: 0x060001F7 RID: 503 RVA: 0x000134F8 File Offset: 0x000116F8
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
			if (Random.value < Time.deltaTime / num2 && this.timeSinceOverride > num4)
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
			if ((Random.value < Time.deltaTime / num2 && this.timeSinceOverride > num3) || (this.dudeMeshHub.position + new Vector3(2f, 2f, 0f) - this.lookOverride.position).sqrMagnitude > num * 1.5f)
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

	// Token: 0x060001F8 RID: 504 RVA: 0x00013B83 File Offset: 0x00011D83
	public void EnableHammer(bool enable)
	{
		this.tip.GetComponent<Rigidbody2D>().simulated = enable;
	}

	// Token: 0x04000342 RID: 834
	public AnimationClip wakeup;

	// Token: 0x04000343 RID: 835
	public Animator anim;

	// Token: 0x04000344 RID: 836
	public Animator potAnim;

	// Token: 0x04000345 RID: 837
	public Transform leftHand;

	// Token: 0x04000346 RID: 838
	public Transform rightHand;

	// Token: 0x04000347 RID: 839
	public Transform leftElbow;

	// Token: 0x04000348 RID: 840
	public Transform rightElbow;

	// Token: 0x04000349 RID: 841
	public Transform center;

	// Token: 0x0400034A RID: 842
	public Transform handle;

	// Token: 0x0400034B RID: 843
	public Transform gripCenterLeft;

	// Token: 0x0400034C RID: 844
	public Transform gripCenterRight;

	// Token: 0x0400034D RID: 845
	public Transform rightHandGripCenter;

	// Token: 0x0400034E RID: 846
	public Transform leftHandGripCenter;

	// Token: 0x0400034F RID: 847
	public SliderJoint2D slider;

	// Token: 0x04000350 RID: 848
	public HingeJoint2D hinge;

	// Token: 0x04000351 RID: 849
	public Vector3 off;

	// Token: 0x04000352 RID: 850
	public Transform lookTarget;

	// Token: 0x04000353 RID: 851
	private Transform lookOverride;

	// Token: 0x04000354 RID: 852
	private float timeSinceOverride;

	// Token: 0x04000355 RID: 853
	private Vector3 lookVel;

	// Token: 0x04000356 RID: 854
	private Transform[] interestingItems;

	// Token: 0x04000357 RID: 855
	public Transform interestingItemParent;

	// Token: 0x04000358 RID: 856
	private bool animating;

	// Token: 0x04000359 RID: 857
	public float blendAmt;

	// Token: 0x0400035A RID: 858
	public float handBlend;

	// Token: 0x0400035B RID: 859
	public PlayerControl pc;

	// Token: 0x0400035D RID: 861
	public Transform dudeMeshHub;

	// Token: 0x0400035E RID: 862
	public Transform hammerMeshHub;

	// Token: 0x0400035F RID: 863
	public Transform potMeshHub;

	// Token: 0x04000360 RID: 864
	private float dudePotOffset = 0.02f;

	// Token: 0x04000361 RID: 865
	public Transform tip;

	// Token: 0x04000362 RID: 866
	private Vector3 leftHandReset;

	// Token: 0x04000363 RID: 867
	private Vector3 rightHandReset;

	// Token: 0x04000364 RID: 868
	private Vector3 leftGripCenterReset;

	// Token: 0x04000365 RID: 869
	private Vector3 rightGripCenterReset;

	// Token: 0x04000366 RID: 870
	private float leftForearmLength;

	// Token: 0x04000367 RID: 871
	private float rightForearmLength;

	// Token: 0x04000368 RID: 872
	private Vector3[] controlPoints;
}
