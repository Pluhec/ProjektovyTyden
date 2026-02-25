using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MainMenuController.AnimatedUIElement))]
public class AnimatedUIElementDrawer : PropertyDrawer
{
    // Výška jednoho řádku v inspectoru
    private float lineHeight = EditorGUIUtility.singleLineHeight;
    private float spacing = EditorGUIUtility.standardVerticalSpacing;

    // Kolik řádků budeme potřebovat (Element, Start Pos, Target Pos, mezera)
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (lineHeight + spacing) * 3 + spacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Začátek property
        EditorGUI.BeginProperty(position, label, property);

        // Získáme property pro jednotlivé položky
        SerializedProperty elementProp = property.FindPropertyRelative("element");
        SerializedProperty startPosProp = property.FindPropertyRelative("startPosition");
        SerializedProperty targetPosProp = property.FindPropertyRelative("targetPosition");

        // 1. Řádek: Element (RectTransform)
        Rect elementRect = new Rect(position.x, position.y, position.width, lineHeight);
        EditorGUI.PropertyField(elementRect, elementProp, new GUIContent("Element"));

        // Získáme referenci na objekt, abychom mohli číst jeho pozici
        RectTransform rectTransform = elementProp.objectReferenceValue as RectTransform;

        // 2. Řádek: Start Position + Capture Button
        Rect startLabelRect = new Rect(position.x, position.y + lineHeight + spacing, EditorGUIUtility.labelWidth, lineHeight);
        Rect startFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + lineHeight + spacing, position.width - EditorGUIUtility.labelWidth - 70, lineHeight);
        Rect startBtnRect = new Rect(position.x + position.width - 65, position.y + lineHeight + spacing, 65, lineHeight);

        GUI.Label(startLabelRect, "Start Pos");
        EditorGUI.PropertyField(startFieldRect, startPosProp, GUIContent.none);

        if (GUI.Button(startBtnRect, "Capture"))
        {
            if (rectTransform != null)
            {
                startPosProp.vector2Value = rectTransform.anchoredPosition;
            }
            else
            {
                Debug.LogWarning("Assign an Element first!");
            }
        }

        // 3. Řádek: Target Position + Capture Button
        Rect targetLabelRect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, EditorGUIUtility.labelWidth, lineHeight);
        Rect targetFieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y + (lineHeight + spacing) * 2, position.width - EditorGUIUtility.labelWidth - 70, lineHeight);
        Rect targetBtnRect = new Rect(position.x + position.width - 65, position.y + (lineHeight + spacing) * 2, 65, lineHeight);

        GUI.Label(targetLabelRect, "Target Pos");
        EditorGUI.PropertyField(targetFieldRect, targetPosProp, GUIContent.none);

        if (GUI.Button(targetBtnRect, "Capture"))
        {
            if (rectTransform != null)
            {
                targetPosProp.vector2Value = rectTransform.anchoredPosition;
            }
            else
            {
                Debug.LogWarning("Assign an Element first!");
            }
        }

        // Konec property
        EditorGUI.EndProperty();
    }
}

