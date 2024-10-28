using UnityEngine;

namespace RoadSystem.Grid
{
    public class GridFactory
    {
        public Tile CreateTile(Tile prefab)
        {
            return GameObject.Instantiate(prefab);
        }
    }
}