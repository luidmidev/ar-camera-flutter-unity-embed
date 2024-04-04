using System;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Config
{
    public class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var x = jObject.GetValueOrDefault("x", 0f);
            var y = jObject.GetValueOrDefault("y", 0f);
            var z = jObject.GetValueOrDefault("z", 0f);
            return new Vector3(x, y, z);
        }
    }

    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var x = jObject.GetValueOrDefault("x", 0f);
            var y = jObject.GetValueOrDefault("y", 0f);
            return new Vector2(x, y);
        }
    }

    public class Vector4JsonConverter : JsonConverter<Vector4>
    {
        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var x = jObject.GetValueOrDefault("x", 0f);
            var y = jObject.GetValueOrDefault("y", 0f);
            var z = jObject.GetValueOrDefault("z", 0f);
            var w = jObject.GetValueOrDefault("w", 0f);
            return new Vector4(x, y, z, w);
        }
    }

    public class QuaternionJsonConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var x = jObject.GetValueOrDefault("x", 0f);
            var y = jObject.GetValueOrDefault("y", 0f);
            var z = jObject.GetValueOrDefault("z", 0f);
            var w = jObject.GetValueOrDefault("w", 0f);
            return new Quaternion(x, y, z, w);
        }
    }

    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(value.r);
            writer.WritePropertyName("g");
            writer.WriteValue(value.g);
            writer.WritePropertyName("b");
            writer.WriteValue(value.b);
            writer.WritePropertyName("a");
            writer.WriteValue(value.a);
            writer.WriteEndObject();
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var r = jObject.GetValueOrDefault("r", 0f);
            var g = jObject.GetValueOrDefault("g", 0f);
            var b = jObject.GetValueOrDefault("b", 0f);
            var a = jObject.GetValueOrDefault("a", 0f);
            return new Color(r, g, b, a);
        }
    }

    public class Matrix4X4JsonConverter : JsonConverter<Matrix4x4>
    {
        public override void WriteJson(JsonWriter writer, Matrix4x4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("m00");
            writer.WriteValue(value.m00);
            writer.WritePropertyName("m01");
            writer.WriteValue(value.m01);
            writer.WritePropertyName("m02");
            writer.WriteValue(value.m02);
            writer.WritePropertyName("m03");
            writer.WriteValue(value.m03);
            writer.WritePropertyName("m10");
            writer.WriteValue(value.m10);
            writer.WritePropertyName("m11");
            writer.WriteValue(value.m11);
            writer.WritePropertyName("m12");
            writer.WriteValue(value.m12);
            writer.WritePropertyName("m13");
            writer.WriteValue(value.m13);
            writer.WritePropertyName("m20");
            writer.WriteValue(value.m20);
            writer.WritePropertyName("m21");
            writer.WriteValue(value.m21);
            writer.WritePropertyName("m22");
            writer.WriteValue(value.m22);
            writer.WritePropertyName("m23");
            writer.WriteValue(value.m23);
            writer.WritePropertyName("m30");
            writer.WriteValue(value.m30);
            writer.WritePropertyName("m31");
            writer.WriteValue(value.m31);
            writer.WritePropertyName("m32");
            writer.WriteValue(value.m32);
            writer.WritePropertyName("m33");
            writer.WriteValue(value.m33);
            writer.WriteEndObject();
        }

        public override Matrix4x4 ReadJson(JsonReader reader, Type objectType, Matrix4x4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            return new Matrix4x4
            {
                m00 = jObject.GetValueOrDefault("m00", 0f),
                m01 = jObject.GetValueOrDefault("m01", 0f),
                m02 = jObject.GetValueOrDefault("m02", 0f),
                m03 = jObject.GetValueOrDefault("m03", 0f),
                m10 = jObject.GetValueOrDefault("m10", 0f),
                m11 = jObject.GetValueOrDefault("m11", 0f),
                m12 = jObject.GetValueOrDefault("m12", 0f),
                m13 = jObject.GetValueOrDefault("m13", 0f),
                m20 = jObject.GetValueOrDefault("m20", 0f),
                m21 = jObject.GetValueOrDefault("m21", 0f),
                m22 = jObject.GetValueOrDefault("m22", 0f),
                m23 = jObject.GetValueOrDefault("m23", 0f),
                m30 = jObject.GetValueOrDefault("m30", 0f),
                m31 = jObject.GetValueOrDefault("m31", 0f),
                m32 = jObject.GetValueOrDefault("m32", 0f),
                m33 = jObject.GetValueOrDefault("m33", 0f)
            };
        }
    }
}