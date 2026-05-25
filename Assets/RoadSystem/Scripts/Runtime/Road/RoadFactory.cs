using UnityEngine;

namespace RoadSystem.Road
{
    public class RoadFactory
    {
        public RoadFiller Create(RoadFiller prefab)
        {
            return GameObject.Instantiate(prefab);
        }
    }
}