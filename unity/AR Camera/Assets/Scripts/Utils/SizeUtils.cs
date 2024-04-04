using Models;

namespace Utils
{
    public static class SizeUtils
    {
        public static Size NormalizeWithWidth(float width, float height, float newWidthInCm)
        {
            var newHeight = newWidthInCm * height / width;
            return new Size(newWidthInCm, newHeight);
        }

        public static Size NormalizeWithHeight(float width, float height, float newHeightInCm)
        {
            var newWidth = newHeightInCm * width / height;
            return new Size(newWidth, newHeightInCm);
        }
    }
}