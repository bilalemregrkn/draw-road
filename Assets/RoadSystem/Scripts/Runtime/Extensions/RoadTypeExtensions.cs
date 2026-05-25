using System;
using RoadSystem.Grid;

namespace RoadSystem.Extension
{
    public static class RoadTypeExtensions
    {
        public static TileTopology ToTileTopology(this RoadShape origin)
        {
            return origin switch
            {
                RoadShape.Straight => TileTopology.TwoNeighbour180,
                RoadShape.DeadEnd => TileTopology.OneNeighbour,
                RoadShape.Corner => TileTopology.TwoNeighbour90,
                RoadShape.ThreeWay => TileTopology.ThreeNeighbour,
                RoadShape.FourWay => TileTopology.FourNeighbour,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
        }

        public static RoadShape ToRoadShape(this TileTopology origin)
        {
            return origin switch
            {
                TileTopology.TwoNeighbour180 => RoadShape.Straight,
                TileTopology.OneNeighbour => RoadShape.DeadEnd,
                TileTopology.TwoNeighbour90 => RoadShape.Corner,
                TileTopology.ThreeNeighbour => RoadShape.ThreeWay,
                TileTopology.FourNeighbour => RoadShape.FourWay,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };
        }
    }
}
