using System;
using System.Linq;
using UnityEngine;

namespace MokomoGamesLib.Runtime.Touch
{
    public class TouchInputProvider
    {
        public enum TouchType
        {
            None = 99,
            Began = 0,
            Moved = 1,
            Stationaruy = 2,
            Ended = 3,
            Canceld = 4
        }

        public TouchInputProvider()
        {
            OnTouched += type =>
            {
                if (type == TouchType.Began) OnBeginTouched?.Invoke();
            };
        }

        public bool IsTouch { get; private set; }
        public event Action<TouchType> OnTouched;
        public event Action OnBeginTouched;

        public void Tick()
        {
            var touch = GetTouch();
            IsTouch = touch == TouchType.Began;
            if (touch != TouchType.None) OnTouched?.Invoke(touch);
        }

        private static TouchType GetTouch()
        {
            if (Application.isEditor)
            {
                if (Input.GetMouseButtonDown(0))
                    return TouchType.Began;
                if (Input.GetMouseButton(0))
                    return TouchType.Moved;
                if (Input.GetMouseButtonUp(0))
                    return TouchType.Ended;
                return TouchType.None;
            }

            if (Input.touchCount > 0) return (TouchType)(int)Input.GetTouch(0).phase;

            return TouchType.None;
        }

        private static Vector3 GetTouchPosition()
        {
            if (Application.isEditor) return GetTouch() != TouchType.None ? Input.mousePosition : Vector3.zero;

            if (Input.touchCount <= 0) return Vector3.zero;
            var touch = Input.GetTouch(0);
            return new Vector3(touch.position.x, touch.position.y, 0f);
        }

        public Vector3 GetTouchWorldPosition(Camera cam)
        {
            return cam.ScreenToWorldPoint(GetTouchPosition());
        }

        public T GetObjByTap<T>() where T : MonoBehaviour
        {
            var mainCam = Camera.main;
            if (mainCam == null) return null;

            var ray = mainCam.ScreenPointToRay(GetTouchPosition());

            var panels =
                Physics
                    .RaycastAll(ray.origin, ray.direction)
                    .Where(x => x.collider != null)
                    .Select(x => x.collider.GetComponent<T>())
                    .Where(x => x != null)
                    .ToList();

            return panels.Any() ? panels.FirstOrDefault() : null;
        }
    }
}