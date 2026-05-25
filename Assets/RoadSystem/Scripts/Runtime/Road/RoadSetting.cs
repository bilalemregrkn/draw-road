using System.Collections.Generic;
using RoadSystem.Grid;
using UnityEngine;

namespace RoadSystem.Road
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Setting/RoadSetting", fileName = "RoadSetting", order = 0)]
    public class RoadSetting : ScriptableObject
    {
        [SerializeField] private List<RoadPiece> listRoadPiece;
        [SerializeField] private Tile tilePrefab;
        public Tile TilePrefab => tilePrefab;

        public RoadPiece GetPrefabRoadPiece(RoadShape shape)
        {
            return listRoadPiece.Find(x => x.type == shape);
        }
    }
}
