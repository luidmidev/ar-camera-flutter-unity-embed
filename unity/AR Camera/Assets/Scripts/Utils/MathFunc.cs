using Models;
using UnityEngine;
using static Utils.SizeUtils;

namespace Utils
{
    public static class MathFunc
    {
        public static bool IsVerticalNormal(Pose pose) => Vector2.Dot(pose.up, Vector2.up) > 0.5f;

        public static Size CalculateScale(float? width, float? height, Texture texture)
        {
            if (width == null && height == null) return NormalizeWithWidth(texture.width, texture.height, 100);
            if (width != null && height != null) return new Size(width.Value, height.Value);
            return width != null ? NormalizeWithWidth(texture.width, texture.height, width.Value) : NormalizeWithHeight(texture.width, texture.height, height.Value);
        }

        public static float CalculateSlope(Vector2 a, Vector2 b) => (b.y - a.y) / (b.x - a.x);
    }
}