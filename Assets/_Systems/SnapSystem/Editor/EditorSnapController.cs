using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnapSystem
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [Flags]
    public enum Axis
    {
        None = 0,
        X = 1 << 0, // 1
        Y = 1 << 1, // 2
        Z = 1 << 2, // 4
        All = X | Y | Z // 1 | 2 | 4 = 7
    }

    public enum TransformType
    {
        Position,
        Rotation,
        Scale
    }


    [Serializable]
    public struct SnapDataContainer
    {
        [SerializeField] private List<SnapData> listSnaps;

        public SnapData GetData(Axis axis)
        {
            return listSnaps.Find(x => x.IsAxisActive(axis));
        }
    }

    [Serializable]
    public struct SnapData
    {
        public Axis axisFlags;
        public bool active;

        private bool Show => active && !freeze;
        public float snap;
        public float offset;

        public bool freeze;
        public float pivot;

        public bool IsAxisActive(Axis axis)
        {
            return (axisFlags & axis) == axis;
        }
    }


    [ExecuteInEditMode]
    public class EditorSnapController : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private SnapDataContainer position;
        [SerializeField] private SnapDataContainer rotation;
        [SerializeField] private SnapDataContainer scale;

        private SnapDataContainer GetData(TransformType type)
        {
            return type switch
            {
                TransformType.Position => position,
                TransformType.Rotation => rotation,
                TransformType.Scale => scale,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        // [OnInspectorInit]
        // private void OnInspectorInit()
        // {
        //     SceneView.duringSceneGui += OnSceneGUI;
        // }
        //
        // private void OnSceneGUI(SceneView obj)
        // {
        //     MyUpdate();
        // }
        // [OnInspectorDispose]
        // private void OnInspectorDispose()
        // {
        //     SceneView.duringSceneGui -= OnSceneGUI;
        // }

        private void Update()
        {
            MyUpdate();
        }


        private void Refresh()
        {
            MyUpdate();
        }


        private float GetSnappedValue(TransformType type, Axis axis, float value)
        {
            var snapData = GetData(type).GetData(axis);
            if (!snapData.active)
                return value;

            if (snapData.freeze)
                return snapData.pivot;

            return Utilities.RoundToNearest(value, snapData.snap, snapData.offset);
        }

        private Vector3 GetSnappedVector3(TransformType type, Vector3 value)
        {
            var x = GetSnappedValue(type, Axis.X, value.x);
            var y = GetSnappedValue(type, Axis.Y, value.y);
            var z = GetSnappedValue(type, Axis.Z, value.z);
            return new Vector3(x, y, z);
        }

        private void MyUpdate()
        {
            if (Application.isPlaying)
                return;

            transform.localPosition = GetSnappedVector3(TransformType.Position, transform.localPosition);
            transform.localEulerAngles = GetSnappedVector3(TransformType.Rotation, transform.localEulerAngles);
            transform.localScale = GetSnappedVector3(TransformType.Scale, transform.localScale);
        }
#endif
    }

    public static class Utilities
    {
        public static float InQuad(float t) => t * t;

        public static bool HasMyChild(this Transform parent, Transform suspectObject)
        {
            // If suspectObject is null, return false
            if (suspectObject == null)
                return false;

            // Check if suspectObject is a direct child of parent
            if (suspectObject.parent == parent)
                return true;

            // Check if suspectObject is a descendant of parent using recursion
            while (suspectObject.parent != null)
            {
                suspectObject = suspectObject.parent;
                if (suspectObject == parent)
                    return true;
            }

            // If suspectObject is not a child or descendant of parent, return false
            return false;
        }

        public static Direction[] AllDirection =>
            new Direction[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left };

        public static Vector3 RoundToNearestXZ(Vector3 value, float scale, float offset = 0)
        {
            var x = RoundToNearest(value.x, scale, offset);
            var z = RoundToNearest(value.z, scale, offset);
            return new Vector3(x, value.y, z);
        }

        public static Vector3 RoundToNearest(Vector3 value, float scale, float offset = 0)
        {
            var x = RoundToNearest(value.x, scale, offset);
            var y = RoundToNearest(value.y, scale, offset);
            var z = RoundToNearest(value.z, scale, offset);
            return new Vector3(x, y, z);
        }

        public static Vector3 RoundToNearestY(Vector3 value, float scale, float offset = 0)
        {
            var y = RoundToNearest(value.y, scale, offset);
            return new Vector3(value.x, y, value.z);
        }

        public static float RoundToNearest(float value, float scale, float offset = 0)
        {
            var offsetValue = value - offset;
            float roundedValue = (float)(Math.Round(offsetValue / scale) * scale);
            return roundedValue + offset;
        }

        public static Direction ToOpposite(this Direction origin)
        {
            return origin switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
        }

        public static Vector3 ToVector3(this Direction origin)
        {
            return origin switch
            {
                Direction.Up => Vector3.forward,
                Direction.Down => Vector3.back,
                Direction.Left => Vector3.left,
                Direction.Right => Vector3.right,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
        }

        public static Direction ToDirection(this Vector3 origin)
        {
            var normalize = origin.normalized;
            if (Mathf.Abs(normalize.x) > Mathf.Abs(normalize.z))
            {
                return normalize.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return normalize.z > 0 ? Direction.Up : Direction.Down;
            }
        }

        public static Direction ToDirection(this Transform main)
        {
            var TOLERANCE = 5;
            var eulerAnglesY = main.eulerAngles.y;

            while (eulerAnglesY < 0)
                eulerAnglesY += 360;

            if (Math.Abs(eulerAnglesY) < TOLERANCE)
                return Direction.Up;

            if (Math.Abs(eulerAnglesY - 90) < TOLERANCE)
                return Direction.Right;

            if (Math.Abs(eulerAnglesY - 180) < TOLERANCE)
                return Direction.Down;

            if (Math.Abs(eulerAnglesY - 270) < TOLERANCE)
                return Direction.Left;

            return Direction.Up;
        }
    }
}