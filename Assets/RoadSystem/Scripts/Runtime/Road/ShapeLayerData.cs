using System;
using System.Collections.Generic;
using RoadSystem.Grid;

namespace RoadSystem.Road
{
    [Serializable]
    public struct ShapeLayerData
    {
        public List<Tile> partOfShapes;
        public Tile center;

        public ShapeLayerData(List<Tile> partOfShapes, Tile center)
        {
            this.partOfShapes = partOfShapes;
            this.center = center;
        }
    }
}
