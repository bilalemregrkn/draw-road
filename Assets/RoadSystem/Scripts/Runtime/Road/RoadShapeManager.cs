using System;
using System.Collections.Generic;
using System.Linq;
using RoadSystem.Extension;
using RoadSystem.Grid;
using UnityEngine;

namespace RoadSystem.Road
{
    public class RoadShapeManager
    {
        private List<RoadShapeData> _layer2;
        private List<RoadShapeData> _layer3;

        private readonly Grid.Grid _gridService;
        private readonly RoadService _roadService;

        public bool IgnoreLayer2; //For Test
        public bool IgnoreLayer3; //For Test

        public RoadShapeManager(RoadService service, Grid.Grid gridService)
        {
            _roadService = service;
            _gridService = gridService;
            _layer2 = new List<RoadShapeData>();
            _layer3 = new List<RoadShapeData>();
        }

        public void RefreshDisplay()
        {
            _roadService.ReleaseTile();
            UpdateLayers();
            UpdateDisplay();
        }


        private void UpdateLayers()
        {
            _layer2 = new List<RoadShapeData>();
            _layer3 = new List<RoadShapeData>();


            if (IgnoreLayer2)
                return;


            foreach (var pair in _gridService.Tiles)
            {
                var tile = pair.Value;
                if (IsBigCorner(tile))
                {
                    var center = tile;
                    var partOfShapes = tile.GetNeighbours();
                    var data = new RoadShapeData(partOfShapes, center);
                    _layer2.Add(data);
                }
            }


            if (IgnoreLayer3)
                return;

            foreach (RoadShapeData shape in _layer2)
            {
                var connectShape = GetConnectLayer2Shape(shape);
                if (connectShape != null)
                {
                    var layer3Data = CreateLayer3Shape(connectShape ?? new RoadShapeData(), shape);
                    if (!_layer3.Contains(layer3Data))
                        _layer3.Add(layer3Data);
                }
            }
        }


        private void UpdateDisplay()
        {
            //Show layer 1
            foreach (var pair in _gridService.Tiles)
            {
                var tile = pair.Value;
                var type = GetRoadType(tile);
                tile.UpdateDisplayPiece(type);
            }

            //Show layer 3
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
                var roadType = hasAllDirection ? RoadType.ShapeS : RoadType.ShapeU;

                shapeData.center.ReleaseRoad();
                shapeData.center.UpdateDisplayPiece(roadType);
                foreach (Tile tile in shapeData.partOfShapes)
                {
                    tile.ReleaseRoad();
                    tile.UpdateDisplayPiece(RoadType.Invisible);
                }
            }

            //Show layer 2
            foreach (var shapeData in _layer2)
            {
                if (AnyTouchOtherLayer2(shapeData))
                    continue;

                if (AnyTouchOtherLayer3(shapeData))
                    continue;

                shapeData.center.ReleaseRoad();
                shapeData.center.UpdateDisplayPiece(RoadType.ShapeBigCorner);
                foreach (Tile tile in shapeData.partOfShapes)
                {
                    tile.ReleaseRoad();
                    tile.UpdateDisplayPiece(RoadType.Invisible);
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

        private bool AnyTouchOtherLayer2(RoadShapeData data)
        {
            foreach (RoadShapeData shape in _layer2)
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

        private bool AnyTouchOtherLayer3(RoadShapeData data)
        {
            foreach (RoadShapeData shape in _layer3)
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
            foreach (RoadShapeData shape in _layer2)
            {
                DrawShape(shape, Color.green, .33f);
            }

            foreach (RoadShapeData shape in _layer3)
            {
                DrawShape(shape, Color.red, .44f);
            }
        }

        private void DrawShape(RoadShapeData data, Color color, float scale = 1)
        {
            Gizmos.color = color;

            foreach (Tile part in data.partOfShapes)
            {
                Gizmos.DrawLine(part.transform.position, data.center.transform.position);
                Gizmos.DrawCube(data.center.transform.position, Vector3.one * scale);
            }
        }

        private RoadShapeData CreateLayer3Shape(RoadShapeData data1, RoadShapeData data2)
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

            return new RoadShapeData(allTile, center);
        }

        private Tile GetCommonLayer2Tile(RoadShapeData data1, RoadShapeData data2)
        {
            var tempAllPartOfShapes = new List<Tile>(data1.partOfShapes);
            tempAllPartOfShapes.AddRange(data2.partOfShapes);

            return tempAllPartOfShapes
                .GroupBy(tile => tile)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .First();
        }

        private RoadShapeData? GetConnectLayer2Shape(RoadShapeData originData)
        {
            foreach (RoadShapeData otherData in _layer2)
            {
                if (otherData.center == originData.center)
                {
                    return null;
                }

                foreach (var otherTile in otherData.partOfShapes)
                {
                    foreach (var originTile in originData.partOfShapes)
                    {
                        if (originTile == otherTile)
                        {
                            return otherData;
                        }
                    }
                }
            }

            return null;
        }

        private bool IsBigCorner(Tile tile)
        {
            var tileType = tile.GetTileType();
            var directions = tile.GetNeighbourDirection();
            if (tileType != TileType.TwoNeighbour90)
            {
                return false;
            }

            foreach (var direction in directions)
            {
                var nextTile = tile.GetNeighbour(direction);
                var nextNextTile = nextTile.GetNeighbour(direction);

                var isNullNextNext = nextNextTile == null;
                var isStraightNext = nextTile.GetTileType() == TileType.TwoNeighbour180;
                if (isNullNextNext || !isStraightNext)
                    return false;
            }

            return true;
        }

        public RoadType GetRoadType(Tile tile)
        {
            var tileType = tile.GetTileType();
            var roadType = tileType.ToRoadType();
            return roadType;
        }
    }

    [Serializable]
    public struct RoadShapeData
    {
        public List<Tile> partOfShapes;
        public Tile center;

        public RoadShapeData(List<Tile> partOfShapes, Tile center)
        {
            this.partOfShapes = partOfShapes;
            this.center = center;
        }
    }
}