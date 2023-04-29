using UnityEngine;
using UnityEngine.UI;

namespace Player
{
    [RequireComponent(typeof(Image), typeof(Button))]
    public class PlayerInputButton : MonoBehaviour
    {
        #region ����
        [DisplayName("��ť����")]
        public string buttonName;
        [DisplayName("��������")]
        public KeyCode keycode;
        [SerializeField, DisplayName("�Ƿ��Զ���")]
        private bool isAutoLink;
        [SerializeField, ConditionalHide("�����㼶", "isAutoLink", true, false)]
        public LayerMask playerlayer;
        [SerializeField, ConditionalHide("���", "isAutoLink", true, true)]
        public Transform playerTrans;
        [HideInInspector]
        public Button downButton;
        [HideInInspector]
        public Image cachedImage;
        #endregion
        #region Unity�¼�
        private void Awake()
        {
            cachedImage = transform.GetComponent<Image>();
            downButton = transform.GetComponent<Button>();
        }
        private void Update()
        {
            if (isAutoLink)
            {
                playerlayer = LayerMask.GetMask("Player");
            }
            else
            {
                playerTrans = PlayerManager.I.playerTrans;
            }
        }
        #endregion

    }
}

