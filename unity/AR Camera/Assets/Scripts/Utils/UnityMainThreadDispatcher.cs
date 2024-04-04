using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher _instance;
        private static bool _applicationIsQuitting;

        private static readonly Queue<Action> ActionQueue = new();

        public static void Initialize()
        {
            if (_instance == null) _instance = new GameObject("UnityMainThreadDispatcher").AddComponent<UnityMainThreadDispatcher>();
        }

        public static void Enqueue(Action action)
        {
            if (_applicationIsQuitting) return;
            lock (ActionQueue) ActionQueue.Enqueue(action);
        }

        private void Update()
        {
            lock (ActionQueue)
            {
                while (ActionQueue.Count > 0) ActionQueue.Dequeue().Invoke();
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}