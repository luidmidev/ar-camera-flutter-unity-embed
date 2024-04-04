using System;
using Newtonsoft.Json;

namespace Models
{
    [Serializable]
    public struct Size
    {
        [JsonProperty("width")] public float? Width;

        [JsonProperty("height")] public float? Height;

        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        public Size(float? width, float? height)
        {
            Width = width;
            Height = height;
        }


        public override string ToString()
        {
            return $"Width: {Width}, Height: {Height}";
        }

        public static Size operator /(Size size, float value)
        {
            return new Size(size.Width / value, size.Height / value);
        }

        public static Size operator *(Size size, float value)
        {
            return new Size(size.Width * value, size.Height * value);
        }


    }
}