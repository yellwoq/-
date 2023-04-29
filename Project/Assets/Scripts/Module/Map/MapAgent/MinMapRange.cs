using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{
    /// <summary>
    /// С��ͼ��Χ
    /// </summary>
    [Serializable, CreateAssetMenu(fileName = "mapArea", menuName = "RPG GAME/Map/new MapArea")]
    public class MinMapRange : ScriptableObject
    {
        [SerializeField, DisplayName("��ͼID")]
        private string mapID;

    }
}
