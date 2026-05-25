using RoadSystem.Road;
using UnityEngine;

namespace RoadSystem.Grid
{
    public class TileFactory
    {
        public Tile CreateTile(Tile prefab)
        {
            return GameObject.Instantiate(prefab);
        }

        public RoadPiece Create(RoadPiece prefab)
        {
            return GameObject.Instantiate(prefab);
        }
    }
}
