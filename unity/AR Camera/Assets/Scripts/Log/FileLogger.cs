using System;
using System.IO;
using UnityEngine;

namespace Log
{
    public static class FileLogger
    {
        private static string PathLog => Application.platform == RuntimePlatform.Android
            ? Application.persistentDataPath + "/log.txt"
            : Application.dataPath + "/log.txt";

        public static void Log(string message)
        {
            var path = PathLog;
            if (!File.Exists(path)) File.Create(path).Dispose();
            File.AppendAllText(PathLog, "[" + DateTime.Now + "] " + message + "\n");
        }

        public static void Clear()
        {
            Debug.Log("Clearing log file: " + PathLog);
            if (File.Exists(PathLog)) File.Delete(PathLog);
        }
    }
}