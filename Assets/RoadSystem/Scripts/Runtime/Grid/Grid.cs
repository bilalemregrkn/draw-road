using System;
using System.Collections.Generic;
using RoadSystem.Draw;
using RoadSystem.Road;
using UnityEngine;

namespace RoadSystem.Grid
{
    public class Grid : MonoBehaviour
    {
        public readonly Dictionary<Vector2, Tile> Tiles = new Dictionary<Vector2, Tile>();
        public readonly List<Tile> ListTileDraft = new List<Tile>();
        public Tile LastTile { get; private set; }
        private GameObject _parent;
        private GridFactory _gridFactory;

        [SerializeField] private float gridScale = 2;
        [SerializeField] private RoadService roadService;

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            EventBus.EventBus.Instance.Subscribe<OnDragSignal>(OnDraw);
            EventBus.EventBus.Instance.Subscribe<OnFingerUpSignal>(OnFingerUp);
        }

        private void OnDisable()
        {
            EventBus.EventBus.Instance.Unsubscribe<OnDragSignal>(OnDraw);
            EventBus.EventBus.Instance.Unsubscribe<OnFingerUpSignal>(OnFingerUp);
        }

        private void OnFingerUp(OnFingerUpSignal signal)
        {
            LastTile = null;
        }

        private void OnDraw(OnDragSignal signal)
        {
            var position = signal.Position;
            var slotPosition = GridUtility.RoundToNearest(position, gridScale);
            var coordinate = new Vector2(slotPosition.x, slotPosition.z);
            CreateTileDraft(coordinate, true);
        }

        private void Initialize()
        {
            _gridFactory = new GridFactory();
        }


        public Tile GetTile(Vector2 coordinate)
        {
            var containsKey = Tiles.ContainsKey(coordinate);
            return containsKey ? Tiles[coordinate] : null;
        }

        public Tile GetOrCreateTile(Vector2 coordinate)
        {
            var containsKey = Tiles.ContainsKey(coordinate);
            if (containsKey)
                return Tiles[coordinate];

            var tilePrefab = roadService.Setting.TilePrefab;
            var tile = _gridFactory.CreateTile(tilePrefab);
            if (_parent == null)
                _parent = new GameObject("Grid");
            tile.transform.SetParent(_parent.transform);
            return tile;
        }

        private void AddTile(Tile tile, bool updateDisplay)
        {
            Tiles.TryAdd(tile.coordinate, tile);

            if (updateDisplay)
                roadService.ShapeManager.RefreshDisplay();
        }

        public void CreateTileDraft(Vector2 coordinate, bool updateDisplay)
        {
            var currentTile = GetOrCreateTile(coordinate);
            if (LastTile == currentTile)
                return;

            currentTile.Initialize(coordinate, roadService);
            ConnectThem(LastTile, currentTile);
            AddTile(currentTile, updateDisplay);

            LastTile = currentTile;

            if (!ListTileDraft.Contains(currentTile))
                ListTileDraft.Add(currentTile);

            roadService.ShapeManager.RefreshDisplay();
        }


        private void ConnectThem(Tile tileA, Tile tileB)
        {
            if (tileB == null || tileA == null)
                return;

            if (Math.Abs(tileA.coordinate.x - tileB.coordinate.x) < 0.1f)
            {
                if (tileA.coordinate.y > tileB.coordinate.y)
                {
                    tileA.down = tileB;
                    tileB.up = tileA;
                }
                else
                {
                    tileA.up = tileB;
                    tileB.down = tileA;
                }
            }
            else
            {
                if (tileA.coordinate.x > tileB.coordinate.x)
                {
                    tileA.left = tileB;
                    tileB.right = tileA;
                }
                else
                {
                    tileA.right = tileB;
                    tileB.left = tileA;
                }
            }
        }
    }
}

namespace RoadSystem.EventBus
{
    internal class Instance
    {
    }
}