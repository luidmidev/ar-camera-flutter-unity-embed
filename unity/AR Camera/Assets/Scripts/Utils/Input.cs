using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEngine.RuntimePlatform;

namespace Utils
{
    public static class Input
    {
        private static bool TouchSupported => Application.platform is Android or IPhonePlayer || UnityEngine.Input.touchSupported;

        private static Touch? FakeTouch => SimulateTouchWithMouse.Instance.FakeTouch;

        public static bool GetButton(string buttonName)
        {
            return UnityEngine.Input.GetButton(buttonName);
        }

        public static bool GetButtonDown(string buttonName)
        {
            return UnityEngine.Input.GetButtonDown(buttonName);
        }

        public static bool GetButtonUp(string buttonName)
        {
            return UnityEngine.Input.GetButtonUp(buttonName);
        }

        public static bool GetMouseButton(int button)
        {
            return UnityEngine.Input.GetMouseButton(button);
        }

        public static bool GetMouseButtonDown(int button)
        {
            return UnityEngine.Input.GetMouseButtonDown(button);
        }

        public static bool GetMouseButtonUp(int button)
        {
            return UnityEngine.Input.GetMouseButtonUp(button);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static int touchCount
        {
            get
            {
                if (TouchSupported) return UnityEngine.Input.touchCount;
                return FakeTouch.HasValue ? 1 : 0;
            }
        }

        public static Touch GetTouch(int index)
        {
            if (TouchSupported) return UnityEngine.Input.GetTouch(index);
            Assert.IsTrue(FakeTouch.HasValue && index == 0);
            return FakeTouch.Value;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static Touch[] touches
        {
            get
            {
                if (TouchSupported) return UnityEngine.Input.touches;
                return FakeTouch.HasValue ? new[] { FakeTouch.Value } : Array.Empty<Touch>();
            }
        }
    }

    internal class SimulateTouchWithMouse
    {
        private static SimulateTouchWithMouse _instance;
        private float lastUpdateTime;
        private Vector3 prevMousePos;
        private Touch? fakeTouch;


        public static SimulateTouchWithMouse Instance => _instance ??= new SimulateTouchWithMouse();

        public Touch? FakeTouch
        {
            get
            {
                Update();
                return fakeTouch;
            }
        }

        private void Update()
        {
            if (Math.Abs(Time.time - lastUpdateTime) < 0.00001) return;

            lastUpdateTime = Time.time;

            var curMousePos = UnityEngine.Input.mousePosition;
            var delta = curMousePos - prevMousePos;
            prevMousePos = curMousePos;

            fakeTouch = CreateTouch(GetPhase(delta), delta);
        }

        private static TouchPhase? GetPhase(Vector3 delta)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                return TouchPhase.Began;
            }

            if (UnityEngine.Input.GetMouseButton(0))
            {
                return delta.sqrMagnitude < 0.01f ? TouchPhase.Stationary : TouchPhase.Moved;
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                return TouchPhase.Ended;
            }

            return null;
        }

        private static Touch? CreateTouch(TouchPhase? phase, Vector3 delta)
        {
            if (!phase.HasValue)
            {
                return null;
            }

            var curMousePos = UnityEngine.Input.mousePosition;
            return new Touch
            {
                phase = phase.Value,
                type = TouchType.Indirect,
                position = curMousePos,
                rawPosition = curMousePos,
                fingerId = 0,
                tapCount = 1,
                deltaTime = Time.deltaTime,
                deltaPosition = delta
            };
        }
    }
}