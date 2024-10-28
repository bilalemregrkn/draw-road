using System.Collections.Generic;
using RoadSystem.Grid;
using UnityEngine;


namespace RoadSystem.Scripts.Runtime.Road
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Setting/RoadSetting", fileName = "RoadSetting", order = 0)]
    public class RoadSetting : ScriptableObject
    {
        [SerializeField] private List<RoadFiller> listRoadFiller;
        [SerializeField] private Tile tilePrefab;
        public Tile TilePrefab => tilePrefab;

        public RoadFiller GetPrefabRoadFiller(RoadType type)
        {
            return listRoadFiller.Find(x => x.type == type);
        }
    }
}