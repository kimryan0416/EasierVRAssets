using UnityEditor;
using UnityEngine;
 
// Script came from "Lev-Lukomskyi"
// URL to forum thread: https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html

[CustomPropertyDrawer(typeof(ShowOnlyAttribute))]
public class ShowOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        string valueStr;
 
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer:
                valueStr = prop.intValue.ToString();
                break;
            case SerializedPropertyType.Boolean:
                valueStr = prop.boolValue.ToString();
                break;
            case SerializedPropertyType.Float:
                valueStr = prop.floatValue.ToString("0.00");
                break;
            case SerializedPropertyType.String:
                valueStr = prop.stringValue;
                break;
            case SerializedPropertyType.Vector3:
                valueStr = "("+prop.vector3Value.x.ToString("0.00")+", "+prop.vector3Value.y.ToString("0.00")+", "+prop.vector3Value.z.ToString("0.00")+")";
                break;
            case SerializedPropertyType.ObjectReference:
                try {
                    valueStr = prop.objectReferenceValue.ToString();
                } catch {
                    valueStr = "None (Game Object)";
                }
                break;
            case SerializedPropertyType.Enum:
                valueStr = prop.enumDisplayNames[prop.enumValueIndex];
                break;
            default:
                valueStr = "(Not Supported)";
                break;
        }
 
        EditorGUI.LabelField(position,label.text, valueStr);
    }
}