using System;
using System.Runtime.InteropServices;
using Log;
using UnityEngine;

namespace FlutterEmbed.SendToFlutter
{
    public static class SendToFlutter
    {
        public static void Send(string data)
        {
#if UNITY_EDITOR
            Debug.Log("SendToFlutter.Send: " + data);
#endif
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    // Use reflection to call the relevant static Kotlin method in the Android plugin
                    using var sendToFlutterClass = new AndroidJavaClass("com.learntoflutter.flutter_embed_unity_android.messaging.SendToFlutter");
                    sendToFlutterClass.CallStatic("sendToFlutter", data);
                }
                else
                {
/*
#if UNITY_IOS
                // Call an obj-C function name
                FlutterEmbedUnityIos_sendToFlutter(data);
#endif
*/
                }
            }
            catch (Exception e)
            {
                FileLogger.Log(e.Message);
                Debug.LogException(e);
            }
        }
/*
#if UNITY_IOS
    // On iOS plugins are statically linked into the executable,
    // so we have to use __Internal as the library name.
    [DllImport("__Internal")]
    // This function is defined in flutter_embed_unity_2022_3_ios/ios/Classes/SendToFlutter.swift
    private static extern void FlutterEmbedUnityIos_sendToFlutter(string data);
#endif
*/
    }
}