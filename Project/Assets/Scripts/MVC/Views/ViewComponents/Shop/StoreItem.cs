using Bag;
using Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MVC
{
    /// <summary>
    /// �̵���ʾ��Ʒ
    /// </summary>
    public class StoreItem : MonoBehaviour, IPointerClickHandler
    {
        [DisplayName("��Ʒ���", true)]
        public int id;
        [DisplayName("��Ʒ����", true)]
        public int num;
        [DisplayName("���������Ʒ����", true, true, "��Ʒ", "������Ʒ")]
        public ItemInPanelType myItemPanelType;
        [SerializeField, DisplayName("��Ʒͼ����ʾ")]
        private Image icon;
        public Text numText;
        public void SetUI()
        {
            Texture2D iconTexture = ResourceManager.Load<Texture2D>(ItemInfoManager.I.GetObjectInfoById(id).icon_name);
            icon.sprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            numText.text = num.ToString();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            PlayerPrefs.SetInt(KeyList.CURRENT_STOREITEM_ID, id);
            if (myItemPanelType == ItemInPanelType.good)
                ShopItemPanel.I.currentHandleType = ShopHandleType.Buy;
            else
                ShopItemPanel.I.currentHandleType = ShopHandleType.Sell;
            ShopItemPanel.I.currentItem = this;
            ShopItemPanel.I.Show();
        }
    }
    public enum ItemInPanelType
    {
        good,
        bagGood
    }
}