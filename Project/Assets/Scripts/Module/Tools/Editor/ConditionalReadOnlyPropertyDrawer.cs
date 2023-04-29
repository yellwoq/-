using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalReadOnlyAttribute))]
public class ConditionalReadOnlyPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalReadOnlyAttribute condRAtt = (ConditionalReadOnlyAttribute)attribute;
        bool enabled = GetConditionalReadOnlyAttributeResult(condRAtt, property);
        GUI.enabled = enabled;
        GUIContent content = null;
        if (condRAtt.Name != null)
            content = new GUIContent(label) { text = condRAtt.Name };
        else
            content = label;
        EditorGUI.PropertyField(position, property, content, true);
        GUI.enabled = true;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalReadOnlyAttribute condRAtt = (ConditionalReadOnlyAttribute)attribute;
        return EditorGUI.GetPropertyHeight(property, new GUIContent(label) { text = condRAtt.Name }, true);
    }
    /// <summary>
    /// ��ȡ����ֻ�����
    /// </summary>
    /// <param name="condRAtt"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private bool GetConditionalReadOnlyAttributeResult(ConditionalReadOnlyAttribute condRAtt, SerializedProperty property)
    {
        bool enabled = true;
        string propertyPath = property.propertyPath;
        //���õĲ����ֶ�·��
        string conditionPath = propertyPath.Replace(property.propertyPath, condRAtt.ConditionalSourceField);
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);
        if (sourcePropertyValue != null)
        {
            //�����ö��
            if (sourcePropertyValue.propertyType == SerializedPropertyType.Enum)
            {
                int enumValue = (int)Mathf.Pow(2, sourcePropertyValue.enumValueIndex);
                enabled = (enumValue & condRAtt.EnumCondition) == enumValue;

            }
            //����Ǽ�������
            else if (sourcePropertyValue.propertyType == SerializedPropertyType.Generic)
            {
                // enabled = condRAtt.Negate ? !sourcePropertyValue : sourcePropertyValue.boolValue;

            }
            else enabled = condRAtt.Negate ? !sourcePropertyValue.boolValue : sourcePropertyValue.boolValue;
        }
        else
        {
            Debug.LogWarning("���ڳ���ʹ��ConditionalHideAttribute������objec���Ҳ���ƥ���SourcePropertyValue: " + condRAtt.ConditionalSourceField);
        }

        return enabled;
    }
}
