using UnityEngine;

namespace RoadSystem.Road
{
    public class RoadFactory
    {
        public RoadPiece Create(RoadPiece prefab)
        {
            return GameObject.Instantiate(prefab);
        }
    }
}
