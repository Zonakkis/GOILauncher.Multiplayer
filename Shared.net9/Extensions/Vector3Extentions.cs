using UnityEngine;

namespace GOILauncher.Multiplayer.Shared.Extensions
{
    public static class Vector3Extentions
    {
        public static bool IsInRectangle(this Vector3 point, Vector2 topLeft, Vector2 bottomRight)
        {
            return point.x >= topLeft.x && point.x <= bottomRight.x && point.y >= bottomRight.y && point.y <= topLeft.y;
        }
        public static bool IsInRectangle(this Vector3 point, float left, float top, float right, float bottom)
        {
            return point.x >= left && point.x <= right && point.y >= bottom && point.y <= top;
        }
    }
}
