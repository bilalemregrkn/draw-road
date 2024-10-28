using System.Collections.Generic;
using RoadSystem.Road;
using RoadSystem.Scripts.Runtime.Road;
using UnityEngine;

namespace RoadSystem.Grid
{
    public class Tile : MonoBehaviour
    {
        public Vector2 coordinate;
        public Tile left;
        public Tile up;
        public Tile down;
        public Tile right;

        RoadService _roadService;
        private RoadFiller _filler;
        public bool Deleted { get; set; }

        private bool _initialized;
        
        public void Initialize(Vector2 paramCoordinate,RoadService service)
        {
            if (_initialized)
                return;
            
            _roadService = service;

            _initialized = true;
            this.coordinate = paramCoordinate;
            var myTransform = transform;
            myTransform.position = new Vector3(coordinate.x, 0, coordinate.y);
            myTransform.name = $"Tile [{paramCoordinate.x},{paramCoordinate.y}]";
        }
        
        public List<Vector2> GetNeighbourDirection()
        {
            var result = new List<Vector2>();
            if (up != null && !up.Deleted)
                result.Add(Vector2.up);
            if (left != null && !left.Deleted)
                result.Add(Vector2.left);
            if (right != null && !right.Deleted)
                result.Add(Vector2.right);
            if (down != null && !down.Deleted)
                result.Add(Vector2.down);

            return result;
        }

        public Tile GetNeighbour(Vector2 direction)
        {
            if (direction == Vector2.up)
                return up;

            if (direction == Vector2.down)
                return down;

            if (direction == Vector2.right)
                return right;

            if (direction == Vector2.left)
                return left;

            return null;
        }

        public List<Tile> GetNeighbours()
        {
            var result = new List<Tile>();

            foreach (var direction in GetNeighbourDirection())
            {
                var tile = GetNeighbour(direction);
                result.Add(tile);
            }

            return result;
        }

        public TileType GetTileType()
        {
            return GetTileType(GetNeighbourDirection());
        }

        private TileType GetTileType(List<Vector2> directions)
        {
            var amount = directions.Count;
            switch (amount)
            {
                case 1:
                    return TileType.OneNeighbour;
                case 2:
                {
                    var horizontal = directions.Contains(Vector2.right) && directions.Contains(Vector2.left);
                    var vertical = directions.Contains(Vector2.up) && directions.Contains(Vector2.down);

                    if (horizontal || vertical)
                    {
                        return TileType.TwoNeighbour180;
                    }
                    else
                    {
                        return TileType.TwoNeighbour90;
                    }
                }
                case 3:
                    return TileType.ThreeNeighbour;
                case 4:
                    return TileType.FourNeighbour;
                default:
                    return TileType.OneNeighbour;
            }
        }

        public void UpdateDisplayPiece(RoadType type)
        {
            _filler = _roadService.InitializeRoadPiece(type, this);
            UpdateDisplayAlpha();
        }


        public void ReleaseRoad()
        {
            _filler?.Release();
        }

        private void UpdateDisplayAlpha()
        {
            _filler?.SetAlpha(1);
        }
    }
}