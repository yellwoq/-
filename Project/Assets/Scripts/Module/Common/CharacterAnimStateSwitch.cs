using UnityEngine;

namespace Common
{
    /// <summary>
    /// ��ɫ�����л�
    /// </summary>
    public class CharacterAnimStateSwitch
    {
        public static float CaculaterAngle(float x, float y)
        {
            float currentAngleX = x * 90f + 90f;//X�� ��ǰ�Ƕ�
            float currentAngleY = y * 90f + 90f;//Y�� ��ǰ�Ƕ�

            //�°�Բ
            if (currentAngleY < 90f)
            {
                if (currentAngleX < 90f)
                {
                    return 270f + currentAngleY;
                }
                else if (currentAngleX > 90f)
                {
                    return 180f + (90f - currentAngleY);
                }
                else
                {
                    return 270f;
                }
            }
            else if (currentAngleY == 90f)
            {
                if (currentAngleX < 90f)
                {
                    return 0;
                }
                else if (currentAngleX > 90f)
                {
                    return 180f;
                }
                else
                {
                    return 270f;
                }
            }
            return currentAngleX;
        }
    }
}