using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OutroController))]
public class OutroControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Vykreslí standardní Inspector (všechny public fields a atributy)
        DrawDefaultInspector();

        // Oddělovač
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug / Testing", EditorStyles.boldLabel);

        OutroController outro = (OutroController)target;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Start Outro WIN"))
        {
            // Spustí výherní outro i v editoru (Play mode musí běžet)
            if (Application.isPlaying)
            {
                outro.gameObject.SetActive(true);
                outro.UserWinPlayOutro();
            }
            else
            {
                Debug.LogWarning("OutroControllerEditor: Outro lze testovat jen v Play mode.");
            }
        }

        if (GUILayout.Button("Start Outro LOSE"))
        {
            if (Application.isPlaying)
            {
                outro.gameObject.SetActive(true);
                outro.UserLosePlayOutro();
            }
            else
            {
                Debug.LogWarning("OutroControllerEditor: Outro lze testovat jen v Play mode.");
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}

