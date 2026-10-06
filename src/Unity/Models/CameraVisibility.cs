using GOILauncher.Multiplayer.Unity.Extensions;
using UnityEngine;

namespace GOILauncher.Multiplayer.Unity.Models
{
    /// <summary>
    /// 远端实例该不该渲染，只取决于本地相机的视野和它自己的位置，与对方有没有发状态无关。
    /// 拆成纯函数有两个理由：它是这段裁剪逻辑里唯一带判断的部分，拆出来才拿得到测试覆盖；
    /// 以及它只收 float 和 <see cref="Vector3"/>，不碰任何 Unity 引擎对象，所以没有 Unity
    /// 运行时的测试进程也能调用——改成收 <c>Camera</c> 就不行了，<c>Camera.transform</c> 是 ECall。
    /// </summary>
    public static class CameraVisibility
    {
        /// <summary>
        /// 判断世界坐标点是否落在正交相机视野内，并向外放宽一个玩家的尺寸，
        /// 让刚要进入视野边缘的玩家已经处于渲染状态。只看 X / Y，Z 不参与。
        /// </summary>
        /// <remarks>
        /// 四条边的外扩量沿用改动前的取值：左、右、下是 <see cref="GameConstants.PlayerWidth"/>，
        /// 只有上边是 <see cref="GameConstants.PlayerHeight"/>。下边用宽度而不是高度是否为笔误
        /// 尚未确认，这里原样保留，由测试把当前取值钉住，避免被顺手改掉。
        /// </remarks>
        public static bool IsWithinView(
            Vector3 cameraPosition, float orthographicSize, float aspect, Vector3 point)
        {
            var halfHeight = orthographicSize;
            var halfWidth = aspect * halfHeight;

            return point.IsInRectangle(
                cameraPosition.x - halfWidth - GameConstants.PlayerWidth,
                cameraPosition.y + halfHeight + GameConstants.PlayerHeight,
                cameraPosition.x + halfWidth + GameConstants.PlayerWidth,
                cameraPosition.y - halfHeight - GameConstants.PlayerWidth);
        }
    }
}
