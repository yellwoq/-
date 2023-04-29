using MVC;
using UnityEngine;

namespace Common
{
    /// <summary>
    /// �ű��ĵ�����
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour, TarenaMVC.INotifier where T : MonoSingleton<T>
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (!instance || !instance.gameObject)
                {
                    //�ڳ����в����Ѿ����ڵĶ���(˵���ͻ��˴�����Awake�е���Instance)
                    instance = FindObjectOfType(typeof(T)) as T;
                    if (instance != null)
                        instance.Initialize();
                    else
                        //�����ű�����(˵��������û�д��ڵĶ���) 
                        new GameObject("Singleton of " + typeof(T)).AddComponent<T>();//��ʱ����ִ��Awake 
                }
                return instance;
            }
        }

        //���󱻴���ʱִ��
        protected void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                Initialize();
            }
        }
        public static T I
        {
            get { return Instance; }
        }
        protected virtual void Initialize() { }

        public void SendNotification(string name, object data = null)
        {
            AppFacade.I.SendNotification(name, data);
        }
    }
}