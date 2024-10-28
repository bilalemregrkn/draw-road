using System;
using UnityEngine;

namespace RoadSystem.Grid
{
    public static class GridUtility
    {
        public static float RoundToNearest(float value, float scale, float offset = 0)
        {
            var offsetValue = value - offset;
            float roundedValue = (float)(Math.Round(offsetValue / scale) * scale);
            return roundedValue + offset;
        }

        public static Vector3 RoundToNearest(Vector3 value, float scale, float offset = 0)
        {
            var x = RoundToNearest(value.x, scale, offset);
            var y = RoundToNearest(value.y, scale, offset);
            var z = RoundToNearest(value.z, scale, offset);
            return new Vector3(x, y, z);
        }
    }
}