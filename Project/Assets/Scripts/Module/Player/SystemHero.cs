using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// ϵͳӢ����Ϣ
    /// </summary>
    [System.Serializable,CreateAssetMenu(fileName ="SystemHero",menuName = "RPG GAME/Hero/new SystemHero")]
    public class SystemHero : ScriptableObject
    {
        [SerializeField,DisplayName("Ӣ��ID")]
        private int heroID;
        public int HeroID => heroID;
        [SerializeField,DisplayName("Ӣ������")]
        private string heroType;
        public string HeroType => heroType;
        [SerializeField,DisplayName("��ɫ��")]
        private string roleName;
        public string RoleName => roleName;
        [SerializeField,TextArea,Header("������Ϣ")]
        private string decription;
        public string Decription => decription;
    }
}
