using System;
using RoadSystem.Grid;

namespace RoadSystem.Extension
{
    public static class Extension
    {
        public static TileType ToTileType(this RoadType origin)
        {
            return origin switch
            {
                RoadType.Straight => TileType.TwoNeighbour180,
                RoadType.DeadEnd => TileType.OneNeighbour,
                RoadType.Corner => TileType.TwoNeighbour90,
                RoadType.ThreeWay => TileType.ThreeNeighbour,
                RoadType.FourWay => TileType.FourNeighbour,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
        }

        public static RoadType ToRoadType(this TileType origin)
        {
            return origin switch
            {
                TileType.TwoNeighbour180 => RoadType.Straight,
                TileType.OneNeighbour => RoadType.DeadEnd,
                TileType.TwoNeighbour90 => RoadType.Corner,
                TileType.ThreeNeighbour => RoadType.ThreeWay,
                TileType.FourNeighbour => RoadType.FourWay,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
        }
    }
}