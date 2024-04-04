using System.Globalization;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Convert;
using static System.Text.Encoding;


namespace Extensions
{
    public static class HttpListenerExtension
    {
        private static void Send(this HttpListenerResponse response, byte[] buffer, int statusCode = 200)
        {
            response.StatusCode = statusCode;
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }

        public static void Send(this HttpListenerResponse response, object body, int statusCode = 200)
        {
            switch (body)
            {
                case float or int or string or bool:
                    response.Send(UTF8.GetBytes(body.ToString()), statusCode);
                    return;

                case byte[] bytes:
                    response.Send(bytes, statusCode);
                    return;

                default:
                {
                    var json = JsonConvert.SerializeObject(body);
                    response.Send(UTF8.GetBytes(json), statusCode);
                    break;
                }
            }
        }

        public static T GetBody<T>(this HttpListenerRequest request)
        {
            var body = new StreamReader(request.InputStream).ReadToEnd();
            var type = typeof(T);

            if (type == typeof(string))
            {
                return (T)ChangeType(body, typeof(T));
            }

            if (type == typeof(byte[]))
            {
                return (T)ChangeType(UTF8.GetBytes(body), typeof(T));
            }

            if (type == typeof(JObject))
            {
                return (T)ChangeType(JObject.Parse(body), typeof(T));
            }

            if (type == typeof(JArray))
            {
                return (T)ChangeType(JArray.Parse(body), typeof(T));
            }

            if (type == typeof(float))
            {
                return (T)ChangeType(float.Parse(body, CultureInfo.InvariantCulture.NumberFormat), typeof(T));
            }

            if (type == typeof(int))
            {
                return (T)ChangeType(int.Parse(body, CultureInfo.InvariantCulture.NumberFormat), typeof(T));
            }

            if (type == typeof(bool))
            {
                return (T)ChangeType(bool.Parse(body), typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(body);
        }
    }
}