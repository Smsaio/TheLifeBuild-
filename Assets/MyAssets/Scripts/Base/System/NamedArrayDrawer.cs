using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif 

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        //インスペクターに設定する文字列の配列を文字列に変換
        try
        {
            int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
            EditorGUI.PropertyField(rect, property, new GUIContent(((NamedArrayAttribute)attribute).names[pos]));
        }
        catch
        {
            EditorGUI.PropertyField(rect, property, label);
        }
    }
}
#endif