using System.Collections.Generic;
using System.Linq;
using RoadSystem.Grid;
using RoadSystem.Scripts.Runtime.Road;
using UnityEngine;

namespace RoadSystem.Road
{
    public class RoadService : MonoBehaviour
    {
        public RoadShapeManager ShapeManager => _roadShapeManager;
        public RoadSetting Setting => setting;

        private RoadFactory _roadFactory;
        private readonly List<RoadFiller> _listFiller = new List<RoadFiller>();
        private GameObject _fillers;
        private RoadShapeManager _roadShapeManager;

        [SerializeField] private RoadSetting setting;
        [SerializeField] private Grid.Grid grid;


        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _roadFactory = new RoadFactory();
            _roadShapeManager = new RoadShapeManager(this, grid);
        }

        public void ReleaseTile()
        {
            foreach (RoadFiller item in _listFiller)
                item.Release();
        }

        public RoadFiller InitializeRoadPiece(RoadType roadType, Tile tile)
        {
            List<Vector2> directions = tile.GetNeighbourDirection();

            if (directions.Count == 0)
                return null;
            if (roadType == RoadType.Invisible)
                return null;

            var coordinate = tile.coordinate;
            if (roadType == RoadType.ShapeBigCorner)
            {
                var total =
                    tile.GetNeighbour(directions[0]).coordinate +
                    tile.GetNeighbour(directions[1]).coordinate;

                coordinate = total * .5f;
            }

            var filler = GetOrCreateFiller(roadType);
            var angle = GetAngle(tile, roadType);

            var flip = false;
            if (roadType == RoadType.ShapeS)
                flip = GetFlipForS(tile);
            filler.Initialize(coordinate, angle, flip);
            if (_fillers == null)
                _fillers = new GameObject("RoadFillers");
            filler.transform.SetParent(_fillers.transform);
            return filler;
        }

        private RoadFiller GetOrCreateFiller(RoadType type)
        {
            var list = _listFiller.Where(x => x.type == type).ToList();
            list = list.Where(x => !x.Initialized).ToList();
            if (list.Count != 0)
            {
                var piece = list[0];
                return piece;
            }

            var prefab = setting.GetPrefabRoadFiller(type);
            var filler = _roadFactory.Create(prefab);
            _listFiller.Add(filler);
            return filler;
        }

        private int GetAngle(Tile tile, RoadType type)
        {
            List<Vector2> directions = tile.GetNeighbourDirection();
            return type switch
            {
                RoadType.DeadEnd => GetAngleYForDeadEnd(directions[0]),
                RoadType.Straight => GetAngleYForStraight(directions[0], directions[1]),
                RoadType.ShapeBigCorner => GetAngleYForCorner(directions[0], directions[1]),
                RoadType.Corner => GetAngleYForCorner(directions[0], directions[1]),
                RoadType.ThreeWay => GetAngleYForT(directions[0], directions[1], directions[2]),
                RoadType.FourWay => 0,
                RoadType.ShapeU => GetAngleYForU(tile),
                RoadType.ShapeS => GetAngleYForS(tile),
                _ => 0
            };
        }

        private int GetAngleYForDeadEnd(Vector2 direction)
        {
            var result = 0;
            if (direction == Vector2.up) result = 0;
            if (direction == Vector2.down) result = 180;
            if (direction == Vector2.right) result = 90;
            if (direction == Vector2.left) result = 270;

            return result;
        }

        private int GetAngleYForCorner(Vector2 directionA, Vector2 directionB)
        {
            bool IsDirection(Vector2 direction) => direction == directionA || direction == directionB;

            bool isLeft = IsDirection(Vector2.left);
            bool isUp = IsDirection(Vector2.up);
            bool isRight = IsDirection(Vector2.right);
            bool isDown = IsDirection(Vector2.down);

            var result = 0;
            if (isLeft && isDown) result = 0;
            if (isLeft && isUp) result = 90;
            if (isRight && isUp) result = 180;
            if (isRight && isDown) result = 270;

            return result;
        }

        private int GetAngleYForStraight(Vector2 directionA, Vector2 directionB)
        {
            bool IsDirection(Vector2 direction) => direction == directionA || direction == directionB;

            bool isHorizontal = IsDirection(Vector2.left) && IsDirection(Vector2.right);
            bool isVertical = IsDirection(Vector2.up) && IsDirection(Vector2.down);

            return isHorizontal ? 0 : 90;
        }

        private int GetAngleYForT(Vector2 directionA, Vector2 directionB, Vector2 directionC)
        {
            bool IsDirection(Vector2 direction) =>
                direction == directionA || direction == directionB || direction == directionC;

            bool isRight = IsDirection(Vector2.right);
            bool isLeft = IsDirection(Vector2.left);
            bool isUp = IsDirection(Vector2.up);
            bool isDown = IsDirection(Vector2.down);

            var result = 0;

            if (isLeft && isUp && isRight) result = 0;
            if (isUp && isRight && isDown) result = 90;
            if (isRight && isDown && isLeft) result = 180;
            if (isLeft && isDown && isUp) result = 270;

            return result;
        }

        private int GetAngleYForU(Tile tile)
        {
            var neighbours = tile.GetNeighbours();
            var directions = new List<Vector2>();
            foreach (Tile item in neighbours)
            {
                var list = item.GetNeighbourDirection();
                directions.AddRange(list);
            }

            Vector2 mostFrequent = directions
                .GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .First();

            bool IsDirection(Vector2 direction) => direction == mostFrequent;

            bool isRight = IsDirection(Vector2.right);
            bool isLeft = IsDirection(Vector2.left);
            bool isUp = IsDirection(Vector2.up);
            bool isDown = IsDirection(Vector2.down);

            var result = 0;

            if (isLeft) result = 0;
            if (isUp) result = 90;
            if (isRight) result = 180;
            if (isDown) result = 270;

            return result - 270;
        }

        private int GetAngleYForS(Tile tile)
        {
            var firstDirection = Vector2.zero;
            var secondDirection = Vector2.zero;

            firstDirection = tile.GetNeighbourDirection().First();
            var neighbour = tile.GetNeighbour(firstDirection);
            var neighbourDirection = neighbour.GetNeighbourDirection();

            foreach (var direction in neighbourDirection)
            {
                if (neighbour.GetNeighbour(direction) == tile)
                    continue;

                secondDirection = direction;
            }

            bool IsFirstDirection(Vector2 direction) => direction == firstDirection;
            bool IsSecondDirection(Vector2 direction) => direction == secondDirection;

            bool fRight = IsFirstDirection(Vector2.right);
            bool fLeft = IsFirstDirection(Vector2.left);
            bool fUp = IsFirstDirection(Vector2.up);
            bool fDown = IsFirstDirection(Vector2.down);

            bool sRight = IsSecondDirection(Vector2.right);
            bool sLeft = IsSecondDirection(Vector2.left);
            bool sUp = IsSecondDirection(Vector2.up);
            bool sDown = IsSecondDirection(Vector2.down);

            var result = 0;

            if ((fLeft && sDown) || (fRight && sUp)) result = 0;
            if ((fUp && sLeft) || (fDown && sRight)) result = 90;

            if ((fUp && sRight) || (fDown && sLeft)) result = 90; //-1
            if ((fLeft && sUp) || (fRight && sDown)) result = 0; //-1


            return result;
        }

        private bool GetFlipForS(Tile tile)
        {
            var firstDirection = Vector2.zero;
            var secondDirection = Vector2.zero;

            firstDirection = tile.GetNeighbourDirection().First();
            var neighbour = tile.GetNeighbour(firstDirection);
            var neighbourDirection = neighbour.GetNeighbourDirection();

            foreach (var direction in neighbourDirection)
            {
                if (neighbour.GetNeighbour(direction) == tile)
                    continue;

                secondDirection = direction;
            }

            bool IsFirstDirection(Vector2 direction) => direction == firstDirection;
            bool IsSecondDirection(Vector2 direction) => direction == secondDirection;

            bool fRight = IsFirstDirection(Vector2.right);
            bool fLeft = IsFirstDirection(Vector2.left);
            bool fUp = IsFirstDirection(Vector2.up);
            bool fDown = IsFirstDirection(Vector2.down);

            bool sRight = IsSecondDirection(Vector2.right);
            bool sLeft = IsSecondDirection(Vector2.left);
            bool sUp = IsSecondDirection(Vector2.up);
            bool sDown = IsSecondDirection(Vector2.down);

            if ((fLeft && sDown) || (fRight && sUp)) return false;
            if ((fUp && sLeft) || (fDown && sRight)) return false;

            if ((fUp && sRight) || (fDown && sLeft)) return true;
            if ((fLeft && sUp) || (fRight && sDown)) return true;

            return false;
        }
    }
}