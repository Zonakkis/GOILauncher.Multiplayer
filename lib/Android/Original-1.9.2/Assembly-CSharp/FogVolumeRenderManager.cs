using System;
using UnityEngine;

// Token: 0x0200001D RID: 29
public class FogVolumeRenderManager : MonoBehaviour
{
	// Token: 0x060000D6 RID: 214 RVA: 0x0000A42C File Offset: 0x0000882C
	public void RenderEye(RenderTexture targetTexture, Vector3 camPosition, Quaternion camRotation, Matrix4x4 camProjectionMatrix, Shader CameraShader)
	{
		this.SecondaryCamera.transform.position = camPosition;
		this.SecondaryCamera.transform.rotation = camRotation;
		this.SecondaryCamera.projectionMatrix = camProjectionMatrix;
		this.SecondaryCamera.targetTexture = targetTexture;
		if (CameraShader != null)
		{
			this.SecondaryCamera.RenderWithShader(CameraShader, "RenderType");
		}
		else
		{
			this.SecondaryCamera.Render();
		}
	}

	// Token: 0x0400018D RID: 397
	public Camera SceneCamera;

	// Token: 0x0400018E RID: 398
	public Camera SecondaryCamera;
}
