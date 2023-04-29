using System;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// ��ͼ��Χ
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "mapArea", menuName = "RPG GAME/Map/new MapArea")]
    public class MapRange : ScriptableObject
    {
        [SerializeField,DisplayName("��ͼID")]
        private string mapID;
        public string MapID=>mapID;
        [SerializeField,DisplayName("�������С��Χ")]
        private Vector2 rangeMin;
        public Vector2 RangeMin=>rangeMin;
        [SerializeField, DisplayName("��������Χ")]
        private Vector2 rangeMax;
        public Vector2 RangeMax=>rangeMax;
    }
}
