using System.Collections.Generic;
using System.Linq;
using RoadSystem.Extension;
using RoadSystem.Grid;
using UnityEngine;

namespace RoadSystem.Road
{
    public class RoadLayerResolver
    {
        private List<ShapeLayerData> _layer2;
        private List<ShapeLayerData> _layer3;

        private readonly GridController _gridService;
        private readonly RoadService _roadService;

        public bool IgnoreLayer2;
        public bool IgnoreLayer3;

        public RoadLayerResolver(RoadService service, GridController gridService)
        {
            _roadService = service;
            _gridService = gridService;
            _layer2 = new List<ShapeLayerData>();
            _layer3 = new List<ShapeLayerData>();
        }

        public void RefreshDisplay()
        {
            _roadService.ReleaseTile();
            UpdateLayers();
            UpdateDisplay();
        }

        private void UpdateLayers()
        {
            _layer2 = new List<ShapeLayerData>();
            _layer3 = new List<ShapeLayerData>();

            if (IgnoreLayer2)
                return;

            foreach (var pair in _gridService.Tiles)
            {
                var tile = pair.Value;
                if (IsBigCorner(tile))
                {
                    var center = tile;
                    var partOfShapes = tile.GetNeighbours();
                    var data = new ShapeLayerData(partOfShapes, center);
                    _layer2.Add(data);
                }
            }

            if (IgnoreLayer3)
                return;

            foreach (ShapeLayerData shape in _layer2)
            {
                var connectShape = GetConnectLayer2Shape(shape);
                if (connectShape != null)
                {
                    var layer3Data = CreateLayer3Shape(connectShape ?? new ShapeLayerData(), shape);
                    if (!_layer3.Contains(layer3Data))
                        _layer3.Add(layer3Data);
                }
            }
        }

        private void UpdateDisplay()
        {
            // Layer 1 — basic tiles
            foreach (var pair in _gridService.Tiles)
            {
                var tile = pair.Value;
                var shape = GetRoadShape(tile);
                tile.UpdateDisplayPiece(shape);
            }

            // Layer 3 — S/U shapes
            foreach (var shapeData in _layer3)
            {
                if (AnyTouchOtherLayer3(shapeData))
                    continue;

                var neighboursDirections = new List<Vector2>();
                foreach (Tile neighbour in shapeData.center.GetNeighbours())
                {
                    var list = neighbour.GetNeighbourDirection();
                    neighboursDirections.AddRange(list);
                }

                var hasAllDirection = HasAllDirections(neighboursDirections);
                var roadShape = hasAllDirection ? RoadShape.ShapeS : RoadShape.ShapeU;

                shapeData.center.ReleaseRoad();
                shapeData.center.UpdateDisplayPiece(roadShape);
                foreach (Tile tile in shapeData.partOfShapes)
                {
                    tile.ReleaseRoad();
                    tile.UpdateDisplayPiece(RoadShape.Invisible);
                }
            }

            // Layer 2 — big corners
            foreach (var shapeData in _layer2)
            {
                if (AnyTouchOtherLayer2(shapeData))
                    continue;

                if (AnyTouchOtherLayer3(shapeData))
                    continue;

                shapeData.center.ReleaseRoad();
                shapeData.center.UpdateDisplayPiece(RoadShape.ShapeBigCorner);
                foreach (Tile tile in shapeData.partOfShapes)
                {
                    tile.ReleaseRoad();
                    tile.UpdateDisplayPiece(RoadShape.Invisible);
                }
            }
        }

        private bool HasAllDirections(List<Vector2> directions)
        {
            bool hasUp = directions.Contains(Vector2.up);
            bool hasLeft = directions.Contains(Vector2.left);
            bool hasRight = directions.Contains(Vector2.right);
            bool hasDown = directions.Contains(Vector2.down);

            return hasUp && hasLeft && hasRight && hasDown;
        }

        private bool AnyTouchOtherLayer2(ShapeLayerData data)
        {
            foreach (ShapeLayerData shape in _layer2)
            {
                if (data.center == shape.center)
                    continue;

                foreach (Tile tile in data.partOfShapes)
                {
                    if (shape.partOfShapes.Contains(tile))
                        return true;
                }
            }

            return false;
        }

        private bool AnyTouchOtherLayer3(ShapeLayerData data)
        {
            foreach (ShapeLayerData shape in _layer3)
            {
                if (data.center == shape.center)
                    continue;

                foreach (Tile tile in data.partOfShapes)
                {
                    if (shape.partOfShapes.Contains(tile))
                        return true;
                }
            }

            return false;
        }

        public void OnDrawGizmos()
        {
            foreach (ShapeLayerData shape in _layer2)
                DrawShape(shape, Color.green, .33f);

            foreach (ShapeLayerData shape in _layer3)
                DrawShape(shape, Color.red, .44f);
        }

        private void DrawShape(ShapeLayerData data, Color color, float scale = 1)
        {
            Gizmos.color = color;

            foreach (Tile part in data.partOfShapes)
            {
                Gizmos.DrawLine(part.transform.position, data.center.transform.position);
                Gizmos.DrawCube(data.center.transform.position, Vector3.one * scale);
            }
        }

        private ShapeLayerData CreateLayer3Shape(ShapeLayerData data1, ShapeLayerData data2)
        {
            var center = GetCommonLayer2Tile(data1, data2);

            var allTile = new List<Tile>();
            foreach (var item in data1.partOfShapes)
            {
                if (item == center)
                    continue;
                allTile.Add(item);
            }

            allTile.Add(data1.center);

            foreach (var item in data2.partOfShapes)
            {
                if (item == center)
                    continue;
                allTile.Add(item);
            }

            allTile.Add(data2.center);

            return new ShapeLayerData(allTile, center);
        }

        private Tile GetCommonLayer2Tile(ShapeLayerData data1, ShapeLayerData data2)
        {
            var tempAllPartOfShapes = new List<Tile>(data1.partOfShapes);
            tempAllPartOfShapes.AddRange(data2.partOfShapes);

            return tempAllPartOfShapes
                .GroupBy(tile => tile)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .First();
        }

        private ShapeLayerData? GetConnectLayer2Shape(ShapeLayerData originData)
        {
            foreach (ShapeLayerData otherData in _layer2)
            {
                if (otherData.center == originData.center)
                    return null;

                foreach (var otherTile in otherData.partOfShapes)
                {
                    foreach (var originTile in originData.partOfShapes)
                    {
                        if (originTile == otherTile)
                            return otherData;
                    }
                }
            }

            return null;
        }

        private bool IsBigCorner(Tile tile)
        {
            var topology = tile.GetTileTopology();
            var directions = tile.GetNeighbourDirection();
            if (topology != TileTopology.TwoNeighbour90)
                return false;

            foreach (var direction in directions)
            {
                var nextTile = tile.GetNeighbour(direction);
                var nextNextTile = nextTile.GetNeighbour(direction);

                var isNullNextNext = nextNextTile == null;
                var isStraightNext = nextTile.GetTileTopology() == TileTopology.TwoNeighbour180;
                if (isNullNextNext || !isStraightNext)
                    return false;
            }

            return true;
        }

        public RoadShape GetRoadShape(Tile tile)
        {
            var topology = tile.GetTileTopology();
            return topology.ToRoadShape();
        }
    }
}
